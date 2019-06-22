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
		private List<AnimatorState> animatorStates;



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

		/// <summary>Get All states in current AnimatorController.</summary>
		List<AnimatorState> GetAllStates()
		{
			List<AnimatorState> output = new List<AnimatorState>();

			ac = Animator.runtimeAnimatorController as AnimatorController;

			foreach (AnimatorControllerLayer i in ac.layers) //for each layer
			{
				AnimatorStateMachine stateMachine = i.stateMachine;
				List<AnimatorState> states = GetAllStatesInMachine(stateMachine);

				foreach (var s in states)
				{
					if (!output.Contains(s))
						output.Add(s);
				}
			}
			return output;
		}
		/// <summary>Get all states in a state machine.</summary>
		List<AnimatorState> GetAllStatesInMachine(AnimatorStateMachine stateMachine)
		{
			List<AnimatorState> output = new List<AnimatorState>();

			if (stateMachine.stateMachines.Length > 0)
			{
				foreach (var sm in stateMachine.stateMachines)
				{
					List<AnimatorState> foundStates = GetStatesInChildMachine(sm);
					for (int i = 0; i < foundStates.Count; i++)
					{
						if (!output.Contains(foundStates[i]))
							output.Add(foundStates[i]);
					}
				}
			}
			foreach (var state in stateMachine.states)
			{
				if (!output.Contains(state.state))
					output.Add(state.state);
			}


			return output;
		}
		/// <summary>Get all states in a child state machine. Works Recursively. </summary>
		List<AnimatorState> GetStatesInChildMachine(ChildAnimatorStateMachine childMachine)
		{
			List<AnimatorState> output = new List<AnimatorState>();
			if (childMachine.stateMachine.stateMachines.Length >0)
			{
				foreach(var sm in childMachine.stateMachine.stateMachines)
				{
					List<AnimatorState> childStates = GetStatesInChildMachine(sm);
					for (int i = 0; i < childStates.Count; i++)
					{
						if (!output.Contains(childStates[i]))
							output.Add(childStates[i]);
					}
				}
			}
			foreach (var state in childMachine.stateMachine.states)
			{
				if (!output.Contains(state.state))
					output.Add(state.state);
			}

			return output;
		}

		AnimatorState GetAnimStateByName(string name)
		{

			foreach (AnimatorState j in animatorStates) //for each state
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
			animatorStates = GetAllStates();
			SetStateVariables();
		}

		void SetStateVariables()
		{
			for (int i = 0; i < animatorStates.Count; i++)
			{
				if (animatorStates[i].name == DODGE_STATE)
				{
					dodgeState = animatorStates[i];
				}
				if (animatorStates[i].name == CHARGE_STATE)
				{
					currentChargeClipName = animatorStates[i].motion.name;
					chargeState = animatorStates[i];
				}
				if (animatorStates[i].name == ATTACK_STATE)
				{
					currentAttackClipName = animatorStates[i].motion.name;
					attackState = animatorStates[i];
				}
				if (animatorStates[i].name == RECOVERY_STATE)
				{
					currentRecoveryClipName = animatorStates[i].motion.name;
					recoveryState = animatorStates[i];
				}
			}
		}


		public void SetMovementPerformed(Vector2 blendParam)
		{
			currentMoveBlend = Vector2.Lerp(currentMoveBlend, blendParam, Time.deltaTime * blendSpeed);

			Animator.SetBool("isMoving", blendParam != Vector2.zero);
			Animator.SetFloat("xMove", currentMoveBlend.x);
			Animator.SetFloat("yMove", currentMoveBlend.y);

		}

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
			
		}

		public void SetDodgeCancelled()
		{
			Animator.SetBool("isDodging", false);
		}

		public void SetAttackAnimations (AttackAnimationData data)
		{
			OverrideClips(currentChargeClipName, data.chargeClip);
			OverrideClips(currentAttackClipName, data.attackClip);
			OverrideClips(currentRecoveryClipName, data.recoveryClip);

			currentChargeClipName = data.chargeClip.name;
			currentAttackClipName = data.attackClip.name;
			currentRecoveryClipName = data.recoveryClip.name;
		}

		public void SetAttackDurations(float chargeDuration, float attackDuration, float recoveryDuration)
		{
			if (chargeState)
			{
				Motion motion = chargeState.motion;
				float currentDuration = motion.averageDuration;

				if (chargeDuration > 0)
					chargeState.speed = currentDuration / chargeDuration;
				else
					chargeState.speed = ANIM_DEFAULT_SPEED;
			}

			if (attackState)
			{
				Motion motion = attackState.motion;
				float currentDuration = motion.averageDuration;

				if (attackDuration > 0)
					attackState.speed = currentDuration / attackDuration;
				else
					attackState.speed = ANIM_DEFAULT_SPEED;
			}

			if (recoveryState)
			{
				Motion motion = recoveryState.motion;
				float currentDuration = motion.averageDuration;

				if (recoveryDuration > 0)
					recoveryState.speed = currentDuration / recoveryDuration;
				else
					recoveryState.speed = ANIM_DEFAULT_SPEED;
			}
		}

		public void SetChargeStarted()
		{
			Animator.SetBool("isAttacking", false);
			Animator.SetBool("isRecovering", false);
			Animator.SetBool("isCharging", true);

		}
		public void SetChargeCancelled()
		{
			Animator.SetBool("isCharging", false);
		}

		public void SetAttackStarted()
		{
			Animator.SetBool("isCharging", false);
			Animator.SetBool("isRecovering", false);
			Animator.SetBool("isAttacking", true);

		}
		public void SetAttackCancelled()
		{
			Animator.SetBool("isAttacking", false);
		}

		public void SetRecoveryStarted()
		{
	
			Animator.SetBool("isCharging", false);
			Animator.SetBool("isAttacking", false);
			Animator.SetBool("isRecovering", true);


		}
		public void SetRecoveryCancelled()
		{
			Animator.SetBool("isRecovering", false);
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