using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;

namespace Server
{
    /// <summary>
    /// A simple bot that acts as the opponent in the game.
    /// To separate this concern from Match, the bot talks to the MockServer
    /// via MatchClient as if it were a real player. So it's parallel to
    /// the real client's MatchManager in its interactions with the MatchClient.
    /// </summary>
    public class Opponent : IDisposable
    {
        // In real life the id should be a guid
        public const string playerId = "bot";
        private const int minThinkingDelayMs = 500;
        private const int maxThinkingDelayMs = 1500;

        private readonly MatchClient matchClient;
        private readonly CancellationTokenSource lifetime = new();
        private bool disposed;

        // Keep hand up to date via the events received from the server
        private CardDto[] hand;

        public Opponent()
        {
            matchClient = new MatchClient(playerId);
            matchClient.MatchEventReceived += HandleMatchEvent;
        }

        public void AddToMatch(string matchId)
        {
            matchClient.JoinMatch(matchId);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            lifetime.Cancel();
            lifetime.Dispose();
            matchClient.MatchEventReceived -= HandleMatchEvent;
            matchClient.Dispose();
            hand = null;
        }

        private async void HandleMatchEvent(MatchEvent matchEvent, string matchEventJson)
        {
            if (disposed)
            {
                return;
            }

            var token = lifetime.Token;
            try
            {
                // Always update the bot's hand with the latest.
                UpdateHand(matchEvent.newMatchState);

                switch (matchEvent.eventType)
                {
                    case MatchEventTypes.RoundStarted:
                        await PlayRoundAsync(token);
                        break;
                    default:
                        break;
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // Leaving the match cancels the bot's pending move.
            }
        }

        private async Task PlayRoundAsync(CancellationToken token)
        {
            // Wait before playing, like a real opponent would
            int delay = UnityEngine.Random.Range(minThinkingDelayMs, maxThinkingDelayMs);
            await Task.Delay(delay, token);
            token.ThrowIfCancellationRequested();

            CardDto chosenCard = ChooseCard();
            matchClient.SelectCard(chosenCard.cardType);
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