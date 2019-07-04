//using System;
using System;
using UnityEngine;
using UnityEngine.AI;


namespace Dungeon.Characters.Enemies
{

	public class EnemyController : CharacterController
	{

		private bool newDestination;
		private Vector3 currentDestination;
		private NavMeshAgent navMeshAgent;
		[SerializeField] private float maxStrollDistance;
		[SerializeField] private bool calculateStrollDistanceFromSpawn;
		private Vector3 startPoint;
		private Vector3 lastPosition;


		#region Getters & Setters		

		EnemyAnimationHandler _eAnimHandler;
		EnemyAnimationHandler EAnimHandler
		{
			get
			{
				if (!_eAnimHandler)
					_eAnimHandler = GetComponentInChildren<EnemyAnimationHandler>();

				return _eAnimHandler;
			}
		}

		public Vector3 GetDirection(Vector3 target)
		{
			return target - transform.position;
		}


		#endregion


		private void Awake()
		{
			navMeshAgent = GetComponent<NavMeshAgent>();
		}

		private void OnEnable()
		{
			startPoint = transform.position;
		}

		private void LateUpdate()
		{
			lastPosition = transform.position;
		}

		public void Stroll()
		{
			bool atDestination = (transform.position - currentDestination).sqrMagnitude <= navMeshAgent.stoppingDistance * navMeshAgent.stoppingDistance + 0.1f;
			if ((!navMeshAgent.pathPending && !navMeshAgent.hasPath) || atDestination || navMeshAgent.isPathStale)
			{
				currentDestination = CreateNewDestination();
				navMeshAgent.SetDestination(currentDestination);
			}
		}
		public void Idle()
		{
			if (!navMeshAgent.isStopped)
				navMeshAgent.SetDestination(transform.position);
		}

		private Vector3 CreateNewDestination()
		{
			Vector3 output = Vector3.zero;
			Vector3 originalPosition = calculateStrollDistanceFromSpawn ? startPoint : transform.position;
			
			Vector2 offset = UnityEngine.Random.insideUnitCircle * maxStrollDistance;
			output = new Vector3(offset.x, 0, offset.y) + originalPosition;
			Debug.Log("New destination: " + output);

			return output;
		}

		public void Following(Vector3 targetPosition, bool targetVisible)
		{ 
			navMeshAgent.SetDestination(targetPosition);

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

		}

		private void RotateTowardsMovement()
		{
			Vector3 dir = transform.position - lastPosition;
			dir.y = 0;
			Quaternion targetRotation = Quaternion.LookRotation(dir);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed);
		}


		public void RotateTowardsPosition(Vector3 in_position)
		{
			Vector3 dir = GetDirection(in_position);
			dir.y = 0;
			Quaternion targetRotation = Quaternion.LookRotation(dir);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed);
		}




		#region Animations


		void UpdateAnimationData()
		{
			
			float movePercentage = GetRunning() ? 1f : !navMeshAgent.isStopped ? 0.5f : 0;
			
			Vector2 blend = new Vector2(0, movePercentage);

			AnimHandler.SetMovementPerformed(blend);

		}

		#endregion
	}
}
