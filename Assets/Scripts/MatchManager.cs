using Contracts;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(MatchClient))]
public class MatchManager : MonoBehaviour
{
    private const string playerId = "player1";

    [SerializeField] private Hand playerHand;
    [SerializeField] private Hand opponentHand;
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private TMP_Text opponentScoreText;
    [SerializeField] private PlayArea playArea;

    private MatchClient matchClient;

    void Start()
    {
        matchClient = GetComponent<MatchClient>();
        MatchState matchState = matchClient.InitMatch();
        matchClient.MatchEventReceived += HandleMatchEvent;

        // TODO populate based on state
        playerHand.PopulateHand();
        opponentHand.PopulateHand();

        SetPlayerScore(0);
        SetOpponentScore(0);

        playerHand.CardClicked += OnCardClicked;
    }

    private void HandleMatchEvent(MatchEvent matchEvent, string matchEventJson)
    {
        switch (matchEvent.eventType)
        {
            case "CardSelected":
                var cardSelectedEvent = JsonUtility.FromJson<MatchEvent<SelectCardEventDetails>>(matchEventJson);
                OnCardSelected(cardSelectedEvent);
                break;
            case "RoundResult":
                // Handle round result event
                break;
            default:
                Debug.LogError("Unknown match event type: " + matchEvent.eventType);
                break;
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
