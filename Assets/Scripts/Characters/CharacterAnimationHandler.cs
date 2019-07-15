using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor.Animations;

namespace Dungeon.Characters
{
	public class CharacterAnimationHandler : MonoBehaviour
	{
		private class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
		{
			public AnimationClipOverrides(int capacity) : base(capacity) { }

			public AnimationClip this[string name]
			{
				get { return this.Find(x => x.Key.name.Equals(name)).Value; }
				set
				{
					int index = this.FindIndex(x => x.Key.name.Equals(name));
					if (index != -1)
						this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
				}
			}
		}

		protected const string CHARGE_NAME = "CHARGE";
		protected const string ATTACK_NAME = "ATTACK";
		protected const string RECOVERY_NAME = "RECOVERY";

		protected const float ANIM_DEFAULT_SPEED = 1f;

		//How fast blendTree values change.
		[SerializeField] protected float blendSpeed = 5f;
		protected Vector2 currentMoveBlend = Vector2.zero;
		
		protected RuntimeAnimatorController ac;
		protected List<AnimationClip> animClips;
		protected List<AnimationState> animationStates;

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

		#endregion


		protected virtual void Awake()
		{
			mainController = Animator.runtimeAnimatorController;
			//animationStates = GetAllStates();
			//SetStateVariables();
		}

		//Assigns values to animationState variables from the animator.
		//protected virtual void SetStateVariables()
		//{
		//	for (int i = 0; i < animationStates.Count; i++)
		//	{
		//		if (animationStates[i].name == CHARGE_STATE)
		//		{
		//			currentChargeClipName = animationStates[i].clip.name;
		//			chargeState = animationStates[i];
		//		}
		//		if (animationStates[i].name == ATTACK_STATE)
		//		{
		//			currentAttackClipName = animationStates[i].clip.name;
		//			attackState = animationStates[i];
		//		}
		//		if (animationStates[i].name == RECOVERY_STATE)
		//		{
		//			currentRecoveryClipName = animationStates[i].clip.name;
		//			recoveryState = animationStates[i];
		//		}
		//	}
		//}

		List<KeyValuePair<AnimationClip, AnimationClip>> currentOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
		RuntimeAnimatorController mainController;
		/// <summary>
		/// Replaces a clip inside animator. Original animation is found with string name and replaced with inserted AnimationClip.
		/// </summary>
		/// <param name="animName">The currently existing animation in animator.</param>
		/// <param name="in_clip">Clip to replace the current animation with.</param>
		protected void OverrideClips(string animName, AnimationClip in_clip)
		{
			AnimatorOverrideController aoc = new AnimatorOverrideController();
			aoc.runtimeAnimatorController = mainController;
			bool replaced = false;

			for (int i = 0; i < currentOverrides.Count; i++)
			{
				if(currentOverrides[i].Key.name == animName)
				{
					currentOverrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(currentOverrides[i].Key, in_clip);
					replaced = true;
				}
			}
			if (!replaced)
			{
				foreach (var c in aoc.animationClips)
				{
					if (c.name == animName)
					{
						currentOverrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(c, in_clip));
					}
				}
			}

			aoc.ApplyOverrides(currentOverrides);
			Animator.runtimeAnimatorController = aoc;
		}

		/// <summary>
		/// Before attack starts, weapon gives appropriate animations for animator to play.
		/// This way animator only has one attack state that plays all animations of all weapons.
		/// </summary>
		public void OverrideAnimations(AnimatorOverrideController aoc)
		{
			Animator.runtimeAnimatorController = aoc;
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
		public void SetAttackData(Items.Weapon.AttackData attackData, float in_chargeDuration, float in_attackDuration, float in_recoveryDuration)
		{
			//var aoc = new AnimatorOverrideController(Animator.runtimeAnimatorController);

			if (attackData.chargeClip) {
				Animator.SetFloat(CHARGE_NAME, attackData.chargeClip.length / in_chargeDuration);
				//aoc[attackData.chargeClip.name] = attackData.chargeClip;
				OverrideClips(attackData.chargeClip.name, attackData.chargeClip);
			}
			if (attackData.attackClip){
				Animator.SetFloat(ATTACK_NAME, attackData.attackClip.length / in_attackDuration);
				OverrideClips(attackData.attackClip.name, attackData.attackClip);
			}
			if (attackData.recoveryClip){
				Animator.SetFloat(RECOVERY_NAME, attackData.recoveryClip.length / in_recoveryDuration);
				OverrideClips(attackData.recoveryClip.name, attackData.recoveryClip);
			}

			//aoc[attackData.attackClip.name] = attackData.attackClip;
			//aoc[attackData.recoveryClip.name] = attackData.recoveryClip;
			//Animator.runtimeAnimatorController = aoc;
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
