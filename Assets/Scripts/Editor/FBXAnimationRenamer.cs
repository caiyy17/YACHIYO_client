using UnityEngine;
using UnityEditor;
using System.IO;

public class FBXAnimationRenamer : EditorWindow
{
    private string folderPath = "Assets/YourFolder"; // 默认文件夹路径

    [MenuItem("Tools/FBX Animation Renamer")]
    public static void ShowWindow()
    {
        GetWindow<FBXAnimationRenamer>("FBX Animation Renamer");
    }

    private void OnGUI()
    {
        GUILayout.Label("FBX Animation Renamer", EditorStyles.boldLabel);

        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

        if (GUILayout.Button("Rename Animations"))
        {
            RenameAnimationsInFolder(folderPath);
        }
    }

    private void RenameAnimationsInFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError($"Directory does not exist: {folderPath}");
            return;
        }

        string[] fbxFiles = Directory.GetFiles(folderPath, "*.fbx", SearchOption.AllDirectories);
        if (fbxFiles.Length == 0)
        {
            Debug.LogError($"No FBX files found in directory: {folderPath}");
            return;
        }

        foreach (string fbxFile in fbxFiles)
        {
            string assetPath = fbxFile.Replace("\\", "/");
            ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;

            if (modelImporter != null)
            {
                Debug.Log($"Processing {assetPath}");

                ModelImporterClipAnimation[] clipAnimations = modelImporter.clipAnimations;

                if (clipAnimations.Length == 0)
                {
                    clipAnimations = modelImporter.defaultClipAnimations;
                }

                if (clipAnimations.Length > 0)
                {
                    for (int i = 0; i < clipAnimations.Length; i++)
                    {
                        string newClipName = Path.GetFileNameWithoutExtension(assetPath);
                        clipAnimations[i].name = newClipName;
                    }

                    modelImporter.clipAnimations = clipAnimations;
                    modelImporter.SaveAndReimport();

                    Debug.Log($"Renamed animations in {assetPath} to {Path.GetFileNameWithoutExtension(assetPath)}");
                }
                else
                {
                    Debug.LogWarning($"No animations found in {assetPath}");
                }
            }
            else
            {
                Debug.LogError($"Failed to get ModelImporter for {assetPath}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Finished renaming animations.");
    }
}
