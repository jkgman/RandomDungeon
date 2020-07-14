using System.Threading.Tasks;
using RandomDungeon.Combat;
using Systems.Targeting;
using UnityEngine;
namespace RandomDungeon.Agent
{
    public enum AgentState { Idle, Movement, Action }
    public enum AgentAction { None, Attack, Roll }

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
        protected float walkSpeed = 1;
        [SerializeField]
        protected float runSpeed = 2;
        [SerializeField]
        protected float rotateSpeed = 650;

        [Header("Roll Parameters")]
        [SerializeField]
        private float rollMagnitude = 1;
        [SerializeField]
        private AnimationCurve rollCurve;
        [SerializeField]
        private float rollDuration = 1;
        protected CharacterController control;

        [Header("Attack Parameters")]
        [SerializeField]
        private CombatWeapon weapon;

        [Header("Animation Triggers"), Tooltip("Make sure the hooked up animator uses these triggers for state handling")]
        [SerializeField]
        private string rollTrigger = "Roll";
        [SerializeField]
        private string attackTrigger = "Attack";
        [SerializeField]
        private string walkTrigger = "Walk";
        [SerializeField]
        private string runTrigger = "Run";
        [SerializeField]
        private string idleTrigger = "Idle";

        [SerializeField]
        private string walkBlendName = "WalkBlend";

        [SerializeField]
        private string idleStateName = "Base Layer.Idle";

        protected float GetSpeed { get => running ? runSpeed : walkSpeed; }

        //State Info
        protected AgentState state = AgentState.Idle;
        protected Target focusTarget;
        protected bool lockState = false;
        protected bool running = false;
        protected AgentAction nextAction = AgentAction.None;

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
                    if(nextAction != AgentAction.None)
                        StartActionState(nextAction);
                    else if(currentMoveInput.magnitude > 0)
                        StartMoveState();
                    break;
                case AgentState.Movement:
                    if(nextAction != AgentAction.None)
                        StartActionState(nextAction);
                    else if(currentMoveInput.magnitude > 0)
                        Move();
                    else
                        StartIdleState();
                    break;
                case AgentState.Action:
                    if(nextAction != AgentAction.None)
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
                animator.SetTrigger(runTrigger);
            else
                animator.SetTrigger(walkTrigger);
            Move();
        }

        public void StartIdleState()
        {
            state = AgentState.Idle;
            animator.SetTrigger(idleTrigger);
        }

        public async Task StartActionState(AgentAction action)
        {
            if(lockState)
            {
                nextAction = action;
                return;
            }
            lockState = true;
            state = AgentState.Action;
            nextAction = AgentAction.None;
            switch(action)
            {
                case AgentAction.Attack:
                    await Attack();
                    break;
                case AgentAction.Roll:
                    await Roll();
                    break;
                case AgentAction.None:
                default:
                    return;
            }
            lockState = false;
            totalMovementVector = Vector2.zero;
            if(nextAction == AgentAction.None)
            {
                StartIdleState();
            }
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
            animator.SetFloat(walkBlendName, 0);

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
            animator.SetFloat(walkBlendName, result);
            control.Move(moveVector);
            transform.LookAt(new Vector3(focusTarget.transform.position.x, transform.position.y, focusTarget.transform.position.z), Vector3.up);
        }

        private async Task Attack()
        {
            weapon.Activate();
            animator.SetTrigger(attackTrigger);
            await AsyncAnimation.WaitForAnimation(idleStateName, animator);
            weapon.DeActivate();
        }

        private async Task Roll()
        {
            Vector3 dir;

            if(currentMoveInput.magnitude > 0)
                dir = currentMoveInput.normalized;
            else
                dir = transform.forward;

            animator.SetTrigger(rollTrigger);
            AsyncAnimation.GlobalTranslate(transform, transform.position + dir * rollMagnitude, rollCurve, rollDuration);
            await AsyncAnimation.WaitForAnimation(idleStateName, animator);
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