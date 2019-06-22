using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine;

namespace Dungeon.Player
{


	/// <summary>
	/// Handles Inputs and actions related to combat such as attacks, dodges and blocks
	/// </summary>
	public class PlayerCombatHandler : MonoBehaviour, IAllowedActions
	{
		[Header("Target crap")]

		[SerializeField] private GameObject targetIndicatorPrefab;
		[SerializeField] private float maxDistToTarget = 10f;
		[SerializeField] private LayerMask targetLayerMask;
		[SerializeField] private LayerMask blockTargetLayerMask;

		private GameObject targetIndicator;
		public UnityEvent targetChangedEvent;


		[Header("Attack shit")]
		[SerializeField] private Transform rightHand;
		[SerializeField] private Transform leftHand;
		private Items.Weapon currentWeapon;
		private IEnumerator currentAttackCo;

		[Header("Dodge shit")]
		[SerializeField] private float dodgeDistance = 3f;
		[SerializeField] private float dodgeDuration = 0.35f;
		[SerializeField, Range(0f,1f)] private float dodgeInvincibilityPercentage = 0.75f;

		[Header("Block shit")]

		[Header("Other shit")]



		float inputDodgeStartTime = 0;
		float inputTargetSwitchTime = 0;
		readonly float inputTargetSwitchInterval = 0.1f;


		public bool IsBlocking
		{
			get;
			private set;
		}
		public bool IsDodging 
		{
			get;
			private set;
		}
		public bool IsStunned
		{
			get;
			private set;
		}
		public bool IsInvincible
		{
			get;
			private set;
		}

		private Player _pManager;
		private Player PManager
		{
			get
			{
				if (!_pManager)
					_pManager = GetComponent<Player>();

				return _pManager;
			}
		}
		private Inputs inputs;



		#region Initialization

		void Awake()
		{
			ControlsSubscribe();

			currentWeapon = GetComponentInChildren<Items.Weapon>();
			if (currentWeapon)
				currentWeapon.CurrentEquipper = transform;
		}

		void Start() {

			if (targetChangedEvent == null)
				targetChangedEvent = new UnityEvent();

			if (PManager.GetCam && PManager.GetCam.CameraTransformsUpdated != null)
				PManager.GetCam.CameraTransformsUpdated.AddListener(CameraReadyEvent);

		}
		void OnDisable() {
			targetChangedEvent = null;

			if (PManager.GetCam && PManager.GetCam.CameraTransformsUpdated != null)
				PManager.GetCam.CameraTransformsUpdated.RemoveListener(CameraReadyEvent);

			ControlsUnsubscribe();
		}

		#endregion

		#region Getters & Setters

		private Transform _target;
		public Transform Target {
			get { return _target; }
			set {
				if (value != _target)
					targetChangedEvent.Invoke();

				_target = value;
			}
		}

		public float GetMaxDistToTarget 
		{
			get { return maxDistToTarget; }
		}

		public Vector3 GetFlatDirectionToTarget() 
		{
			if (Target)
			{
				var dirToTarget = Target.transform.position - transform.position;
				dirToTarget.y = 0;
				return dirToTarget.normalized;
			} 
			else
			{
				return -PManager.PController.GetFlatMoveDirection();
			}
		}

		#endregion

		#region Events

		//Is called after camera has updated its transform.
		void CameraReadyEvent() {
			// Target indicator needs the latest camera position, otherwise looks bad at low fps.
			SetTargetIndicator();
		}

		#endregion

		void Update() 
		{
			UpdateTarget();
			UpdateDebug();
		}

		void UpdateDebug()
		{
			if (currentWeapon && PlayerDebugCanvas.Instance)
			{
				if (currentWeapon.IsAttacking)
				{
					if (currentWeapon.CurrentAttackState == AttackState.charge)
						PlayerDebugCanvas.Instance.SetDebugText("Charge");
					if (currentWeapon.CurrentAttackState == AttackState.attack)
						PlayerDebugCanvas.Instance.SetDebugText("Attack");
					if (currentWeapon.CurrentAttackState == AttackState.recovery)
						PlayerDebugCanvas.Instance.SetDebugText("Recovery");
				}
				else
				{

					PlayerDebugCanvas.Instance.SetDebugText("Idle");
				}
			}
		}

