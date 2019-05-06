﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;


///
/// 
/// Camera follows a dummy-position, which is lerping to player.
/// Camera relative to dummy is set in lateupdate without lerping.
/// During lock-on (targeting enemy) camera locks its direction behind player, showing the enemy all the time.
/// 
///

public struct CameraTargetingData
{
	public Vector3 forward;
	public Vector3 position;
	public float fov;
}

public class CameraController : MonoBehaviour
{
	[SerializeField] private float distDefault = 4f;
	[SerializeField] private float distTargeting = 4f;

	[SerializeField] private float angleVTargeting = 30f;
	[SerializeField] private float angleHTargeting = 30f;

	[SerializeField] private float lookSensitivityMouse = 10f;
	[SerializeField] private float lookSensitivityGamepad = 100f;

	[SerializeField] private LayerMask collidingLayers;


	//Waits until camera starts going back to default angle.
	private readonly float autoCameraDelayAfterLookInput = 0.5f;

	//Camera angle limits and default for when inputs are not detected
	private readonly float minVAngle = -85f;
	private readonly float maxVAngle = 40f;
	private readonly float defaultVAngle = -15f;

	//Different lerp speeds for different situations/states
	private readonly float lerpDummyTargeting = 10f;
	private readonly float lerpDirTargeting = 20f;
	private readonly float lerpLookRotTargeting = 30f;
	private readonly float lerpDummyDefault = 10f;
	private readonly float lerpDirDefault = 15f;
	private readonly float lerpLookRotDefault = 150f;
	private readonly float lerpLookAtPosDefault = 200f;
	private readonly float lerpCamToDefault = 2f;

	private Vector3 dummyPos = Vector3.zero; // Position that follows player or its target, Camera always follows dummyPos and not player.
	private Vector3 oldDummyPos = Vector3.zero; // Last Frame
	private Vector3 lookAtPoint = Vector3.zero;
	private float currentDistance; //Multiplier for camera orbit direction (from dummyPos)
	private PlayerController player;


	//Input variables
	private Vector2 lookModifier = Vector2.zero; //The raw value from "Look" input
	private float lastLookInput = 0; //Last Time.time when "Look" input was detected
	private float targetChangeTime = 0; //Last Time.time that player's target changed in any way


	//The angle that currentAngle lerps towards
	private Vector2 _rawAngle;
	private Vector2 RawAngle
	{
		get{ return _rawAngle; }
		set
		{
			_rawAngle.x = Mathf.Clamp(value.x, minVAngle, maxVAngle);
			_rawAngle.y = value.y;
		}
	}

	//CurrentAngle, which aims to be same as rawAngle but lerps behind.
	private Vector2 _curAngle;
	public Vector2 CurAngle {
		get { return _curAngle; }
		private set
		{
			_curAngle.x = Mathf.Clamp(value.x, minVAngle, maxVAngle);
			_curAngle.y = value.y;
		}
	}


	void OnEnable() 
	{
		GetPlayer();
		if (player)
		{
			AddEventListeners();
			ControlsSubscribe();
		}
	}
	void OnDisable() 
	{
		RemoveEventListeners();
		ControlsUnsubscribe();
	}

	void AddEventListeners()
	{
		if (player && player.targetChangedEvent != null)
			player.targetChangedEvent.AddListener(TargetChanged);

	}
	void RemoveEventListeners()
	{
		if (player && player.targetChangedEvent != null)
			player.targetChangedEvent.RemoveListener(TargetChanged);

	}


	#region Inputs

	private Inputs inputs;
	
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

	void InputTargetLock(InputAction.CallbackContext context) {
		//Todo:
		//Get targets, no idea how
		//if no targets, reset camera:
		CameraTargetingData data = new CameraTargetingData() {
			forward = -GetCurrentDirection(),
			position = transform.position,
			fov = Camera.main.fieldOfView
		};
		bool success = player.SetTarget(data);
		if (!success)
			ResetCamera();
		
	}
	void InputLookAround(InputAction.CallbackContext context)
	{
        lookModifier = context.ReadValue<Vector2>() * Time.deltaTime;

		if (context.control.layout == "Stick")
			lookModifier *= lookSensitivityGamepad;
		else
			lookModifier *= lookSensitivityMouse;

		lastLookInput = Time.time;
	}

