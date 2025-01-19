using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class CloudCodeEditorWindow : EditorWindow
{
    private MonoScript cloudCodeScript;
    private List<MethodInfo> cloudFunctions;
    private Vector2 scrollPosition;
    private Dictionary<string, string[]> parameterInputs;
    private bool isAuthenticated;

    [MenuItem("Tools/Cloud Code Editor")]
    public static void OpenWindow()
    {
        GetWindow<CloudCodeEditorWindow>("Cloud Code Editor");
    }

    private async void OnEnable()
    {
        cloudFunctions = new List<MethodInfo>();
        parameterInputs = new Dictionary<string, string[]>();
        isAuthenticated = false;

        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignedIn += () =>
                {
                    Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
                    isAuthenticated = true;
                };

                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            else
            {
                Debug.Log("Already signed in as: " + AuthenticationService.Instance.PlayerId);
                isAuthenticated = true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to initialize Unity Services: " + ex.Message);
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Cloud Code Editor", EditorStyles.boldLabel);

        if (!isAuthenticated)
        {
            EditorGUILayout.HelpBox("Not signed into Unity Services. Functions cannot be executed.", MessageType.Warning);
            return;
        }

        cloudCodeScript = (MonoScript)EditorGUILayout.ObjectField("Cloud Code Script", cloudCodeScript, typeof(MonoScript), false);

        if (GUILayout.Button("Load Cloud Functions"))
        {
            LoadCloudFunctions();
        }

        if (cloudFunctions.Count > 0)
        {
            EditorGUILayout.LabelField("Cloud Functions", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var method in cloudFunctions)
            {
                DrawFunctionUI(method);
            }
            EditorGUILayout.EndScrollView();
        }
    }

    private void LoadCloudFunctions()
    {
        if (cloudCodeScript == null)
        {
            Debug.LogWarning("No script selected.");
            return;
        }

        Type scriptType = cloudCodeScript.GetClass();
        if (scriptType == null)
        {
            Debug.LogWarning("Unable to load the selected script class.");
            return;
        }

        cloudFunctions.Clear();
        parameterInputs.Clear();

        foreach (var method in scriptType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            if (method.IsPublic && method.DeclaringType == scriptType)
            {
                cloudFunctions.Add(method);
                ParameterInfo[] parameters = method.GetParameters();
                parameterInputs[method.Name] = new string[parameters.Length];
            }
        }
    }

    private void DrawFunctionUI(MethodInfo method)
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(method.Name, EditorStyles.boldLabel);
        if (GUILayout.Button("Run", GUILayout.Width(100)))
        {
            RunFunction(method);
        }
        EditorGUILayout.EndHorizontal();

        ParameterInfo[] parameters = method.GetParameters();
        for (int i = 0; i < parameters.Length; i++)
        {
            ParameterInfo param = parameters[i];
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(param.Name + " (" + param.ParameterType.Name + ")", GUILayout.Width(150));
            parameterInputs[method.Name][i] = EditorGUILayout.TextField(parameterInputs[method.Name][i]);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private object ConvertParameter(string input, Type type)
    {
        try
        {
            if (type == typeof(int))
            {
                return int.Parse(input);
            }
            else if (type == typeof(float))
            {
                return float.Parse(input);
            }
            else if (type == typeof(bool))
            {
                return bool.Parse(input);
            }
            else if (type == typeof(string))
            {
                return input;
            }
            else if (type.IsEnum)
            {
                return Enum.Parse(type, input);
            }
            else
            {
                throw new NotSupportedException("Unsupported parameter type: " + type.Name);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error converting parameter: {ex.Message}");
            return null;
        }
    }

    private void RunFunction(MethodInfo method)
    {
        try
        {
            object instance = null;
            if (!method.IsStatic)
            {
                instance = Activator.CreateInstance(method.DeclaringType);
            }

            ParameterInfo[] parameters = method.GetParameters();
            object[] convertedParameters = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                convertedParameters[i] = ConvertParameter(parameterInputs[method.Name][i], parameters[i].ParameterType);
            }

            object result = method.Invoke(instance, convertedParameters);

            if (result != null)
            {
                string jsonResult = JsonUtility.ToJson(result, true);
                Debug.Log($"Function {method.Name} returned:\n{jsonResult}");
            }
            else
            {
                Debug.Log($"Function {method.Name} executed successfully with no return value.");
            }
        }
        catch (TargetInvocationException tie)
        {
            Debug.LogError($"Error invoking {method.Name}: {tie.InnerException?.Message}\nStackTrace: {tie.InnerException?.StackTrace}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error invoking {method.Name}: {ex.Message}\nStackTrace: {ex.StackTrace}");
        }
    }
}
