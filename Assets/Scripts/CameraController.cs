using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 
/// Camera follows a dummy-position, which is lerping to player.
/// Camera relative to dummy is set in lateupdate without lerping.
/// During lock-on (targeting enemy) camera locks its direction behind player, showing the enemy all the time.
/// 
/// </summary>



public class CameraController : MonoBehaviour
{
	[SerializeField] private float currentHAngle = 0;
	[SerializeField] private float currentVAngle = 0;
	[SerializeField] private float distDefault = 4f;

	[SerializeField] private float distTargeting = 4f;
	[SerializeField] private float angleVTargeting = 30f;
	[SerializeField] private float angleHTargeting = 30f;

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
				currentHAngle = Vector3.SignedAngle(Vector3.forward, rawFlatDir, Vector3.up) + angleHTargeting;
				currentVAngle = angleVTargeting;
				dir = Quaternion.Euler(-currentVAngle, currentHAngle, 0) * Vector3.forward;
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
			dir = Quaternion.AngleAxis(currentVAngle, GetVectorRight()) * rawFlatDir;

			//Make sure the horizontal angle value matches current angle
			currentHAngle = Vector3.SignedAngle(Vector3.forward, rawFlatDir, Vector3.up);
			// currentVAngle = Vector3.SignedAngle(rawFlatDir, dir, Vector3.Cross(rawFlatDir, Vector3.up));

			newPos += (dir.normalized * distDefault);
			transform.position = newPos;
		}
	}

	void ApplyInputPosition()
	{
		//TODO :
		//modify camera with input values.
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
			newRot = Quaternion.LookRotation((player.GetPos - transform.position).normalized);
			transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * lerpCamDefault);
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
