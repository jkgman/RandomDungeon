using UnityEngine;
using UnityEngine.VFX;

public class CharacterBuffsAndEffects : MonoBehaviour
{
	[Header("Take damage")]
	
	[SerializeField] private VisualEffect damageVFX;
	[SerializeField] private VisualEffect deathVFX;

	[Header("Mesh References etc")]
	[SerializeField] private Transform meshVisualsParent;

	//Possibly for the future....
	//[Header("Buffs")]
	//List<Renderer> allRenderers;
	//Color iceBuffColor;

	public void PlayDeathParticles()
	{
		if (deathVFX != null)
		{
			if (deathVFX.GetVector3("Directional Force") != null)
				deathVFX.SetVector3("Directional Force", Vector3.zero);
			damageVFX.transform.position = transform.position;
			deathVFX.enabled = true;
			deathVFX.Play();
		}
	}

	public void PlayDamageParticles()
	{
		if (damageVFX != null)
		{
			deathVFX.enabled = true;
			if (damageVFX.GetVector3("Directional Force") != null)
				damageVFX.SetVector3("Directional Force", Vector3.zero);

			damageVFX.transform.position = transform.position;
			deathVFX.Play();
		}
	}
	public void PlayDamageParticles(Vector3 position)
	{
		if (damageVFX != null)
		{
			deathVFX.enabled = true;
			if (damageVFX.GetVector3("Directional Force") != null)
				damageVFX.SetVector3("Directional Force", Vector3.zero);

			damageVFX.transform.position = position;
			damageVFX.Play();
		}
	}
	public void PlayDamageParticles(Vector3 position, Vector3 force)
	{

		if (damageVFX != null)
		{
			deathVFX.enabled = true;
			if (damageVFX.GetVector3("Directional Force") != null)
				damageVFX.SetVector3("Directional Force", force);

			damageVFX.transform.position = position;
			damageVFX.Play();
		}
	}



	public void SetInvisible()
	{
		if (meshVisualsParent)
			meshVisualsParent.gameObject.SetActive(false);
			
	}

	public void SetVisible()
	{
		if (meshVisualsParent)
			meshVisualsParent.gameObject.SetActive(true);

	}


}
