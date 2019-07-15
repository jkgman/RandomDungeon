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

	public class Enemy : Character, ITargetable, IAllowedEnemyActions
	{

		[SerializeField] private float minIdleTime;
		[SerializeField] private float maxIdleTime;
		[SerializeField] private float minStrollTime;
		[SerializeField] private float maxStrollTime;

		private float idleDuration;
		private float currentIdleTime;
		private float strollDuration;
		private float currentStrollTime;

        //Todo: this can be handled different down the line, for now one enemy will have key as a variable and drop it on death
        #region Drop
        [SerializeField] private GameObject drop;
        public void SetDrop(GameObject obj) {
            Debug.Log("drop");
            if (drop != null)
            {
                drop = obj;
            }
        }
        public void Drop() {
			if (drop)
	            Instantiate(drop, transform.position, transform.rotation);
        }
        #endregion

        private EnemyState _currentState = EnemyState.idle;
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
		

		private EnemyCombatHandler _eCombat;
		private EnemyCombatHandler ECombat
		{
			get{
				if (!_eCombat)
					_eCombat = GetComponent<EnemyCombatHandler>();
				return _eCombat;
			}
		}
		private EnemyController _eController;
		private EnemyController EController
		{
			get
			{
				if (!_eController)
					_eController = GetComponent<EnemyController>();
				return _eController;
			}
		}




		protected override void Update()
		{
			base.Update();

			if (!isActive)
				return;

			UpdateEnemyState();

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


		public void UpdateEnemyState()
		{
			if (ECombat.AllowAttack())
			{
				SetCurrentState(EnemyState.attacking);
			}
			else if (ECombat.CanFindTarget())
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
			EController.Idle();
			currentIdleTime += Time.deltaTime;
		}
		private void StrollingUpdate()
		{
			EController.Stroll();
			currentStrollTime += Time.deltaTime;
		}
		private void FollowingUpdate()
		{
			EController.Following(ECombat.GetTargetPosition(), ECombat.CanSeeTarget());

		}
		private void AttackingUpdate()
		{
			EController.RotateTowardsPosition(ECombat.GetTargetPosition());
			ECombat.AttackUpdate();
		}



		protected override IEnumerator DieRoutine()
		{
			//Death animation or whatever.
			//For now it is just a particle effect and disappear.
			Debug.Log("Enemy dies nad disables");
			dieRoutineStarted = true;

            //Todo: Drop call
            Drop();

			Effects.PlayDeathParticles();
			Effects.SetInvisible();
			DisableColliders();
			isActive = false;

			yield return new WaitForSeconds(2f);
			Destroy(this.gameObject);
		}

		public bool AllowMove()
		{
			bool output = true;

			output = EController.AllowMove() ? output : false;
			output = ECombat.AllowMove() ? output : false;

			return output;
		}
		public bool AllowRun()
		{
			bool output = true;

			output = EController.AllowRun() ? output : false;
			output = ECombat.AllowRun() ? output : false;

			return output;
		}
		public bool AllowAttack()
		{
			bool output = true;

			output = EController.AllowAttack() ? output : false;
			output = ECombat.AllowAttack() ? output : false;

			return output;
		}
		public bool AllowRotate()
		{
			bool output = true;

			output = EController.AllowRotate() ? output : false;
			output = ECombat.AllowRotate() ? output : false;

			return output;
		}
	}
}
