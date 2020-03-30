using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Characters
{

	public class PlayerAnimationHandler : CharacterAnimationHandler
	{

		private AnimationState dodgeState;


		//Called when dodge action starts
		public void SetDodgeStarted(Vector2 direction, float duration = -1f)
		{
			Animator.SetBool("isDodging", true);
			float d = duration;

			if (dodgeState)
			{

				if (d > 0)
					dodgeState.speed = dodgeState.length / d;
				else
					dodgeState.speed = ANIM_DEFAULT_SPEED;
			}

			
			Animator.SetFloat("xDodge", direction.x);
			
		//Called when Charge phase of attack starts
		}
		
		//Called when dodge action ends or gets cancelled.
		public void SetDodgeCancelled()
		{
			Animator.SetBool("isDodging", false);
		}


	}
}