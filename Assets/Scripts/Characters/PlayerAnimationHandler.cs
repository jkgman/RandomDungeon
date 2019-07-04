using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

namespace Dungeon.Characters
{

	public class PlayerAnimationHandler : CharacterAnimationHandler
	{
		private AnimatorState dodgeState;


		//Assigns values to animationState variables from the animator.
		protected override void SetStateVariables()
		{
			base.SetStateVariables();

			for (int i = 0; i < animatorStates.Count; i++)
			{
				if (animatorStates[i].name == DODGE_STATE)
				{
					dodgeState = animatorStates[i];
					break;
				}
			}
		}


		//Called when dodge action starts
		public void SetDodgeStarted(Vector2 direction, float duration = -1f)
		{
			Animator.SetBool("isDodging", true);
			float d = duration;

			if (dodgeState)
			{
				Motion motion = dodgeState.motion;
				float currentDuration = motion.averageDuration;

				if (d > 0)
					dodgeState.speed = currentDuration / d;
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