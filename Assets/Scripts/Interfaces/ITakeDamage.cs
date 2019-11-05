using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon
{

	public interface ITakeDamage
	{
		void TakeDamage(HitData hit);
	}
}
