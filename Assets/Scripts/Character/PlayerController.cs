using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Systems.Targeting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace INDEV.Player
{
    public enum MoveState { Idle, Walk, Run, Attack, Roll }

    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField]
        private Animator animator;
        [Header("Asset References")]
        [SerializeField]
        private TargetSystem targetSystem;
        [Header("Control Parameters")]
        [SerializeField]
        private float walkSpeed;
        [SerializeField]
        private float runSpeed;
        [SerializeField]
        private float rotateSpeed = 90;

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
        private CharacterController control;

        private MoveState currentState = MoveState.Idle;

        private bool running = false;

        private bool lockedOn = false;
        private Target lockOnTarget;

        private Vector3 direction = Vector3.forward;
        private Vector3 currentMoveInput;
        private Vector3 totalMovementVector;

        private void Start()
        {
            control = GetComponent<CharacterController>();
        }

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
            if(context.started) { 
                lockedOn = false;
                running = true;
            }
            else if(context.canceled)
                running = false;
        }

        public void OnTargetLock(InputAction.CallbackContext context)
        {
            if(!context.started)
                return;

            if(!lockedOn)
            {
                lockedOn = true;
                lockOnTarget = targetSystem.FindClosestTarget(transform.position);
            }
            else
            {
                lockedOn = false;
            }
        }

        public void OnAttack(InputAction.CallbackContext context) {
            if(!context.started || currentState == MoveState.Roll || currentState == MoveState.Attack)
                return;
            Attack();
        }

        public void OnRoll(InputAction.CallbackContext context)
        {
            if(!context.started || currentState == MoveState.Roll || currentState == MoveState.Attack)
                return;
            Roll();
        }

        #endregion Input Handling

        void Update()
        {
            if(currentState == MoveState.Roll || currentState == MoveState.Attack)
            {
                return;
            }
            if(!EvaluateMovement())
            {
                ChangeState(MoveState.Idle);
            }
        }

        private bool EvaluateMovement()
        {
            bool moved = false;
            Vector3 moveVector = (totalMovementVector.magnitude > 0) ? totalMovementVector : currentMoveInput;
            moveVector = (moveVector.magnitude <= moveVector.normalized.magnitude) ? moveVector : moveVector.normalized;
            moveVector *= Time.deltaTime;

            if(moveVector.magnitude > 0)
            {
                if(lockedOn)
                    LockedOnMovement(moveVector);
                else
                    FreeMove(moveVector);
                moved = true;
            }
            totalMovementVector = Vector2.zero;
            direction = transform.forward;
            return moved;
        }

        private void FreeMove(Vector3 moveVector) 
        {
            if(running)
            {
                moveVector *= runSpeed;
                ChangeState(MoveState.Run);
            }
            else
            {
                moveVector *= walkSpeed;
                ChangeState(MoveState.Walk);
                animator.SetFloat("WalkBlend", 0);
                //    Vector2 dir = new Vector2(moveVec.x, moveVec.z);
                //    float angle = Vector2Extensions.GetDegree(dir);
                //    animator.SetFloat("WalkBlend", angle);
            }

            float angle = Vector3.Angle(direction, moveVector);
            Vector3 Move = direction * moveVector.magnitude * ((360 - angle) / 360);
            control.Move(Move);

            if(angle > Time.deltaTime * rotateSpeed)
            {
                transform.Rotate(Vector3.up, Time.deltaTime * rotateSpeed * Mathf.Sign(Vector3.SignedAngle(direction, moveVector, Vector3.up)));
            }
        }

        private void LockedOnMovement(Vector3 moveVector) {
            moveVector *= walkSpeed;
            ChangeState(MoveState.Walk);
            Vector2 dir = new Vector2(moveVector.x, moveVector.z);
            float dirAngle = Vector2Extensions.GetDegree(dir);
            float targetAngle = Vector2Extensions.GetDegree(lockOnTarget.transform.position - transform.position);
            float result = (dirAngle - targetAngle) - Mathf.CeilToInt((dirAngle - targetAngle) / 360f) * 360f;
            if(result < 0)
            {
                result += 360f;
            }
            animator.SetFloat("WalkBlend", result);
            control.Move(moveVector);
            transform.LookAt(new Vector3(lockOnTarget.transform.position.x,transform.position.y, lockOnTarget.transform.position.z), Vector3.up);
        }
        
        #region Priority States

        //priority states cancel whatever your currently doing to execute them if current state is cancelable, otherwise queues up for next action
        private async Task Attack()
        {
            ChangeState(MoveState.Attack);
            await Awaiters.Seconds(.867f);
            await Awaiters.NextFrame;
            totalMovementVector = Vector2.zero;
            currentState = MoveState.Idle;
        }

        private async Task Roll()
        {
            ChangeState(MoveState.Roll);
            Vector3 dir;
            if(currentMoveInput.magnitude > 0)
                dir = currentMoveInput.normalized;
            else
                dir = direction;
            await AsyncAnimation.GlobalTranslate(transform, transform.position + dir * rollMagnitude, rollCurve, 2.4f);
            totalMovementVector = Vector2.zero;
            currentState = MoveState.Idle;
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

        private async Task Move(Vector3 destination, AnimationCurve curve, float duration) {

        }

        private void ChangeState(MoveState state) {
            if(state != currentState) {
                animator.SetTrigger(stateDictionary[state]);
                currentState = state;
            }
        }
    }
}