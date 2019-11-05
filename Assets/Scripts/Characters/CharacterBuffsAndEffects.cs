using UnityEngine;
using UnityEngine.Experimental.VFX;

namespace Dungeon.Characters
{

	public class CharacterBuffsAndEffects : MonoBehaviour
	{
		#region Variables & References
		[Header("Take damage")]
	
		[SerializeField] private VisualEffect damageVFX = null;
		[SerializeField] private VisualEffect deathVFX = null;

		[Header("Mesh References etc")]
		[SerializeField] private Transform meshVisualsParent = null;

		#endregion Variables & References

		#region Exposed Functions

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

		#endregion Exposed Functions

		#region Exposed Functions - Particles

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

		#endregion Exposed Functions - Particles

	}
}
