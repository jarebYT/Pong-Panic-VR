# Project Overview 
- Game Title: Pong Panic VR
- High-Level Concept: An immersive 1v1 VR ping pong simulation with high-fidelity physics, realistic dynamic sounds, and a back-to-back dual-sided gameplay scoreboard to support multiple players/VR perspectives.
- Tone / Art Direction: Professional, clean, responsive, high-performance VR table tennis.

# Game Mechanics 
## Core Gameplay Loop
- Complete 1v1 table tennis logic with service validation, turn tracking, scoring, double-bounces, volleys, and deuce advantage.
- Smooth ball physics with configurable lunar gravity for precise, enjoyable exchange pacing.

# UI
- **Double-sided (Back-to-Back) Central Scoreboard**: Located directly above the net, with one panel facing Player 1 (positive Z looking negative) and another panel facing Player 2 (negative Z looking positive). This ensures both players can see the score, notifications, and service instructions perfectly and naturally without mirrored text.

# Key Asset & Context
1. `Assets/Scripts/BallSoundController.cs`: A high-fidelity, procedurally synthesized audio generator for table tennis. It generates perfect organic "clicks", "knocks", and "thuds" on paddle, table, and ground impacts. It dynamically scales the amplitude and pitch based on collision velocity.
2. `Assets/Scripts/Ball.cs`: Integrates `BallSoundController` directly to trigger authentic physical sound feedback during collisions.
3. `Assets/Scripts/GameplayUIManager.cs`: Upgraded to manage both Player 1 and Player 2 (back-to-back) text displays in sync.
4. `Forest.unity` (Scene): Duplicates and rotates text elements on `GameplayUICanvas` to create the back-to-back panels, and registers them in the `GameplayUIManager` inspector.

# Implementation Steps
### Step 1: Create Procedural Table Tennis Sound Controller
- **Description**: Implement `BallSoundController.cs` using `OnAudioFilterRead` to generate high-fidelity, velocity-scaled ping-pong clicks (paddle), knocks (table), and thuds (ground) without depending on external files.
- **Assigned role**: developer
- **Dependencies**: None

### Step 2: Integrate Sound Controller into Ball Collisions
- **Description**: Update `Ball.cs` to call `BallSoundController.PlayCollisionSound(...)` on table, paddle, and ground hits, passing the relative collision velocity.
- **Assigned role**: developer
- **Dependencies**: Step 1

### Step 3: Upgrade GameplayUIManager for Dual-Sided Scoreboard
- **Description**: Add P2-screen serialized fields to `GameplayUIManager.cs` and update both Player 1 and Player 2 panels in sync.
- **Assigned role**: developer
- **Dependencies**: None

### Step 4: Duplicate and Configure Scene UI elements
- **Description**: Reorganize `GameplayUICanvas` in `Forest.unity` to have a Player 1 UI panel and a Player 2 UI panel rotated 180 degrees. Wire them in `GameplayUIManager`.
- **Assigned role**: developer
- **Dependencies**: Step 3

# Verification & Testing
1. **Audio Polish**: Verify that bouncing the ball on the table produces a wooden "knock", hitting the paddle produces a clean "click", and falling on the floor produces a soft "thud".
2. **UI Polish**: Verify that both player sides see the scores, countdown, and service instructions clearly and in their native direction without mirroring.
