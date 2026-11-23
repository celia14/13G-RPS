using TMPro;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [SerializeField] private Hand playerHand;
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private Hand opponentHand;
    [SerializeField] private TMP_Text opponentScoreText;
    [SerializeField] private PlayArea playArea;

    void Start()
    {
        playerHand.PopulateHand();
        opponentHand.PopulateHand();
        SetPlayerScore(0);
        SetOpponentScore(0);
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
