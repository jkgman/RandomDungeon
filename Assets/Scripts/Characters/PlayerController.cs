using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;

namespace Dungeon.Characters
{
/// <summary>
/// Handles player movement and rotation.
/// </summary>
	public class PlayerController : MonoBehaviour, IAllowedActions
	{

		[SerializeField] private bool debugVisuals = false;

		[Header("Movement Controlling")]

		[SerializeField] private bool keepMomentumInAir = true;
		[SerializeField] private float gravity = 5f;
		[SerializeField] private float moveSpeed = 15f;
		[SerializeField, Range(0f, 1f)] private float accelerationSpeed = 0.2f;
		[SerializeField, Range(0f, 1f)] private float deaccelerationSpeed = 0.5f;
		[SerializeField] private float runSpeedMultiplier = 1.5f;
		[SerializeField] private float rotationSpeed = 10f;
		[SerializeField] private LayerMask groundLayerMask;


		private float moveInputToggleTime;
		private Vector3 currentMoveSpeedRaw;
		private Vector3 moveVelocityAtToggle;
		private Vector3 lastNonZeroMoveDirection = Vector3.forward;

		bool lookRotFromInput = true;
		private Quaternion lookRotRaw = Quaternion.identity;

		private Vector3 currentMoveOffset;
		private float vSpeed = 0;
		private bool _isRunning;
		private bool isGrounded;
		private Vector3 slopeNormal;


		[Header("Inputs")]


		private float inputTargetSwitchTime = 0;
		private float inputTargetSwitchInterval = 0.1f;
		private float inputRunStartTime = 0;

		private Inputs inputs;
		private Vector2 moveInputRaw;



		#region References

		private Player _pManager;
		private Player PManager {
			get {
				if (!_pManager)
					_pManager = GetComponent<Player>();
				return _pManager;
			}
		}

		private CharacterController _controller;
		private CharacterController Controller
		{
			get
			{
				if (!_controller)
					_controller = GetComponent<CharacterController>();

				return _controller;
			}
		}

		PlayerAnimationHandler _animHandler;
		PlayerAnimationHandler AnimHandler
		{
			get
			{
				if (!_animHandler)
					_animHandler = GetComponentInChildren<PlayerAnimationHandler>();

				return _animHandler;
			}
		}

		#endregion


		#region Getters & Setters

		/// <summary>
		/// MoveSpeed with runMultiplier
		/// </summary>
		/// <returns></returns>
		private float GetMaxSpeed()
		{
			return moveSpeed * runSpeedMultiplier;
		}

