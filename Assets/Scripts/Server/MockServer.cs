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
    /// This class also handles the lifecycle of matches and of bot opponents -
    /// in the future those concerns could be more separated.
    /// </summary>
    /// <remarks
    /// Because it seemed interesting, I set it up to handle multiple matches,
    /// as if multiple players could be playing simultaneously. One goal was to
    /// ensure players aren't getting every event, only their own, so events are
    /// sent by specific Matches. The server also delegates incoming commands.
    /// </remarks>
    public class MockServer
    {
        // Singleton instance of the server for the entire game
        public static MockServer Instance { get; private set; } = new MockServer();

        // Private constructor to enforce singleton
        private MockServer() { }

        // Keep track of multiple matches (theoretical for now)
        private readonly Dictionary<string, Match> matches = new();

        private readonly Dictionary<string, Opponent> bots = new();

        /// <summary>
        /// Stub for login. Returns session details (currently just the username).
        /// </summary>
        public UserSession Login(string username, string password)
        {
            bool isAuthenticated = MockAuthenticator.Instance.Authenticate(username, password);
            if (isAuthenticated)
            {
                return new UserSession { playerId = username };
            }
            else
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }
        }

        /// <summary>
        /// Processes a command from the player. Delegates the command to a particular match based on the match ID.
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

        /// <summary>
        /// Creates a new match between the player and a new bot opponent.
        /// The client provides a callback to receive match events for their specific match.
        /// </summary>
        public string CreateMatch(string playerId, Action<string> playerEventCallback)
        {
            // Immediately create a new match for the player vs a bot opponent.
            // In real life there would be some kind of matchmaking rather than immediately creating a match.
            Match match = new();
            matches[match.MatchId] = match;
            match.Completed += OnMatchCompleted;

            Opponent opponent = new();
            bots[match.MatchId] = opponent;

            try
            {
                // Add the player
                match.AddPlayer(playerId, playerEventCallback);

                // Opponent subscribes itself
                opponent.AddToMatch(match.MatchId);
                
                // Now that we have both players, start the match
                match.StartMatch();
                return match.MatchId;
            }
            catch
            {
                EndMatch(match.MatchId);
                throw;
            }
        }

        /// <summary>
        /// Ends the match and does cleanup, including the bot.
        /// </summary>
        public void EndMatch(string matchId)
        {
            if (string.IsNullOrEmpty(matchId))
            {
                throw new ArgumentException("Missing MatchId", nameof(matchId));
            }

            if (matches.TryGetValue(matchId, out var match))
            {
                matches.Remove(matchId);
                match.Completed -= OnMatchCompleted;
                match.Dispose();
            }

            if (bots.TryGetValue(matchId, out var opponent))
            {
                bots.Remove(matchId);
                opponent.Dispose();
            }
        }

        private void OnMatchCompleted(Match match)
        {
            EndMatch(match.MatchId);
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

            matches[matchId].AddPlayer(playerId, playerEventCallback);
        }
    }

}