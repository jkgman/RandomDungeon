using Dungeon.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace INDEV.Enemy {
    public class EnemyBehavior : MonoBehaviour
    {
        Dungeon.Characters.Player player;
        Seeker seeker;
        void Start()
        {
            player = FindObjectOfType<Dungeon.Characters.Player>();
            Seeker seeker = GetComponent<Seeker>();
            if (!player)
            {
                throw new System.Exception("no player found");
            }
        }
        void Update() {
            //if player is close enough
            //attack
            //if player close enough
            //walk to player
            //else
            //idle
        }
        public void OnPathComplete(Path p)
        {
            Debug.Log("Yay, we got a path back. Did it have an error? " + p.error);
        }
    }
}