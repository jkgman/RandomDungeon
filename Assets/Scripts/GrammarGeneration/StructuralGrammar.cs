using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StructuralGrammar
{
    [SerializeField]
    private ScriptableGrammar _grammar;
    [SerializeField]
    private List<Pairing> _pairs;
    [System.NonSerialized]
    public bool instantiated = false;
    private Grammar runtimeGrammar;

    public StructuralGrammar(ScriptableGrammar grammar, List<Pairing> pairs)
    {
        _grammar = grammar;
        _pairs = pairs;
        runtimeGrammar = _grammar.Grammar.GetCopy();
    }

    public Grammar GrammarSettings { get => runtimeGrammar; }
    public List<Pairing> Pairs { get => _pairs; set => _pairs = value; }

    public void ReplacePairingIndex(int replaceGrammarIndex, int replaceGrammarConnectionPoint, int newGrammarIndex, int newGrammarConnectionPoint)
    {
        for (int i = 0; i < _pairs.Count; i++)
        {
            if (_pairs[i].ExternalConnectionGrammarIndex == replaceGrammarIndex && _pairs[i].ExternalConnectionPointIndex == replaceGrammarConnectionPoint)
            {

                _pairs[i] = new Pairing(_pairs[i].InternalConnectionPointIndex, newGrammarIndex, newGrammarConnectionPoint);
            }
        }
    }
    public StructuralGrammar GetCopy()
    {
        List<Pairing> pairs = new List<Pairing>();
        for (int j = 0; j < _pairs.Count; j++)
        {
            pairs.Add(new Pairing(_pairs[j].InternalConnectionPointIndex, _pairs[j].ExternalConnectionGrammarIndex, _pairs[j].ExternalConnectionPointIndex));
        }
        return new StructuralGrammar(_grammar, pairs);
    }
}