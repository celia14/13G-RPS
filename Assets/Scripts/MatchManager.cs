using Contracts;
using TMPro;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    private const string playerId = "player1"; // TODO

    [SerializeField] private Hand playerHand;
    [SerializeField] private Hand opponentHand;
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private TMP_Text opponentScoreText;
    [SerializeField] private PlayArea playArea;

    private MatchClient matchClient;

    void Start()
    {
        matchClient = new MatchClient(playerId);
        MatchState matchState = matchClient.CreateAndJoinMatch();
        matchClient.MatchEventReceived += HandleMatchEvent;

        foreach (var playerState in matchState.playerStates)
        {
            UpdatePlayerState(playerState);
        }

        playerHand.CardClicked += OnCardClicked;
    }

    private void HandleMatchEvent(MatchEvent matchEvent, string matchEventJson)
    {
        switch (matchEvent.eventType)
        {
            case MatchEventTypes.MatchStarted:
                Debug.Log("Match started");
                break;
            case MatchEventTypes.CardSelected:
                var cardSelectedEvent = JsonUtility.FromJson<MatchEvent<SelectCardEventDetails>>(matchEventJson);
                OnCardSelected(cardSelectedEvent);
                break;
            case MatchEventTypes.RoundResolved:
                // Handle round result event
                break;
            default:
                break;
        }
    }

    private void UpdatePlayerState(MatchPlayerState playerState)
    {
        if (playerState.playerId == playerId)
        {
            playerHand.PopulateHand(playerState.hand);
            SetPlayerScore(playerState.score);
        }
        else
        {
            opponentHand.PopulateHand(playerState.hand);
            SetOpponentScore(playerState.score);
        }
    }

    private void OnCardClicked(Card card)
    {
        matchClient.SelectCard(card.cardType);
    }

    private void OnCardSelected(MatchEvent<SelectCardEventDetails> matchEvent)
    {
        Debug.Log($"Player {matchEvent.details.playerId} selected card {matchEvent.details.selectedCard.cardType}");
        if (matchEvent.details.playerId == playerId)
        {
            var card = playerHand.GetCard(matchEvent.details.selectedCard.cardType);
            playArea.MoveCardToPlayerSlot(card);
        }
        else
        {
            var card = opponentHand.GetCard(matchEvent.details.selectedCard.cardType);
            playArea.MoveCardToOpponentSlot(card);
        }
    
    }

    private void SetPlayerScore(int score)
    {
        playerScoreText.text = score.ToString();
    }
    
    private void SetOpponentScore(int score)
    {
        opponentScoreText.text = score.ToString();
    }
}
