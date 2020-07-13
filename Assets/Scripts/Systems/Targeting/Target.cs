using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.Targeting
{
    public class Target : MonoBehaviour
    {
        [SerializeField]
        private TargetSystem targetSystem;
        private void OnEnable()
        {
            targetSystem.Register(this);
        }
        private void OnDisable()
        {
            targetSystem.Unregister(this);
        }
    }
}