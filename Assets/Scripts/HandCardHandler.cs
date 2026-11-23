using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles user interactions with cards in hand. Enable on hand cards only.
/// </summary>
[RequireComponent(typeof(Card))]
public class HandCardHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public event System.Action<Card> Clicked;

    [SerializeField] private float hoverOffset = 20f;

    private Card card;
    private Transform cardImageTransform;

    private Vector3 imagePosition;

    private void Awake()
    {
        card = GetComponent<Card>();
        cardImageTransform = card.Image.transform;
    }

    // Click
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isActiveAndEnabled)
        {
            Clicked?.Invoke(card);
        }
    }

    // Hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        cardImageTransform.transform.localPosition = imagePosition + new Vector3(0, hoverOffset, 0);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        cardImageTransform.transform.localPosition = imagePosition;
    }

    public void OnEnable()
    {
        imagePosition = cardImageTransform.transform.localPosition;
    }

    public void OnDisable()
    {
        // Make sure to reset when interactions are disabled
        cardImageTransform.transform.localPosition = imagePosition;
    }
}
