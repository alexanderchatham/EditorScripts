using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using UnityEngine;
using System.IO;

public enum iOSPermissionType
{
    HealthKit,
    Bluetooth,
    Camera,
    Microphone,
    LocationWhenInUse,
    LocationAlways,
    Motion,
    PhotoLibrary
}

public static class iOSPostBuildProcessor
{
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

    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget != BuildTarget.iOS)
            return;

        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);

        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        var rootDict = plist.root;

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

        var proj = new PBXProject();
        proj.ReadFromFile(projPath);

#if UNITY_2019_3_OR_NEWER
        string target = proj.GetUnityMainTargetGuid();
        string entitlementsFileName = "Unity-iPhone.entitlements";
#else
        string target = proj.TargetGuidByName(PBXProject.GetUnityTargetName());
        string entitlementsFileName = target + ".entitlements";
#endif

        string entitlementsPath = Path.Combine(pathToBuiltProject, entitlementsFileName);
        var entitlements = new PlistDocument();
        if (File.Exists(entitlementsPath))
        {
            entitlements.ReadFromFile(entitlementsPath);
        }
        else
        {
            if (entitlements.root.values.ContainsKey("com.apple.developer.healthkit") == false)
            {
                entitlements.root.SetBoolean("com.apple.developer.healthkit", true);
            }

        }

        // Add HealthKit entitlement
        entitlements.root.SetBoolean("com.apple.developer.healthkit", true);
        entitlements.WriteToFile(entitlementsPath);

        proj.AddFile(entitlementsFileName, entitlementsFileName);
        proj.SetBuildProperty(target, "CODE_SIGN_ENTITLEMENTS", entitlementsFileName);

        // Add capabilities
        var capManager = new ProjectCapabilityManager(projPath, entitlementsFileName, target);
        capManager.AddHealthKit();
        capManager.AddBackgroundModes(
            BackgroundModesOptions.BackgroundFetch |
            BackgroundModesOptions.RemoteNotifications |
            BackgroundModesOptions.ExternalAccessoryCommunication 
);
        capManager.AddPushNotifications(true);
        capManager.AddInAppPurchase();
        capManager.AddAssociatedDomains(new[] { "applinks:yourgame.example.com" }); // customize this
        capManager.AddGameCenter();
        capManager.AddSignInWithApple();
        capManager.WriteToFile();

        proj.WriteToFile(projPath);

        Debug.Log("✅ iOS Post Build: Info.plist, entitlements, and capabilities configured.");
    }
}
