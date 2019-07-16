using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//A very simple script to show how to turn a Mecanim character into a ragdoll
//when the user presses space, assuming that the Unity Ragdoll Wizard has
//been used to add the ragdoll RigidBody components

//To use, do the following:
//Add "GetUpFromBelly" and "GetUpFromBack" bool inputs to the Animator controller
//and corresponding transitions from any state to the get up animations. When the ragdoll mode
//is turned on, Mecanim stops where it was and it needs to transition to the get up state immediately
//when it is resumed. Therefore, make sure that the blend times of the transitions to the get up animations are set to zero.

namespace Dungeon.Characters
{

	public class RagdollScript : MonoBehaviour {

		//Possible states of the ragdoll
		public enum RagdollState
		{
			animated,    //Mecanim is fully in control
			ragdolled,   //Mecanim turned off, physics controls the ragdoll
			blendToAnim  //Mecanim in control, but LateUpdate() is used to partially blend in the last ragdolled pose
		}

		//Declare a class that will hold useful information for each body part
		private class BodyPart
		{
			public Transform transform;
			public Vector3 storedPosition;
			public Quaternion storedRotation;
		}


		//How long do we blend when transitioning from ragdolled to animated
		[SerializeField]
		private float ragdollToMecanimBlendTime = 0.5f;
		[SerializeField]
		private Transform mainModel;

		public RagdollState ragdollState = RagdollState.animated;

		float mecanimToGetUpTransitionTime = 0.05f;

		//A helper variable to store the time when we transitioned from ragdolled to blendToAnim state
		float ragdollingEndTime = -100;

		//Additional vectores for storing the pose the ragdoll ended up in.
		Vector3 ragdolledHipPosition, ragdolledHeadPosition, ragdolledFeetPosition;

		//Declare a list of body parts, initialized in Start()
		List<BodyPart> bodyParts = new List<BodyPart>();

		private Animator anim;
		Rigidbody[] bodies;


	
		void DisableRagdoll()
		{

			foreach (Rigidbody rb in bodies)
			{
				rb.isKinematic= true;
			}
			anim.enabled = true;
	
		}
	
		void EnableRagdoll()
		{
			anim.enabled = false;

			foreach (Rigidbody rb in bodies)
			{
				rb.isKinematic = false;
			}
		}

		public void Reset()
		{
			DisableRagdoll();
			ragdollState = RagdollState.animated;
		}

		// Use this for initialization
		void Start () {
			//Set all RigidBodies to kinematic so that they can be controlled with Mecanim
			//and there will be no glitches when transitioning to a ragdoll
			anim = GetComponentInChildren<Animator>();
			bodies = GetComponentsInChildren<Rigidbody>(true);

			//Set all RigidBodies to kinematic so that they can be controlled with Mecanim
			DisableRagdoll();

			//Finds all transforms inside model and adds them to a bodyPart list.
			CreateBodyparts();
		}

		private void CreateBodyparts()
		{
			//Find all the transforms in the character
			Component[] components = mainModel.GetComponentsInChildren(typeof(Transform));

			//For each of the transforms, create a BodyPart instance and store the transform 
			foreach (Component c in components)
			{
				BodyPart bodyPart = new BodyPart();
				bodyPart.transform = c as Transform;
				bodyParts.Add(bodyPart);
			}
		}

		//public property that can be set to toggle between ragdolled and animated character
		public bool Ragdolled
		{
			get
			{
				return ragdollState != RagdollState.animated;
			}
			set
			{
				if (value == true)
				{
					if (ragdollState == RagdollState.animated)
					{
						//Transition from animated to ragdolled
						EnableRagdoll(); //allow the ragdoll RigidBodies to react to the environment
						ragdollState = RagdollState.ragdolled;
					}
				}
				else
				{
					if (ragdollState == RagdollState.ragdolled)
					{
						//Transition from ragdolled to animated through the blendToAnim state
						DisableRagdoll(); //disable gravity etc.
						ragdollingEndTime = Time.time; //store the state change time
						anim.enabled = true; //enable animation
						ragdollState = RagdollState.blendToAnim;

						//Store the ragdolled position for blending
						foreach (BodyPart b in bodyParts)
						{
							b.storedRotation = b.transform.rotation;
							b.storedPosition = b.transform.position;
						}

						//Remember some key positions
						ragdolledFeetPosition = 0.5f * (anim.GetBoneTransform(HumanBodyBones.LeftToes).position + anim.GetBoneTransform(HumanBodyBones.RightToes).position);
						ragdolledHeadPosition = anim.GetBoneTransform(HumanBodyBones.Head).position;
						ragdolledHipPosition = anim.GetBoneTransform(HumanBodyBones.Hips).position;

						//Initiate the get up animation
						if (anim.GetBoneTransform(HumanBodyBones.Hips).forward.y > 0) //hip hips forward vector pointing upwards, initiate the get up from back animation
						{
							anim.SetBool("GetUpFromBack", true);
							anim.CrossFade("Get Up Back", 0, 0, 0, 0);
						}
						else
						{
							anim.SetBool("GetUpFromBelly", true);
							anim.CrossFade("Get Up Front", 0, 0, 0, 0);
						}
					} //if (state==RagdollState.ragdolled)
				}   //if value==false	
			} //set
		}

