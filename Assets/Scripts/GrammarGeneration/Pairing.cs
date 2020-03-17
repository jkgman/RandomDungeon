using UnityEngine;

[System.Serializable]
public struct Pairing
{
    [SerializeField]
    int _externalConnectionGrammarIndex;
    [SerializeField]
    int _internalConnectionPointIndex;
    [SerializeField]
    int _externalConnectionPointIndex;

    /// <summary>
    /// Index in structure for external grammar for this pair
    /// </summary>
    public int ExternalConnectionGrammarIndex { get => _externalConnectionGrammarIndex;}
    /// <summary>
    /// index of this grammars connection for this pair
    /// </summary>
    public int InternalConnectionPointIndex { get => _internalConnectionPointIndex; }
    /// <summary>
    /// index of targets connection for this pair
    /// </summary>
    public int ExternalConnectionPointIndex { get => _externalConnectionPointIndex; }


    public Pairing(int internalConnectionPointIndex, int externalConnectionGrammarIndex, int externalConnectionPointIndex)
    {
        _externalConnectionGrammarIndex = externalConnectionGrammarIndex;
        _internalConnectionPointIndex = internalConnectionPointIndex;
        _externalConnectionPointIndex = externalConnectionPointIndex;
    }
}
