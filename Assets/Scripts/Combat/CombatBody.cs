using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RandomDungeon.Combat
{
    public class CombatBody : MonoBehaviour
    {
        public Action DeathEvent;

        private bool invincibility = false;
        public bool Invincibility { get => invincibility; set => value = invincibility; }

        [SerializeField]
        private int maxHealth;
        private int currentHealth;

        [SerializeField]
        List<CombatWeapon> weapons = new List<CombatWeapon>();

        private void Start()
        {
            currentHealth = maxHealth;
        }

        public void SetHealth(int newHealth)
        {
            currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        }

        public void AdjustHealth(int adjustment)
        {
            if(Invincibility && adjustment < 0)
                return;
            Debug.Log("body adjusted for " + Mathf.Clamp(currentHealth + Mathf.RoundToInt(adjustment), 0, maxHealth));
            currentHealth = Mathf.Clamp(currentHealth + Mathf.RoundToInt(adjustment), 0, maxHealth);
            Debug.Log("new health " +currentHealth);
            if (currentHealth <= 0)
            {
                DeathEvent.Invoke();
            }
        }

        public bool IsOwnedWeapon(CombatWeapon weapon) {
            foreach(var ownedWeapon in weapons)
            {
                if(weapon == ownedWeapon)
                    return true;
            }
            return false;
        }
    }
}