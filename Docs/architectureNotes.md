# Architecture notes

## Additions/changes
Because it seemed interesting and I wanted to explore, I took a few steps further than the test description.
1. Handle multiple matches from different players on the server (not possible with the MockServer running inside the game, but interesting to think about.)
2. Make the server opponent more lifelike, in order to exercise some concerns that would come up in a pvp game.
   - The opponent "thinks" for a random delay before selecting a card, then the player's client responds to that async event.
   - I also have the opponent communicating via MatchClient, just like the player's real MatchManager. It would be less indirection to hook the bot directly into the Match game code - but using MatchClient gives a trail of commands and events exactly as if two players were playing.
3. Added a history display of previous rounds so you can remember what the opponent already threw. (Besides looking terrible like the rest of the UI, the history is implemented hackily for now - oh well.)

## Server-client communication outline
- The client sends commands when the user does something (like selects a card.)
- The server handles the implications of that, calculates the new game state, and then fires an event reflecting that something happened (cardSelected).
- So the original client should wait for the server before moving the card the user clicked. (Would be nice to show a pending state but didn't get to it!)
- Events are broadcast to any participant in the match, aka, it won't tell the player "You selected a card", it will say "[username] selected a card" and the player client figures out how to represent that in the UI.
- All server events include the latest match state. That way clients don't have to reconstruct where they should be using their own copy of game logic, they can check against the up-to-date state. (Although see Todo #1, this part isn't fully baked yet.)

## UI
Not the strongest part of this game since I was more interested in the above topics. Besides that it should look less ugly, better interactions and animations would also help communicate what's going on to the player.

Note: I wasn't sure if this was expected, but I used a Canvas for everything - since it's all flat, and that way I could lay things out with anchors.


## GenAI usage
I worked with GenAI (GPT-6.1 Sol in VS Code) to work with the scripts throughout. In this case, since I was approaching this as a chance to explore and learn more about Unity myself, I wanted to be the one to write the code so I didn't let it make changes as an agent.
- Before any coding or consulting the LLM I wrote down my design thoughts in [initialDesign.md](initialDesign.md) so I could think it through.
- I used the Copilot chat to ask questions about Unity, from what's the difference between Awake and Start on up, and some about C#, like how to manage typing for events of different unknown types. Typically this was information, sometimes I got a code example I then used.
- I had inline text suggestions on and used that to save time and remember the syntax when writing code.
- At the end I did use the full agent to make an edit - adding cancellation tokens and dispoal code to the match-related scripts.

## Todo (if I were spending more time on it)
1. The biggest is managing the UI in a safe way based on incoming events. This is even more of a concern in real life if there's a risk of events being missed and the UI having to catch up.
   - Right now it's mostly imperative, with incoming events triggering cards to move to different areas.
   - We could also do it declaratively, where we make sure the UI always reflects the latest server state on the latest event.
     - But this clashes with the idea of adding animations, pauses, etc. - those could mean the view lags behind the latest model by design.
   - I think it's some kind of queue - either a queue of states and we know how to animate between states, or a queue of UI actions running on their own layer separate from the incoming gameplay events... Not sure.
2. One weakness is cards are handled by type (rock, etc), but it prevents the game from expanding to allow duplicates in the deck which would be interesting, and generally feels a little iffy. So identifying cards could be worth revisiting.
3. Error handling and hardening, handling disconnections etc.
4. Obviously, the UI could be much much better so the game is clear and exciting to play.