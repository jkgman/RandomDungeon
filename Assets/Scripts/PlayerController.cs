using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Input;


public class PlayerController : MonoBehaviour
{
	public Transform debugTarget;
	[SerializeField] private GameObject targetIndicatorPrefab;
	[SerializeField] private float maxDistToTarget = 10f;
	[SerializeField] private Transform _target; //Temp for testing
	[SerializeField] private float moveSpeed = 15f;
	[SerializeField] private float rotationSpeed = 10f;

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

	public void SetTarget(Transform in_traget)
	{

	}
	public bool SetTarget()
	{
		if (debugTarget)
		{
			Target = Target == null ? debugTarget : null;
		}
		return true; //Return true if target changed in any way
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
