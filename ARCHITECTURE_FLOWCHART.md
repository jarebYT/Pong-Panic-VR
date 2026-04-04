# 🏗️ ARCHITECTURE & FLUX DE DONNÉES

## Communication Architecture Diagram

```
┏═══════════════════════════════════════════════════════════════════════════┓
║                         PONG PANIC VR ARCHITECTURE                        ║
└═══════════════════════════════════════════════════════════════════════════┘

                              GameManager (Empty)
                                    │
                ┌───────────────────┼───────────────────┐
                │                   │                   │
         PingPongManager      BallManager         GameManager
         (Game Logic)        (Ball Lifecycle)     (Scene Management)
                │                   │
                │                   │
         ┌──────┴────────┐    ┌────┴──────┐
         │               │    │           │
      Player1         Player2  Ball       │
      (Score,Paddle) (Score)   (Physics)  │
         │               │    │           │
         ├─ Paddle1      ├─Paddle2 ├─AimAssist
         ├─ SideCollider1├─SideCollider2─├─ServiceHandler
         ├─ ServicePoint1├─ServicePoint2 ├─Rigidbody
         └─ Counters     └─ Counters     └─Collider


🔄 EVENT FLOW:
═════════════

1. SERVICE PHASE:
   ═════════════════════════════════════════════════════════════
   
   Player grabs Ball
          │
          ├─→ ServiceHandler.OnGrabbed()
          │    └─→ Rigidbody.isKinematic = true
          │
   Player positions & releases
          │
          ├─→ ServiceHandler.OnReleased()
          │    └─→ rb.velocity = hand velocity
          │
   Ball leaves hand
          │
          ├─→ Ball.OnCollisionEnter(table)
          │    └─→ Ball.HandleServiceTableHit()
          │        └─→ PingPongManager.OnServiceTableHit()
          │            ├─ if (1st touch) → countServiceSideTouch++
          │            └─ if (2nd touch) → GameState = GAME ✅
          │
   OR
   
   Service Fault (> 2 touches on same side)
          │
          ├─→ Ball.OnCollisionEnter(table, same side again)
          │    └─→ Ball.HandleDoubleTouchSameSide()
          │        └─→ PingPongManager.OnDoubleTouchSameSide()
          │            └─→ PingPongManager.AwardPoint(opponent)
          │                └─→ PingPongManager.ServiceFault()
          │                    ├─ Switch server
          │                    └─ ResetRally()


2. GAME/RALLY PHASE:
   ═════════════════════════════════════════════════════════════
   
   Ball in air (traveling to opponent)
          │
   GameState = GAME
          │
   Ball hits opponent's table
          │
          ├─→ Ball.OnCollisionEnter(table, opposite side)
          │    └─→ Ball.HandleTableCollision()
          │        └─→ PingPongManager.OnTableHit()
          │            └─→ SwitchActivePlayer()
          │                └─ activePlayer can now hit
          │
   Opponent's paddle hits ball
          │
          ├─→ Ball.OnCollisionEnter(paddle)
          │    └─→ Ball.HandlePaddleCollision()
          │        ├─ if (different paddle) → PingPongManager.OnPaddleHit()
          │        │   └─ SwitchActivePlayer() or increment counter
          │        └─ if (same paddle again) → PingPongManager.OnAdditionalPaddleTouch()
          │            └─ FAULT if count > 1 → AwardPoint() + ResetRally()
          │
   Ball returns across net
          │
          ├─→ Ball.OnCollisionEnter(table, original side)
          │    └─→ Ball.HandleTableCollision()
          │        └─→ PingPongManager.OnTableHit()
          │            └─→ SwitchActivePlayer()
          │                └─ back to original player
          │
   [Rally continues until someone hits ground]


3. OUT OF PLAY (POINT AWARDED):
   ═════════════════════════════════════════════════════════════
   
   Ball hits ground
          │
          ├─→ Ball.OnCollisionEnter(ground)
          │    └─→ Ball.HandleGroundCollision()
          │        ├─ Ball.OnBallDestroyed.Invoke()
          │        │   └─ BallManager.HandleBallDestroyed()
          │        │       └─ currentBall = null
          │        │
          │        └─→ PingPongManager.OnBallOutOfPlay(lastTableSideTouched)
          │            ├─ if (on activePlayer side) → AwardPoint(inactivePlayer)
          │            └─ if (on inactivePlayer side) → AwardPoint(activePlayer)
          │                └─ AwardPoint()
          │                    ├─ player.AddScore()
          │                    ├─ CheckWinCondition()
          │                    │  └─ if (score >= 11 && lead >= 2) → EndGame()
          │                    └─ ResetRally()
          │                        ├─ GameState = SERVICE
          │                        ├─ Reset counters
          │                        └─ BallManager.SpawnNewBall()
          │                            └─ New Ball Instance
          │
   Destroy(Ball)
          │
          └─→ OnBallDestroyed event triggered
               └─→ Cleanup


🎯 AIM ASSIST PARALLEL FLOW:
═════════════════════════════════════════════════════════════

During Flight (FixedUpdate):
          │
   AimAssist checks trajectory
          │
   Predicts ball position (0.1s ahead)
          │
   if (distance_from_center > table_width/3)
          │
          ├─→ ApplyTrajectoryCorrection()
          │    ├─ Calculate correction vector
          │    ├─ Add subtle force (0.15x multiplier)
          │    └─ Keep ball near center
          │
   Visual Debug Ray (yellow)
          │
   [Ball continues with corrected trajectory]


📊 STATE TRANSITIONS:
═════════════════════════════════════════════════════════════

PingPongManager.GameState:

    ┌─────────┐
    │ SERVICE │ ◄─────┐
    └────┬────┘       │
         │            │ ServiceFault()
         │ OnServiceTableHit()  or
         │ (2 touches OK)       │ ResetRally()
         │            │        │ after point
         ▼            │        │
    ┌─────────┐       │        │
    │  GAME   │───────┘◄───────┘
    └────┬────┘       OnTableHit()
         │
         │ One player falls to ground
         │
         ▼
    ┌──────────┐
    │ INACTIVE │ (Game Over)
    │ (Winner) │ CheckWinCondition() reached
    └──────────┘


📍 COLLISION DETECTION LAYERS:
═════════════════════════════════════════════════════════════

Ball Colliders:
  ├─ Sphere Collider (Is Trigger = OFF)
  │   └─ Physical collision with Table, Paddle, Ground
  │
  └─ (Optional) Trigger Collider
      └─ Detects proximity without physics

Table Elements:
  ├─ Table Main Collider (Is Trigger = OFF)
  │   └─ Physical collision (reflects ball)
  │
  └─ SideColliders (Is Trigger = ON)
      ├─ SideCollider1 (Player1 side - all the way left)
      └─ SideCollider2 (Player2 side - all the way right)
      └─ Used ONLY to identify which side ball touched

Paddle Colliders:
  └─ Paddle Colliders (Is Trigger = OFF)
      └─ Physical collision (reflects ball, tracks paddle contact)

Ground Collider:
  └─ Ground Collider (Is Trigger = OFF)
      └─ Physical collision → Ball destroyed, point awarded


🔀 DATA FLOW SUMMARY:
═════════════════════════════════════════════════════════════

User Input (VR Grab)
        │
        ▼
ServiceHandler ────────────┐
        │                  │
        ▼                  │
Ball Physics + AimAssist  │
        │                  │
        ▼                  │
Collision Detected         │
        │                  │
        ▼                  │
Ball.OnCollisionEnter()    │
        │                  │
        ▼                  │
PingPongManager Event Handler ◄────┘
        │                  (reference from BallManager)
        ▼
State Update + Score Update
        │
        ▼
BallManager.SpawnNewBall() [if point scored]
        │
        ▼
Cycle recommence...


🎮 EXAMPLE GAME FLOW:
═════════════════════════════════════════════════════════════

TIME    EVENT                           STATE       SCORE
────────────────────────────────────────────────────────
00:00   Game Start                      SERVICE     0-0
        Player1 takes ball
        Player1 serves
        
00:05   Ball hits Table (Player1 side)  SERVICE     0-0
        countServiceSideTouch = 1
        
00:07   Ball hits Table (Player2 side)  SERVICE→    0-0
        countServiceSideTouch = 2       GAME
        GameState = GAME! ✅
        
00:15   Ball hits Paddle2               GAME        0-0
        Switch player → Player2 active
        
00:18   Ball hits Table (Player1 side)  GAME        0-0
        Switch player → Player1 active
        
00:20   Ball hits Paddle1               GAME        0-0
        Switch player → Player2 active
        
00:25   Ball hits Table (Player2 side)  GAME        0-0
        Switch player → Player1 active
        
00:28   Ball hits Ground                GAME        0-1
        (hit Player1 side)
        AwardPoint(Player2)
        ResetRally()
        
00:30   Player2 serves (lost coin flip)  SERVICE    0-1
        ...
```

