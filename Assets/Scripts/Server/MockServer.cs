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
    public class MockServer
    {
        // Singleton instance of the server for the entire game
        public static MockServer Instance { get; private set; } = new MockServer();

        // Private constructor to enforce singleton
        private MockServer() { }

        // Track theoretical multiple matches
        private readonly Dictionary<string, Match> matches = new();

        /// <summary>
        /// Creates a new match and returns the match itself (so the client can subscribe to match events.)
        /// In real life there should be some other subscription mechanism.
        /// </summary>
        public Match CreateMatch()
        {
            Debug.Log("SERVER: Creating new match");
            var match = new Match("player1"); // TODO player's ID from login
            matches[match.MatchId] = match;

            Debug.Log("SERVER: Created match with ID: " + match.MatchId);
            return match;
        }

        /// <summary>
        /// Processes a command from the player. Delegate to a particular match based on the match ID.
        /// </summary>
        public void PlayerCommand(string commandJson)
        {
            Debug.Log("SERVER: Processing player command: " + commandJson);

            PlayerCommand command;
            try
            {
                command = JsonUtility.FromJson<PlayerCommand>(commandJson);
            }
            catch (Exception ex)
            {
                Debug.LogError("SERVER: Failed to parse command JSON: " + ex.Message);
                throw;
            }

            Debug.Log("SERVER: Received command: " + command.commandType);

            if (string.IsNullOrEmpty(command?.matchId))
            {
                Debug.LogError("SERVER: Missing MatchId");
                throw new ArgumentException("Missing MatchId");
            }

            if (!matches.ContainsKey(command.matchId))
            {
                Debug.LogError("SERVER: Match not found: " + command.matchId);
                throw new ArgumentException("Match not found");
            }

            // Give the command to the match
            matches[command.matchId].ProcessCommand(command);
        }
    }

}