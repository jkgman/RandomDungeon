using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace INDEV.Player
{
    public enum MoveState { walk, run, strafe }

    public class PlayerController : MonoBehaviour
    {

        private Vector3 MovementVector;

        void Input(Vector3 newMovement)
        {
            //add newMovement vector normalized into movement vector
        }

        // Update is called once per frame
        void Update()
        {
            //Move movement vector amount 

            //run any combat commands
        }

        public async void Move() { }

        public async void Roll() { }
        public async void LockOn() { }
    }
}