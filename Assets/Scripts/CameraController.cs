using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


///
/// 
/// Camera follows a dummy-position, which is lerping to player.
/// Camera relative to dummy is set in lateupdate without lerping.
/// During lock-on (targeting enemy) camera locks its direction behind player, showing the enemy all the time.
/// 
///

namespace Dungeon
{
	public struct CameraTargetingData
	{
		public Vector3 forward;
		public Vector3 position;
		public float fov;
	}

	public class CameraController : MonoBehaviour
	{
		//Different lerp speeds for different situations/states
		private const float LERP_ANGLE_DEFAULT = 3f;            //Camera offset direction from player. good values 1-8, bigger value makes turn radius smaller
		private const float LERP_ANGLE_TARGETING = 10f;			//Same as above, Affects positioning. 
		private const float LERP_DUMMY_DEFAULT = 8f;			//Dummy is a position that follows player & target. It is used to determine camera positioning
		private const float LERP_DUMMY_TARGETING = 12f;          //Dummy is a position that follows player & target. It is used to determine camera positioning
		private const float LERP_ROTATION_DEFAULT = 100f;		//Look rotation speed
		private const float LERP_ROTATION_TARGETING = 50f;		//Look rotation speed
		private const float LERP_LOOKAT_POS_DEFAULT = 50f;		//Dummy position for look rotation
		private const float LERP_LOOKAT_POS_TARGETING = 80f;	//Dummy position for look rotation
		private const float LERP_CAM_TO_DEFAULT = 2f;			//Speed of going back to default vertical angle when no inputs detected
		private const float LERP_CAM_DISTANCE_TO_DEFAULT = 10f; //Speed of going back to default distance after hitting walls etc


		[SerializeField] private Vector3 lookAtOffset = new Vector3();
		[SerializeField] private float distDefault = 4f;
		[SerializeField] private float distTargeting = 4f;

		[SerializeField] private float angleVTargeting = 30f;
		[SerializeField] private float angleHTargeting = 30f;

		[SerializeField] private float lookSensitivityMouse = 10f;
		[SerializeField] private float lookSensitivityGamepad = 100f;

		[SerializeField] private LayerMask collidingLayers;


		//Waits until camera starts going back to default angle.
		private readonly float autoCameraDelayAfterLookInput = 0.5f;

		//Camera angle limits and default for when inputs are not detected
		private readonly float minVAngle = -85f;
		private readonly float maxVAngle = 40f;
		private readonly float defaultVAngle = -25f;


		private Vector3 dummyPos = Vector3.zero; // Position that follows player or its target, Camera always follows dummyPos and not player.
		private Vector3 oldDummyPos = Vector3.zero; // Last Frame
		private Vector3 lookAtPoint = Vector3.zero;
		private float currentDistance; //Multiplier for camera orbit direction (from dummyPos)


		//Input variables
		private Vector2 lookModifier = Vector2.zero; //The raw value from "Look" input
		private float lastLookInput = 0; //Last Time.time when "Look" input was detected
		private float targetChangeTime = 0; //Last Time.time that player's target changed in any way

		public UnityEvent CameraTransformsUpdated;




		#region Getters & Setters

		private Characters.PlayerCombatHandler _playerCombat;
		private Characters.PlayerCombatHandler PlayerCombat
		{
			get
			{
				if (!_playerCombat)
				{
					var go = GameObject.FindGameObjectWithTag("Player");
					if (go)
						_playerCombat = go.GetComponent<Characters.PlayerCombatHandler>();
				}
				return _playerCombat;
			}
		}
		private Characters.Player _player;
		private Characters.Player Player
		{
			get
			{
				if (!_player)
				{
					var go = GameObject.FindGameObjectWithTag("Player");
					if (go)
						_player = go.GetComponent<Characters.Player>();
				}
				return _player;
			}
		}


		private Vector3 PlayerPos
		{
			get { return Player.GetPhysicalPosition(); }
		}
		private ITargetable PlayerTarget
		{
			get { return PlayerCombat.CurrentTarget; }

		}

		//The angle that currentAngle lerps towards
		private Vector2 _rawAngle;
		private Vector2 RawAngle {
			get { return _rawAngle; }
			set {
				_rawAngle.x = Mathf.Clamp(value.x, minVAngle, maxVAngle);
				_rawAngle.y = value.y;
			}
		}