		#region Targeting

		void UpdateTarget() {
			if (Target && (Target.position - transform.position).magnitude > maxDistToTarget)
			{
				Target = null;
			}
		}

		void SetTargetIndicator() {
			if (!Target || !PManager.GetCam) // Indicator relies on camera
			{
				if (targetIndicator)
					Destroy(targetIndicator);
			} else
			{
				if (!targetIndicator)
				{
					if (targetIndicatorPrefab)
						targetIndicator = Instantiate(targetIndicatorPrefab, Target.transform.position, Quaternion.LookRotation(PManager.GetCam.transform.position - Target.transform.position));
				} else
				{
					targetIndicator.transform.position = Target.transform.position;
					targetIndicator.transform.rotation = Quaternion.LookRotation(PManager.GetCam.GetCurrentDirection());
				}
			}

		}

		public bool SetTarget(CameraTargetingData data) {
			Transform oldTarget = Target;

			if (Target != null)
			{
				Target = null;
				return true;
			}

			Transform[] nearbyTargets = FindTargets();
			if (nearbyTargets != null && nearbyTargets.Length > 0)
				Target = FindBestTarget(data, nearbyTargets);

			return Target != oldTarget; //Return true if target changed in any way
		}

		Transform[] FindTargets() {
			//Sphere check on all nearby enemies.
			//Adds objects with Enemy script in temp list
			//Converts list into output array.

			List<Transform> temp = new List<Transform>();
			Collider[] cols = Physics.OverlapSphere(transform.position, maxDistToTarget, targetLayerMask.value);
			if (cols != null && cols.Length > 0)
			{
				for (int i = 0; i < cols.Length; i++)
				{
					var enemy = cols[i].GetComponent<Enemy.Enemy>();
					if (enemy && enemy.CanBeTargeted() && !temp.Contains(enemy.transform))
					{
						temp.Add(enemy.transform);
					}
				}
			}
			Transform[] output = new Transform[temp.Count];
			for (int i = 0; i < temp.Count; i++)
			{
				output[i] = temp[i];
			}

			return output;
		}

		Transform FindBestTarget(CameraTargetingData camData, Transform[] allTheBoisToBeTargeted) {
			Transform output = null;
			float currentBestAngle = -1;

			for (int i = 0; i < allTheBoisToBeTargeted.Length; i++)
			{
				float angle = Vector3.Angle(camData.forward, (allTheBoisToBeTargeted[i].position - camData.position));
				bool inView = angle < camData.fov * 0.65f;
				bool farEnoughFromCamera = Vector3.Distance(camData.position, allTheBoisToBeTargeted[i].position) > 3f;
				if (inView && farEnoughFromCamera)
				{
					//Check if target is visible in camera.
					RaycastHit hit;
					Physics.Raycast(camData.position, (allTheBoisToBeTargeted[i].position - camData.position).normalized, out hit, maxDistToTarget, blockTargetLayerMask.value);
					if (!hit.collider || hit.collider.transform == allTheBoisToBeTargeted[i])
					{
						//Check that the angle is better than currentBest.
						if (currentBestAngle < 0 || angle < currentBestAngle)
						{
							//Its the fucking best so far so assign that mf to output.
							output = allTheBoisToBeTargeted[i];
							currentBestAngle = angle;
						}
					}
				}
			}
			return output;
		}

