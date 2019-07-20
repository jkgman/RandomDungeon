using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dungeon.Characters
{ 
	[RequireComponent(typeof(CharacterBuffsAndEffects))]
	[RequireComponent(typeof(Stats))]
	public class Character : MonoBehaviour
	{
		#region Structs enums classes
		
		[System.Serializable]
		protected struct CharacterCollider
		{
			public Collider col;
			public CharacterColliderType colType;
		}
		protected enum CharacterColliderType
		{
			torso,
			arm,
			leg,
			head
		}
		
		#endregion Structs enums classes

		#region Variables & References

		//_______ Start of Exposed variables
		[SerializeField, Tooltip("All Characted colliders and their body positions.")]
		protected List<CharacterCollider> colliders;
		//_______ End of Exposed variables


		//_______ Start of Hidden variables
		protected bool isActive = true;
		protected bool dieRoutineStarted = false;
		//_______ End of Hidden variables



		//_______ Start of Class References
		private Stats _stats;
		protected Stats Stats
		{
			get
			{
				if (!_stats) _stats = GetComponent<Stats>();
				return _stats;
			}
		}
		private CharacterBuffsAndEffects _effects;
		protected CharacterBuffsAndEffects Effects
		{
			get{
				if (_effects)
					_effects = GetComponent<CharacterBuffsAndEffects>();

				return _effects;
			}
		}
		private RagdollScript _ragdoll;
		protected RagdollScript Ragdoll
		{
			get{
				if (!_ragdoll)
					_ragdoll = GetComponent<RagdollScript>();

				return _ragdoll;
			}
		}
		//_______ End of Class References


		#endregion Variables & References

		#region Getters & Setters

		/// <summary>
		/// Gets current player position. If ragdolled, gets position from hip location.
		/// </summary>
		public Vector3 GetPhysicalPosition()
		{
			if (Ragdoll && Ragdoll.IsRagdolling)
			{
				return Ragdoll.GetPhysicalPosition();
			}
			else
			{
				return transform.position;
			}
		}

		#endregion Getters & Setters

		#region Initialization & Updates

		protected virtual void Update()
		{
			CheckDeath();
		}

		private void CheckDeath()
		{
			//IF not dying but should be, start dying.
			if (!Stats.health.IsAlive() && !dieRoutineStarted)
				StartCoroutine(DieRoutine());

		}

		#endregion Initialization & Updates

		#region Coroutines
		/// <summary>
		/// Plays effects, disables colliders etc in this routine. Destroys the gameObject if needed.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator DieRoutine()
		{
			if (Effects)
			{
				Effects.PlayDeathParticles();
			}
			DisableColliders();

			yield return new WaitForSeconds(2f);

			Destroy(gameObject);
		}

		#endregion Coroutines

		#region Helper Functions

		protected void EnableColliders()
		{
			for (int i = 0; i < colliders.Count; i++)
			{
				colliders[i].col.enabled = true;
			}
		}
		protected void DisableColliders()
		{
			for (int i = 0; i < colliders.Count; i++)
			{
				colliders[i].col.enabled = false;
			}
		}

		#endregion Helper Functions
	}
}