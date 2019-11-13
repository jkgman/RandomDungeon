using System.Collections;
using UnityEngine;



namespace Dungeon.Characters
{
	using Dungeon.Items;

	public class CharacterCombatHandler : MonoBehaviour
	{
		#region Variables & References

		//_______ Start of Exposed Variables
		[Header("Attack stuff")]
		[SerializeField] protected Transform rightHand;
		[SerializeField] protected Transform leftHand;
		//_______ End of Exposed Variables

		//_______ Start of Hidden Variables
		protected IEnumerator currentAttackCo;
		protected IEnumerator pendingAttackCo;

		protected float elapsedAttackTime;
		protected bool attackPending;
		protected bool interrupt;
		//_______ End of Hidden Variables


		//_______ Start of Class References
		private CharacterAnimationHandler _cAnimHandler;
		private CharacterAnimationHandler CAnimHandler
		{
			get
			{
				if (!_cAnimHandler) _cAnimHandler = GetComponentInChildren<CharacterAnimationHandler>();
				return _cAnimHandler;
			}
		}
		private CharacterMovement _cMovement;
		private CharacterMovement CMovement
		{
			get
			{
				if (!_cMovement) _cMovement = GetComponentInChildren<CharacterMovement>();
				return _cMovement;
			}
		}
		private Weapon _cWeapon;
		private Weapon CWeapon
		{
			get
			{
				if (!_cWeapon) _cWeapon = GetComponentInChildren<Weapon>();
				if (_cWeapon) _cWeapon.CurrentEquipper = transform;
				return _cWeapon;
			}
		}
		//_______ End of Class References

		#endregion Variables & References

		#region Getters & Setters

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

		#endregion Getters & Setters



		#region Initialization & Updates

		protected virtual void Update()
		{
			elapsedAttackTime += Time.deltaTime;
		}

		#endregion

		#region Exposed Functions
		/// <summary>
		/// Interrupts ongoing actions, such as attack.
		/// </summary>
		/// <param name="force">Forces the interruption to happen, even when action is not set to be cancellable.</param>
		public virtual void InterruptCombat(bool force = false)
		{
			CancelAttack(force);
		}
		#endregion Exposed Functions

		#region Attack Functions

		private void SetAttackData()
		{
			elapsedAttackTime = 0;
			if (CAnimHandler)
				CAnimHandler.SetAttackData(CWeapon.GetCurrentAttackData());
		}

		protected virtual void Attack(AttackType type)
		{
			if (!CWeapon) return;

			bool pending = CWeapon.IsAttacking;
			pending &= !CWeapon.AttackAllowed(elapsedAttackTime);
			pending &= CWeapon.AttackPendingAllowed(elapsedAttackTime);

			if (pending) //Start waiting until attack is available.
			{
				attackPending = true;

				if (pendingAttackCo != null)
					StopCoroutine(pendingAttackCo);

				pendingAttackCo = WaitForPendingAttack(type);
				StartCoroutine(pendingAttackCo);
			}
			else //Start attck
			{
				if (currentAttackCo != null)
				{
					StopCoroutine(currentAttackCo);
					Debug.Log("Stopped attack coroutine");
				}

				currentAttackCo = AttackRoutine(type);
				StartCoroutine(currentAttackCo);
			}
		}

		IEnumerator WaitForPendingAttack(AttackType type)
		{
			while(attackPending)
			{
				if (!CWeapon)
				{
					attackPending = false;
				}
				else if (!CWeapon.IsAttacking || CWeapon.AttackAllowed(elapsedAttackTime))
				{
					attackPending = false;
					CancelAttack(true);
					Attack(type);
				}
				yield return null;
			}
		}

		protected virtual void CancelAttack(bool force = false)
		{
			if (CWeapon && (CWeapon.AttackCancellable() || force))
			{
				CWeapon.EndAttacking();

				if (currentAttackCo != null)
					StopCoroutine(currentAttackCo);

			}
			else if (!CWeapon && currentAttackCo != null)
			{
				StopCoroutine(currentAttackCo);
			}
		}

		protected void MoveCharacterByWeapon(float offsetTotal, float t01)
		{
			if (!CWeapon) return;
			if (!CMovement) return;

			float moveOffset = CWeapon.GetMoveDistanceFromCurve(t01) - offsetTotal;

			if (moveOffset < -0.2f)
				CMovement.ExternalMove(transform.forward * moveOffset);

		}

		#endregion Attack Functions

		#region Attack Coroutines

