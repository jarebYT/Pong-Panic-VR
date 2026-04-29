# 🎮 SETUP VR SIMPLE - Raquettes Fixées aux Mains

**Objectif:** Tes raquettes suivent exactement ta main en VR. Chaque joueur choisit sa propre main (gauche ou droite).

---

## 📋 ÉTAPES (5 MINUTES)

### ✅ ÉTAPE 1: Player1 → Ajouter HandPaddleBinding

1. **Hierarchy → Click "Player1"**
   ```
   Hierarchy → Player1 ← CLICK ICI
   ```

2. **Inspector → "Add Component"**
   ```
   Inspector (à droite)
   → Cherche le bouton "Add Component" (bas du panel)
   → Click "Add Component"
   ```

3. **Search "HandPaddleBinding"**
   ```
   Type dans la search: "HandPaddleBinding"
   → Tu verras le script dans la liste
   → Click dessus
   ```

4. **Configure HandPaddleBinding:**
   ```
   Après avoir ajouté le composant, tu vois:

   [HandPaddleBinding]
   ├─ Hand Selection
   │  └─ Use Left Hand: ☑ ou ☐ ← CHOIX DU JOUEUR 1
   │     (checked = main gauche, unchecked = main droite)
   │
   ├─ Visual References
   │  ├─ Paddle: [?] ← DRAG PADDLE1 ICI
   │  └─ Hide Controller Model: ☑
   ```

5. **Assign Paddle1:**
   ```
   Dans Hierarchy, cherche "Paddle1" sous "Player1"
   Drag "Paddle1" to "Paddle" field in Inspector
   ```

6. **Choisis la main du Joueur 1:**
   ```
   [HandPaddleBinding]
   Use Left Hand: ☑ (GAUCHE) ou ☐ (DROITE)
   
   → C'est au choix du joueur 1!
   → Mets ce qu'il préfère
   ```

---

### ✅ ÉTAPE 2: Player2 → Ajouter HandPaddleBinding

**Répète exactement la même chose pour Player2:**

1. **Hierarchy → Click "Player2"**
2. **Inspector → Add Component → HandPaddleBinding**
3. **Assign Paddle2**
4. **Choisis la main du Joueur 2:**
   ```
   [HandPaddleBinding]
   Use Left Hand: ☑ (GAUCHE) ou ☐ (DROITE)
   
   → Au choix du joueur 2!
   → Peut être différent du joueur 1
   ```

---

### ✅ ÉTAPE 3: Assigner HandPaddleBinding dans Player.cs

**Pour Player1:**

1. **Hierarchy → Click "Player1"**
2. **Inspector → Cherche le composant "Player"**
   ```
   Tu vois:
   [Player (Script)]
   ├─ Score: 0
   ├─ Paddle: [Paddle1]
   ├─ Side Collider: [SideCollider1]
   ├─ Service Point: [ServicePoint1]
   ├─ Hand Paddle Binding: [?] ← ASSIGN ICI
   ```

3. **Drag HandPaddleBinding component:**
   ```
   Cherche le composant "HandPaddleBinding" dans l'Inspector
   C'est juste au-dessus du Player script (ou en-dessous)
   
   Drag la ligne "HandPaddleBinding (Script)" 
   to "Hand Paddle Binding" field du Player script
   ```

**Pour Player2:**
- Répète la même chose avec Player2

---

### ✅ ÉTAPE 4: Vérifier les Tags

**IMPORTANT:** Les Paddles doivent avoir le tag "Paddle"

1. **Click "Paddle1" dans Hierarchy**
2. **Inspector (en haut) → "Tag" dropdown**
   ```
   Si tu vois "Untagged":
   - Click le dropdown
   - Select "Paddle" (ou create "Paddle" si existe pas)
   ```
3. **Répète pour Paddle2**

---

### ✅ ÉTAPE 5: Test - Press Play

1. **Press ▶️ Play button**
2. **Regarde la Console (Window → General → Console):**
   ```
   Tu dois voir:
   [HandPaddleBinding] Player1 paddle bound to LEFT hand (ou RIGHT)
   [HandPaddleBinding] Paddle parented to LeftController (ou RightController)
   [HandPaddleBinding] Controller visuals hidden
   [HandPaddleBinding] Player2 paddle bound to LEFT hand (ou RIGHT)
   [HandPaddleBinding] Paddle parented to RightController (ou LeftController)
   [HandPaddleBinding] Controller visuals hidden
   ```

