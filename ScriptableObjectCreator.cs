// ScriptableObjectCreator.cs
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

public class ScriptableObjectCreator : EditorWindow
{
    public string objectName = "NewObject";
    public Type selectedObjectType;
    private string[] scriptableObjectTypes;
    private int selectedTypeIndex = 0;
    public ScriptableObject currentObject;

    [MenuItem("Tools/Scriptable Object Creator")]
    public static void ShowWindow()
    {
        var window = GetWindow<ScriptableObjectCreator>("Scriptable Object Creator");
        window.autoRepaintOnSceneChange = true;
        window.minSize = new Vector2(300, 200);
    }

    void OnEnable()
    {
        // Find all ScriptableObject types in the ScriptableObjects folder
        scriptableObjectTypes = AssetDatabase.FindAssets("t:Script")
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Where(path => path.StartsWith("Assets/Scripts/ScriptableObjects"))
            .Select(path => AssetDatabase.LoadAssetAtPath<MonoScript>(path))
            .Where(script => script != null && script.GetClass() != null && typeof(ScriptableObject).IsAssignableFrom(script.GetClass()))
            .Select(script => script.GetClass().AssemblyQualifiedName)
            .ToArray();
    }

    void OnGUI()
    {
        GUILayout.Label("Create a New ScriptableObject", EditorStyles.boldLabel);

        if (scriptableObjectTypes.Length == 0)
        {
            GUILayout.Label("No ScriptableObject types found in the specified folder.", EditorStyles.helpBox);
            return;
        }

        selectedTypeIndex = EditorGUILayout.Popup("ScriptableObject Type", selectedTypeIndex, scriptableObjectTypes.Select(type => type.Split(',')[0]).ToArray());
        objectName = EditorGUILayout.TextField("Object Name", objectName);

        GUILayout.Label("Selected Object", EditorStyles.boldLabel);
        ScriptableObject previousObject = currentObject;
        currentObject = (ScriptableObject)EditorGUILayout.ObjectField(currentObject, typeof(ScriptableObject), true);

        if (currentObject != null && currentObject != previousObject)
        {
            selectedObjectType = currentObject.GetType();
            objectName = AssetDatabase.GetAssetPath(currentObject).Split('/').Last().Replace(".asset", "");
            selectedTypeIndex = Array.IndexOf(scriptableObjectTypes, selectedObjectType.AssemblyQualifiedName);
        }

        if (GUILayout.Button("Create ScriptableObject"))
        {
            CreateNewScriptableObject();
        }

        if (currentObject != null)
        {
            GUILayout.Space(10);
            GUILayout.Label("Set Properties", EditorStyles.boldLabel);

            foreach (var field in currentObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object fieldValue = field.GetValue(currentObject);
                GUILayout.BeginHorizontal();
                GUILayout.Label(ObjectNames.NicifyVariableName(field.Name), GUILayout.Width(150));

                GUI.SetNextControlName(field.Name);

                if (field.FieldType == typeof(int))
                {
                    field.SetValue(currentObject, EditorGUILayout.IntField((int)fieldValue));
                }
                else if (field.FieldType == typeof(float))
                {
                    field.SetValue(currentObject, EditorGUILayout.FloatField((float)fieldValue));
                }
                else if (field.FieldType == typeof(string))
                {
                    field.SetValue(currentObject, EditorGUILayout.TextField((string)fieldValue));
                }
                else if (field.FieldType == typeof(bool))
                {
                    field.SetValue(currentObject, EditorGUILayout.Toggle((bool)fieldValue));
                }
                else if (field.FieldType.IsEnum)
                {
                    field.SetValue(currentObject, EditorGUILayout.EnumPopup((Enum)fieldValue));
                }
                GUILayout.EndHorizontal();
            }

            // Adjust window size based on content
            float height = (scriptableObjectTypes.Length + 3) * 40;
            minSize = new Vector2(300, height);
            maxSize = new Vector2(600, height);
        }
    }

    private void CreateNewScriptableObject()
    {
        if (scriptableObjectTypes.Length == 0 || selectedTypeIndex < 0 || selectedTypeIndex >= scriptableObjectTypes.Length)
        {
            Debug.LogError("Invalid ScriptableObject type selected.");
            return;
        }

        string selectedTypeName = scriptableObjectTypes[selectedTypeIndex];
        Type objectType = Type.GetType(selectedTypeName);

        if (objectType == null)
        {
            Debug.LogError($"Could not find type {selectedTypeName}.");
            return;
        }

        currentObject = ScriptableObject.CreateInstance(objectType);

        string folderPath = "Assets/ScriptableObjects";
        string typeFolderPath = $"{folderPath}/{objectType.Name}";

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        }

        if (!AssetDatabase.IsValidFolder(typeFolderPath))
        {
            AssetDatabase.CreateFolder(folderPath, objectType.Name);
        }

        string assetPath = $"{typeFolderPath}/{objectName}.asset";
        AssetDatabase.CreateAsset(currentObject, assetPath);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = currentObject;

        // Automatically set fields and focus on the first property
        FocusOnFirstProperty();

        Debug.Log($"Created new ScriptableObject of type {objectType.Name} at {assetPath}");
    }

    private void FocusOnFirstProperty()
    {
        if (currentObject != null)
        {
            var firstField = currentObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();
            if (firstField != null)
            {
                EditorGUI.FocusTextInControl(firstField.Name);
            }
        }
    }

    private void SaveCurrentObject()
    {
        if (currentObject == null)
        {
            Debug.LogError("No ScriptableObject to save.");
            return;
        }

        EditorUtility.SetDirty(currentObject);
        AssetDatabase.SaveAssets();
        Debug.Log("ScriptableObject properties saved.");
    }
}
