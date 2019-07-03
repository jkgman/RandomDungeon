using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Characters
{
	public class EnemyAnimationHandler : CharacterAnimationHandler
	{
		public void SetAttackStarted()
		{
			Animator.SetBool("isAttacking", true);
		}
		public void SetAttackCancelled()
		{
			Animator.SetBool("isAttacking", false);
		}

	}
}
