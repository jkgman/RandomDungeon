using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stamina
{
	private float maxStamina;
	private float currentStamina;

	public Stamina (float in_maxStamina)
	{
		maxStamina = in_maxStamina;
		currentStamina = maxStamina;
	}


	public void AddStamina(float amount)
	{
		if (amount > 0)
		{
			currentStamina += amount;
			currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
		}

	}

	public void SubstractStamina(float amount)
	{
		currentStamina -= Mathf.Abs(amount);
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
}
