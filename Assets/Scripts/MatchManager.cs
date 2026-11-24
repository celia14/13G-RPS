using System.Threading.Tasks;
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
    [SerializeField] private TMP_Text playerWinnerText;
    [SerializeField] private TMP_Text opponentWinnerText;
    [SerializeField] private PlayArea playArea;
    [SerializeField] private History history;

    private MatchClient matchClient;

    void Start()
    {
        /*
        if (GameManager.Instance.Session == null)
        {
            // Redirect the user to login if there's an issue with the session
            GameManager.Instance.ShowTitleScreen();
            return;
        }
        playerId = GameManager.Instance.Session.playerId;
        */
        playerId = "player";

        matchClient = new MatchClient(playerId);
        matchClient.MatchEventReceived += HandleMatchEvent;
        matchClient.CreateAndJoinMatch();

        playerHand.CardClicked += OnCardClicked;
    }

    private async void HandleMatchEvent(MatchEvent matchEvent, string matchEventJson)
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
                await OnRoundResolvedAsync(roundResolvedEvent);
                break;
            case MatchEventTypes.MatchEnded:
                var matchEndedEvent = JsonConvert.DeserializeObject<MatchEvent<MatchEndedEventDetails>>(matchEventJson)
                    ?? throw new JsonSerializationException("MatchEnded event cannot be null.");
                OnMatchEnded(matchEndedEvent);
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

    private void OnRoundStarted(MatchEvent matchEvent)
    {
        ClearPlayArea();
    }

    private void ClearPlayArea()
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

    private async Task OnRoundResolvedAsync(MatchEvent<RoundResolvedEventDetails> matchEvent)
    {
        // Here's where we could do some animations, etc.
        await Task.Delay(1000);
        playArea.RevealCards();

        // But for now just update to to reflect the updated scores
        UpdateMatchState(matchEvent.newMatchState);
    }

    private async void OnMatchEnded(MatchEvent<MatchEndedEventDetails> matchEvent)
    {
        ClearPlayArea();

        if (matchEvent.details.isDraw)
        {
            playerWinnerText.text = "Draw!";
            opponentWinnerText.text = "Draw!";
            playerWinnerText.gameObject.SetActive(true);
            opponentWinnerText.gameObject.SetActive(true);
        }
        else if (matchEvent.details.winningPlayerId == playerId)
        {
            playerWinnerText.gameObject.SetActive(true);
        }
        else
        {
            opponentWinnerText.gameObject.SetActive(true);
        }

        _ = LeaveMatchAsync();
    }

    private async Task LeaveMatchAsync()
    {
        await Task.Delay(3000);
        GameManager.Instance.ShowTitleScreen();
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