		void SwitchTarget(int direction) {
			Transform[] allTheBoisToBeTargeted = FindTargets();
			CameraTargetingData camData = PManager.GetCam.GetTargetingData();

			Transform newTarget = null;
			float currentBestAngle = -1;

			for (int i = 0; i < allTheBoisToBeTargeted.Length; i++)
			{
				Vector3 dirToBoi = (allTheBoisToBeTargeted[i].position - transform.position);
				dirToBoi.y = 0;
				dirToBoi.Normalize();
				float angle = Vector3.SignedAngle(GetFlatDirectionToTarget(), dirToBoi, Vector3.up);
				bool inView = Mathf.Abs(angle) < 100f;
				bool farEnoughFromCamera = Vector3.Distance(camData.position, allTheBoisToBeTargeted[i].position) > 3f;
				bool correctSide = (direction > 0 && angle > 0) || (direction < 0 && angle < 0);
				if (inView && farEnoughFromCamera && correctSide)
				{
					//Check if target is visible in camera.
					RaycastHit hit;
					Physics.Raycast(camData.position, (allTheBoisToBeTargeted[i].position - camData.position).normalized, out hit, maxDistToTarget, blockTargetLayerMask.value);
					Debug.Log(hit.collider);
					if (!hit.collider || hit.collider.transform == allTheBoisToBeTargeted[i])
					{
						//Check that the angle is better than currentBest.
						if (currentBestAngle < 0 || Mathf.Abs(angle) < currentBestAngle)
						{
							//Its the fucking best so far so assign that mf to output.
							newTarget = allTheBoisToBeTargeted[i];
							currentBestAngle = Mathf.Abs(angle);
						}
					}
				}
			}

			if (newTarget)
				Target = newTarget;

		}

		#endregion

		#region Combat

		void Doge()
		{
			if (PManager.AllowDodge())
			{
				Vector3 dodgeDir = PManager.PController.GetFlatMoveDirection(false);
				dodgeDir.y = 0;
				StartCoroutine(DodgeRoutine(dodgeDir.normalized));
				
			}
		}

		IEnumerator DodgeRoutine(Vector3 direction)
		{
			IsDodging = true;

			float t = 0;
			float invincibilityMargin = (1f - dodgeInvincibilityPercentage) / 2;
			
			float angle = Vector3.SignedAngle(transform.forward, Vector3.forward, Vector3.up);
			Vector3 relativeMoveDirection = Quaternion.Euler(0, angle, 0) * direction;
			Vector2 blend = new Vector2(relativeMoveDirection.x, relativeMoveDirection.z).normalized;
			PManager.PAnimation.SetDodgeStarted(blend, dodgeDuration);

			while (IsDodging && t < dodgeDuration)
			{
				IsInvincible = (t / dodgeDuration > invincibilityMargin) && (t / dodgeDuration < 1 - invincibilityMargin);
				float distThisFrame = (dodgeDistance / dodgeDuration) * Time.smoothDeltaTime;
				Vector3 offset = direction * distThisFrame;
				PManager.PController.ExternalMove(offset);

				yield return null;
				t += Time.smoothDeltaTime;
			}

			IsDodging = false;
			PManager.PAnimation.SetDodgeCancelled();

			yield return null;
		}

		void Attack()
		{
			if (PManager.AllowAttack())
			{
				if (currentAttackCo != null)
					StopCoroutine(currentAttackCo);


				currentAttackCo = LightAttackRoutine();
				StartCoroutine(currentAttackCo);
			}
		}

		void SetAttackDurations()
		{
			float charge = currentWeapon.GetChargeDuration();
			float attack = currentWeapon.GetAttackDuration();
			float recovery = currentWeapon.GetRecoveryDuration();

			PManager.PAnimation.SetAttackDurations(charge, attack, recovery);
		}

		IEnumerator LightAttackRoutine()
		{
			currentWeapon.StartAttacking(AttackType.lightAttack);
			SetAttackDurations();

			yield return null; //Wait for one frame because animator sucks ass (ignores booleans if setting durations in same frame)

			yield return StartCoroutine(LightAttackCharge());
			yield return StartCoroutine(LightAttackAttack());
			yield return StartCoroutine(LightAttackRecovery());

			currentWeapon.EndAttacking();

		}

