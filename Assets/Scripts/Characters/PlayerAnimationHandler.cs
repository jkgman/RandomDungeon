using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Characters
{

	public class PlayerAnimationHandler : CharacterAnimationHandler
	{

		protected const string DODGE_NAME = "DODGE";


		//Called when dodge action starts
		public void SetDodgeStarted()
		{
			Animator.SetBool("isDodging", true);
		}
		
		//Called when dodge action ends or gets cancelled.
		public void SetDodgeCancelled()
		{
			Animator.SetBool("isDodging", false);
		}


	}
}