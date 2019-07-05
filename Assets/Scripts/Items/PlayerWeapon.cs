using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Dungeon.Items
{
	public class PlayerWeapon : Weapon
	{
		[System.Serializable]
		private struct AllowedActionsGeneral
		{
			public bool allowAttackDuringCharge;
			public bool allowAttackDuringAttack;
			public bool allowAttackDuringRecovery;
			public bool allowComboDuringCharge;
			public bool allowComboDuringAttack;
			public bool allowComboDuringRecovery;
			public bool allowCancelDuringRecovery;

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





		[SerializeField] private AllowedActionsSpecific actionsDuringTargeting;
		[SerializeField] private AllowedActionsSpecific actionsDuringFree;
		[SerializeField] private AllowedActionsGeneral actionsGeneral;


		public override bool AttackCancellable()
		{
			//Recovery can be skipped by other actions
			return CurrentAttackState == AttackState.recovery && actionsGeneral.allowCancelDuringRecovery && IsAttacking;
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


		public override void StartAttacking(AttackType type)
		{
			CurrentAttackIndex = CanCombo() && IsAttacking ? CurrentAttackIndex + 1 : 0;
			base.StartAttacking(type);
			Debug.Log("Player attack started");
		}
	}
}