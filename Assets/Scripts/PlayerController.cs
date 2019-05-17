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
		[SerializeField] private float rotationSpeed = 10f;
		[SerializeField] private LayerMask groundLayerMask;

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
			if (!Controller)
				return;

			UpdateBooleans();

			if (!(!isGrounded && keepMomentumInAir))
			{
				currentMoveOffset = Vector3.zero;
				Move();
			}

			Rotate();
			ApplyGravity();
			MoveAlongSlope();
			ApplyMovementToController();

			ResetInputModifiers();

		}


		void UpdateBooleans()
		{
			updateGizmos = true;

			isGrounded = CheckGrounded();
			slopeNormal = CheckSlopeNormal();
		}

		#endregion

		#region Movement

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

		void Move()
		{
			Vector3 newMoveOffset = Vector3.zero;
			if (PManager.PCombat.Target)
			{
				//This calculation moves along circular path around target according to sideways input (moveInputRaw.x)
				Vector2 pos = new Vector2(transform.position.x, transform.position.z);
				Vector2 targetPos = new Vector2(PManager.PCombat.Target.transform.position.x, PManager.PCombat.Target.transform.position.z);

				float currentAngle = Vector3.SignedAngle(Vector3.forward, -PManager.PCombat.GetFlatDirectionToTarget(), Vector3.up);
				Vector2 newPosWithSidewaysOffset = MovePointAlongCircle(currentAngle, pos,targetPos, -moveInputRaw.x * moveSpeed * Time.deltaTime);

				//Apply sideways inputs to transform position
				if (!float.IsNaN(newPosWithSidewaysOffset.x) && !float.IsNaN(newPosWithSidewaysOffset.y)) // Apparently this happens too
					newMoveOffset += new Vector3(newPosWithSidewaysOffset.x, transform.position.y, newPosWithSidewaysOffset.y) - transform.position;

				//Add forward input and apply to transform position as well
				Quaternion rot = Quaternion.LookRotation(PManager.PCombat.GetFlatDirectionToTarget());
				Vector3 forwardMoveDir = rot * new Vector3(0, 0, moveInputRaw.y);
				newMoveOffset += forwardMoveDir * moveSpeed * Time.deltaTime;

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

				Quaternion rot = Quaternion.LookRotation(-PManager.GetCam.GetCurrentFlatDirection());
				Vector3 realMoveDirection = rot * new Vector3(moveInputRaw.x, 0, moveInputRaw.y);
				newMoveOffset += realMoveDirection * moveSpeed * Time.deltaTime;
			}

			newMoveOffset.y = 0;
			currentMoveOffset = newMoveOffset;
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
				if (moveInputRaw != Vector2.zero)
				{
					var lookDir = new Vector3(moveInputRaw.x, 0, moveInputRaw.y);
					var dir = Quaternion.LookRotation(lookDir) * -PManager.GetCam.GetCurrentFlatDirection();
					transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSpeed);
				}
			}
		}

		void MoveAlongSlope()
		{
			if (isGrounded && currentMoveOffset.magnitude > 0)
			{
			
				Vector3 relativeRight = Vector3.Cross(currentMoveOffset, Vector3.up);
				Vector3 newForward = Vector3.Cross(slopeNormal, relativeRight);
			
				if (newForward.magnitude > 0)
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
			Controller.Move(currentMoveOffset);
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

			inputs.Player.Move.performed += InputMove;
			inputs.Player.Move.Enable();
		}

		void ControlsUnsubscribe()
		{
			inputs.Player.Move.performed -= InputMove;
			inputs.Player.Move.Disable();
		}



		void InputMove(InputAction.CallbackContext context)
		{
			moveInputRaw = context.ReadValue<Vector2>();

		}



		#endregion


		#region Debug
		
		bool updateGizmos = false;

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


			updateGizmos = false;
		}

		#endregion

	}
}
