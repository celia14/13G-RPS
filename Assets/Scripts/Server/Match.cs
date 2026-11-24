using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using Newtonsoft.Json;
using UnityEngine;

namespace Server
{
    /// <summary>
    /// Manages a single 3-round match between two players.
    /// </summary>
    public class Match
    {
        private readonly string matchId;

        private MatchStatus status = MatchStatus.New;
        private int round = 0;
        private readonly Dictionary<string, MatchPlayerState> playerStates = new();

        public string MatchId => matchId;

        // Clone the state when returning it via getter so the caller can't touch the actual state
        public MatchState State => new()
        {
            matchId = matchId,
            status = status,
            round = round,

            // Clone the player states and return in an array
            playerStates = playerStates.Values.Select(ps => ClonePlayerState(ps)).ToArray()
        };

        /// <summary>
        /// Publishes match events. The player's client
        /// will subscribe to keep its game state updated.
        /// </summary>
        public event Action<string> EventPublished;

        public Match()
        {
            matchId = Guid.NewGuid().ToString();
        }

        public void AddPlayer(string playerId, Action<string> playerEventCallback)
        {
            if (playerStates.Count >= 2)
            {
                throw new InvalidOperationException("Match already has two players");
            }
            if (status != MatchStatus.New)
            {
                throw new InvalidOperationException("Match has already started");
            }
            
            // Add the player
            playerStates.Add(playerId, new MatchPlayerState { playerId = playerId });

            // Subscript the player to this match's events
            EventPublished += playerEventCallback;
        }

        public void StartMatch()
        {
            if (playerStates.Count < 2)
            {
                throw new ArgumentException("Two players must have joined");
            }

            // Init hands for both players
            foreach (var playerId in playerStates.Keys)
            {
                playerStates[playerId].hand = GetStartingHand();
            }

            status = MatchStatus.Playing;
            EmitMatchEvent(MatchEventTypes.MatchStarted, new { });

            StartRound();
        }

        /// <summary>
        /// Processes a command from the player.
        /// </summary>
        public void ProcessCommand(PlayerCommand command, string commandJson)
        {
            switch (command.commandType)
            {
                case "SelectCard":
                    var selectCardCommand = JsonConvert.DeserializeObject<PlayerCommand<SelectCardCommandDetails>>(commandJson)
                        ?? throw new JsonSerializationException("SelectCard command cannot be null.");
                    SelectCard(selectCardCommand.playerId, selectCardCommand.details.selectedCard);
                    break;
                default:
                    throw new ArgumentException($"Unknown command type: {command.commandType}");
            }
        }

        private CardDto[] GetStartingHand()
        {
            // Always populate one of each type
            CardType[] types = (CardType[])Enum.GetValues(typeof(CardType));

            // Shuffle
            for (int i = 0; i < types.Length; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, types.Length);
                (types[randomIndex], types[i]) = (types[i], types[randomIndex]);
            }

            // Starting hand of CardDtos
            CardDto[] startingHand = new CardDto[types.Length];
            foreach (var type in types)
            {
                startingHand[(int)type] = new CardDto { cardType = type };
            }

            return startingHand;
        }

        private void StartRound()
        {
            if (status != MatchStatus.Playing)
            {
                throw new InvalidOperationException("Cannot start a round when the match is not in playing status.");
            }

            if (playerStates.Values.Any(p => p.hand == null || p.hand.Length == 0))
            {
                throw new InvalidOperationException("Cannot start a round when any player has an empty hand.");
            }

            round++; // First round is 1 - seems right to me if we wanted to display it

            // Clear selected cards for all players
            foreach (var player in playerStates.Values)
            {
                player.selectedCard = null;
            }

            // Notify round started
            EmitMatchEvent(MatchEventTypes.RoundStarted, new { });
        }

