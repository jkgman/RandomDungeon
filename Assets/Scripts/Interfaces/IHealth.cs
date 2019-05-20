using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealth
{
	void SubstractHealth(float amount);
	void AddHealth(float amount);
	void Die();
}
