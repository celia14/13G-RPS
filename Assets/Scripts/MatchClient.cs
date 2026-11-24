using System;
using Contracts;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Client for communicating with the server about a particular match via player commands and
/// server events. Current connection method is to pass a callback to subscribe to match events.
/// </summary>
/// <remarks>
/// The nature of the Scene setup is this will only live for a single match - but 
///  probably whether it's tied to a single match or could be reused could be clarified more.
/// </remarks>
public class MatchClient : IDisposable
{
    private readonly Server.MockServer server = Server.MockServer.Instance;

    private readonly string playerId;
    private string matchId;
    private bool disposed;

    public event Action<MatchEvent, string> MatchEventReceived;

    public MatchClient(string playerId)
    {
        this.playerId = playerId;
    }

    public void CreateAndJoinMatch()
    {
        EnsureCanJoin();
        string createdMatchId = server.CreateMatch(playerId, HandleMatchEvent);
        if (disposed)
        {
            server.EndMatch(createdMatchId);
        }
        else
        {
            matchId = createdMatchId;
        }
    }

    public void JoinMatch(string matchId)
    {
        EnsureCanJoin();
        server.JoinMatch(matchId, playerId, HandleMatchEvent);
        this.matchId = matchId;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        MatchEventReceived = null;
        string joinedMatchId = matchId;
        matchId = null;
        if (joinedMatchId != null)
        {
            server.EndMatch(joinedMatchId);
        }
    }

    private void EnsureCanJoin()
    {
        ThrowIfDisposed();
        if (matchId != null)
        {
            throw new InvalidOperationException("Client has already joined a match.");
        }
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(MatchClient));
        }
    }

    public void SelectCard(CardType cardType)
    {
        SendPlayerCommand("SelectCard",
            new SelectCardCommandDetails
            {
                selectedCard = new CardDto { cardType = cardType }
            }
        );
    }
    
    private void SendPlayerCommand<TDetails>(string commandType, TDetails details)
    {
        ThrowIfDisposed();
        if (matchId == null)
        {
            throw new InvalidOperationException("Client has not joined an active match.");
        }

        PlayerCommand<TDetails> command = new()
        {
            playerId = playerId,
            matchId = matchId,
            commandType = commandType,
            details = details
        };
        var commandJson = JsonConvert.SerializeObject(command);
        Debug.Log($"{playerId.ToUpper()} CLIENT: Sending command {commandJson}");
        server.PlayerCommand(commandJson);
    }

    private void HandleMatchEvent(string matchEventJson)
    {
        if (disposed)
        {
            return;
        }

        try {
            Debug.Log($"{playerId.ToUpper()} CLIENT: Received event {matchEventJson}");
            var matchEvent = JsonConvert.DeserializeObject<MatchEvent>(matchEventJson)
                ?? throw new JsonSerializationException("Match event cannot be null.");
            // CreateMatch publishes synchronously, before it returns the match ID.
            matchId = matchEvent.eventType == MatchEventTypes.MatchEnded ? null : matchEvent.matchId;
            MatchEventReceived?.Invoke(matchEvent, matchEventJson);
        }
        catch (Exception ex)
        {
            Debug.LogError($"{playerId.ToUpper()} CLIENT: Failed to handle match event: {ex.Message}");
            throw;
        }
    }
}