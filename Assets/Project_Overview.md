# Pong Panic VR Technical Overview

## 1. Project Description
**Pong Panic VR** is an immersive Virtual Reality table tennis (ping pong) simulation designed for standalone VR headsets (specifically targeting Android/Quest). The project focuses on realistic ball physics, intuitive XR interactions, and a clean gameplay loop. It features multiple environments (Forest, Aquarium, Space) and is built using the Unity 6 (6000.3.9f1) engine with the Universal Render Pipeline (URP) for high-performance mobile VR rendering.

## 2. Gameplay Flow / User Loop
1.  **Boot/Lobby:** The user starts in a designated `Lobby` scene where they can familiarize themselves with XR controls and potentially select maps or equipment.
2.  **Service Phase:** The `PingPongManager` starts in the `Service` state. A ball is spawned at the active player's service point. The player must toss and hit the ball so it bounces on their side once and then the opponent's side.
3.  **Rally Phase:** Once a valid serve is completed, the game enters the `Game` state. Players must hit the ball back and forth, ensuring it bounces only once on the opponent's side.
4.  **Point Scoring:** Points are awarded based on faults (double bounces, ball out of bounds, double paddle hits). The `PingPongManager` tracks score and handles the 2-point service rotation.
5.  **Win Condition:** The game continues until a player reaches the `winScore` (default 11) with at least a 2-point lead.

## 3. Architecture
The project follows a centralized manager pattern with event-driven updates for physics interactions.

*   **PingPongManager:** The central authority for game state, scoring, and rule enforcement. It processes collision events forwarded by the ball.
*   **Ball Lifecycle:** Managed by `BallManager`, which handles instantiation and ensures the `PingPongManager` always has a reference to the active `Ball` instance.
*   **XR Binding:** `HandPaddleBinding` dynamically attaches the paddle prefab to the player's chosen XR controller (left or right) at runtime.
*   **UI Communication:** Uses a dedicated `GameplayUIManager` to handle world-space notifications and score displays, decoupling game logic from UI implementation.

`Location: Assets/Scripts`

## 4. Game Systems & Domain Concepts

### Ping Pong Physics & Rules
*   `Ball`: Monitors collisions with tags (`Table`, `Paddle`, `Ground`) and triggers events in the `PingPongManager`.
*   `PingPongManager`: Implements the state machine (`Service`, `Game`, `Inactive`) and validates rules like service side touches and rally hit counts.
*   `AimAssist`: Provides optional physics correction to help players hit the table more consistently in a VR environment.
*   `Player`: Data container for player-specific state, including their score, assigned `SideCollider`, and `ServicePoint`.

`Location: Assets/Scripts`

### XR Interaction System
*   `HandPaddleBinding`: Uses a name-based search (`Left`/`Right` + `Controller`/`Hand`) to find XR Origin components and parents the paddle to them.
*   `VRHandFingerController`: Likely manages finger animations or hand poses when holding the paddle.
*   `GrabCollisionHandler`: Specialized logic for handling physical collisions during XR grab interactions.

`Location: Assets/Scripts/General` and `Assets/Scripts`

### Environment & Cosmetics
*   `BoomboxPlaylist`: A simple audio management system for the Lobby scene.
*   `LobbyBounds`: Restricts player movement or ball containment within the lobby area.
*   `bird_movement`: Ambient AI for the Forest map environment.

`Location: Assets/Scripts/Lobby` and `Assets/Scripts/Forest`

## 5. Scene Overview
*   **XR_Core:** Contains the XR Origin, input actions, and persistent XR setup.
*   **Lobby:** The starting point and social/setup area.
*   **Maps (Forest, Aquarium, Space, etc.):** Additive or standalone scenes containing environment geometry, lighting settings, and the table tennis table setup.
*   **Game:** Likely a template or master scene that coordinates the loading of map assets and the game manager.

`Location: Assets/Scenes`

## 6. UI System
The project uses **World-Space UGUI** with **TextMesh Pro** for immersive feedback.

*   `GameplayUIManager`: Manages a world-space `Canvas` positioned above the table. It handles smooth fading of notifications using `CanvasGroup` and coroutines.
*   `ServiceUIManager`: High-level wrapper that coordinates specific instructions during the service phase (e.g., "Grab the ball and serve!").
*   **Templates:** Uses `.scenetemplate` files in `Settings/Project Configuration` to ensure UI and lighting consistency across different maps.

`Location: Assets/Scripts`

## 7. Asset & Data Model
*   **Prefabs:** The `Ball.prefab` and various `Models/Paddle` assets are the core interactive objects.
*   **Materials:** Optimized URP materials, with specific sub-folders for environment themes (Aquarium, Space, Forest).
*   **Universal Render Pipeline (URP):** Uses `Performance URP Config` specifically tuned for mobile VR (Android).
*   **Models:** Uses `.glb` and `.fbx` formats. Environment assets are organized by biome.

`Location: Assets/Materials`, `Assets/Models`, `Assets/Settings`

## 8. Notes, Caveats & Gotchas
*   **Scene Loading:** The `GameManager` script contains commented-out code for additive scene loading; verify current scene transition logic if hits are not registering.
*   **Tag Dependency:** The `Ball` script relies strictly on Unity Tags (`Table`, `Paddle`, `Ground`) to identify collision logic. If new table parts are added, they must be tagged correctly.
*   **Controller Binding:** `HandPaddleBinding` searches for strings like "Left" and "Controller" in the hierarchy. Changing the name of the XR Origin children will break paddle attachment.
*   **Service Logic:** The `Service` state requires exactly two bounces (one on the server's side, then one on the opponent's). The `countServiceSideTouch` counter in `Player` tracks this.

` — modifying this logic requires updates to `PingPongManager.OnServiceTableHit`.