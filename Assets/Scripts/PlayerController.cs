using UnityEngine;
using UnityEngine.Experimental.Input;


public class PlayerController : MonoBehaviour
{
	[SerializeField] private float maxDistToTarget = 10f;
	[SerializeField] private Transform _target; //Temp for testing

	private Vector2 moveDirection;
	private Vector2 velocityModifier;


	private bool _isRunning;
	private bool _isGrounded;
	private bool _isBlocking;
	private bool _isStunned;
	private bool _isAttacking;

	public Transform Target
	{
		get { return _target; }
		set { _target = value; }
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
        ControlsSubscribe();
    }
    void OnDisable()
    {
        ControlsUnsubscribe();
    }


	void Update()
	{
		if (Target && (Target.position - transform.position).magnitude > maxDistToTarget)
		{
			Target = null;
		}
	}


    void ControlsSubscribe()
    {
        if (inputs == null)
            inputs = new Inputs();

        inputs.Player.Move.performed += Move;
		inputs.Player.Move.Enable();
    }
    void ControlsUnsubscribe()
    {
        inputs.Player.Move.performed -= Move;
		inputs.Player.Move.Disable();
	}


	#region HandleControls

	void Move(InputAction.CallbackContext context)
    {
        Debug.Log("Move vector: "+context.ReadValue<Vector2>());
    }
	void Jump(InputAction.CallbackContext context)
	{
	
	}
	void TargetLock(InputAction.CallbackContext context)
	{

	}
	void SwitchTarget(InputAction.CallbackContext context)
	{

	}
	void LookAround(InputAction.CallbackContext context)
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