		//CurrentAngle, which aims to be same as rawAngle but lerps behind.
		private Vector2 _curAngle;
		public Vector2 CurAngle {
			get { return _curAngle; }
			private set {
				_curAngle.x = Mathf.Clamp(value.x, minVAngle, maxVAngle);
				_curAngle.y = value.y;
			}
		}

		public CameraTargetingData GetTargetingData() {
			return new CameraTargetingData() {
				forward = -GetCurrentDirection(),
				position = transform.position,
				fov = Camera.main.fieldOfView
			};
		}
		/// <summary>
		/// Gets Vector right relative to last rawFlatDir between camera and target.
		/// </summary>
		Vector3 GetRawRight() {
			//var r = Vector3.Cross(rawFlatDirNoOffset, Vector3.up);
			var r = Vector3.Cross(Quaternion.Euler(0, RawAngle.y, 0) * Vector3.forward, Vector3.up);
			r.y = 0;
			return r.normalized;
		}

		public Vector3 GetRawDirection() {
			return (Quaternion.Euler(new Vector3(RawAngle.x, RawAngle.y, 0)) * Vector3.forward).normalized;
		}
		public Vector3 GetRawFlatDirection() {
			return (Quaternion.Euler(new Vector3(0, RawAngle.y, 0)) * Vector3.forward).normalized;
		}

		public Vector3 GetCurrentDirection() {
			return (Quaternion.Euler(new Vector3(CurAngle.x, CurAngle.y, 0)) * Vector3.forward).normalized;
		}
		public Vector3 GetCurrentFlatDirection() {
			var dir = GetCurrentDirection();
			dir.y = 0;
			return dir.normalized;
		}
		#endregion




		void Awake() 
		{
			if (PlayerCombat && Player)
			{
				AddEvents();
				ControlsSubscribe();
			}
		}
		void OnDisable() 
		{
			RemoveEvents();
			ControlsUnsubscribe();
		}

		void AddEvents()
		{
			if (PlayerCombat && PlayerCombat.targetChangedEvent != null)
				PlayerCombat.targetChangedEvent.AddListener(TargetChanged);
			if (CameraTransformsUpdated == null)
				CameraTransformsUpdated = new UnityEvent();

		}
		void RemoveEvents()
		{
			if (PlayerCombat && PlayerCombat.targetChangedEvent != null)
				PlayerCombat.targetChangedEvent.RemoveListener(TargetChanged);

			if (CameraTransformsUpdated != null)
				CameraTransformsUpdated.RemoveAllListeners();

			CameraTransformsUpdated = null;

		}


		#region Inputs

		private Inputs inputs;
	
		void ControlsSubscribe() 
		{
			if (inputs == null)
				inputs = new Inputs();

			inputs.Player.Look.performed += InputLookAround;
			inputs.Player.Look.Enable();
		}
		void ControlsUnsubscribe() 
		{
			inputs.Player.Look.performed -= InputLookAround;
			inputs.Player.Look.Disable();
		}

		void InputLookAround(InputAction.CallbackContext context)
		{
			lookModifier = context.ReadValue<Vector2>() * Time.smoothDeltaTime;

			if (context.control.layout == "Stick")
				lookModifier *= lookSensitivityGamepad;
			else
				lookModifier *= lookSensitivityMouse;

			lastLookInput = Time.time;
		}

		#endregion


		void TargetChanged()
		{
			targetChangeTime = Time.time;
		}

		void Update()
		{
			updateGizmos = true;
		}

		void LateUpdate()
		{
			if (!PlayerCombat || !Player) //This script relies on player.
				return;

			SetDummyPosition(); // Dummy is position target for camera.
			UpdateAutomaticCameraAngleDirection(); // Automatic camera movements
			ProcessCurrentAngle(); // Lerps current angles towards raw angles
			ApplyInputDirection(); // Offsets to default camera movements modified by inputs
			PhysicsCheck();
			SetPositionByDirection(); //Finally applies camera position according to earlier modifier values
			SetRotation(); // Rotates face towards current target

			//Send a public event that camera lateupdate is done.
			CameraTransformsUpdated.Invoke();
		}


