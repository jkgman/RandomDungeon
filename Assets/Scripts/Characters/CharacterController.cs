using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dungeon.Characters
{

	public class CharacterController : MonoBehaviour
	{

		[SerializeField] protected float moveSpeed = 15f;
		[SerializeField] protected float runSpeedMultiplier = 1.5f;
		[SerializeField] protected float rotationSpeed = 10f;
		[SerializeField, Range(0f, 1f)] protected float accelerationSpeed = 0.2f;
		[SerializeField, Range(0f, 1f)] protected float deaccelerationSpeed = 0.5f;

		protected Vector3 currentMoveSpeed;
		protected Vector3 currentMoveOffset;
		protected Vector3 lastNonZeroMoveDirection = Vector3.forward;
		protected Quaternion lookRotRaw = Quaternion.identity;


		private CharacterAnimationHandler _animHandler;
		public CharacterAnimationHandler AnimHandler
		{
			get
			{
				if (!_animHandler)
					_animHandler = GetComponentInChildren<CharacterAnimationHandler>();

				return _animHandler;
			}
		}





		/// <summary>
		/// Last non-zero direction where player has moved.
		/// </summary>
		private Vector3 GetLastFlatMoveDirection()
		{
			Vector3 output = lastNonZeroMoveDirection;
			output.y = 0;
			if (output.magnitude == 0)
				return Vector3.forward;
			else
				return output.normalized;
		}

		/// <summary>
		/// Gets direction from CurrentMoveOffset. If allowZero is false, last non-zero flat move direction is returned.
		/// </summary>
		public Vector3 GetFlatMoveDirection(bool allowZero = true)
		{
			if (allowZero || currentMoveOffset.magnitude > 0)
				return new Vector3(currentMoveOffset.x, 0, currentMoveOffset.z).normalized;
			else
				return GetLastFlatMoveDirection();
		}



		public virtual void ExternalMove(Vector3 offset)
		{

		}
		public virtual void ExternalRotate(Vector3 lookDirection, bool instant = false)
		{
			var dir = lookDirection;
			dir.y = 0;

			if (instant)
			{
				lookRotRaw = Quaternion.LookRotation(dir);
				transform.rotation = lookRotRaw;
			}
			else
			{
				lookRotRaw = Quaternion.LookRotation(dir);
			}
		}
	}
}