---

## 🔗 REFERENCE TRACKING

### BallManager Reference Chain

```
PingPongManager
        │
        ├─ [Inspector] → BallManager
        │
BallManager
        │
        ├─ [Inspector] → Ball Prefab (Asset)
        │
        ├─ Runtime → currentBall (Active Ball Instance)
        │
        └─ SpawnBall()
            │
            └─ Instantiate(prefab)
                │
                └─ ball.SetPingPongManager(this.pingPongManager)
                    │
                    └─ Ball now has reference!


Reference Update Loop:

Spawn         Collision       Reset
  ▼             ▼              ▼
NewBall  ──→  Ball.cs  ──→  BallManager
Prefab        (has ref)      (updates)
              interacts    with PingPong
          with PingPongMgr


🚀 KEY INSIGHT:
   BallManager = Single source of truth für active ball reference!
```

---

## ✅ COMMUNICATION CHECKLIST

Every callback must follow this pattern:

```
Ball.cs
  │
  ├─→ OnCollisionEnter(collision)
  │    └─ Determines collision TYPE
  │
  ├─ HandleXxxCollision(GameObject)
  │    └─ Calls pingPongManager.OnXxxEvent(details)
  │
PingPongManager.cs
  │
  ├─→ OnXxxEvent(details)
  │    └─ Updates GameState
  │
  ├─ Updates Player counters / score
  │
  ├─ Calls BallManager.SpawnNewBall() if needed
  │
  └─ Debug.Log() for tracing

✅ Rule: Ball NEVER directly modifies scores
✅ Rule: PingPongManager calls BallManager for ball spawning
✅ Rule: All state changes logged for debugging
```