		void SetDummyPosition()
		{
			oldDummyPos = dummyPos;

			if (PlayerTarget != null)
			{
				//Vectors and positions between target and player.
				Vector3 midPos = (PlayerTarget.GetPosition() + PlayerPos) / 2f;
				Vector3 vectorBetween = PlayerTarget.GetPosition() - PlayerPos;
				//Weight towards player the further away target is
				Vector3 goalPos = Vector3.Lerp(midPos, PlayerPos, PlayerCombat.GetMaxDistToTarget / vectorBetween.magnitude);
				//Lerp determines how much camera lags behind player
				dummyPos = Vector3.Lerp(dummyPos, goalPos + lookAtOffset, Time.smoothDeltaTime * LERP_DUMMY_TARGETING);
			}
			else
			{
				//Lerp determines how much camera lags behind player
				dummyPos = Vector3.Lerp(dummyPos, PlayerPos + lookAtOffset, Time.smoothDeltaTime * LERP_DUMMY_DEFAULT);
			}
		}

		void UpdateAutomaticCameraAngleDirection()
		{
			if (PlayerTarget != null)
			{
				//Get new direction from player-target relation
				var newFlatDir = dummyPos - PlayerTarget.GetPosition();
				newFlatDir.y = 0;
				newFlatDir.Normalize();

				//Applies offset angles so that player is not blocking view to enemy
				var x = angleVTargeting;
				var y = Vector3.SignedAngle(Vector3.forward, newFlatDir, Vector3.up) + angleHTargeting;
				RawAngle = new Vector2(x, y);
			}
			else if (lastLookInput + autoCameraDelayAfterLookInput < Time.time)
			{
				//Get new direction from old raw position because with transform.position rawAngle does not work properly elsewhere.
				var oldRawPos = oldDummyPos + (GetRawDirection() * currentDistance);
				var newFlatDir = (oldRawPos - dummyPos).normalized;
				newFlatDir.y = 0;
				newFlatDir.Normalize();
				//Set rawAngle values to match current angle
				var y = Vector3.SignedAngle(Vector3.forward, newFlatDir, Vector3.up);
				RawAngle = new Vector2(RawAngle.x, y);
			
			}
		}

		void ProcessCurrentAngle()
		{
			if (PlayerTarget != null)
			{
				//"Lerp weight" when defaulting to new rotation after target has changed. (between 0.5 and 1)
				float lerpWeight = Mathf.Clamp01((Time.time - targetChangeTime) / 1f);
				//Because of angles being between -180 and 180, quaternion is needed to lerp between eulers.
				Quaternion yRot = Quaternion.Slerp(Quaternion.Euler(0, CurAngle.y, 0), Quaternion.Euler(0, RawAngle.y, 0), Time.smoothDeltaTime * LERP_ANGLE_TARGETING * lerpWeight);
				var y = Vector3.SignedAngle(Vector3.forward, yRot * Vector3.forward, Vector3.up);
				//Vertical angles are ok with mathf.lerp
				var x = Mathf.Lerp(CurAngle.x, RawAngle.x, Time.smoothDeltaTime * LERP_ANGLE_TARGETING);

				CurAngle = new Vector2(x, y);
			}
			else
			{

				if (autoCameraDelayAfterLookInput != 0 && lastLookInput + autoCameraDelayAfterLookInput < Time.time) 
				{
					//Lerp towards default angle (vertical angles are never extreme -> mathf.lerp is ok)
					float lerpWeight = Mathf.Clamp01((Time.time - (lastLookInput + autoCameraDelayAfterLookInput)) / 3f); //Takes 3s to be full strength
					RawAngle = new Vector2(Mathf.Lerp(RawAngle.x, defaultVAngle, Time.smoothDeltaTime * LERP_CAM_TO_DEFAULT * lerpWeight), RawAngle.y);
				}

				//Because of angles being between -180 and 180, quaternion is needed to lerp between eulers.
				Quaternion yRot = Quaternion.Slerp(Quaternion.Euler(0, CurAngle.y, 0), Quaternion.Euler(0, RawAngle.y, 0), Time.smoothDeltaTime * LERP_ANGLE_DEFAULT);
				var y = Vector3.SignedAngle(Vector3.forward, yRot * Vector3.forward, Vector3.up);
				//Vertical angles are ok with mathf.lerp
				var x = Mathf.Lerp(CurAngle.x, RawAngle.x, Time.smoothDeltaTime * LERP_ANGLE_DEFAULT);

				CurAngle = new Vector2(x, y);
			}

		}

		void ApplyInputDirection()
		{
			if (PlayerTarget == null)
			{
				CurAngle += new Vector2(lookModifier.y, lookModifier.x);
				RawAngle += new Vector2(lookModifier.y, lookModifier.x);

				//Reset modifier in case there is no more input.
				lookModifier = Vector2.zero;
			}
		}

