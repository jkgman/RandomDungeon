using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

namespace Dungeon.Player
{
	public class PlayerAnimationHandler : MonoBehaviour
	{
		private const float ANIM_DEFAULT_SPEED = 1f;

		[SerializeField] private float blendSpeed = 5f;
		[SerializeField] private string dodgeAnimName = "DODGE";
		[SerializeField] private string attackAnimName = "ATTACK";



		private Vector2 currentMoveBlend = Vector2.zero;
		private AnimatorController ac;
		private List<AnimatorState> allStates;



		#region Getters & Setters

		private Animator _animator;
		private Animator Animator
		{
			get
			{
				if (!_animator)
					_animator = GetComponentInChildren<Animator>();

				return _animator;
			}
		}

		void GetAllStates()
		{
			ac = Animator.runtimeAnimatorController as AnimatorController;
			ChildAnimatorState[] ch_animStates;
			AnimatorStateMachine stateMachine;
			allStates = new List<AnimatorState>();

			foreach (AnimatorControllerLayer i in ac.layers) //for each layer
			{
				stateMachine = i.stateMachine;
				ch_animStates = stateMachine.states;
				foreach (ChildAnimatorState j in ch_animStates) //for each state
				{
					allStates.Add(j.state);
				}
			}


		}

		AnimatorState GetAnimStateByName(string name)
		{

			foreach (AnimatorState j in allStates) //for each state
			{
				if (j.name == name)
					return j;
			}

			Debug.LogWarning("Anim state was not found with name: " + name);
			return null;
		}

		#endregion


		void Awake()
		{
			GetAllStates();
		}



		public void SetMovementPerformed(Vector2 blendParam)
		{
			currentMoveBlend = Vector2.Lerp(currentMoveBlend, blendParam, Time.deltaTime * blendSpeed);

			Animator.SetBool("move", blendParam != Vector2.zero);
			Animator.SetFloat("sidewaysMove", currentMoveBlend.x);
			Animator.SetFloat("forwardMove", currentMoveBlend.y);

		}

		public void SetDodgeStarted(Vector2 direction, float duration = -1f)
		{
			Animator.SetBool("dodge", true);
			float d = duration;

			AnimatorState dodgeState = GetAnimStateByName(dodgeAnimName);

			if (dodgeState)
			{
				Motion motion = dodgeState.motion;
				float currentDuration = motion.averageDuration;

				if (d > 0)
					dodgeState.speed = currentDuration / d;
				else
					dodgeState.speed = ANIM_DEFAULT_SPEED;
			}

			
			Animator.SetFloat("sidewaysDodge", direction.x);
			Animator.SetFloat("forwardsDodge", direction.y);
			
		}

		public void SetDodgeCancelled()
		{
			Animator.SetBool("dodge", false);
		}

		public void SetAttackStarted(float duration = -1f)
		{
			Animator.SetBool("attack", true);

			float d = duration;

			AnimatorState attackState = GetAnimStateByName(attackAnimName);

			if (attackState)
			{
				Motion motion = attackState.motion;
				float currentDuration = motion.averageDuration;

				if (d > 0)
					attackState.speed = currentDuration / d;
				else
					attackState.speed = ANIM_DEFAULT_SPEED;

			
			}

		}
		public void SetAttackCancelled()
		{
			Animator.SetBool("attack", false);
		}
	}
}