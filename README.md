# Unity Editor Scripts
A collection of useful Unity editor scripts to streamline development workflows.

## Features
- **Transparent Thumbnail Generator:** Quickly generate transparent PNG thumbnails for selected prefabs.
- **Anchor Setter:** Automatically adjust anchors for selected UI elements.
- **Cloud Code Editor:** Interact with Unity Cloud Code directly from the Unity Editor.
- **Scriptable Object Creator:** Easily Create and Edit scriptable objects.
- **iOS Post Build Processor:** Automatically adds permission descriptions, modifies entitlements, and enables required capabilities (like HealthKit, Background Modes, Push Notifications, etc.) to your Xcode project after building for iOS.

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
  2. Press `Ctrl + ~` (tilde) or use `Tools > Anchors > Set Anchors`.

- **Cloud Code Editor:**
  1. Open the Cloud Code Editor via `Tools > Cloud Code Editor`.
  2. Load a MonoScript containing Cloud Code functions and execute them.

- **ScriptableObject Creator:**
  1. Open the ScriptableObject Creator via `Tools > Scriptable Object Creator`.
  2. Select a `ScriptableObject` type from the dropdown menu.
  3. Enter a name for the new object and click **Create ScriptableObject**.
  4. Use the built-in UI to set properties for the created object directly in the editor.
  5. ScriptableObject Scripts must be saved in `Assets/Scripts/ScriptableObjects`.

- **iOS Post Build Processor:**
  1. After building your Unity project for iOS, this script will automatically:
     - Add `Info.plist` usage descriptions for common permissions like HealthKit, Bluetooth, Camera, etc.
     - Create and update the `.entitlements` file to include HealthKit access.
     - Enable key Xcode capabilities such as:
       - HealthKit
       - Background Modes (processing, fetch, remote notifications, external accessory)
       - Push Notifications
       - In-App Purchases
       - Associated Domains (customizable)
       - Game Center
       - Sign In with Apple
  2. No manual setup in Xcode needed for permissions or capabilities!

## License
MIT License. See [LICENSE](LICENSE) for details.

## Contributions
Contributions, issues, and feature requests are welcome!
