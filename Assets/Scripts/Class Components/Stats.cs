﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
	[SerializeField] private float damageStackInterval = 0.1f;
	private float lastTimeDamaged = 0;

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
		health = new Health(maxHealth);
		stamina = new Stamina(maxStamina);
	}
}