		void PhysicsCheck()
		{
			Physics.SphereCast(dummyPos, 0.5f, GetCurrentDirection(), out RaycastHit hit, distDefault, collidingLayers.value);
			if (hit.collider)
			{
				currentDistance = (dummyPos - hit.point).magnitude;
			}
			else
			{
				if (PlayerTarget != null)
					currentDistance = Mathf.Lerp(currentDistance, distTargeting, Time.smoothDeltaTime* LERP_CAM_DISTANCE_TO_DEFAULT);
				else
					currentDistance = Mathf.Lerp(currentDistance, distDefault, Time.smoothDeltaTime* LERP_CAM_DISTANCE_TO_DEFAULT);
			}
		}

		void SetPositionByDirection()
		{
			Vector3 newPos = dummyPos;
			newPos += (GetCurrentDirection() * currentDistance);
			transform.position = newPos;
		}

		public void ResetCamera()
		{
			if (PlayerTarget == null)
			{
				var x = defaultVAngle;
				var y = Vector3.SignedAngle(Vector3.forward , -PlayerCombat.transform.forward, Vector3.up);
				RawAngle = new Vector2(x, y);
			}
		}

		void SetRotation()
		{
			Quaternion newRot = transform.rotation;
			Quaternion rawLookRot = Quaternion.identity;

			if (PlayerTarget != null)
			{
				//"Lerp weight" when defaulting to new position after target has changed.
				float lerpWeight = Mathf.Clamp01((Time.time - targetChangeTime) / 1f);
				Vector3 newPoint = (PlayerTarget.GetPosition() + PlayerPos) / 2f;
				lookAtPoint = Vector3.Lerp(lookAtPoint, newPoint, Time.smoothDeltaTime * LERP_LOOKAT_POS_TARGETING * lerpWeight);
				//The goal to look at
				rawLookRot = Quaternion.LookRotation((lookAtPoint - transform.position).normalized);
				//The real direction we look at
				newRot = Quaternion.Slerp(newRot, rawLookRot, Time.smoothDeltaTime * LERP_ROTATION_TARGETING);
			}
			else
			{
				//"Lerp weight" when defaulting to new position after target has changed.
				float lerpWeight = Mathf.Clamp01((Time.time - targetChangeTime) / 1f);
				lookAtPoint = Vector3.Lerp(lookAtPoint, dummyPos, Time.smoothDeltaTime * LERP_LOOKAT_POS_DEFAULT * lerpWeight);

				//The goal to look at
				rawLookRot = Quaternion.LookRotation((lookAtPoint - transform.position).normalized);

				//The real direction we look at
				newRot = Quaternion.Slerp(newRot, rawLookRot, Time.smoothDeltaTime * LERP_ROTATION_DEFAULT);
			}

			transform.rotation = newRot;
		}
	
		#region Debug

		[SerializeField] private bool debugVisuals = false;
		Vector3[] guiPositions = new Vector3[200];
		Vector3[] guiPositionsDummy = new Vector3[200];
		Vector3[] guiForw = new Vector3[200];
		int guiPosIndex = 0;
		bool updateGizmos = false;

		void OnDrawGizmos() {
			if (!debugVisuals)
				return;
			if (Application.isPlaying && updateGizmos)
			{
				guiPositions[guiPosIndex] = transform.position;
				guiPositionsDummy[guiPosIndex] = dummyPos;
				guiForw[guiPosIndex] = -GetCurrentDirection();
			}
			for (int i = 0; i < guiPositions.Length; i++)
			{
				Gizmos.color = Color.green;
				if (guiPositions[i] != null)
					Gizmos.DrawWireSphere(guiPositions[i], 0.15f);
				Gizmos.color = Color.blue;
				if (guiPositionsDummy[i] != null)
					Gizmos.DrawWireSphere(guiPositionsDummy[i], 0.075f);
				Gizmos.color = Color.grey;
				if (guiPositions[i] != null && guiPositionsDummy[i] != null)
					Gizmos.DrawLine(guiPositions[i], guiPositionsDummy[i]);
				Gizmos.color = Color.black;
				if (guiForw[i] != null)
					Gizmos.DrawRay(guiPositions[i], guiForw[i]*2);



			}
			guiPosIndex++;
			if (guiPosIndex >= 200)
				guiPosIndex = 0;

			updateGizmos = false;
		}

		#endregion
	}
}
