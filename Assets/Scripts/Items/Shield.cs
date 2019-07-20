using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dungeon.Items
{
/// <summary>
/// Item that has blocking properties and can absorb damage. WORK IN PROGRESS
/// </summary>
	public class Shield : MonoBehaviour
	{
		[SerializeField] private Collider blockCollider = null;
		//[SerializeField] private bool canParry = true;
		//[SerializeField] private float blockStrength = 1f;

		private Characters.CharacterCombatHandler cCombat;
		private bool isEquipped = false;


		public void Equip(Characters.PlayerCombatHandler in_pCombat)
		{
			cCombat = in_pCombat;
			isEquipped = true;
			blockCollider.isTrigger = true;
		}

		public void UnEquip()
		{
			isEquipped = false;
			cCombat = null;
			blockCollider.isTrigger = false;
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
