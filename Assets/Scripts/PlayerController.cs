using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Experimental.Input;
using System.Collections;

namespace Dungeon.Player
{
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
		private Vector3 currentForward;
		private Vector3 lastMoveDirection;

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



		#region Getters & Setters

		private PlayerManager _pManager;
		private PlayerManager PManager {
			get {
				if (!_pManager)
					_pManager = GetComponent<PlayerManager>();
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
	
		public Vector3 GetPos()
		{
			return transform.position;
		}

		private float GetMaxSpeed()
		{
			return moveSpeed * runSpeedMultiplier;
		}

		public Vector3 GetFlatMoveInputDirection()
		{
			var lookDir = new Vector3(moveInputRaw.x, 0, moveInputRaw.y);
			
			
			if (lookDir.magnitude == 0)
			{
				return Vector3.zero;
			}
			else
			{
				var dir = Quaternion.LookRotation(lookDir) * -PManager.GetCam.GetCurrentFlatDirection();
				return dir.normalized;
			}
		}

		public Vector3 GetFlatMoveDirection(bool allowZero = true)
		{
			if (allowZero || currentMoveOffset.magnitude > 0)
				return new Vector3(currentMoveOffset.x, 0, currentMoveOffset.z).normalized;
			else
				return new Vector3(lastMoveDirection.x, 0, lastMoveDirection.z).normalized;
		}


		private bool SetRunning(bool value)
		{
			if (PManager.AllowRun())
			{
				_isRunning = value;
				return _isRunning;
			}
			else
			{
				_isRunning = false;
				return _isRunning;
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

				RaycastHit[] hits = Physics.RaycastAll(hit.point + GetFlatMoveInputDirection() * 0.05f + Vector3.up, Vector3.down, 2f, groundLayerMask.value);
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
			SetCurrentForward();
			CheckGrounded();                //Makes ground checks so they do not need to be repeated multiple times
			CalculateMoveSpeed();           //Assigns acceleration to inputs
			UpdateVelocity();				//Sets movementOffset (velocity) from input's moveSpeed
			Rotate();						//Rotates towards movement direction or towards target
			ApplyGravity();					//Set Y offset to moveSpeed
			MoveAlongSlope();				//Calculate modified move direction for smooth slope movement
			ApplyMovementToController();    //Gives movement to Unity Character Controller
			SetMovementVariables();
			UpdateAnimationData();			//Send movement data for animation handling
		}

		void UpdateDebug()
		{
			setGizmos = true;
		}

		#endregion
		

		#region Movement

		void SetCurrentForward()
		{
			if (moveInputRaw.magnitude > 0)
			{
				currentForward = -PManager.GetCam.GetCurrentFlatDirection();
			}
		}

		void CalculateMoveSpeed()
		{
			if (!PManager.AllowMove())
			{
				moveInputRaw = Vector2.zero;
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

				if (PManager.PCombat.Target)
				{
					//This calculation moves along circular path around target according to sideways input (moveInputRaw.x)
					Vector2 pos = new Vector2(transform.position.x, transform.position.z);
					Vector2 targetPos = new Vector2(PManager.PCombat.Target.transform.position.x, PManager.PCombat.Target.transform.position.z);

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
					float flatDistanceToTarget = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(PManager.PCombat.Target.transform.position.x, 0, PManager.PCombat.Target.transform.position.z));
					if (flatDistanceToTarget < 0.5f)
					{
						Vector3 newPos = new Vector3(PManager.PCombat.Target.transform.position.x, transform.position.y, PManager.PCombat.Target.transform.position.z) - PManager.PCombat.GetFlatDirectionToTarget().normalized*0.5f;
						newMoveOffset += newPos - transform.position;
					}
				}
				else
				{

					Quaternion rot = Quaternion.LookRotation(currentForward);
					Vector3 realMoveDirection = rot * new Vector3(currentMoveSpeedRaw.x, 0, currentMoveSpeedRaw.z);
					newMoveOffset += realMoveDirection * Time.deltaTime;
				}
	
				newMoveOffset.y = 0;
				currentMoveOffset = newMoveOffset;
			}

		}

		void Rotate()
		{
			if (PManager.AllowRotate())
			{
				if (PManager.PCombat.Target)
				{
					var lookpos = PManager.PCombat.Target.transform.position - transform.position;
					lookpos.y = 0;
					var rot = Quaternion.LookRotation(lookpos);
					transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * rotationSpeed);
				}
				else
				{
					if (currentMoveSpeedRaw.magnitude > 0)
					{
						var lookDir = new Vector3(currentMoveSpeedRaw.x, 0, currentMoveSpeedRaw.z);
						var dir = Quaternion.LookRotation(lookDir) * currentForward;
						transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSpeed);
					}
				}
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

		void SetMovementVariables()
		{
			if (currentMoveOffset.magnitude > 0)
				lastMoveDirection = currentMoveOffset.normalized;
		}

		public void ExternalMove(Vector3 offset)
		{
			if (Controller)
			{
				Controller.Move(offset);
			}
		}



		#endregion

		#region HandleInputs

		void ResetInputModifiers()
		{
			//These should be zero for next frame in case no input was given
			moveInputRaw = Vector3.zero;
		}



		void ControlsSubscribe()
		{
			if (inputs == null)
				inputs = new Inputs();

			inputs.Player.Move.started += InputMoveStarted;
			inputs.Player.Move.performed += InputMovePerformed;
			inputs.Player.Move.cancelled += InputMoveCancelled;
			inputs.Player.Move.Enable();

			inputs.Player.RunAndDodge.started += InputRunStarted;
			inputs.Player.RunAndDodge.performed -= InputRunPerformed;
			inputs.Player.RunAndDodge.cancelled += InputRunCancelled;
			inputs.Player.RunAndDodge.Disable();
		}

		void ControlsUnsubscribe()
		{
			inputs.Player.Move.started += InputMoveStarted;
			inputs.Player.Move.performed -= InputMovePerformed;
			inputs.Player.Move.cancelled -= InputMoveCancelled;
			inputs.Player.Move.Disable();


			inputs.Player.RunAndDodge.started -= InputRunStarted;
			inputs.Player.RunAndDodge.performed -= InputRunPerformed;
			inputs.Player.RunAndDodge.cancelled -= InputRunCancelled;
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
				SetRunning(true);
			}
		}
		void InputRunCancelled(InputAction.CallbackContext context) 
		{
			SetRunning(false);
		}

		#endregion

		#region Animations
		PlayerAnimationHandler _animHandler;
		PlayerAnimationHandler AnimHandler {
			get {
				if (!_animHandler)
					_animHandler = GetComponentInChildren<PlayerAnimationHandler>();

				return _animHandler;
			}
		}

		void UpdateAnimationData()
		{
			float movePercentage = currentMoveSpeedRaw.magnitude / GetMaxSpeed();
			Vector3 relativeMoveDirection = (transform.rotation * currentForward);
			Vector2 blend = new Vector2(relativeMoveDirection.x, relativeMoveDirection.z).normalized * movePercentage;

			AnimHandler.SetMovement(blend);

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
