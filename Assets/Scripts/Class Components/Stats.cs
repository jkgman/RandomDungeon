using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Characters
{
	public class Stats : MonoBehaviour
	{
		[SerializeField] private float damageStackInterval = 0.1f;

		public float maxHealth;
		public Health health;

		public float maxStamina;
		public Stamina stamina;

		private Character Character
		{
			get{ return GetComponent<Character>(); }
		}

		private CharacterBuffsAndEffects Effects
		{
			get{ return GetComponent<CharacterBuffsAndEffects>(); }
		}

		private void Awake()
		{
			health = new Health(maxHealth, damageStackInterval);
			stamina = new Stamina(maxStamina);
		}
	}
}
