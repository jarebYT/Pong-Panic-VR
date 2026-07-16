# Project Overview 
- Game Title: Pong Panic VR
- High-Level Concept: A highly-immersive 1v1 Virtual Reality table tennis (ping pong) simulation designed for standalone VR headsets (Oculus Quest 2). The game focuses on realistic ball physics, intuitive VR controller-based racket movements, and robust game state management.
- Players: Single player against a physical AI opponent (simulating a multiplayer 1v1 match locally).
- Inspiration / Reference Games: Eleven Table Tennis VR, Sports Scramble.
- Tone / Art Direction: Clean, low-poly, stylized forest environment.
- Target Platform: Meta Quest 2 / Standalone Android VR.
- Screen Orientation / Resolution: Standalone VR (Stereoscopic Landscape).
- Render Pipeline: Universal Render Pipeline (URP).

# Game Mechanics 
## Core Gameplay Loop
1. **Service State**: The game spawns a ball at the server's service point. The server grabs the ball, tosses it into the air, and hits it. The ball must bounce once on the server's side of the table and then once on the receiver's side.
2. **Rally State**: Once a valid serve is completed, the game transitions to the Rally phase. Both players must hit the ball back and forth. The ball must bounce exactly once on the defender's side of the table before they can hit it. Volleys (hitting the ball before a bounce) are illegal and result in an automatic point for the opponent.
3. **Point Scoring**: Points are awarded on faults (double bounce on same side, ball hitting the ground, hitting out of bounds, double hit on a paddle, or out-of-turn hits).
4. **Rotation and Win**: The serve rotates every 2 points. At 10-10 (Deuce), the serve rotates every 1 point. The first player to reach 11 points with at least a 2-point lead wins the game.

## Controls and Input Methods
- **VR Controller Tracking**: The local player controls their paddle which is bound to their physical VR Controller via `HandPaddleBinding.cs`.
- **Ball Interaction**: Grab and hold the ball using the grip/trigger buttons via `XRGrabInteractable`, then toss and release it to serve.

# UI
A world-space canvas (`GameplayUICanvas`) is positioned above the table. It has been reorganized to prevent text overlapping by separating components vertically:
- **ScoreText** (Top): Displays live scores, the serving player (🎾 indicator), and active player's turn (◀ / ▶ indicators).
- **NotificationText** (Center): Displays point-scored celebrations and fault reasons.
- **ServiceText** (Bottom): Displays pulsing/blinking service instructions (e.g. "Saisis la balle et lance !").

# Key Asset & Context
The following files and assets are modified or created to solve all gameplay issues:
1. `Assets/Scripts/Ball.cs`: Updated to safely cache `Rigidbody` in Awake/Start to prevent null references during the immediate post-instantiation lifecycle.
2. `Assets/Scripts/AimAssist.cs`: Upgraded to support a **Lunar Gravity** mode which applies a custom, gentle downward force to make gameplay slower and highly reactive in VR.
3. `Assets/Scripts/OpponentAI.cs`: Redesigned to smoothly track the ball, intercept at realistic table heights, simulate natural human-like reaction delay, and perform perfect legal serves when serving.
4. `Assets/Scripts/GameplayUIManager.cs`: Reconfigured to separate UI text components vertically in the scene to completely eliminate text overlapping.
5. `Forest.unity` (Scene): Deactivated duplicate player cameras/inputs (`XrOrigin2`) to prevent mirrored hand movements. Placed `Paddle2` at a realistic height and adjusted the text transforms on `GameplayUICanvas`.

# Implementation Steps
### Step 1: Fix Ball Spawn and Gravity/Kinematic Lifecycle Bug
- **Description**: Fix the lifecycle issue where `Ball.ResetBallState()` is called before `Start()`, causing `rb` to be null and preventing the ball from staying kinematic.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: No

### Step 2: Implement Lunar Gravity / Slow-Motion Physics on Ball
- **Description**: Add a custom lunar gravity force to the ball prefab. When gravity is active, apply a gentle custom downward force (e.g., `-3.5f` m/s²) to make gameplay slower and highly reactive in VR.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 3: Prevent Text Overlapping on World Space Canvas
- **Description**: Vertically space out `ScoreText`, `NotificationText`, and `ServiceText` on the `GameplayUICanvas` so they never superimpose.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 4: Fix Dual Headset / Mirrored Controller Movement Bug
- **Description**: Deactivate `XrOrigin2` inside the `Player2` hierarchy in the scene. This removes the duplicate visual hands/controllers tracking the player's physical hands, leaving only the Player 1 controls active.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 5: Redesign Opponent AI and Reset Table Height
- **Description**: Reconfigure `Paddle2` initial height in the scene near table level. Rewrite `OpponentAI.cs` to smoothly move the paddle on X and Z while keeping Y controlled at table height.
- **Assigned role**: developer
- **Dependencies**: Step 1, Step 4
- **Parallelizable**: No

# Verification & Testing
1. **Lifecycle Check**: Verify that when the ball spawns, it remains perfectly stationary at the service point until grabbed by the player.
2. **Physics Check**: Verify that when the ball is thrown, it falls in slow motion (lunar gravity) and bounces smoothly off the table and rackets.
3. **UI Check**: Verify that scores, turn indicators (🎾, ◀ / ▶), notifications, and service instructions are clearly legible and do not overlap.
4. **Mirroring Check**: Verify that no duplicate visual hands are tracking the local player in parallel.
5. **AI Check**: Verify that the AI reacts to the ball, hits it back smoothly, and serves legally when it's its turn to serve.
