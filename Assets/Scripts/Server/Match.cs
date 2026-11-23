using System;
using System.Collections.Generic;
using System.Linq;
using Contracts;
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
            Debug.Log($"SERVER: Adding player {playerId} to match {matchId}");
            
            if (playerStates.Count >= 2)
            {
                throw new InvalidOperationException("Match already has two players");
            }
            
            // Add the player
            playerStates.Add(playerId, new MatchPlayerState { playerId = playerId });

            // Subscript the player to this match's events
            EventPublished += playerEventCallback;
        }

        public void StartMatch()
        {
            Debug.Log($"SERVER: Player count before starting match: {playerStates.Count}");
            foreach (var playerId in playerStates.Keys)
            {
                Debug.Log($"SERVER: Player in match: {playerId}");
            }

            if (playerStates.Count < 2)
            {
                throw new ArgumentException("Two players must have joined");
            }

            Debug.Log($"SERVER: Starting match {matchId}");

            // Init hands for both players
            foreach (var playerId in playerStates.Keys)
            {
                playerStates[playerId].hand = GetStartingHand();
            }

            status = MatchStatus.Playing;

            // TODO emit event to Start the opponent's turn
        }

        /// <summary>
        /// Processes a command from the player.
        /// </summary>
        public void ProcessCommand(PlayerCommand command, string commandJson)
        {
            Debug.Log($"SERVER: Processing command {command.commandType} from player {command.playerId}");
            switch (command.commandType)
            {
                case "SelectCard":
                    var selectCardCommand = JsonUtility.FromJson<PlayerCommand<SelectCardCommandDetails>>(commandJson);
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

        private void SelectCard(string playerId, CardDto selectedCard)
        {
            Debug.Log($"SERVER: Player {playerId} selected {selectedCard.cardType}");

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

            // Broadcast the selection
            EventPublished?.Invoke(BuildMatchEvent(
                MatchEventTypes.SelectCard,
                new SelectCardEventDetails
                {
                    playerId = playerId,
                    selectedCard = selectedCard
                }
            ));
        }

        // Utils

        private string BuildMatchEvent<TDetails>(string eventType, TDetails details)
        {
            var matchEvent = new MatchEvent<TDetails>
            {
                matchId = matchId,
                eventType = eventType,
                details = details
            };
            return JsonUtility.ToJson(matchEvent);
        }

        private static MatchPlayerState ClonePlayerState(MatchPlayerState state)
        {
            if (state == null) return null;

            return new MatchPlayerState()
            {
                playerId = state.playerId,
                score = state.score,
                hand = state.hand?.Select(CloneCard).ToArray(),
                selectedCard = CloneCard(state.selectedCard),
            };
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
    }

}