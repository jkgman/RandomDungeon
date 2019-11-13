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
		#region Variables & References

		//_______ Start of Exposed Variables
		[SerializeField] private float minIdleTime = 0;
		[SerializeField] private float maxIdleTime = 10;
		[SerializeField] private float minStrollTime = 0;
		[SerializeField] private float maxStrollTime = 10;
		//_______ End of Exposed Variables

		//_______ Start of Hidden Variables
		private float idleDuration;
		private float currentIdleTime;
		private float strollDuration;
		private float currentStrollTime;
		//_______ End of Hidden Variables

		//_______ Start of Class References
		private EnemyCombatHandler _eCombat;
		private EnemyCombatHandler ECombat
		{
			get
			{
				if (!_eCombat)
					_eCombat = GetComponent<EnemyCombatHandler>();
				return _eCombat;
			}
		}
		private EnemyMovement _eMovement;
		private EnemyMovement EMovement
		{
			get
			{
				if (!_eMovement)
					_eMovement = GetComponent<EnemyMovement>();
				return _eMovement;
			}
		}
		//_______ End of Class References

		#endregion Variables & References

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

		#region Getters & Setters

		private EnemyState _currentState = EnemyState.idle;
		public EnemyState GetCurrentState()
		{
			return _currentState;
		}
		public void SetCurrentState(EnemyState newState)
		{
			_currentState = newState;
		}



		#endregion Getters & Setters

		#region Initialization & Updates

		/// <summary>
		/// Checks what current state is and calls state-specific Update function.
		/// </summary>
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

		/// <summary>
		/// Called from update when state is Idle
		/// </summary>
		private void IdleUpdate()
		{
			EMovement.Idle();
			currentIdleTime += Time.deltaTime;
		}

		/// <summary>
		/// Called from update when state is Stroll
		/// </summary>
		private void StrollingUpdate()
		{
			EMovement.Stroll();
			currentStrollTime += Time.deltaTime;
		}

		/// <summary>
		/// Called from update when state is Follow
		/// </summary>
		private void FollowingUpdate()
		{
			EMovement.Following(ECombat.GetTargetPosition(), ECombat.CanSeeTarget());

		}

		/// <summary>
		/// Called from update when state is Attack
		/// </summary>
		private void AttackingUpdate()
		{
			EMovement.RotateTowardsPosition(ECombat.GetTargetPosition());
			ECombat.AttackUpdate(AttackType.lightAttack);
		}

		/// <summary>
		/// Finds appropriate AI state from several bool checks.
		/// </summary>
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

		/// <summary>
		/// Check if current idle time is more than idle duration.
		/// </summary>
		private bool IdleDone()
		{
			return currentIdleTime > idleDuration;
		}
		/// <summary>
		/// Check if current stroll time is more than stroll duration.
		/// </summary>
		private bool StrollDone()
		{
			return currentStrollTime > strollDuration;
		}

		/// <summary>
		/// Reset all timers. called when state changes.
		/// </summary>
		private void ResetTimers()
		{
			currentIdleTime = 0;
			currentStrollTime = 0;
			idleDuration = Random.Range(minIdleTime, maxIdleTime);
			strollDuration = Random.Range(minStrollTime, maxStrollTime);
		}

		#endregion Initialization & Updates

		#region Routines

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

		#endregion Routines

		#region IAllowedActions

		public bool AllowMove()
		{
			bool output = true;

			output = EMovement.AllowMove() ? output : false;
			output = ECombat.AllowMove() ? output : false;

			return output;
		}
		public bool AllowRun()
		{
			bool output = true;

			output = EMovement.AllowRun() ? output : false;
			output = ECombat.AllowRun() ? output : false;

			return output;
		}
		public bool AllowAttack()
		{
			bool output = true;

			output = EMovement.AllowAttack() ? output : false;
			output = ECombat.AllowAttack() ? output : false;

			return output;
		}
		public bool AllowRotate()
		{
			bool output = true;

			output = EMovement.AllowRotate() ? output : false;
			output = ECombat.AllowRotate() ? output : false;

			return output;
		}

		#endregion IAllowedActions

		#region ITargetable
		public Transform GetTransform()
		{
			return transform;
		}
		public bool IsTargetable()
		{
			return Stats.health.IsAlive() && isActive;
		}

		public Vector3 GetPosition()
		{
			return transform.position;
		}
		#endregion ITargetable

	}
}
