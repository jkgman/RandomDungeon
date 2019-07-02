using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITakeDamage
{
	void TakeDamage(float amount);
	void TakeDamageAtPosition(float amount, Vector3 position);
	void TakeDamageWithForce(float amount, Vector3 hitForce);
	void TakeDamageAtPositionWithForce(float amount, Vector3 position, Vector3 hitForce);
}
