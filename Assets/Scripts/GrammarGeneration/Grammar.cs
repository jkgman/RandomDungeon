using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Grammar {
    [SerializeField]
    private string _grammerName = "Grammar";
    [SerializeField]
    private GameObject _visual = default;
    [SerializeField]
    private List<Connection> _connections = new List<Connection>();
    [SerializeField]
    private List<Definition> _definitions = new List<Definition>();
    [System.NonSerialized]
    public bool Generated = false;

    public Grammar(string grammerName, GameObject visual, List<Connection> connections, List<Definition> definitions)
    {
        _grammerName = grammerName;
        _visual = visual;
        _connections = connections;
        _definitions = definitions;
    }

    public bool Terminant { get => (_definitions.Count <= 0); }
    public string GrammerName { get => _grammerName; }
    public GameObject Visual { get => _visual; }
    public List<Connection> Connections { get => _connections; }
    public List<Definition> Definitions { get => _definitions; }


    public Definition GetRandomDefinition()
    {
        return Definitions[Random.Range(0, Definitions.Count)];
    }

    public Grammar GetCopy() {
        List<Connection> connections = new List<Connection>();
        List<Definition> definitions = new List<Definition>();
        for (int i = 0; i < _connections.Count; i++)
        {
            connections.Add(new Connection(_connections[i].Position, _connections[i].Direction));
        }
        for (int i = 0; i < _definitions.Count; i++)
        {
            List<Pairing> translator = new List<Pairing>();
            for (int j = 0; j < _definitions[i].translator.Count; j++)
            {
                translator.Add(new Pairing(_definitions[i].translator[j].InternalConnectionPointIndex, _definitions[i].translator[j].ExternalConnectionGrammarIndex, _definitions[i].translator[j].ExternalConnectionPointIndex));
            }
            definitions.Add(new Definition(_definitions[i].structureName, translator));
        }
        return new Grammar(_grammerName, _visual, connections, definitions);
    }
}
