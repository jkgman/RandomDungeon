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
		#region Variables & References

		//_______ Start of Events
		public UnityEvent targetChangedEvent;
		//_______ End of Events

		//_______ Start of Exposed variables
		[Header("Targeting")]
		[Tooltip("Create target graphic from this")]
		[SerializeField] private GameObject targetIndicatorPrefab = null;
		[Tooltip("Get targets from this radius. Disconnects with active target outside the radius.")]
		[SerializeField] private float maxDistToTarget = 10f;
		[Tooltip("Which layers are checked for possible targets.")]
		[SerializeField] private LayerMask targetLayerMask = new LayerMask();
		[Tooltip("If target is behind obstacles on these layers, target disconnects.")]
		[SerializeField] private LayerMask blockTargetLayerMask = new LayerMask();
		[Tooltip("Automatically look for new target when previous disconnects.")]
		[SerializeField] private bool autoNewTarget = true; //________________________________________________________________________TO DO

		[Header("Dodging")]
		[Tooltip("Determines how player moves during dodge.")]
		[SerializeField] private AnimationCurve dodgeDistance = new AnimationCurve();
		[Tooltip("How long dodge should last.")]
		[SerializeField] private float dodgeDuration = 0.35f;
		[Tooltip("Amount of time during dodge when damage wont be taken. Invincibility is placed in the middle of dodge.")]
		[SerializeField, Range(0f, 1f)] private float dodgeInvincibilityPercentage = 0.75f;
		//_______ End of Exposed variables

		//_______ Start of Hidden variables
		private GameObject targetIndicator; //Current indicator created from targetIndicatorPrefab
		private float inputDodgeStartTime = 0; //Used for timers
		private float inputTargetSwitchTime = 0; //Used for timers
		private float inputTargetSwitchInterval = 0.1f; //Minimum amount between changing the target.
		//_______ End of Hidden variables

		//_______ Start of Class References
		private PlayerWeapon _pWeapon;
		private PlayerWeapon PWeapon
		{
			get
			{
				if (!_pWeapon)
					_pWeapon = GetComponentInChildren<PlayerWeapon>();

				return _pWeapon;
			}
		}

		private Inputs _inputs;
		private Inputs Inputs
		{
			get
			{
				if (_inputs == null)
					_inputs = new Inputs();

				return _inputs;
			}
		}

		private CameraController _cam;
		private CameraController Cam
		{
			get
			{
				if (!_cam)
				{
					var temp = Camera.main;
					if (temp)
						_cam = temp.transform.root.GetComponent<CameraController>();
				}
				return _cam;
			}
		}

		private PlayerMovement _pMovement;
		private PlayerMovement PMovement
		{
			get
			{
				if (!_pMovement)
					_pMovement = GetComponent<PlayerMovement>();

				return _pMovement;
			}
		}
		private PlayerAnimationHandler _pAnimation;
		private PlayerAnimationHandler PAnimation
		{
			get
			{
				if (!_pAnimation)
					_pAnimation = GetComponent<PlayerAnimationHandler>();

				return _pAnimation;
			}
		}
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
		//_______ End of Class References

		#endregion Variables & References

		#region Getters & Setters

		private ITargetable _currentTarget;
		public ITargetable CurrentTarget {
			get { return _currentTarget; }
			set {
				if (value != _currentTarget)
					targetChangedEvent.Invoke();

				_currentTarget = value;
			}
		}

		public float GetMaxDistToTarget 
		{
			get { return maxDistToTarget; }
		}

		public Vector3 GetFlatDirectionToTarget() 
		{
			if (CurrentTarget != null)
			{
				var dirToTarget = CurrentTarget.GetPosition() - transform.position;
				dirToTarget.y = 0;
				return dirToTarget.normalized;
			} 
			else
			{
				return -PMovement.GetFlatMoveDirection();
			}
		}

		#endregion Getters & Setters

		#region Initialization & Updates

		private void OnEnable()
		{
			ControlsSubscribe();
		}

		private void Start()
		{
			if (targetChangedEvent == null)
				targetChangedEvent = new UnityEvent();

			if (Cam && Cam.CameraTransformsUpdated != null)
				Cam.CameraTransformsUpdated.AddListener(CameraReadyEvent);

		}
		
		private void OnDisable() 
		{
			targetChangedEvent.RemoveAllListeners();
			targetChangedEvent = null;

			if (Cam && Cam.CameraTransformsUpdated != null)
				Cam.CameraTransformsUpdated.RemoveListener(CameraReadyEvent);

			ControlsUnsubscribe();
		}

		protected override void Update() 
		{
			base.Update();
			UpdateTarget();
			UpdateDebug();
		}

		void UpdateDebug()
		{
			if (PWeapon && PlayerDebugCanvas.Instance)
			{
				if (PWeapon.IsAttacking)
				{
					if (PWeapon.CurrentAttackState == AttackState.charge)
						PlayerDebugCanvas.Instance.SetDebugText("Charge");
					if (PWeapon.CurrentAttackState == AttackState.attack)
						PlayerDebugCanvas.Instance.SetDebugText("Attack");
					if (PWeapon.CurrentAttackState == AttackState.recovery)
						PlayerDebugCanvas.Instance.SetDebugText("Recovery");
				}
				else
				{

					PlayerDebugCanvas.Instance.SetDebugText("Idle");
				}
			}
		}

		/// <summary>
		/// Check if target is still valid. If not, disconnect and check if new target should be found.
		/// </summary>
		void UpdateTarget()
		{
			if (CurrentTarget != null && ((CurrentTarget.GetPosition() - transform.position).magnitude > maxDistToTarget || !CurrentTarget.IsTargetable()))
			{
				CurrentTarget = null;

				if (autoNewTarget)
				{
					bool success = SetTarget(Cam.GetTargetingData());
					Debug.Log("InputTargetLock: " + success);
					if (!success)
						Cam.ResetCamera();
				}
			}
		}

		/// <summary>
		/// Updates target indicator graphic's position rotation and existence. Is called after cameraController has updated position.
		/// </summary>
		void UpdateTargetIndicator()
		{
			if (CurrentTarget == null || !Cam) // Indicator relies on camera
			{
				if (targetIndicator)
					Destroy(targetIndicator);
			}
			else
			{
				if (!targetIndicator)
				{
					if (targetIndicatorPrefab)
						targetIndicator = Instantiate(targetIndicatorPrefab, CurrentTarget.GetPosition(), Quaternion.LookRotation(Cam.transform.position - CurrentTarget.GetPosition()));
				}
				else
				{
					targetIndicator.transform.position = CurrentTarget.GetPosition();
					targetIndicator.transform.rotation = Quaternion.LookRotation(Cam.transform.position - targetIndicator.transform.position);
				}
			}

		}

		#endregion

		#region Events

		//Is called after camera has updated its transform.
		void CameraReadyEvent() {
			// Target indicator needs the latest camera position, otherwise looks bad at low fps.
			UpdateTargetIndicator();
		}

		#endregion

		#region Targeting

		/// <summary>
		/// Looks for targets and sets CurrentTarget if one is found.
		/// </summary>
		/// <param name="data">Camera's position, rotation and field of view</param>
		private bool SetTarget(CameraTargetingData data) {
			ITargetable oldTarget = CurrentTarget;

			if (CurrentTarget != null)
			{
				CurrentTarget = null;
				return true;
			}

			ITargetable[] nearbyTargets = FindTargets();
			if (nearbyTargets != null && nearbyTargets.Length > 0)
				CurrentTarget = FindBestTarget(data, nearbyTargets);

			return CurrentTarget != oldTarget; //Return true if target changed in any way
		}

		/// <summary>
		/// Gets an array of available targets in maxDistToTarget radius.
		/// </summary>
		private ITargetable[] FindTargets() {
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

		/// <summary>
		/// Looks through an array of available targets and chooses the one that is most suitable in terms of direction, distance and visibility.
		/// </summary>
		/// <param name="camData">Camera's position, rotation and field of view.</param>
		/// <param name="allTheBoisToBeTargeted">Array of available targets</param>
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


		/// <summary>
		/// Tries to look for new targets if there are more than current target available, and switches.
		/// </summary>
		/// <param name="direction">Should targets be looked from left or right (-1 or +1)</param>
		void SwitchTarget(int direction) {
			ITargetable[] allTheBoisToBeTargeted = FindTargets();
			CameraTargetingData camData = Cam.GetTargetingData();

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
				CurrentTarget = newTarget;

		}

		#endregion

		#region Combat

		/// <summary>
		/// Start dodge if allowed.
		/// </summary>
		void Dodge()
		{
			if (Player.AllowDodge())
			{
				Debug.Log("Dodge allowed");
				StartCoroutine(DodgeRoutine());
				
			}
		}

		/// <summary>
		/// Updates position and invincibility throughtout the dodge duration
		/// </summary>
		IEnumerator DodgeRoutine()
		{
			IsDodging = true;

			float t = 0;
			float invincibilityMargin = (1f - dodgeInvincibilityPercentage) / 2;

			PAnimation.SetDodgeStarted();

			float currentOffset = 0;
			float lastEvaluation = 0;
			Vector3 dir = PMovement.GetTransformedInputDirection(false);
			transform.forward = dir;

			while (IsDodging && t < dodgeDuration)
			{
				IsInvincible = (t / dodgeDuration > invincibilityMargin) && (t / dodgeDuration < 1 - invincibilityMargin);

				currentOffset = dodgeDistance.Evaluate(t / dodgeDuration) - lastEvaluation;
				lastEvaluation += currentOffset;
				PMovement.ExternalMove(dir * currentOffset);

				yield return null;
				transform.forward = dir;
				t += Time.smoothDeltaTime;
			}

			PAnimation.SetDodgeCancelled();
			IsDodging = false;

			yield return null;
		}

		/// <summary>
		/// Start attack if allowed.
		/// </summary>
		protected override void Attack()
		{
			Debug.Log("Tried attack. Allowed:" + Player.AllowAttack());
			if (Player.AllowAttack())
			{
				base.Attack();
			}
		}
	
		#endregion

		#region HandleInputs

		void ControlsSubscribe() 
		{

			Inputs.Player.TargetLock.performed += InputTargetLock;
			Inputs.Player.TargetLock.Enable();
			Inputs.Player.SwitchTarget.started += InputTargetSwitch;
			Inputs.Player.SwitchTarget.Enable();
			Inputs.Player.RunAndDodge.started += InputDodgeStarted;
			Inputs.Player.RunAndDodge.canceled += InputDodgeCancelled;
			Inputs.Player.RunAndDodge.Enable();
			Inputs.Player.Attack.started += InputAttackStarted;
			Inputs.Player.Attack.canceled += InputAttackCancelled;
			Inputs.Player.Attack.Enable();
			//inputs.Player.Move.performed += InputAttackCancellationPerformed;
			//inputs.Player.Move.Enable();
		}

		void ControlsUnsubscribe() 
		{
			Inputs.Player.TargetLock.performed -= InputTargetLock;
			Inputs.Player.SwitchTarget.started -= InputTargetSwitch;
			Inputs.Player.RunAndDodge.started -= InputDodgeStarted;
			Inputs.Player.RunAndDodge.canceled -= InputDodgeCancelled;
			//inputs.Player.Move.performed -= InputAttackCancellationPerformed;

		}

		void InputTargetLock(InputAction.CallbackContext context) 
		{

			bool success = SetTarget(Cam.GetTargetingData());
			Debug.Log("InputTargetLock: " + success);
			if (!success)
				Cam.ResetCamera();

		}

		void InputTargetSwitch(InputAction.CallbackContext context) 
		{
			if (CurrentTarget == null)
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
			if (PWeapon.IsAttacking)
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

			if (PWeapon)
				output = PWeapon.AttackPendingAllowed(elapsedAttackTime) ? output : false;
			else
				output = false;

			return output;
		}

		public bool AllowDodge() 
		{
			bool output = true;

			output = IsDodging ? false : output;
			output = IsStunned ? false : output;
			if (PWeapon && PWeapon.IsAttacking)
				output = PWeapon.AttackCancellable() ? output : false;

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
