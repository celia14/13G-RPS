using UnityEngine;

public enum CardType
{
    Rock,
    Paper,
    Scissors
}

[RequireComponent(typeof(SpriteRenderer))]
public class Card : MonoBehaviour
{
    public CardType cardType;
    public bool isFaceUp;

    [SerializeField] private Sprite rockSprite;
    [SerializeField] private Sprite paperSprite;
    [SerializeField] private Sprite scissorsSprite;
    [SerializeField] private Sprite backSprite;

    private SpriteRenderer spriteRenderer;

    public void Init(CardType type, bool faceUp)
    {
        cardType = type;
        isFaceUp = faceUp;
        SetSprite();
    }

    public void Flip()
    {
        isFaceUp = !isFaceUp;
        SetSprite();
    }

    // Awake is called during initialization, before Start
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetSprite();
    }

    private void SetSprite()
    {
        spriteRenderer.sprite = GetSprite();
    }

    private Sprite GetSprite()
    {
        if (!isFaceUp)
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