		private void ResetPosition()
		{
			//If we are waiting for Mecanim to start playing the get up animations, update the root of the mecanim
			//character to the best match with the ragdoll
			Vector3 animatedToRagdolled = ragdolledHipPosition - anim.GetBoneTransform(HumanBodyBones.Hips).position;
			Vector3 newRootPosition = transform.position + animatedToRagdolled;

			//Now cast a ray from the computed position downwards and find the highest hit that does not belong to the character 
			RaycastHit[] hits = Physics.RaycastAll(new Ray(newRootPosition, Vector3.down));
			newRootPosition.y = transform.position.y;
			foreach (RaycastHit hit in hits)
			{
				if (!hit.transform.IsChildOf(transform))
				{
					newRootPosition.y = Mathf.Max(newRootPosition.y, hit.point.y);
				}
			}
			var offset = newRootPosition - transform.position;
	

			if (GetComponent<CharacterController>())
				GetComponent<CharacterController>().ExternalMove(offset);


		}

		void LateUpdate()
		{
			//Clear the get up animation controls so that we don't end up repeating the animations indefinitely
			anim.SetBool("GetUpFromBelly", false);
			anim.SetBool("GetUpFromBack", false);

			//Blending from ragdoll back to animated
			if (ragdollState == RagdollState.blendToAnim)
			{
				if (Time.time <= ragdollingEndTime + mecanimToGetUpTransitionTime)
				{

					ResetPosition();

					//Get body orientation in ground plane for both the ragdolled pose and the animated get up pose
					Vector3 ragdolledDirection = ragdolledHeadPosition - ragdolledFeetPosition;
					ragdolledDirection.y = 0;

					Vector3 meanFeetPosition = 0.5f * (anim.GetBoneTransform(HumanBodyBones.LeftFoot).position + anim.GetBoneTransform(HumanBodyBones.RightFoot).position);
					Vector3 animatedDirection = anim.GetBoneTransform(HumanBodyBones.Head).position - meanFeetPosition;
					animatedDirection.y = 0;

					//Try to match the rotations. Note that we can only rotate around Y axis, as the animated characted must stay upright,
					//hence setting the y components of the vectors to zero. 
					transform.rotation *= Quaternion.FromToRotation(animatedDirection.normalized, ragdolledDirection.normalized);
				}
				//compute the ragdoll blend amount in the range 0...1
				float ragdollBlendAmount = 1.0f - (Time.time - ragdollingEndTime - mecanimToGetUpTransitionTime) / ragdollToMecanimBlendTime;
				ragdollBlendAmount = Mathf.Clamp01(ragdollBlendAmount);

				//In LateUpdate(), Mecanim has already updated the body pose according to the animations. 
				//To enable smooth transitioning from a ragdoll to animation, we lerp the position of the hips 
				//and slerp all the rotations towards the ones stored when ending the ragdolling
				foreach (BodyPart b in bodyParts)
				{
					if (b.transform != transform)
					{ //this if is to prevent us from modifying the root of the character, only the actual body parts
					  //position is only interpolated for the hips
						if (b.transform == anim.GetBoneTransform(HumanBodyBones.Hips))
							b.transform.position = Vector3.Lerp(b.transform.position, b.storedPosition, ragdollBlendAmount);
						//rotation is interpolated for all body parts
						b.transform.rotation = Quaternion.Slerp(b.transform.rotation, b.storedRotation, ragdollBlendAmount);
					}
				}

				//if the ragdoll blend amount has decreased to zero, move to animated state
				if (ragdollBlendAmount == 0)
				{
					ragdollState = RagdollState.animated;
					return;
				}
			}
		}
	}
}
