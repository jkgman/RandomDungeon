using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Definition/Grammar")]
public class ScriptableGrammar : ScriptableObject
{
    [SerializeField]
    private Grammar grammar;
    [SerializeField]
    private List<Transform> ConnectionTransforms = new List<Transform>();
    public Grammar Grammar { get => grammar; }
    public Grammar RuntimeGrammar { get => grammar.GetCopy(); }

    public void CompileConnections() {
        grammar.Connections.RemoveRange(0, grammar.Connections.Count);
        for (int i = 0; i < ConnectionTransforms.Count; i++)
        {
            grammar.Connections.Add(new Connection(ConnectionTransforms[i].position, ConnectionTransforms[i].rotation.eulerAngles));
        }
    }

}

[CustomEditor(typeof(ScriptableGrammar))]
public class LookAtPointEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Compile Connections"))
        {
            ((ScriptableGrammar)target).CompileConnections();
        }
        SetDirty();
    }
}