3. **Regarde la Hierarchy pendant Play:**
   ```
   APRÈS Play, selon les choix:
   
   Si Player1 = LEFT, Player2 = RIGHT:
   XROrigin (Joueur 1)
   ├─ Camera Offset
   ├─ LeftController
   │  └─ Paddle1 ← Parented!
   └─ RightController
      └─ (invisible, unused)
   
   XROrigin (Joueur 2)
   ├─ Camera Offset
   ├─ LeftController
   │  └─ (invisible, unused)
   └─ RightController
      └─ Paddle2 ← Parented!
   ```

---

## 🎮 EN VR (Quest 2)

**Joueur 1 voit:** Sa raquette sur la main qu'il a choisie (LEFT ou RIGHT)
**Joueur 2 voit:** Sa raquette sur la main qu'il a choisie (peut être la même ou différente du joueur 1)

Chacun peut:
- Utiliser sa **main gauche** → `Use Left Hand: ☑`
- Utiliser sa **main droite** → `Use Left Hand: ☐`

Chacun choisit ce qui lui est le plus confortable! 👍

---

## ❌ SI ÇA NE MARCHE PAS

### Erreur: "Could not find controller"

**Vérifie:**
1. Dans Hierarchy → XROrigin existe?
   - Si non: File → Open Scene → cherche une scene avec XROrigin
   - Ou: Assets → Samples → XR Interaction Toolkit → import

2. Dans XROrigin → les controllers existent?
   ```
   XROrigin
   ├─ Camera Offset
   ├─ LeftController ← DOIT EXISTER
   └─ RightController ← DOIT EXISTER
   ```

3. Les noms contiennent "Left" et "Right"?
   - Si noms bizarres (ex: "Controller [L]"): Rename-les
   - Right-click → Rename → change en "LeftController" et "RightController"

---

### Erreur: "Paddle not visible"

**Vérifie:**
1. Paddle1 → Inspector → Mesh Renderer
   - "Enabled" doit être ☑

2. Paddle1 → localPosition après Play
   - Si c'est très loin: adjust la position du prefab

---

### Erreur: "Ball passes through paddle"

**Config des Colliders:**

Paddle1:
```
Inspector
├─ Box Collider
│  ├─ Is Trigger: ☐ (UNCHECKED)
│  └─ Size/Center: bon
│
└─ Rigidbody (si existe)
   └─ Collision Detection: Continuous
```

Ball:
```
Inspector
├─ Sphere Collider
│  └─ Is Trigger: ☐ (UNCHECKED)
│
└─ Rigidbody
   ├─ Collision Detection: Continuous
   └─ Mass: 0.0027
```

---

## ✅ CHECKLIST FINAL

- [ ] HandPaddleBinding ajouté à Player1
- [ ] HandPaddleBinding.Use Left Hand: choisi (ta préférence)
- [ ] HandPaddleBinding.Paddle = Paddle1
- [ ] Player1.Hand Paddle Binding assigné
- [ ] HandPaddleBinding ajouté à Player2
- [ ] HandPaddleBinding.Use Left Hand: choisi (sa préférence)
- [ ] HandPaddleBinding.Paddle = Paddle2
- [ ] Player2.Hand Paddle Binding assigné
- [ ] LeftController et RightController existent
- [ ] Noms contiennent "Left" et "Right"
- [ ] Play → Console montre success logs
- [ ] Paddles ont tag "Paddle"
- [ ] Box Colliders: Is Trigger = OFF
- [ ] Ball Collider: Is Trigger = OFF

---

## 🎉 RÉSULTAT

```
Joueur 1 choisit sa main (gauche ou droite)
Joueur 2 choisit sa main (gauche ou droite)
↓
Play → Console ok → Raquettes liées aux mains choisies
↓
Build sur Quest 2 × 2 → Mettez les casques
↓
Joueur 1 voit sa raquette sur sa main choisie
Joueur 2 voit sa raquette sur sa main choisie
↓
Chacun joue avec la main de son choix
↓
Balle rebondit, collisions parfaites
↓
🎮 MULTIJOUEUR VR FONCTIONNEL!
```

---

**C'est tout ce qu'il faut faire. Rien de compliqué.**

Chaque joueur choisit juste quelle main utiliser. 👍



