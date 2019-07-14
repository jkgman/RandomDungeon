using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

namespace Dungeon.Characters
{
	public class CharacterAnimationHandler : MonoBehaviour
	{

		protected const string DODGE_STATE = "DODGE";
		protected const string CHARGE_STATE = "CHARGE";
		protected const string ATTACK_STATE = "ATTACK";
		protected const string RECOVERY_STATE = "RECOVERY";
		protected const float ANIM_DEFAULT_SPEED = 1f;

		//How fast blendTree values change.
		[SerializeField] protected float blendSpeed = 5f;
		protected Vector2 currentMoveBlend = Vector2.zero;

		protected AnimatorState chargeState;
		protected AnimatorState attackState;
		protected AnimatorState recoveryState;

		protected string currentChargeClipName;
		protected string currentAttackClipName;
		protected string currentRecoveryClipName;


		protected AnimatorController ac;
		protected List<AnimatorState> animatorStates;



		#region Getters & Setters

		private Animator _animator;
		protected Animator Animator
		{
			get
			{
				if (!_animator)
					_animator = GetComponentInChildren<Animator>();

				return _animator;
			}
		}

		/// <summary>Get All states in current AnimatorController.</summary>
		protected List<AnimatorState> GetAllStates()
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
		protected List<AnimatorState> GetAllStatesInMachine(AnimatorStateMachine stateMachine)
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

		/// <summary>Get all states in a child state machine.</summary>
		protected List<AnimatorState> GetStatesInChildMachine(ChildAnimatorStateMachine childMachine)
		{
			List<AnimatorState> output = new List<AnimatorState>();
			if (childMachine.stateMachine.stateMachines.Length > 0)
			{
				foreach (var sm in childMachine.stateMachine.stateMachines)
				{
					//Calls current function again if more child states are found inside this one.
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

		#endregion


		protected virtual void Awake()
		{
			animatorStates = GetAllStates();
			SetStateVariables();
		}

		//Assigns values to animationState variables from the animator.
		protected virtual void SetStateVariables()
		{
			for (int i = 0; i < animatorStates.Count; i++)
			{
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

		/// <summary>
		/// Replaces a clip inside animator. Original animation is found with string name and replaced with inserted AnimationClip.
		/// </summary>
		/// <param name="animName">The currently existing animation in animator.</param>
		/// <param name="in_clip">Clip to replace the current animation with.</param>
		protected void OverrideClips(string animName, AnimationClip in_clip)
		{

			AnimatorOverrideController aoc = new AnimatorOverrideController();
			aoc.runtimeAnimatorController = Animator.runtimeAnimatorController;
			var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();
			foreach (var c in aoc.animationClips)
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

		/// <summary>
		/// Before attack starts, weapon gives appropriate animations for animator to play.
		/// This way animator only has one attack state that plays all animations of all weapons.
		/// </summary>
		/// <param name="data">Includes clips for all attack phases.</param>
		public void SetAttackAnimations(Items.Weapon.AttackAnimationData data)
		{
			OverrideClips(currentChargeClipName, data.chargeClip);
			OverrideClips(currentAttackClipName, data.attackClip);
			OverrideClips(currentRecoveryClipName, data.recoveryClip);

			currentChargeClipName = data.chargeClip.name;
			currentAttackClipName = data.attackClip.name;
			currentRecoveryClipName = data.recoveryClip.name;
		}



		//Called every frame a voluntary movement happens.
		public void SetMovementPerformed(Vector2 blendParam)
		{
			currentMoveBlend = Vector2.Lerp(currentMoveBlend, blendParam, Time.deltaTime * blendSpeed);

			Animator.SetBool("isMoving", blendParam != Vector2.zero);
			Animator.SetFloat("xMove", currentMoveBlend.x);
			Animator.SetFloat("yMove", currentMoveBlend.y);

		}

		/// <summary>
		/// Before attack starts, all attack phases have durations determined by weapon and its current attack. This sets animations to match weapon's durations.
		/// </summary>
		/// <param name="chargeDuration">Duration of charge phase of the attack.</param>
		/// <param name="attackDuration">Duration of hitting/attacking phase of the attack.</param>
		/// <param name="recoveryDuration">Duration of recovery phase of the attack.</param>
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

		//Called when Charge phase of attack starts
		public void SetChargeStarted()
		{
			Animator.SetBool("isAttacking", false);
			Animator.SetBool("isRecovering", false);
			Animator.SetBool("isCharging", true);

		}
		//Called when Recovery phase of attack ends. Usually goes to attack phase right after.
		public void SetChargeCancelled()
		{
			Animator.SetBool("isCharging", false);
		}

		//Called when attack phase (hitting action inside the complete attack) starts
		public void SetAttackStarted()
		{
			Animator.SetBool("isCharging", false);
			Animator.SetBool("isRecovering", false);
			Animator.SetBool("isAttacking", true);

		}

		//Called when attack ends. Not necessarily "cancelled"
		public void SetAttackCancelled()
		{
			Animator.SetBool("isAttacking", false);
		}

		//Called when Recovery phase of attack starts
		public void SetRecoveryStarted()
		{

			Animator.SetBool("isCharging", false);
			Animator.SetBool("isAttacking", false);
			Animator.SetBool("isRecovering", true);


		}
		//Called when Recovery phase of attack ends or is cancelled.
		public void SetRecoveryCancelled()
		{
			Animator.SetBool("isRecovering", false);
		}
	}
}
