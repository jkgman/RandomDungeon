using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Dungeon.Characters
{
	using Dungeon.Items;

	public class CharacterCombatHandler : MonoBehaviour
	{

		[Header("Attack shit")]
		[SerializeField] protected Transform rightHand;
		[SerializeField] protected Transform leftHand;
		protected IEnumerator currentAttackCo;
		private Weapon currentWeapon;
		protected Weapon GetCurrentWeapon()
		{
			return currentWeapon;
		}

		public bool IsBlocking
		{
			get;
			protected set;
		}
		public bool IsDodging
		{
			get;
			protected set;
		}
		public bool IsStunned
		{
			get;
			protected set;
		}
		public bool IsInvincible
		{
			get;
			protected set;
		}

		private Character _character;
		private Character Character
		{
			get
			{
				if (!_character)
					_character = GetComponent<Character>();

				return _character;
			}
		}

		#region Initialization

		protected virtual void Awake()
		{
			currentWeapon = GetComponentInChildren<Weapon>();
			if (currentWeapon)
				currentWeapon.CurrentEquipper = transform;
		}
		protected virtual void Start()
		{

		}
		protected virtual void OnDisable()
		{

		}

		#endregion


		#region Attack stuff

		void SetAttackDurations()
		{
			float charge = currentWeapon.GetChargeDuration();
			float attack = currentWeapon.GetAttackDuration();
			float recovery = currentWeapon.GetRecoveryDuration();

			Character.AnimationHandler.SetAttackDurations(charge, attack, recovery);
		}

		protected virtual void Attack()
		{
			if (currentAttackCo != null)
			{
				StopCoroutine(currentAttackCo);
				Debug.Log("Stopped attack coroutine");
			}

			currentAttackCo = LightAttackRoutine();
			StartCoroutine(currentAttackCo);
		}

		protected IEnumerator LightAttackRoutine()
		{
			Debug.Log("attack coroutine started");
			currentWeapon.StartAttacking(AttackType.lightAttack);
			SetAttackDurations();

			yield return null; //Wait for one frame because animator sucks ass (ignores booleans if setting durations in same frame)

			yield return StartCoroutine(LightAttackCharge());
			yield return StartCoroutine(LightAttackAttack());
			yield return StartCoroutine(LightAttackRecovery());

			currentWeapon.EndAttacking();

		}

		protected IEnumerator LightAttackCharge()
		{
			currentWeapon.CurrentAttackState = AttackState.charge;
			Character.AnimationHandler.SetChargeStarted();

			float t = 0;
			float t01 = 0;
			float moveOffset = 0;
			float offsetTotal = 0;
			Vector3 moveDirection = Character.CharacterController.GetFlatMoveDirection(allowZero: false);

			while (currentWeapon.IsAttacking &&
					currentWeapon.CurrentAttackType == AttackType.lightAttack &&
					t < currentWeapon.GetCurrentActionDuration())
			{
				t01 = Mathf.Clamp01(t / currentWeapon.GetCurrentActionDuration());
				moveOffset = currentWeapon.GetMoveDistanceFromCurve(t01) - offsetTotal;
				moveDirection = UpdateAttackMoveDirection(moveDirection);
				CharacterMovementDuringAttack(moveDirection, moveOffset);

				offsetTotal = currentWeapon.GetMoveDistanceFromCurve(t01);
				t += Time.smoothDeltaTime;
				yield return null;
			}

			Character.AnimationHandler.SetChargeCancelled();

		}

		protected virtual void CharacterMovementDuringAttack(Vector3 moveDirection, float moveOffset)
		{
			Character.CharacterController.ExternalMove(moveDirection * moveOffset);
			
		}

		protected IEnumerator LightAttackAttack()
		{
			currentWeapon.CurrentAttackState = AttackState.attack;
			Character.AnimationHandler.SetAttackStarted();

			float t = 0;
			float t01 = 0;
			float moveOffset = 0;
			float offsetTotal = 0;

			Vector3 moveDirection = Character.CharacterController.GetFlatMoveDirection(allowZero: false);

			while (currentWeapon.IsAttacking &&
					currentWeapon.CurrentAttackType == AttackType.lightAttack &&
					t < currentWeapon.GetCurrentActionDuration())
			{
				t01 = Mathf.Clamp01(t / currentWeapon.GetCurrentActionDuration());
				moveDirection = UpdateAttackMoveDirection(moveDirection);

				t01 = Mathf.Clamp01(t / currentWeapon.GetCurrentActionDuration());
				moveOffset = currentWeapon.GetMoveDistanceFromCurve(t01) - offsetTotal;
				Character.CharacterController.ExternalMove(moveDirection * moveOffset);

				offsetTotal = currentWeapon.GetMoveDistanceFromCurve(t01);
				t += Time.smoothDeltaTime;
				yield return null;
			}

			Character.AnimationHandler.SetAttackCancelled();

		}
		protected IEnumerator LightAttackRecovery()
		{
			currentWeapon.CurrentAttackState = AttackState.recovery;
			Character.AnimationHandler.SetRecoveryStarted();

			float t = 0;
			float t01 = 0;
			float moveOffset = 0;
			float offsetTotal = 0;

			Vector3 moveDirection = Character.CharacterController.GetFlatMoveDirection(allowZero: false);


			while (currentWeapon.IsAttacking &&
					currentWeapon.CurrentAttackType == AttackType.lightAttack &&
					t < currentWeapon.GetCurrentActionDuration())
			{
				t01 = Mathf.Clamp01(t / currentWeapon.GetCurrentActionDuration());

				moveDirection = UpdateAttackMoveDirection(moveDirection);

				t01 = Mathf.Clamp01(t / currentWeapon.GetCurrentActionDuration());
				moveOffset = currentWeapon.GetMoveDistanceFromCurve(t01) - offsetTotal;
				Character.CharacterController.ExternalMove(moveDirection * moveOffset);

				offsetTotal = currentWeapon.GetMoveDistanceFromCurve(t01);
				t += Time.smoothDeltaTime;
				yield return null;
			}

			Character.AnimationHandler.SetRecoveryCancelled();
		}

		protected virtual Vector3 UpdateAttackMoveDirection(Vector3 current)
		{
			return transform.forward;
		}

	#endregion
	}
}
