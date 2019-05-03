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

			var flatDir = -vectorBetween.normalized;
			flatDir.y = 0;

			var dir = Quaternion.AngleAxis(lockOnXAngle, Vector3.up) * Quaternion.AngleAxis(lockOnYAngle, transform.right) * flatDir;

			var dist = dir * (cameraDistance + DISTANCE_MARGIN);
			if (dist.magnitude > maxDistFromPlayer)
				dist = dist.normalized * maxDistFromPlayer;
			if (dist.magnitude < minDistFromPlayer)
				dist = dist.normalized * minDistFromPlayer;

			newPos = player.GetPos + dist;
			transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * lockOnLerpSpeed);

		}
		else
		{
			Vector3 dirFromPlayer = (transform.position - player.GetPos).normalized;
			newPos = player.GetPos + dirFromPlayer * distFromPlayer;
			transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * lockOffLerpSpeed);
		}


	}

	void SetRotation()
	{
		if (player.Target)
		{
			Vector3 middlepoint = (player.Target.position + player.GetPos) / 2f;
			transform.rotation = Quaternion.LookRotation((middlepoint - transform.position).normalized);
		}
		else
		{
			transform.LookAt(player.transform);
		}
	}
}
