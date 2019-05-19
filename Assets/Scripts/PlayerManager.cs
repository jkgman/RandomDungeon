using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Player
{
	public class PlayerManager : MonoBehaviour
	{
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
	}
}
