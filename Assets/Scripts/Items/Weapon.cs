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
		private struct AttackData
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

		[System.Serializable]
		private struct AllowedActionsGeneral
		{
			public bool allowAttackDuringCharge;
			public bool allowAttackDuringAttack;
			public bool allowAttackDuringRecovery;
			public bool allowComboDuringCharge;
			public bool allowComboDuringAttack;
			public bool allowComboDuringRecovery;

		}

		[System.Serializable]
		private struct AllowedActionsSpecific
		{
			public bool rotatableDuringCharge;
			public bool rotatableDuringAttack;
			public bool rotatableDuringRecovery;
			public bool movableDuringCharge;
			public bool movableDuringAttack;
			public bool movableDuringRecovery;
		}


		[Header("General data")]
		[SerializeField] private bool isEquipped;
		[SerializeField] private bool staggerableDuringAction;
		[SerializeField] private List<AttackData> lightAttacks = new List<AttackData>();
		[SerializeField] private List<AttackData> heavyAttacks = new List<AttackData>();

		[SerializeField] private AllowedActionsSpecific actionsDuringTargeting;
		[SerializeField] private AllowedActionsSpecific actionsDuringFree;
		[SerializeField] private AllowedActionsGeneral actionsGeneral;

		public Transform CurrentEquipper
		{
			get;
			set;
		}

		public float CurrentMoveDistance(float evaluationTime)
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

		public bool CanRotate(bool hasTarget)
		{
			switch (CurrentAttackState)
			{
				case AttackState.charge:
					if (hasTarget)
						return actionsDuringTargeting.rotatableDuringCharge;
					else
						return actionsDuringFree.rotatableDuringCharge;

				case AttackState.attack:
					if (hasTarget)
						return actionsDuringTargeting.rotatableDuringAttack;
					else
						return actionsDuringFree.rotatableDuringAttack;

				case AttackState.recovery:
					if (hasTarget)
						return actionsDuringTargeting.rotatableDuringRecovery;
					else
						return actionsDuringFree.rotatableDuringRecovery;

				default:
					return true;
			}

		}

		public bool CanMove(bool hasTarget)
		{
			switch (CurrentAttackState)
			{
				case AttackState.charge:
					if (hasTarget)
						return actionsDuringTargeting.movableDuringCharge;
					else
						return actionsDuringFree.movableDuringCharge;

				case AttackState.attack:
					if (hasTarget)
						return actionsDuringTargeting.movableDuringAttack;
					else
						return actionsDuringFree.movableDuringAttack;

				case AttackState.recovery:
					if (hasTarget)
						return actionsDuringTargeting.movableDuringRecovery;
					else
						return actionsDuringFree.movableDuringRecovery;
				default:
				return true;
			}
		}

		public bool CanAttack(bool hasTarget)
		{
			if (IsAttacking)
			{
				switch (CurrentAttackState)
			{
				case AttackState.charge:
					return hasTarget ? actionsGeneral.allowAttackDuringCharge : actionsGeneral.allowAttackDuringCharge;
					
				case AttackState.attack:
					return hasTarget ? actionsGeneral.allowAttackDuringAttack : actionsGeneral.allowAttackDuringAttack;
					
				case AttackState.recovery:
					return hasTarget ? actionsGeneral.allowAttackDuringRecovery : actionsGeneral.allowAttackDuringRecovery;
					
				default:
					return true;
			}
			}
			else
			{
				return true;
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

		public void StartAttacking(AttackType type)
		{
			CurrentAttackType = type;
			CurrentAttackIndex = CanCombo() && IsAttacking ? CurrentAttackIndex + 1 : 0;
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

		private bool CanCombo()
		{
			bool rightTiming = false; //check against current allowed actions.
			bool hasCombo = false; // check if attacktype has more attacks on list.

			if (CurrentAttackType == AttackType.lightAttack)
				hasCombo = CurrentAttackIndex < lightAttacks.Count - 1;
			if (CurrentAttackType == AttackType.heavyAttack)
				hasCombo = CurrentAttackIndex < heavyAttacks.Count - 1;

			switch (CurrentAttackState)
			{
				case AttackState.charge:
					rightTiming = actionsGeneral.allowComboDuringCharge;
					break;
				case AttackState.attack:
					rightTiming = actionsGeneral.allowComboDuringAttack;
					break;
				case AttackState.recovery:
					rightTiming = actionsGeneral.allowComboDuringRecovery;
					break;
				default:
					break;
			}

			return rightTiming && hasCombo;
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
				Vector3 force = (other.transform.position - CurrentEquipper.position).normalized * GetCurrentDamage()*2f;
				dmg.TakeDamageAtPositionWithForce(GetCurrentDamage(), position, force);
			}
		}

		private void OnTriggerStay(Collider other)
		{
			if (!IsAttacking || CurrentAttackState != AttackState.attack)
				return;

			ITakeDamage dmg = other.GetComponentInParent<ITakeDamage>();

			if (dmg != null && other.transform != CurrentEquipper)
			{
				//Position and force are placeholder tests.
				Vector3 position = other.ClosestPointOnBounds(transform.position);
				Vector3 force = (other.transform.position - transform.parent.position).normalized * GetCurrentDamage() *0.5f;
				dmg.TakeDamageAtPositionWithForce(GetCurrentDamage(), position, force);
			}
		}

	}
}
