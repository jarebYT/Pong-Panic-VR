# 🎮 GUIDE D'INTÉGRATION - UI ET FEEDBACK VISUEL

## ✨ NOUVEAUX SYSTÈMES AJOUTÉS

### 1. **GameplayUIManager** - Notifications de points
Affiche les notifications textuelles quand un joueur marque un point.
- Messages: "Joueur X a marqué 1 point"
- Affichage du score en temps réel
- Instructions de service
- Animations de fade in/out

### 2. **BallFeedback** - Effet "Pouf" de disparition
Crée un effet visuel quand la balle disparait:
- Animation de scaling (la balle grandit puis disparait)
- Génération de particules
- Son de disparition (optionnel)

### 3. **ServiceUIManager** - Indications de service
Affiche les instructions au joueur qui sert:
- "Saisis la balle et lance!"
- Pulsation visuelle pendant le service
- Feedback de validité du service

---

## 🛠️ CONFIGURATION EN UNITY

### **ÉTAPE 1: Créer le Canvas UI**

1. Clique droit dans Hierarchy → UI → Canvas
2. Nomme-le `GameplayUICanvas`
3. Configure:
   - **Canvas Scaler**: Set a `Scale With Screen Size`
   - **Render Mode**: `World Space` (pour VR)
   - **Position**: (0, 2, 2) - Au-dessus de la table
   - **Rotation**: (0, 0, 0)
   - **Scale**: (0.005, 0.005, 0.005) - Pour être visible en VR

### **ÉTAPE 2: Ajouter les textes UI**

Dans `GameplayUICanvas`, crée 3 Text éléments:

#### **a) Notification Text**
```
Name: NotificationText
Position: (0, 0, 0)
Text Component:
  ├─ Text: "Joueur X a marqué un point"
  ├─ Font Size: 60
  ├─ Color: Yellow
  ├─ Alignment: Center
```

#### **b) Score Text**
```
Name: ScoreText
Position: (0, 100, 0)
Text Component:
  ├─ Text: "Player1: 0 - 0 :Player2"
  ├─ Font Size: 50
  ├─ Color: White
  ├─ Alignment: Center
```

#### **c) Service Text**
```
Name: ServiceText
Position: (0, -100, 0)
Text Component:
  ├─ Text: "🎾 Service Player1\nSaisis la balle!"
  ├─ Font Size: 40
  ├─ Color: Cyan
  ├─ Alignment: Center
```

### **ÉTAPE 3: Assigner GameplayUIManager**

1. Ajoute un GameObject vide: `UIManager`
2. Attach script: `GameplayUIManager.cs`
3. Dans l'Inspector:
   ```
   Canvas: GameplayUICanvas
   Notification Text: NotificationText
   Score Text: ScoreText
   Service Text: ServiceText
   
   Notification Duration: 2.5
   Fade In Duration: 0.3
   Fade Out Duration: 0.5
   ```

### **ÉTAPE 4: Assigner ServiceUIManager**

1. Ajoute un GameObject vide: `ServiceUIManager`
2. Attach script: `ServiceUIManager.cs`
3. Dans l'Inspector:
   ```
   Gameplay UI Manager: [drag UIManager]
   Grab Ball Instruction: "Saisis la balle et lance!"
   Bounce Instruction: "La balle doit rebondir sur ta zone!"
   Serve Valid Instruction: "Service valide! Jeu commence!"
   ```

### **ÉTAPE 5: Ajouter BallFeedback au Ball Prefab**

1. Ouvre le Ball prefab: `Assets/Prefabs/Ball.prefab`
2. Attach script: `BallFeedback.cs`
3. Configure:
   ```
   Enable Poof Effect: ON ✓
   Poof Duration: 0.3
   Poof Scale: 1.2
   Spawn Particles: ON ✓
   Particle Count: 20
   Particle Speed: 5
   Particle Lifetime: 1
   Play Sound: OFF (optionnel)
   ```

### **ÉTAPE 6: Assigner UI Managers au GameManager**

1. Clique sur `GameManager` dans la Hierarchy
2. Sur le composant `PingPongManager`, ajoute:
   ```
   Gameplay UI Manager: [drag UIManager]
   Service UI Manager: [drag ServiceUIManager]
   ```

---

## 🎯 FLUX DU JEU AVEC UI

```
Game Start
  ↓
  ShowServiceStart() → "🎾 Service Player1\nSaisis la balle!"
  ↓
Player grabs ball
  ↓
Player releases ball
  ↓
Ball bounces on table
  ↓
[if valid] Ball goes to other side → Game starts
  ↓
[Someone misses]
  ↓
ShowPointScored() → "Player2 a marqué un point!\nScore: 1"
  UpdateScoreDisplay() → "Player1: 0 - 1 :Player2"
  PlayPoofEffect() → Ball disappears with animation
  ↓
ResetRally()
  ↓
ShowServiceStart() → Next player to serve
```

---

## 📋 CHECKLIST DE SETUP

- [ ] Canvas créé et configuré (Render Mode: World Space)
- [ ] 3 Text éléments créés (NotificationText, ScoreText, ServiceText)
- [ ] GameplayUIManager attaché et configuré
- [ ] ServiceUIManager attaché et configuré
- [ ] BallFeedback attaché au Ball prefab
- [ ] PingPongManager a les références UI assignées
- [ ] Aucune erreur dans la Console

---

## 🎨 CUSTOMISATION

### **Changer les couleurs**
Dans GameplayUIManager:
- `pointNotificationColor` → Jaune (quand point marqué)
- `serviceNotificationColor` → Cyan (indications service)
- `warningColor` → Rouge (fautes)

### **Changer les messages**
Dans ServiceUIManager:
- `grabBallInstruction`
- `bounceInstruction`
- `serveValidInstruction`

### **Ajuster l'effet Poof**
Dans BallFeedback:
- `poofDuration` → 0.3 (plus = plus lent)
- `poofScale` → 1.2 (plus = plus gros avant disparition)
- `particleCount` → 20 (plus = plus de particules)

---

## ✅ RÉSUMÉ

Ce système ajoute:
1. ✅ Notifications de score
2. ✅ Affichage du score en temps réel
3. ✅ Instructions de service
4. ✅ Effet de disparition "Pouf" pour la balle
5. ✅ Particules de feedback

Le gameplay est maintenant **très visuellement informatif!** 🎮
