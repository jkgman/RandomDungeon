using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{
	private const float DISTANCE_MARGIN = 1.0f;

	public float distFromPlayer = 4f;
	public float lockOnXAngle = 30f;
	public float lockOnYAngle = 30f;
	public float minDistFromPlayer = 4f;
	public float maxDistFromPlayer = 6f;
	public float fovDefault = 70f;
	public float fovTargeting = 60f;
	public float lockOnLerpSpeed = 5f;
	public float lockOffLerpSpeed = 15f;


	private Vector3 flatDir = Vector3.zero;
	private Vector3 rawFlatPos = Vector3.zero;
	private Vector3 dir = Vector3.zero;
	private float aspectRatio;
	private float tanFov;

	private Camera cam;
	private PlayerController player;


	void Awake()
	{
		var go = GameObject.FindGameObjectWithTag("Player");
		if (go)
			player = go.GetComponent<PlayerController>();

		cam = GetComponentInChildren<Camera>();
		aspectRatio = Screen.width / Screen.height;
		tanFov = Mathf.Tan(Mathf.Deg2Rad * cam.fieldOfView / 2.0f);
	}

	void LateUpdate()
	{
		if (!player)
			return;

		SetPosition();
		SetRotation();


	}

	void SetPosition()
	{
		Vector3 newPos = Vector3.zero;

		if (player.Target)
		{
			Vector3 vectorBetween = player.Target.position - player.GetPos;

			// Calculate the new distance.
			float distBetween = vectorBetween.magnitude;
			float cameraDistance = (distBetween / 2.0f / aspectRatio) / tanFov;

			flatDir = -vectorBetween;
			flatDir.y = 0;
			flatDir.Normalize();

			if (flatDir != Vector3.zero)
			{
				//Applies offset angles so that player is not blocking view to enemy
				var goalDir = Quaternion.AngleAxis(lockOnXAngle, Vector3.up) * Quaternion.AngleAxis(lockOnYAngle, transform.right) * flatDir;
				//Use quaternion to slerp direction, handles closeups better than pure vectors.
				dir = Quaternion.Slerp(Quaternion.LookRotation(dir), Quaternion.LookRotation(goalDir), Time.deltaTime * lockOnLerpSpeed) * Vector3.forward;
			}

			//Apply position without lerp, as it was added for the direction already
			newPos = player.GetPos + (dir * distFromPlayer);
			transform.position = newPos;

		}
		else
		{
			//If real position was used, offsetting would cause camera to spin. RawFlatPos used instead.
			flatDir = (rawFlatPos - player.GetPos);
			flatDir.y = 0;
			flatDir.Normalize();

			//Store the real position before applying offsetting angles
			rawFlatPos = player.GetPos + (flatDir * distFromPlayer); 

			dir = Quaternion.AngleAxis(lockOnXAngle, Vector3.up) * Quaternion.AngleAxis(lockOnYAngle, transform.right) * flatDir;

			newPos = Vector3.Lerp(transform.position, player.GetPos + (dir * distFromPlayer), Time.deltaTime * lockOffLerpSpeed);
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
		}
		else
		{
			newRot = Quaternion.LookRotation((player.GetPos - transform.position).normalized);
		}

		transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * lockOffLerpSpeed);
	}
}
