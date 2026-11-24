using UnityEngine;

public class PlayArea : MonoBehaviour
{
    [SerializeField] private Transform playerSlot;
    [SerializeField] private Transform opponentSlot;

    public void MoveCardToPlayerSlot(Card card)
    {
        MoveCardToSlot(card, playerSlot, true);
    }

    public void MoveCardToOpponentSlot(Card card)
    {
        MoveCardToSlot(card, opponentSlot);
    }

    public void ClearPlayerSlot()
    {
        ClearSlot(playerSlot);
    }

    public void ClearOpponentSlot()
    {
        ClearSlot(opponentSlot);
    }

    public void RevealCards()
    {
        RevealCard(playerSlot);
        RevealCard(opponentSlot);
    }

    private void MoveCardToSlot(Card card, Transform slot, bool allowInteraction = false)
    {
        if (slot.childCount > 0)
        {
            Debug.LogWarning("Replacing existing card in slot");
            Destroy(slot.GetChild(0).gameObject);
        }

        card.Flip(false);
        card.SetInteractionMode(allowInteraction ? CardInteractionMode.Played : CardInteractionMode.None);
        card.transform.SetParent(slot, false);
        card.transform.localPosition = Vector3.zero;
    }

    private void ClearSlot(Transform slot)
    {
        if (slot.childCount > 0)
        {
            Destroy(slot.GetChild(0).gameObject);
        }
    }

    private void RevealCard(Transform slot)
    {
        if (slot.childCount > 0)
        {
            var card = slot.GetChild(0).GetComponent<Card>();
            card.SetInteractionMode(CardInteractionMode.None);
            card.Flip(true);
        }
    }
}
