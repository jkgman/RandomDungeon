using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine;


namespace Dungeon.Characters
{
	using Dungeon.Items;
	/// <summary>
	/// Handles Inputs and actions related to combat such as attacks, dodges and blocks
	/// </summary>
	public class PlayerCombatHandler : CharacterCombatHandler, IAllowedPlayerActions
	{
		[Header("Target crap")]

		[SerializeField] private GameObject targetIndicatorPrefab;
		[SerializeField] private float maxDistToTarget = 10f;
		[SerializeField] private LayerMask targetLayerMask;
		[SerializeField] private LayerMask blockTargetLayerMask;

		private GameObject targetIndicator;
		public UnityEvent targetChangedEvent;


		[Header("Dodge shit")]
		[SerializeField]
		private AnimationCurve dodgeDistance;
		[SerializeField] private float dodgeDuration = 0.35f;
		[SerializeField, Range(0f,1f)] private float dodgeInvincibilityPercentage = 0.75f;

		//[Header("Block shit")]

		//[Header("Other shit")]


		protected PlayerWeapon CurrentWeapon
		{
			get{ return (PlayerWeapon)GetCurrentWeapon(); }
		}

		float inputDodgeStartTime = 0;
		float inputTargetSwitchTime = 0;
		readonly float inputTargetSwitchInterval = 0.1f;

		private Inputs PlayerInputs
		{
			get { return Player.Inputs; }
		}




		#region Initialization

		protected override void OnEnable()
		{
			base.OnEnable();
			ControlsSubscribe();
			
		}

		protected override void Start()
		{
			base.Start();

			if (targetChangedEvent == null)
				targetChangedEvent = new UnityEvent();

			if (Player.GetCam && Player.GetCam.CameraTransformsUpdated != null)
				Player.GetCam.CameraTransformsUpdated.AddListener(CameraReadyEvent);

		}
		protected override void OnDisable() 
		{
			base.OnDisable();

			targetChangedEvent = null;

			if (Player.GetCam && Player.GetCam.CameraTransformsUpdated != null)
				Player.GetCam.CameraTransformsUpdated.RemoveListener(CameraReadyEvent);

			ControlsUnsubscribe();
		}

		#endregion

		#region Getters & Setters


		private Player _player;
		private Player Player
		{
			get
			{
				if (!_player)
					_player = GetComponent<Player>();

				return _player;
			}
		}

