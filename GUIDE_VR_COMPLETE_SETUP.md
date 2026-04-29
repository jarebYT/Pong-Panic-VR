# 🎮 GUIDE COMPLET VR SETUP - Pong Panic VR (Quest 2)

> **Status:** Configuration Paddle + Controllers pour VR Gameplay

---

## 📋 TABLE DES MATIÈRES

1. [XROrigin Vérification](#xrorigin)
2. [Hiérarchie des Paddles](#paddle-hierarchy)
3. [Components à Attacher](#components)
4. [Configuration Inspector](#inspector-setup)
5. [Checklist de Test](#test-checklist)
6. [Troubleshooting](#troubleshooting)

---

## <a id="xrorigin"></a>✅ 1. VÉRIFIER XRORIGIN

### Cherche dans ta hiérarchie:

```
Hierarchy
├── XR Origin (ou XROriginCamera)
│   ├── Camera Offset
│   │   ├── Main Camera
│   │   ├── LeftController (XR Controller)
│   │   │   ├── Model
│   │   │   └── Raycast
│   │   └── RightController (XR Controller)
│   │       ├── Model
│   │       └── Raycast
│   └── [Other components]
```

### **Si tu ne l'as pas:**

1. Window → XR → Device Simulator (pour tester en Editor)
   OU
2. XR Interaction Toolkit → Samples → Starter Assets (importer la scène sample)

### **Vérification Importante:**

```
✅ Left Controller name contient "Left"
✅ Right Controller name contient "Right"
✅ Les deux ont XRBaseController component OU XRDirectInteractor
✅ Les deux ont Transform positionnée correctement
```

---

## <a id="paddle-hierarchy"></a>🏓 2. HIÉRARCHIE DES PADDLES

### Chaque paddle doit avoir cette structure:

```
Player1 (GameObject)
├── Paddle1 (MESH du paddle)
│   └── [Box Collider - pour collision balle]
│   └── [AJOUTER: PaddleController.cs script]
├── SideCollider1
├── ServicePoint1
└── [Player.cs script existant]

Player2 (Même structure)
├── Paddle2 (MESH)
│   └── [Box Collider]
│   └── [AJOUTER: PaddleController.cs script]
├── SideCollider2
├── ServicePoint2
└── [Player.cs script existant]
```

---

## <a id="components"></a>⚙️ 3. COMPONENTS À ATTACHER

### Sur chaque Paddle GameObject (Paddle1 et Paddle2):

**AJOUTER le composant PaddleController:**

```
Paddle1 (GameObject)
├── Mesh Filter + Renderer (existant ✅)
├── Box Collider (existant ✅)
├── Rigidbody
│   └── Body Type: Dynamic
│   └── Use Gravity: OFF ⚠️ IMPORTANT
│   └── Constraints: Freeze Rotation (XYZ)
│   └── Collision Detection: Continuous
│   └── Mass: 0.5 (très léger, pour pas slowing down la balle)
└── ✨ PaddleController (NEW SCRIPT)
```

### Sur GameManager:

**AJOUTER le script VRControllerSetup:**

```
GameManager (GameObject)
├── PingPongManager (existant ✅)
├── BallManager (existant ✅)
├── ✨ VRControllerSetup (NEW SCRIPT)
└── [Other managers]
```

---

## <a id="inspector-setup"></a>🔧 4. CONFIGURATION INSPECTOR

### **4.1 Chaque Paddle1/Paddle2 - PaddleController Settings:**

Sélectionne `Paddle1` dans la hiérarchie et configure:

```
[PaddleController script]

🎯 Controller Reference:
   ├─ Controller Transform: [LEAVE EMPTY pour auto-find]
   ├─ Interactor: [OPTIONAL - leave empty]

🎯 Paddle Constraints:
   ├─ Constrain To Table: ✅ ON
   ├─ Min Y: -0.4
   ├─ Max Y: 0.4
   ├─ Min Z: -0.5
   ├─ Max Z: 0.5

🎯 Smoothing:
   ├─ Use Smooth Following: ✅ ON
   ├─ Smooth Speed: 15
```

**Même configuration pour Paddle2** (copie les valeurs)

---

### **4.2 Player1 + Player2 - Player Settings:**

Sélectionne `Player1` et vérifie:

```
[Player script]

Paddle: Paddle1 (GameObject) ✅
Side Collider: SideCollider1 (BoxCollider component) ✅
Service Point: ServicePoint1 (Transform) ✅
Paddle Controller: Paddle1 (PaddleController component) ✨ NEW

[Player script also auto-finds PaddleController in Initialize()]
```

**Même pour Player2** (Paddle2, SideCollider2, etc.)

---

### **4.3 GameManager - VRControllerSetup:**

Sélectionne `GameManager` et configure:

```
[VRControllerSetup script]

🎯 Player References:
   ├─ Player 1: Player1 (GameObject) 📌
   ├─ Player 2: Player2 (GameObject) 📌

🎯 Controller Search Settings:
   ├─ Auto Find Controllers: ✅ ON
   ├─ Use Hand Tracking: ❌ OFF (pour Quest 2 controllers)
```

**⚠️ IMPORTANT:** Auto Find Controllers va automatiquement trouver Left/Right controllers et assigner!

---

## <a id="test-checklist"></a>✅ 5. CHECKLIST DE TEST

Avant de lancer le jeu, vérife tout ça:

### **5.1 Structure Vérifiée:**
- [ ] XROrigin existe dans la scène
- [ ] Left/Right Controllers présents et nommés correctement
- [ ] Chaque Paddle a PaddleController script
- [ ] GameManager a VRControllerSetup script
- [ ] Pas d'erreurs dans la Console

### **5.2 Components Assignés:**
- [ ] Player1.Paddle → Paddle1
- [ ] Player1.PaddleController → Paddle1 (PaddleController)
- [ ] Player2.Paddle → Paddle2
- [ ] Player2.PaddleController → Paddle2 (PaddleController)
- [ ] VRControllerSetup.Player1 → Player1
- [ ] VRControllerSetup.Player2 → Player2

### **5.3 Rigidbody Settings:**
- [ ] Paddle Rigidbodies sont Dynamic
- [ ] Use Gravity: OFF
- [ ] Collision Detection: Continuous
- [ ] Rotation: Frozen (XYZ)

### **5.4 Physics Settings (Edit → Project Settings → Physics):**
- [ ] Default Material → Friction: 0.5
- [ ] Default Material → Bounce: 0.7
- [ ] Gravity: -9.81 Y
- [ ] Timestep: 0.02 (50 FPS)

---

## 🎮 6. FLOW AU LANCEMENT

Quand tu appuies sur Play:

```
1. GameManager crée PingPongManager, BallManager, VRControllerSetup
   ↓
2. VRControllerSetup.Start()
   ├─ Cherche XRDirectInteractor (ou XRBaseController)
   ├─ Trouve LeftController et RightController
   ├─ Appelle player1.SetPaddleController(leftController.transform)
   ├─ Appelle player2.SetPaddleController(rightController.transform)
   └─ Log "Controllers assigned ✅"
   ↓
3. PingPongManager.Start()
   ├─ Appelle player1.Initialize()
   │  └─ player1 auto-trouve PaddleController si pas assigné
   ├─ Appelle player2.Initialize()
   │  └─ player2 auto-trouve PaddleController si pas assigné
   ├─ Ball spawne à ServicePoint
   └─ Game State: SERVICE
   ↓
4. PaddleController.FixedUpdate() (chaque frame)
   ├─ Lit controllerTransform.position (ta main en VR)
   ├─ Applique les constraints
   ├─ Déplace la paddle pour suivre ta main
   └─ [Balle se bounce sur la paddle qui bouge]
```

---

## <a id="troubleshooting"></a>🔧 TROUBLESHOOTING

### ❌ "Paddles don't move with my controllers"

**Cause:** Controller pas assigné ou pas trouvé

**Fix:**
1. Vérifie que Left/Right Controller existent dans XROrigin
2. Vérifie leur nom: doit contenir "Left" ou "Right"
3. Console → Cherche les logs de VRControllerSetup:
   ```
   [VRControllerSetup] Found Left Controller: ...
   [VRControllerSetup] Found Right Controller: ...
   ```
4. Si logs manquent → Controllers pas trouvés → assign manuellement dans Inspector

**Manual Assignment (si auto-find échoue):**
```
GameManager → VRControllerSetup script
├─ Auto Find Controllers: ❌ OFF
├─ Puis: SetControllers() sera appelé manuellement (code)

OU en drag-drop dans l'Inspector après trouver les controllers
```

---

### ❌ "Ball passes through paddle"

**Cause:** Paddle Rigidbody pas configuré, ou collision detection désactivée

**Fix:**
1. Paddle1 → Inspector → Rigidbody
   - Collision Detection: **Continuous** (pas "Discrete")
   - Use Gravity: **OFF**
2. Ball → Inspector → Rigidbody
   - Collision Detection: **Continuous**
3. Edit → Project Settings → Physics
   - Default Solver Iterations: 6-8
   - Default Solver Velocity Iterations: 2

---

### ❌ "Paddles are jittery/shaky"

**Cause:** Smooth Speed trop bas, ou pas assez de smoothing

**Fix:**
```
Paddle1 → PaddleController
├─ Use Smooth Following: ✅ ON
├─ Smooth Speed: 20-25 (increase)

OU utilise Rigidbody.velocity approach (déjà fait dans le script)
```

---

### ❌ "In Editor: No controllers detected"

**Normal!** L'Editor n'a pas de vrais controllers.

**Solution:**
1. Utilise le Device Simulator (Window → XR → Device Simulator)
   - Simule des controllers avec la souris
2. Build et test sur Quest 2 directement
3. Utilise Hand Tracking instead (si disponible)

---

### ❌ "Works in Editor, nothing on Quest 2"

**Cause:** Build Settings pas configurés pour Quest 2

**Fix:**
1. File → Build Settings
2. Scene: ta scene avec GameManager
3. Platform: **Android**
4. XR Plugin Management → OpenXR settings
   - ✅ Meta Quest 2
   - ✅ Hand Tracking (optionnel)
5. Build & Run

---

## 📊 EXPECTED BEHAVIOR

### ✅ Correct Setup:

```
[Quest 2 Headset]
   ↓
[Contrôleur gauche] → Paddle1 suit ta main gauche
[Contrôleur droit] → Paddle2 suit ta main droite
   ↓
Balle rebondit sur tes paddles
   ↓
Points comptés automatiquement
```

### Test Simple:
1. Grab la balle avec ta main (les deux fonctionnent)
2. Release → la balle devrait voler
3. Bouge ta main → la paddle devrait te suivre
4. Utilise la paddle pour retourner la balle

---

## 🚀 NEXT STEPS

Une fois que tout marche:

1. **Test Multi-Joueur:** 2 casques VR (2 Quest 2)
2. **IA Opponent:** Script AIOpponent pour joueur solo
3. **UI/Scoring:** Afficher le score en VR
4. **Maps:** Duplicate ta scène et change les visuels
5. **Difficulty Levels:** Paramètres selon le niveau

---

**Status:** ✅ Ready to Test on Quest 2
**Last Updated:** April 29, 2026
