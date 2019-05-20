using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Experimental.Input;
using UnityEngine;

namespace Dungeon.Player
{
	public enum AttackType
	{
		lightAttack,
		heavyAttack
		//lightChargedAttack,
		//heavyChargedAttack
	}




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
		[SerializeField] private Items.Weapon currentWeapon;

		[Header("Dodge shit")]
		[SerializeField] private float dodgeDirectionalDistance = 3f;
		[SerializeField] private float dodgeDirectionalDuration = 0.35f;
		[SerializeField] private float dodgeBackstepDistance = 1.5f;
		[SerializeField] private float dodgeBackstepDuration = 0.2f;
		[SerializeField, Range(0f,1f)] private float dodgeInvincibilityPercentage = 0.75f;

		[Header("Block shit")]

		[Header("Other shit")]



		float inputDodgeStartTime = 0;
		float inputTargetSwitchTime = 0;
		readonly float inputTargetSwitchInterval = 0.1f;


		public bool isBlocking
		{
			get;
			private set;
		}
		public bool isDodging 
		{
			get;
			private set;
		}
		public bool isStunned
		{
			get;
			private set;
		}
		public bool isAttacking
		{
			get;
			private set;
		}
		public bool isInvincible
		{
			get;
			private set;
		}

		private PlayerManager _pManager;
		private PlayerManager PManager
		{
			get
			{
				if (!_pManager)
					_pManager = GetComponent<PlayerManager>();

				return _pManager;
			}
		}
		private Inputs inputs;



		#region Initialization

		void Awake()
		{
			ControlsSubscribe();
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

		public float GetMaxDistToTarget {
			get { return maxDistToTarget; }
		}

		public Vector3 GetFlatDirectionToTarget() {
			if (Target)
			{
				var dirToTarget = Target.transform.position - transform.position;
				dirToTarget.y = 0;
				return dirToTarget.normalized;
			} else
			{
				return -PManager.GetCam.GetCurrentFlatDirection();
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
					var enemy = cols[i].GetComponent<Enemy>();
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
				Vector3 dodgeDir = PManager.PController.GetFlatMoveDirection();
				if (dodgeDir.magnitude > 0)
				{
					dodgeDir.y = 0;
					StartCoroutine(DodgeRoutine(dodgeDir.normalized, false));
				}
				else
				{
					dodgeDir = -transform.forward;
					dodgeDir.y = 0;
					StartCoroutine(DodgeRoutine(dodgeDir.normalized, true));

				}
			}
		}

		IEnumerator DodgeRoutine(Vector3 direction, bool backstep)
		{
			isDodging = true;

			float t = 0;
			float duration = backstep ? dodgeBackstepDuration : dodgeDirectionalDuration;
			float distance = backstep ? dodgeBackstepDistance : dodgeDirectionalDistance;
			float invincibilityMargin = (1f - dodgeInvincibilityPercentage) / 2;
			
			float angle = Vector3.SignedAngle(transform.forward, Vector3.forward, Vector3.up);
			Vector3 relativeMoveDirection = Quaternion.Euler(0, angle, 0) * direction;
			Vector2 blend = new Vector2(relativeMoveDirection.x, relativeMoveDirection.z).normalized;
			PManager.PAnimation.SetDodgeStarted(blend, backstep, duration);

			while (isDodging && t < duration)
			{
				isInvincible = (t / duration > invincibilityMargin) && (t / duration < 1 - invincibilityMargin);
				float distThisFrame = (distance / duration) * Time.smoothDeltaTime;
				Vector3 offset = direction * distThisFrame;
				PManager.PController.ExternalMove(offset);

				yield return null;
				t += Time.smoothDeltaTime;
			}

			isDodging = false;
			PManager.PAnimation.SetDodgeCancelled();

			yield return null;
		}


		void Attack()
		{
			if (PManager.AllowAttack())
			{
				StartCoroutine(AttackRoutine());
			}
		}

		IEnumerator AttackRoutine()
		{
			isAttacking = true;
			float t = 0;

			PManager.PAnimation.SetAttackStarted();

			while (isAttacking && t < currentWeapon.GetAttackDuration())
			{
				t += Time.smoothDeltaTime;
				yield return null;
			}

			PManager.PAnimation.SetAttackCancelled();

			isAttacking = false;
			yield return null;
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
			inputs.Player.RunAndDodge.cancelled += InputDodgeCancelled;
			inputs.Player.RunAndDodge.Enable();
			inputs.Player.Attack.started += InputAttackStarted;
			inputs.Player.Attack.cancelled += InputAttackCancelled;
			inputs.Player.Attack.Enable();
		}

		void ControlsUnsubscribe() 
		{
			inputs.Player.TargetLock.performed -= InputTargetLock;
			inputs.Player.TargetLock.Disable();
			inputs.Player.SwitchTarget.started -= InputTargetSwitch;
			inputs.Player.SwitchTarget.Disable();
			inputs.Player.RunAndDodge.started -= InputDodgeStarted;
			inputs.Player.RunAndDodge.cancelled -= InputDodgeCancelled;
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

			output = isDodging ? false : output;
			output = isAttacking ? false : output;
			output = isStunned ? false : output;

			return output;
		}

		public bool AllowRun() 
		{
			bool output = true;

			output = isDodging ? false : output;
			output = isAttacking ? false : output;
			output = isStunned ? false : output;
			output = isBlocking ? false : output;

			return output;
		}

		public bool AllowAttack() 
		{
			bool output = true;

			output = isDodging ? false : output;
			output = isStunned ? false : output;

			return output;
		}

		public bool AllowDodge() 
		{
			bool output = true;

			output = isDodging ? false : output;
			output = isAttacking ? false : output;
			output = isStunned ? false : output;

			return output;
		}
		public bool AllowRotate()
		{
			bool output = true;

			output = isDodging ? false : output;
			output = isAttacking ? false : output;
			output = isStunned ? false : output;

			return output;
		}
		
		#endregion

	}
}
