Rock, Paper, Scissors
In Unity, create a simulated online card game with a client and a “server”. For the purposes
of this test the “server” can exist as a client-side simulation. No actual networking code is
required. The client and “server” should communicate with each other via json.
Communications should be logged to the unity console. All interactions should be server
authoritative.
When the app is run, the user is presented with a Login Screen. When successfully logged in,
the user should be taken to the Game Scene. Once the game is complete, the user is returned
to the login screen to repeat the process as desired.
Login Scene
● Text field for user name
● Text field for password
● Button to “Login”
○ When clicked, send the contents of both text fields to the server. Server should
then authenticate and respond to the client with a success/fail message
○ If login failed : Print “login failed” to console
○ If login success : Load the game scene
Login Credentials:
Thirteen
Games
Game Scene
User’s Hand
● 3 cards “Rock”, “Paper”, “Scissors” in a horizontal, center aligned row at the
bottom of the screen.
● The User has a numerical score in the bottom left corner of the screen initialized
to 0
Server Hand
● Opponent (server) is presented with the same 3 cards, with their values hidden
from the user aligned in a horizontal, center aligned row at the top of the
screen
● The Opponent (server) has a numerical score in the top right corner of the screen
initialized to 0
Play Space
● Between the user and server hands is the play space where played cards will be
presented.
● User’s card is always presented on the left
● Server’s card is always presented on the right

Interactions:
● When the user clicks on one of their cards, it is moved to the play space
● Server will, then, choose a card randomly from their hand that is revealed and moved to
the play space
● 1 point will be granted to the player with the winning card in the play space using
traditional “Rock, Paper, Scissors” rules. No score is awarded in the event of a tie.
● Accumulated points should display as a running total in the appropriate player’s score
area.
● Once points are awarded, the cards that were played are destroyed.
● Once all cards have been played, the winner is declared and after a short delay the user
is returned to the Login Screen