using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace INDEV.Player
{
    public enum MoveState { Walk, Run, Idle, BackStrafe }

    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField]
        private Animator animator;
        [Header("Control Parameters")]
        [SerializeField]
        private float walkSpeed;
        [SerializeField]
        private float runSpeed;
        [SerializeField]
        private float rotateSpeed = 90;

        //Make sure the values match up with the animators triggers
        private Dictionary<MoveState, string> stateDictionary = new Dictionary<MoveState, string>()
        {
            { MoveState.Walk, "Walk"},
            { MoveState.Run, "Run" },
            { MoveState.Idle, "Idle"}
        };
        private CharacterController control;
        private MoveState currentState;
        private bool running = false;
        private bool lockedOn = false;

        private Vector3 direction = Vector3.forward;
        private Vector3 currentMoveInput;
        private Vector3 totalMovementVector;

        private void Start()
        {
            control = GetComponent<CharacterController>();
        }

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
                running = true;
            else if(context.canceled)
                running = false;
        }



        void Update()
        {
            Move();
        }

        public void Move()
        {
            Vector3 moveVec = (totalMovementVector.magnitude > 0) ? totalMovementVector : currentMoveInput;
            moveVec = (moveVec.magnitude <= moveVec.normalized.magnitude) ? moveVec : moveVec.normalized;
            totalMovementVector = Vector2.zero;
            if(moveVec.magnitude > 0)
            {
                moveVec *= Time.deltaTime;
                if(running)
                {
                    moveVec *= runSpeed;
                    ChangeState(MoveState.Run);
                }
                else
                {
                    moveVec *= walkSpeed;
                    ChangeState(MoveState.Walk);
                    animator.SetFloat("WalkBlend", 0);
                    //    Vector2 dir = new Vector2(moveVec.x, moveVec.z);
                    //    float angle = Vector2Extensions.GetDegree(dir);
                    //    animator.SetFloat("WalkBlend", angle);
                }

                float angle = Vector3.Angle(direction, moveVec);
                Vector3 Move = direction * moveVec.magnitude * ((360-angle) / 360);
                control.Move(Move);

                if(angle > Time.deltaTime * rotateSpeed)
                {
                    transform.Rotate(Vector3.up, Time.deltaTime * rotateSpeed * Mathf.Sign(Vector3.SignedAngle(direction, moveVec, Vector3.up)));
                }

            }
            else
            {
                ChangeState(MoveState.Idle);
            }
            direction = transform.forward;
        }

        public void Roll() {
            //cancel current animation and start rolling, this locks movement
        }

        private void ChangeState(MoveState state) {
            if(state != currentState) {
                animator.ResetTrigger(stateDictionary[currentState]);
                animator.SetTrigger(stateDictionary[state]);
                currentState = state;
            }
        }
    }
}