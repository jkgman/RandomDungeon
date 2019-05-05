using System.Collections;
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



public class CameraController : MonoBehaviour
{
	[SerializeField] private Vector2 _rawAngle;
	private Vector2 RawAngle
	{
		get{ return _rawAngle; }
		set
		{
			_rawAngle.x = Mathf.Clamp(value.x, minVAngle, maxVAngle);
			_rawAngle.y = value.y;
		}
	}
	[SerializeField] private Vector2 _curAngle;
	public Vector2 CurAngle {
		get { return _curAngle; }
		private set
		{
			_curAngle.x = Mathf.Clamp(value.x, minVAngle, maxVAngle);
			_curAngle.y = value.y;
		}
	}
	[SerializeField, Range(-89f, -1f)] private float minVAngle = -45f;
	[SerializeField, Range( 89f,  1f)] private float maxVAngle = 80f;
	[SerializeField] private float defaultVAngle = 25f;
	[SerializeField] private float distDefault = 4f;
	[SerializeField] private float autoCameraDelayAfterLookInput = 0.5f;

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
	[SerializeField] private float lerpCamToDefault = 2f;

	private Vector3 dummyPos = Vector3.zero;
	//private Vector3 rawFlatDirNoOffset = Vector3.zero;
	private Vector3 rawFlatPos = Vector3.zero;
	//private Vector3 rawDir = Vector3.zero;
	private PlayerController player;

	private Vector2 lookModifier = Vector2.zero;
	private float lastLookInput = 0;
	private float targetChangeTime = 0;

	void OnEnable() 
	{
		GetPlayer();
		AddEventListeners();
		ControlsSubscribe();
	}
	void OnDisable() 
	{
		RemoveEventListeners();
		ControlsUnsubscribe();
	}

	void AddEventListeners()
	{
		if (player)
			player.targetChangedEvent.AddListener(TargetChanged);

	}
	void RemoveEventListeners()
	{
		if (player)
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

	void InputTargetLock(InputAction.CallbackContext context)
	{
		//Todo:
		//Get targets, no idea how
		//if no targets, reset camera:
		bool success = player.SetTarget();
		if (!success)
			ResetCamera();
		
	}
	void InputLookAround(InputAction.CallbackContext context)
	{
        lookModifier = context.ReadValue<Vector2>() * Time.deltaTime * sensitivityH;
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
		SetPositionByDirection(); //Finally applies camera position according to earlier modifier values
		SetRotation(); // Rotates face towards current target


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
			//Get new direction from camera-player relation
			var newFlatDir = (transform.position - dummyPos).normalized;
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
			Quaternion yRot = Quaternion.Slerp(Quaternion.Euler(0, CurAngle.y, 0), Quaternion.Euler(0, RawAngle.y, 0), Time.deltaTime * lerpCamTargeting);
			var y = Vector3.SignedAngle(Vector3.forward, yRot * Vector3.forward, Vector3.up);
			//Vertical angles are ok with mathf.lerp
			var x = Mathf.Lerp(CurAngle.x, RawAngle.x, Time.deltaTime * lerpCamTargeting);

			CurAngle = new Vector2(x, y);
		}
		else
		{

			if (lastLookInput + autoCameraDelayAfterLookInput < Time.time) //Lerp towards default angle (vertical angles are never extreme -> mathf.lerp is ok)
			{
				float lerpWeight = Mathf.Clamp01((Time.time - (lastLookInput + autoCameraDelayAfterLookInput)) / 3f); //Takes 3s to be full strength
				RawAngle = new Vector2(Mathf.Lerp(RawAngle.x, defaultVAngle, Time.deltaTime * lerpCamToDefault * lerpWeight), RawAngle.y);
			}

			//Because of angles being between -180 and 180, quaternion is needed to lerp between eulers.
			Quaternion yRot = Quaternion.Slerp(Quaternion.Euler(0, CurAngle.y, 0), Quaternion.Euler(0, RawAngle.y, 0), Time.deltaTime * lerpCamDefault);
			var y = Vector3.SignedAngle(Vector3.forward, yRot * Vector3.forward, Vector3.up);
			//Vertical angles are ok with mathf.lerp
			var x = Mathf.Lerp(CurAngle.x, RawAngle.x, Time.deltaTime * lerpCamDefault);

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

	void SetPositionByDirection()
	{
		Vector3 newPos = dummyPos;
		newPos += (GetCurrentDirection() * distDefault);
		transform.position = newPos;
	}

	void ResetCamera()
	{
		if (!player.Target)
		{
			var x = defaultVAngle;
			var y = Vector3.SignedAngle(-player.transform.forward, Vector3.forward, Vector3.up);
			RawAngle = new Vector2(x, y);
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
			float lerpWeight = Mathf.Clamp01(Time.time - targetChangeTime);
			newRot = Quaternion.LookRotation(-GetRawDirection());
			transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * 100f * lerpWeight);
		}

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
