using Contracts;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(MatchClient))]
public class MatchManager : MonoBehaviour
{
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

        // TODO populate based on state
        playerHand.PopulateHand();
        opponentHand.PopulateHand();

        SetPlayerScore(0);
        SetOpponentScore(0);

        playerHand.CardClicked += OnCardClicked;
    }

    private void OnCardClicked(Card card)
    {
        // TODO Show a "selecting" state?

        matchClient.SelectCard(card.cardType);

        //playArea.MoveCardToPlayerSlot(card);
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