		IEnumerator LightAttackCharge()
		{
			currentWeapon.CurrentAttackState = AttackState.charge;
			PManager.PAnimation.SetChargeStarted();

			float t = 0;
			float t01 = 0;
			float moveOffset = 0;
			float offsetTotal = 0;
			Vector3 moveDirection = PManager.PController.GetFlatMoveDirection(allowZero: false);

			while (	currentWeapon.IsAttacking && 
					currentWeapon.CurrentAttackType == AttackType.lightAttack && 
					t < currentWeapon.GetCurrentActionDuration())
			{
				t01 = Mathf.Clamp01(t / currentWeapon.GetCurrentActionDuration());
				moveOffset = currentWeapon.CurrentMoveDistance(t01) - offsetTotal;
				moveDirection = UpdateAttackMoveDirection(moveDirection);
				PManager.PController.ExternalMove(moveDirection * moveOffset);

				if (currentWeapon.CanRotate(Target != null))
					PManager.PController.ExternalRotateToInputDirection();

				offsetTotal = currentWeapon.CurrentMoveDistance(t01);
				t += Time.smoothDeltaTime;
				yield return null;
			}
			
			PManager.PAnimation.SetChargeCancelled();

		}
		IEnumerator LightAttackAttack()
		{
			currentWeapon.CurrentAttackState = AttackState.attack;
			PManager.PAnimation.SetAttackStarted();
			
			float t = 0;
			float t01 = 0;
			float moveOffset = 0;
			float offsetTotal = 0;

			Vector3 moveDirection = PManager.PController.GetFlatMoveDirection(allowZero: false);

			while (	currentWeapon.IsAttacking &&
					currentWeapon.CurrentAttackType == AttackType.lightAttack &&
					t < currentWeapon.GetCurrentActionDuration())
			{
				t01 = Mathf.Clamp01(t / currentWeapon.GetCurrentActionDuration());
				moveDirection = UpdateAttackMoveDirection(moveDirection);

				t01 = Mathf.Clamp01(t / currentWeapon.GetCurrentActionDuration());
				moveOffset = currentWeapon.CurrentMoveDistance(t01) - offsetTotal;
				PManager.PController.ExternalMove(moveDirection * moveOffset);

				if (currentWeapon.CanRotate(Target != null))
					PManager.PController.ExternalRotateToInputDirection();

				offsetTotal = currentWeapon.CurrentMoveDistance(t01);
				t += Time.smoothDeltaTime;
				yield return null;
			}
			
			PManager.PAnimation.SetAttackCancelled();

		}
		IEnumerator LightAttackRecovery()
		{
			currentWeapon.CurrentAttackState = AttackState.recovery;
			PManager.PAnimation.SetRecoveryStarted();

			float t = 0;
			float t01 = 0;
			float moveOffset = 0;
			float offsetTotal = 0;

			Vector3 moveDirection = PManager.PController.GetFlatMoveDirection(allowZero: false);


			while (	currentWeapon.IsAttacking &&
					currentWeapon.CurrentAttackType == AttackType.lightAttack &&
					t < currentWeapon.GetCurrentActionDuration())
			{
				t01 = Mathf.Clamp01(t / currentWeapon.GetCurrentActionDuration());

				moveDirection = UpdateAttackMoveDirection(moveDirection);

				t01 = Mathf.Clamp01(t / currentWeapon.GetCurrentActionDuration());
				moveOffset = currentWeapon.CurrentMoveDistance(t01) - offsetTotal;
				PManager.PController.ExternalMove(moveDirection * moveOffset);

				if (currentWeapon.CanRotate(Target != null))
					PManager.PController.ExternalRotateToInputDirection();

				offsetTotal = currentWeapon.CurrentMoveDistance(t01);
				t += Time.smoothDeltaTime;
				yield return null;
			}
			
			PManager.PAnimation.SetRecoveryCancelled();
		}

		Vector3 UpdateAttackMoveDirection(Vector3 current)
		{
			Vector3 output = transform.forward;

			//if (Target)
			//{
			//	if (currentWeapon.CanRotate(hasTarget: true))
			//		output = GetFlatDirectionToTarget();
			//}
			//else
			//{
			//	if (currentWeapon.CanRotate(hasTarget: false))
			//		output = PManager.PController.GetTransformedInputDirection(allowZero: false);
			//}

			return output;
		}

