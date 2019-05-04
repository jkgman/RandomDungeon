using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;


/// <summary>
/// 
/// Camera follows a dummy-position, which is lerping to player.
/// Camera relative to dummy is set in lateupdate without lerping.
/// During lock-on (targeting enemy) camera locks its direction behind player, showing the enemy all the time.
/// 
/// </summary>



public class CameraController : MonoBehaviour
{
	[SerializeField] private float _currentHAngle = 0;
	private float CurrentHAngle
	{
		get { return _currentHAngle; }
		set { _currentHAngle = value; }
	}

	[SerializeField] private float minVAngle = -45;
	[SerializeField] private float maxVAngle = 80;
	[SerializeField] private float _currentVAngle = 0;
	private float CurrentVAngle
	{
		get { return _currentVAngle; }
		set
		{
			if (value < minVAngle)
				_currentVAngle = minVAngle;
			else if (value > maxVAngle)
				_currentVAngle = maxVAngle;
			else
				_currentVAngle = value;
		}
	}
	[SerializeField] private float distDefault = 4f;

	[SerializeField] private float distTargeting = 4f;
	[SerializeField] private float angleVTargeting = 30f;
	[SerializeField] private float angleHTargeting = 30f;

	[SerializeField] private float sensitivityH = 10f;
	[SerializeField] private float sensitivityV = 10f;

	[Header("Lerp speeds")]
	[SerializeField] private float lerpDummyTargeting = 5f;
	[SerializeField] private float lerpCamTargeting = 20f;
	[SerializeField] private float lerpDummyDefault = 8f;
	[SerializeField] private float lerpCamDefault = 20f;

	private Vector3 dummyPos = Vector3.zero;
	private Vector3 rawFlatDir = Vector3.zero;
	private Vector3 rawFlatPos = Vector3.zero;
	private Vector3 dir = Vector3.zero;
	private PlayerController player;

	private Vector2 lookModifier = Vector2.zero;


	#region Inputs

	private Inputs inputs;

	void OnEnable() 
	{
		ControlsSubscribe();
	}
	void OnDisable() 
	{
		ControlsUnsubscribe();
	}

	void ControlsSubscribe() 
	{
		if (inputs == null)
			inputs = new Inputs();

		inputs.Player.TargetLock.performed += InputTargetLock;
		inputs.Player.Look.performed += InputLookAround;
		inputs.Player.TargetLock.Enable();
		inputs.Player.Look.Enable();
	}
	void ControlsUnsubscribe() 
	{
		inputs.Player.TargetLock.performed -= InputTargetLock;
		inputs.Player.Look.performed -= InputLookAround;
		inputs.Player.TargetLock.Disable();
		inputs.Player.Look.Disable();
	}

	void InputTargetLock(InputAction.CallbackContext context)
	{
		//Todo:
		//Get targets, no idea how
		//if no targets, reset camera:
		ResetCamera();
	}
	void InputLookAround(InputAction.CallbackContext context)
	{
        lookModifier = context.ReadValue<Vector2>() * Time.deltaTime * sensitivityH; ;
	}



	#endregion

	void Awake()
	{
		var go = GameObject.FindGameObjectWithTag("Player");
		if (go)
			player = go.GetComponent<PlayerController>();
			
	}


	void LateUpdate()
	{
		if (!player) //This script relies on player.
			return;

		SetDummyPosition(); // Dummy is position target for camera.
		UpdateDefaultPosition(); // Automatic camera movements
		ApplyInputPosition(); // Offsets to default camera movements modified by inputs
		SetRotation(); // Rotates towards current target


	}

	void SetDummyPosition()
	{
		if (player.Target)
		{
			//Vectors and positions between target and player.
			Vector3 midPos = (player.Target.position + player.GetPos) / 2f;
			Vector3 vectorBetween = player.Target.position - player.GetPos;
			//Weight towards player the further away target is
			Vector3 goalPos = Vector3.Lerp(midPos, player.GetPos, player.GetMaxDistToTarget / vectorBetween.magnitude);
			dummyPos = Vector3.Lerp(dummyPos, goalPos, Time.deltaTime * lerpDummyTargeting);
		}
		else
		{
			dummyPos = Vector3.Lerp(dummyPos, player.GetPos, Time.deltaTime * lerpDummyDefault);
		}
	}

