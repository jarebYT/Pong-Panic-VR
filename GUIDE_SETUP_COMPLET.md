# 🎮 GUIDE COMPLET D'INTÉGRATION - Pong Panic VR

## 📋 TABLE DES MATIÈRES
1. [Architecture Générale](#architecture)
2. [Setup des GameObjects](#gameobjects)
3. [Configuration des Scripts](#scripts)
4. [Tags et Layers](#tags)
5. [Physics Settings](#physics)
6. [Checklist de Vérification](#checklist)

---

## <a id="architecture"></a>📐 ARCHITECTURE GÉNÉRALE

### Hiérarchie de Scène Recommandée
```
Scene (Game.unity)
├── GameManager (GameObject vide)
│   ├── PingPongManager (SCRIPT)
│   ├── BallManager (SCRIPT)
│   └── SceneManager (SCRIPT existant)
├── Players
│   ├── Player1 (GameObject)
│   │   ├── Paddle1 (Mesh)
│   │   ├── SideCollider1 (Collider)
│   │   └── ServicePoint1 (Position)
│   └── Player2 (GameObject)
│       ├── Paddle2 (Mesh)
│       ├── SideCollider2 (Collider)
│       └── ServicePoint2 (Position)
├── Table
│   ├── TableMesh (Mesh)
│   └── TableCollider (Collider - IMPORTANT)
├── Ball (Prefab Instance à générer)
│   ├── Sphere (Mesh)
│   ├── Rigidbody
│   ├── SphereCollider
│   ├── Ball (SCRIPT)
│   ├── AimAssist (SCRIPT)
│   └── ServiceHandler (SCRIPT)
└── Ground (Collider invisible)
```

---

## <a id="gameobjects"></a>🎯 CONFIGURATION DES GAMEOBJECTS

### **1. GameManager (GameObject parent)**

```
Name: GameManager
Position: (0, 0, 0)
```

Attachez à ce GameObject:
- ✅ **PingPongManager** (Script)
- ✅ **BallManager** (Script)

---

### **2. Player1 (GameObject)**

```
Name: Player1
Position: (-2.7, 0, 0)  // Côté gauche de la table
```

**Hiérarchie enfants:**

#### **2a. Paddle1 (Mesh + Collider)**
```
Name: Paddle1
Parent: Player1
Position: Local (0, 0, 0)
Mesh: Rectangular paddle (0.17m x 0.1m x 1.83m)
```
**Composants:**
- Mesh Filter + Mesh Renderer (affichage)
- Box Collider (IMPORTANT: actif pour détection collision)
- ❌ Pas de Rigidbody

**Tag:** `Paddle`

#### **2b. SideCollider1 (Collider invisible)**
```
Name: SideCollider1
Parent: Player1
Position: Local (0, 0, 0)
```
**Composants:**
- Box Collider (côté gauche de la table)
  - Taille: (~0.3m x 0.05m x 3m)
  - Position: décalée vers le côté gauche
  - ❌ Is Trigger: OUI
  - ❌ Pas de Rigidbody

**But:** Identifier le côté du joueur pour savoir qui perd le point

#### **2c. ServicePoint1 (Empty GameObject)**
```
Name: ServicePoint1
Parent: Player1
Position: Local (0, 0.5, -0.5)  // Légèrement en l'air devant le joueur
```
Sert de position de spawn pour la balle lors du service

#### **2d. Player Script**
```
Attachez: Player.cs au GameObject Player1
```

Assignez dans l'Inspecteur:
- Paddle: Paddle1 (GameObject)
- Side Collider: SideCollider1 (Box Collider component)
- Service Point: ServicePoint1 (Transform)
- Score: 0 (initialisé automatiquement)

---

### **3. Player2 (Exactement comme Player1 en miroir)**

```
Name: Player2
Position: (2.7, 0, 0)  // Côté droit
```

Répétez la même hiérarchie que Player1 mais du côté opposé:
- Paddle2
- SideCollider2
- ServicePoint2
- Script Player attaché

⚠️ **IMPORTANT:** Les colliders des joueurs doivent être sur les côtés OPPOSÉS

```
SideCollider1 (Player1): X = -1.5 (côté gauche)
SideCollider2 (Player2): X = +1.5 (côté droit)
```

---

### **4. Table (Zone de jeu)**

```
Name: Table
Position: (0, 0, 0)
```

**Composants:**
- Mesh Filter + Mesh Renderer (affichage table)
- ⚠️ **TRÈS IMPORTANT: Box Collider**
  - Taille: (1.525m x 0.05m x 2.74m) [dimensions réelles ping pong]
  - Is Trigger: ❌ NON (collision physique)
  - ❌ Pas de Rigidbody

**Tag:** `Table`

**Note:** Vous pouvez créer 2 colliders si vous voulez tracker les 2 côtés séparément:
- TableLeft (côté gauche, pour déterminer si balle va à droite)
- TableRight (côté droit, pour déterminer si balle va à gauche)

---

### **5. Ball (À générer automatiquement)**

Ne créez PAS manuellement! **BallManager le fera.**

Créez plutôt un **Prefab Ball** avec:

```
Name: BallPrefab ou Ball
Position: (0, 0.5, 0)
```

**Composants:**
```
✅ Mesh Filter + Sphere Mesh
✅ Mesh Renderer
✅ Rigidbody
   - Mass: 0.0027 kg (balle ping pong réelle)
   - Drag: 0.1
   - Angular Drag: 0.05
   - Gravity: ON
   - Collision Detection: Continuous
   - Constraints: None (liberté totale)
✅ Sphere Collider (radius ≈ 0.02m)
   - Is Trigger: ❌ NON
✅ Ball (SCRIPT)
✅ AimAssist (SCRIPT)
✅ ServiceHandler (SCRIPT) [Optionnel si vous gérez le service différemment]
```

**Tag:** `Ball` (optionnel, utilisé pour identité visuelle)

---

### **6. Ground (Zone "Out of Bounds")**

```
Name: Ground
Position: (0, -1, 0)
Scale: (10, 0.1, 10)  // Largement assez grand
```

**Composants:**
```
✅ Box Collider
   - Is Trigger: ❌ NON (détection physique)
   - ❌ Pas visible (optionnel: créer avec mesh)
✅ Rigidbody
   - Body Type: Static
```

**Tag:** `Ground`

**Couleur:** Gris transparent (optionnel pour visibilité)

---

## <a id="scripts"></a>⚙️ CONFIGURATION DES SCRIPTS

### **PingPongManager - Assignations Inspecteur**

Sur le GameObject "GameManager", sélectionnez le composant PingPongManager et remplissez:

```
Player 1: [Drag Player1 GameObject ici]
Player 2: [Drag Player2 GameObject ici]
Ball Manager: [Drag le composant BallManager du GameManager]
Win Score: 11
Win Margin: 2
```

---

### **BallManager - Assignations Inspecteur**

Sur le GameObject "GameManager", sélectionnez le composant BallManager et remplissez:

```
Ball Prefab: [Drag le prefab Ball que vous créez]
Ping Pong Manager: [Assign le component PingPongManager du même GameObject]
```

⚠️ **IMPORTANT:** Le Ball Prefab doit être un prefab, pas une instance!

**Créez le prefab:**
1. Créez un GameObject "Ball" avec tous les composants listés
2. Drag-drop ce Ball dans le dossier Assets/Prefabs/ (créez si besoin)
3. Supprimez l'instance de la scène
4. Dans BallManager, assignez le prefab depuis Assets/Prefabs/

---

### **Ball - Aucune assignation requise**

Ball.cs trouvera automatiquement:
- ✅ GetComponent<Rigidbody>()
- ✅ GetComponent<AimAssist>()

Mais reçoit `SetPingPongManager()` de BallManager

---

### **AimAssist - Assignations Inspecteur** 

Sur le Ball Prefab, le composant AimAssist a besoin:

```
Enable Aim Assist: ✅ TRUE
Assist Force Multiplier: 0.15 (commencer ici, ajuster si trop/pas assez)
Table Width: 1.525 (largeur réelle ping pong)
Detection Distance: 2.0 (m)
Table Center: (0, 0, 0) [Position du centre de la table]
Max Correction Time: 0.5 (secondes)
```

**Comment ajuster:**
- **Trop d'assistance:** Balle trop "facile" → réduire `Assist Force Multiplier` à 0.10
- **Pas assez:** Balle tombe souvent → augmenter à 0.20
- **Balle flottante:** Vérifier `Detection Distance` (augmenter à 3.0)

---

### **ServiceHandler - Assignations Inspecteur**

Sur le Ball Prefab, le composant ServiceHandler a besoin:

```
Grab Interactable: [Auto-trouvé si XRGrabInteractable existe]
Ping Pong Manager: [Sera assigné dynamiquement]
Rigidbody: [Auto-trouvé]
```

⚠️ **Important VR:** Assurez-vous que le Ball a un composant `XRGrabInteractable`
Si pas présent, le ajouter:
1. Sélectionnez Ball Prefab
2. Add Component → XRGrabInteractable
3. C'est tout! ServiceHandler le trouvera automatiquement

---

## <a id="tags"></a>🏷️ SETUP TAGS ET LAYERS

### **Créer les Tags**

Menu: Edit → Project Settings → Tags & Layers

Ajoutez ces tags:
```
☑ Paddle
☑ Table
☑ Ground
☑ Ball (optionnel)
☑ Player1Paddle
☑ Player2Paddle
```

### **Assigner les Tags**

1. **Paddle1** et **Paddle2**: `Paddle`
2. **Table**, **TableLeft**, **TableRight**: `Table`
3. **Ground**: `Ground`
4. **Ball**: `Ball` (optionnel)

---

## <a id="physics"></a>⚙️ PHYSICS SETTINGS

Menu: Edit → Project Settings → Physics

### **Raycast Settings** (Pour détection collisions)
```
Default Material: Standard (friction~0.4, bounce~0.4)
Bounce Threshold: 2
Default Solver Iterations: 6
```

### **Gravity**
```
Gravity: (0, -9.81, 0)
```

### **Collision Matrix**

Assurez-vous que:
- ✅ Ball collide avec Table
- ✅ Ball collide avec Paddle
- ✅ Ball collide avec Ground
- ❌ Ball NE collide PAS avec elle-même (car une seule balle à la fois)
- ❌ SideColliders (triggers) NE collident avec rien physiquement

---

## <a id="checklist"></a>✅ CHECKLIST COMPLÈTE DE VÉRIFICATION

Avant de lancer le jeu:

### **GameObjects**
- [ ] GameManager existe avec PingPongManager et BallManager
- [ ] Player1 et Player2 existent avec hiérarchie complète
- [ ] Ball prefab existe dans Assets/Prefabs/
- [ ] Table existe avec collider
- [ ] Ground existe

### **Scripts Assignés**
- [ ] PingPongManager sur GameManager
- [ ] BallManager sur GameManager
- [ ] Player sur Player1 et Player2
- [ ] Ball sur Ball prefab
- [ ] AimAssist sur Ball prefab
- [ ] ServiceHandler sur Ball prefab (optionnel)

### **Colliders et Triggers**
- [ ] Table collider: **Is Trigger = OFF** ✅
- [ ] Paddle colliders: **Is Trigger = OFF** ✅
- [ ] SideColliders: **Is Trigger = ON** ✅
- [ ] Ground collider: **Is Trigger = OFF** ✅
- [ ] Ball collider: **Is Trigger = OFF** ✅

### **Tags**
- [ ] Paddles tagguées `Paddle`
- [ ] Table tagguée `Table`
- [ ] Ground tagguée `Ground`

### **Rigidbodies**
- [ ] Ball a Rigidbody ✅
- [ ] Table a Rigidbody (Static ou pas de RB) ✓
- [ ] Ground a Rigidbody (Static) ✅
- [ ] Paddles: **SANS Rigidbody** ✅

### **Assignments Inspecteur**
- [ ] PingPongManager: Player1, Player2, BallManager remplis
- [ ] BallManager: Ball Prefab (pas instance), PingPongManager
- [ ] AimAssist: Tous paramètres remplis, Enable = ON
- [ ] ServiceHandler: Trouvé automatiquement

### **Positions Logiques**
- [ ] Player1 côté -X (gauche)
- [ ] Player2 côté +X (droite)
- [ ] Table au centre (0, 0, 0)
- [ ] ServicePoints à hauteur du joueur

### **VR XR Setup**
- [ ] Scene inclut XR_Core.unity (pour caméra et input)
- [ ] Ball a XRGrabInteractable (pour pouvoir attraper)
- [ ] Hands setup correct dans XR_Core ou scene VR

---

## 🎮 GAMEPLAY EXPLIQUÉ

### **Service**
1. Player1 complète son tour de service
2. Prend la balle et la place au ServicePoint
3. Lance - doit toucher son côté de table, puis côté opposé
4. Si 2 touches OK → Game commence
5. Si faute → Point pour Player2, Player2 serve

### **Rally (En jeu)**
1. Balle en mouvement sur table
2. Player peut la frapper quand elle arrive
3. Balle va à côté adverse → Player switch
4. Si même player frappe 2x → Faute → Point adverse
5. Si balle hit sol → Point au joueur dont la table était touchée

### **Aim Assist**
- Balle veering trop far → corrige légèrement vers center
- N'empêche pas de jouer, juste aide précision VR
- Ajustable en game, désactivable

---

## 🔧 TROUBLESHOOTING

### **Balle ne collide pas avec table**
- ❌ Table collider: Is Trigger = ON → Changer à OFF
- ❌ Table collider: Collider size trop petit → Agrandir

### **Balle passe à travers paddle**
- ❌ Paddle collider manquant → Ajouter Box Collider
- ❌ Rigidbody Body Type = Dynamic → Paddle ne doit PAS avoir Rigidbody
- ✅ Ball Rigidbody: Collision Detection = Continuous

### **Score ne monte pas**
- ❌ Tags mal placés → Vérifier tous les tags (Table, Paddle, Ground)
- ❌ PingPongManager not assigned → Vérifier Inspector
- ❌ Ball.SetPingPongManager() jamais appelé → Debug log "Ball has no PingPongManager"

### **Ball.SetPingPongManager() log error**
- ❌ BallManager Prefab mal assigné → Doit être un Prefab, pas instance
- ✅ Vérifier BallManager → Ball Prefab assigné correctement

### **Balle ne disparaît pas au sol**
- ❌ Pas de Destroy() appelé → Check Ball.cs HandleGroundCollision()
- ❌ Ground tag mal placé → Vérifier tag "Ground"

### **Aim Assist trop fort**
- ✅ Réduire `Assist Force Multiplier` à 0.10 ou 0.05

---

## 🎯 PROCHAINES ÉTAPES

1. **UI Score Display:**
   - Créer Canvas avec Text pour afficher PingPongManager.GetScoreDisplay()

2. **Win Screen:**
   - Afficher winner quand GameState = Inactive
   - Bouton Restart → ReloadScene
   - Bouton Lobby → LoadScene("Lobby")

3. **Audio Effects:**
   - Ping sound quand balle hit paddle
   - Ding sound quand point marqué
   - Tada sound quand jeu gagnée

4. **Visual Feedback:**
   - Ball glow quand en air
   - Paddle highlight quand peut frapper

---

Bon jeu! 🏓🎮
