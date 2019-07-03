using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Characters
{
	public class Health
	{
		float maxHealth;

		public float CurrentHealth
		{
			get;
			private set;
		}

		public Health(float in_maxHealth)
		{
			maxHealth = in_maxHealth;
			CurrentHealth = maxHealth;
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
			CurrentHealth -= Mathf.Abs(value);
			CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

		}
	}
}