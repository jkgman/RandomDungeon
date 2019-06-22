using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITargetable
{
	bool IsTargetable();
	Vector3 GetPosition();
	Transform GetTransform();

}
