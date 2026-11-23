using UnityEngine;
using UnityEngine.UI;

public enum CardType
{
    Rock,
    Paper,
    Scissors
}

// The player can interact with the card differently depending on its
// current situation, so we need to track that. Behavior is enabled and
// disabled by toggling Handler scripts.
public enum CardInteractionMode
{
    None,
    Hand,
    Played
}

[RequireComponent(typeof(HandCardHandler))]

[RequireComponent(typeof(PlayedCardHandler))]
public class Card : MonoBehaviour
{
    public CardType cardType;
    public bool isRevealed;

    [SerializeField] private Image image;
    [SerializeField] private Sprite rockSprite;
    [SerializeField] private Sprite paperSprite;
    [SerializeField] private Sprite scissorsSprite;
    [SerializeField] private Sprite backSprite;

    // Getter for image - useful for handler scripts
    public Image Image => image;
    
    private CardInteractionMode interactionMode;
    private HandCardHandler handCardHandler;
    private PlayedCardHandler playedCardHandler;

    public void Init(CardType type, bool faceUp)
    {
        cardType = type;
        isRevealed = faceUp;
        SetSprite();
    }

    public void Flip(bool faceUp)
    {
        isRevealed = faceUp;
        SetSprite();
    }

    public void SetInteractionMode(CardInteractionMode mode)
    {
        interactionMode = mode;
        handCardHandler.enabled = mode == CardInteractionMode.Hand;
        playedCardHandler.enabled = mode == CardInteractionMode.Played;
    }

    private void Awake()
    {
        handCardHandler = GetComponent<HandCardHandler>();
        playedCardHandler = GetComponent<PlayedCardHandler>();
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
