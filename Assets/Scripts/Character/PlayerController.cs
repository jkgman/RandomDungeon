using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Systems.Targeting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RandomDungeon.Agent
{

    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : AgentBody
    {

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
                    animator.SetTrigger(runTrigger);
                }
            }
            else if(context.canceled) {
                running = false;
                if(state == AgentState.Movement)
                {
                    animator.SetTrigger(walkTrigger);
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
            nextAction = AgentAction.Attack;
        }

        public void OnRoll(InputAction.CallbackContext context)
        {
            if(!context.started)
                return;
            nextAction = AgentAction.Roll;
        }

        #endregion Input Handling



    }
}