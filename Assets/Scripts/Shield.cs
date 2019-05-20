using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dungeon.Items
{
	public class Shield : MonoBehaviour, ITakeDamage
	{
		[SerializeField] private Collider blockCollider;
		[SerializeField] private bool canParry = true;
		[SerializeField] private float blockStrength;

		Player.PlayerCombatHandler pCombat;
		bool isEquipped;


		public void Equip(Player.PlayerCombatHandler in_pCombat)
		{
			pCombat = in_pCombat;
			isEquipped = true;
		}

		public void UnEquip()
		{
			isEquipped = false;
			pCombat = null;
		}

		void StartBlocking()
		{
			//Collider enabled
			blockCollider.enabled = true;
			blockCollider.isTrigger = false;
		}

		void StopBlocking()
		{
			//Collider disabled
			blockCollider.isTrigger = true;
			blockCollider.enabled = false;
		}

		public void TakeDamage(float amount)
		{
			//Remove stamina of amount
			//Substract amount from blockStrength and remove health if amount is more than blockStrength
			
		}



	}
}
