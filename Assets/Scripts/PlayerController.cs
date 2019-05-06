using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Experimental.Input;


public class PlayerController : MonoBehaviour
{
	public Transform debugTarget;
	[SerializeField] private GameObject targetIndicatorPrefab;
	[SerializeField] private float moveSpeed = 15f;
	[SerializeField] private float rotationSpeed = 10f;

	[Header("Target stuff")]
	[SerializeField] private float maxDistToTarget = 10f;
	[SerializeField] private LayerMask targetLayerMask;
	[SerializeField] private LayerMask blockTargetLayerMask;



	private Vector2 moveDirectionRaw;
	private GameObject targetIndicator;


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

	private bool _isRunning;
	private bool _isGrounded;
	private bool _isBlocking;
	private bool _isStunned;
	private bool _isAttacking;

	public UnityEvent targetChangedEvent;

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

    private Inputs inputs;

    void OnEnable()
    {
		if (targetChangedEvent == null)
			targetChangedEvent = new UnityEvent();
		
        ControlsSubscribe();
    }
    void OnDisable()
    {
        ControlsUnsubscribe();
    }


	void Update()
	{
		UpdateTarget();
		Move();
		Rotate();
		ResetInputModifiers();

	}

	void UpdateTarget()
	{
		if (Target && (Target.position - transform.position).magnitude > maxDistToTarget)
		{
			Target = null;
		}

		// Start of Indicator
		if (!Target)
		{
			if (targetIndicator)
				Destroy(targetIndicator);
		} 
		else
		{
			if (!Cam) // Indicator relies on camera
				return;

			if (!targetIndicator)
			{
				if (targetIndicatorPrefab)
					targetIndicator = Instantiate(targetIndicatorPrefab, Target.transform.position, Quaternion.LookRotation(Cam.transform.position - Target.transform.position));
			} 
			else
			{
				targetIndicator.transform.position = Target.transform.position;
				targetIndicator.transform.rotation = Quaternion.LookRotation(Cam.transform.position - Target.transform.position);
			}
		}
		// End of Indicator
	}

	
	public bool SetTarget(CameraTargetingData data)
	{
		Transform oldTarget = Target;
		
		if (Target != null)
		{
			Target = null;
			return true;
		}
		if (debugTarget)
		{
			Target = debugTarget;
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
		//TODO
		//Find a target thats closest to the middle of the current view.
		//Target must not be blocked from raycast.

		Transform output = null;
		float currentBestAngle = -1;
		Debug.Log("alltheboys: " + allTheBoisToBeTargeted.Length);

		for (int i = 0; i < allTheBoisToBeTargeted.Length; i++)
		{
			float angle = Vector3.Angle(camData.forward, (allTheBoisToBeTargeted[i].position - camData.position));
			bool inView = angle < camData.fov*0.65f;
			if (inView)
			{
				Debug.Log("inView = true");
				//Check if target is visible in camera.
				RaycastHit hit;
				Physics.Raycast(camData.position, (allTheBoisToBeTargeted[i].position - camData.position).normalized, out hit, maxDistToTarget, blockTargetLayerMask.value);
				Debug.Log(hit.collider);
				if (!hit.collider || hit.collider.transform == allTheBoisToBeTargeted[i])
				{
					Debug.Log("raycast succeeded");
					//Check that the angle is better than currentBest.
					if (currentBestAngle < 0 || angle < currentBestAngle)
					{
						//Its the fucking best so far so assign that mf to output.
						Debug.Log("whole shit succeeded");
						output = allTheBoisToBeTargeted[i];
						currentBestAngle = angle;
					}
				}
			}
		}

		

		return output;
	}
	

	void Move()
	{
		Quaternion rot = Quaternion.Euler(0, Cam.CurAngle.y, 0);
		Vector3 realMoveDirection = rot * new Vector3(-moveDirectionRaw.x, 0, -moveDirectionRaw.y);
		transform.position += realMoveDirection * moveSpeed * Time.deltaTime;
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
			if (moveDirectionRaw != Vector2.zero)
			{
				var lookpos = new Vector3(moveDirectionRaw.x, 0, moveDirectionRaw.y);
				var dir = Quaternion.LookRotation(lookpos) * -Cam.GetCurrentFlatDirection();
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSpeed);
			}
		}
	}

	void ResetInputModifiers()
	{
		//These should be zero for next frame in case no input was given
		moveDirectionRaw = Vector3.zero;

	}

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


	#region HandleControls

	void InputMove(InputAction.CallbackContext context)
    {
        moveDirectionRaw = context.ReadValue<Vector2>();

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

}
