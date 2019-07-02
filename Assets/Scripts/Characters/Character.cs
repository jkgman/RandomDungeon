using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterBuffsAndEffects))]
[RequireComponent(typeof(Stats))]
public class Character : MonoBehaviour, ITakeDamage
{

	private Stats _stats;
	public Stats Stats
	{
		get
		{
			if (!_stats)
				_stats = GetComponent<Stats>();

			return _stats;
		}
	}

	private CharacterBuffsAndEffects _effects;
	public CharacterBuffsAndEffects Effects
	{
		get
		{
			if (!_effects)
				_effects = GetComponent<CharacterBuffsAndEffects>();

			return _effects;
		}
	}

	public void Die()
	{
		StartCoroutine(DieRoutine());
	}

	protected virtual IEnumerator DieRoutine()
	{
		yield return null;
	}



	public void TakeDamage(float amount)
	{
		if (!Stats.health.IsAlive())
			return;

		Stats.health.SubstractHealth(amount);

		if (!Stats.health.IsAlive())
		{
			Die();
		}
	}

	public void TakeDamageAtPosition(float amount, Vector3 position)
	{
		if (!Stats.health.IsAlive())
			return;

		Stats.health.SubstractHealth(amount);

		if (Effects)
		{
			Effects.PlayDamageParticles(position);
		}
		

		if (!Stats.health.IsAlive())
		{
			Die();
		}
	}

	public void TakeDamageWithForce(float amount,Vector3 hitForce)
	{
		if (!Stats.health.IsAlive())
			return;

		Stats.health.SubstractHealth(amount);

		if (Effects)
		{
			Effects.PlayDamageParticles(hitForce);
		}

		if (!Stats.health.IsAlive())
		{
			Die();
		}
	}

	public void TakeDamageAtPositionWithForce(float amount, Vector3 position, Vector3 hitForce)
	{
		if (!Stats.health.IsAlive())
			return;

		Stats.health.SubstractHealth(amount);

		if (Effects)
		{
			Effects.PlayDamageParticles(position, hitForce);
		}

		if (!Stats.health.IsAlive())
		{
			Die();
		}
	}
}
