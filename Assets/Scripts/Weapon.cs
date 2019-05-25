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
	/// </summary>
	public class Weapon : MonoBehaviour
	{


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
			public Player.AttackAnimationData animData;
		}

		[System.Serializable]
		private struct AllowedActions
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
		[SerializeField] private float stackInterval;
		[SerializeField] private List<AttackData> lightAttacks = new List<AttackData>();
		[SerializeField] private List<AttackData> heavyAttacks = new List<AttackData>();
		[SerializeField] private AllowedActions actionsDuringTargeting;
		[SerializeField] private AllowedActions actionsDuringFree;

		private AttackType _currentAttackType;
		private AttackState _currentAttackState;
		private int attackIndex = 0;

		public float CurrentMoveDistance(float evaluationTime)
		{
			switch (_currentAttackType)
			{
				case AttackType.lightAttack:
				{
					switch (_currentAttackState)
					{
						case AttackState.charge:
							return lightAttacks[attackIndex].chargeMoveCurve.Evaluate(evaluationTime);
						case AttackState.attack:
							return lightAttacks[attackIndex].attackMoveCurve.Evaluate(evaluationTime);
						case AttackState.recovery:
							return lightAttacks[attackIndex].recoveryMoveCurve.Evaluate(evaluationTime);
						default:
							return 0;
					}

				}

				case AttackType.heavyAttack:
				{
					switch (_currentAttackState)
					{
						case AttackState.charge:
							return heavyAttacks[attackIndex].chargeMoveCurve.Evaluate(evaluationTime);
						case AttackState.attack:
							return heavyAttacks[attackIndex].attackMoveCurve.Evaluate(evaluationTime);
						case AttackState.recovery:
							return heavyAttacks[attackIndex].recoveryMoveCurve.Evaluate(evaluationTime);
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
			switch (_currentAttackState)
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
			switch (_currentAttackState)
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

		public AttackType CurrentAttackType
		{
			get{ return _currentAttackType; }
			set{ _currentAttackType = value; }
		}
		public AttackState CurrentAttackState
		{
			get { return _currentAttackState; }
			set { _currentAttackState = value; }
		}

		public bool IsAttacking
		{
			get;
			set;
		}

		//public Player.AttackAnimationData GetAttackAnimationData(AttackType attackType, AttackState attackState)
		//{
		//	AnimationClip clip;
		//	switch (attackType)
		//	{
		//		case AttackType.lightAttack:
		//			clip = lightAttacks
		//	}
		//	Player.AttackAnimationData newData = new Player.AttackAnimationData()
		//	{
						
		//	}
		//}

		public float GetCurrentDamage()
		{
			switch (_currentAttackType)
			{
				case AttackType.lightAttack:
					return lightAttacks[attackIndex].attackDamage;

				case AttackType.heavyAttack:
					return heavyAttacks[attackIndex].attackDamage;

				default:
					return 0;
			}
		}

		public float GetCurrentActionDuration()
		{
			switch (_currentAttackType)
			{
				case AttackType.lightAttack:
				{
					switch (_currentAttackState)
					{
						case AttackState.charge:
							return lightAttacks[attackIndex].chargeDuration;
						case AttackState.attack:
							return lightAttacks[attackIndex].attackDuration;
						case AttackState.recovery:
							return lightAttacks[attackIndex].recoveryDuration;
						default:
							return 0;
					}

				}

				case AttackType.heavyAttack:
				{
					switch (_currentAttackState)
					{
						case AttackState.charge:
							return heavyAttacks[attackIndex].chargeDuration;
						case AttackState.attack:
							return heavyAttacks[attackIndex].attackDuration;
						case AttackState.recovery:
							return heavyAttacks[attackIndex].recoveryDuration;
						default:
							return 0;
					}
				}
				default:
					Debug.LogWarning("Action duration not known, set to 0");
					return 0;
			}
		}

		public float GetChargeDuration(AttackType type, int index = 0)
		{
			switch (type)
			{
				case AttackType.lightAttack:
					return lightAttacks[index].chargeDuration;
				case AttackType.heavyAttack:
					return heavyAttacks[index].chargeDuration;
				default:
					return 0;
			}
		}
		public float GetAttackDuration(AttackType type, int index = 0)
		{
			switch (type)
			{
				case AttackType.lightAttack:
					return lightAttacks[index].attackDuration;
				case AttackType.heavyAttack:
					return heavyAttacks[index].attackDuration;
				default:
					return 0;
			}
		}
		public float GetRecoveryDuration(AttackType type, int index = 0)
		{
			switch (type)
			{
				case AttackType.lightAttack:
					return lightAttacks[index].recoveryDuration;
				case AttackType.heavyAttack:
					return heavyAttacks[index].recoveryDuration;
				default:
					return 0;
			}
		}
		void OnTriggerEnter(Collider other)
		{
			if (!IsAttacking)
				return;

			ITakeDamage dmg = other.GetComponent<ITakeDamage>();

			if (dmg != null)
			{
				Debug.Log("Weapon dealing "+ GetCurrentDamage() + " damage to: " + other.gameObject.name);
				dmg.TakeDamage(GetCurrentDamage());
			}
		}

	}
}
