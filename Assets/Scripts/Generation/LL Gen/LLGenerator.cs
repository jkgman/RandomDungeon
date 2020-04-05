using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LLGenerator : ScriptableObject
{
    protected HLArea area;

    public virtual void Generate(HLArea containingArea) {
        area = containingArea;
        StartGen();
    }
    protected abstract void StartGen();
}
