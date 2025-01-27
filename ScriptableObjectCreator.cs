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
    private Vector2 scrollPosition;

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
        int arrayLines = 0;
        GUILayout.Label("Create a New ScriptableObject", EditorStyles.boldLabel);

        if (scriptableObjectTypes.Length == 0)
        {
            GUILayout.Label("No ScriptableObject types found in the specified folder.", EditorStyles.helpBox);
            return;
        }

        selectedTypeIndex = EditorGUILayout.Popup("ScriptableObject Type", selectedTypeIndex, scriptableObjectTypes.Select(type => type.Split(',')[0]).ToArray());
        objectName = EditorGUILayout.TextField("Object Name", objectName);

        ScriptableObject previousObject = currentObject;
        if (currentObject != null && currentObject != previousObject)
        {
            selectedObjectType = currentObject.GetType();
            objectName = AssetDatabase.GetAssetPath(currentObject).Split('/').Last().Replace(".asset", "");
            selectedTypeIndex = Array.IndexOf(scriptableObjectTypes, selectedObjectType.AssemblyQualifiedName);
        }
        else if (currentObject == null && previousObject != null)
        {
            selectedObjectType = null;
            objectName = "NewObject";
            selectedTypeIndex = 0;
        }
        if(currentObject == null)
        {

            if (GUILayout.Button("Create ScriptableObject"))
            {
                CreateNewScriptableObject();
            }
        }

        GUILayout.Label("Selected Object", EditorStyles.boldLabel);
        currentObject = (ScriptableObject)EditorGUILayout.ObjectField(currentObject, typeof(ScriptableObject), true);

        if (currentObject != null)
        {
            GUILayout.Space(10);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Set Properties", EditorStyles.boldLabel);

            foreach (var field in currentObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object fieldValue = field.GetValue(currentObject);
                GUILayout.BeginHorizontal();

                if (!field.FieldType.IsArray&&field.FieldType != typeof(Gradient))
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
                else if (field.FieldType == typeof(GameObject))
                {
                    field.SetValue(currentObject, (GameObject)EditorGUILayout.ObjectField((GameObject)fieldValue, typeof(GameObject), true));
                }
                else if (field.FieldType == typeof(Texture))
                {
                    field.SetValue(currentObject, (Texture)EditorGUILayout.ObjectField((Texture)fieldValue, typeof(Texture), true));
                }
                else if (field.FieldType == typeof(Material))
                {
                    field.SetValue(currentObject, (Material)EditorGUILayout.ObjectField((Material)fieldValue, typeof(Material), true));
                }
                else if (field.FieldType == typeof(AudioClip))
                {
                    field.SetValue(currentObject, (AudioClip)EditorGUILayout.ObjectField((AudioClip)fieldValue, typeof(AudioClip), true));
                }
                else if (field.FieldType == typeof(Vector2))
                {
                    field.SetValue(currentObject, EditorGUILayout.Vector2Field("", (Vector2)fieldValue));
                }
                else if (field.FieldType == typeof(Vector3))
                {
                    field.SetValue(currentObject, EditorGUILayout.Vector3Field("", (Vector3)fieldValue));
                }
                else if (field.FieldType == typeof(Vector4))
                {
                    Vector4 temp = (Vector4)fieldValue;
                    temp = EditorGUILayout.Vector4Field("", temp);
                    field.SetValue(currentObject, temp);
                }
                else if (field.FieldType == typeof(Color))
                {
                    field.SetValue(currentObject, EditorGUILayout.ColorField((Color)fieldValue));
                }
                else if (field.FieldType == typeof(Gradient))
                {
                    Gradient tempGradient = fieldValue as Gradient ?? new Gradient();
                    SerializedObject serializedObject = new SerializedObject(currentObject);
                    SerializedProperty gradientProp = serializedObject.FindProperty(field.Name);
                    EditorGUILayout.PropertyField(gradientProp, true);
                    serializedObject.ApplyModifiedProperties();
                }

                else if (field.FieldType.IsArray)
                {
                    SerializedObject serializedObject = new SerializedObject(currentObject);
                    SerializedProperty arrayProperty = serializedObject.FindProperty(field.Name);
                    EditorGUILayout.PropertyField(arrayProperty, true);
                    serializedObject.ApplyModifiedProperties();
                }
                else if (field.FieldType.IsEnum)
                {
                    field.SetValue(currentObject, EditorGUILayout.EnumPopup((Enum)fieldValue));
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            if (GUILayout.Button("Save ScriptableObject"))
            {
                SaveCurrentObject();
            }

            // Adjust window size based on content
            float height = currentObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).Length * 20+60+120;
            minSize = new Vector2(300, height);
            maxSize = new Vector2(600, height+300);
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
