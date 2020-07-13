using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.Targeting {
    [CreateAssetMenu(fileName = "TargetSystem", menuName = "Systems/TargetSystem")]
    public class TargetSystem : ScriptableObject
    {
        List<Target> targets = new List<Target>();

        public void Register(Target target) 
        {
            if(!targets.Contains(target))
            {
                targets.Add(target);
            }
        }

        public void Unregister(Target target)
        {
            targets.Remove(target);
        }

        public Target FindClosestTarget(Vector3 searchPos) 
        {
            if(targets.Count <= 0)
                return null;

            Target closest = targets[0];
            foreach(var target in targets)
            {
                if((searchPos - target.transform.position).magnitude < (searchPos - closest.transform.position).magnitude)
                    closest = target;
            }
            return closest;
        }
    }
}
