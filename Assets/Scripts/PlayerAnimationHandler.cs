using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
	[SerializeField] private float blendSpeed = 5f;
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

	Vector2 currentMoveBlend = Vector2.zero;

    public void SetMovement(Vector2 blendParam)
	{
		currentMoveBlend = Vector2.Lerp(currentMoveBlend, blendParam, Time.deltaTime * blendSpeed);

		Anim.SetBool("move", blendParam != Vector2.zero);
		Anim.SetFloat("sidewaysMove",currentMoveBlend.x);
		Anim.SetFloat("forwardMove", currentMoveBlend.y);

	}
}
