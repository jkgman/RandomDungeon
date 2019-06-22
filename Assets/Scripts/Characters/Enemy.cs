using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Enemy
{
	public class Enemy : MonoBehaviour
	{
		bool isActive = true;
		private EnemyStats _stats;
		public EnemyStats Stats
		{
			get
			{
				if (!_stats)
					_stats = GetComponent<EnemyStats>();
				return _stats;
			}
		}

		public bool CanBeTargeted()
		{
			//This is later used for mimics and other enemies that should not be possible to target at some states
			return isActive; 
		}
	}
}
