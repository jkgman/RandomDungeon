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
    /// Weapon determines what attack animations will be used.
	/// </summary>
    /// 
    /// 
    /// Remember to create an override animation controller which overrides all three attack states for every new weapon/attacktype
    /// Same overridecontroller can be used for identical animations. (animation speeds can be still assigned separately)
    
	public class Weapon : MonoBehaviour
	{

        //All attacks have their own data.
		[System.Serializable]
		public struct AttackData
		{
			public float chargeDuration;                //Anticipation should be long
			public float attackDuration;                //Fast part where contact happens
			public float recoveryDuration;              //Recovery should be long
			public float attackDamage;
			public AnimationCurve chargeMoveCurve;      //Forced move during charge
			public AnimationCurve attackMoveCurve;      //Forced move during attack
			public AnimationCurve recoveryMoveCurve;    //Forced move during recovery
			public AnimatorOverrideController overrides;//Animations that will be used for this attack
			public AllowedActions allowedActions;       //List of parameters for what actions are allowed during this attack
		}

		[System.Serializable]
		public class AllowedActions
		{
			[Range(0,1f)]
			public float allowComboInputStartTime;      //Start time (0-1 scaled to attack length) of when combo input will be accepted
			[Range(0,1f)]
			public float allowComboInputEndTime;        //Start time (0-1 scaled to attack length) of when combo input will NO LONGER be accepted
            [Range(0,1f)]
			public float allowComboStartTime;           //Earliest time (0-1 scaled to attack length) when combo attack can override current attack
            public AnimationCurve moveMultiplierCurve;  //Multiplies voluntary movement speed relative to current attack duration (0-1 like above)
			public AnimationCurve rotationMultiplierCurve; //Multiplies voluntary rotation speed relative to current attack duration (0-1 like above)

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

        /// <summary>
        /// Uses animationcurve data to evaluate how much forced movement should have been applied by evaluation time
        /// </summary>
        /// <param name="evaluationTime">Current time of attack in value 0-1, scaled to attack duration.</param>
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
        /// <summary>
        /// If only current attack state's elapsed time is known, this returns the complete elapsed time scaled to 0-1
        /// </summary>
        /// <param name="currentAttackStateTime">Elapsed time of current attack state</param>
        /// <returns></returns>
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

        /// <summary>
        /// Sum of all three attack state durations
        /// </summary>
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
                    //Uhh dunno what else to return...
					return new AttackData();
			}
		}
        /// <summary>
        /// Gets AllowedActions object according to current attack type and attack state
        /// </summary>
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
		
        /// <summary>
        /// Used for multiplying all voluntary rotation during attack
        /// </summary>
		public float GetRotationSpeedMultiplier(float currentAttackStateTime)
		{
			if (GetCurrentAllowedActions() != null)
				return GetCurrentAllowedActions().rotationMultiplierCurve.Evaluate(currentAttackStateTime);
			else
				return 0;

		}
        /// <summary>
        /// Used for multiplying all voluntary movement during attack
        /// </summary>
        public float GetMoveSpeedMultiplier(float currentAttackStateTime)
		{
			if (GetCurrentAllowedActions() != null)
				return GetCurrentAllowedActions().moveMultiplierCurve.Evaluate(currentAttackStateTime);
			else
				return 0;
		}

		

        /// <summary>
        /// Checks if combo inputs should be accepted.
        /// </summary>
		public bool ComboPendingAllowed(float elapsedAttackTime)
		{
			if (!IsAttacking)
				return true;

			float t = elapsedAttackTime / GetAttackCompleteDuration();
			return t > GetCurrentAllowedActions().allowComboInputStartTime;
		}

        /// <summary>
        /// Checks if weapon is ready to accept an attack
        /// </summary>
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

        /// <summary>
        /// Enables attacking and sets current attack type.
        /// </summary>
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



        /// <summary>
        /// Weapon uses only trigger checks for damaging others.
        /// Checks if attack is valid and if other collider has ITakeDamage interface to deal damage.
        /// </summary>
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

        /// <summary>
        /// Weapon uses only trigger checks for damaging others.
        /// Checks if attack is valid and if other collider has ITakeDamage interface to deal damage.
        /// </summary>
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
