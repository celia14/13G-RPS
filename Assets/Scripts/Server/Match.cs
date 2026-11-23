using System;
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
        private readonly MatchState state = new MatchState();

        public string MatchId => matchId;
        public MatchState State => state;

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
        }

        /// <summary>
        /// Processes a command from the player.
        /// </summary>
        public void ProcessCommand(PlayerCommand command)
        {
            switch (command.commandType)
            {
                case "SelectCard":
                    if (command is PlayerCommand<SelectCardCommandDetails> selectCardCommand)
                    {
                        SelectCard(selectCardCommand.playerId, selectCardCommand.details.selectedCard);
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Unknown command type: {command.commandType}");
            }
        }

        private void SelectCard(string playerId, CardDto selectedCard)
        {
            Debug.Log($"SERVER: Player {playerId} selected {selectedCard.cardType}");
        }
    }

}