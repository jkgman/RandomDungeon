using Systems.Targeting;
using UnityEngine;
namespace RandomDungeon.Agent
{
    public enum AgentState { Idle, Movement, Action }
    public enum Action { None, Attack, Roll }

    [RequireComponent(typeof(CharacterController))]
    public class AgentBody : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField]
        protected Animator animator;
        [Header("Asset References")]
        [SerializeField]
        protected TargetSystem targetSystem;
        [Header("Movement Parameters")]
        [SerializeField]
        protected float walkSpeed;
        [SerializeField]
        protected float runSpeed;
        [SerializeField]
        protected float rotateSpeed = 650;

        protected CharacterController control;

        protected float GetSpeed { get => running ? runSpeed : walkSpeed; }

        //State Info
        protected AgentState state = AgentState.Idle;
        protected Target focusTarget;
        protected bool lockState = false;
        protected bool running = false;
        protected Action nextAction = Action.None;

        //Inputs
        protected Vector3 currentMoveInput;
        protected Vector3 totalMovementVector;


        private void Start()
        {
            control = GetComponent<CharacterController>();
        }

        private void Update()
        {
            //if idle state or walking
            switch(state)
            {
                case AgentState.Idle:
                    if(nextAction != Action.None)
                        StartActionState(nextAction);
                    else if(currentMoveInput.magnitude > 0)
                        StartMoveState();
                    break;
                case AgentState.Movement:
                    if(nextAction != Action.None)
                        StartActionState(nextAction);
                    else if(currentMoveInput.magnitude > 0)
                        Move();
                    else
                        StartIdleState();
                    break;
                case AgentState.Action:
                    if(nextAction != Action.None)
                        StartActionState(nextAction);
                    break;
                default:
                    break;
            }
        }

        public void StartMoveState()
        {
            state = AgentState.Movement;
            if(running)
                animator.SetTrigger("Run");
            else
                animator.SetTrigger("Walk");
            Move();
        }

        public void StartIdleState()
        {
            state = AgentState.Idle;
            animator.SetTrigger("Idle");
        }
        public void StartActionState(Action action)
        {
            if(lockState)
            {
                nextAction = action;
                return;
            }
            state = AgentState.Action;
            //do action
            switch(action)
            {
                case Action.None:
                    return;
                case Action.Attack:
                    break;
                case Action.Roll:
                    break;
                default:
                    break;
            }

            //on action finish set to idle state
            //do nextAction if present
        }

        public void Move()
        {
            Vector3 moveVector = (totalMovementVector.magnitude > 0) ? totalMovementVector : currentMoveInput;
            moveVector = (moveVector.magnitude <= moveVector.normalized.magnitude) ? moveVector : moveVector.normalized;
            moveVector *= Time.deltaTime;

            if(focusTarget)
                LockedOnMovement(moveVector);
            else
                FreeMove(moveVector);

            totalMovementVector = Vector2.zero;
        }

        private void FreeMove(Vector3 moveVector)
        {
            moveVector *= GetSpeed;
            animator.SetFloat("WalkBlend", 0);

            float angle = Vector3.Angle(transform.forward, moveVector);
            Vector3 Move = transform.forward * moveVector.magnitude * ((360 - angle) / 360);
            control.Move(Move);

            if(angle > Time.deltaTime * rotateSpeed)
            {
                transform.Rotate(Vector3.up, Time.deltaTime * rotateSpeed * Mathf.Sign(Vector3.SignedAngle(transform.forward, moveVector, Vector3.up)));
            }
        }

        private void LockedOnMovement(Vector3 moveVector)
        {
            moveVector *= GetSpeed;
            Vector2 dir = new Vector2(moveVector.x, moveVector.z);
            float dirAngle = Vector2Extensions.GetDegree(dir);
            float targetAngle = Vector2Extensions.GetDegree(focusTarget.transform.position - transform.position);
            float result = (dirAngle - targetAngle) - Mathf.CeilToInt((dirAngle - targetAngle) / 360f) * 360f;
            if(result < 0)
            {
                result += 360f;
            }
            animator.SetFloat("WalkBlend", result);
            control.Move(moveVector);
            transform.LookAt(new Vector3(focusTarget.transform.position.x, transform.position.y, focusTarget.transform.position.z), Vector3.up);
        }


        public void FocusTarget()
        {
            focusTarget = targetSystem.FindClosestTarget(transform.position);
        }

        public void UnfocusTarget()
        {
            focusTarget = null;
        }


    }
}