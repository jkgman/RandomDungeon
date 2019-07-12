using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BakeNavmeshAfterScale : MonoBehaviour
{
	NavMeshSurface surface;
	NavMeshData meshData;
	Vector3 scale;
	void Awake()
	{
		meshData = new NavMeshData();
		scale = transform.lossyScale;
	}
	void Update()
	{
		if (scale != transform.lossyScale)
		{
			surface.UpdateNavMesh(meshData);
			scale = transform.lossyScale;
		}
	}
}
