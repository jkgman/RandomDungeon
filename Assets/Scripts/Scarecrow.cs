﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Characters
{
	public class Scarecrow : MonoBehaviour
	{
		[SerializeField] private Damageable damageable;
		[SerializeField] private CharacterBuffsAndEffects buffs;
		[SerializeField] private Stats stats;


		void Start()
		{
			
			damageable = GetComponent <Damageable> ();
			if (damageable)
				GetComponent<Damageable>().damagedEvent.AddListener(Damaged);
			buffs = GetComponent<CharacterBuffsAndEffects>();
			stats = GetComponent<Stats>();
		}

		void Damaged()
		{
			if (stats)
			{
				if (!stats.health.IsAlive())
				{
					//Died
					if (buffs)
					{
						buffs.PlayDeathParticles();
					}

					StartCoroutine(DieAndRespawn());
		
				}
				else
				{
					//Took damage
					if (buffs)
					{
						if (damageable)
							buffs.PlayDamageParticles(damageable.GetLastHitData().position, damageable.GetLastHitData().force);
						else
							buffs.PlayDamageParticles();
					}
				}
			}
		}

		private IEnumerator DieAndRespawn()
		{
			if (GetComponent<CharacterBuffsAndEffects>())
			{
				GetComponent<CharacterBuffsAndEffects>().SetInvisible();
			}
			
			if (GetComponent<Damageable>())
			{
				GetComponent<Damageable>().SetDamageable(false);
			}
			if (GetComponent<Targetable>())
			{
				GetComponent<Targetable>().SetTargetable(false);
			}

			yield return new WaitForSeconds(5f);

			if (GetComponent<CharacterBuffsAndEffects>())
			{
				GetComponent<CharacterBuffsAndEffects>().SetVisible();
			}
			if (GetComponent<Damageable>())
			{
				GetComponent<Damageable>().SetDamageable(true);
			}
			if (GetComponent<Targetable>())
			{
				GetComponent<Targetable>().SetTargetable(true);
			}

			if (GetComponent<Stats>())
			{
				GetComponent<Stats>().Reset();
			}
		}
	}
}