		#endregion


		#region HandleInputs




		void ControlsSubscribe() 
		{
			if (inputs == null)
				inputs = new Inputs();


			inputs.Player.TargetLock.performed += InputTargetLock;
			inputs.Player.TargetLock.Enable();
			inputs.Player.SwitchTarget.started += InputTargetSwitch;
			inputs.Player.SwitchTarget.Enable();
			inputs.Player.RunAndDodge.started += InputDodgeStarted;
			inputs.Player.RunAndDodge.canceled += InputDodgeCancelled;
			inputs.Player.RunAndDodge.Enable();
			inputs.Player.Attack.started += InputAttackStarted;
			inputs.Player.Attack.canceled += InputAttackCancelled;
			inputs.Player.Attack.Enable();
		}

		void ControlsUnsubscribe() 
		{
			inputs.Player.TargetLock.performed -= InputTargetLock;
			inputs.Player.TargetLock.Disable();
			inputs.Player.SwitchTarget.started -= InputTargetSwitch;
			inputs.Player.SwitchTarget.Disable();
			inputs.Player.RunAndDodge.started -= InputDodgeStarted;
			inputs.Player.RunAndDodge.canceled -= InputDodgeCancelled;
			inputs.Player.RunAndDodge.Enable();
		}

		void InputTargetLock(InputAction.CallbackContext context) 
		{

			bool success = SetTarget(PManager.GetCam.GetTargetingData());
			Debug.Log("InputTargetLock: " + success);
			if (!success)
				PManager.GetCam.ResetCamera();

		}

		void InputTargetSwitch(InputAction.CallbackContext context) 
		{
			if (!Target)
				return;

			if (Time.time - inputTargetSwitchTime < inputTargetSwitchInterval)
				return;
			else
				inputTargetSwitchTime = Time.time;


			//NOTE
			//Input system currently bugged with scroll wheel, will be fixed in next update probably.
			//Causes mouse movement to register as scrolling, at least sometimes


			int targetSwitchDirection = 0;

			if (context.control.layout == "Stick")
			{
				targetSwitchDirection = context.ReadValue<Vector2>().x > 0 ? 1 : -1;
			} else
			{
				//Downwards is +1 (right)
				targetSwitchDirection = context.ReadValue<Vector2>().y > 0 ? -1 : 1;
			}

			SwitchTarget(targetSwitchDirection);

		}


		void InputDodgeStarted(InputAction.CallbackContext context) 
		{
			inputDodgeStartTime = Time.time;
		}
		void InputDodgeCancelled(InputAction.CallbackContext context) 
		{
			if (Time.time - inputDodgeStartTime < PManager.inputMaxPressTime)
			{
				Doge();
			}
		}

		void InputAttackStarted(InputAction.CallbackContext context)
		{
			Attack();
		}
		void InputAttackCancelled(InputAction.CallbackContext context)
		{

		}



		#endregion

		#region IAllowedActions
		
		public bool AllowMove() 
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;
			if (currentWeapon && currentWeapon.IsAttacking)
				output = currentWeapon.CanMove(Target != null) ? output : false;

			return output;
		}

		public bool AllowRun() 
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;
			output = IsBlocking ? false : output;
			if (currentWeapon && currentWeapon.IsAttacking)
				output = currentWeapon.CanMove(Target != null) ? false : output;

			return output;
		}

		public bool AllowAttack() 
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;
			output = currentWeapon.CanAttack(Target != null) ? output : false;

			return output;
		}

		public bool AllowDodge() 
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;
			if (currentWeapon)
				output = currentWeapon.IsAttacking ? false : output;

			return output;
		}
		public bool AllowRotate()
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;
			if (currentWeapon && currentWeapon.IsAttacking)
				output = false;
				
			return output;
		}
		
		#endregion

	}
}
