using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Experimental.Input;
using System.Collections;

namespace Dungeon.Player
{
	public class PlayerController : MonoBehaviour
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
		private Vector3 currentMoveVelocity;
		private Vector3 moveVelocityAtToggle;
		private Vector3 currentForward;

		private Vector3 currentMoveOffset;
		private float vSpeed = 0;
		private bool _isRunning;
		private bool isGrounded;
		private Vector3 slopeNormal;

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

		public float GetMaxSpeed()
		{
			return moveSpeed * runSpeedMultiplier;
		}

		Vector3 GetFlatMoveDirection()
		{
			var lookDir = new Vector3(moveInputRaw.x, 0, moveInputRaw.y);
			var dir = Quaternion.LookRotation(lookDir) * -PManager.GetCam.GetCurrentFlatDirection();
			return dir.normalized;
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

				RaycastHit[] hits = Physics.RaycastAll(hit.point + GetFlatMoveDirection() * 0.05f + Vector3.up, Vector3.down, 2f, groundLayerMask.value);
				for (int i = 0; i < hits.Length; i++)
				{
					if (hits[i].collider == hit.collider)
					{
						output = hits[i].normal;
						break;
					}
				}
			}

			Debug.Log("No slope");

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
			ApplyMovementToController();	//Gives movement to Unity Character Controller
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
			if (moveInputRaw.magnitude > 0)
			{
				Vector3 newMoveSpeed = new Vector3(moveInputRaw.x, 0, moveInputRaw.y) * moveSpeed * (_isRunning ? runSpeedMultiplier : 1f);
				currentMoveVelocity = Vector3.Lerp(moveVelocityAtToggle, newMoveSpeed, (Time.time - moveInputToggleTime) / accelerationSpeed);
			}
			else
			{
				currentMoveVelocity = Vector3.Lerp(moveVelocityAtToggle, Vector3.zero, (Time.time - moveInputToggleTime) / deaccelerationSpeed);

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
					Vector2 newPosWithSidewaysOffset = MovePointAlongCircle(currentAngle, pos,targetPos, -currentMoveVelocity.x * Time.deltaTime);

					//Apply sideways inputs to transform position
					if (!float.IsNaN(newPosWithSidewaysOffset.x) && !float.IsNaN(newPosWithSidewaysOffset.y)) // Apparently this happens too
						newMoveOffset += new Vector3(newPosWithSidewaysOffset.x, transform.position.y, newPosWithSidewaysOffset.y) - transform.position;

					//Add forward input and apply to transform position as well
					Quaternion rot = Quaternion.LookRotation(PManager.PCombat.GetFlatDirectionToTarget());
					Vector3 forwardMoveDir = rot * new Vector3(0, 0, currentMoveVelocity.z);
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
					Vector3 realMoveDirection = rot * new Vector3(currentMoveVelocity.x, 0, currentMoveVelocity.z);
					newMoveOffset += realMoveDirection * Time.deltaTime;
				}
	
				newMoveOffset.y = 0;
				currentMoveOffset = newMoveOffset;
			}

		}

		void Rotate()
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
				if (currentMoveVelocity.magnitude > 0)
				{
					var lookDir = new Vector3(currentMoveVelocity.x, 0, currentMoveVelocity.z);
					var dir = Quaternion.LookRotation(lookDir) * currentForward;
					transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSpeed);
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


		void ResetInputModifiers()
		{
			//These should be zero for next frame in case no input was given
			moveInputRaw = Vector3.zero;
		}

		#endregion

		#region HandleInputs

		float inputTargetSwitchTime = 0;
		readonly float inputTargetSwitchInterval = 0.1f;


		void ControlsSubscribe()
		{
			if (inputs == null)
				inputs = new Inputs();

			inputs.Player.Move.started += InputMoveStarted;
			inputs.Player.Move.performed += InputMovePerformed;
			inputs.Player.Move.cancelled += InputMoveCancelled;
			inputs.Player.Move.Enable();
		}

		void ControlsUnsubscribe()
		{
			inputs.Player.Move.started += InputMoveStarted;
			inputs.Player.Move.performed -= InputMovePerformed;
			inputs.Player.Move.cancelled -= InputMoveCancelled;
			inputs.Player.Move.Disable();
		}



		void InputMoveStarted(InputAction.CallbackContext context) {
			moveInputRaw = context.ReadValue<Vector2>();
			moveInputToggleTime = Time.time;
			moveVelocityAtToggle = currentMoveVelocity;
			Debug.Log("InputMoveStarted");

		}

		void InputMovePerformed(InputAction.CallbackContext context) {
			moveInputRaw = context.ReadValue<Vector2>();
			Debug.Log("InputMovePerformed");

		}

		void InputMoveCancelled(InputAction.CallbackContext context) 
		{
			moveInputRaw = Vector2.zero;
			moveInputToggleTime = Time.time;
			moveVelocityAtToggle = currentMoveVelocity;
			Debug.Log("InputMoveCancelled");

		}



		#endregion

		#region Animations

		Animator Anim {
			get { return GetComponentInChildren<Animator>(); }
		}

		void UpdateAnimationData()
		{
			Anim.SetBool("move", moveInputRaw != Vector2.zero);

			if (moveInputRaw.magnitude == 0)
			{
				Anim.SetFloat("sidewaysMove", Mathf.Lerp(Anim.GetFloat("sidewaysMove"), moveInputRaw.x, Time.deltaTime));
				Anim.SetFloat("forwardMove", Mathf.Lerp(Anim.GetFloat("forwardMove"), moveInputRaw.y, Time.deltaTime));
			}
			else
			{
				Anim.SetFloat("sidewaysMove", moveInputRaw.x);
				Anim.SetFloat("forwardMove", moveInputRaw.y);
			}

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

	}
}
