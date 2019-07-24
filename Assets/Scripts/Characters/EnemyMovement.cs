using System;
using UnityEngine;
using UnityEngine.AI;


namespace Dungeon.Characters.Enemies
{

	public class EnemyMovement : CharacterMovement, IAllowedEnemyActions
	{

		#region Variables & References

		//_______ Start of Exposed Variables
		[Tooltip("Max distance from where new random stroll point is found.")]
		[SerializeField] private float maxStrollDistance = 5f;
		[Tooltip("If true, new random stroll point is within maxStrollDistance from spawn point." +
				 "If False, new point is calculated from current position instead.")]
		[SerializeField] private bool calculateStrollDistanceFromSpawn = false;
		//_______ End of Exposed Variables

		//_______ Start of Hidden Variables
		private Vector3 currentDestination; //Strolling destination.
		private Vector3 startPoint; //Spawn position.
		private Vector3 lastPosition; //Set on lateUpdate. Used to determine rotation.
		//_______ End of Hidden Variables

		#endregion Variables & References

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
		private EnemyAnimationHandler _eAnimHandler;
		private EnemyAnimationHandler EAnimHandler
		{
			get
			{
				if (!_eAnimHandler)
					_eAnimHandler = GetComponent<EnemyAnimationHandler>();
				return _eAnimHandler;
			}
		}

		public Vector3 GetDirection(Vector3 target)
		{
			return target - transform.position;
		}

		#endregion Getters & Setters

		#region Initialization & Updates

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

		#endregion Initialization & Updates

		#region State Updates

		/// <summary>
		/// Sets new strolling destinations for Agent. Updates rotation and animations.
		/// </summary>
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
		
		/// <summary>
		/// Enemy is just chilling around like some cool dude. Updates animations
		/// </summary>
		public void Idle()
		{
			if (Agent.isOnNavMesh)
			{
				if (!Agent.isStopped)
					Agent.SetDestination(transform.position);
			}
			UpdateAnimationData();
		}
		/// <summary>
		/// Sets destination to target. If target visible, enemy runs. Updates animations
		/// </summary>
		/// <param name="targetPosition"></param>
		/// <param name="targetVisible"></param>
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

		#endregion State Updates

		#region Hidden Functions

		/// <summary>
		/// Creates a strolling destination according to maxDistance and checks if it should be from spawn point or current position.
		/// </summary>
		private Vector3 CreateNewDestination()
		{
			Vector3 output = Vector3.zero;
			Vector3 originalPosition = calculateStrollDistanceFromSpawn ? startPoint : transform.position;
			
			Vector2 offset = UnityEngine.Random.insideUnitCircle * maxStrollDistance;
			output = new Vector3(offset.x + originalPosition.x, transform.position.y, offset.y + originalPosition.z);
			

			return output;
		}

		/// <summary>
		/// Gets direction from last and current positions and updates rotation.
		/// </summary>
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

		/// <summary>
		/// Updates variables related to running state.
		/// </summary>
		/// <param name="value"></param>
		protected override void SetRunning(bool value)
		{
			base.SetRunning(value);

			if (Agent)
			{
				Agent.speed = GetRunning() ? moveSpeed * runSpeedMultiplier : moveSpeed;
			}
		}

		#endregion Hidden Functions

		#region Exposed Functions

		/// <summary>
		/// Rotates towards a specific position, usually the target.
		/// </summary>
		public void RotateTowardsPosition(Vector3 in_position)
		{
			if (!Enemy.AllowRotate())
				return;

			Vector3 dir = GetDirection(in_position);
			dir.y = 0;
			Quaternion targetRotation = Quaternion.LookRotation(dir);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed);
		}

		/// <summary>
		/// Forced movement from outside the class.
		/// </summary>
		/// <param name="offset"></param>
		public override void ExternalMove(Vector3 offset)
		{
			Agent.ResetPath();
			Agent.Move(offset);
		}

		#endregion Exposed Functions

		#region Animations

		/// <summary>
		/// Sets animation to idle/move according to movement data.
		/// </summary>
		void UpdateAnimationData()
		{
			bool stopped = !Agent.isOnNavMesh || Agent.isStopped;
			float movePercentage = GetRunning() ? 1f : !stopped ? 0.5f : 0;
			
			Vector2 blend = new Vector2(0, movePercentage);

			EAnimHandler.SetMovementPerformed(!stopped, blend);

		}

		#endregion Animations

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