	#endregion



	void GetPlayer()
	{
		var go = GameObject.FindGameObjectWithTag("Player");
		if (go)
			player = go.GetComponent<PlayerController>();
	}

	void TargetChanged()
	{
		targetChangeTime = Time.time;
	}

	void LateUpdate()
	{
		if (!player) //This script relies on player.
			return;

		SetDummyPosition(); // Dummy is position target for camera.
		UpdateAutomaticCameraAngleDirection(); // Automatic camera movements
		ProcessCurrentAngle(); // Lerps current angles towards raw angles
		ApplyInputDirection(); // Offsets to default camera movements modified by inputs
		PhysicsCheck();
		SetPositionByDirection(); //Finally applies camera position according to earlier modifier values
		SetRotation(); // Rotates face towards current target



	}

	void SetDummyPosition()
	{
		oldDummyPos = dummyPos;

		if (player.Target)
		{
			//Vectors and positions between target and player.
			Vector3 midPos = (player.Target.position + player.GetPos) / 2f;
			Vector3 vectorBetween = player.Target.position - player.GetPos;
			//Weight towards player the further away target is
			Vector3 goalPos = Vector3.Lerp(midPos, player.GetPos, player.GetMaxDistToTarget / vectorBetween.magnitude);
			//Lerp determines how much camera lags behind player
			dummyPos = Vector3.Lerp(dummyPos, goalPos, Time.deltaTime * lerpDummyTargeting);
		}
		else
		{
			//Lerp determines how much camera lags behind player
			dummyPos = Vector3.Lerp(dummyPos, player.GetPos, Time.deltaTime * lerpDummyDefault);
		}
	}

	void UpdateAutomaticCameraAngleDirection()
	{
		if (player.Target)
		{
			//Get new direction from player-target relation
			var newFlatDir = dummyPos - player.Target.position;
			newFlatDir.y = 0;
			newFlatDir.Normalize();

			//Applies offset angles so that player is not blocking view to enemy
			var x = angleVTargeting;
			var y = Vector3.SignedAngle(Vector3.forward, newFlatDir, Vector3.up) + angleHTargeting;
			RawAngle = new Vector2(x, y);
		}
		else if (lastLookInput + autoCameraDelayAfterLookInput < Time.time)
		{
			//Get new direction from old raw position because with transform.position rawAngle does not work properly elsewhere.
			var oldRawPos = oldDummyPos + (GetRawDirection() * currentDistance);
			var newFlatDir = (oldRawPos - dummyPos).normalized;
			newFlatDir.y = 0;
			newFlatDir.Normalize();
			//Set rawAngle values to match current angle
			var y = Vector3.SignedAngle(Vector3.forward, newFlatDir, Vector3.up);
			RawAngle = new Vector2(RawAngle.x, y);
			
		}
	}

	void ProcessCurrentAngle()
	{
		if (player.Target)
		{
			//Because of angles being between -180 and 180, quaternion is needed to lerp between eulers.
			Quaternion yRot = Quaternion.Slerp(Quaternion.Euler(0, CurAngle.y, 0), Quaternion.Euler(0, RawAngle.y, 0), Time.deltaTime * lerpDirTargeting);
			var y = Vector3.SignedAngle(Vector3.forward, yRot * Vector3.forward, Vector3.up);
			//Vertical angles are ok with mathf.lerp
			var x = Mathf.Lerp(CurAngle.x, RawAngle.x, Time.deltaTime * lerpDirTargeting);

			CurAngle = new Vector2(x, y);
		}
		else
		{

			if (autoCameraDelayAfterLookInput != 0 && lastLookInput + autoCameraDelayAfterLookInput < Time.time) 
			{
				//Lerp towards default angle (vertical angles are never extreme -> mathf.lerp is ok)
				float lerpWeight = Mathf.Clamp01((Time.time - (lastLookInput + autoCameraDelayAfterLookInput)) / 3f); //Takes 3s to be full strength
				RawAngle = new Vector2(Mathf.Lerp(RawAngle.x, defaultVAngle, Time.deltaTime * lerpCamToDefault * lerpWeight), RawAngle.y);
			}

			//Because of angles being between -180 and 180, quaternion is needed to lerp between eulers.
			Quaternion yRot = Quaternion.Slerp(Quaternion.Euler(0, CurAngle.y, 0), Quaternion.Euler(0, RawAngle.y, 0), Time.deltaTime * lerpDirDefault);
			var y = Vector3.SignedAngle(Vector3.forward, yRot * Vector3.forward, Vector3.up);
			//Vertical angles are ok with mathf.lerp
			var x = Mathf.Lerp(CurAngle.x, RawAngle.x, Time.deltaTime * lerpDirDefault);

			CurAngle = new Vector2(x, y);
		}

	}

