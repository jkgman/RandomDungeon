using Pathfinding;
using UnityEngine;

namespace RandomDungeon.Agent
{
    [RequireComponent(typeof(Seeker))]
    public class EnemyController : AgentBody
    {

        private PlayerController _player;
        private AIPath _Pather;

        [Header("Combat Variables")]
        [SerializeField]
        private float sightDistance = 5;


        private PlayerController Player
        {
            get {
                if(!_player)
                    _player = FindObjectOfType<PlayerController>();
                return _player;
            }
        }
        private AIPath Pather
        {
            get {
                if(!_Pather)
                    _Pather = GetComponent<AIPath>();
                return _Pather;
            }
        }


        void Update()
        {
            //when moving once player gets close enough go from pathfinding to control
            switch(state)
            {
                case AgentState.Idle:
                    //if player in attack range attack
                    //if player in sight path towards
                    //else patrol
                    break;
                case AgentState.Movement:
                    //if moving to player and player moved repath
                    //if reached end of path go to idle
                    //else continue moving
                    break;
                case AgentState.Action:
                    break;
                default:
                    break;
            }
        }

        public void OnPathComplete(Path p)
        {
            Debug.Log("Yay, we got a path back. Did it have an error? " + p.error);
        }


        //public void OnMove(InputAction.CallbackContext context)
        //{
        //    Vector2 input = context.ReadValue<Vector2>();
        //    Vector3 projectedVector = new Vector3(input.x, 0, input.y);
        //    currentMoveInput = projectedVector;
        //    totalMovementVector += projectedVector;
        //}

        public void Run(bool start)
        {
            if(start)
            {
                UnfocusTarget();
                running = true;
                if(state == AgentState.Movement)
                {
                    animator.SetTrigger("Run");
                }
            }
            else
            {
                running = false;
                if(state == AgentState.Movement)
                {
                    animator.SetTrigger("Walk");
                }
            }
        }

        public void OnTargetLock()
        {
            if(running)
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

        public void Attack()
        {
            nextAction = AgentAction.Attack;
        }

        public void Roll()
        {
            nextAction = AgentAction.Roll;
        }

    }
}