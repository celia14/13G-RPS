using Contracts;
using UnityEngine;

public class MatchClient : MonoBehaviour
{
    private readonly Server.MockServer server = Server.MockServer.Instance;

    private string matchId;

    public MatchState InitMatch()
    {
        var match = server.CreateMatch();
        matchId = match.MatchId;
        return match.State;
    }

    public void SelectCard(CardType cardType)
    {
        SendPlayerCommand(new PlayerCommand<SelectCardCommandDetails>
        {
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
        var commandJson = JsonUtility.ToJson(command);
        server.PlayerCommand(commandJson);
    }
}