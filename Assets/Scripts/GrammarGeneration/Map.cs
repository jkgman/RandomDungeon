using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField]
    Structure Level;
    [SerializeField]
    Connection StartConnection;
    [SerializeField]
    StructureLibrary library;
    //list of all grammers

    //replace grammers until all are final types
    private void Start()
    {
        while (Generate(Level));
        Display(Level, Level.RuntimeGrammars[0], Level.RuntimeGrammars[0].GrammarSettings.Connections[0], StartConnection);
    }
    private void OnDestroy()
    {
        Level.Deinitalize();
    }
    private bool Generate(Structure structure) {
        for (int i = 0; i < structure.RuntimeGrammars.Count; i++)
        {
            if (structure.RuntimeGrammars[i].GrammarSettings.Terminant || structure.RuntimeGrammars[i].GrammarSettings.Generated)
            {
                continue;
            }
            else
            {
                StructuralGrammar currentGrammar = structure.RuntimeGrammars[i];
                //choose definition
                Definition definition = currentGrammar.GrammarSettings.GetRandomDefinition();
                int currentGrammarCount = structure.RuntimeGrammars.Count;
                //Add new grammars
                AddDefinition(structure, definition, currentGrammarCount);
                //set all pairs referencing the old piece to this 
                RehookPairings(structure, i, currentGrammarCount, definition);
                //todo remove list entry for old grammar, decrement all pairings referencing place of deletion or after by one

                structure.RuntimeGrammars[i].GrammarSettings.Generated = true;

                return true;
            }
        }
        return false;
    }

    private void AddDefinition(Structure structure, Definition definition, int newGrammarsOffset) {

        //add defiition grammars to structure
        Structure definingStructure = library.FindStructure(definition.structureName);
        for (int i = 0; i < definingStructure.RuntimeGrammars.Count; i++)
        {
            structure.RuntimeGrammars.Add(definingStructure.RuntimeGrammars[i]);
            //increment all pair references by quantitty of current grammars
            for (int j = 0; j < structure.RuntimeGrammars[structure.RuntimeGrammars.Count-1].Pairs.Count; j++)
            {
                structure.RuntimeGrammars[structure.RuntimeGrammars.Count - 1].Pairs[j] = new Pairing(
                    structure.RuntimeGrammars[structure.RuntimeGrammars.Count - 1].Pairs[j].InternalConnectionPointIndex,
                    structure.RuntimeGrammars[structure.RuntimeGrammars.Count - 1].Pairs[j].ExternalConnectionGrammarIndex + newGrammarsOffset,
                    structure.RuntimeGrammars[structure.RuntimeGrammars.Count - 1].Pairs[j].ExternalConnectionPointIndex);
            }
        }
    }

    private void RehookPairings(Structure structure, int grammarIndex, int newGrammarsOffset, Definition definition) {
        //rehooks incoming
        for (int i = 0; i < definition.translator.Count; i++)
        {
            for (int j = 0; j < structure.RuntimeGrammars.Count; j++)
            {
                structure.RuntimeGrammars[j].ReplacePairingIndex(grammarIndex, definition.translator[i].InternalConnectionPointIndex, definition.translator[i].ExternalConnectionGrammarIndex + newGrammarsOffset, definition.translator[i].ExternalConnectionPointIndex);
            }
        }
        //rehooks outgoing

    }

    int safetyBreak = 0;
    private void Display(Structure structure, StructuralGrammar currentGrammar, Connection thisGrammarConnection, Connection targetGrammarConnection )
    {
        safetyBreak++;
        if (safetyBreak >= 50)
        {
            throw new Exception("I am an error through new Exception");
        }
        if (currentGrammar.instantiated)
            return;

        float angleDiff = Vector3.SignedAngle(thisGrammarConnection.Direction , - targetGrammarConnection.Direction, Vector3.up);

        Transform grammarObject = Instantiate(currentGrammar.GrammarSettings.Visual, targetGrammarConnection.Position - thisGrammarConnection.Position, Quaternion.identity, transform).transform;
        grammarObject.RotateAround(targetGrammarConnection.Position, Vector3.up, angleDiff);
        currentGrammar.instantiated = true;

        for (int i = 0; i < currentGrammar.Pairs.Count; i++)
        {
            Vector3 pos = targetGrammarConnection.Position + (Quaternion.AngleAxis(angleDiff, Vector3.up) * (currentGrammar.GrammarSettings.Connections[currentGrammar.Pairs[i].InternalConnectionPointIndex].Position - thisGrammarConnection.Position));
            Vector3 dir = Quaternion.AngleAxis(angleDiff, Vector3.up) * currentGrammar.GrammarSettings.Connections[currentGrammar.Pairs[i].InternalConnectionPointIndex].Direction;
            Debug.Log(structure.RuntimeGrammars[currentGrammar.Pairs[i].ExternalConnectionGrammarIndex]);
            Debug.Log(structure.RuntimeGrammars[currentGrammar.Pairs[i].ExternalConnectionGrammarIndex].GrammarSettings.Connections[currentGrammar.Pairs[i].ExternalConnectionPointIndex]);
            Display(structure,
                structure.RuntimeGrammars[currentGrammar.Pairs[i].ExternalConnectionGrammarIndex],
                structure.RuntimeGrammars[currentGrammar.Pairs[i].ExternalConnectionGrammarIndex].GrammarSettings.Connections[currentGrammar.Pairs[i].ExternalConnectionPointIndex],
                new Connection(pos, dir));
        }
    }

    private void DisplayNextGrammar() {

    }
}
