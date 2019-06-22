using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class CharacterBuffsAndEffects : MonoBehaviour
{
	[Header("Take damage")]
	
	[SerializeField] private VisualEffect damageVFX;
	[SerializeField] private VisualEffect deathVFX;


	//Possibly for the future....
	//[Header("Buffs")]
	//List<Renderer> allRenderers;
	//Color iceBuffColor;


	public void PlayDeathParticles()
	{
		if (deathVFX != null)
		{
			deathVFX.transform.parent = null;
			deathVFX.Play();
		}
	}

	public void PlayDamageParticles(Vector3 position)
	{
		if (damageVFX != null)
		{
			damageVFX.transform.position = position;
			damageVFX.Play();
		}
	}
	public void PlayDamageParticles()
	{
		if (damageVFX != null)
		{
			damageVFX.transform.position = transform.position;
			deathVFX.Play();
		}
	}


	public void SetInvisible()
	{
		foreach(var t in GetComponentsInChildren<Transform>())
		{
			if (t != deathVFX.transform && t != damageVFX.transform)
				t.gameObject.SetActive(false);
		}
	}




}
