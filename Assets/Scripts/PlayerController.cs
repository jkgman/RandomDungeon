using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Experimental.Input;
using System.Collections;

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

	
	[Header("Target crap")]
	
	[SerializeField] private GameObject targetIndicatorPrefab;
	[SerializeField] private float maxDistToTarget = 10f;
	[SerializeField] private LayerMask targetLayerMask;
	[SerializeField] private LayerMask blockTargetLayerMask;

	private GameObject targetIndicator;
	public UnityEvent targetChangedEvent;

	
	[Header("Combat shit")]

	[SerializeField] private Weapon currentWeapon;

	private bool _isBlocking;
	private bool _isStunned;
	private bool _isAttacking;



    private Inputs inputs;
	private Vector2 moveInputRaw;



	#region Getters & Setters

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

	private CameraController _cam;
	private CameraController Cam
	{
		get
		{
			if (!_cam)
			{
				_cam = Camera.main.GetComponentInParent<CameraController>();
			}

			return _cam;
		}
	}

	private Transform _target;
	public Transform Target
	{
		get { return _target; }
		set
		{
			if (value != _target)
				targetChangedEvent.Invoke();
			
			_target = value;
		}
	}

	public float GetMaxDistToTarget
	{
		get { return maxDistToTarget; }
	}
	
	public Vector3 GetPos
	{
		get{ return transform.position; }
	}

	Vector3 GetFlatMoveDirection()
	{
		var lookDir = new Vector3(moveInputRaw.x, 0, moveInputRaw.y);
		var dir = Quaternion.LookRotation(lookDir) * -Cam.GetCurrentFlatDirection();
		return dir.normalized;
	}


	#endregion


	void OnEnable()
    {
		
		if (targetChangedEvent == null)
			targetChangedEvent = new UnityEvent();

		if (Cam && Cam.CameraTransformsUpdated != null)
			Cam.CameraTransformsUpdated.AddListener(CameraReadyEvent);
		
        ControlsSubscribe();
    }
    void OnDisable()
    {
		targetChangedEvent = null;

		if (Cam && Cam.CameraTransformsUpdated != null)
			Cam.CameraTransformsUpdated.RemoveListener(CameraReadyEvent);

		ControlsUnsubscribe();
    }


	void Update()
	{
		if (!Controller)
			return;

		SetBooleans();
		UpdateTarget();

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

	void SetBooleans()
	{
		updateGizmos = true;

		isGrounded = CheckGrounded();
		slopeNormal = CheckSlopeNormal();
	}

	//Is called after camera has updated its transform.
	void CameraReadyEvent()
	{
		// Target indicator needs the latest camera position, otherwise looks bad at low fps.
		SetTargetIndicator();
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

	bool CheckGrounded()
	{
		bool g = RaycastGrounded() || ControllerGrounded();
		return g;
	}

	bool ControllerGrounded()
	{
		return (Controller.isGrounded || Controller.collisionFlags.HasFlag(CollisionFlags.Below) || Controller.collisionFlags.HasFlag(CollisionFlags.CollidedBelow));
	}

	//Vector3 groundCheckPosition = Vector3.zero;
	bool RaycastGrounded()
	{
		float distance = Controller.height*0.5f + Controller.skinWidth - Controller.radius*0.9f;
		//groundCheckPosition = transform.position + (Vector3.down * distance);
		bool check1 = Physics.SphereCastAll(transform.position, Controller.radius, Vector3.down, distance, groundLayerMask).Length > 0;
		//bool check2 = Physics.CheckSphere(groundCheckPosition, Controller.radius, groundLayerMask.value);
		//bool check3 = Physics.Raycast(transform.position + Controller.center, Vector3.down, Controller.height / 2f + Controller.skinWidth, groundLayerMask.value);
		//Debug.Log("Ground checks: " + check1 + ", " + check2 + ", " + check3);
		return check1;
		//return Physics.Raycast(transform.position + Controller.center, Vector3.down, Controller.height/2f + Controller.skinWidth, groundLayerMask.value);
	}

	Vector3 CheckSlopeNormal()
	{
		Vector3 output = Vector3.up;

		if (!isGrounded)
			return output;

		float distance = Controller.height;

		//First spherecast finds the first point that has collided with player
		//Sometimes the point is a corner of something which causes the normal to be fucked
		//Fix was to make a raycast right in front of the point that spherecast found and take the normal from there.

		RaycastHit hit;
		Physics.SphereCast(transform.position + Controller.center, Controller.radius, Vector3.down, out hit, distance, groundLayerMask.value);
		if (hit.collider)
		{
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

	void Move()
	{
		Vector3 newMoveOffset = Vector3.zero;
		if (Target)
		{
			//This calculation moves along circular path around target according to sideways input (moveInputRaw.x)
			Vector2 pos = new Vector2(transform.position.x, transform.position.z);
			Vector2 targetPos = new Vector2(Target.transform.position.x, Target.transform.position.z);

			float currentAngle = Vector3.SignedAngle(Vector3.forward, -GetFlatDirectionToTarget(), Vector3.up);
			Vector2 newPosWithSidewaysOffset = MoveAlongCircle(currentAngle, pos,targetPos, -moveInputRaw.x * moveSpeed * Time.deltaTime);

			//Apply sideways inputs to transform position
			if (!float.IsNaN(newPosWithSidewaysOffset.x) && !float.IsNaN(newPosWithSidewaysOffset.y)) // Apparently this happens too
				newMoveOffset += new Vector3(newPosWithSidewaysOffset.x, transform.position.y, newPosWithSidewaysOffset.y) - transform.position;

			//Add forward input and apply to transform position as well
			Quaternion rot = Quaternion.LookRotation(GetFlatDirectionToTarget());
			Vector3 forwardMoveDir = rot * new Vector3(0, 0, moveInputRaw.y);
			newMoveOffset += forwardMoveDir * moveSpeed * Time.deltaTime;

			//Prevent from going too close. This might become problematic for combat but we'll see.
			float flatDistanceToTarget = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(Target.transform.position.x, 0, Target.transform.position.z));
			if (flatDistanceToTarget < 0.5f)
			{
				Vector3 newPos = new Vector3(Target.transform.position.x, transform.position.y, Target.transform.position.z) - GetFlatDirectionToTarget().normalized*0.5f;
				newMoveOffset += newPos - transform.position;
			}
		}
		else
		{

			Quaternion rot = Quaternion.LookRotation(-Cam.GetCurrentFlatDirection());
			Vector3 realMoveDirection = rot * new Vector3(moveInputRaw.x, 0, moveInputRaw.y);
			newMoveOffset += realMoveDirection * moveSpeed * Time.deltaTime;
		}

		newMoveOffset.y = 0;
		currentMoveOffset = newMoveOffset;
	}

	void Rotate()
	{
		if (Target)
		{
			var lookpos = Target.transform.position - transform.position;
			lookpos.y = 0;
			var rot = Quaternion.LookRotation(lookpos);
			transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * rotationSpeed);
		}
		else
		{
			if (moveInputRaw != Vector2.zero)
			{
				var lookDir = new Vector3(moveInputRaw.x, 0, moveInputRaw.y);
				var dir = Quaternion.LookRotation(lookDir) * -Cam.GetCurrentFlatDirection();
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSpeed);
			}
		}
	}


	Vector3 relativeRight = Vector3.zero; //TEMP
	Vector3 newForward = Vector3.zero;
	void MoveAlongSlope()
	{
		if (isGrounded && currentMoveOffset.magnitude > 0)
		{
			
			relativeRight = Vector3.Cross(currentMoveOffset, Vector3.up);
			newForward = Vector3.Cross(slopeNormal, relativeRight);
			
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




	Vector3 GetFlatDirectionToTarget()
	{
		if (Target)
		{
			var dirToTarget = Target.transform.position - transform.position;
			dirToTarget.y = 0;
			return dirToTarget.normalized;
		}
		else
		{
			return -Cam.GetCurrentFlatDirection();
		}
	}
	Vector2 MoveAlongCircle(float currentAngle, Vector2 currentPoint, Vector2 centerPoint, float distance)
	{
		var r = Vector2.Distance(currentPoint, centerPoint);
		var a1 = currentAngle * (Mathf.PI / 180);
		var a2 = a1 + distance / r;
		var p2 = Vector2.zero;
		p2.x = centerPoint.x + r * Mathf.Sin(a2);
		p2.y = centerPoint.y + r * Mathf.Cos(a2);
		return p2;
	}



	#region Targeting

	void UpdateTarget()
	{
		if (Target && (Target.position - transform.position).magnitude > maxDistToTarget)
		{
			Target = null;
		}
	}

	void SetTargetIndicator()
	{
		if (!Target || !Cam) // Indicator relies on camera
		{
			if (targetIndicator)
				Destroy(targetIndicator);
		} else
		{
			if (!targetIndicator)
			{
				if (targetIndicatorPrefab)
					targetIndicator = Instantiate(targetIndicatorPrefab, Target.transform.position, Quaternion.LookRotation(Cam.transform.position - Target.transform.position));
			} else
			{
				targetIndicator.transform.position = Target.transform.position;
				targetIndicator.transform.rotation = Quaternion.LookRotation(Cam.GetCurrentDirection());
			}
		}

	}

	public bool SetTarget(CameraTargetingData data)
	{
		Transform oldTarget = Target;
		
		if (Target != null)
		{
			Target = null;
			return true;
		}

		Transform[] nearbyTargets = FindTargets();
		if (nearbyTargets != null && nearbyTargets.Length > 0)
			Target = FindBestTarget(data, nearbyTargets);

		return Target != oldTarget; //Return true if target changed in any way
	}

	Transform[] FindTargets()
	{
		//Sphere check on all nearby enemies.
		//Adds objects with Enemy script in temp list
		//Converts list into output array.

		List<Transform> temp = new List<Transform>();
		Collider[] cols = Physics.OverlapSphere(transform.position, maxDistToTarget, targetLayerMask.value);
		if (cols != null && cols.Length > 0)
		{
			for (int i = 0; i < cols.Length; i++)
			{
				var enemy = cols[i].GetComponent<Enemy>();
				if (enemy && enemy.CanBeTargeted() && !temp.Contains(enemy.transform))
				{
					temp.Add(enemy.transform);
				}
			}
		}
		Transform[] output = new Transform[temp.Count];
		for (int i = 0; i < temp.Count; i++)
		{
			output[i] = temp[i];
		}
		
		return output;
	}

	Transform FindBestTarget(CameraTargetingData camData, Transform[] allTheBoisToBeTargeted)
	{
		Transform output = null;
		float currentBestAngle = -1;

		for (int i = 0; i < allTheBoisToBeTargeted.Length; i++)
		{
			float angle = Vector3.Angle(camData.forward, (allTheBoisToBeTargeted[i].position - camData.position));
			bool inView = angle < camData.fov*0.65f;
			bool farEnoughFromCamera = Vector3.Distance(camData.position, allTheBoisToBeTargeted[i].position) > 3f;
			if (inView && farEnoughFromCamera)
			{
				//Check if target is visible in camera.
				RaycastHit hit;
				Physics.Raycast(camData.position, (allTheBoisToBeTargeted[i].position - camData.position).normalized, out hit, maxDistToTarget, blockTargetLayerMask.value);
				if (!hit.collider || hit.collider.transform == allTheBoisToBeTargeted[i])
				{
					//Check that the angle is better than currentBest.
					if (currentBestAngle < 0 || angle < currentBestAngle)
					{
						//Its the fucking best so far so assign that mf to output.
						output = allTheBoisToBeTargeted[i];
						currentBestAngle = angle;
					}
				}
			}
		}
		return output;
	}

	void SwitchTarget(int direction)
	{
		Transform[] allTheBoisToBeTargeted = FindTargets();
		CameraTargetingData camData = Cam.GetTargetingData();

		Transform newTarget = null;
		float currentBestAngle = -1;

		for (int i = 0; i < allTheBoisToBeTargeted.Length; i++)
		{
			Vector3 dirToBoi = (allTheBoisToBeTargeted[i].position - transform.position);
			dirToBoi.y = 0;
			dirToBoi.Normalize();
			float angle = Vector3.SignedAngle(GetFlatDirectionToTarget(), dirToBoi, Vector3.up);
			bool inView = Mathf.Abs(angle) < 100f;
			bool farEnoughFromCamera = Vector3.Distance(camData.position, allTheBoisToBeTargeted[i].position) > 3f;
			bool correctSide = (direction > 0 && angle > 0) || (direction < 0 && angle < 0);
			if (inView && farEnoughFromCamera && correctSide)
			{
				//Check if target is visible in camera.
				RaycastHit hit;
				Physics.Raycast(camData.position, (allTheBoisToBeTargeted[i].position - camData.position).normalized, out hit, maxDistToTarget, blockTargetLayerMask.value);
				Debug.Log(hit.collider);
				if (!hit.collider || hit.collider.transform == allTheBoisToBeTargeted[i])
				{
					//Check that the angle is better than currentBest.
					if (currentBestAngle < 0 || Mathf.Abs(angle) < currentBestAngle)
					{
						//Its the fucking best so far so assign that mf to output.
						newTarget = allTheBoisToBeTargeted[i];
						currentBestAngle = Mathf.Abs(angle);
					}
				}
			}
		}

		if (newTarget)
			Target = newTarget;
			
	}

	#endregion

	#region HandleControls


	void ControlsSubscribe()
    {
        if (inputs == null)
            inputs = new Inputs();

        inputs.Player.Move.performed += InputMove;
		inputs.Player.Move.Enable();
		inputs.Player.TargetLock.performed += InputTargetLock;
		inputs.Player.TargetLock.Enable();
		inputs.Player.SwitchTarget.started += InputTargetSwitchStarted;
		inputs.Player.SwitchTarget.Enable();
	}
	void ControlsUnsubscribe()
    {
        inputs.Player.Move.performed -= InputMove;
		inputs.Player.Move.Disable();
		inputs.Player.TargetLock.performed -= InputTargetLock;
		inputs.Player.TargetLock.Disable();
		inputs.Player.SwitchTarget.started -= InputTargetSwitchStarted;
		inputs.Player.SwitchTarget.Disable();
	}

	void InputTargetLock(InputAction.CallbackContext context) {

		bool success = SetTarget(Cam.GetTargetingData());
		if (!success)
			Cam.ResetCamera();

	}

	float inputTargetSwitchTime = 0;
	float inputTargetSwitchInterval = 0.1f;
	void InputTargetSwitchStarted(InputAction.CallbackContext context) 
	{
		if (!Target)
			return;

		if (Time.time - inputTargetSwitchTime < inputTargetSwitchInterval)
			return;
		else
			inputTargetSwitchTime = Time.time;
		

		//NOTE
		//Input system currently bugged with scroll wheel, will be fixed in next update probably.
		//Causes mouse movement to register as scrolling, at least sometimes


		int targetSwitchDirection = 0;
		
		if (context.control.layout == "Stick")
		{
			targetSwitchDirection = context.ReadValue<Vector2>().x > 0 ? 1 : -1;
		}
		else
		{
			//Downwards is +1 (right)
			targetSwitchDirection = context.ReadValue<Vector2>().y > 0 ? -1 : 1;
		}
		
		SwitchTarget(targetSwitchDirection);

	}

	void InputMove(InputAction.CallbackContext context)
    {
        moveInputRaw = context.ReadValue<Vector2>();

    }
	void InputJump(InputAction.CallbackContext context)
	{
	
	}
	void InputLook(InputAction.CallbackContext context)
	{

	}


	void Dodge(InputAction.CallbackContext context)
	{

	}
	void Block(InputAction.CallbackContext context)
	{

	}
	void Attack(InputAction.CallbackContext context)
	{

	}
	void ChargeAttack(InputAction.CallbackContext context)
	{

	}


	#endregion


	Vector3[] guiPositions = new Vector3[200];
	int guiPosIndex = 0;
	bool updateGizmos = false;

	void OnDrawGizmos()
	{
		if (!debugVisuals)
			return;
		Gizmos.color = Color.red;
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




		Gizmos.DrawLine(transform.position, transform.position + relativeRight.normalized);
		Gizmos.DrawRay(transform.position, newForward.normalized*3f);
		Gizmos.DrawRay(transform.position, Vector3.down * Controller.height * 0.5f);
		//Gizmos.DrawWireSphere(groundCheckPosition, Controller.radius);

		updateGizmos = false;
	}

}
