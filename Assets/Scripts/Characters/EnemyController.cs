//using System;
using System;
using UnityEngine;
using UnityEngine.AI;


namespace Dungeon.Characters.Enemies
{

	public class EnemyController : CharacterController
	{

		private bool newDestination;
		public Vector3 currentDestination;
		private NavMeshAgent navMeshAgent;
		[SerializeField] private float maxStrollDistance;
		[SerializeField] private bool calculateStrollDistanceFromSpawn;
		private Vector3 startPoint;
		private Vector3 lastPosition;


		#region Getters & Setters		
		
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

			if (navMeshAgent.isOnNavMesh)
			{
				bool atDestination = (transform.position - navMeshAgent.destination).sqrMagnitude <= navMeshAgent.stoppingDistance * navMeshAgent.stoppingDistance * 1.1f;
				if ((!navMeshAgent.pathPending && !navMeshAgent.hasPath) || atDestination || navMeshAgent.isPathStale)
				{
					currentDestination = CreateNewDestination();
					navMeshAgent.SetDestination(currentDestination);
				}
			}
			UpdateAnimationData();
		}
		public void Idle()
		{
			if (navMeshAgent.isOnNavMesh)
			{
				if (!navMeshAgent.isStopped)
					navMeshAgent.SetDestination(transform.position);
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
			if (navMeshAgent.isOnNavMesh)
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
			UpdateAnimationData();
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
			bool stopped = !navMeshAgent.isOnNavMesh || navMeshAgent.isStopped;
			float movePercentage = GetRunning() ? 1f : !stopped ? 0.5f : 0;
			
			Vector2 blend = new Vector2(0, movePercentage);

			AnimHandler.SetMovementPerformed(blend);

		}

		#endregion



		void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(currentDestination, .2f);
		}
	}
}
