using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dungeon.Characters
{

	public class PlayerIK : MonoBehaviour
	{
		private readonly string leftFootAnimVariableName = "leftFootGrounded";
		private readonly string rightFootAnimVariableName = "rightFootGrounded";

		private Animator anim;

		private Vector3 rightFootPosition, leftFootPosition;
		private Vector3 leftFootIKPosition, rightFootIKPosition;
		private Quaternion leftFootIKRotation, rightFootIKRotation;
		private float lastPelvisPositionY, lastRightFootPositionY, lastLeftFootPositionY;
		[Header("Foot Grounding")]

		[SerializeField]
		private bool enableFootIK = true;
		[SerializeField, Range(0, 2f)]
		private float heightFromGround = 1.14f;
		[SerializeField, Range(0, 2f)]
		private float downDistance = 1.14f;
		[SerializeField]
		private LayerMask groundLayerMask;
		[SerializeField]
		private float pelvisOffset = 0;
		[SerializeField]
		private float pelvisOffsetFromFeet = 0;
		[SerializeField, Range(0, 1f)]
		private float feetToIKPositionSpeed = 0.25f;
		[SerializeField, Range(0, 10f)]
		private float pelvisUpAndDownSpeed;


		[SerializeField]
		private bool rotateFeet = false;
		[SerializeField]
		private bool showSolverDebug = false;



		void OnEnable()
		{
			anim = GetComponent<Animator>();
		}

		#region FootGrounding

		private void FixedUpdate()
		{
			if (!enableFootIK) return;
			if (!anim) return;
			if (!anim.enabled) return;

			AdjustFootTargets(ref rightFootPosition, HumanBodyBones.RightFoot);
			AdjustFootTargets(ref leftFootPosition, HumanBodyBones.LeftFoot);

			FootPositionSolver(rightFootPosition, ref rightFootIKPosition, ref rightFootIKRotation);
			FootPositionSolver(leftFootPosition, ref leftFootIKPosition, ref leftFootIKRotation);
		}

		private void OnAnimatorIK(int layerIndex)
		{
			if (!enableFootIK) return;
			if (!anim) return;
			if (!anim.enabled) return;

			MovePelvisHeight();

			anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
			anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);

			if (rotateFeet)
				anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, anim.GetFloat(rightFootAnimVariableName));

			if (rotateFeet)
				anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, anim.GetFloat(leftFootAnimVariableName));

			MoveFeetToIKPoint(AvatarIKGoal.RightFoot, rightFootIKPosition, rightFootIKRotation, ref lastRightFootPositionY);
			MoveFeetToIKPoint(AvatarIKGoal.LeftFoot, leftFootIKPosition, leftFootIKRotation, ref lastLeftFootPositionY);
		}

		#endregion


		#region FootGroundingMethods

		private void MoveFeetToIKPoint(AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFootPositionY)
		{
			Vector3 targetIKPosition = anim.GetIKPosition(foot);

			if (positionIKHolder != Vector3.zero)
			{
				targetIKPosition = transform.InverseTransformPoint(targetIKPosition);
				positionIKHolder = transform.InverseTransformPoint(positionIKHolder);

				float yVariable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, feetToIKPositionSpeed);
				targetIKPosition.y += yVariable;
				lastFootPositionY = yVariable;

				targetIKPosition = transform.TransformPoint(targetIKPosition);
				anim.SetIKRotation(foot, rotationIKHolder);
			}

			anim.SetIKPosition(foot, targetIKPosition);
		}

		private void MovePelvisHeight()
		{
			if (rightFootIKPosition == Vector3.zero ||
				leftFootIKPosition == Vector3.zero ||
				lastPelvisPositionY == 0)
			{
				lastPelvisPositionY = anim.bodyPosition.y;
				return;
			}

			//If current state on any layer has footIK enabled, update ik accordingly
			//Otherwise reset pelvis.
			bool offsetPelvis = false;
			for (int i = 0; i < anim.layerCount; i++)
			{
				if (anim.GetCurrentAnimatorStateInfo(i).IsTag("FootIK"))
				{
					offsetPelvis = true;
					Debug.Log("Found IK Tag");
				}

			}


			Vector3 newPelvisPosition = anim.bodyPosition;
			if (offsetPelvis)
			{
				float leftOffsetPosition = leftFootIKPosition.y - transform.position.y;
				float rightOffsetPosition = rightFootIKPosition.y - transform.position.y;
				float totalOffset = Mathf.Min(leftOffsetPosition,rightOffsetPosition);
				newPelvisPosition = anim.bodyPosition + Vector3.up * totalOffset;

				float currentFootAverageY = (anim.GetBoneTransform(HumanBodyBones.LeftFoot).position.y + anim.GetBoneTransform(HumanBodyBones.RightFoot).position.y) / 2;
				if (newPelvisPosition.y - currentFootAverageY < pelvisOffsetFromFeet)
					newPelvisPosition.y = currentFootAverageY + pelvisOffsetFromFeet;

			}

			newPelvisPosition.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPosition.y, pelvisUpAndDownSpeed);
			anim.bodyPosition = newPelvisPosition;
			lastPelvisPositionY = newPelvisPosition.y;
		}

		private void FootPositionSolver(Vector3 fromSkyPosition, ref Vector3 footIKPosition, ref Quaternion footIKRotation)
		{
			RaycastHit footHit;

			if (showSolverDebug)
				Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down*(downDistance + heightFromGround), Color.yellow);

			if (Physics.Raycast(fromSkyPosition, Vector3.down, out footHit, downDistance + heightFromGround, groundLayerMask))
			{
				footIKPosition = fromSkyPosition;
				footIKPosition.y = footHit.point.y + pelvisOffset;
				footIKRotation = Quaternion.FromToRotation(Vector3.up, footHit.normal) * transform.rotation;

				return;
			}

			footIKPosition = Vector3.zero; //Failed
		}

		private void AdjustFootTargets (ref Vector3 footPositions, HumanBodyBones foot)
		{
			footPositions = anim.GetBoneTransform(foot).position;
			footPositions.y = transform.position.y + heightFromGround;


		}

		#endregion


	}
}