        private void SelectCard(string playerId, CardDto selectedCard)
        {
            if (selectedCard == null)
            {
                throw new ArgumentException("Selected card cannot be null");
            }

            if (!playerStates.ContainsKey(playerId))
            {
                throw new ArgumentException($"Invalid playerId: {playerId}");
            }

            if (playerStates[playerId].hand == null || !playerStates[playerId].hand.Any(c => AreCardsEqual(c, selectedCard)))
            {
                throw new InvalidOperationException($"Player {playerId} does not have the selected card in hand.");
            }

            // Remove from hand and set it as selected
            playerStates[playerId].selectedCard = selectedCard;
            playerStates[playerId].hand = playerStates[playerId].hand?.Where(c => !AreCardsEqual(c, selectedCard)).ToArray();

            // Broadcast the selection -> client will make a UI update
            EmitMatchEvent(
                MatchEventTypes.CardSelected,
                new CardSelectedEventDetails
                {
                    playerId = playerId,
                    selectedCard = selectedCard
                }
            );

            // Playing a card may resolve the round if the opponent has already played theirs
            CheckResolveRound();
        }

        private void CheckResolveRound()
        {
            if (playerStates.Values.All(p => p.selectedCard != null))
            {
                ResolveRound();
            }
        }

        private void ResolveRound()
        {
            // Find the winner
            var players = playerStates.Values.ToArray();
            var result = ScoreRps(players[0].selectedCard, players[1].selectedCard);
            var winner = result > 0 ? players[0] : result < 0 ? players[1] : null;

            if (winner != null)
            {
                winner.score++;
            }

            // Notify all clients about the round result
            EmitMatchEvent(
                MatchEventTypes.RoundResolved,
                new RoundResolvedEventDetails
                {
                    isDraw = result == 0,
                    winningPlayerId = winner?.playerId,
                    playedCards = players.Select(p => CloneCard(p.selectedCard)).ToArray(),
                }
            );

            Debug.Log("Round resolved. Winner: " + (winner?.playerId ?? "Draw"));

            // TODO end of match

            // Then start the next round asynchronously
            _ = StartRoundAsync();
        }

        /// <summary>
        /// Starts the next round after a short delay for the results to sink in.
        /// This should really be handled by the client based on its animations/ux/etc.
        /// </summary>
        private async Task StartRoundAsync()
        {
            await Task.Delay(3000);

            Debug.Log("Starting next round...");
            StartRound();
        }

        // Utils

        private void EmitMatchEvent<TDetails>(string eventType, TDetails details)
        {
            var matchEvent = new MatchEvent<TDetails>
            {
                matchId = matchId,
                eventType = eventType,
                details = details,
                newMatchState = State
            };
            var eventJson = JsonConvert.SerializeObject(matchEvent);
            Debug.Log($"SERVER: Sending event {eventJson}");
            EventPublished?.Invoke(eventJson);
        }

        private static MatchPlayerState ClonePlayerState(MatchPlayerState state)
        {
            if (state == null) return null;

            var newState = new MatchPlayerState()
            {
                playerId = state.playerId,
                score = state.score,
                hand = state.hand?.Select(CloneCard).ToArray(),
                selectedCard = CloneCard(state.selectedCard),
            };

            return newState;
        }

        private static CardDto CloneCard(CardDto card)
        {
            if (card == null) return null;

            return new CardDto()
            {
                cardType = card.cardType
            };
        }

        // Defining this to make it simpler later if cards change (eg start using an id)
        private static bool AreCardsEqual(CardDto card1, CardDto card2)
        {
            if (card1 == null && card2 == null) return true;
            if (card1 == null || card2 == null) return false;
            return card1.cardType == card2.cardType;
        }

        private static int ScoreRps(CardDto card1, CardDto card2)
        {
            // Draw
            if (AreCardsEqual(card1, card2)) return 0;

            // Card1 wins
            if (card1.cardType == CardType.Rock && card2.cardType == CardType.Scissors) return 1;
            if (card1.cardType == CardType.Scissors && card2.cardType == CardType.Paper) return 1;
            if (card1.cardType == CardType.Paper && card2.cardType == CardType.Rock) return 1;

            // Card2 wins
            return -1;
        }
    }

}