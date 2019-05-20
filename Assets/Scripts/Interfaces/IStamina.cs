using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStamina
{
	void SubstractStamina(float amount);
	void AddStamina(float amount);
	float GetCurrentStamina();
	bool HasEnoughStamina(float amountNeeded);
}
