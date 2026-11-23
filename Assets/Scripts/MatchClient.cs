using System;
using Contracts;
using UnityEngine;

public class MatchClient : MonoBehaviour
{
    private readonly Server.MockServer server = Server.MockServer.Instance;

    private string matchId;

    public event Action<MatchEvent, string> MatchEventReceived;

    public MatchState InitMatch()
    {
        var matchState = server.CreateMatch(HandleMatchEvent);
        matchId = matchState.matchId;
        return matchState;
    }

    public void SelectCard(CardType cardType)
    {
        SendPlayerCommand(new PlayerCommand<SelectCardCommandDetails>
        {
            playerId = "player1", // TODO
            matchId = matchId,
            commandType = "SelectCard",
            details = new SelectCardCommandDetails
            {
                selectedCard = new CardDto { cardType = cardType }
            }
        });
    }
    
    private void SendPlayerCommand(PlayerCommand command)
    {
        Debug.Log($"CLIENT: Sending player {command.commandType} command");
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