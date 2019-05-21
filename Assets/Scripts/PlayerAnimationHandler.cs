using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

namespace Dungeon.Player
{
	[System.Serializable]
	public struct AttackAnimationData
	{
		public AnimationClip chargeClip;
		public AnimationClip attackClip;
		public AnimationClip recoveryClip;
	}

	public class PlayerAnimationHandler : MonoBehaviour
	{
		private const string DODGE_STATE = "DODGE";
		private const string CHARGE_STATE= "CHARGE";
		private const string ATTACK_STATE= "ATTACK";
		private const string RECOVERY_STATE= "RECOVERY";
		private const float ANIM_DEFAULT_SPEED = 1f;


		[SerializeField] private float blendSpeed = 5f;

		private AnimatorState dodgeState;
		private AnimatorState chargeState;
		private AnimatorState attackState;
		private AnimatorState recoveryState;

		private string currentChargeClipName;
		private string currentAttackClipName;
		private string currentRecoveryClipName;


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
					if (j.state.name == DODGE_STATE)
					{
						dodgeState = j.state;
					}
					if (j.state.name == CHARGE_STATE)
					{
						currentChargeClipName = j.state.motion.name;
						chargeState = j.state;
					}
					if (j.state.name == ATTACK_STATE)
					{
						currentAttackClipName = j.state.motion.name;
						attackState = j.state;
					}
					if (j.state.name == RECOVERY_STATE)
					{
						currentRecoveryClipName = j.state.motion.name;
						recoveryState = j.state;
					}
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

		public void SetAttackAnimations (AttackAnimationData data)
		{
			OverrideClips(currentChargeClipName, data.chargeClip);
			OverrideClips(currentAttackClipName, data.attackClip);
			OverrideClips(currentRecoveryClipName, data.recoveryClip);
		}

		public void SetChargeStarted(AnimationClip clip, float duration)
		{
			Animator.SetBool("isCharging", true);
		}
		public void SetChargeCancelled()
		{
			Animator.SetBool("isCharging", false);
		}

		public void SetAttackStarted(float duration = -1f)
		{
			Animator.SetBool("attack", true);

			float d = duration;

			//AnimatorState attackState = GetAnimStateByName(attackAnimName);

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


		private void OverrideClips(string animName, AnimationClip in_clip)
		{
			
			AnimatorOverrideController aoc = new AnimatorOverrideController();
			aoc.runtimeAnimatorController = Animator.runtimeAnimatorController;
			var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();
			foreach ( var c in aoc.animationClips)
			{
				if (c.name == animName)
				{
					anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(c, in_clip));
					break;
				}
			}

			aoc.ApplyOverrides(anims);
			Animator.runtimeAnimatorController = aoc;
		}
	}
}