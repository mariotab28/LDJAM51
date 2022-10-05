using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
class CustomEditorTools : EditorWindow
{
    [MenuItem("Tools/PiecesDatabaseTool")]
    public static void ShowWindow()
    {
        GetWindow<CustomEditorTools>("PiecesDatabaseTool Window");
    }

    private void OnGUI()
    {
        GUILayout.Label("Reload Piece Database", EditorStyles.boldLabel);
        if (GUILayout.Button("Reload Pieces"))
        {
            GameObject.Find("GameManager").GetComponent<GameManager>().LoadPiecesData();
        }
    }
}
#endif
