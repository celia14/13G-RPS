using UnityEngine;

/// <summary>
/// Displays the previous rounds. This is very hacky, I just wanted it when playing.
/// Really the history should be part of the match's state and flow from the
/// authorative state instead of just moving UI objects.
/// </summary>
public class History : MonoBehaviour
{
    [SerializeField] private GameObject historyRoundPrefab;
    [SerializeField] private Card cardPrefab;

    public void MoveToHistory(Card playerCard, Card opponentCard)
    {
        var historyRound = Instantiate(historyRoundPrefab, transform);
        var playerSlot = historyRound.transform.Find("Player Slot");
        var opponentSlot = historyRound.transform.Find("Opponent Slot");
    
        MoveCardToSlot(playerCard, playerSlot);
        MoveCardToSlot(opponentCard, opponentSlot);
    }

    private void MoveCardToSlot(Card card, Transform slot)
    {
        card.transform.SetParent(slot, false);
        card.Flip(true);
        card.SetInteractionMode(CardInteractionMode.None);
        card.transform.localPosition = Vector3.zero;

        // Set the card to 50 x 75
        var rect = card.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(50f, 75f);
    }
}
