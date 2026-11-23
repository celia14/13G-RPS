using UnityEngine;

public class PlayArea : MonoBehaviour
{
    [SerializeField] private Transform playerSlot;
    [SerializeField] private Transform opponentSlot;

    public void MoveCardToPlayerSlot(Card card)
    {
        MoveCardToSlot(card, playerSlot);
    }

    public void MoveCardToOpponentSlot(Card card)
    {
        MoveCardToSlot(card, opponentSlot);
    }

    private void MoveCardToSlot(Card card, Transform slot)
    {
        if (slot.childCount > 0)
        {
            Debug.LogWarning("Replacing existing card in slot");
            Destroy(slot.GetChild(0).gameObject);
        }

        card.Flip(false);
        card.ToggleHandInteractions(false);
        card.TogglePlayedInteractions(true);
        card.transform.SetParent(slot, false);
        card.transform.localPosition = Vector3.zero;
    }
}