	void UpdateDefaultPosition()
	{
		Vector3 newPos = dummyPos;

		if (player.Target)
		{
			rawFlatDir = dummyPos - player.Target.position;
			rawFlatDir.y = 0;
			rawFlatDir.Normalize();

			if (rawFlatDir != Vector3.zero)
			{
				//Applies offset angles so that player is not blocking view to enemy
				CurrentHAngle = Vector3.SignedAngle(Vector3.forward, rawFlatDir, Vector3.up) + angleHTargeting;
				CurrentVAngle = angleVTargeting;
				dir = Quaternion.Euler(-CurrentVAngle, CurrentHAngle, 0) * Vector3.forward;
			}
			
			newPos += (dir.normalized * distTargeting);
			transform.position = newPos;
		}
		else
		{
			//If real position was used, offsetting would cause camera to spin. RawFlatDir used instead.
			var newFlatDir = (transform.position - dummyPos).normalized;
			newFlatDir.y = 0;
			newFlatDir.Normalize();
			
			rawFlatDir = Vector3.Lerp(rawFlatDir, newFlatDir, Time.deltaTime * lerpCamDefault);
			//dir = Quaternion.AngleAxis(currentVAngle, GetVectorRight()) * rawFlatDir;

			//Make sure the horizontal angle value matches current angle
			CurrentHAngle = Vector3.SignedAngle(Vector3.forward, rawFlatDir, Vector3.up);
			dir = Quaternion.Euler(-CurrentVAngle, CurrentHAngle, 0) * Vector3.forward;
			// currentVAngle = Vector3.SignedAngle(rawFlatDir, dir, Vector3.Cross(rawFlatDir, Vector3.up));

			newPos += (dir.normalized * distDefault);
			transform.position = newPos;
		}
	}

	void ApplyInputPosition()
	{
		if (!player.Target)
		{
			CurrentHAngle += lookModifier.x;
			CurrentVAngle += lookModifier.y;

			//Reset modifier in case there is no more input.
			lookModifier = Vector2.zero;

			dir = Quaternion.Euler(-CurrentVAngle, CurrentHAngle, 0) * Vector3.forward;
			
			//Update rawdir, it is used as a reference in default camera position handling
			rawFlatDir = new Vector3(dir.x, 0, dir.z).normalized;

			Vector3 newPos = dummyPos;
			newPos += (dir.normalized * distDefault);
			transform.position = newPos;
		}
	}

	void ResetCamera()
	{
		if (!player.Target)
		{
			CurrentHAngle = Vector3.SignedAngle(-player.transform.forward, Vector3.forward, Vector3.up);
			CurrentVAngle = 30f;

			dir = Quaternion.Euler(-CurrentVAngle, CurrentHAngle, 0) * Vector3.forward;

			//Update rawdir, it is used as a reference in default camera position handling
			rawFlatDir = new Vector3(dir.x, 0, dir.z).normalized;

			Vector3 newPos = dummyPos;
			newPos += (dir.normalized * distDefault);
			transform.position = newPos;
		}
	}

	void SetRotation()
	{
		Quaternion newRot = Quaternion.identity;

		if (player.Target)
		{
			Vector3 middlepoint = (player.Target.position + player.GetPos) / 2f;
			newRot = Quaternion.LookRotation((middlepoint - transform.position).normalized);
			transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * lerpCamTargeting);
		}
		else
		{
			newRot = Quaternion.LookRotation(-dir.normalized);
			transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * lerpCamDefault);
			transform.rotation = newRot;
		}

	}
	
	/// <summary>
	/// Gets Vector right relative to last rawFlatDir between camera and target.
	/// </summary>
	Vector3 GetVectorRight()
	{
		var r = Vector3.Cross(rawFlatDir, Vector3.up);
		r.y = 0;
		return r.normalized;
	}

}
