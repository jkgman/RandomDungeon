using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dungeon.Characters
{

	public class CharacterMovement : MonoBehaviour
	{

		#region Variables & References

		//_______ Start of Exposed variables
		[Tooltip("Default max movement speed.")]
		[SerializeField] protected float moveSpeed = 5f;
		[Tooltip("Multiplier of moveSpeed when running is true.")]
		[SerializeField] protected float runSpeedMultiplier = 1.5f;
		[Tooltip("Maximum turn speed.")]
		[SerializeField] protected float rotationSpeed = 10f;
		[Tooltip("Time to go from initial speed to wanted speed.")]
		[SerializeField, Range(0f, 1f)] protected float accelerationDuration = 0.2f;
		[Tooltip("Time to stop after no movement inputs detected.")]
		[SerializeField, Range(0f, 1f)] protected float deaccelerationDuration = 0.5f;
		//_______ End of Exposed variables

		//_______ Start of Hidden variables
		protected float moveSpeedMultiplier;
		protected float rotationSpeedMultiplier;
		protected Vector3 currentMoveSpeed;
		protected Vector3 currentMoveOffset;
		protected Vector3 lastNonZeroMoveDirection = Vector3.forward;
		protected Quaternion lookRotRaw = Quaternion.identity;
		//_______ End of Hidden variables

		//_______ Start of Class References
		private CharacterAnimationHandler _cAnimHandler;
		private CharacterAnimationHandler CAnimHandler {
			get	{
				if (!_cAnimHandler) _cAnimHandler = GetComponentInChildren<CharacterAnimationHandler>();
				return _cAnimHandler;
			}
		}
		protected Stats _stats;
		private Stats Stats
		{
			get{
				if (!_stats) _stats = GetComponent<Stats>();
				return _stats;
			}
		}
		//_______ End of Class References

		#endregion Variables & References

		#region Getters & Setters

		public bool IsRunning { get; protected set; }
		protected bool GetRunning()
		{
			return IsRunning;
		}
		protected virtual void SetRunning(bool value)
		{
			IsRunning = value;
		}

		public bool IsGrounded{ get; protected set; }
		protected virtual void CheckGrounded()
		{
			//Do checks here if necessary
			IsGrounded = true;
		}


		public void SetMoveSpeedMultiplier(float multiplier)
		{
			moveSpeedMultiplier = multiplier;
		}
		public void SetRotationSpeedMultiplier(float multiplier)
		{
			rotationSpeedMultiplier = multiplier;
		}

		/// <summary>
		/// Last non-zero direction where player has moved.
		/// </summary>
		private Vector3 GetLastFlatMoveDirection()
		{
			Vector3 output = lastNonZeroMoveDirection;
			output.y = 0; //Output should be flat.
			//If output is zero, return current forward.
			return (output.sqrMagnitude == 0) ? transform.forward : output.normalized;
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

		#endregion Getters & Setters

		#region Initialization & Updates
		
		protected virtual void Update()
		{
			SetMoveAndRotationSpeedMultipliers();
			CheckFallenThroughWorld();

		}

		/// <summary>
		/// Restores move and rotation speed to 1 if they have been changed.
		/// </summary>
		private void SetMoveAndRotationSpeedMultipliers()
		{
			//Always slowly restore these multipliers.
			moveSpeedMultiplier = Mathf.Clamp01(moveSpeedMultiplier + Time.deltaTime);
			rotationSpeedMultiplier = Mathf.Clamp01(rotationSpeedMultiplier + Time.deltaTime);
		}

		/// <summary>
		/// Sometimes things fly into the abyss. Kills fallen character.
		/// </summary>
		private void CheckFallenThroughWorld()
		{
			//If fallen out of world, die.
			if (transform.position.y < -100f && Stats)
				Stats.health.SubstractHealth(10000f);
		}


		#endregion Initialization & Updates

		#region Exposed Functions

		/// <summary>
		/// Can be called from outside to force move character.
		/// </summary>
		public virtual void ExternalMove(Vector3 offset)
		{
			transform.position += offset;
		}

		/// <summary>
		/// Can be called from outside to force rotate character.
		/// </summary>
		/// <param name="lookDirection">Direction to rotate towards</param>
		/// <param name="instant">Should rotation happen right away</param>
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

		#endregion Exposed Functions
	}
}
