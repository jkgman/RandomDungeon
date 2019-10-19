using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;
using System;

namespace Dungeon.Characters
{
/// <summary>
/// Handles player movement and rotation.
/// </summary>
	public class PlayerMovement : CharacterMovement, IAllowedPlayerActions
	{

		#region Variables

		//_______ Start of Exposed variables

		[Header("Debug")]
		[SerializeField] private bool debugVisuals = false;

		[Header("Movement Controlling")]
		[SerializeField] private bool keepMomentumInAir = true;
		[SerializeField] private float gravity = 5f;
		[SerializeField] private LayerMask groundLayerMask;

		[Header("Rolling")]
		[SerializeField] private float fallSpeedToRoll = 1f;
		[SerializeField] private float rollingDuration = 0.5f;
		[SerializeField] private AnimationCurve rollingDistance = new AnimationCurve();

		[Header("Losing Balance")]
		[SerializeField] private float fallSpeedToLoseBalance = 2f;
		[SerializeField] private float getUpDelay = 1f;
		[SerializeField] private float getUpDuration = 1f;

		//_______ End of Exposed variables


		//_______ Start of Hidden variables

		private float vSpeed = 0;           //Current vertical speed (gravity)
		private bool setGizmos = false;		//Runtime Gizmo updates

		private float lostBalanceTime;		//Time.time from when lostBalance
		IEnumerator lostBalanceIEnumerator;	//Coroutine for handling lostBalance
		private bool _lostBalance;			
		private bool LostBalance			//Get&Set for _lostBalance. Starts & stops coroutines on Set.
		{
			get { return _lostBalance; }
			set
			{
				if (value == true)
				{
					lostBalanceTime = Time.time;

					if (!_lostBalance)
					{
						_lostBalance = value;

						//Start new coroutine
						if (lostBalanceIEnumerator != null)
							StopCoroutine(lostBalanceIEnumerator);
						lostBalanceIEnumerator = LostBalanceRoutine();
						StartCoroutine(lostBalanceIEnumerator);
					}
					else
						_lostBalance = value;

				}
				else
					_lostBalance = value;


			}
		}

		private float rollingTime;			//Time.time from when rolled
		IEnumerator rollingIEnumerator;		//Coroutine for handling rolling
		private bool _rolling;
		private bool Rolling				//Get&Set for _lostBalance. Starts & stops coroutines on Set.
		{
			get { return _rolling; }
			set
			{
				
				if (value)
				{
					rollingTime = Time.time;

					if (!_rolling)
					{
						_rolling = value;
						//Start new coroutine
						if (rollingIEnumerator != null)
							StopCoroutine(rollingIEnumerator);
						rollingIEnumerator = RollingRoutine();
						StartCoroutine(rollingIEnumerator);
					}
					else
						_rolling = value;

				}
				else
					_rolling = value;


			}
		}				

		private Vector3 slopeNormal;		//Current ground normal
		private Vector3 spawnPosition;
		
		private float inputRunStartTime = 0;				//Time.time when run input started
		private Vector3 moveVelocityAtToggle;				//Used for accelerations when input happens/doesnt happen
		private float moveInputToggleTime;                  //Time.time when move input started or ended

		private Vector2 moveInputRaw;   //Value of current move input.
		//_______ End of Hidden variables

		#endregion Variables


		#region References

		private Player _player;
		private Player Player {
			get {
				if (!_player)
					_player = GetComponent<Player>();
				return _player;
			}
		}

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

		private RagdollScript _ragdoll;
		protected RagdollScript Ragdoll
		{
			get
			{
				if (!_ragdoll)
					_ragdoll = GetComponent<RagdollScript>();

				return _ragdoll;
			}
		}

		private Inputs _inputs;
		private Inputs Inputs
		{
			get 
			{ 
				if (_inputs == null)
					_inputs = new Inputs();
				return _inputs;

			}
		}

		private CharacterController _unityController;
		public CharacterController UnityController
		{
			get
			{
				if (!_unityController)
					_unityController = GetComponent<CharacterController>();

				return _unityController;
			}
		}

