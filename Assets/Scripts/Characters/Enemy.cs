using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Characters
{
	public class Enemy : Character, ITargetable
	{
		bool isActive = true;

		protected override IEnumerator DieRoutine()
		{
			//Death animation or whatever.
			//For now it is just a particle effect and disappear.

			Effects.PlayDeathParticles();
			Effects.SetInvisible();

			isActive = false;
			yield return new WaitForSeconds(2f);
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
