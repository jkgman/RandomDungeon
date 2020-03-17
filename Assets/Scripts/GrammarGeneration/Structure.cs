using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Definition/Structure")]
public class Structure : ScriptableObject
{
    [SerializeField]
    List<StructuralGrammar> ScriptableStructuralGrammar = new List<StructuralGrammar>();
    [System.NonSerialized]
    List<StructuralGrammar> runtimeGrammars = new List<StructuralGrammar>();
    public List<StructuralGrammar> RuntimeGrammars {
        get {
            if (runtimeGrammars.Count == 0)
            {
                for (int i = 0; i < ScriptableStructuralGrammar.Count; i++)
                {
                    runtimeGrammars.Add(ScriptableStructuralGrammar[i].GetCopy());
                }
            }
            return runtimeGrammars;
        }
    }

    public void Deinitalize() {
        runtimeGrammars.RemoveRange(0, runtimeGrammars.Count - 1);
    }
}
