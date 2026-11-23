using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles user interactions with cards in hand. Enable on hand cards only.
/// </summary>
public class HandCardHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Transform cardImage;
    [SerializeField] private float hoverOffset = 20f;

    private Vector3 imagePosition;

    private void Awake()
    {
        imagePosition = cardImage.localPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        cardImage.transform.localPosition = imagePosition + new Vector3(0, hoverOffset, 0);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        cardImage.transform.localPosition = imagePosition;
    }

    public void OnDisable()
    {
        // Make sure to reset when interactions are disabled
        cardImage.transform.localPosition = imagePosition;
    }
}
