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
        private const string botId = "00000000-0000-0000-0000-000000000000";

        private readonly string matchId;
        private readonly string player1Id;
        private readonly string player2Id;

        private MatchStatus status = MatchStatus.New;
        private int round = 0;
        private readonly Dictionary<string, MatchPlayerState> playerStates;

        public string MatchId => matchId;

        // Clone the state when returning it via getter so the caller can't touch the actual state
        public MatchState State => new()
        {
            matchId = matchId,
            status = status,
            round = round,
            playerStates = new[] {
                ClonePlayerState(playerStates[player1Id]),
                ClonePlayerState(playerStates[player2Id])
            }
        };

        /// <summary>
        /// Publishes match events. The player's client
        /// will subscribe to keep its game state updated.
        /// </summary>
        public event Action<string> EventPublished;

        public Match(string player1, string player2 = botId)
        {
            matchId = Guid.NewGuid().ToString();
            player1Id = player1;
            player2Id = player2;

            playerStates = new Dictionary<string, MatchPlayerState>
            {
                { player1Id, new MatchPlayerState { playerId = player1Id } },
                { player2Id, new MatchPlayerState { playerId = player2Id } }
            };

            playerStates[player1Id].hand = GetStartingHand();
            playerStates[player2Id].hand = GetStartingHand();

            // Start immediately on load/creation for now
            StartMatch();
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

        private void StartMatch()
        {
            Debug.Log($"SERVER: Starting match {matchId}");
            status = MatchStatus.Playing;
            // Start the opponent's turn
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