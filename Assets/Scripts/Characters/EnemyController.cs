//using System;
using UnityEngine;
using UnityEngine.AI;


namespace Dungeon.Characters.Enemies
{

	public class EnemyController : CharacterController
	{

		private bool isRunning;
		private bool newDestination;
		private Vector3 currentDestination;
		private NavMeshAgent navMeshAgent;



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


		private void Strolling()
		{
			if (newDestination)
				navMeshAgent.SetDestination(currentDestination);

		}

		public void Following(Vector3 targetPosition, bool targetVisible)
		{ 
			navMeshAgent.SetDestination(targetPosition);

			if (targetVisible)
				RotateTowardsPosition(targetPosition);
			else
				RotateTowardsMovement();

		}

		private void RotateTowardsMovement()
		{
			Vector3 dir = navMeshAgent.nextPosition - transform.position;
			dir.y = 0;
			Quaternion targetRotation = Quaternion.LookRotation(dir);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed);
		}


		public void RotateTowardsPosition(Vector3 in_position)
		{
			Quaternion targetRotation = Quaternion.LookRotation(GetDirection(in_position));
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed);
		}




		#region Animations


		void UpdateAnimationData()
		{
			
			float movePercentage = isRunning ? 1f : !navMeshAgent.isStopped ? 0.5f : 0;
			
			Vector2 blend = new Vector2(0, movePercentage);

			AnimHandler.SetMovementPerformed(blend);

		}

		#endregion
	}
}
