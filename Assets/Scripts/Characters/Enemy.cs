using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Characters.Enemies
{
	public enum EnemyState
	{
		idle,
		strolling,
		following,
		attacking
	}

	public class Enemy : Character, ITargetable
	{

		[SerializeField] private float minIdleTime;
		[SerializeField] private float maxIdleTime;
		[SerializeField] private float minStrollTime;
		[SerializeField] private float maxStrollTime;
		[SerializeField] private float strollDistance;
		[SerializeField] private bool calculateStrollDistanceFromSpawn;
		private Vector3 startPoint;

		private float idleDuration;
		private float currentIdleTime;
		private float strollDuration;
		private float currentStrollTime;
		bool isActive = true;




		private void OnEnable()
		{
			startPoint = transform.position;
		}

		private EnemyState _currentState;
		public EnemyState GetCurrentState()
		{
			return _currentState;
		}
		public void SetCurrentState(EnemyState newState)
		{
			_currentState = newState;
		}



		public bool IsTargetable()
		{
			return Stats.health.IsAlive() && isActive;
		}

		public Vector3 GetPosition()
		{
			return transform.position;
		}

		public Transform GetTransform()
		{
			return transform;
		}

		private EnemyCombatHandler _combat;
		private EnemyCombatHandler Combat
		{
			get{
				if (!_combat)
					_combat = GetComponent<EnemyCombatHandler>();
				return _combat;
			}
		}
		private EnemyController _controller;
		private EnemyController Controller
		{
			get
			{
				if (!_controller)
					_controller = GetComponent<EnemyController>();
				return _controller;
			}
		}




		void Update()
		{
			switch (GetCurrentState())
			{
				case EnemyState.idle:
					IdleUpdate();
					break;
				case EnemyState.strolling:
					StrollingUpdate();
					break;
				case EnemyState.following:
					FollowingUpdate();
					break;
				case EnemyState.attacking:
					AttackingUpdate();
					break;
				default:
					break;
			}
		}


		public void SetNewEnemyState()
		{
			if (Combat.CanAttack())
			{
				SetCurrentState(EnemyState.attacking);
			}
			else if (Combat.CanFindTarget())
			{
				SetCurrentState(EnemyState.following);
			}
			else if (GetCurrentState() == EnemyState.idle)
			{
				if (IdleDone())
				{
					SetCurrentState(EnemyState.strolling);
					ResetTimers();
				}
			}
			else if (GetCurrentState() == EnemyState.strolling)
			{
				if (StrollDone())
				{
					SetCurrentState(EnemyState.idle);
					ResetTimers();
				}
			}
			else
			{
				SetCurrentState(EnemyState.idle);
				ResetTimers();
			}
		}

		private bool IdleDone()
		{
			return currentIdleTime > idleDuration;
		}
		private bool StrollDone()
		{
			return currentStrollTime > strollDuration;
		}

		private void ResetTimers()
		{
			currentIdleTime = 0;
			currentStrollTime = 0;
			idleDuration = Random.Range(minIdleTime, maxIdleTime);
			strollDuration = Random.Range(minStrollTime, maxStrollTime);
		}

		private void IdleUpdate()
		{
			currentIdleTime += Time.deltaTime;
		}
		private void StrollingUpdate()
		{
			currentStrollTime += Time.deltaTime;
		}
		private void FollowingUpdate()
		{
			if (Combat.CanAttack() || !Combat.CanFindTarget())
				SetNewEnemyState();

			Controller.Following(Combat.GetTargetPosition(), Combat.CanSeeTarget());

		}
		private void AttackingUpdate()
		{
			if (Combat.CanAttack())
				Combat.AttackUpdate();
			else
				SetNewEnemyState();
		}



		protected override IEnumerator DieRoutine()
		{
			//Death animation or whatever.
			//For now it is just a particle effect and disappear.

			Effects.PlayDeathParticles();
			Effects.SetInvisible();

			isActive = false;
			yield return new WaitForSeconds(2f);
			Destroy(this.gameObject);
		}
	}
}
