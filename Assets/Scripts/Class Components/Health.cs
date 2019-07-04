using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Characters
{
	public class Health
	{
		float maxHealth;
		private float damageStackInterval = 0.1f;
		private float lastTimeDamaged = 0;

		public float CurrentHealth
		{
			get;
			private set;
		}

		public Health(float in_maxHealth, float in_damageStackInterval)
		{
			maxHealth = in_maxHealth;
			CurrentHealth = maxHealth;
			damageStackInterval = in_damageStackInterval;
		}


		public bool IsAlive()
		{
			if (CurrentHealth > 0)
				return true;
			else
				return false;
		}


		public void AddHealth(float value)
		{
			if (value > 0)
			{
				CurrentHealth += value;
				CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);
			}
		}

		public void SubstractHealth(float value)
		{
			if (lastTimeDamaged + damageStackInterval < Time.time)
			{
				CurrentHealth -= Mathf.Abs(value);
				CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);
				Debug.Log("Current Health: " + CurrentHealth);
				lastTimeDamaged = Time.time;
			}

		}
	}
}