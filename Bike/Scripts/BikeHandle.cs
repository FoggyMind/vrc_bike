
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class BikeHandle : UdonSharpBehaviour {
	public HandlePickup controllerL;
	public HandlePickup controllerR;
	public Transform visual;

	[Range(20.0f, 60.0f)]
	public float maxAngle = 40.0f;
	public float visualAngleK = 0.9f;
	private float angle = 0.0f;

	[HideInInspector][UdonSynced(UdonSyncMode.Linear)] public float steering = 0;
	[HideInInspector] public float motor = 0;
	[HideInInspector] public float brakeR = 0;
	[HideInInspector] public float brakeF = 0;

	private bool active = false;
	private bool vrMode = false;

	void Start() {
		Deactivate();
	}

	public void Activate() {
		active = true;

		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		Networking.SetOwner(Networking.LocalPlayer, controllerL.gameObject);
		Networking.SetOwner(Networking.LocalPlayer, controllerR.gameObject);

		vrMode = Networking.LocalPlayer.IsUserInVR();

		if (vrMode) {
			controllerL.gameObject.SetActive(true);
			controllerR.gameObject.SetActive(true);
		}
		else {
			controllerL.gameObject.SetActive(false);
			controllerR.gameObject.SetActive(false);
		}
	}

	public void Deactivate() {
		active = false;

		if (controllerL.picked) {
			controllerL.pickup.Drop();
		}
		if (controllerR.picked) {
			controllerR.pickup.Drop();
		}

		controllerL.gameObject.SetActive(false);
		controllerR.gameObject.SetActive(false);
	}

	public void ProcessInput(float dt) {
		if (!active || !Networking.IsOwner(gameObject)) {
			UpdateVisual();
			return;
		}

		motor = 0.0f;
		brakeR = 0.0f;
		brakeF = 0.0f;

		if (vrMode) {
			controllerL.ProcessPosition();
			controllerR.ProcessPosition();

			CalculateAngle();

			steering = Mathf.Clamp(-angle / maxAngle, -1.0f, 1.0f);

			if (controllerL.picked) {
				brakeF = Input.GetAxis("Oculus_CrossPlatform_PrimaryIndexTrigger"); //left
				brakeR = brakeF;
			}
			if (controllerR.picked) {
				motor = Input.GetAxis("Oculus_CrossPlatform_SecondaryIndexTrigger"); //right
			}

			//if (controllerL.jump || controllerR.jump) {
			//	motor = 1.0f;
			//}
		}
		else {
			var delta = dt / 0.4f;

			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
				steering = Mathf.Max(steering - delta, -1.0f);
			}
			else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
				steering = Mathf.Min(steering + delta, 1.0f);
			}
			else if (Mathf.Abs(angle) < 0.01f) {
				if (steering < 0.0f) {
					steering = Mathf.Min(steering + delta, 0.0f);
				}
				if (steering > 0.0f) {
					steering = Mathf.Max(steering - delta, 0.0f);
				}
			}

			if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
				motor = 1.0f;
			}

			if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
				brakeR = 1.0f;
				brakeF = 1.0f;
			}
		}

		UpdateVisual();
	}

	private void CalculateAngle() {
		var a = 0.0f;
		Vector3 lp = controllerL.transform.localPosition;
		Vector3 rp = controllerR.transform.localPosition;
		lp.z = 0.0f;
		rp.z = 0.0f;

		if (rp.x > lp.x) {
			Vector3 v = rp - lp;

			if (rp.y > lp.y) {
				a = Mathf.Acos(Vector3.Dot(v.normalized, Vector3.right)) * Mathf.Rad2Deg; //0..90
			}
			else {
				a = -Mathf.Acos(Vector3.Dot(v.normalized, Vector3.right)) * Mathf.Rad2Deg; //0..-90
			}
		}
		else {
			Vector3 v = lp - rp;

			if (rp.y > lp.y) {
				a = Mathf.Acos(Vector3.Dot(v.normalized, Vector3.left)) * Mathf.Rad2Deg; //90..180
			}
			else {
				a = -Mathf.Acos(Vector3.Dot(v.normalized, Vector3.left)) * Mathf.Rad2Deg; //-90..-180
			}
		}

		if (angle > 90.0f && a < 0.0f) {
			a += 360.0f;
		}
		else if (angle < -90.0f && a > 0.0f) {
			a -= 360.0f;
		}

		angle = Mathf.Clamp(a, -maxAngle, maxAngle);
	}

	private void UpdateVisual() {
		if (visual != null) {
			visual.transform.localRotation = Quaternion.Euler(0, 0, -steering * maxAngle * visualAngleK);
		}
	}
}
