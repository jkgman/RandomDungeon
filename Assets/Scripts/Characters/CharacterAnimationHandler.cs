using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor.Animations;

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

		protected string currentChargeClipName;
		protected string currentAttackClipName;
		protected string currentRecoveryClipName;
		float chargeDuration;
		float attackDuration;
		float recoveryDuration;

		protected RuntimeAnimatorController ac;
		protected List<AnimationClip> animClips;
		protected List<AnimationState> animationStates;

		protected AnimationState chargeState;
		protected AnimationState attackState;
		protected AnimationState recoveryState;


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
		protected List<AnimationState> GetAllStates()
		{
			List<AnimationState> output = new List<AnimationState>();

			//ac = Animator.runtimeAnimatorController;
			//Animator.stat
			//foreach (AnimatorControllerLayer i in ac.layers) //for each layer
			//{
			//	AnimatorStateMachine stateMachine = i.stateMachine;
			//	List<AnimatorState> states = GetAllStatesInMachine(stateMachine);

			//	foreach (var s in states)
			//	{
			//		if (!output.Contains(s))
			//			output.Add(s);
			//	}
			//}
			return output;
		}

		#endregion


		protected virtual void Awake()
		{
			animationStates = GetAllStates();
			SetStateVariables();
		}

		//Assigns values to animationState variables from the animator.
		protected virtual void SetStateVariables()
		{
			for (int i = 0; i < animationStates.Count; i++)
			{
				if (animationStates[i].name == CHARGE_STATE)
				{
					currentChargeClipName = animationStates[i].clip.name;
					chargeState = animationStates[i];
				}
				if (animationStates[i].name == ATTACK_STATE)
				{
					currentAttackClipName = animationStates[i].clip.name;
					attackState = animationStates[i];
				}
				if (animationStates[i].name == RECOVERY_STATE)
				{
					currentRecoveryClipName = animationStates[i].clip.name;
					recoveryState = animationStates[i];
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
		public void SetAttackDurations(float in_chargeDuration, float in_attackDuration, float in_recoveryDuration)
		{
			chargeDuration = in_chargeDuration;
			attackDuration = in_attackDuration;
			recoveryDuration = in_recoveryDuration;
			
			//if (chargeState)
			//{

			//	if (chargeDuration > 0)
			//		chargeState.speed = chargeState.length / chargeDuration;
			//	else
			//		chargeState.speed = ANIM_DEFAULT_SPEED;
			//}

			//if (attackState)
			//{
			//	if (attackDuration > 0)
			//		attackState.speed = attackState.length / attackDuration;
			//	else
			//		attackState.speed = ANIM_DEFAULT_SPEED;
			//}

			//if (recoveryState)
			//{
			//	if (recoveryDuration > 0)
			//		recoveryState.speed = recoveryState.length / recoveryDuration;
			//	else
			//		recoveryState.speed = ANIM_DEFAULT_SPEED;
			//}
		}

		//Called when Charge phase of attack starts
		public void SetChargeStarted()
		{
			Animator.SetBool("isAttacking", false);
			Animator.SetBool("isRecovering", false);
			Animator.SetBool("isCharging", true);
			//SetChargeDuration();

		}

		//void SetChargeDuration()
		//{
		//	AnimatorStateInfo info = new AnimatorStateInfo();
		//	for (int i = 0; i < Animator.layerCount; i++)
		//	{
		//		if (Animator.GetCurrentAnimatorStateInfo(i).IsName(CHARGE_STATE))
		//			info = Animator.GetCurrentAnimatorStateInfo(i);
		//		if (Animator.GetNextAnimatorStateInfo(i).IsName(CHARGE_STATE))
		//			info = Animator.GetCurrentAnimatorStateInfo(i);

		//	}

		//	if (info.IsName(CHARGE_STATE))
		//	{
		//		if (chargeDuration > 0)
		//			chargeState.speed = chargeState.length / chargeDuration;
		//		else
		//			chargeState.speed = ANIM_DEFAULT_SPEED;
		//	}


		//}
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
