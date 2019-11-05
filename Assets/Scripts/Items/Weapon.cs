using System;
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

	[System.Serializable]
	public struct HitData
	{
		public Collider col;
		public Vector3 position;
		public float damage;
		public Vector3 force;
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
		public struct AttackData
		{
			public float chargeDuration;                 //Anticipation should be the longest part of attack
			public float attackDuration;                 //Fast part where contact happens
			public float recoveryDuration;              //Added juiciness is given through recovery
			public float attackDamage;
			public AnimationCurve chargeMoveCurve;
			public AnimationCurve attackMoveCurve;
			public AnimationCurve recoveryMoveCurve;
			public AnimatorOverrideController overrides;
			public AllowedActions allowedActions;
		}

		[System.Serializable]
		public class AllowedActions
		{
			[Range(0,1f)]
			public float allowComboInputStartTime;
			[Range(0,1f)]
			public float allowComboInputEndTime;
			[Range(0,1f)]
			public float allowComboStartTime;
			public AnimationCurve moveMultiplierCurve;
			public AnimationCurve rotationMultiplierCurve;

		}



		[Header("General data")]
		[SerializeField] protected bool staggerableDuringAction;
		//[SerializeField] protected AllowedActions actions;
		[SerializeField] protected List<AttackData> lightAttacks = new List<AttackData>();
		[SerializeField] protected List<AttackData> heavyAttacks = new List<AttackData>();


		public AttackState CurrentAttackState
		{
			get;
			set;
		}
		public AttackType CurrentAttackType
		{
			get;
			set;
		}

		public bool IsAttacking
		{
			get;
			protected set;
		}
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
		private float GetRelativeElapsedTime(float currentAttackStateTime)
		{
			float t = currentAttackStateTime;

			switch (CurrentAttackState)
			{
				case AttackState.attack:
					t += GetCurrentAttackData().chargeDuration;
				break;
				case AttackState.recovery:
					t += GetCurrentAttackData().chargeDuration + GetCurrentAttackData().attackDuration;	
				break;

				default:
				break;
			}

			return t / GetAttackCompleteDuration();
		}

		private float GetAttackCompleteDuration()
		{
			float t = 0;
			t += GetCurrentAttackData().chargeDuration;
			t += GetCurrentAttackData().attackDuration;
			t += GetCurrentAttackData().recoveryDuration;

			return t;
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
			switch (CurrentAttackState)
			{
				case AttackState.charge:
					return GetCurrentAttackData().chargeDuration;
				case AttackState.attack:
					return GetCurrentAttackData().attackDuration;
				case AttackState.recovery:
					return GetCurrentAttackData().recoveryDuration;
				default:
					return 0;
			}
			
		}
		public AttackData GetCurrentAttackData()
		{
			switch (CurrentAttackType)
			{
				case AttackType.lightAttack:
					return lightAttacks[CurrentAttackIndex];
				case AttackType.heavyAttack:
					return heavyAttacks[CurrentAttackIndex];
				default:
					return new AttackData();
			}
		}

		private AllowedActions GetCurrentAllowedActions()
		{
			switch (CurrentAttackType)
			{
				case AttackType.lightAttack:
				{
					switch (CurrentAttackState)
					{
						case AttackState.charge:
							return lightAttacks[CurrentAttackIndex].allowedActions;
						case AttackState.attack:
							return lightAttacks[CurrentAttackIndex].allowedActions;
						case AttackState.recovery:
							return lightAttacks[CurrentAttackIndex].allowedActions;
						default:
							return null;
					}

				}

				case AttackType.heavyAttack:
				{
					switch (CurrentAttackState)
					{
						case AttackState.charge:
							return heavyAttacks[CurrentAttackIndex].allowedActions;
						case AttackState.attack:
							return heavyAttacks[CurrentAttackIndex].allowedActions;
						case AttackState.recovery:
							return heavyAttacks[CurrentAttackIndex].allowedActions;
						default:
							return null;
					}
				}
				default:
					return null;
			}
		}
		
		public float GetRotationSpeedMultiplier(float currentAttackStateTime)
		{
			if (GetCurrentAllowedActions() != null)
				return GetCurrentAllowedActions().rotationMultiplierCurve.Evaluate(currentAttackStateTime);
			else
				return 0;

		}
		public float GetMoveSpeedMultiplier(float currentAttackStateTime)
		{
			if (GetCurrentAllowedActions() != null)
				return GetCurrentAllowedActions().moveMultiplierCurve.Evaluate(currentAttackStateTime);
			else
				return 0;
		}

		


		public bool AttackPendingAllowed(float elapsedAttackTime)
		{
			if (!IsAttacking)
				return true;

			float t = elapsedAttackTime / GetAttackCompleteDuration();
			return t > GetCurrentAllowedActions().allowComboInputStartTime;
		}

		public bool AttackAllowed(float elapsedAttackTime)
		{
			if (IsAttacking)
			{
				float t = elapsedAttackTime / GetAttackCompleteDuration();
				if (GetCurrentAllowedActions().allowComboStartTime < t)
					return true;
				else
					return false;
			}
			else
			{
				return true;
			}
			
		}

		protected int CurrentAttackIndex
		{
			get;
			set;
		}

		public virtual void StartAttacking(AttackType type)
		{
			CurrentAttackType = type;
			CurrentAttackState = AttackState.charge;
			IsAttacking = true;
		}
		public virtual void EndAttacking()
		{
			IsAttacking = false;
		}




		private void OnTriggerEnter(Collider other)
		{
			if (!IsAttacking || CurrentAttackState != AttackState.attack)
				return;

			ITakeDamage dmg = other.GetComponentInParent<ITakeDamage>();

			if (dmg != null && other.transform != CurrentEquipper)
			{
				HitData hit = new HitData()
				{
					col = other,
					position = other.ClosestPointOnBounds(transform.position),
					damage = GetCurrentDamage(),
					force = (other.transform.position - CurrentEquipper.position).normalized * GetCurrentDamage() * 0.5f
				};
			
				dmg.TakeDamage(hit);
			}
		}

		private void OnTriggerStay(Collider other)
		{
			if (!IsAttacking || CurrentAttackState != AttackState.attack)
				return;

			ITakeDamage dmg = other.GetComponentInParent<ITakeDamage>();

			if (dmg != null && other.transform != CurrentEquipper)
			{
				HitData hit = new HitData()
				{
					col = other,
					position = other.ClosestPointOnBounds(transform.position),
					damage = GetCurrentDamage(),
					force = (other.transform.position - CurrentEquipper.position).normalized * GetCurrentDamage() * 0.5f
				};

				dmg.TakeDamage(hit);
			}
		}

	}
}
