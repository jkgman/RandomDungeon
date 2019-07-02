using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{

	float attackDelayTimer;
	float attackDelay;
	public float turnSpeed;
	public float viewRangeModifier = 5;
	bool canAttack;
	bool timerReset;
	public LayerMask obstacles;
	NavMeshAgent navMeshAgent;



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

	private void Awake()
	{
		navMeshAgent = GetComponent<NavMeshAgent>();
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
