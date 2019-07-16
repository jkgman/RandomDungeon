using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor.Animations;

namespace Dungeon.Characters
{
	public class CharacterAnimationHandler : MonoBehaviour
	{

		protected const string CHARGE_NAME = "CHARGE";
		protected const string ATTACK_NAME = "ATTACK";
		protected const string RECOVERY_NAME = "RECOVERY";
		protected const float ANIM_DEFAULT_SPEED = 1f;


		//How fast blendTree values change.
		[SerializeField] protected float blendSpeed = 5f;
		protected Vector2 currentMoveBlend = Vector2.zero;
		
		private List<KeyValuePair<AnimationClip, AnimationClip>> currentOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
		private RuntimeAnimatorController mainController;


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
		}


		/// <summary>
		/// Replaces a clip inside animator. Original animation is found with string name and replaced with inserted AnimationClip.
		/// </summary>
		/// <param name="animName">The currently existing animation in animator.</param>
		/// <param name="in_clip">Clip to replace the current animation with.</param>
		protected void AddOverrideClip(string animName, AnimationClip in_clip)
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
		/// Resets overridden clip back to default inside animator.
		/// </summary>
		/// <param name="animName">The currently existing animation's name in animator.</param>
		protected void RemoveOverrideClip(string animName)
		{
			AnimatorOverrideController aoc = new AnimatorOverrideController();
			aoc.runtimeAnimatorController = mainController;

			for (int i = 0; i < currentOverrides.Count; i++)
			{
				if (currentOverrides[i].Key.name == animName)
				{
					currentOverrides.RemoveAt(i);
				}
			}

			aoc.ApplyOverrides(currentOverrides);
			Animator.runtimeAnimatorController = aoc;
		}


		/// <summary>
		/// Call every frame when movement input happens.
		/// </summary>
		/// <param name="blendParam">Normalized movement vector direction local to where character faces.</param>
		public void SetMovementPerformed(Vector2 blendParam)
		{
			currentMoveBlend = Vector2.Lerp(currentMoveBlend, blendParam, Time.deltaTime * blendSpeed);

			Animator.SetBool("isMoving", blendParam != Vector2.zero);
			Animator.SetFloat("xMove", currentMoveBlend.x);
			Animator.SetFloat("yMove", currentMoveBlend.y);

		}

		public void SetGrounded(bool state)
		{
			Animator.SetBool("isGrounded", state);
		}

		/// <summary>
		/// Updates animations to match weapon's current attack. Sets animation speeds according to weapon's current attack duration.
		/// </summary>
		public void SetAttackData(Items.Weapon.AttackData attackData)
		{

			if (attackData.chargeClip) {
				Animator.SetFloat(CHARGE_NAME, attackData.chargeClip.length / attackData.chargeDuration);
				AddOverrideClip(attackData.chargeClip.name, attackData.chargeClip);
			}
			if (attackData.attackClip){
				Animator.SetFloat(ATTACK_NAME, attackData.attackClip.length / attackData.attackDuration);
				AddOverrideClip(attackData.attackClip.name, attackData.attackClip);
			}
			if (attackData.recoveryClip){
				Animator.SetFloat(RECOVERY_NAME, attackData.recoveryClip.length / attackData.recoveryDuration);
				AddOverrideClip(attackData.recoveryClip.name, attackData.recoveryClip);
			}

		}
		/// <summary>
		///Called when Charge phase of attack starts
		/// </summary>
		public void SetChargeStarted()
		{
			Animator.SetBool("isAttacking", false);
			Animator.SetBool("isRecovering", false);
			Animator.SetBool("isCharging", true);

		}

		/// <summary>
		///Called when Recovery phase of attack ends. Usually goes to attack phase right after.
		/// </summary>
		public void SetChargeCancelled()
		{
			Animator.SetBool("isCharging", false);
		}

		/// <summary>
		///Called when attack phase (hitting action) starts.
		/// </summary>
		public void SetAttackStarted()
		{
			Animator.SetBool("isCharging", false);
			Animator.SetBool("isRecovering", false);
			Animator.SetBool("isAttacking", true);

		}

		/// <summary>
		///Called when attack (hitting phase of the whole attack) ends.
		/// </summary>
		public void SetAttackCancelled()
		{
			Animator.SetBool("isAttacking", false);
		}

		/// <summary>
		///Called when Recovery phase of attack starts
		/// </summary>
		public void SetRecoveryStarted()
		{
			Animator.SetBool("isCharging", false);
			Animator.SetBool("isAttacking", false);
			Animator.SetBool("isRecovering", true);

		}
		/// <summary>
		///Called when Recovery phase of attack ends or is cancelled.
		/// </summary>
		public void SetRecoveryCancelled()
		{
			Animator.SetBool("isRecovering", false);
		}
	}
}
