using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Systems.Targeting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RandomDungeon.Agent
{
    public enum MoveState { Idle, Walk, Run, Attack, Roll }

    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : AgentBody
    {
        [Header("Roll Parameters")]
        [SerializeField]
        private float rollMagnitude = 1;
        [SerializeField]
        private AnimationCurve rollCurve;

        //Make sure the values match up with the animators triggers
        private Dictionary<MoveState, string> stateDictionary = new Dictionary<MoveState, string>()
        {
            { MoveState.Walk, "Walk"},
            { MoveState.Run, "Run" },
            { MoveState.Idle, "Idle"},
            { MoveState.Attack, "Attack"},
            { MoveState.Roll, "Roll"}
        };


        #region Input Handling

        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            Vector3 projectedVector = new Vector3(input.x, 0, input.y);
            currentMoveInput = projectedVector;
            totalMovementVector += projectedVector;
        }

        public void OnRun(InputAction.CallbackContext context)
        {
            if(context.started)
            {
                UnfocusTarget();
                running = true;
                if(state == AgentState.Movement)
                {
                    animator.SetTrigger("Run");
                }
            }
            else if(context.canceled) {
                running = false;
                if(state == AgentState.Movement)
                {
                    animator.SetTrigger("Walk");
                }
            }
        }

        public void OnTargetLock(InputAction.CallbackContext context)
        {
            if(!context.started || running)
                return;

            if(focusTarget == null)
            {
                FocusTarget();
            }
            else
            {
                UnfocusTarget();
            }
        }

        public void OnAttack(InputAction.CallbackContext context) 
        {
            if(!context.started)
                return;
            nextAction = Action.Attack;
        }

        public void OnRoll(InputAction.CallbackContext context)
        {
            if(!context.started)
                return;
            nextAction = Action.Roll;
        }

        #endregion Input Handling


        #region Priority States

        //priority states cancel whatever your currently doing to execute them if current state is cancelable, otherwise queues up for next action
        private async Task Attack()
        {
            await Awaiters.Seconds(.867f);
            await Awaiters.NextFrame;
            totalMovementVector = Vector2.zero;
        }

        private async Task Roll()
        {
            Vector3 dir;
            if(currentMoveInput.magnitude > 0)
                dir = currentMoveInput.normalized;
            else
                dir = transform.forward;
            await AsyncAnimation.GlobalTranslate(transform, transform.position + dir * rollMagnitude, rollCurve, 2.4f);
            totalMovementVector = Vector2.zero;
        }
        #endregion Priority States

        private async Task WaitForAnimation(string Animation)
        {
            while(animator.GetCurrentAnimatorStateInfo(0).IsName(Animation) || animator.GetNextAnimatorStateInfo(0).IsName(Animation))
            {
                Debug.Log("await");
                await Awaiters.NextFrame;
            }
        }


    }
}