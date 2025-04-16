using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;
using System.IO;

public static class iOSPermissionsPostProcessor
{
    // Define your permissions here
    private static readonly Dictionary<iOSPermissionType, string> permissionDescriptions = new Dictionary<iOSPermissionType, string>
    {
        { iOSPermissionType.HealthKit, "This app uses HealthKit to track your workouts and provide XP in-game." },
        { iOSPermissionType.Bluetooth, "Bluetooth is used to connect to nearby health devices." },
        { iOSPermissionType.Camera, "The camera is used for AR and character customization." },
        { iOSPermissionType.Microphone, "The microphone is used for voice input." },
        { iOSPermissionType.LocationWhenInUse, "Location is used while the app is active to customize game content." },
        { iOSPermissionType.LocationAlways, "Location is used in the background to provide game updates." },
        { iOSPermissionType.Motion, "Motion data is used to track player movement for rewards." },
        { iOSPermissionType.PhotoLibrary, "The photo library is used to save and load game screenshots." }
    };

    [PostProcessBuild(999)] // High number so it runs late
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget != BuildTarget.iOS)
            return;

        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        PlistElementDict rootDict = plist.root;

        foreach (var kvp in permissionDescriptions)
        {
            switch (kvp.Key)
            {
                case iOSPermissionType.HealthKit:
                    rootDict.SetString("NSHealthShareUsageDescription", kvp.Value);
                    rootDict.SetString("NSHealthUpdateUsageDescription", kvp.Value);
                    break;
                case iOSPermissionType.Bluetooth:
                    rootDict.SetString("NSBluetoothAlwaysUsageDescription", kvp.Value);
                    break;
                case iOSPermissionType.Camera:
                    rootDict.SetString("NSCameraUsageDescription", kvp.Value);
                    break;
                case iOSPermissionType.Microphone:
                    rootDict.SetString("NSMicrophoneUsageDescription", kvp.Value);
                    break;
                case iOSPermissionType.LocationWhenInUse:
                    rootDict.SetString("NSLocationWhenInUseUsageDescription", kvp.Value);
                    break;
                case iOSPermissionType.LocationAlways:
                    rootDict.SetString("NSLocationAlwaysAndWhenInUseUsageDescription", kvp.Value);
                    break;
                case iOSPermissionType.Motion:
                    rootDict.SetString("NSMotionUsageDescription", kvp.Value);
                    break;
                case iOSPermissionType.PhotoLibrary:
                    rootDict.SetString("NSPhotoLibraryUsageDescription", kvp.Value);
                    break;
            }
        }

        plist.WriteToFile(plistPath);
        Debug.Log("✅ iOS permissions added to Info.plist");
    }
}
