using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Input;


public class PlayerController : MonoBehaviour
{
	public Transform debugTarget;
	[SerializeField] private float maxDistToTarget = 10f;
	[SerializeField] private Transform _target; //Temp for testing
	[SerializeField] private float moveSpeed = 15f;

	private Vector2 moveDirectionRaw;
	private Vector2 velocityModifier;

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

	}

	void UpdateTarget()
	{
		if (Target && (Target.position - transform.position).magnitude > maxDistToTarget)
		{
			Target = null;
		}
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
