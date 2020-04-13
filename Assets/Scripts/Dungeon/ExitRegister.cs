using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitRegister : MonoBehaviour
{
    void Start()
    {
        LevelInitializer init = FindObjectOfType<LevelInitializer>();
        if (init)
        {
            init.ExitLocation = transform.position;
        }
        else
        {
            throw new System.Exception("No LevelInitializer found in scene.");
        }
    }
}
