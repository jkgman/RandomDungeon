using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dungeon
{
	public enum AttackType
	{
		lightAttack,
		heavyAttack
		//lightChargedAttack,
		//heavyChargedAttack
	}
	public enum AttackState
	{
		charge,
		attack,
		recovery
	}
}

namespace Dungeon.Items
{

	/// <summary>
	/// Attached to weapons to give them individual properties.
	/// Currently cramped up in one class, it will be split into different types of weapons.
	/// Enemies will be using weapon class as well in the future.
	/// </summary>
	public class Weapon : MonoBehaviour
	{
		[System.Serializable]
		public struct AttackAnimationData
		{
			public AnimationClip chargeClip;
			public AnimationClip attackClip;
			public AnimationClip recoveryClip;
		}

		[System.Serializable]
		protected struct AttackData
		{
			public float chargeDuration;                 //Anticipation should be the longest part of attack
			public float attackDuration;                 //Fast part where contact happens
			public float recoveryDuration;              //Added juiciness is given through recovery
			public float attackDamage;
			public AnimationCurve chargeMoveCurve;
			public AnimationCurve attackMoveCurve;
			public AnimationCurve recoveryMoveCurve;
			public AttackAnimationData animData;
		}


		[Header("General data")]
		[SerializeField] protected bool isEquipped;
		[SerializeField] protected bool staggerableDuringAction;
		[SerializeField] protected List<AttackData> lightAttacks = new List<AttackData>();
		[SerializeField] protected List<AttackData> heavyAttacks = new List<AttackData>();


		public Transform CurrentEquipper
		{
			get;
			set;
		}

		public virtual bool AttackCancellable()
		{
			return false;
		}

		public float GetMoveDistanceFromCurve(float evaluationTime)
		{
			switch (CurrentAttackType)
			{
				case AttackType.lightAttack:
				{
					switch (CurrentAttackState)
					{
						case AttackState.charge:
							return lightAttacks[CurrentAttackIndex].chargeMoveCurve.Evaluate(evaluationTime);
						case AttackState.attack:
							return lightAttacks[CurrentAttackIndex].attackMoveCurve.Evaluate(evaluationTime);
						case AttackState.recovery:
							return lightAttacks[CurrentAttackIndex].recoveryMoveCurve.Evaluate(evaluationTime);
						default:
							return 0;
					}

				}

				case AttackType.heavyAttack:
				{
					switch (CurrentAttackState)
					{
						case AttackState.charge:
							return heavyAttacks[CurrentAttackIndex].chargeMoveCurve.Evaluate(evaluationTime);
						case AttackState.attack:
							return heavyAttacks[CurrentAttackIndex].attackMoveCurve.Evaluate(evaluationTime);
						case AttackState.recovery:
							return heavyAttacks[CurrentAttackIndex].recoveryMoveCurve.Evaluate(evaluationTime);
						default:
							return 0;
					}
				}
				default:
					return 0;
			}
		}


		public AttackType CurrentAttackType
		{
			get;
			set;
		}
		public AttackState CurrentAttackState
		{
			get;
			set;
		}
		public int CurrentAttackIndex
		{
			get;
			set;
		}

		public bool IsAttacking
		{
			get;
			private set;
		}

		public virtual void StartAttacking(AttackType type)
		{
			CurrentAttackType = type;
			CurrentAttackState = AttackState.charge;
			IsAttacking = true;
		}

		public void EndAttacking()
		{
			IsAttacking = false;
		}

		public float GetCurrentDamage()
		{
			switch (CurrentAttackType)
			{
				case AttackType.lightAttack:
					return lightAttacks[CurrentAttackIndex].attackDamage;

				case AttackType.heavyAttack:
					return heavyAttacks[CurrentAttackIndex].attackDamage;

				default:
					return 0;
			}
		}

		public float GetCurrentActionDuration()
		{
			switch (CurrentAttackType)
			{
				case AttackType.lightAttack:
				{
					switch (CurrentAttackState)
					{
						case AttackState.charge:
							return lightAttacks[CurrentAttackIndex].chargeDuration;
						case AttackState.attack:
							return lightAttacks[CurrentAttackIndex].attackDuration;
						case AttackState.recovery:
							return lightAttacks[CurrentAttackIndex].recoveryDuration;
						default:
							return 0;
					}

				}

				case AttackType.heavyAttack:
				{
					switch (CurrentAttackState)
					{
						case AttackState.charge:
							return heavyAttacks[CurrentAttackIndex].chargeDuration;
						case AttackState.attack:
							return heavyAttacks[CurrentAttackIndex].attackDuration;
						case AttackState.recovery:
							return heavyAttacks[CurrentAttackIndex].recoveryDuration;
						default:
							return 0;
					}
				}
				default:
					Debug.LogWarning("Action duration not known, set to 0");
					return 0;
			}
		}

		public float GetChargeDuration()
		{
			switch (CurrentAttackType)
			{
				case AttackType.lightAttack:
					return lightAttacks[CurrentAttackIndex].chargeDuration;
				case AttackType.heavyAttack:
					return heavyAttacks[CurrentAttackIndex].chargeDuration;
				default:
					return 0;
			}
		}
		public float GetAttackDuration()
		{
			switch (CurrentAttackType)
			{
				case AttackType.lightAttack:
					return lightAttacks[CurrentAttackIndex].attackDuration;
				case AttackType.heavyAttack:
					return heavyAttacks[CurrentAttackIndex].attackDuration;
				default:
					return 0;
			}
		}
		public float GetRecoveryDuration()
		{
			switch (CurrentAttackType)
			{
				case AttackType.lightAttack:
					return lightAttacks[CurrentAttackIndex].recoveryDuration;
				case AttackType.heavyAttack:
					return heavyAttacks[CurrentAttackIndex].recoveryDuration;
				default:
					return 0;
			}
		}


		private void OnTriggerEnter(Collider other)
		{
			if (!IsAttacking || CurrentAttackState != AttackState.attack)
				return;

			ITakeDamage dmg = other.GetComponentInParent<ITakeDamage>();

			if (dmg != null && other.transform != CurrentEquipper)
			{
				//Position and force are placeholder tests.
				Vector3 position = other.ClosestPointOnBounds(transform.position);
				Vector3 force = (other.transform.position - CurrentEquipper.position).normalized * GetCurrentDamage()*0.5f;
				dmg.TakeDamageAtPositionWithForce(GetCurrentDamage(), position, force);
			}
		}

		private void OnTriggerStay(Collider other)
		{
			if (!IsAttacking || CurrentAttackState != AttackState.attack)
				return;

			ITakeDamage dmg = other.GetComponentInParent<ITakeDamage>();

			if (dmg != null && dmg.GetTransform() != CurrentEquipper)
			{
				Debug.Log("Attack trigger, dmg: " + dmg + "CurrentEquipper: " + CurrentEquipper.name);
				//Position and force are placeholder tests.
				Vector3 position = other.ClosestPointOnBounds(transform.position);
				Vector3 force = (other.transform.position - transform.parent.position).normalized * GetCurrentDamage() *0.5f;
				dmg.TakeDamageAtPositionWithForce(GetCurrentDamage(), position, force);
			}
		}

	}
}
