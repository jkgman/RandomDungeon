using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IHealth, IStamina, ITakeDamage
{
	private float currentHealth;
	private float maxHealth;
	private bool isAlive;

	private float currentStamina;
	private float maxStamina;


	void Update()
	{
		if (currentHealth == 0 && isAlive)
			Die();
	}

	public void AddHealth(float amount)
	{
		currentHealth += amount;
		currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
	}

	public void SubstractHealth(float amount)
	{
		currentHealth -= amount;
		currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
		
	}
	public void Die()
	{
		isAlive = false;	
	}


	public void AddStamina(float amount)
	{
		currentStamina += amount;
		currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

	}

	public void SubstractStamina(float amount)
	{
		currentStamina -= amount;
		currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
	}

	public float GetCurrentStamina()
	{
		return currentStamina;
	}

	public bool HasEnoughStamina(float amountNeeded)
	{
		return (currentStamina - amountNeeded) > 0;
	}

	public void TakeDamage(float amount)
	{
		SubstractHealth(amount);
	}
}
