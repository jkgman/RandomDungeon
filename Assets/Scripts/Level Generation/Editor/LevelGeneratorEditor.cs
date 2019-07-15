using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor
{
    LevelGenerator gen;
    Editor levelSettings;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Generate"))
        {
            gen.Generate(false);
        }
        CreateCachedEditor(gen.settings, null, ref levelSettings);
        levelSettings.OnInspectorGUI();
    }

    private void OnEnable()
    {
        gen = (LevelGenerator)target;
    }
}
#endif