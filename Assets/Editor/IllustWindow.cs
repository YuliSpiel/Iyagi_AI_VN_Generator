using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class IllustWindow : EditorWindow
{
    [MenuItem("Window/Illust")]
    static void Init()
    {
        IllustWindow window = GetWindow<IllustWindow>();
        window.minSize = new Vector2(200, 200);
        window.maxSize = new Vector2(1000, 500);
        window.Show();
    }

    void OnGUI()
    {
        
    }
}
