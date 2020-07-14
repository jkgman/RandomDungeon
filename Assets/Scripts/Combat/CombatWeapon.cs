using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RandomDungeon.Combat
{
    [RequireComponent(typeof(Collider))]
    public class CombatWeapon : MonoBehaviour
    {
        [SerializeField]
        private float damage = 1;
        private Collider combatCollider;

        public float Damage { get => damage; }

        private void OnEnable()
        {
            if(combatCollider == null)
                combatCollider = GetComponent<Collider>();
            combatCollider.enabled = false;
        }

        public void Activate() {
            combatCollider.enabled = true;
        }
        public void DeActivate()
        {
            combatCollider.enabled = false;
        }
    }
}
