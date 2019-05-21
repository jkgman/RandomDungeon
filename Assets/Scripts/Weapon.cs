using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dungeon.Items
{
	public enum WeaponType
	{
		lightSword
		//heavySword,
		//axe,
		//hammer,
		//club
	}

	/// <summary>
	/// Attached to weapons to give them individual properties.
	/// </summary>
	public class Weapon : MonoBehaviour
	{
		[SerializeField] private WeaponType type;
		[SerializeField] private float chargeDuration;
		[SerializeField] private float attackDuration;
		[SerializeField] private float attackDamage;
		[SerializeField] private AnimationCurve forwardMoveCurve;

		//[SerializeField] private bool canBeCharged;
		//[SerializeField] private bool canHeavyAttack;
		[SerializeField] private bool allowStaggerDuringAttack;
		[SerializeField] private bool rotateDuringTargetAttack;
		[SerializeField] private bool rotateDuringTargetDamaging;
		[SerializeField] private bool rotateDuringFreeAttack;
		[SerializeField] private bool rotateDuringFreeDamaging;
		[SerializeField] private bool moveDuringFreeAttack;
		[SerializeField] private bool moveDuringFreeDamaging;
		[SerializeField] private bool moveDuringTargetAttack;
		[SerializeField] private bool moveDuringTargetDamaging;

		//During hit time weapon is dealing damage and attack combo is allowed.
		[SerializeField, Range(0, 1f)] private float damageStart = 0.4f;
		[SerializeField, Range(0, 1f)] private float damageEnd = 0.85f;

		void OnValidate()
		{
			if (damageStart > damageEnd)
				damageEnd = damageStart;
		}
		
		public WeaponType GetWeaponType()
		{
			return type;
		}

		public float CurrentAttackMoveSpeed(float evaluationTime)
		{
			return forwardMoveCurve.Evaluate(evaluationTime);
		}


		public bool CanRotate(bool hasTarget)
		{
			if (IsDamaging)
			{
				return hasTarget ? rotateDuringTargetDamaging : rotateDuringFreeDamaging;
			}
			if (IsAttacking)
			{
				return hasTarget ? rotateDuringTargetAttack : rotateDuringFreeAttack;
			}

			return true;
		}

		public bool CanMove(bool hasTarget)
		{
			if (IsDamaging)
			{
				return hasTarget ? moveDuringTargetDamaging : moveDuringFreeDamaging;
			}
			if (IsAttacking)
			{
				return hasTarget ? moveDuringTargetAttack : moveDuringFreeAttack;
			}

			return true;
		}




		public float GetAttackDuration()
		{
			return attackDuration;
		}
		public float GetHitStart()
		{
			return damageStart;
		}
		public float GetHitEnd()
		{
			return damageEnd;
		}
		public bool IsDamaging
		{
			get;
			set;
		}
		public bool IsAttacking
		{
			get;
			set;
		}

		void OnTriggerEnter(Collider other)
		{
			if (!IsDamaging)
				return;

			ITakeDamage dmg = other.GetComponent<ITakeDamage>();

			if (dmg != null)
			{
				Debug.Log("Weapon dealing damage to: " + other.gameObject.name);
				dmg.TakeDamage(attackDamage);
			}
		}

	}
}
