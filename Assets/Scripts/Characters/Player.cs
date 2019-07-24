using System;
using System.Collections;
using UnityEngine;

namespace Dungeon.Characters
{

	/// <summary>
	/// Master script of player that holds references to sub-scripts.
	/// </summary>
	public class Player : Character, IAllowedPlayerActions
	{

		#region Variables & References
		//If same key uses multiple bindings depending on the length of input, this is used.
		//Example: Running=Press&Hold - Dodge=Press&Release
		public static readonly float inputSinglePressMaxTime = 0.3f;



		//_______ Start of Class References
		private PlayerCombatHandler _pCombat;
		private PlayerCombatHandler PCombat
		{
			get
			{
				if (!_pCombat)
					_pCombat = GetComponent<PlayerCombatHandler>();
				return _pCombat;
			}
		}
		private PlayerMovement _pMovement;
		private PlayerMovement PMovement
		{
			get
			{
				if (!_pMovement)
					_pMovement = GetComponent<PlayerMovement>();
				return _pMovement;
			}
		}
		private PlayerAnimationHandler _pAnimation;
		private PlayerAnimationHandler PAnimation
		{
			get
			{
				if (!_pAnimation)
					_pAnimation = GetComponent<PlayerAnimationHandler>();
				return _pAnimation;
			}
		}
		//_______ End of Class References

		#endregion Variables & References



		#region Routines

		protected override IEnumerator DieRoutine()
		{
			dieRoutineStarted = true;

			Effects.PlayDeathParticles();
			DisableColliders();
			Ragdoll.StartRagdoll();
	
			isActive = false;

			yield return new WaitForSeconds(2f);
			Respawn();
		}

		#endregion

		#region Hidden Functions

		void Respawn()
		{
			Ragdoll.Reset();
			Stats.health.AddHealth(100000f);
			transform.position = PMovement.GetSpawnPosition();
			Effects.SetVisible();

			EnableColliders();
			isActive = true;
			dieRoutineStarted = false;
		}
		#endregion Hidden Functions

		#region IAllowedActions

		public bool AllowMove() 
		{
			bool output = true;
			
			output = PMovement.AllowMove() ? output : false;
			output = PCombat.AllowMove() ? output : false;
			output = !Ragdoll.IsRagdolling ? output : false;


			return output;
		}
		public bool AllowRun() 
		{
			bool output = true;
			
			output = PMovement.AllowRun() ? output : false;
			output = PCombat.AllowRun() ? output : false;
			output = !Ragdoll.IsRagdolling ? output : false;

			return output;
		}
		public bool AllowAttack() 
		{
			bool output = true;

			output = PMovement.AllowAttack() ? output : false;
			output = PCombat.AllowAttack() ? output : false;
			output = !Ragdoll.IsRagdolling ? output : false;

			return output;
		}
		public bool AllowDodge()
		{
			bool output = true;

			output = PMovement.AllowDodge() ? output : false;
			output = PCombat.AllowDodge() ? output : false;
			output = !Ragdoll.IsRagdolling ? output : false;

			return output;
		}
		public bool AllowRotate()
		{
			bool output = true;

			output = PMovement.AllowRotate() ? output : false;
			output = PCombat.AllowRotate() ? output : false;
			output = !Ragdoll.IsRagdolling ? output : false;

			return output;
		}

		#endregion

	}
}
