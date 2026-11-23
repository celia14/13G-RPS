using System;
using System.Collections.Generic;
using Contracts;
using UnityEngine;

namespace Server
{
    /// <summary>
    /// Mock server that the client code can interact with via JSON messages.
    /// The client sends commands and subscribes to events from the server.
    /// </summary>
    public class Server
    {
        // Track theoretical multiple matches
        private readonly Dictionary<string, Match> matches = new();

        /// <summary>
        /// Processes a command from the player. Delegate to a particular match based on the match ID.
        /// </summary>
        public void PlayerCommand(string commandJson)
        {
            Debug.Log("Processing player command: " + commandJson);

            PlayerCommand command;
            try
            {
                command = JsonUtility.FromJson<PlayerCommand>(commandJson);
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to parse command JSON: " + ex.Message);
                throw ex;
            }

            Debug.Log("Received command: " + command.commandType);

            if (string.IsNullOrEmpty(command?.matchId))
            {
                Debug.LogError("Missing MatchId");
                throw new ArgumentException("Missing MatchId");
            }

            if (!matches.ContainsKey(command.matchId))
            {
                Debug.LogError("Match not found: " + command.matchId);
                throw new ArgumentException("Match not found");
            }

            // Give the command to the match
            matches[command.matchId].ProcessCommand(command);
        }
    }

}