	void ApplyInputDirection()
	{
		if (!player.Target)
		{
			CurAngle += new Vector2(lookModifier.y, lookModifier.x);
			RawAngle += new Vector2(lookModifier.y, lookModifier.x);

			//Reset modifier in case there is no more input.
			lookModifier = Vector2.zero;
		}
	}


	void PhysicsCheck()
	{
		Physics.SphereCast(dummyPos, 0.5f, GetCurrentDirection(), out RaycastHit hit, distDefault, collidingLayers.value);
		if (hit.collider)
		{
			currentDistance = (dummyPos-hit.point).magnitude;
		}
		else
		{
			currentDistance = Mathf.Lerp(currentDistance, distDefault, Time.deltaTime*lerpDirDefault);
		}
	}

	void SetPositionByDirection()
	{
		Vector3 newPos = dummyPos;
		newPos += (GetCurrentDirection() * currentDistance);
		transform.position = newPos;
	}

	void ResetCamera()
	{
		if (!player.Target)
		{
			var x = defaultVAngle;
			var y = Vector3.SignedAngle(Vector3.forward , -player.transform.forward, Vector3.up);
			RawAngle = new Vector2(x, y);

		}
	}

	void SetRotation()
	{
		Quaternion newRot = transform.rotation;
		Quaternion rawLookRot = Quaternion.identity;

		if (player.Target)
		{
			lookAtPoint = (player.Target.position + player.GetPos) / 2f;
			//The goal to look at
			rawLookRot = Quaternion.LookRotation((lookAtPoint - transform.position).normalized);
			//The real direction we look at
			newRot = Quaternion.Slerp(newRot, rawLookRot, Time.deltaTime * lerpLookRotTargeting);
		}
		else
		{
			//"Lerp weight" when defaulting to new position after target has changed.
			float defaultingTime = Mathf.Clamp01((Time.time - targetChangeTime) / 5f);
			lookAtPoint = Vector3.Lerp(lookAtPoint, dummyPos, Time.deltaTime * lerpLookAtPosDefault * defaultingTime);

			//The goal to look at
			rawLookRot = Quaternion.LookRotation((lookAtPoint - transform.position).normalized);

			//The real direction we look at
			newRot = Quaternion.Slerp(newRot, rawLookRot, Time.deltaTime * lerpLookRotDefault);
		}

		transform.rotation = newRot;
	}
	
	/// <summary>
	/// Gets Vector right relative to last rawFlatDir between camera and target.
	/// </summary>
	Vector3 GetRawRight()
	{
		//var r = Vector3.Cross(rawFlatDirNoOffset, Vector3.up);
		var r = Vector3.Cross(Quaternion.Euler(0, RawAngle.y, 0) * Vector3.forward, Vector3.up);
		r.y = 0;
		return r.normalized;
	}

	Vector3 GetRawDirection()
	{
		return (Quaternion.Euler(new Vector3(RawAngle.x, RawAngle.y, 0)) * Vector3.forward).normalized;
	}
	Vector3 GetCurrentDirection()
	{
		return (Quaternion.Euler(new Vector3(CurAngle.x, CurAngle.y, 0)) * Vector3.forward).normalized;
	}
	public Vector3 GetCurrentFlatDirection()
	{
		var dir = GetCurrentDirection();
		dir.y = 0;
		return dir.normalized;
	}
}
