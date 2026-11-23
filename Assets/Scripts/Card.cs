using UnityEngine;
using UnityEngine.UI;

public enum CardType
{
    Rock,
    Paper,
    Scissors
}

[RequireComponent(typeof(HandCardHandler))]
public class Card : MonoBehaviour
{
    public CardType cardType;
    public bool isRevealed;

    [SerializeField] private Image image;
    [SerializeField] private Sprite rockSprite;
    [SerializeField] private Sprite paperSprite;
    [SerializeField] private Sprite scissorsSprite;
    [SerializeField] private Sprite backSprite;
    
    private HandCardHandler handCardHandler;

    public void Init(CardType type, bool faceUp, bool allowInteraction)
    {
        cardType = type;
        isRevealed = faceUp;
        SetSprite();
        ToggleInteraction(allowInteraction);
    }

    public void Flip(bool faceUp)
    {
        isRevealed = faceUp;
        SetSprite();
    }

    public void ToggleInteraction(bool allowInteraction)
    {
        handCardHandler.enabled = allowInteraction;
    }

    private void Awake()
    {
        handCardHandler = GetComponent<HandCardHandler>();
        SetSprite();
    }

    private void SetSprite()
    {
        image.sprite = GetSprite();
    }

    private Sprite GetSprite()
    {
        if (!isRevealed)
        {
            return backSprite;
        }

        switch (cardType)
        {
            case CardType.Rock:
                return rockSprite;
            case CardType.Paper:
                return paperSprite;
            case CardType.Scissors:
                return scissorsSprite;
        }

        return null;
    }
}
