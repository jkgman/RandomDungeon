using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Dungeon
{
    [System.Serializable]
    public struct ArmorCollider
    {
        public Collider col;
        public float baseArmor;

    }


    [RequireComponent(typeof(Stats))]
	public class CollisionDamageHandler : MonoBehaviour, ITakeDamage
	{
        public List<ArmorCollider> colliders;
		
        [HideInInspector]
		public UnityEvent damagedEvent = new UnityEvent();

		private HitData lastHitData;


        /// <summary>
        /// Set all damageable colliders active/inactive
        /// </summary>
		public void SetColliders(bool state)
		{
			foreach(var d in colliders)
			{
				d.col.enabled = state;
			}
		}

		public void TakeDamage(HitData hit)
		{
			foreach(var colData in colliders)
			{
				if (hit.col == colData.col)
				{
					lastHitData = hit;
					float amount = hit.damage - colData.baseArmor;
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
