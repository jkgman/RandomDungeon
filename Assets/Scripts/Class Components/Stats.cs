using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon
{
	public class Stats : MonoBehaviour
	{
		[SerializeField] private float damageStackInterval = 0.1f;

		public float maxHealth;
		public Health health;

		public float maxStamina;
		public Stamina stamina;
		

		private void Awake()
		{
			health = new Health(maxHealth, damageStackInterval);
			stamina = new Stamina(maxStamina);
		}

		public void Reset()
		{
			health.Reset();
			stamina.Reset();
		}
	}
}
