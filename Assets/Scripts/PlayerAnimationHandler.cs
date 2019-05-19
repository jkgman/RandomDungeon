using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
public class PlayerAnimationHandler : MonoBehaviour
{
	[SerializeField] private float blendSpeed = 5f;
	[SerializeField] private float dodgeDefaultDuration = 1f;
	[SerializeField] private string dodgeAnimName = "Dodge";

	Vector2 currentMoveBlend = Vector2.zero;

	private Animator _animator;
	private Animator Animator
	{
		get
		{
			if (!_animator)
				_animator = GetComponentInChildren<Animator>();

			return _animator;
		}
	}

	private AnimatorController ac;
	public List<string> AnimStateNames;

	void Awake()
	{
		GetAllStates();
	}

	void GetAllStates()
	{
		ac = Animator.runtimeAnimatorController as AnimatorController;
		ChildAnimatorState[] ch_animStates;
		AnimatorStateMachine stateMachine;

		foreach (AnimatorControllerLayer i in ac.layers) //for each layer
		{
			stateMachine = i.stateMachine;
			ch_animStates = stateMachine.states;
			foreach (ChildAnimatorState j in ch_animStates) //for each state
			{
				AnimStateNames.Add(j.state.name);
			}
		}


	}


	public void SetMovement(Vector2 blendParam)
	{
		currentMoveBlend = Vector2.Lerp(currentMoveBlend, blendParam, Time.deltaTime * blendSpeed);

		Animator.SetBool("move", blendParam != Vector2.zero);
		Animator.SetFloat("sidewaysMove",currentMoveBlend.x);
		Animator.SetFloat("forwardMove", currentMoveBlend.y);

	}

	public void SetDodgeStart(Vector2 direction, bool backstep = false, float duration = -1f)
	{
		Animator.SetBool("dodge", true);
		float d = duration;
		if (d <= 0)
			d = dodgeDefaultDuration;

		AnimatorState dodgeState = GetAnimStateByName(dodgeAnimName);

		if (dodgeState)
		{
			Motion motion = dodgeState.motion;
			float defaultDuration = motion.averageDuration;
			dodgeState.speed = defaultDuration / duration;
		}
		else
			Debug.LogWarning("Dodge clip was not found with name: " + dodgeAnimName);
		

		if (backstep)
		{
			Animator.SetFloat("sidewaysDodge", 0);
			Animator.SetFloat("forwardsDodge", -1);
		}
		else
		{
			Animator.SetFloat("sidewaysDodge", direction.x);
			Animator.SetFloat("forwardsDodge", direction.y);
		}
	}

	public void SetDodgeEnd()
	{
		Animator.SetBool("dodge", false);
	}


	AnimatorState GetAnimStateByName(string name)
	{
		ChildAnimatorState[] ch_animStates;
		AnimatorStateMachine stateMachine;

		foreach (AnimatorControllerLayer i in ac.layers) //for each layer
		{
			stateMachine = i.stateMachine;
			ch_animStates = stateMachine.states;

			foreach (ChildAnimatorState j in ch_animStates) //for each state
			{
				if (j.state.name == name)
					return j.state;
			}
		}
		return null;
	}
	
}
