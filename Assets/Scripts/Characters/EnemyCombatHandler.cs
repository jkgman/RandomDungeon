using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace Dungeon.Characters
{
	using Dungeon.Items;

	public class EnemyCombatHandler : CharacterCombatHandler
	{
		[SerializeField] private float targetFindDistance;
		[SerializeField] private float attackDelay;
		[SerializeField] private LayerMask obstacleMask;
		private float attackDelayTimer;
		private bool attackTimerReset;
		private Transform target;
		private NavMeshAgent navMeshAgent;
		private float targetSeenLastTime;
		protected EnemyWeapon CurrentWeapon
		{
			get { return (EnemyWeapon)GetCurrentWeapon(); }
		}


		protected override void OnEnable()
		{
			base.OnEnable();
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

			if (Physics.Raycast(transform.position, GetTargetDirection(), GetTargetDirection().magnitude, obstacleMask))
			{
				return false;
			}
			else
			{
				//if the player is behind an obstacle
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

		public bool TargetIsAttackable()
		{
			//Check that target is close enough and delayTimer has run
			bool canAttack = true;

			if (!CanSeeTarget())
			{
				canAttack = false;
			}

			float sqrLen = GetTargetDirection().sqrMagnitude;
			if (sqrLen > navMeshAgent.stoppingDistance * navMeshAgent.stoppingDistance)
			{
				canAttack = false;
			}

			return canAttack;
		}

		public void AttackUpdate()
		{
			bool canAttack = true;
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

			if (!CurrentWeapon.IsAttacking && canAttack)
			{
				Debug.Log("attack called");
				Attack();
			}
			else if (CurrentWeapon.IsAttacking)
			{
				attackTimerReset = false;
			}
		}

	}
}
