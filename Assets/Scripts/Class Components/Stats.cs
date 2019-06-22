using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour, ITakeDamage
{
	[SerializeField] private float damageStackInterval = 0.1f;
	private float lastTimeDamaged = 0;

	public float maxHealth;
	public Health health;

	public float maxStamina;
	public Stamina stamina;

	private void Awake()
	{
		health = new Health(maxHealth);
		stamina = new Stamina(maxStamina);
	}

	public void TakeDamage(float amount)
	{
		if (damageStackInterval + lastTimeDamaged < Time.time)
		{
			health.SubstractHealth(amount);
			lastTimeDamaged = Time.time;
			Debug.Log("Took damage. Alive: " + health.IsAlive() + ", health: " + health.CurrentHealth);
		}
	}
}
