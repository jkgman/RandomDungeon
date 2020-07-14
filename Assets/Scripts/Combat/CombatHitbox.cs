using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RandomDungeon.Combat
{
    [RequireComponent(typeof(Collider))]
    public class CombatHitbox : MonoBehaviour
    {
        [SerializeField]
        private CombatBody parentBody;
        [SerializeField]
        private float damageModifier;

        public void RecieveHit(float damage) {
            Debug.Log("collider hit with " + Mathf.RoundToInt(damage * damageModifier));
            parentBody.AdjustHealth(Mathf.RoundToInt(damage * damageModifier));
        }

        private void OnTriggerEnter(Collider other)
        {
            CombatWeapon weapon = other.GetComponent<CombatWeapon>();
            if(weapon && !parentBody.IsOwnedWeapon(weapon))
            {
                RecieveHit(weapon.Damage);
            }
        }
    }
}