using System;

// Specify type aliases for clarity
using PlayerId = System.String;
using MatchId = System.String;

namespace Contracts
{
    public enum CardType
    {
        Rock = 0,
        Paper = 1,
        Scissors = 2
    }

    [Serializable]
    public class CardDto
    {
        public CardType cardType;
    }

    [Serializable]
    public class MatchPlayerState
    {
        public PlayerId playerId;
        public int score;
        public CardDto[] hand;
        public CardDto selectedCard;
    }

    public enum MatchStatus
    {
        New = 0,
        Playing = 1,
        Complete = 2
    }

    [Serializable]
    public class MatchState
    {
        public MatchId matchId;
        public MatchPlayerState[] playerStates;
        public MatchStatus status;
        public int round;
    }

    /// <summary>
    /// A player's action sent to the server
    /// </summary>
    [Serializable]
    public class PlayerCommand
    {
        /// <summary>
        /// Must be a const from PlayerCommandTypes
        /// </summary>
        public string commandType;
        public MatchId matchId;
        public PlayerId playerId;
    }

    [Serializable]
    public class PlayerCommand<TDetails> : PlayerCommand
    {
        public TDetails details;
    }

    public static class PlayerCommandTypes
    {
        public const string SelectCard = "SelectCard";
    }

    [Serializable]
    public class SelectCardCommandDetails
    {
        public CardDto selectedCard;
    }

    /// <summary>
    /// A match event sent to all clients involved in the match
    /// </summary>
    [Serializable]
    public class MatchEvent
    {
        /// <summary>
        /// Must be a const from MatchEventTypes
        /// </summary>
        public string eventType;
        public MatchId matchId;
        public MatchState newMatchState;
    }

    [Serializable]
    public class MatchEvent<TDetails> : MatchEvent
    {
        public TDetails details;
    }

    public static class MatchEventTypes
    {
        public const string MatchStarted = "MatchStarted";
        public const string RoundStarted = "RoundStarted";
        public const string CardSelected = "CardSelected";
        public const string RoundResolved = "RoundResolved";
    }

    [Serializable]
    public class CardSelectedEventDetails
    {
        public PlayerId playerId;
        public CardDto selectedCard;
    }

    [Serializable]
    public class RoundResolvedEventDetails
    {
        public bool isDraw;
        public PlayerId? winningPlayerId;
        public CardDto[] playedCards;
    }
}