using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace INDEV.Player
{
    public enum MoveState { walk, run, strafe }

    public class PlayerController : MonoBehaviour
    {
        private void Start()
        {
        }
        private Vector3 MovementVector;

        void Update()
        {
            //Move movement vector amount 

            //run any combat commands
        }

        public async void Move() {
            //start move animation blended towards direction, or continue it
        }

        public async void Roll() {
            //cancel current animation and start rolling, this locks movement
        }


    }
}