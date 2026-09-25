# Design Journal: Mother Pong

(PS. This MDM journal was written post finishing the game with preexisiting thoughts I had jotted down during the development in real time also from real-time git commit messages. However, I did mess the timing up out of confusion so please keep that in mind.) 

## Journal Update: Git Commit Bundle #1
* **What:** I increased the ball movement speed, attached the 'MotherHelper' script to trigger a help prompt after the 2nd paddle hit, and forced a 2-second Mother control takeover (making the paddle glow red) regardless of the user's choice.

* **So What:** This sets up the core thematic conflict where "help" masks invasive aggression. I didn't think much about adding the red glow besides wanting to visually see a change while coding, but looking back at my reflections on Chapter 2 of Pippin Barr’s *The Stuff Games Are Made Of*, I realize I felt that initial pull to explicitly direct the player to my intended narrative experience instead of letting the game's "stuff" speak for itself. I added the red because I wanted the threat to be apparent, but I didn't want it to feel too explicit. 

* **Now What:** I still feel like I should have left out the red and kept the paddle white. Keeping it white would highlight the realistic, manipulative, and subtle dynamic of an enmeshed parent, leading to a deeper feeling of shock or betrayal when the player realizes they can't control the paddle.

## Journal Update: Git Commit Bundle #2
* **What:**  Physics constraints were added so the ball moves horizontally instead of bouncing vertically, and I changed the code so Mother's control increases gradually over time.

* **So What:** Vertical bouncing allowed long wait times that gave the player passive relief, deflating the emotional tension. Restricting the ball to a horizontal bias keeps the gameplay smoother and eliminates dead time, keeping the emphasis on Mother's active changes to the system. Gradually scaling her control emphasizes the slow erosion of player input.

* **Now What:** Now that the gameplay loop is established and the gradual invasion is working, I need to focus on how the game naturally plays out into a total loss of autonomy. My focus is on how to keep it from being generic or predictable. How to keep the user experience uncomfortable. I'm trying to figure out how to "pull the rug out from under" the player so it feels like more of a slap in the face or a rising panic for the player not for shock value but to convey what an enmeshed relationship realistically develops into.

## Journal Update: Git Commit Bundle #3
* **What:** I set a hard threshold where Mother takes complete, permanent control after exactly 6 player paddle hits, leaving two bots playing against each other where they both struggle to score or win.

* **So What:** When I first ran the code and watched the stalemate, I felt like it was predictable. I worried I was taking an easy, predictable ending and getting lazy with it. I couldn't imagine how the game would progress past total control, but I knew I didn't want an explicit scripted ending but just a natural play-out of her dominance. However, sitting with the running game after I finished coding, I had a sudden realization: the opponent bot was moving exactly like the Mother-controlled paddle. This wasn't planned from day one. I was trying to think of a more explicit conclusion, but I fell in love with this discovery because it perfectly represents the dynamic, and the idea of how the opponent was a past victim of the same system, meaning this subtle cycle was hidden right in front of the player from the beginning.

* **Now What:** The project is mechanically and conceptually complete. By watching the two bots fail to beat each other, the game successfully materializes how an enmeshed person ends up in a cyclical loop with everyone in their life. 


## Tooling Disclosure
* **Code & Comments:** AI assistance was used to help implement certain portions of the codebase (such as providing the horizontal ball constraints, complex math logic, and scaffolding for the MotherHelper logic) and to generate inline comments for the scripts.
