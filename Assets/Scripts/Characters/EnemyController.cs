using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
	private enum EnemyState
	{
		idle,
		strolling,
		following,
		attacking
	}

	[SerializeField] private LayerMask obstacles;
	[SerializeField] private float turnSpeed;
	[SerializeField] private float viewRangeModifier = 5;
	[SerializeField] private float strollDistance;
	[SerializeField] private bool calculateStrollDistanceFromSpawn;
	[SerializeField] private float attackDelay;
	[SerializeField] private float minIdleTime;
	[SerializeField] private float maxIdleTime;

	private Vector3 startPoint;


	private float idleTime;
	private float attackDelayTimer;
	private bool canAttack;
	private bool timerReset;
	private NavMeshAgent navMeshAgent;



	Transform _playerTrans;
	Transform PlayerTrans
	{
		get
		{
			if (!_playerTrans)
			{
				GameObject temp = GameObject.FindGameObjectWithTag("Player");
				if (temp)
					_playerTrans = temp.transform;
			}

			return _playerTrans;
		}
	}




	private void Awake()
	{
		navMeshAgent = GetComponent<NavMeshAgent>();
	}
	private void OnEnable()
	{
		startPoint = transform.position;
	}

	private void Update()
	{
		Move();
		if (PlayerTrans && CanSeeTarget())
		{
			RotateTowardsTraget();

			if (CanAttack())
			{
				Attack();
			}
		}
	}


	public Vector3 TargetDirection()
	{
		return PlayerTrans.position - transform.position;
	}


	public bool CanAttack()
	{
		if (attackDelayTimer < Time.time - attackDelay)
			canAttack = true;

		float sqrLen = TargetDirection().sqrMagnitude;
		if (sqrLen < navMeshAgent.stoppingDistance * navMeshAgent.stoppingDistance)
		{
			//Reset timer once
			if (!timerReset)
			{
				canAttack = false;
				attackDelayTimer = Time.time;
				timerReset = true;
			}
		}
		else
		{
			canAttack = false;
			timerReset = false;
		}

		return canAttack;
	}

	public bool CanSeeTarget()
	{

		if (Physics.Raycast(transform.position, TargetDirection(), navMeshAgent.stoppingDistance + viewRangeModifier, obstacles))
		{
			//if the player is behind an obstacle
			return false;
		}
		else
			return true;
	}



	public void Move()
	{
		if (PlayerTrans && TargetDirection().sqrMagnitude < viewRangeModifier*viewRangeModifier)
			navMeshAgent.SetDestination(PlayerTrans.position);
		else
			navMeshAgent.SetDestination(transform.position);
	}

	public void RotateTowardsTraget()
	{
		if (PlayerTrans)
		{
			Quaternion targetRotation = Quaternion.LookRotation(TargetDirection());
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed);
		}
	}
	public void Attack()
	{
		//Weapon attack etc.

	}
}
