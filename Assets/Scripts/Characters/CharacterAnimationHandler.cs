using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

namespace Dungeon.Characters
{
	public class CharacterAnimationHandler : MonoBehaviour
	{



		protected AnimatorController ac;
		protected List<AnimatorState> animatorStates;



		#region Getters & Setters

		private Animator _animator;
		protected Animator Animator
		{
			get
			{
				if (!_animator)
					_animator = GetComponentInChildren<Animator>();

				return _animator;
			}
		}

		/// <summary>Get All states in current AnimatorController.</summary>
		List<AnimatorState> GetAllStates()
		{
			List<AnimatorState> output = new List<AnimatorState>();

			ac = Animator.runtimeAnimatorController as AnimatorController;

			foreach (AnimatorControllerLayer i in ac.layers) //for each layer
			{
				AnimatorStateMachine stateMachine = i.stateMachine;
				List<AnimatorState> states = GetAllStatesInMachine(stateMachine);

				foreach (var s in states)
				{
					if (!output.Contains(s))
						output.Add(s);
				}
			}
			return output;
		}

		/// <summary>Get all states in a state machine.</summary>
		List<AnimatorState> GetAllStatesInMachine(AnimatorStateMachine stateMachine)
		{
			List<AnimatorState> output = new List<AnimatorState>();

			if (stateMachine.stateMachines.Length > 0)
			{
				foreach (var sm in stateMachine.stateMachines)
				{
					List<AnimatorState> foundStates = GetStatesInChildMachine(sm);
					for (int i = 0; i < foundStates.Count; i++)
					{
						if (!output.Contains(foundStates[i]))
							output.Add(foundStates[i]);
					}
				}
			}
			foreach (var state in stateMachine.states)
			{
				if (!output.Contains(state.state))
					output.Add(state.state);
			}


			return output;
		}

		/// <summary>Get all states in a child state machine.</summary>
		List<AnimatorState> GetStatesInChildMachine(ChildAnimatorStateMachine childMachine)
		{
			List<AnimatorState> output = new List<AnimatorState>();
			if (childMachine.stateMachine.stateMachines.Length > 0)
			{
				foreach (var sm in childMachine.stateMachine.stateMachines)
				{
					//Calls current function again if more child states are found inside this one.
					List<AnimatorState> childStates = GetStatesInChildMachine(sm);
					for (int i = 0; i < childStates.Count; i++)
					{
						if (!output.Contains(childStates[i]))
							output.Add(childStates[i]);
					}
				}
			}
			foreach (var state in childMachine.stateMachine.states)
			{
				if (!output.Contains(state.state))
					output.Add(state.state);
			}

			return output;
		}

		#endregion


		protected virtual void Awake()
		{
			animatorStates = GetAllStates();
		}



		/// <summary>
		/// Replaces a clip inside animator. Original animation is found with string name and replaced with inserted AnimationClip.
		/// </summary>
		/// <param name="animName">The currently existing animation in animator.</param>
		/// <param name="in_clip">Clip to replace the current animation with.</param>
		protected void OverrideClips(string animName, AnimationClip in_clip)
		{

			AnimatorOverrideController aoc = new AnimatorOverrideController();
			aoc.runtimeAnimatorController = Animator.runtimeAnimatorController;
			var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();
			foreach (var c in aoc.animationClips)
			{
				if (c.name == animName)
				{
					anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(c, in_clip));
					break;
				}
			}

			aoc.ApplyOverrides(anims);
			Animator.runtimeAnimatorController = aoc;
		}
	}
}
