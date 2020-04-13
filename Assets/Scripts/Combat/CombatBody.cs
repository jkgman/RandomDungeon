using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace INDEV.Combat
{
    public class CombatBody : MonoBehaviour
    {

        public Action DeathEvent;

        private int MaxHealth;
        private int CurrentHealth;

        public void AdjustHealth(int adjustment)
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth + adjustment, 0, MaxHealth);
            if (CurrentHealth == 0)
            {
                DeathEvent.Invoke();
            }
        }
        public async void LightAttack() { }
        public async void HeavyAttack() { }
        public async void Block() { }
    }
}