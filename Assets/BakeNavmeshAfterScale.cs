using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshSurface))]
public class BakeNavmeshAfterScale : MonoBehaviour
{
	NavMeshSurface surface;
	Vector3 scale;

	void OnEnable()
	{
		surface = GetComponent<NavMeshSurface>();
		scale = transform.lossyScale;
	}
	void Update()
	{
		if (surface && scale != transform.lossyScale || surface.navMeshData == null)
		{
			surface.RemoveData();
			surface.BuildNavMesh();
			scale = transform.lossyScale;
		}
	}
}
