using System;
using Contracts;
using UnityEngine;

public class MatchClient
{
    private readonly Server.MockServer server = Server.MockServer.Instance;

    private readonly string playerId;
    private string matchId;

    public event Action<MatchEvent, string> MatchEventReceived;

    public MatchClient(string playerId)
    {
        this.playerId = playerId;
    }

    public void CreateAndJoinMatch()
    {
        matchId = server.CreateMatch(playerId, HandleMatchEvent);
    }

    public void JoinMatch(string matchId)
    {
        server.JoinMatch(matchId, playerId, HandleMatchEvent);
        this.matchId = matchId;
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
        Debug.Log($"CLIENT: Sending {commandType} command");
        PlayerCommand<TDetails> command = new()
        {
            playerId = playerId,
            matchId = matchId,
            commandType = commandType,
            details = details
        };
        var commandJson = JsonUtility.ToJson(command);
        server.PlayerCommand(commandJson);
    }

    private void HandleMatchEvent(string matchEventJson)
    {
        try {
            Debug.Log($"CLIENT: Received match event JSON: {matchEventJson}");
            var matchEvent = JsonUtility.FromJson<MatchEvent>(matchEventJson);
            MatchEventReceived?.Invoke(matchEvent, matchEventJson);
        }
        catch (Exception ex)
        {
            Debug.LogError($"CLIENT: Failed to handle match event: {ex.Message}");
            throw;
        }
    }
}