using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace Dungeon.Characters.Enemies
{
	using Dungeon.Items;

	public class EnemyCombatHandler : CharacterCombatHandler , IAllowedEnemyActions
	{
		#region Variables & References

		//_______ Start of Exposed Variables
		[Tooltip("Max distance to see and remember target position.")]
		[SerializeField] private float targetFindDistance = 10f; 
		[Tooltip("Max distance from target where attack will be carried.")]
		[SerializeField] private float maxAttackDistance = 1f; 
		[Tooltip("Delay before starting to attack and between attacks.")]
		[SerializeField] private float attackDelay = 0.5f;
		[Tooltip("Time until target can not be found anymore after seeing target last time.")]
		[SerializeField] private float forgetTargetDelay = 5f;
		[Tooltip("Obstacles that can block visibility to target.")]
		[SerializeField] private LayerMask obstacleMask = new LayerMask();
		//_______ End of Exposed Variables

		//_______ Start of Hidden Variables
		private float attackDelayTimer;
		private bool attackTimerReset;
		private Transform target;
		private float targetSeenLastTime;
		//_______ End of Hidden Variables

		//_______ Start of Class References
		private EnemyWeapon _eWeapon;
		private EnemyWeapon EWeapon
		{
			get
			{
				if (!_eWeapon) _eWeapon = GetComponentInChildren<EnemyWeapon>();
				if (_eWeapon) _eWeapon.CurrentEquipper = transform;
				return _eWeapon;
			}
		}
		private NavMeshAgent _agent;
		private NavMeshAgent Agent
		{
			get{
				if (!_agent)
					_agent = GetComponent<NavMeshAgent>();

				return _agent;
			}
		}
		private Enemy _enemy;
		private Enemy Enemy
		{
			get
			{
				if (!_enemy) _enemy = GetComponentInChildren<Enemy>();
				return _enemy;
			}
		}
		//_______ End of Class References

		#endregion Variables & References

		#region Getters & Setters

		/// <summary>
		/// Finds player in scene and gets its transform position.
		/// </summary>
		/// <returns></returns>
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

		/// <summary>
		/// Direction from current position to player (target)
		/// </summary>
		/// <returns></returns>
		public Vector3 GetTargetDirection()
		{
			return GetTargetPosition() - transform.position;
		}

		/// <summary>
		/// Check if target visibility is blocked by any obstacles
		/// </summary>
		/// <returns></returns>
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
	
		/// <summary>
		/// Check if target is close enough to traverse there. Uses a timer from last time target was seen and forgets target position if enough time has passed after last time seen.
		/// </summary>
		/// <returns></returns>
		public bool CanFindTarget()
		{
			if (targetSeenLastTime + forgetTargetDelay > Time.time)
			{
				//For 5 seconds after having seen target, it can see players position within its range.
				float sqrDist = (Agent.stoppingDistance + targetFindDistance) * (Agent.stoppingDistance + targetFindDistance);
				if (GetTargetDirection().sqrMagnitude < sqrDist)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Checks if target is visible and close enough.
		/// </summary>
		/// <returns></returns>
		private bool TargetIsAttackable()
		{
			//Check that target is close enough and delayTimer has run
			bool canAttack = true;

			if (!CanSeeTarget())
			{
				canAttack = false;
			}

			float sqrLen = GetTargetDirection().sqrMagnitude;
			if (sqrLen > maxAttackDistance * maxAttackDistance)
			{
				canAttack = false;
			}

			return canAttack;
		}

		#endregion Getters & Setters

		#region Initialization & Updates

		/// <summary>
		/// Called from Enemy when state is Attack.
		/// </summary>
		public void AttackUpdate(AttackType type)
		{
			bool canAttack = true;
			float sqrLen = GetTargetDirection().sqrMagnitude;
			if (sqrLen < Agent.stoppingDistance * Agent.stoppingDistance)
			{
				//Reset timer once
				if (!attackTimerReset)
				{
					canAttack = false;
					attackDelayTimer = Time.time;
					attackTimerReset = true;
				}
				else if (Time.time - attackDelayTimer < attackDelay)
				{
					canAttack = false;
				}
			}
			else
			{
				canAttack = false;
				attackTimerReset = false;
			}

			if (EWeapon)
			{
				if (!EWeapon.IsAttacking && canAttack)
				{
					Debug.Log("attack called");
					Attack(type);
				}
				else if (EWeapon.IsAttacking)
				{
					attackTimerReset = false;
				}
			}
		}

		#endregion Initialization & Updates

		#region IAllowedActions

		public bool AllowMove()
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;

			return output;
		}

		public bool AllowRun()
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;
			output = IsBlocking ? false : output;

			if (EWeapon && EWeapon.IsAttacking)
				output =  false;

			return output;
		}

		public bool AllowAttack()
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;
			output = TargetIsAttackable() ? output : false;
			if (EWeapon && EWeapon.IsAttacking)
				output = EWeapon.ComboPendingAllowed(elapsedAttackTime) ? output : false;

			return output;
		}

		public bool AllowRotate()
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;

			return output;
		}

		#endregion
	}
}
