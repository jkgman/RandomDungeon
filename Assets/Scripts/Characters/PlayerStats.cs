using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour, ITakeDamage
{
	public float maxHealth;
	public Health health;

	public float maxStamina;
	public Stamina stamina;

	void Awake()
	{
		health = new Health(maxHealth);
		stamina = new Stamina(maxStamina);
	}

	public void TakeDamage(float amount)
	{
		health.SubstractHealth(amount);
	}
}