		private CameraController _cam;
		private CameraController Cam
		{
			get
			{
				if (!_cam)
				{
					var temp = Camera.main;
					if (temp)
						_cam = temp.transform.root.GetComponent<CameraController>();
				}
				return _cam;
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
				var dir = Quaternion.LookRotation(moveDir) * -Cam.GetCurrentFlatDirection();
				return dir.normalized;
			}
		}
		
		protected override void SetRunning(bool value)
		{
			if (Player.AllowRun())
			{
				base.SetRunning(value);

			}
			else
			{
				base.SetRunning(false);
			}
		}

		#endregion

		#region Tests & Checks & Calculations

		protected override void CheckGrounded() 
		{
			bool g = RaycastGrounded() || ControllerGrounded();
			IsGrounded = g;
		}

		/// <summary>
		/// Calculates the difference in y-position of playerController bottom-point and closest ground-point from it.
		/// </summary>
		/// <returns>Returns distance to ground. If point is above bottom, returns negative value.</returns>
		public float DistanceToGround()
		{
			Vector3 bottom = transform.position + UnityController.center + (Vector3.down * (UnityController.height/2 + UnityController.radius));

			Ray ray = new Ray();
			ray.direction = Vector3.down;
			ray.origin = transform.position + UnityController.center;
			RaycastHit[] hits = Physics.SphereCastAll(ray, UnityController.radius, 50f, groundLayerMask);

			if (hits.Length > 0)
			{
				Vector3 closestPoint = hits[0].point;
				float distance = bottom.y - closestPoint.y;

				//Find closest point
				for (int i = 0; i < hits.Length; i++)
				{
					if (closestPoint.y - hits[i].point.y < distance)
					{
						closestPoint = hits[i].point;
						distance = bottom.y - closestPoint.y;

					}
				}
				Debug.DrawLine(bottom, closestPoint, Color.blue);
				Debug.DrawRay(closestPoint, Vector3.down, Color.black);
				return distance;
			}
			else
			{ 
				Debug.Log("Dist to ground: 1000");

				return 1000f;
			}
		}
		/// <summary>
		/// Check if CharacterController thinks it is grounded.
		/// </summary>
		/// <returns></returns>
		bool ControllerGrounded() {
			return (UnityController.isGrounded || UnityController.collisionFlags.HasFlag(CollisionFlags.Below) || UnityController.collisionFlags.HasFlag(CollisionFlags.CollidedBelow));
		}
		
		/// <summary>
		/// Additional physics check to see if ground is near enough.
		/// </summary>
		/// <returns></returns>
		bool RaycastGrounded() 
		{
			float distance = UnityController.height * 0.5f + UnityController.skinWidth - UnityController.radius * 0.95f;
			//groundCheckPosition = transform.position + (Vector3.down * distance);
			bool check = Physics.SphereCastAll(transform.position + UnityController.center, UnityController.radius, Vector3.down, distance, groundLayerMask).Length > 0;
			return check;
		}

