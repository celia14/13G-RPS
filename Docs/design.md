This is my overall design before getting into the details of setting it up in Unity.

# Gameplay

## Terms
- throw = each player plays a card which go head to head
- match = the 3-play showdown

## State
(Pseudo TS)
```
PlayerId: string;

RpsMatchState: {
    playerIds: PlayerId[];
    playerStateById: Record<PlayerId, RpsMatchPlayerState>;
    throw: int; // ??
    status: 'playing' | 'complete'; // ??
}

RpsMatchPlayerState: {
    score: int;
    hand: Card[]; // selected card removed
    selected: Card | undefined; // the card to throw
}

CardType: 'rock' | 'paper' | 'scissors';

// Making it an object in case of future enhancements
Card: {
    type: CardType;
}
```

### MatchManager
A global manager tracking the player and opponent states, managing user input, and rendering the outcome.
- At any point, it should be able to render the current state idempotently.
- It talks to scripts that are responsible for the UI.

## Client-server flow
Server authoritative meaning the match state is tracked on the server, user actions are sent to the server, and the results are communicated back.

### Communications
The communication needs to be bidirectional: the player takes actions that change the state, but the opponent also takes actions that need to be communicated to the player.

So it's a WebSocket-style live connection with pub/sub events.

### Events considerations

Events cover what happened in the match and aren't from a particular player's point of view - each client can sort out whether the event is about them or their opponent and render that.

It should be possible (hypothetically) to replay the whole match via the events.

Seems easiest to have each event carry the entire new state as well as the details of the event.

From the client's point of view it's like: state A -> receive event containing Event X and new state B -> immediately update current state to B but handle the rendering of X first. That way if the connection is interrupted we know we still have the correct state.

### Flow

1. Player starts a match -> server initializes the match and establishes the connection
   - Two players connecting is out of scope - the server will always init the simulated opponent
2. On receiving the init message, client renders the match
3. Player clicks a card to play -> send to server, which updates the playing card in the state -> server emits SelectCard event -> client renders the selected card
   - Thinking the click triggers a "selecting" visual state but we wait for the server confirmation to show "selected"
4. On the server, opponent selects a card (for the simulated opponent, this is at random and with a random delay for realism) -> SelectCard event -> client renders the opponent's selection
5. Once both cards are selected, resolve the throw
   - I think the server should (immediately) emit a ResolveThrow event after both players have selected a card.
   - This events tells us the new state, who won, plus each selected card. That way if there's an issue with the previous selection events we can just catch up.
6. Client animates the selected cards and resolution and updates the score.
7. Client moves on to the next throw, or declares the winner. Server kicks off the simulated opponent's next throw, or wraps up the match.

### Events
```
Event: {
    newState: RpsMatchState;
}

SelectCardEvent extends Event: {
    playerId: PlayerId;
    selectedIndexInHand: int; // We already have the new state, this is to enable animation, so hand index seems easiest
}

ResolveThrowEvent extends Event: {
    winnerId: PlayerId;
    thrownCardByPlayerId: Record<PlayerId, Card>; // In case we need to recreate the throw animation
}

```

## Display

Struggling a little with how to manage the UI, need to learn more about the right Unity way to do it.

The MatchManager should coordinate everything, but scripts on the different elements can run animations etc.

### Hand & HandCards
Hand object has a HandController script. I don't think we need to distinguish between player and opponent in the hand controller. The main difference for now is whether the cards are OpponentHandCards or PlayerHandCards...
- PlayerHandCards are face up
- They tell the MatchManager about a card being selected when they're clicked
- They also react on hover

The MatchManager tells the hands what cards to render and when to select a card (which moves it out of hand to the play area.) I think that the MatchManager should just have a direct reference to the two hands.

I'd put the Hand in charge of animating the card from the hand to the PlayArea, and then... letting the MatchManager know it's done, and the MatchManager informs the PlayArea?

### PlayArea
Selected cards are moved face down into the PlayArea. When the throw is resolved, they're flipped face up (and some kind of amazing clash animation plays (out of scope)).

I think the MatchManager holds a direct reference to the PlayArea and tells it what to do.

### Timing question

Assuming we have animations, or even if not, the player needs time to process what happened:

Player and opponent SelectCard events from the server should be able to be rendered in parallel, but both animations should be done before starting to animate the throw resolution.

From the MatchManager's point of view, it should process events immediately and know the final state of the model, but we need someone to be in charge of tracking the animation state and starting the throw resolution in the PlayArea.

Maybe it's the PlayArea that knows when it's ready to resolve but waits for a signal that the cards are in place?

One option is to get around this hackily by adding a delay on animating the resolve long enough to show a card is selected (since it must be precipitated by a player selecting a card, that delay is pretty fixed.)
