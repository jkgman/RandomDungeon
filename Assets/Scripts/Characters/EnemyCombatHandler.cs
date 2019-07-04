using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Dungeon.Characters
{

	public class EnemyCombatHandler : CharacterCombatHandler
	{
		[SerializeField] private float targetFindDistance;
		[SerializeField] private float attackDelay;
		[SerializeField] private LayerMask obstacles;
		private float attackDelayTimer;
		private bool canAttack;
		private bool attackTimerReset;
		private Transform target;
		private NavMeshAgent navMeshAgent;
		private float targetSeenLastTime;


		protected override void Awake()
		{
			base.Awake();
			navMeshAgent = GetComponent<NavMeshAgent>();
		}

		public Vector3 GetTargetPosition()
		{
			if (!target)
			{
				var temp = GameObject.FindGameObjectWithTag("Player");
				if (temp)
					target = temp.transform;
			}

			return target.position;
		}

		public Vector3 GetTargetDirection()
		{
			return GetTargetPosition() - transform.position;
		}

		public bool CanSeeTarget()
		{

			if (Physics.Raycast(transform.position, GetTargetDirection(), navMeshAgent.stoppingDistance + targetFindDistance, obstacles))
			{
				//if the player is behind an obstacle
				return false;
			}
			else
			{
				targetSeenLastTime = Time.time;
				return true;
			}
		}

		public bool CanFindTarget()
		{
			if (targetSeenLastTime + 5f > Time.time)
			{
				//For 5 seconds after having seen target, it can see players position within its range.
				float sqrDist = (navMeshAgent.stoppingDistance + targetFindDistance) * (navMeshAgent.stoppingDistance + targetFindDistance);
				if (GetTargetDirection().sqrMagnitude < sqrDist)
					return true;
			}

			return false;
		}

		public bool CanAttack()
		{
			//Check that target is close enough and delayTimer has run

			if (attackDelayTimer < Time.time - attackDelay)
				canAttack = true;

			float sqrLen = GetTargetDirection().sqrMagnitude;
			if (sqrLen < navMeshAgent.stoppingDistance * navMeshAgent.stoppingDistance)
			{
				//Reset timer once
				if (!attackTimerReset)
				{
					canAttack = false;
					attackDelayTimer = Time.time;
					attackTimerReset = true;
				}
			}
			else
			{
				canAttack = false;
				attackTimerReset = false;
			}

			return canAttack;
		}

		public void AttackUpdate()
		{
			Attack();
		}

	}
}
