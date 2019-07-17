//using System;
using System;
using UnityEngine;
using UnityEngine.AI;


namespace Dungeon.Characters.Enemies
{

	public class EnemyController : CharacterController, IAllowedEnemyActions
	{

		private bool newDestination;
		public Vector3 currentDestination;
		[SerializeField] private float maxStrollDistance;
		[SerializeField] private bool calculateStrollDistanceFromSpawn;
		private Vector3 startPoint;
		private Vector3 lastPosition;


		#region Getters & Setters		

		private NavMeshAgent _agent;
		private NavMeshAgent Agent
		{
			get
			{
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
				if (!_enemy)
					_enemy = GetComponent<Enemy>();
				return _enemy;
			}
		}


		public Vector3 GetDirection(Vector3 target)
		{
			return target - transform.position;
		}


		#endregion

		
		private void OnEnable()
		{
			startPoint = transform.position;
			if (Agent)
			{
				Agent.speed = moveSpeed;
				Agent.updateRotation = false;
			}
		}

		private void LateUpdate()
		{
			lastPosition = transform.position;
		}

		public void Stroll()
		{
			if (Agent.isOnNavMesh)
			{
				SetRunning(false);
				bool atDestination = (transform.position - Agent.destination).sqrMagnitude <= Agent.stoppingDistance * Agent.stoppingDistance * 1.1f;
				if ((!Agent.pathPending && !Agent.hasPath) || atDestination || Agent.isPathStale)
				{
					currentDestination = CreateNewDestination();
					Agent.SetDestination(currentDestination);
				}
			}

			RotateTowardsMovement();
			UpdateAnimationData();
		}
		public void Idle()
		{
			if (Agent.isOnNavMesh)
			{
				if (!Agent.isStopped)
					Agent.SetDestination(transform.position);
			}
			UpdateAnimationData();
		}

		private Vector3 CreateNewDestination()
		{
			Vector3 output = Vector3.zero;
			Vector3 originalPosition = calculateStrollDistanceFromSpawn ? startPoint : transform.position;
			
			Vector2 offset = UnityEngine.Random.insideUnitCircle * maxStrollDistance;
			output = new Vector3(offset.x + originalPosition.x, transform.position.y, offset.y + originalPosition.z);
			

			return output;
		}

		public void Following(Vector3 targetPosition, bool targetVisible)
		{ 
			if (Agent.isOnNavMesh)
				Agent.SetDestination(targetPosition);

			if (targetVisible)
			{
				SetRunning(true);
				RotateTowardsPosition(targetPosition);
			}
			else
			{
				SetRunning(false);
				RotateTowardsMovement();
			}
			UpdateAnimationData();
		}

		private void RotateTowardsMovement()
		{
			if (!Enemy.AllowRotate())
				return;

			Vector3 dir = transform.position - lastPosition;
			dir.y = 0;
			if (dir.sqrMagnitude != 0)
			{
				Quaternion targetRotation = Quaternion.LookRotation(dir);
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed);
			}
		}


		public void RotateTowardsPosition(Vector3 in_position)
		{
			if (!Enemy.AllowRotate())
				return;

			Vector3 dir = GetDirection(in_position);
			dir.y = 0;
			Quaternion targetRotation = Quaternion.LookRotation(dir);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed);
		}

		protected override void SetRunning(bool value)
		{
			base.SetRunning(value);

			if (Agent)
			{
				Agent.speed = GetRunning() ? moveSpeed * runSpeedMultiplier : moveSpeed;

			}
		}
		public override void ExternalMove(Vector3 offset)
		{
			Agent.ResetPath();
			Agent.Move(offset);
		}

		#region Animations


		void UpdateAnimationData()
		{
			bool stopped = !Agent.isOnNavMesh || Agent.isStopped;
			float movePercentage = GetRunning() ? 1f : !stopped ? 0.5f : 0;
			
			Vector2 blend = new Vector2(0, movePercentage);

			AnimHandler.SetMovementPerformed(!stopped, blend);

		}

		#endregion



		#region IAllowedActions

		public bool AllowMove()
		{
			//This script currently does not have anything disabling its own actions.
			return true;
		}

		public bool AllowRun()
		{
			//This script currently does not have anything disabling its own actions.
			return true;
		}
		public bool AllowRotate()
		{
			//This script currently does not have anything disabling its own actions.
			return true;
		}
		public bool AllowAttack()
		{
			//This script currently does not have anything disabling other classes' actions.
			return true;
		}

		#endregion
	}
}
