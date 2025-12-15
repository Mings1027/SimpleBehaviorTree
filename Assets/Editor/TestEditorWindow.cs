using UnityEditor;
using UnityEngine;

public class TestEditorWindow : EditorWindow
{
    [MenuItem("Window/Basic Editor Window")]
    public static void Open()
    {
        GetWindow<TestEditorWindow>("Basic Window");
    }

    private void OnGUI()
    {
        GUILayout.Label("Hello EditorWindow", EditorStyles.boldLabel);

        if (GUILayout.Button("Click Me"))
        {
            Debug.Log("Button clicked");
        }
    }
}