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
	public class PlayerCombatHandler : CharacterCombatHandler, IAllowedActions
	{
		[Header("Target crap")]

		[SerializeField] private GameObject targetIndicatorPrefab;
		[SerializeField] private float maxDistToTarget = 10f;
		[SerializeField] private LayerMask targetLayerMask;
		[SerializeField] private LayerMask blockTargetLayerMask;

		private GameObject targetIndicator;
		public UnityEvent targetChangedEvent;


		[Header("Dodge shit")]
		[SerializeField] private float dodgeDistance = 3f;
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

		private Inputs inputs;




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

		void Update() 
		{
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
				Vector3 dodgeDir = Player.PController.GetFlatMoveDirection(false);
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
			Player.PAnimation.SetDodgeStarted(blend, dodgeDuration);

			while (IsDodging && t < dodgeDuration)
			{
				IsInvincible = (t / dodgeDuration > invincibilityMargin) && (t / dodgeDuration < 1 - invincibilityMargin);
				float distThisFrame = (dodgeDistance / dodgeDuration) * Time.smoothDeltaTime;
				Vector3 offset = direction * distThisFrame;
				Player.PController.ExternalMove(offset);

				yield return null;
				t += Time.smoothDeltaTime;
			}

			IsDodging = false;
			Player.PAnimation.SetDodgeCancelled();

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


		protected override void CharacterMovementDuringAttack(Vector3 moveDirection, float moveOffset)
		{
			base.CharacterMovementDuringAttack(moveDirection, moveOffset);

			if (CurrentWeapon.CanRotate(Target != null))
				Player.PController.ExternalRotateToInputDirection();

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
			inputs.Player.Move.performed += InputAttackCancellationPerformed;
			inputs.Player.Move.Enable();
		}

		void ControlsUnsubscribe() 
		{
			inputs.Player.TargetLock.performed -= InputTargetLock;
			inputs.Player.SwitchTarget.started -= InputTargetSwitch;
			inputs.Player.RunAndDodge.started -= InputDodgeStarted;
			inputs.Player.RunAndDodge.canceled -= InputDodgeCancelled;
			inputs.Player.Move.performed -= InputAttackCancellationPerformed;

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
			if (Time.time - inputDodgeStartTime < Player.inputMaxPressTime)
			{
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
			if (CurrentWeapon && CurrentWeapon.IsAttacking)
				output = CurrentWeapon.CanMove(Target != null) ? output : false;

			return output;
		}

		public bool AllowRun() 
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;
			output = IsBlocking ? false : output;
			if (CurrentWeapon && CurrentWeapon.IsAttacking)
				output = CurrentWeapon.CanMove(Target != null) ? false : output;

			return output;
		}

		public bool AllowAttack() 
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;
			output = CurrentWeapon.CanAttack(Target != null) ? output : false;

			return output;
		}

		public bool AllowDodge() 
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;
			if (CurrentWeapon)
				output = CurrentWeapon.IsAttacking ? false : output;

			return output;
		}
		public bool AllowRotate()
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;
			if (CurrentWeapon && CurrentWeapon.IsAttacking)
				output = false;
				
			return output;
		}
		
		#endregion

	}
}
