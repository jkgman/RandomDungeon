using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
	[SerializeField] private float blendSpeed = 5f;
	[SerializeField] private float dodgeDefaultDuration = 1f;
	[SerializeField] private string dodgeAnimName = "Dodge";

	Vector2 currentMoveBlend = Vector2.zero;

	private Animator _anim;
	private Animator Anim
	{
		get
		{
			if (!_anim)
				_anim = GetComponentInChildren<Animator>();

			return _anim;
		}
	}



	public void SetMovement(Vector2 blendParam)
	{
		currentMoveBlend = Vector2.Lerp(currentMoveBlend, blendParam, Time.deltaTime * blendSpeed);

		Anim.SetBool("move", blendParam != Vector2.zero);
		Anim.SetFloat("sidewaysMove",currentMoveBlend.x);
		Anim.SetFloat("forwardMove", currentMoveBlend.y);

	}

	public void SetDodgeStart(Vector2 direction, bool backstep = false, float duration = -1f)
	{
		Anim.SetBool("dodge", true);
		float d = duration;
		if (d <= 0)
			d = dodgeDefaultDuration;

		AnimationClip dodgeClip = GetAnimationClip(dodgeAnimName);
		if (dodgeClip)
		{
			int frames = (int)(dodgeClip.frameRate * dodgeClip.length);
			dodgeClip.frameRate = frames / duration;
		}
		else
			Debug.LogWarning("Dodge clip was not found with name: " + dodgeAnimName);
		

		if (backstep)
		{
			Anim.SetFloat("sidewaysDodge", 0);
			Anim.SetFloat("forwardsDodge", -1);
		}
		else
		{
			Anim.SetFloat("sidewaysDodge", direction.x);
			Anim.SetFloat("forwardsDodge", direction.y);
		}
	}

	public void SetDodgeEnd()
	{
		Anim.SetBool("dodge", false);
	}


	public AnimationClip GetAnimationClip(string name)
	{
		if (!Anim) return null; // no animator

		foreach (AnimationClip clip in Anim.runtimeAnimatorController.animationClips)
		{
			Debug.Log("Clip name:: " + clip.name);
			if (clip.name == name)
			{
				return clip;
			}
		}
		return null; // no clip by that name
	}
}
