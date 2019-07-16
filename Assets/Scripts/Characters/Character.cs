using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dungeon.Characters
{ 
	[RequireComponent(typeof(CharacterBuffsAndEffects))]
	[RequireComponent(typeof(Stats))]
	public class Character : MonoBehaviour, ITakeDamage
	{
		private enum CharacterColliderType
		{
			torso,
			arm,
			leg,
			head
		}

		[System.Serializable]
		private struct CharacterCollider
		{
			public Collider col;
			public CharacterColliderType colType;
		}


		protected bool isActive = true;
		protected bool dieRoutineStarted = false;



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
		private CharacterAnimationHandler _animationHandler;
		public CharacterAnimationHandler AnimationHandler
		{
			get
			{
				if (!_animationHandler)
					_animationHandler = GetComponent<CharacterAnimationHandler>();

				return _animationHandler;
			}
		}

		private CharacterController _characterController;
		public CharacterController CharacterController
		{
			get
			{
				if (!_characterController)
					_characterController = GetComponent<CharacterController>();

				return _characterController;
			}
		}

		private CharacterCombatHandler _characterCombatHandler;
		public CharacterCombatHandler CharacterCombatHandler
		{
			get
			{
				if (!_characterController)
					_characterCombatHandler = GetComponent<CharacterCombatHandler>();

				return _characterCombatHandler;
			}
		}

		[SerializeField]
		List<CharacterCollider> colliders;
		[SerializeField]
		bool debugRagdoll;
		public RagdollScript Ragdoll
		{
			get{ return GetComponent<RagdollScript>(); }
		}

		protected virtual void Update()
		{
			if (!Stats.health.IsAlive() && !dieRoutineStarted)
				StartCoroutine(DieRoutine());

			if (transform.position.y < -200f)
				Stats.health.SubstractHealth(10000f);

			DebugRagdoll();

		}

		private void DebugRagdoll()
		{
			if (debugRagdoll)
			{
				if (Ragdoll.ragdollState != RagdollScript.RagdollState.ragdolled)
				{
					Ragdoll.StartRagdoll();
					DisableColliders();
				}

			}
			else
			{
				if (Ragdoll.ragdollState == RagdollScript.RagdollState.ragdolled)
				{
					if (CharacterController.IsGrounded())
					{
						Ragdoll.EndRagdoll();
						EnableColliders();
						debugRagdoll = false;
					}
				}
			}
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
		}


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

		public Transform GetTransform()
		{
			return transform;
		}




	}
}