		private ITargetable _target;
		public ITargetable Target {
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
			if (Target != null)
			{
				var dirToTarget = Target.GetPosition() - transform.position;
				dirToTarget.y = 0;
				return dirToTarget.normalized;
			} 
			else
			{
				return -Player.PController.GetFlatMoveDirection();
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

		protected override void Update() 
		{
			base.Update();
			UpdateTarget();
			UpdateDebug();
		}

		void UpdateDebug()
		{
			if (GetCurrentWeapon() && PlayerDebugCanvas.Instance)
			{
				if (GetCurrentWeapon().IsAttacking)
				{
					if (GetCurrentWeapon().CurrentAttackState == AttackState.charge)
						PlayerDebugCanvas.Instance.SetDebugText("Charge");
					if (GetCurrentWeapon().CurrentAttackState == AttackState.attack)
						PlayerDebugCanvas.Instance.SetDebugText("Attack");
					if (GetCurrentWeapon().CurrentAttackState == AttackState.recovery)
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
			if (Target != null && ((Target.GetPosition() - transform.position).magnitude > maxDistToTarget || !Target.IsTargetable()))
			{
				Target = null;
			}
		}

		void SetTargetIndicator() {
			if (Target == null || !Player.GetCam) // Indicator relies on camera
			{
				if (targetIndicator)
					Destroy(targetIndicator);
			} else
			{
				if (!targetIndicator)
				{
					if (targetIndicatorPrefab)
						targetIndicator = Instantiate(targetIndicatorPrefab, Target.GetPosition(), Quaternion.LookRotation(Player.GetCam.transform.position - Target.GetPosition()));
				} else
				{
					targetIndicator.transform.position = Target.GetPosition();
					targetIndicator.transform.rotation = Quaternion.LookRotation(Player.GetCam.GetCurrentDirection());
				}
			}

		}

		public bool SetTarget(CameraTargetingData data) {
			ITargetable oldTarget = Target;

			if (Target != null)
			{
				Target = null;
				return true;
			}

			ITargetable[] nearbyTargets = FindTargets();
			if (nearbyTargets != null && nearbyTargets.Length > 0)
				Target = FindBestTarget(data, nearbyTargets);

			return Target != oldTarget; //Return true if target changed in any way
		}

		ITargetable[] FindTargets() {
			//Sphere check on all nearby enemies.
			//Adds objects with ITargetable in temp list
			//Converts list into output array.

			List<ITargetable> temp = new List<ITargetable>();
			Collider[] cols = Physics.OverlapSphere(transform.position, maxDistToTarget, targetLayerMask.value);
			if (cols != null && cols.Length > 0)
			{
				for (int i = 0; i < cols.Length; i++)
				{
					var targetable = cols[i].GetComponent<ITargetable>();
					if (targetable != null && targetable.IsTargetable() && !temp.Contains(targetable))
					{
						temp.Add(targetable);
					}
				}
			}
			ITargetable[] output = new ITargetable[temp.Count];
			for (int i = 0; i < temp.Count; i++)
			{
				output[i] = temp[i];
			}

			return output;
		}

		ITargetable FindBestTarget(CameraTargetingData camData, ITargetable[] allTheBoisToBeTargeted) {
			ITargetable output = null;
			float currentBestAngle = -1;

			for (int i = 0; i < allTheBoisToBeTargeted.Length; i++)
			{
				float angle = Vector3.Angle(camData.forward, (allTheBoisToBeTargeted[i].GetPosition() - camData.position));
				bool inView = angle < camData.fov * 0.65f;
				bool farEnoughFromCamera = Vector3.Distance(camData.position, allTheBoisToBeTargeted[i].GetPosition()) > 3f;
				if (inView && farEnoughFromCamera)
				{
					//Check if target is visible in camera.
					RaycastHit hit;
					Physics.Raycast(camData.position, (allTheBoisToBeTargeted[i].GetPosition() - camData.position).normalized, out hit, maxDistToTarget, blockTargetLayerMask.value);
					if (!hit.collider || hit.collider.transform == allTheBoisToBeTargeted[i].GetTransform())
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
			ITargetable[] allTheBoisToBeTargeted = FindTargets();
			CameraTargetingData camData = Player.GetCam.GetTargetingData();

			ITargetable newTarget = null;
			float currentBestAngle = -1;

			for (int i = 0; i < allTheBoisToBeTargeted.Length; i++)
			{
				Vector3 dirToBoi = (allTheBoisToBeTargeted[i].GetPosition() - transform.position);
				dirToBoi.y = 0;
				dirToBoi.Normalize();
				float angle = Vector3.SignedAngle(GetFlatDirectionToTarget(), dirToBoi, Vector3.up);
				bool inView = Mathf.Abs(angle) < 100f;
				bool farEnoughFromCamera = Vector3.Distance(camData.position, allTheBoisToBeTargeted[i].GetPosition()) > 3f;
				bool correctSide = (direction > 0 && angle > 0) || (direction < 0 && angle < 0);
				if (inView && farEnoughFromCamera && correctSide)
				{
					//Check if target is visible in camera.
					RaycastHit hit;
					Physics.Raycast(camData.position, (allTheBoisToBeTargeted[i].GetPosition() - camData.position).normalized, out hit, maxDistToTarget, blockTargetLayerMask.value);
					Debug.Log(hit.collider);
					if (!hit.collider || hit.collider.transform == allTheBoisToBeTargeted[i].GetTransform())
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

			if (newTarget != null)
				Target = newTarget;

		}

		#endregion

		#region Combat

		void Dodge()
		{
			if (Player.AllowDodge())
			{
				Debug.Log("Dodge allowed");
				StartCoroutine(DodgeRoutine());
				
			}
		}

		IEnumerator DodgeRoutine()
		{
			IsDodging = true;

			float t = 0;
			float invincibilityMargin = (1f - dodgeInvincibilityPercentage) / 2;

			Player.PAnimation.SetDodgeStarted();

			float currentOffset = 0;
			float lastEvaluation = 0;
			Vector3 dir = Player.PController.GetTransformedInputDirection(false);
			transform.forward = dir;

			while (IsDodging && t < dodgeDuration)
			{
				IsInvincible = (t / dodgeDuration > invincibilityMargin) && (t / dodgeDuration < 1 - invincibilityMargin);

				currentOffset = dodgeDistance.Evaluate(t / dodgeDuration) - lastEvaluation;
				lastEvaluation += currentOffset;
				Player.PController.ExternalMove(dir * currentOffset);

				yield return null;
				t += Time.smoothDeltaTime;
			}

			Player.PAnimation.SetDodgeCancelled();
			IsDodging = false;

			yield return null;
		}

		protected override void Attack()
		{
			Debug.Log("Tried attack. Allowed:" + Player.AllowAttack());
			if (Player.AllowAttack())
			{
				base.Attack();
			}
		}
		
		protected override IEnumerator LightAttackCharge()
		{
			GetCurrentWeapon().CurrentAttackState = AttackState.charge;
			Player.AnimationHandler.SetChargeStarted();

			float t01 = 0;
			float offsetTotal = 0;
			currentAttackStateTime = 0;
			Vector3 dir = Player.PController.GetTransformedInputDirection(false);


			while (GetCurrentWeapon().IsAttacking &&
					GetCurrentWeapon().CurrentAttackType == AttackType.lightAttack &&
					currentAttackStateTime < GetCurrentWeapon().GetCurrentActionDuration())
			{
				t01 = Mathf.Clamp01(currentAttackStateTime / GetCurrentWeapon().GetCurrentActionDuration());
				MoveCharacter(offsetTotal, t01);

				Player.CharacterController.SetMoveSpeedMultiplier(GetCurrentWeapon().GetMoveSpeedMultiplier(t01));
				Player.CharacterController.SetRotationSpeedMultiplier(GetCurrentWeapon().GetRotationSpeedMultiplier(t01));

				dir = Player.PController.GetTransformedInputDirection(false);
				Player.CharacterController.ExternalRotate(dir);


				offsetTotal = GetCurrentWeapon().GetMoveDistanceFromCurve(t01);
				yield return null;
			}

			Player.AnimationHandler.SetChargeCancelled();

		}
	

		#endregion


		#region HandleInputs




		void ControlsSubscribe() 
		{

			PlayerInputs.Player.TargetLock.performed += InputTargetLock;
			PlayerInputs.Player.TargetLock.Enable();
			PlayerInputs.Player.SwitchTarget.started += InputTargetSwitch;
			PlayerInputs.Player.SwitchTarget.Enable();
			PlayerInputs.Player.RunAndDodge.started += InputDodgeStarted;
			PlayerInputs.Player.RunAndDodge.canceled += InputDodgeCancelled;
			PlayerInputs.Player.RunAndDodge.Enable();
			PlayerInputs.Player.Attack.started += InputAttackStarted;
			PlayerInputs.Player.Attack.canceled += InputAttackCancelled;
			PlayerInputs.Player.Attack.Enable();
			//inputs.Player.Move.performed += InputAttackCancellationPerformed;
			//inputs.Player.Move.Enable();
		}

		void ControlsUnsubscribe() 
		{
			PlayerInputs.Player.TargetLock.performed -= InputTargetLock;
			PlayerInputs.Player.SwitchTarget.started -= InputTargetSwitch;
			PlayerInputs.Player.RunAndDodge.started -= InputDodgeStarted;
			PlayerInputs.Player.RunAndDodge.canceled -= InputDodgeCancelled;
			//inputs.Player.Move.performed -= InputAttackCancellationPerformed;

		}

		void InputTargetLock(InputAction.CallbackContext context) 
		{

			bool success = SetTarget(Player.GetCam.GetTargetingData());
			Debug.Log("InputTargetLock: " + success);
			if (!success)
				Player.GetCam.ResetCamera();

		}

		void InputTargetSwitch(InputAction.CallbackContext context) 
		{
			if (Target == null)
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
			if (Time.time - inputDodgeStartTime < Player.inputSinglePressMaxTime)
			{
				Debug.Log("Trying to dodge");
				Dodge();
			}
		}

		void InputAttackStarted(InputAction.CallbackContext context)
		{
			Attack();
		}
		void InputAttackCancelled(InputAction.CallbackContext context)
		{

		}
		void InputAttackCancellationPerformed(InputAction.CallbackContext context)
		{
			//Gets called by all inputs that have ability to cancel attack, such as movement
			if (CurrentWeapon.IsAttacking)
				CancelAttack();
		}


		#endregion

		#region IAllowedActions
		
		public bool AllowMove() 
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;

			return output;
		}

		public bool AllowRun() 
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;
			output = IsBlocking ? false : output;

			return output;
		}

		public bool AllowAttack() 
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;

			if (CurrentWeapon)
				output = CurrentWeapon.AttackPendingAllowed(currentAttackStateTime) ? output : false;
			else
				output = false;

			return output;
		}

		public bool AllowDodge() 
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;
			if (CurrentWeapon && CurrentWeapon.IsAttacking)
				output = CurrentWeapon.AttackCancellable() ? output : false;

			return output;
		}
		public bool AllowRotate()
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;
				
			return output;
		}
		
		#endregion

	}
}
