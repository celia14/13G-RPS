using UnityEngine;

public class PlayArea : MonoBehaviour
{
    [SerializeField] private Transform playerSlot;
    [SerializeField] private Transform opponentSlot;

    public void AddCardToPlayerSlot(GameObject card)
    {
        if (playerSlot.childCount > 0)
        {
            Debug.LogWarning("Replacing existing card in player slot");
            Destroy(playerSlot.GetChild(0).gameObject);
        }
        card.transform.SetParent(playerSlot, false);
    }

    public void AddCardToOpponentSlot(GameObject card)
    {
        if (opponentSlot.childCount > 0)
        {
            Debug.LogWarning("Replacing existing card in opponent slot");
            Destroy(opponentSlot.GetChild(0).gameObject);
        }
        card.transform.SetParent(opponentSlot, false);
    }
}
