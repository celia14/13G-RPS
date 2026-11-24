using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles user interactions with played cards.
/// </summary>
[RequireComponent(typeof(Card))]
public class PlayedCardHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverOffset = 20f;

    private Card card;
    private Transform cardImageTransform;

    private Vector3 imagePosition;
    private bool wasFlipped;

    private void Awake()
    {
        card = GetComponent<Card>();
        cardImageTransform = card.Image.transform;
    }

    // Hover: peek at the card
    // This is buggy/fragile on changing interaction modes... I don't think
    // the peek should be handled by changing the card image itself, so there's
    // always a real source of truth. (But out of scope, it was just a fun idea.)
    public void OnPointerEnter(PointerEventData eventData)
    {
        cardImageTransform.localPosition = imagePosition + Vector3.up * hoverOffset;
    
        wasFlipped = card.isRevealed;
        card.Flip(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        cardImageTransform.localPosition = imagePosition;
    
        card.Flip(wasFlipped);
    }

    public void OnEnable()
    {
        imagePosition = cardImageTransform.localPosition;
        wasFlipped = card.isRevealed;
    }

    public void OnDisable()
    {
        // Make sure to reset when interactions are disabled
        cardImageTransform.transform.localPosition = imagePosition;
    
        // Reset the card to its previous revealed state
        card.Flip(wasFlipped);
    }
}
