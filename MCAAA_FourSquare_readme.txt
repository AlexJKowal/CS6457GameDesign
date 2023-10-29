Game Repo: 
https://github.gatech.edu/mwallis6/cs6457-mcaaa

i. Start scene file
* FourSquare

ii. How to play and what parts of the level to observe technology requirements

* Once start the game, you can use keyboard direction keys or "wsad" to control human players movement
* During service time, you can use your mouse to control target hit position by hovering your mouse to enemies' square,
You should see a reticle dot (a tiny black dot) moving with your mouse indicating the target position, 
then just click left mouse key to release the ball
* During the play, you should be able to move your human player, make sure the ball should bounce once before hitting it back
* There are some minimal scoring and victory screens developed for alpha so far, if you won 3 times you should see them

iii. Known problem areas

* Games not reset properly after victory
* Win/Loss sometimes not calculated accurately
* Multiple Ball collision sounds in some rare scenarios   
* Human Players sometimes lose control when ate the collectives in very rare situations

iv. Manifest of which files authored by each teammate:
1. Detail who on the team did what

* Matt
- Implemented initial AI behaviour and FSM for ball path prediction, creating different states for handling
lob and smash shots, picking the ball up and firing it.
- After transition to NavMeshAgent AI, made updates to AI system and wrote integration-related
code to pull back in various behaviours like ball location prediction.
- Implemented game manager functionality related to scoring tracking, level progression,
resetting game state
- Implemented per-square ground state change based on ball bouncing behaviour
- Implemented reticle aiming system
- Implemented shot clock and associated UI
- Implemented initial smash shot behaviour
- Implemented component for dictating appearance of power up collectibles
- Taking care of PR reviews.

* Jeff Duan
Focused on AI player behaviors and ball throwing logic based on newton's law, initialized GameManager etc.
Taking care of PR reviews.

* AJK
Taking care of some assets to improve game feels, such as scoreboard, fence and cartoon figures includes their animations
Taking care of PR reviews.

* Cheyenne
Taking care of audio assets, pause manager, tutorial, and celebration animations etc.
Taking care of PR reviews.

* acdowns
Taking care of scaffolding project and now focusing on isometric camera etc.


2. For each team member, list each asset implemented.

* Matt Wallis
Simple reticle aim asset, basic material swapping for ground squares / OOB plane

* Jeff Duan
Taken care of ball/squares assets

* AJK
Taken care of fences/scoreboard & cartoon figures and their animations

* Cheyenne
Taken care of most audio assets

* acdowns
take care of Scaffolding the project


3. Make sure to list C# script files individually so we can confirm
each team member contributed to code writing

* Matt Wallis
PlayerController.cs, AIPlayerController.cs, BallThrowing.cs, GameManager.cs, EventManager.cs, GroundVisualState.cs
LocalizedCanvasElement.cs, SwapMaterial.cs, RandomCollectible.cs, ReticleMovement.cs, SceneLoader.cs, 
DeletedEnemyControlScript.cs (contains FSM, but is currently repressed due to in-progress switch to NavMeshAgent)

* Jeff Duan
PlayerController.cs, AIPlayerController.cs, BollThrowing.cs, GameManager.cs, NormalDistribution.cs, Projection.cs etc.

* AJK
GameManager.cs, ScoreBoardNumSetManager.cs, PlayerController.cs etc

* Cheyenne
AudioEventManager.cs, GameManager.cs, EventSound3D.cs, EventManager.cs, TutorialManager etc.

* acdowns
AIPlayerController.cs, ResetRound.cs etc.