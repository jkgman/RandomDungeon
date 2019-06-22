using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Enemy
{
	public class Enemy : MonoBehaviour
	{
		bool isActive = true;
		public EnemyStats stats;

		void Awake()
		{
			stats = GetComponent<EnemyStats>();
		}

		public bool CanBeTargeted()
		{
			//This is later used for mimics and other enemies that should not be possible to target at some states
			return isActive; 
		}
	}
}
