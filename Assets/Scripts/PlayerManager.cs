using UnityEngine;

namespace Dungeon.Player {

	public class PlayerManager : MonoBehaviour, IAllowedActions
	{
		
		//If same key uses multiple bindings depending on the length of input, this is used.
		//Example: Running=Press&Hold - Dodge=Press&Release
		public readonly float inputMaxPressTime = 0.3f;



		#region IAllowedActions

		public bool AllowMove() 
		{
			bool output = true;
			
			output = PController.AllowMove() ? output : false;
			output = PCombat.AllowMove() ? output : false;

			return output;
		}
		public bool AllowRun() 
		{
			bool output = true;
			
			output = PController.AllowRun() ? output : false;
			output = PCombat.AllowRun() ? output : false;

			return output;
		}
		public bool AllowAttack() 
		{
			bool output = true;

			output = PController.AllowAttack() ? output : false;
			output = PCombat.AllowAttack() ? output : false;

			return output;
		}
		public bool AllowDodge()
		{
			bool output = true;

			output = PController.AllowDodge() ? output : false;
			output = PCombat.AllowDodge() ? output : false;

			return output;
		}
		public bool AllowRotate()
		{
			bool output = true;

			output = PController.AllowRotate() ? output : false;
			output = PCombat.AllowRotate() ? output : false;

			return output;
		}

		#endregion

		#region Getters & Setters

		private PlayerController _pController;
		public PlayerController PController
		{
			get {
				if (!_pController)
					_pController = GetComponent<PlayerController>();

				return _pController;
			}
		}

		private PlayerAnimationHandler _pAnimation;
		public PlayerAnimationHandler PAnimation
		{
			get {
				if (!_pAnimation)
					_pAnimation = GetComponent<PlayerAnimationHandler>();

				return _pAnimation;
			}
		}

		private PlayerCombatHandler _pCombat;
		public PlayerCombatHandler PCombat
		{
			get
			{
				if (!_pCombat)
					_pCombat = GetComponent<PlayerCombatHandler>();

				return _pCombat;
			}
		}

		private CameraController _cam;
		public CameraController GetCam {
			get
			{
				if (!_cam)
				{
					_cam = Camera.main.GetComponentInParent<CameraController>();
				}

				return _cam;
			}
		}

		#endregion
	}
}
