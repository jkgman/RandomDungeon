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
	public class PlayerController : CharacterController, IAllowedPlayerActions
	{

		[SerializeField] private bool debugVisuals = false;

		[Header("Movement Controlling")]

		[SerializeField] private bool keepMomentumInAir = true;
		[SerializeField] private float gravity = 5f;

		[SerializeField] private LayerMask groundLayerMask;



		private float vSpeed = 0;
		private bool isGrounded;
		private Vector3 slopeNormal;
		private Vector3 spawnPosition;


		[Header("Inputs")]


		private float inputTargetSwitchTime = 0;
		private float inputTargetSwitchInterval = 0.1f;
		private float inputRunStartTime = 0;
		private Vector3 moveVelocityAtToggle; //Used for accelerations when input happens/doesnt happen
		private float moveInputToggleTime;


		private Vector2 moveInputRaw;



		#region References

		private Player _player;
		private Player Player {
			get {
				if (!_player)
					_player = GetComponent<Player>();
				return _player;
			}
		}

		private Inputs PlayerInputs
		{
			get { return Player.Inputs; }
		}

		private UnityEngine.CharacterController _unityController;
		public UnityEngine.CharacterController UnityController
		{
			get
			{
				if (!_unityController)
					_unityController = GetComponent<UnityEngine.CharacterController>();

				return _unityController;
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

		public Vector3 GetSpawnPosition()
		{
			return spawnPosition;
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
				var dir = Quaternion.LookRotation(moveDir) * -Player.GetCam.GetCurrentFlatDirection();
				return dir.normalized;
			}
		}
		
		protected override void SetRunning(bool value)
		{
			if (Player.AllowRun())
			{
				base.SetRunning(value);
				Debug.Log("running:" + GetRunning());
			}
			else
			{
				base.SetRunning(false);
				Debug.Log("not running");
			}
		}

		#endregion

		#region Tests & Checks & Calculations

		public override bool IsGrounded() 
		{
			bool g = RaycastGrounded() || ControllerGrounded();
			isGrounded = g;
			return g;
		}

		bool ControllerGrounded() {
			return (UnityController.isGrounded || UnityController.collisionFlags.HasFlag(CollisionFlags.Below) || UnityController.collisionFlags.HasFlag(CollisionFlags.CollidedBelow));
		}
	
		bool RaycastGrounded() 
		{
			float distance = UnityController.height * 0.5f + UnityController.skinWidth - UnityController.radius * 0.9f;
			//groundCheckPosition = transform.position + (Vector3.down * distance);
			bool check = Physics.SphereCastAll(transform.position, UnityController.radius, Vector3.down, distance, groundLayerMask).Length > 0;
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

			float distance = UnityController.height;
			RaycastHit hit;
			Physics.SphereCast(transform.position + UnityController.center, UnityController.radius, Vector3.down, out hit, distance, groundLayerMask.value);
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
		void OnEnable()
		{
			spawnPosition = transform.position;
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
			IsGrounded();                //Makes ground checks so they do not need to be repeated multiple times
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
			if (!Player.AllowMove())
			{
				currentMoveSpeed = Vector3.zero;
			}
			else if (moveInputRaw.magnitude > 0)
			{
				Vector3 newMoveSpeed = new Vector3(moveInputRaw.x, 0, moveInputRaw.y) * moveSpeed * (GetRunning() ? runSpeedMultiplier : 1f);
				currentMoveSpeed = Vector3.Lerp(moveVelocityAtToggle, newMoveSpeed, (Time.time - moveInputToggleTime) / accelerationDuration);
			}
			else
			{
				if (moveVelocityAtToggle.magnitude < currentMoveSpeed.magnitude)
					moveVelocityAtToggle = currentMoveSpeed;
				
				currentMoveSpeed = Vector3.Lerp(moveVelocityAtToggle, Vector3.zero, (Time.time - moveInputToggleTime) / deaccelerationDuration);
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

				if (Player.PCombat.Target != null)
				{
					//This calculation moves along circular path around target according to sideways input (moveInputRaw.x)
					Vector2 pos = new Vector2(transform.position.x, transform.position.z);
					Vector2 targetPos = new Vector2(Player.PCombat.Target.GetPosition().x, Player.PCombat.Target.GetPosition().z);

					float currentAngle = Vector3.SignedAngle(Vector3.forward, -Player.PCombat.GetFlatDirectionToTarget(), Vector3.up);
					Vector2 newPosWithSidewaysOffset = MovePointAlongCircle(currentAngle, pos,targetPos, -currentMoveSpeed.x * Time.deltaTime);

					//Apply sideways inputs to transform position
					if (!float.IsNaN(newPosWithSidewaysOffset.x) && !float.IsNaN(newPosWithSidewaysOffset.y)) // Apparently this happens too
						newMoveOffset += new Vector3(newPosWithSidewaysOffset.x, transform.position.y, newPosWithSidewaysOffset.y) - transform.position;

					//Add forward input and apply to transform position as well
					Quaternion rot = Quaternion.LookRotation(Player.PCombat.GetFlatDirectionToTarget());
					Vector3 forwardMoveDir = rot * new Vector3(0, 0, currentMoveSpeed.z);
					newMoveOffset += forwardMoveDir * Time.deltaTime;

					//Prevent from going too close. This might become problematic for combat but we'll see.
					float flatDistanceToTarget = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(Player.PCombat.Target.GetPosition().x, 0, Player.PCombat.Target.GetPosition().z));
					if (flatDistanceToTarget < 0.5f)
					{
						Vector3 newPos = new Vector3(Player.PCombat.Target.GetPosition().x, transform.position.y, Player.PCombat.Target.GetPosition().z) - Player.PCombat.GetFlatDirectionToTarget().normalized*0.5f;
						newMoveOffset += newPos - transform.position;
					}
				}
				else
				{

					Quaternion rot = Quaternion.LookRotation(-Player.GetCam.GetCurrentFlatDirection());
					Vector3 realMoveDirection = rot * new Vector3(currentMoveSpeed.x, 0, currentMoveSpeed.z);
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

			if (Player.AllowRotate())
			{
				if (Player.PCombat.Target != null)
				{
					var lookdir = Player.PCombat.Target.GetPosition() - transform.position;
					lookdir.y = 0;
					lookRotRaw = Quaternion.LookRotation(lookdir);
				}
				else/* if (GetTransformedInputDirection().sqrMagnitude > 0)*/
				{
					var lookdir = GetFlatMoveDirection(allowZero: false);
					lookdir.y = 0;
					lookRotRaw = Quaternion.LookRotation(lookdir);
				}
			}

		}

		void MoveAlongSlope()
		{
			if (UnityController && isGrounded && currentMoveOffset.magnitude > 0)
			{
				CheckSlopeNormal();
				
				Vector3 relativeRight = Vector3.Cross(currentMoveOffset, Vector3.up);
				Vector3 newForward = Vector3.Cross(slopeNormal, relativeRight);
				bool goingUp = Vector3.Angle(new Vector3(slopeNormal.x,0,slopeNormal.z).normalized, new Vector3(currentMoveOffset.x,0,currentMoveOffset.z).normalized) > 45f;
				bool withinSlopeLimit = Vector3.Angle(slopeNormal, Vector3.up) < UnityController.slopeLimit;
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
			if (UnityController)
				UnityController.Move(currentMoveOffset);
			else
				Debug.LogWarning("No Unity Character Controller found in Player. Movement not applied.");
		}

		void LateSetMovementVariables()
		{
			if (currentMoveOffset.sqrMagnitude > 0)
				lastNonZeroMoveDirection = currentMoveOffset.normalized;
			else if (moveInputRaw.sqrMagnitude > 0 && Player.AllowMove())
				lastNonZeroMoveDirection = GetTransformedInputDirection();
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
		public override void ExternalMove(Vector3 offset)
		{
			UnityController.Move(offset);
		}
		#endregion

		#region HandleInputs



		void ControlsSubscribe()
		{
			PlayerInputs.Player.Move.started += InputMoveStarted;
			PlayerInputs.Player.Move.performed += InputMovePerformed;
			PlayerInputs.Player.Move.canceled += InputMoveCancelled;
			PlayerInputs.Player.Move.Enable();

			PlayerInputs.Player.RunAndDodge.started += InputRunStarted;
			PlayerInputs.Player.RunAndDodge.performed += InputRunPerformed;
			PlayerInputs.Player.RunAndDodge.canceled += InputRunCancelled;
			PlayerInputs.Player.RunAndDodge.Enable();
		}

		void ControlsUnsubscribe()
		{
			PlayerInputs.Player.Move.started += InputMoveStarted;
			PlayerInputs.Player.Move.performed -= InputMovePerformed;
			PlayerInputs.Player.Move.canceled -= InputMoveCancelled;
			PlayerInputs.Player.Move.Disable();


			PlayerInputs.Player.RunAndDodge.started -= InputRunStarted;
			PlayerInputs.Player.RunAndDodge.performed -= InputRunPerformed;
			PlayerInputs.Player.RunAndDodge.canceled -= InputRunCancelled;
			PlayerInputs.Player.RunAndDodge.Disable();
		}



		void InputMoveStarted(InputAction.CallbackContext context) 
		{
			moveInputRaw = context.ReadValue<Vector2>();
			moveInputToggleTime = Time.time;
			moveVelocityAtToggle = currentMoveSpeed;

		}

		void InputMovePerformed(InputAction.CallbackContext context) 
		{
			moveInputRaw = context.ReadValue<Vector2>();

		}

		void InputMoveCancelled(InputAction.CallbackContext context) 
		{
			moveInputRaw = Vector2.zero;
			moveInputToggleTime = Time.time;
			moveVelocityAtToggle = currentMoveSpeed;

		}


		void InputRunStarted(InputAction.CallbackContext context) 
		{
			inputRunStartTime = Time.time;

		}
		void InputRunPerformed(InputAction.CallbackContext context) 
		{
			Debug.Log("Running input performed");
			if (Time.time - inputRunStartTime > Player.inputSinglePressMaxTime)
			{
				Debug.Log("Setting running true");
				SetRunning(true);
			}
		}
		void InputRunCancelled(InputAction.CallbackContext context) 
		{
			SetRunning(false);
		}

		#endregion

		#region Animations


		void UpdateAnimationData()
		{
			float movePercentage = currentMoveSpeed.magnitude / GetMaxSpeed();
			float angle = Vector3.SignedAngle(transform.forward, Vector3.forward, Vector3.up);
			Vector3 relativeMoveDirection = Quaternion.Euler(0, angle, 0) * GetFlatMoveDirection();
			Vector2 blend = new Vector2(relativeMoveDirection.x, relativeMoveDirection.z).normalized * movePercentage;

			AnimHandler.SetMovementPerformed(blend);
			AnimHandler.SetGrounded(IsGrounded());

		}

		#endregion

		#region Debug

		bool setGizmos = false;

		void OnDrawGizmos()
		{
			if (!debugVisuals)
				return;

			Gizmos.color = Color.red;

			Gizmos.DrawRay(transform.position, Vector3.down * UnityController.height * 0.5f);

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
