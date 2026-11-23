using System;
using Contracts;

namespace Server
{
    /// <summary>
    /// A simple bot that acts as the opponent in the game.
    /// To separate this concern from Match, the bot talks to the MockServer
    /// via MatchClient as if it were a real player. So it's parallel to
    /// the real client's MatchManager in its interactions with the MatchClient.
    /// </summary>
    public class Opponent
    {
        public const string playerId = "00000000-0000-0000-0000-000000000000";

        private MatchClient matchClient;

        // Keep hand up to date via the events received from the server
        private CardDto[] hand;

        public Opponent()
        {
            matchClient = new MatchClient(playerId);
        }

        public void AddToMatch(string matchId)
        {
            MatchState matchState = matchClient.JoinMatch(matchId);
            UpdateHand(matchState);
        }

        public void PlayRound()
        {
            // TODO Timer for realism

            CardDto chosenCard = ChooseCard();
            matchClient.SelectCard(chosenCard.cardType);
        }

        private void HandleMatchEvent(MatchEvent matchEvent, string matchEventJson)
        {
            // The bot doesn't care what's actually going on, it just wants the latest state
            // Arguably, getting it from events instead of just checking the Match is
            // unnecessary indirection, but I like the bot acting like a real client.
            UpdateHand(matchEvent.newMatchState);
        }

        private void UpdateHand(MatchState matchState)
        {
            foreach (var player in matchState.playerStates)
            {
                if (player.playerId == playerId)
                {
                    hand = player.hand;
                    break;
                }
            }
        }

        private CardDto ChooseCard()
        {
            // Choose a card from hand at random
            int randomIndex = UnityEngine.Random.Range(0, hand.Length);
            return hand[randomIndex];
        }
    }
}