using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon
{
	public class Targetable : MonoBehaviour, ITargetable
	{
		bool targetable = true;



		public Vector3 GetPosition()
		{
			return transform.position;
		}

		public Transform GetTransform()
		{
			return transform;
		}

		public bool IsTargetable()
		{
			return targetable;
		}

		public void SetTargetable(bool state)
		{
			targetable = state;
		}
	}
}
