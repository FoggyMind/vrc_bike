
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

[RequireComponent(typeof(Rigidbody))]
public class Bike : UdonSharpBehaviour {
	[SerializeField] private BikeHandle handle;
	[SerializeField] private CastWheel frontWheel;
	[SerializeField] private CastWheel rearWheel;
	[SerializeField] private Transform cm;

	[Header("Drive force on speed")]
	[SerializeField] private float brakeTorque = 250f;
	[SerializeField] private float lowTorque = 200f;
	[SerializeField] private float topTorque = 10f;
	[SerializeField] private float topSpeed = 35f;
	
	[Header("Wind force on speed")]
	[SerializeField] private float lowDrag = 0.01f;
	[SerializeField] private float topDrag = 0.1f;
	[SerializeField] private float lowDragSpeed = 35f;
	[SerializeField] private float topDragSpeed = 70f;
	[HideInInspector][SerializeField] private float downForceK = 0.01f;

	[Header("Laying on turn")]
	[SerializeField] private float maxLayingAngle = 30f;
	[SerializeField] private float maxLayingAngleVelocity = 10f;
	[SerializeField] private float layingTime = 0.1f;

	[Header(" ")]
	[SerializeField] private GameObject[] activeateOnSeat;
	[SerializeField] private Text speedText;

	[Header(" ")]
	float steering = 0f;
	float motor = 0f;
	float brakeF = 0f;
	float brakeR = 0f;
	float velocity = 0f;
	float layingAngle = 0f;
	[HideInInspector] public bool seated = false;

	Rigidbody rb;
	Vector3 resetPos;
	Quaternion resetRot;

	void Start() {
		rb = GetComponent<Rigidbody>();
		resetPos = transform.position;
		resetRot = transform.rotation;
	}

	void FixedUpdate() {
		if (!seated) {
			return;
		}

		if (Networking.LocalPlayer == null || Networking.IsOwner(gameObject)) {
			ProcessPhysics(Time.fixedDeltaTime);
		}
	}

	public void LateUpdate() {
		if (Networking.LocalPlayer == null || Networking.IsOwner(gameObject)) {
			if (!seated) {
				if (!rb.isKinematic) {
					ProcessPhysics(Time.deltaTime);
					ProcessInput(Time.deltaTime);
				}
			}
			else {
				ProcessInput(Time.deltaTime);
				UpdateUI();
			}

			if (cm != null) {
				rb.centerOfMass = Vector3.Scale(cm.localPosition, transform.localScale);
			}
		}
	}

	private void ProcessPhysics(float dt) {
		CalculateVelocity();
		ProcessWindForce();
		LayOnTurn(dt);
	}

	private void CalculateVelocity() {
		if (rb.velocity.magnitude < 0.01f) {
			velocity = 0.0f;
			return;
		}
		var direction = transform.forward;
		var sign = Mathf.Sign(Vector3.Dot(rb.velocity.normalized, direction));
		velocity = Vector3.Project(rb.velocity, direction).magnitude * sign;
	}

	private void ProcessWindForce() {
		var ax = lowDragSpeed;
		var ay = lowDrag;
		var bx = topDragSpeed;
		var by = topDrag;
		var v = Mathf.Abs(velocity) * 3.6f;
		var d = Mathf.Max(ay, ay + (v - ax) * (by - ay) / (bx - ax));
		rb.drag = d;
		rb.angularDrag = d;

		//rb.AddForce(-transform.up * downForceK * (0.5f * velocity * velocity));
	}

	private void ProcessInput(float dt) {
		if (!seated) {
			handle.steering = 0.0f;
			handle.motor = 0.0f;
			handle.brakeF = 0.0f;
			handle.brakeR = 0.0f;
		}

		handle.ProcessInput(dt);

		steering = handle.steering;
		motor = handle.motor;
		brakeF = handle.brakeF;
		brakeR = handle.brakeR;

		//if (velocity < 0.1f && brakeR > 0.1f && motor > 0.1f) {
		//	motor = -motor;
		//	brakeR = 0.0f;
		//}

		if (velocity <= 0.1f && brakeF > 0.1f && motor < 0.1f) {
			var temp = motor;
			motor = -brakeF;
			brakeF = temp;
			brakeR = temp;
		}

		frontWheel.brake = brakeF * brakeTorque;
		rearWheel.brake = brakeR * brakeTorque;

		var ax = 0;
		var ay = lowTorque;
		var bx = topSpeed;
		var by = topTorque;
		var v = Mathf.Abs(velocity) * 3.6f;
		var torque = Mathf.Max(0.0f, ay + (v - ax) * (by - ay) / (bx - ax));
		rearWheel.torque = motor * torque;
	}

	private void LayOnTurn(float dt) {
		var targetLayingAngle = maxLayingAngle * -steering;

		targetLayingAngle = Mathf.Lerp(0, targetLayingAngle, Mathf.Min(Mathf.Abs(velocity) / maxLayingAngleVelocity, 1f));

		layingAngle = Mathf.LerpAngle(layingAngle, targetLayingAngle, dt / layingTime);

		var euler = transform.rotation.eulerAngles;
		transform.rotation = Quaternion.Euler(euler.x, euler.y, layingAngle);
	}

	public void OnSeat() {
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		Networking.SetOwner(Networking.LocalPlayer, frontWheel.gameObject);
		Networking.SetOwner(Networking.LocalPlayer, rearWheel.gameObject);
		Networking.SetOwner(Networking.LocalPlayer, handle.gameObject);

		seated = true;
		rb.isKinematic = false;
		handle.Activate();

		foreach (var target in activeateOnSeat) {
			target.SetActive(true);
		}
	}

	public void OnLeave() {
		seated = false;
		handle.Deactivate();

		foreach (var target in activeateOnSeat) {
			target.SetActive(false);
		}
	}

	public void ResetTransformOther() {
		if (!seated) {
			ResetTransform();
		}
	}

	public void ResetTransform() {
		transform.position = resetPos;
		transform.rotation = resetRot;
		rb.velocity = new Vector3(0, 0, 0);
	}

	private void UpdateUI() {
		if (speedText != null) {
			float KmHour = velocity * 3.6f;
			speedText.text = KmHour.ToString("F0");
		}
	}
}
