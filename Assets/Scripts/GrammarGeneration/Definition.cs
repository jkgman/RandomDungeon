using System.Collections.Generic;

[System.Serializable]
public struct Definition
{
    public string structureName;
    public List<Pairing> translator;

    public Definition(string structureName, List<Pairing> translator)
    {
        this.structureName = structureName;
        this.translator = translator;
    }
}