		/// <summary>
		/// Gets input direction relative to camera's flat forward direction.
		/// </summary>
		/// <param name="allowZero">If no input, returns transform.forward instead of zero</param>
		public Vector3 GetTransformedInputDirection(bool allowZero = true)
		{
			if (moveInputRaw.magnitude == 0)
			{
				if (allowZero)
					return Vector3.zero;
				else
				{
					return transform.forward;
				}
			}
			else
			{
				var moveDir = new Vector3(moveInputRaw.x, 0, moveInputRaw.y);
				var dir = Quaternion.LookRotation(moveDir) * -PManager.GetCam.GetCurrentFlatDirection();
				return dir.normalized;
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


		private bool IsRunning
		{
			get { return _isRunning; }
			set
			{
				if (PManager.AllowRun())
				{
					_isRunning = value;
				}
				else
				{
					_isRunning = false;
				}
			}
		}

		#endregion

		#region Tests & Checks & Calculations

		bool CheckGrounded() 
		{
			bool g = RaycastGrounded() || ControllerGrounded();
			isGrounded = g;
			return g;
		}

		bool ControllerGrounded() {
			return (Controller.isGrounded || Controller.collisionFlags.HasFlag(CollisionFlags.Below) || Controller.collisionFlags.HasFlag(CollisionFlags.CollidedBelow));
		}
	
		bool RaycastGrounded() 
		{
			float distance = Controller.height * 0.5f + Controller.skinWidth - Controller.radius * 0.9f;
			//groundCheckPosition = transform.position + (Vector3.down * distance);
			bool check = Physics.SphereCastAll(transform.position, Controller.radius, Vector3.down, distance, groundLayerMask).Length > 0;
			return check;
		}

		Vector3 CheckSlopeNormal() 
		{
			Vector3 output = Vector3.up;

			if (!isGrounded)
				return output;

			//First spherecast finds the first point that has collided with player
			//Sometimes the point is a corner of something which causes the hit.normal to be fucked
			//Fix was to make a raycast right in front of the point that spherecast found and take the normal from there.

			float distance = Controller.height;
			RaycastHit hit;
			Physics.SphereCast(transform.position + Controller.center, Controller.radius, Vector3.down, out hit, distance, groundLayerMask.value);
			if (hit.collider)
			{
				output = hit.normal;

				RaycastHit[] hits = Physics.RaycastAll(hit.point + GetTransformedInputDirection() * 0.05f + Vector3.up, Vector3.down, 2f, groundLayerMask.value);
				for (int i = 0; i < hits.Length; i++)
				{
					if (hits[i].collider == hit.collider)
					{
						output = hits[i].normal;
						break;
					}
				}
			}


			slopeNormal = output;
			return output;
		}



		Vector2 MovePointAlongCircle(float currentAngle, Vector2 currentPoint, Vector2 centerPoint, float distance) 
		{
			var r = Vector2.Distance(currentPoint, centerPoint);
			var a1 = currentAngle * (Mathf.PI / 180);
			var a2 = a1 + distance / r;
			var p2 = Vector2.zero;
			p2.x = centerPoint.x + r * Mathf.Sin(a2);
			p2.y = centerPoint.y + r * Mathf.Cos(a2);
			return p2;
		}

		#endregion

		#region Initialization

		void Awake()
		{
			ControlsSubscribe();
		}
		void OnDisable()
		{
			ControlsUnsubscribe();
		}

		#endregion

		#region Updates

		void Update()
		{
			UpdateDebug();
			CheckGrounded();                //Makes ground checks so they do not need to be repeated multiple times
			CalculateMoveSpeed();           //Assigns acceleration to inputs
			UpdateVelocity();				//Sets movementOffset (velocity) from input's moveSpeed
			Rotate();						//Rotates towards movement direction or towards target
			ApplyGravity();					//Set Y offset to moveSpeed
			MoveAlongSlope();				//Calculate modified move direction for smooth slope movement
			ApplyMovementToController();    //Gives movement to Unity Character Controller
			UpdateAnimationData();			//Send movement data for animation handling
		}

		void LateUpdate()
		{
			LateSetMovementVariables();
		}

		void UpdateDebug()
		{
			setGizmos = true;
		}

		#endregion
		

		#region Movement
		
		void CalculateMoveSpeed()
		{
			if (!PManager.AllowMove())
			{
				currentMoveSpeedRaw = Vector3.zero;
			}
			else if (moveInputRaw.magnitude > 0)
			{
				Vector3 newMoveSpeed = new Vector3(moveInputRaw.x, 0, moveInputRaw.y) * moveSpeed * (_isRunning ? runSpeedMultiplier : 1f);
				currentMoveSpeedRaw = Vector3.Lerp(moveVelocityAtToggle, newMoveSpeed, (Time.time - moveInputToggleTime) / accelerationSpeed);
			}
			else
			{
				if (moveVelocityAtToggle.magnitude < currentMoveSpeedRaw.magnitude)
					moveVelocityAtToggle = currentMoveSpeedRaw;
				
				currentMoveSpeedRaw = Vector3.Lerp(moveVelocityAtToggle, Vector3.zero, (Time.time - moveInputToggleTime) / deaccelerationSpeed);
			}

		}

		void ApplyGravity()
		{
			if (isGrounded)
			{
				vSpeed = -gravity * Time.deltaTime;
			}
			else
			{
				// apply gravity acceleration to vertical speed:
				vSpeed += -gravity * Time.deltaTime;
				currentMoveOffset += Vector3.up*vSpeed * Time.deltaTime;
			}
		}

		void UpdateVelocity()
		{
			if (isGrounded || !keepMomentumInAir)
			{
				Vector3 newMoveOffset = Vector3.zero;

				if (PManager.PCombat.Target != null)
				{
					//This calculation moves along circular path around target according to sideways input (moveInputRaw.x)
					Vector2 pos = new Vector2(transform.position.x, transform.position.z);
					Vector2 targetPos = new Vector2(PManager.PCombat.Target.GetPosition().x, PManager.PCombat.Target.GetPosition().z);

					float currentAngle = Vector3.SignedAngle(Vector3.forward, -PManager.PCombat.GetFlatDirectionToTarget(), Vector3.up);
					Vector2 newPosWithSidewaysOffset = MovePointAlongCircle(currentAngle, pos,targetPos, -currentMoveSpeedRaw.x * Time.deltaTime);

					//Apply sideways inputs to transform position
					if (!float.IsNaN(newPosWithSidewaysOffset.x) && !float.IsNaN(newPosWithSidewaysOffset.y)) // Apparently this happens too
						newMoveOffset += new Vector3(newPosWithSidewaysOffset.x, transform.position.y, newPosWithSidewaysOffset.y) - transform.position;

					//Add forward input and apply to transform position as well
					Quaternion rot = Quaternion.LookRotation(PManager.PCombat.GetFlatDirectionToTarget());
					Vector3 forwardMoveDir = rot * new Vector3(0, 0, currentMoveSpeedRaw.z);
					newMoveOffset += forwardMoveDir * Time.deltaTime;

					//Prevent from going too close. This might become problematic for combat but we'll see.
					float flatDistanceToTarget = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(PManager.PCombat.Target.GetPosition().x, 0, PManager.PCombat.Target.GetPosition().z));
					if (flatDistanceToTarget < 0.5f)
					{
						Vector3 newPos = new Vector3(PManager.PCombat.Target.GetPosition().x, transform.position.y, PManager.PCombat.Target.GetPosition().z) - PManager.PCombat.GetFlatDirectionToTarget().normalized*0.5f;
						newMoveOffset += newPos - transform.position;
					}
				}
				else
				{

					Quaternion rot = Quaternion.LookRotation(-PManager.GetCam.GetCurrentFlatDirection());
					Vector3 realMoveDirection = rot * new Vector3(currentMoveSpeedRaw.x, 0, currentMoveSpeedRaw.z);
					newMoveOffset += realMoveDirection * Time.deltaTime;
				}
	
				newMoveOffset.y = 0;
				currentMoveOffset = newMoveOffset;
			}

		}

		void Rotate()
		{
			UpdateLookRotRaw();

			transform.rotation = Quaternion.Slerp(transform.rotation, lookRotRaw, Time.deltaTime * rotationSpeed);
			
		}

		private void UpdateLookRotRaw()
		{
			if (PManager.AllowRotate() && GetTransformedInputDirection().magnitude > 0)
				lookRotFromInput = true;

			if (PManager.AllowRotate())
			{
				if (PManager.PCombat.Target != null)
				{
					var lookpos = PManager.PCombat.Target.GetPosition() - transform.position;
					lookpos.y = 0;
					lookRotRaw = Quaternion.LookRotation(lookpos);
				}
				else if (lookRotFromInput)
				{
					var lookpos = GetFlatMoveDirection(allowZero: false);
					lookpos.y = 0;
					lookRotRaw = Quaternion.LookRotation(lookpos);
				}
			}
			else
			{
				lookRotFromInput = false;
			}

		}

		void MoveAlongSlope()
		{
			if (Controller && isGrounded && currentMoveOffset.magnitude > 0)
			{
				CheckSlopeNormal();
				
				Vector3 relativeRight = Vector3.Cross(currentMoveOffset, Vector3.up);
				Vector3 newForward = Vector3.Cross(slopeNormal, relativeRight);
				bool goingUp = Vector3.Angle(new Vector3(slopeNormal.x,0,slopeNormal.z).normalized, new Vector3(currentMoveOffset.x,0,currentMoveOffset.z).normalized) > 45f;
				bool withinSlopeLimit = Vector3.Angle(slopeNormal, Vector3.up) < Controller.slopeLimit;
				if (newForward.magnitude > 0 && (!goingUp || withinSlopeLimit))
				{
					float magnitude = new Vector3(currentMoveOffset.x, 0, currentMoveOffset.z).magnitude;
					float storeY = currentMoveOffset.y;
					currentMoveOffset = newForward.normalized * magnitude;
					currentMoveOffset.y += storeY;
				}
			
			}
		}

		void ApplyMovementToController()
		{
			if (Controller)
				Controller.Move(currentMoveOffset);
			else
				Debug.LogWarning("No Unity Character Controller found in Player. Movement not applied.");
		}

		void LateSetMovementVariables()
		{
			if (currentMoveOffset.magnitude > 0)
				lastNonZeroMoveDirection = currentMoveOffset.normalized;
			else if (moveInputRaw.magnitude > 0 && PManager.AllowMove())
				lastNonZeroMoveDirection = GetTransformedInputDirection();
		}


		public void ExternalMove(Vector3 offset)
		{
			if (Controller)
			{
				Controller.Move(offset);
			}
		}
		public void ExternalRotate(Vector3 lookDirection, bool instant = false)
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
		public void ExternalRotateToInputDirection(bool instant = false)
		{
			if (moveInputRaw.magnitude > 0)
			{
				Debug.Log("Should be updating rotation now");
				var dir = GetTransformedInputDirection();
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

		#endregion

		#region HandleInputs



		void ControlsSubscribe()
		{
			if (inputs == null)
				inputs = new Inputs();

			inputs.Player.Move.started += InputMoveStarted;
			inputs.Player.Move.performed += InputMovePerformed;
			inputs.Player.Move.canceled += InputMoveCancelled;
			inputs.Player.Move.Enable();

			inputs.Player.RunAndDodge.started += InputRunStarted;
			inputs.Player.RunAndDodge.performed -= InputRunPerformed;
			inputs.Player.RunAndDodge.canceled += InputRunCancelled;
			inputs.Player.RunAndDodge.Disable();
		}

		void ControlsUnsubscribe()
		{
			inputs.Player.Move.started += InputMoveStarted;
			inputs.Player.Move.performed -= InputMovePerformed;
			inputs.Player.Move.canceled -= InputMoveCancelled;
			inputs.Player.Move.Disable();


			inputs.Player.RunAndDodge.started -= InputRunStarted;
			inputs.Player.RunAndDodge.performed -= InputRunPerformed;
			inputs.Player.RunAndDodge.canceled -= InputRunCancelled;
			inputs.Player.RunAndDodge.Disable();
		}



		void InputMoveStarted(InputAction.CallbackContext context) 
		{
			moveInputRaw = context.ReadValue<Vector2>();
			moveInputToggleTime = Time.time;
			moveVelocityAtToggle = currentMoveSpeedRaw;

		}

		void InputMovePerformed(InputAction.CallbackContext context) 
		{
			moveInputRaw = context.ReadValue<Vector2>();

		}

		void InputMoveCancelled(InputAction.CallbackContext context) 
		{
			moveInputRaw = Vector2.zero;
			moveInputToggleTime = Time.time;
			moveVelocityAtToggle = currentMoveSpeedRaw;

		}


		void InputRunStarted(InputAction.CallbackContext context) 
		{
			inputRunStartTime = Time.time;

		}
		void InputRunPerformed(InputAction.CallbackContext context) 
		{
			if (Time.time - inputRunStartTime > PManager.inputMaxPressTime)
			{
				IsRunning = true;
			}
		}
		void InputRunCancelled(InputAction.CallbackContext context) 
		{
			IsRunning = false;
		}

		#endregion

		#region Animations


		void UpdateAnimationData()
		{
			float movePercentage = currentMoveSpeedRaw.magnitude / GetMaxSpeed();
			float angle = Vector3.SignedAngle(transform.forward, Vector3.forward, Vector3.up);
			Vector3 relativeMoveDirection = Quaternion.Euler(0, angle, 0) * GetFlatMoveDirection();
			Vector2 blend = new Vector2(relativeMoveDirection.x, relativeMoveDirection.z).normalized * movePercentage;

			AnimHandler.SetMovementPerformed(blend);

		}

		#endregion

		#region Debug

		bool setGizmos = false;

		void OnDrawGizmos()
		{
			if (!debugVisuals)
				return;

			Gizmos.color = Color.red;

			Gizmos.DrawRay(transform.position, Vector3.down * Controller.height * 0.5f);
			//Gizmos.DrawLine(transform.position, transform.position + relativeRight.normalized);
			//Gizmos.DrawRay(transform.position, newForward.normalized*3f);
			//Gizmos.DrawWireSphere(groundCheckPosition, Controller.radius);
		
		
			//if (Application.isPlaying && updateGizmos)
			//	guiPositions[guiPosIndex] = transform.position;
			//for (int i = 0; i < guiPositions.Length; i++)
			//{
			//	if (guiPositions[i] != null)
			//		Gizmos.DrawWireSphere(guiPositions[i], 0.15f);
			//}
			//guiPosIndex++;
			//if (guiPosIndex >= guiPositions.Length)
			//	guiPosIndex = 0;


			setGizmos = false;
		}



		#endregion

		#region IAllowedActions

		public bool AllowMove() 
		{
			//This script currently does not have anything disabling its own actions.
			return true;
		}

		public bool AllowRun() 
		{
			//This script currently does not have anything disabling its own actions.
			return true;
		}
		public bool AllowRotate()
		{
			//This script currently does not have anything disabling its own actions.
			return true;
		}
		public bool AllowAttack() 
		{
			//This script currently does not have anything disabling other classes' actions.
			return true;
		}

		public bool AllowDodge() 
		{
			//This script currently does not have anything disabling other classes' actions.
			return true;
		}

		#endregion
	}
}