		/// <summary>
		/// Returns the normal of current ground.
		/// </summary>
		/// <returns></returns>
		Vector3 CheckSlopeNormal() 
		{
			Vector3 output = Vector3.up;

			if (DistanceToGround() > 0.25f)
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

		/// <summary>
		/// Can be used to calculate sideways movement path when targeting an enemy.
		/// </summary>
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

		protected override void Update()
		{
			base.Update();
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

		IEnumerator RollingRoutine()
		{
			float currentOffset = 0;
			float lastEvaluation = 0;
			bool rollStarted = false;
			while (Time.time - rollingTime < rollingDuration && Rolling)
			{
				if (LostBalance)
				{
					Rolling = false;
				}
				else if (DistanceToGround() > 0.1f)
				{
					if (!rollStarted)
						rollingTime = Time.time;
					else
						Rolling = false;
				}

				if (Rolling && DistanceToGround() < 0.1f)
				{
					PCombat.InterruptCombat(true);
					rollStarted = true;
					currentOffset = rollingDistance.Evaluate((Time.time - rollingTime)/rollingDuration) - lastEvaluation;
					lastEvaluation += currentOffset;
					ExternalMove(transform.forward * currentOffset);

					if (Time.time - rollingTime > rollingDuration)
						Rolling = false;
				}

				yield return null;
			}

			Rolling = false;
			yield return null;
		}

		IEnumerator LostBalanceRoutine()
		{
			if (!Ragdoll)
			{
				LostBalance = false;
				yield break;
			}

			bool gettingUp = false;
			float gettingUpTime = 0;

			while (!gettingUp)
			{
				if (!Ragdoll.IsRagdolling)
				{
					if (IsGrounded)
					{
						PCombat.InterruptCombat(true);
						Ragdoll.StartRagdoll();
					}
					else
					{
						lostBalanceTime = Time.time;
					}
				}
				else if (Ragdoll.IsMoving())
				{
					lostBalanceTime = Time.time;
				}
				if (Time.time - lostBalanceTime > getUpDelay && !gettingUp)
				{
					gettingUp = true;
					Ragdoll.EndRagdoll();
					SetRotationFromDirection(Ragdoll.GetDirection());
				}

				yield return null;
			}
			while (LostBalance)
			{
				if (Ragdoll.IsRagdolling)
				{
					gettingUpTime = Time.time;
				}
				else if (Time.time - gettingUpTime > getUpDuration)
				{
					moveSpeedMultiplier = 0;
					LostBalance = false;
				}

				yield return null;
			}
		}

		private void SetRotationFromDirection(Vector3 direction)
		{
			lookRotRaw = Quaternion.LookRotation(direction);
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
			if (Player.AllowMove() && moveInputRaw.sqrMagnitude > 0)
			{
				Vector3 newMoveSpeed = new Vector3(moveInputRaw.x, 0, moveInputRaw.y) * moveSpeed * (GetRunning() ? runSpeedMultiplier : 1f) * moveSpeedMultiplier;
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

			if (vSpeed < -fallSpeedToLoseBalance)
			{
				Rolling = false;
				LostBalance = true;
			}
			else if (vSpeed < -fallSpeedToRoll)
			{
				Rolling = true;
			}


			if (IsGrounded)
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
			if (IsGrounded || !keepMomentumInAir)
			{
				Vector3 newMoveOffset = Vector3.zero;

				if (PCombat.CurrentTarget != null)
				{
					//This calculation moves along circular path around target according to sideways input (moveInputRaw.x)
					Vector2 pos = new Vector2(transform.position.x, transform.position.z);
					Vector2 targetPos = new Vector2(PCombat.CurrentTarget.GetPosition().x, PCombat.CurrentTarget.GetPosition().z);

					float currentAngle = Vector3.SignedAngle(Vector3.forward, -PCombat.GetFlatDirectionToTarget(), Vector3.up);
					Vector2 newPosWithSidewaysOffset = MovePointAlongCircle(currentAngle, pos,targetPos, -currentMoveSpeed.x * Time.deltaTime);

					//Apply sideways inputs to transform position
					if (!float.IsNaN(newPosWithSidewaysOffset.x) && !float.IsNaN(newPosWithSidewaysOffset.y)) // Apparently this happens too
						newMoveOffset += new Vector3(newPosWithSidewaysOffset.x, transform.position.y, newPosWithSidewaysOffset.y) - transform.position;

					//Add forward input and apply to transform position as well
					Quaternion rot = Quaternion.LookRotation(PCombat.GetFlatDirectionToTarget());
					Vector3 forwardMoveDir = rot * new Vector3(0, 0, currentMoveSpeed.z);
					newMoveOffset += forwardMoveDir * Time.deltaTime;

					//Prevent from going too close. This might become problematic for combat but we'll see.
					float flatDistanceToTarget = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(PCombat.CurrentTarget.GetPosition().x, 0, PCombat.CurrentTarget.GetPosition().z));
					if (flatDistanceToTarget < 0.5f)
					{
						Vector3 newPos = new Vector3(PCombat.CurrentTarget.GetPosition().x, transform.position.y, PCombat.CurrentTarget.GetPosition().z) - PCombat.GetFlatDirectionToTarget().normalized*0.5f;
						newMoveOffset += newPos - transform.position;
					}
				}
				else
				{

					Quaternion rot = Quaternion.LookRotation(-Cam.GetCurrentFlatDirection());
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

			transform.rotation = Quaternion.Slerp(transform.rotation, lookRotRaw, Time.deltaTime * rotationSpeed * rotationSpeedMultiplier);
			
		}

		private void UpdateLookRotRaw()
		{

			if (Player.AllowRotate())
			{
				if (PCombat.CurrentTarget != null)
				{
					var lookdir = PCombat.CurrentTarget.GetPosition() - transform.position;
					lookdir.y = 0;
					lookRotRaw = Quaternion.LookRotation(lookdir);
				}
				else/* if (GetTransformedInputDirection().sqrMagnitude > 0)*/
				{
					var lookdir = GetTransformedInputDirection();
					lookdir.y = 0;
					if (lookdir != Vector3.zero)
						lookRotRaw = Quaternion.LookRotation(lookdir);
				}
			}

		}

		void MoveAlongSlope()
		{
			if (UnityController && IsGrounded && currentMoveOffset.magnitude > 0)
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
			if (Player.AllowMove())
			{
				if (currentMoveOffset.sqrMagnitude > 0)
					lastNonZeroMoveDirection = currentMoveOffset.normalized;
				else if (moveInputRaw.sqrMagnitude > 0)
					lastNonZeroMoveDirection = GetTransformedInputDirection();

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
		public override void ExternalMove(Vector3 offset)
		{
			UnityController.Move(offset);
		}
		

		#endregion

		#region HandleInputs



		void ControlsSubscribe()
		{
			Inputs.Player.Move.started += InputMoveStarted;
			Inputs.Player.Move.performed += InputMovePerformed;
			Inputs.Player.Move.canceled += InputMoveCancelled;
			Inputs.Player.Move.Enable();

			Inputs.Player.RunAndDodge.started += InputRunStarted;
			Inputs.Player.RunAndDodge.performed += InputRunPerformed;
			Inputs.Player.RunAndDodge.canceled += InputRunCancelled;
			Inputs.Player.RunAndDodge.Enable();
		}

		void ControlsUnsubscribe()
		{
			Inputs.Player.Move.started += InputMoveStarted;
			Inputs.Player.Move.performed -= InputMovePerformed;
			Inputs.Player.Move.canceled -= InputMoveCancelled;
			Inputs.Player.Move.Disable();


			Inputs.Player.RunAndDodge.started -= InputRunStarted;
			Inputs.Player.RunAndDodge.performed -= InputRunPerformed;
			Inputs.Player.RunAndDodge.canceled -= InputRunCancelled;
			Inputs.Player.RunAndDodge.Disable();
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

			if (Time.time - inputRunStartTime > Player.inputSinglePressMaxTime)
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


		void UpdateAnimationData()
		{
			float movePercentage = currentMoveSpeed.magnitude / GetMaxSpeed();
			float angle = Vector3.SignedAngle(transform.forward, Vector3.forward, Vector3.up);
			Vector3 relativeMoveDirection = Quaternion.Euler(0, angle, 0) * GetFlatMoveDirection();
			Vector2 blend = new Vector2(relativeMoveDirection.x, relativeMoveDirection.z).normalized * movePercentage;

			PAnimation.SetMovementPerformed(moveInputRaw.sqrMagnitude>0 && Player.AllowMove(), blend);
			PAnimation.SetGrounded(DistanceToGround() < 0.4f);
			PAnimation.LostBalance(LostBalance);
			PAnimation.Rolling(Rolling);

		}

		#endregion

		#region Debug


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
			bool output = true;
			output = LostBalance ? false : output;
			output = Rolling ? false : output;
			return output;
		}

		public bool AllowRun() 
		{
			bool output = true;
			output = LostBalance ? false : output;
			output = Rolling ? false : output;
			return output;
		}
		public bool AllowRotate()
		{
			bool output = true;
			output = LostBalance ? false : output;
			output = Rolling ? false : output;
			return output;
		}
		public bool AllowAttack() 
		{
			bool output = true;
			output = LostBalance ? false : output;
			return output;
		}

		public bool AllowDodge() 
		{
			bool output = true;

			output = LostBalance ? false : output;
			output = Rolling ? false : output;

			return output;
		}

		#endregion
	}
}
