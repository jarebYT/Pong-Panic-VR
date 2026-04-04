# ⚡ QUICK START CHECKLIST - 5 MINUTES

Suivez cette checklist pour intégrer rapidement le code.

## STEP 1: Vérifier les Scripts (1 min)
- [ ] Allez dans `Assets/Scripts/`
- [ ] Vérifiez que ces fichiers existent:
  - `Player.cs` ✅ (refactorisé)
  - `Ball.cs` ✅ (corrigé)
  - `PingPongManager.cs` ✅ (réécrire)
  - `BallManager.cs` ✅ (NOUVEAU)
  - `AimAssist.cs` ✅ (NOUVEAU)
  - `ServiceHandler.cs` ✅ (NOUVEAU)
  - `GameManager.cs` (inchangé, laissez comme est)

---

## STEP 2: Créer la Structure de Scène (2 min)

### 2a. GameManager
```
Create Empty GameObject
  Name: GameManager
  Position: (0, 0, 0)
  
  Add Components:
    ✅ PingPongManager (Script)
    ✅ BallManager (Script)
```

### 2b. Players
```
Create 2 Empty GameObjects:
  Name: Player1, Position: (-2.7, 0, 0)
  Name: Player2, Position: (2.7, 0, 0)

Pour chaque joueur:
  ├─ Create Child: Paddle1/Paddle2
  │   └─ Add Mesh (rectangular paddle)
  │   └─ Add Box Collider
  │   └─ Tag: Paddle
  │
  ├─ Create Child: SideCollider1/2
  │   └─ Add Box Collider (côté gauche/droit)
  │   └─ Is Trigger: ✅ ON
  │
  ├─ Create Child: ServicePoint1/2
  │   └─ Empty GameObject (juste une position)
  │
  └─ Attach Script: Player.cs
     └─ Assign in Inspector:
        - Paddle: [drag Paddle1/2]
        - Side Collider: [drag SideCollider component]
        - Service Point: [drag ServicePoint1/2]
```

### 2c. Table
```
Create GameObject: Table
  Position: (0, 0, 0)
  
  Add:
    └─ Mesh (ping pong table)
    └─ Box Collider (size: 1.525 x 0.05 x 2.74)
       └─ Is Trigger: ❌ OFF
    └─ Tag: Table
```

### 2d. Ground
```
Create GameObject: Ground
  Position: (0, -1, 0)
  Scale: (10, 0.1, 10)
  
  Add:
    └─ Box Collider
       └─ Is Trigger: ❌ OFF
    └─ Rigidbody
       └─ Body Type: Static
    └─ Tag: Ground
```

---

## STEP 3: Créer Ball Prefab (1.5 min)

```
Create GameObject in Scene: Ball
  Position: (0, 1, 0)
  
  Add Components:
    ✅ Mesh Filter + Sphere Mesh
    ✅ Mesh Renderer
    ✅ Rigidbody
       └─ Mass: 0.0027
       └─ Drag: 0.1
       └─ Collision Detection: Continuous
    ✅ Sphere Collider (radius 0.02)
       └─ Is Trigger: ❌ OFF
    ✅ Ball (Script)
    ✅ AimAssist (Script)
    ✅ ServiceHandler (Script)
    ✅ XRGrabInteractable (pour VR)
       └─ Add si manquant: Add Component → XRGrabInteractable

Créez Prefab:
  1. Select Ball in Hierarchy
  2. Drag vers Assets/Prefabs/ (créez dossier si besoin)
  3. Delete Ball de la scène
```

---

## STEP 4: Assigner les Références (1 min)

### Sur GameManager > PingPongManager
```
Player 1: [drag Player1 GameObject]
Player 2: [drag Player2 GameObject]
Ball Manager: [drag BallManager component]
Win Score: 11
Win Margin: 2
```

### Sur GameManager > BallManager
```
Ball Prefab: [drag le prefab Ball depuis Assets/Prefabs/]
Ping Pong Manager: [drag le component PingPongManager]
```

### Sur Ball Prefab > AimAssist
```
Enable Aim Assist: ✅ ON
Assist Force Multiplier: 0.15
Table Width: 1.525
Table Center: (0, 0, 0)
```

---

## STEP 5: Tags & Vérification Final (0.5 min)

### Créer Tags
```
Edit → Project Settings → Tags & Layers
Add: Paddle, Table, Ground
```

### Assigner Tags
```
Paddle1, Paddle2: Paddle
Table: Table
Ground: Ground
```

### Final Checklist
```
☑ PingPongManager assigné Player1, Player2, BallManager
☑ BallManager assigné Ball Prefab (pas instance)
☑ Ball Prefab dans Assets/Prefabs/
☑ Tous les colliders: Is Trigger correctement (OFF pour physics, ON pour triggers)
☑ Tags assignés (Paddle, Table, Ground)
☑ XRGrabInteractable sur Ball ou scene setup
☑ SideColliders sont des Trigger (Is Trigger = ON)
```

---

## STEP 6: Test Rapide

```
1. Play the scene
2. Debug console doit afficher:
   ✅ "Game Initialized - Player1 serves first"
   ✅ Au lieu de crash ou null reference

3. Grab ball, serve:
   ✅ "Ball grabbed for serve"
   ✅ "Game started!" après 2 touches correctes

4. Si point:
   ✅ "Point to Player2!" ou "Point to Player1!"
   ✅ Score updated
   ✅ New ball spawned
```

---

## 💡 TROUBLESHOOTING RAPIDE

| Problème | Solution |
|----------|----------|
| NullReferenceException | BallManager.Ball Prefab pas assigné |
| Balle pass through table | Table collider: Is Trigger = OFF ✅ |
| Score ne monte pas | Tags manquants (Paddle, Table, Ground) |
| Balle disparaît | Ground collider: Is Trigger = OFF ✅ |
| Aim Assist trop fort | Réduire Assist Force Multiplier à 0.10 |
| Service ne marche pas | Ball doit avoir XRGrabInteractable |

---

## 📊 Verification Rapide de la Structure

Ouvrez Game.unity → Hierarchy:

```
✅ GameManager
   ✅ PingPongManager (with Player1, Player2, BallManager)
   ✅ BallManager (with Ball Prefab, PingPongManager)
✅ Player1
   ✅ Paddle1 (Tag: Paddle)
   ✅ SideCollider1 (Is Trigger: ON)
   ✅ ServicePoint1
✅ Player2
   ✅ Paddle2 (Tag: Paddle)
   ✅ SideCollider2 (Is Trigger: ON)
   ✅ ServicePoint2
✅ Table (Tag: Table)
✅ Ground (Tag: Ground)

❌ Ball ne doit PAS être dans la scène (SpawnedByBallManager)
```

---

## ✨ YOU'RE READY!

Si tout est coché ✅, appuyez sur Play!

Console doit montrer:
```
Game Initialized - Player1 serves first
```

Pas d'erreur? C'est bon! 🎉

Besoin d'aide? Lire GUIDE_SETUP_COMPLET.md ou RESUME_CORRECTIONS.md
