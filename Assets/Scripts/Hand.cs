using Contracts;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public event System.Action<Card> CardClicked;

    public bool isPlayer;

    [SerializeField] private Card cardPrefab;

    private Transform handTransform;

    public void PopulateHand()
    {
        // Always populate one of each type
        CardType[] types = (CardType[])System.Enum.GetValues(typeof(CardType));
        
        // Shuffle
        for (int i = 0; i < types.Length; i++)
        {
            int randomIndex = Random.Range(i, types.Length);
            (types[randomIndex], types[i]) = (types[i], types[randomIndex]);
        }

        foreach (CardType type in types)
        {
            AddCard(type);
        }
    }

    public void ClearHand()
    {
        foreach (Transform child in handTransform)
        {
            Destroy(child.gameObject);
        }
    }

    // Find a card by type. This assumes that there is only one card of each type in the hand.
    public Card GetCard(CardType type)
    {
        foreach (Transform child in handTransform)
        {
            Card card = child.GetComponent<Card>();
            if (card != null && card.cardType == type)
            {
                return card;
            }
        }
        return null;
    }

    private void Awake()
    {
        handTransform = transform;
    }

    private void Start()
    {
        //PopulateHand();
    }

    private void AddCard(CardType type)
    {
        Card card = Instantiate(cardPrefab, handTransform);
        card.Init(type, isPlayer);
        card.SetInteractionMode(isPlayer ? CardInteractionMode.Hand : CardInteractionMode.None);

        // Forward click events up
        card.GetComponent<HandCardHandler>().Clicked += OnCardClicked;
    }

    private void OnCardClicked(Card card)
    {
        CardClicked?.Invoke(card);
    }
}
