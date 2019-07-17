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

		protected float currentAttackStateTime;
		protected bool attackPending;

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

		protected virtual void OnEnable()
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

		protected virtual void Update()
		{
			currentAttackStateTime += Time.deltaTime;

		}

		#endregion


		#region Attack stuff

		void SetAttackData()
		{
			Character.AnimationHandler.SetAttackData(currentWeapon.GetCurrentAttackData());
		}

		protected virtual void Attack()
		{
			if (currentWeapon.IsAttacking && !currentWeapon.AttackAllowed(currentAttackStateTime) && currentWeapon.AttackPendingAllowed(currentAttackStateTime))
			{
				//Start waiting until attack is available.
				attackPending = true;
				StartCoroutine(WaitForPendingAttack());
			}
			else //Start attck
			{
				if (currentAttackCo != null)
				{
					StopCoroutine(currentAttackCo);
					Debug.Log("Stopped attack coroutine");
				}

				currentAttackCo = LightAttackRoutine();
				StartCoroutine(currentAttackCo);
			}
		}

		IEnumerator WaitForPendingAttack()
		{
			while(attackPending)
			{
				if (!currentWeapon.IsAttacking || currentWeapon.AttackAllowed(currentAttackStateTime))
				{
					attackPending = false;
					Attack();
				}
				yield return null;
			}
		}

		protected virtual void CancelAttack()
		{
			if (currentWeapon.IsAttacking && currentWeapon.AttackCancellable())
			{
				if (currentAttackCo != null)
				{
					StopCoroutine(currentAttackCo);
					Debug.Log("Stopped attack coroutine");
					currentWeapon.EndAttacking();
				}
			}
		}

		protected void MoveCharacter(float offsetTotal, float t01)
		{
			float moveOffset = currentWeapon.GetMoveDistanceFromCurve(t01) - offsetTotal;
			Character.CharacterController.ExternalMove(transform.forward * moveOffset);

		}


		protected IEnumerator LightAttackRoutine()
		{
			Debug.Log("attack coroutine started");
			currentWeapon.StartAttacking(AttackType.lightAttack);
			SetAttackData();

			yield return null; //Wait for one frame because animator sucks ass (ignores booleans if setting durations in same frame)

			yield return StartCoroutine(LightAttackCharge());
			yield return StartCoroutine(LightAttackAttack());
			yield return StartCoroutine(LightAttackRecovery());

			currentWeapon.EndAttacking();

		}


		protected virtual IEnumerator LightAttackCharge()
		{
			currentWeapon.CurrentAttackState = AttackState.charge;
			Character.AnimationHandler.SetChargeStarted();

			float t01 = 0;
			float offsetTotal = 0;
			currentAttackStateTime = 0;


			while (currentWeapon.IsAttacking &&
					currentWeapon.CurrentAttackType == AttackType.lightAttack &&
					currentAttackStateTime < currentWeapon.GetCurrentActionDuration())
			{
				t01 = Mathf.Clamp01(currentAttackStateTime / currentWeapon.GetCurrentActionDuration());
				MoveCharacter(offsetTotal, t01);

				Character.CharacterController.SetMoveSpeedMultiplier(currentWeapon.GetMoveSpeedMultiplier(t01));
				Character.CharacterController.SetRotationSpeedMultiplier(currentWeapon.GetRotationSpeedMultiplier(t01));



				offsetTotal = currentWeapon.GetMoveDistanceFromCurve(t01);
				yield return null;
			}

			Character.AnimationHandler.SetChargeCancelled();

		}


		protected virtual IEnumerator LightAttackAttack()
		{
			currentWeapon.CurrentAttackState = AttackState.attack;
			Character.AnimationHandler.SetAttackStarted();

			float t01 = 0;
			float offsetTotal = 0;
			currentAttackStateTime = 0;

			while ( currentWeapon.IsAttacking &&
					currentWeapon.CurrentAttackType == AttackType.lightAttack &&
					currentAttackStateTime < currentWeapon.GetCurrentActionDuration())
			{
				t01 = Mathf.Clamp01(currentAttackStateTime / currentWeapon.GetCurrentActionDuration());

				MoveCharacter(offsetTotal, t01);
				Character.CharacterController.SetMoveSpeedMultiplier(currentWeapon.GetMoveSpeedMultiplier(t01));
				Character.CharacterController.SetRotationSpeedMultiplier(currentWeapon.GetRotationSpeedMultiplier(t01));

				offsetTotal = currentWeapon.GetMoveDistanceFromCurve(t01);
				yield return null;
			}

			Character.AnimationHandler.SetAttackCancelled();

		}
		protected virtual IEnumerator LightAttackRecovery()
		{
			currentWeapon.CurrentAttackState = AttackState.recovery;
			Character.AnimationHandler.SetRecoveryStarted();

			float t01 = 0;
			float offsetTotal = 0;
			currentAttackStateTime = 0;

			while ( currentWeapon.IsAttacking &&
					currentWeapon.CurrentAttackType == AttackType.lightAttack &&
					currentAttackStateTime < currentWeapon.GetCurrentActionDuration())
			{
				t01 = Mathf.Clamp01(currentAttackStateTime / currentWeapon.GetCurrentActionDuration());

				MoveCharacter(offsetTotal, t01);
				Character.CharacterController.SetMoveSpeedMultiplier(currentWeapon.GetMoveSpeedMultiplier(t01));
				Character.CharacterController.SetRotationSpeedMultiplier(currentWeapon.GetRotationSpeedMultiplier(t01));

				offsetTotal = currentWeapon.GetMoveDistanceFromCurve(t01);
				yield return null;
			}

			Character.AnimationHandler.SetRecoveryCancelled();
		}
		
	#endregion
	}
}
