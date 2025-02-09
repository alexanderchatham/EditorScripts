# Unity Editor Scripts
A collection of useful Unity editor scripts to streamline development workflows.

## Features
- **Transparent Thumbnail Generator:** Quickly generate transparent PNG thumbnails for selected prefabs.
- **Anchor Setter:** Automatically adjust anchors for selected UI elements.
- **Cloud Code Editor:** Interact with Unity Cloud Code directly from the Unity Editor.
- **Scriptable Object Creator:** Easily Create and Edit scriptable objects.

## Installation
1. Clone or download this repository.
2. Copy the `EditorScripts` folder into your Unity project's `Assets` directory.

## Usage
- **Thumbnail Generator:**
  1. Select one or more prefabs in the Project window.
  2. Go to `Tools > Generate Thumbnails`.
  3. Thumbnails will be saved to `Assets/Resources/Thumbnails/`.

- **Anchor Setter:**
  1. Select UI elements in the Scene window.
  2. Press `Ctrl + `` or use `Tools > Anchors > Set Anchors`.

- **Cloud Code Editor:**
  1. Open the Cloud Code Editor via `Tools > Cloud Code Editor`.
  2. Load a MonoScript containing Cloud Code functions and execute them.

- **ScriptableObject Creator:**
  1. Open the ScriptableObject Creator via `Tools > Scriptable Object Creator`.
  2. Select a `ScriptableObject` type from the dropdown menu.
  3. Enter a name for the new object and click **Create ScriptableObject**.
  4. Use the built-in UI to set properties for the created object directly in the editor.
  5. ScriptableObject Scripts must be saved in `Assets/Scripts/ScriptableObjects`. 


## License
MIT License. See [LICENSE](LICENSE) for details.

## Contributions
Contributions, issues, and feature requests are welcome!
