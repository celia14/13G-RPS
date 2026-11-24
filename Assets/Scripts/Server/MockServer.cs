using System;
using System.Collections.Generic;
using Contracts;
using Newtonsoft.Json;
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
        /// Creates a new match and adds the player to it.
        /// The client provides a callback to receive match events for their specific match.
        /// This one is awkward and is making a lot of assumptions for a single player vs bot scenario.
        /// </summary>
        public string CreateMatch(string playerId, Action<string> playerEventCallback)
        {
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
            match.StartMatch();
            
            return match.MatchId;
        }

        
        /// <summary>
        /// Subscribe to an existing match with this player.
        /// </summary>
        public void JoinMatch(string matchId, string playerId, Action<string> playerEventCallback)
        {
            if (!matches.ContainsKey(matchId))
            {
                throw new ArgumentException("Match not found");
            }

            var match = matches[matchId];
            match.AddPlayer(playerId, playerEventCallback);
        }

        /// <summary>
        /// Processes a command from the player. Delegate to a particular match based on the match ID.
        /// </summary>
        public void PlayerCommand(string commandJson)
        {
            Debug.Log("SERVER: Received command " + commandJson);

            PlayerCommand command;
            try
            {
                command = JsonConvert.DeserializeObject<PlayerCommand>(commandJson)
                    ?? throw new JsonSerializationException("Player command cannot be null.");
            }
            catch (Exception ex)
            {
                Debug.LogError("SERVER: Failed to parse command JSON: " + ex.Message);
                throw;
            }

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