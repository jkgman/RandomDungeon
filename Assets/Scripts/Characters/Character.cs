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

		#region Variables & References

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
		private CharacterBuffsAndEffects _buffsEffects;
		protected CharacterBuffsAndEffects BuffsEffects
		{
			get{
				if (!_buffsEffects)
					_buffsEffects = GetComponent<CharacterBuffsAndEffects>();

				return _buffsEffects;
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
        private CollisionDamageHandler _colDmgHandler;
        protected CollisionDamageHandler ColDmgHandler
        {
            get
            {
                if (!_colDmgHandler)
                    _colDmgHandler = GetComponent<CollisionDamageHandler>();

                return _colDmgHandler;
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

        protected void Start()
        {
            if (ColDmgHandler)
                ColDmgHandler.damagedEvent.AddListener(Damaged);
        }

        void Damaged()
        {
            if (Stats)
            {
                if (!Stats.health.IsAlive() && !dieRoutineStarted)
                {
                    //Died
                    if (BuffsEffects)
                    {
                        BuffsEffects.PlayDeathParticles();
                    }

				    StartCoroutine(DieRoutine());

                }
                else
                {
                    //Took damage
                    if (BuffsEffects)
                    {
                        Debug.Log("Should play damage particles");
                        if (ColDmgHandler)
                            BuffsEffects.PlayDamageParticles(ColDmgHandler.GetLastHitData().position, ColDmgHandler.GetLastHitData().force);
                        else
                            BuffsEffects.PlayDamageParticles();
                    }
                }
            }
        }

		#endregion Initialization & Updates

		#region Coroutines
		/// <summary>
		/// Plays effects, disables colliders etc in this routine. Destroys the gameObject if needed.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator DieRoutine()
		{
			if (BuffsEffects)
			{
				BuffsEffects.PlayDeathParticles();
			}

            ColDmgHandler.SetColliders(false);

			yield return new WaitForSeconds(2f);

			Destroy(gameObject);
		}

		#endregion Coroutines
        
	}
}