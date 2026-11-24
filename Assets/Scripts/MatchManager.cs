using Contracts;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    private string playerId;

    [SerializeField] private Hand playerHand;
    [SerializeField] private Hand opponentHand;
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private TMP_Text opponentScoreText;
    [SerializeField] private PlayArea playArea;
    [SerializeField] private History history;

    private MatchClient matchClient;

    void Start()
    {
        if (GameManager.Instance.Session == null)
        {
            // Redirect the user to login
            GameManager.Instance.ShowTitle();
            return;
        }
        playerId = GameManager.Instance.Session.playerId;

        matchClient = new MatchClient(playerId);
        matchClient.MatchEventReceived += HandleMatchEvent;
        matchClient.CreateAndJoinMatch();

        playerHand.CardClicked += OnCardClicked;
    }

    private void HandleMatchEvent(MatchEvent matchEvent, string matchEventJson)
    {
        switch (matchEvent.eventType)
        {
            case MatchEventTypes.MatchStarted:
                // Populate initial hand
                UpdateMatchState(matchEvent.newMatchState);
                break;
            case MatchEventTypes.RoundStarted:
                OnRoundStarted(matchEvent);
                break;
            case MatchEventTypes.CardSelected:
                var cardSelectedEvent = JsonConvert.DeserializeObject<MatchEvent<CardSelectedEventDetails>>(matchEventJson)
                    ?? throw new JsonSerializationException("CardSelected event cannot be null.");
                OnCardSelected(cardSelectedEvent);
                break;
            case MatchEventTypes.RoundResolved:
                var roundResolvedEvent = JsonConvert.DeserializeObject<MatchEvent<RoundResolvedEventDetails>>(matchEventJson)
                    ?? throw new JsonSerializationException("RoundResolved event cannot be null.");
                OnRoundResolved(roundResolvedEvent);
                break;
            default:
                break;
        }
    }

    private void UpdateMatchState(MatchState matchState)
    {
        foreach (var playerState in matchState.playerStates)
        {
            UpdatePlayerState(playerState);
        }
    }

    private void UpdatePlayerState(MatchPlayerState playerState)
    {
        Debug.Log(JsonConvert.SerializeObject(playerState));
        if (playerState.playerId == playerId)
        {
            playerHand.PopulateHand(playerState.hand);
            SetPlayerScore(playerState.score);
            if (playerState.selectedCard == null)
            {
                playArea.ClearPlayerSlot();
            }
            // Assumption: We won't depend on plain Update
            // to move the card to the slot, that's only OnCardSelected...
        }
        else
        {
            opponentHand.PopulateHand(playerState.hand);
            SetOpponentScore(playerState.score);
            if (playerState.selectedCard == null)
            {
                playArea.ClearOpponentSlot();
            }
        }
    }

    private void OnRoundStarted(MatchEvent matchEvent)
    {
        // Move any existing cards to the history
        // Note, doing the history hackily since I just want it to exist.
        // Really we need to keep better track than just moving the card objects.
        if (playArea.PlayerCard != null && playArea.OpponentCard != null)
        {
            history.MoveToHistory(playArea.PlayerCard, playArea.OpponentCard);
        }

        // Just in case, clear the play area
        playArea.ClearPlayerSlot();
        playArea.ClearOpponentSlot();
    }

    private void OnCardClicked(Card card)
    {
        matchClient.SelectCard(card.cardType);
    }

    private void OnCardSelected(MatchEvent<CardSelectedEventDetails> matchEvent)
    {
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

    private void OnRoundResolved(MatchEvent<RoundResolvedEventDetails> matchEvent)
    {
        // Here's where we could do some animations, etc.
        playArea.RevealCards();

        // But for now just update to to reflect the updated scores
        UpdateMatchState(matchEvent.newMatchState);
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
