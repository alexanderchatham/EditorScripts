using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using Unity.EditorCoroutines.Editor;

public class TransparentThumbnailGenerator : MonoBehaviour
{
    [MenuItem("Tools/Generate Thumbnails")]
    public static void GenerateTransparentPrefabThumbnails()
    {
        GameObject[] selectedPrefabs = Selection.gameObjects;
        if (selectedPrefabs.Length == 0)
        {
            Debug.LogWarning("No prefabs selected. Please select the prefabs you want to generate thumbnails for.");
            return;
        }

        EditorCoroutineUtility.StartCoroutineOwnerless(GenerateThumbnailsCoroutine(selectedPrefabs));
    }

    private static IEnumerator GenerateThumbnailsCoroutine(GameObject[] prefabs)
    {
        string savePath = "Assets/Resources/Thumbnails/";
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        if (!AssetDatabase.IsValidFolder(savePath))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Thumbnails");
        }

        // Create a temporary camera for rendering
        GameObject cameraObj = new GameObject("ThumbnailCamera");
        Camera thumbnailCamera = cameraObj.AddComponent<Camera>();
        thumbnailCamera.backgroundColor = new Color(0, 0, 0, 0); // Transparent background
        thumbnailCamera.clearFlags = CameraClearFlags.Depth;
        thumbnailCamera.orthographic = true;
        thumbnailCamera.enabled = false;

        // Create a RenderTexture
        int textureSize = 256;
        RenderTexture renderTexture = new RenderTexture(textureSize, textureSize, 24);
        thumbnailCamera.targetTexture = renderTexture;

        foreach (GameObject prefab in prefabs)
        {
            GameObject instance = Instantiate(prefab);
            instance.transform.position = Vector3.zero;

            Bounds bounds = GetObjectBounds(instance);
            float size = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            float padding = 1.2f; // Add 20% padding around the object
            thumbnailCamera.orthographicSize = size * padding / 2;

            // Set the camera position and rotation
            Vector3 cameraDirection = new Vector3(-5, 5, -5); // 45 degrees down and to the right
            thumbnailCamera.transform.position = bounds.center + cameraDirection * size * padding;
            thumbnailCamera.transform.LookAt(bounds.center);

            // Render the prefab
            thumbnailCamera.Render();

            // Read the RenderTexture into a Texture2D
            RenderTexture.active = renderTexture;
            Texture2D thumbnail = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            thumbnail.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
            thumbnail.Apply();
            RenderTexture.active = null;

            // Save the thumbnail as PNG
            byte[] bytes = thumbnail.EncodeToPNG();
            string filePath = Path.Combine(savePath, prefab.name + "_thumbnail.png");
            File.WriteAllBytes(filePath, bytes);
            Debug.Log($"Thumbnail saved to: {filePath}");

            // Clean up
            DestroyImmediate(instance);
            renderTexture.Release();
            yield return new EditorWaitForSeconds(0.1f);
        }

        // Clean up the camera and RenderTexture
        DestroyImmediate(cameraObj);
        renderTexture.Release();
        DestroyImmediate(renderTexture);

        AssetDatabase.Refresh();
        Debug.Log("Transparent prefab thumbnail generation complete.");
    }

    private static Bounds GetObjectBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(obj.transform.position, Vector3.zero);
        }

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }
}
