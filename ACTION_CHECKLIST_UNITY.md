# ✅ ACTION CHECKLIST - EXACT STEPS IN UNITY

**OBJECTIF:** Setup complet en 15-20 minutes. Zéro ambiguïté.

---

## ÉTAPE 1️⃣: VÉRIFIER XRORIGIN (2 min)

### 1.1 - Cherche dans Hierarchy:
- [ ] Clique sur "XR Origin" (ou "XROriginCamera")
- [ ] Expand tous les enfants (Ctrl+Click l'arrow)
- [ ] Tu dois voir:
  ```
  XR Origin
  ├── Camera Offset
  │   ├── Main Camera
  │   ├── LeftController (ou Left Hand ou GrabInteractor [L])
  │   └── RightController (ou Right Hand ou GrabInteractor [R])
  ```

### 1.2 - Si tu ne vois PAS ça:
```
EITHER:
A) Window → XR → Device Simulator → (importer les samples)
B) Aller dans Samples folder:
   Assets → Samples → XR Interaction Toolkit → Starter Assets
   → Drag "XR Rig" scene dans ta scène
C) Assets → VRTemplateAssets → Scenes → VRScene (si existe)
```

### 1.3 - Vérifier les noms des Controllers:
- [ ] Left Controller GameObjects MUST contain "Left" dans le name
  - Exemple valide: `LeftController`, `Left Hand`, `XR Hand Left`, etc.
  - Exemple invalide: `Controller`, `Hand`, `Grip`
  
- [ ] Right Controller GameObjects MUST contain "Right" dans le name
  - Exemple valide: `RightController`, `Right Hand`, `XR Hand Right`, etc.

**Si noms sont mauvais:** Rename-les!
(Right-click → Rename ou F2)

---

## ÉTAPE 2️⃣: AJOUTER RIGIDBODY AUX PADDLES (3 min)

### 2.1 - Hierarchy → Sélectionne Paddle1
```
Hierarchy
├── Player1
│   ├── Paddle1 ← CLICK ICI
│   ├── SideCollider1
│   └── ServicePoint1
```

### 2.2 - Inspector (à droite) → Add Component
- [ ] Click "Add Component" button
- [ ] Type "Rigidbody" dans la search
- [ ] Click "Rigidbody"

### 2.3 - Configure Rigidbody pour Paddle1:
```
Inspector - Rigidbody
┌─────────────────────────────────────────┐
│ Body Type:       [⦿ Dynamic] ○ Static   │
│                                         │
│ Mass:            [0.5]                 │
│ Drag:            [0]                   │
│ Angular Drag:    [0]                   │
│                                         │
│ Use Gravity:     ☐ (UNCHECK!)          │
│ Collision Detn:  [Continuous] ⭐      │
│                                         │
│ Constraints ▼                           │
│   ☑ Freeze Position X                  │
│   ☐ Freeze Position Y                  │
│   ☑ Freeze Position Z                  │
│   ☑ Freeze Rotation X                  │
│   ☑ Freeze Rotation Y                  │
│   ☑ Freeze Rotation Z                  │
└─────────────────────────────────────────┘
```

⚠️ **IMPORTANT:**
- `Use Gravity` DOIT être ☐ (unchecked)
- `Collision Detection` DOIT être "Continuous"
- `Freeze Position X` et `Z` pour que X/Z ne changent pas

### 2.4 - Répète la même chose pour Paddle2
```
Hierarchy → Player2 → Paddle2 → Add Rigidbody
Inspector → Même configuration que Paddle1
```

---

## ÉTAPE 3️⃣: AJOUTER PADDLECONTROLLER SCRIPT (2 min)

### 3.1 - Paddle1 → Add Component
- [ ] Inspector → "Add Component"
- [ ] Type "PaddleController"
- [ ] Click "PaddleController" (le script qu'on vient de créer)

### 3.2 - PaddleController Settings pour Paddle1:
```
Inspector - PaddleController
┌──────────────────────────────────────────┐
│ CONTROLLER REFERENCE                     │
│ ├─ Controller Transform: [LEAVE EMPTY]   │
│ └─ Interactor: [LEAVE EMPTY]             │
│                                          │
│ PADDLE CONSTRAINTS                       │
│ ├─ Constrain To Table: ☑ (checked)      │
│ ├─ Min Y: [-0.4]                        │
│ ├─ Max Y: [0.4]                         │
│ ├─ Min Z: [-0.5]                        │
│ └─ Max Z: [0.5]                         │
│                                          │
│ SMOOTHING                                │
│ ├─ Use Smooth Following: ☑              │
│ └─ Smooth Speed: [15]                   │
└──────────────────────────────────────────┘
```

**Notes:**
- Controller Transform: LEAVE EMPTY (VRControllerSetup l'assignera)
- Les constraints gardent la paddle dans la bonne zone

### 3.3 - Répète pour Paddle2
```
Hierarchy → Player2 → Paddle2 → Add Component → PaddleController
Inspector → Même settings que Paddle1
```

---

## ÉTAPE 4️⃣: VÉRIFIER/ASSIGNER PLAYER SCRIPTS (2 min)

### 4.1 - Sélectionne Player1 dans Hierarchy
```
Hierarchy → Player1 ← CLICK
```

### 4.2 - Vérifier Player.cs dans Inspector:
```
Inspector - Player component
┌──────────────────────────────────────────┐
│ Score: [0]                               │
│ Paddle: [Paddle1] ⭐ MUST BE ASSIGNED   │
│ Side Collider: [SideCollider1 Collider]  │
│ Service Point: [ServicePoint1]           │
│ Paddle Controller: [Paddle1]  ⭐ NEW    │
└──────────────────────────────────────────┘
```

**Si Paddle Controller est vide:**
- [ ] Drag `Paddle1` (le GameObject) to "Paddle Controller" field

### 4.3 - Répète pour Player2
```
Hierarchy → Player2
Inspector → Player component
├─ Paddle: [Paddle2]
├─ Side Collider: [SideCollider2 Collider]
├─ Service Point: [ServicePoint2]
└─ Paddle Controller: [Paddle2]
```

---

## ÉTAPE 5️⃣: AJOUTER VRCONTROLLERSETUP (2 min)

### 5.1 - Sélectionne GameManager dans Hierarchy
```
Hierarchy → GameManager ← CLICK
```

### 5.2 - Add Component → VRControllerSetup
- [ ] Inspector → "Add Component"
- [ ] Type "VRControllerSetup"
- [ ] Click "VRControllerSetup"

### 5.3 - Configure VRControllerSetup:
```
Inspector - VRControllerSetup
┌──────────────────────────────────────────┐
│ PLAYER REFERENCES                        │
│ ├─ Player 1: [Player1] ⭐ DRAG HERE    │
│ └─ Player 2: [Player2] ⭐ DRAG HERE    │
│                                          │
│ CONTROLLER SEARCH SETTINGS               │
│ ├─ Auto Find Controllers: ☑ (checked)   │
│ └─ Use Hand Tracking: ☐ (unchecked)     │
└──────────────────────────────────────────┘
```

**Comment drag Player1 et Player2:**
1. Drag `Player1` GameObject from Hierarchy to "Player 1" field
2. Drag `Player2` GameObject from Hierarchy to "Player 2" field

---

## ÉTAPE 6️⃣: VÉRIFIER BOX COLLIDERS (2 min)

### 6.1 - Sélectionne Paddle1
```
Hierarchy → Player1 → Paddle1
```

### 6.2 - Regarde le Box Collider dans Inspector:
```
Inspector - Box Collider
┌──────────────────────────────────┐
│ Center: (0, 0, 0)               │
│ Size: (0.17, 0.1, 1.83)         │
│ Material: Default               │
│ Is Trigger: ☐ (UNCHECKED!)      │
└──────────────────────────────────┘
```

⚠️ **CRUCIAL:** `Is Trigger` = ☐ (must be unchecked)

### 6.3 - Vérifie le Ball Prefab aussi:
```
Hierarchy → Ball (ou dans Assets → Ball Prefab)
Inspector → Sphere Collider
├─ Is Trigger: ☐ (unchecked)
```

---

## ÉTAPE 7️⃣: PHYSICS PROJECT SETTINGS (2 min)

### 7.1 - Edit → Project Settings
```
Menu bar → Edit → Project Settings
```

### 7.2 - Click "Physics" in left panel
```
Project Settings window
├─ Physics ← CLICK HERE
├─ Physics 2D
└─ [Other settings...]
```

### 7.3 - Vérifier/Configurer les settings:

**Physics Section:**
```
Physics Settings
├─ Gravity: X=[0] Y=[-9.81] Z=[0]
├─ Default Material
│  ├─ Friction: [0.5]
│  ├─ Bounce Combine: [Average]
│  └─ Bounce: [0.7]
├─ Solver Settings
│  ├─ Solver Iterations: [6]
│  ├─ Solver Velocity Iterations: [2]
│  └─ Default Solver Iterations: [6]
└─ Timestep: [0.02]
```

---

## ÉTAPE 8️⃣: TEST EN EDITOR (5 min)

### 8.1 - Vérifie la Console (bottom panel):
```
Click: Window → General → Console
```

### 8.2 - Press Play ▶️
```
Top center de l'editor → PLAY button
```

### 8.3 - Regarde la Console pour les logs:
```
Console output (doit montrer):
[VRControllerSetup] Searching for XR Controllers...
[VRControllerSetup] Found Left Controller: LeftController
[VRControllerSetup] Found Right Controller: RightController
[VRControllerSetup] Assigned Left Controller to Player1
[VRControllerSetup] Assigned Right Controller to Player2
[PingPongManager] Game Initialized - Player1 serves first
```

**Si tu ne vois PAS ces logs:**
- [ ] Pause the game (espace)
- [ ] Check Console for errors
- [ ] Fix les erreurs (missing assignments, etc.)

### 8.4 - Utilise le Device Simulator:
```
Editor top → Window → XR → Device Simulator
```

Ça ouvrira une fenêtre pour simuler les contrôleurs avec la souris.

### 8.5 - Test dans Scene View:
- [ ] Bouge la souris → Paddles doivent bouger
- [ ] Ou utilise les vrais controllers en VR

---

## ÉTAPE 9️⃣: BUILD & TEST SUR QUEST 2 (5 min)

### 9.1 - File → Build Settings
```
Menu → File → Build Settings
```

### 9.2 - Configure:
```
Build Settings
├─ Scenes In Build: [ta scène avec GameManager]
├─ Platform: [Android] ← SELECT THIS
├─ Run Device: [Your Quest 2]
└─ Build & Run
```

### 9.3 - Une fois sur Quest 2:
- [ ] Mets le casque
- [ ] Tu dois voir les deux paddles
- [ ] Bouge tes mains → Paddles bougent
- [ ] Grab la balle avec une main → elle te suit
- [ ] Release → elle vole
- [ ] Utilise l'autre paddle pour retourner

---

## ÉTAPE 🔟: TROUBLESHOOTING

### ❌ Logs disent "Found Left Controller" mais paddles ne bougent pas:
**Cause:** Les Paddles n'ont pas de Rigidbody ou le PaddleController n'est pas attaché

**Fix:**
- [ ] Paddle1 → Inspector → Check for Rigidbody component
- [ ] Paddle1 → Inspector → Check for PaddleController component
- [ ] Si manquants → Add Component

---

### ❌ Console montre ERROR: "Player1 has no PaddleController"
**Cause:** Le PaddleController n'est pas assigné à Player.cs

**Fix:**
- [ ] Player1 → Inspector → Player script
- [ ] "Paddle Controller" field → Drag Paddle1

---

### ❌ Ball passe à travers la paddle:
**Cause:** Collider configuration

**Fix:**
- [ ] Paddle1 Box Collider: `Is Trigger` = ☐ OFF
- [ ] Ball Sphere Collider: `Is Trigger` = ☐ OFF
- [ ] Rigidbody Collision Detection = Continuous

---

### ❌ Paddles pas trouvées au lancement (logs say "Could not find"):
**Cause:** Noms des controllers pas correct

**Fix:**
- [ ] Right-click LeftController → Rename → Renomme avec "Left" dedans
- [ ] Right-click RightController → Rename → Renomme avec "Right" dedans
- [ ] Play again

---

## ✅ FINAL CHECKLIST AVANT DE TESTER

- [ ] Paddle1 a Rigidbody (Body Type: Dynamic)
- [ ] Paddle1 Rigidbody: Use Gravity = OFF
- [ ] Paddle1 Rigidbody: Collision Detection = Continuous
- [ ] Paddle1 a PaddleController script
- [ ] Paddle1 Box Collider: Is Trigger = OFF
- [ ] Player1.Paddle = Paddle1
- [ ] Player1.PaddleController = Paddle1
- [ ] Paddle2: Same as Paddle1
- [ ] Player2.Paddle = Paddle2
- [ ] Player2.PaddleController = Paddle2
- [ ] GameManager.VRControllerSetup.Player1 = Player1
- [ ] GameManager.VRControllerSetup.Player2 = Player2
- [ ] Auto Find Controllers = ON
- [ ] NO errors in Console when Play
- [ ] Logs show "Found Left/Right Controller"
- [ ] Logs show "Assigned ... to Player1/2"
- [ ] Physics Timestep = 0.02
- [ ] Ball Rigidbody: Collision Detection = Continuous
- [ ] Ball Sphere Collider: Is Trigger = OFF

---

**🎉 Si tu checks tout ça, ça va marcher 100%!**