		/// <summary>
		/// The main routine of attacking, goes through phases charge, attack and recovery.
		/// </summary>
		protected IEnumerator AttackRoutine(AttackType type)
		{
			CWeapon.StartAttacking(type);
			SetAttackData();

			yield return null; //Wait for one frame because animator sucks ass (ignores booleans if setting durations in same frame)

			yield return StartCoroutine(AttackCharge());
			yield return StartCoroutine(AttackAttack());
			yield return StartCoroutine(AttackRecovery());

			CWeapon.EndAttacking();

		}

		/// <summary>
		/// First phase of attack, drawing weapon to charge position.
		/// Applies movement curves and tells weapon what is going on.
		/// </summary>
		protected virtual IEnumerator AttackCharge()
		{
			CWeapon.CurrentAttackState = AttackState.charge;
			CAnimHandler.SetChargeStarted(); //Tells animator to start charge animation.

			float t01 = 0;
			float offsetTotal = 0;
			float t = 0;

			bool loop = CWeapon != null;

			while (loop)
			{
				t01 = Mathf.Clamp01(t / CWeapon.GetCurrentActionDuration());

				MoveCharacterByWeapon(offsetTotal, t01); // Offset total needs to be from previous update.
				offsetTotal = CWeapon.GetMoveDistanceFromCurve(t01);

				CMovement.SetMoveSpeedMultiplier(CWeapon.GetMoveSpeedMultiplier(t));
				CMovement.SetRotationSpeedMultiplier(CWeapon.GetRotationSpeedMultiplier(t));

				t += Time.smoothDeltaTime;


				yield return null;
				//Check if while loop can continue. For readability reasons this way
				loop &= CWeapon != null;
				loop &= CWeapon.IsAttacking;
				loop &= t < CWeapon.GetCurrentActionDuration();

			}

			CAnimHandler.SetChargeCancelled(); //Tells animator to stop animation.

		}

		/// <summary>
		/// Second phase of attack, the actual damaging action.
		/// Applies movement curves and tells weapon what is going on.
		/// </summary>
		protected virtual IEnumerator AttackAttack()
		{
			CWeapon.CurrentAttackState = AttackState.attack;
			CAnimHandler.SetAttackStarted(); //Tells animator to start attack animation.

			float t01 = 0;
			float offsetTotal = 0;
			float t = 0;
			bool loop = CWeapon != null;

			while (loop)
			{
				t01 = Mathf.Clamp01(t / CWeapon.GetCurrentActionDuration());

				MoveCharacterByWeapon(offsetTotal, t01);
				offsetTotal = CWeapon.GetMoveDistanceFromCurve(t01);

				CMovement.SetMoveSpeedMultiplier(CWeapon.GetMoveSpeedMultiplier(t));
				CMovement.SetRotationSpeedMultiplier(CWeapon.GetRotationSpeedMultiplier(t));

				t += Time.smoothDeltaTime;

				yield return null;
				//Check if while loop can continue. For readability reasons this way
				loop &= CWeapon != null;
				loop &= CWeapon.IsAttacking;
				loop &= t < CWeapon.GetCurrentActionDuration();
			}

			CAnimHandler.SetAttackCancelled(); //Tells animator to stop animation.

		}


		/// <summary>
		/// Last phase of attack, recovering back to original pose.
		/// Applies movement curves and tells weapon what is going on.
		/// </summary>
		protected virtual IEnumerator AttackRecovery()
		{
			CWeapon.CurrentAttackState = AttackState.recovery;
			CAnimHandler.SetRecoveryStarted(); //Tells animator to start recovery animation.

			float t01 = 0;
			float offsetTotal = 0;
			float t = 0;
			bool loop = CWeapon != null;

			while (loop)
			{
				t01 = Mathf.Clamp01(t / CWeapon.GetCurrentActionDuration());

				MoveCharacterByWeapon(offsetTotal, t01);
				offsetTotal = CWeapon.GetMoveDistanceFromCurve(t01);

				CMovement.SetMoveSpeedMultiplier(CWeapon.GetMoveSpeedMultiplier(t));
				CMovement.SetRotationSpeedMultiplier(CWeapon.GetRotationSpeedMultiplier(t));

				t += Time.smoothDeltaTime;

				yield return null;
				//Check if while loop can continue. For readability reasons this way
				loop &= CWeapon != null;
				loop &= CWeapon.IsAttacking;
				loop &= t < CWeapon.GetCurrentActionDuration();
			}

			CAnimHandler.SetRecoveryCancelled(); //Tells animator to stop animation.
		}
		
		#endregion
	}
}
