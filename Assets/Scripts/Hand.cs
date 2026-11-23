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
        card.ToggleHandInteractions(isPlayer);

        // Forward click events up
        card.GetComponent<HandCardHandler>().Clicked += OnCardClicked;
    }

    private void OnCardClicked(Card card)
    {
        CardClicked?.Invoke(card);
    }
}
