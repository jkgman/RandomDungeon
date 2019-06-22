using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Enemy
{
	public class Enemy : MonoBehaviour, ITargetable
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
		private CharacterBuffsAndEffects _effects;
		public CharacterBuffsAndEffects Effects
		{
			get
			{
				if (!_effects)
					_effects = GetComponent<CharacterBuffsAndEffects>();
				return _effects;
			}
		}


		void Update()
		{
			if (!Stats.health.IsAlive())
			{
				StartCoroutine(DieRoutine());
			}
		}

		IEnumerator DieRoutine()
		{
			//Death animation or whatever.
			//For now it is just a particle effect and disappear.

			Effects.PlayDeathParticles();
			Effects.SetInvisible();
			yield return new WaitForSeconds(2f);
			isActive = false;
			Destroy(this.gameObject);
		}

		public bool IsTargetable()
		{
			return Stats.health.IsAlive() && isActive;
		}

		public Vector3 GetPosition()
		{
			return transform.position;
		}

		public Transform GetTransform()
		{
			return transform;
		}
	}
}
