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
        /// Creates a new match and returns the initial state. (TODO starting event instead)
        /// The client provides a callback to receive match events for their specific match.
        /// This one is awkward and is making a lot of assumptions for a single player vs bot scenario.
        /// </summary>
        public MatchState CreateMatch(string playerId, Action<string> playerEventCallback)
        {
            Debug.Log("SERVER: Creating new match");

            // Immediately create a new match for the player vs a bot opponent.
            // In the future there would be some kind of matchmaking rather than immediately creating a match.
            Match match = new();
            matches[match.MatchId] = match;

            // Add the player
            match.AddPlayer(playerId, playerEventCallback);

            // Create an opponent and have it subscribe itself
            Opponent opponent = new();
            opponent.AddToMatch(match.MatchId);

            // Now that we have both players, start the match
            Debug.Log("SERVER: Starting match " + match.MatchId);
            match.StartMatch();
            return match.State;
        }

        
        /// <summary>
        /// Subscribe to an existing match and get the initial state.
        /// </summary>
        public MatchState JoinMatch(string matchId, string playerId, Action<string> playerEventCallback)
        {
            Debug.Log("SERVER: Joining match with ID: " + matchId);
            if (!matches.ContainsKey(matchId))
            {
                throw new ArgumentException("Match not found");
            }

            var match = matches[matchId];
            match.AddPlayer(playerId, playerEventCallback);
            return match.State;
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
                throw new ArgumentException("Missing MatchId");
            }

            if (!matches.ContainsKey(command.matchId))
            {
                throw new ArgumentException("Match not found");
            }

            // Give the command to the match
            matches[command.matchId].ProcessCommand(command, commandJson);
        }
    }

}