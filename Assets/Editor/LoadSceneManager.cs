using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class LoadSceneManager : EditorWindow
{
    private Vector2 scrollPosition;
    private string[] scenePaths;
    private string[] sceneNames;

    [MenuItem("Window/Scene Manager")]
    public static void ShowWindow()
    {
        GetWindow<LoadSceneManager>("Scene Manager");
    }

    private void OnEnable()
    {
        // Получаем все сцены из Build Settings
        RefreshSceneList();
    }

    private void RefreshSceneList()
    {
        scenePaths = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
        
        // Извлекаем только имена сцен без пути
        sceneNames = scenePaths
            .Select(path => System.IO.Path.GetFileNameWithoutExtension(path))
            .ToArray();
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene Manager", EditorStyles.boldLabel);

        // Кнопка обновления списка
        if (GUILayout.Button("Refresh Scene List"))
        {
            RefreshSceneList();
        }

        // Список сцен
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        for (int i = 0; i < sceneNames.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label(sceneNames[i], EditorStyles.label);
            
            // Сохраняем текущий цвет GUI
            Color originalColor = GUI.color;
            // Устанавливаем зеленый цвет для кнопки
            GUI.color = Color.green;
            
            if (GUILayout.Button("Load", GUILayout.Width(60)))
            {
                // Сохраняем текущую сцену перед переключением
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(scenePaths[i]);
                }
            }
            
            // Восстанавливаем исходный цвет
            GUI.color = originalColor;
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
    }
}