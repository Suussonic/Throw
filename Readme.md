# Throw

Projet Unity VR centré sur le lancer d'armes et la destruction de cibles ou de ballons dans plusieurs niveaux. Le projet utilise Unity 6, XR Interaction Toolkit, OpenXR/Meta XR et ECS pour une partie des ennemis.

## Aperçu

Le joueur évolue dans une scène principale VR qui sert de menu et de hub, puis charge différents niveaux de jeu. Chaque niveau possède son propre score. Le score est calculé dans la scène active via `ScoreUI`, puis enregistré lors du changement de scène pour être réaffiché dans la scène principale grâce à `LevelScoreDisplay`.

## Structure du dépôt

- `VR Game/` : projet Unity principal.
- `VR Game/Assets/Scenes/` : scènes de menu et de niveaux.
- `VR Game/Assets/Scripts/` : scripts gameplay, UI, core et ECS.
- `3D Model/` : sources Blender des assets 3D.

## Version et stack technique

- Unity : `6000.3.7f1`
- XR : `com.unity.xr.interaction.toolkit`, `com.unity.xr.openxr`, `com.meta.xr.sdk.all`, `com.unity.xr.hands`
- Input : `com.unity.inputsystem`
- ECS : `com.unity.entities`, `com.unity.physics`, `com.unity.entities.graphics`
- Render pipeline : `URP`

## Scènes principales

- `Main VR Scene` : menu principal VR et affichage des scores par niveau.
- `StaticBalloonLevel` : niveau de ballons statiques.
- `AgressiveBalloonLevel` : niveau de ballons agressifs.
- `Level Test` : niveau de test.

## Fonctionnement général

### Navigation et boucle de jeu

Le flux principal est géré par `GameLoopManager`.

- charge les scènes
- conserve l'état global avec `GameStateSO`
- sauvegarde automatiquement le score courant avant changement de scène
- permet le retour au menu principal

Le script `changeLevel` déclenche le chargement d'un niveau depuis une interaction VR.

### Système de score

Le système de score repose sur trois scripts principaux.

#### `ScoreUI`

Présent dans les scènes de niveau.

- stocke la valeur courante du score
- met à jour le texte UI pendant la partie
- expose `AddScore`, `SubtractScore`, `ResetScore`, `SetScore` et `GetScore`

#### `LevelScoreManager`

Gestionnaire central de persistance.

- enregistre le score par niveau dans `PlayerPrefs`
- conserve la dernière valeur enregistrée pour chaque niveau
- permet la lecture du score même si le singleton n'est pas encore initialisé

#### `LevelScoreDisplay`

Présent dans la scène principale sur un `TextMeshProUGUI`.

- lit le score du niveau choisi via `LevelType`
- affiche le score au format `Score: X/Y`
- se met à jour automatiquement à intervalle régulier

### Comment le score circule entre les scènes

1. En niveau, `Target` ajoute des points au `ScoreUI`.
2. Lors d'un retour menu ou d'un changement de scène, `GameLoopManager` sauvegarde la valeur courante du `ScoreUI`.
3. Dans `Main VR Scene`, `LevelScoreDisplay` lit la valeur sauvegardée et l'affiche.

## Scripts importants

- `Assets/Scripts/Core/GameLoopManager.cs` : navigation entre scènes, sauvegarde du score, état global.
- `Assets/Scripts/Core/GameStateSO.cs` : état de jeu et niveau courant.
- `Assets/Scripts/Core/LevelScoreManager.cs` : persistance des scores.
- `Assets/Scripts/UI/ScoreUI.cs` : affichage et stockage du score courant en niveau.
- `Assets/Scripts/UI/LevelScoreDisplay.cs` : affichage des scores enregistrés dans la scène principale.
- `Assets/Scripts/Gameplay/Target.cs` : attribution de points lorsqu'une cible est touchée.
- `Assets/Scripts/Gameplay/LevelCompletionChecker.cs` : fin de niveau automatique et sauvegarde du score.
- `Assets/Scripts/VrAction/changeLevel.cs` : chargement des niveaux depuis les interactions VR.

## Mise en place du score dans la scène principale

Pour afficher le score d'un niveau dans `Main VR Scene` :

1. Ajouter `LevelScoreDisplay` sur un `TextMeshProUGUI`.
2. Régler `Level Type` sur le niveau à afficher.
3. Régler `Max Score` selon le score maximum attendu.
4. Vérifier que `Score Text` référence bien le bon texte UI.

Exemple :

- un texte configuré sur `LevelTest` affiche le score du niveau test
- un texte configuré sur `PassivLevel` affiche le score du niveau passif
- un texte configuré sur `AgressiveLevel` affiche le score du niveau agressif

## Lancer le projet

1. Ouvrir le dossier `VR Game` avec Unity `6000.3.7f1`.
2. Vérifier que les packages XR et Input sont installés.
3. Ouvrir `Main VR Scene`.
4. Lancer le Play Mode depuis Unity.

## Entrées debug utiles

Dans `GameLoopManager`, quelques touches clavier sont prévues pour les tests :

- `P` : lancer le niveau passif
- `A` : lancer le niveau agressif
- `M` : retour menu
- `Q` : quitter le jeu
- `Escape` : pause / reprise