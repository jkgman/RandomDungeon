using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Dungeon.Items
{
	public class PlayerWeapon : Weapon
	{



		public override bool AttackCancellable()
		{
			//Recovery can be skipped by other actions
			return CurrentAttackState == AttackState.recovery && actionsGeneral.allowCancelDuringRecovery && IsAttacking;
		}




		public override void StartAttacking(AttackType type)
		{
			CurrentAttackIndex = CanCombo() && IsAttacking ? CurrentAttackIndex + 1 : 0;
			base.StartAttacking(type);
			Debug.Log("Player attack started");
		}
	}
}