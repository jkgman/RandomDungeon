using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Dungeon
{

	[RequireComponent(typeof(Stats))]
	public class Damageable : MonoBehaviour, ITakeDamage
	{
		[System.Serializable]
		public class DamageableData
		{
			public Collider collider;
			public float armor;

		}
		

		public UnityEvent damagedEvent = new UnityEvent();
		public List<DamageableData> damageableDataList = new List<DamageableData>();

		private HitData lastHitData;

		public void SetDamageable(bool state)
		{
			foreach(var d in damageableDataList)
			{
				d.collider.enabled = state;
			}
		}

		public void TakeDamage(HitData hit)
		{
			foreach(var data in damageableDataList)
			{
				if (hit.col == data.collider)
				{
					lastHitData = hit;
					float amount = hit.damage - data.armor;
					if (amount > 0)
					{ 
						if (GetComponent<Stats>())
							GetComponent<Stats>().health.SubstractHealth(amount);
						
						damagedEvent.Invoke();
					}
						
				}
			}
		}

		public HitData GetLastHitData()
		{
			return lastHitData;
		}
	}
}
