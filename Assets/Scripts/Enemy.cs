using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	bool isActive = true;

    public bool CanBeTargeted()
	{
		//This is for mimics and other enemies that should not be possible to target at some points
		return isActive; 
	}
}
