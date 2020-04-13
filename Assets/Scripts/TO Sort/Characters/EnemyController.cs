//using System;
using Pathfinding;
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

        private Seeker _seeker;
        private Seeker Seeker
        {
            get {
                if (!_seeker)
                    _seeker = GetComponent<Seeker>();
                return _seeker;
            }
        }
        private AIPath _Pather;
        private AIPath Pather
        {
            get {
                if (!_Pather)
                    _Pather = GetComponent<AIPath>();
                return _Pather;
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
        void Awake() {
            if (!Pather)
            {
                throw new Exception("No pather found on enemy");
            }
        }
		
		private void OnEnable()
		{
			startPoint = transform.position;
			if (Pather)
			{
                Pather.maxSpeed = moveSpeed;
                Pather.updateRotation = false;
			}
		}

		private void LateUpdate()
		{
			lastPosition = transform.position;
		}

		public void Stroll()
		{
			SetRunning(false);
			bool atDestination = (transform.position - Pather.destination).sqrMagnitude <= Pather.endReachedDistance * Pather.endReachedDistance * 1.1f;
			if ((!Pather.pathPending && !Pather.hasPath) || atDestination || transform.position == lastPosition)
			{
				currentDestination = CreateNewDestination();
                Pather.destination=(currentDestination);
			}
			

			RotateTowardsMovement();
			UpdateAnimationData();
		}
		public void Idle()
		{

            if (!Pather.isStopped)
                Pather.destination=(transform.position);
			
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
            Pather.destination=(targetPosition);

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

			Pather.maxSpeed = GetRunning() ? moveSpeed * runSpeedMultiplier : moveSpeed;

		}
		public override void ExternalMove(Vector3 offset)
		{
			//Agent.ResetPath();
            Pather.Move(offset);
		}

		#region Animations


		void UpdateAnimationData()
		{
			bool stopped = Pather.isStopped;
			float movePercentage = GetRunning() ? 1f : !stopped ? 0.5f : 0;
			
			Vector2 blend = new Vector2(0, movePercentage);

			AnimHandler.SetMovementPerformed(blend);

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
