using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dungeon.Items
{
	public enum WeaponType
	{
		lightSword
		//heavySword,
		//axe,
		//hammer,
		//club
	}
	public class Weapon : MonoBehaviour
	{
		[SerializeField] private bool canBeCharged;
		[SerializeField] private bool canHeavyAttack;

		[SerializeField] private float chargeDuration;
		[SerializeField] private float attackDuration;
		[SerializeField] private float attackDamage;
		[SerializeField] private WeaponType type;
		
		public WeaponType GetWeaponType()
		{
			return type;
		}
		public float GetAttackDuration()
		{
			return attackDuration;
		}

	}
}
