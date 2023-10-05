
using UdonSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class CastWheel : UdonSharpBehaviour {
	public Rigidbody rb;
	public Transform bumper;
	public Transform view;

	[Header("Cast")]
	public LayerMask layerMask;
	[Range(2, 12)]
	public int sectors = 2;
	Vector3[] sectorsPos;

	[Header("Tire")]
	public float mass = 1f;
	public float radius = 0.35f;
	public float smallRadius = 0.05f;

	[Header("Suspension")]
	public float length = 0.2f;
	public float spring = 1000f;
	public float damper = 100f;
	public float damperTime = 0.1f;

	[Header("Friction")]
	public float axisFrictionForce = 10f;
	public float sideFrictionCoeff = 1.5f;

	[Header(" ")]
	public float currLength;
	public float lastLength;
	public float forceY;
	public float forceZ;
	public float forceX;

	public float torque;
	public float brake;
	public Vector3 hitVelocity;


	void Start() {
		InitSectors();
	}

	void InitSectors() {
		sectors += sectors % 2;
		sectorsPos = new Vector3[sectors + 1];
		float angleStep = (180.0f / sectors) * Mathf.Deg2Rad;

		for (int i = 0; i <= sectors; i++) {
			sectorsPos[i] = new Vector3(
				0,
				Mathf.Sin(angleStep * i) * (radius - smallRadius),
				Mathf.Cos(angleStep * i) * (radius - smallRadius)
			);
		}
	}

	void FixedUpdate() {
		var dt = Time.fixedDeltaTime;

		var up = transform.up;
		var dw = -up;
		var fw = transform.forward;
		var rt = transform.right;
		var origin = transform.position + up * length;

		lastLength = currLength;
		var dumpedLength = Mathf.Clamp(lastLength + Mathf.Lerp(0, length, dt / damperTime), 0, length);

		var hitDistance = Mathf.Infinity;
		var hitPoint = Vector3.zero;
		var hitNormal = Vector3.zero;

		for (int i = 0; i < sectors; i++) {
			var a = origin + fw * sectorsPos[i + 0].z + dw * sectorsPos[i + 0].y;
			var b = origin + fw * sectorsPos[i + 1].z + dw * sectorsPos[i + 1].y;

			if (Physics.CapsuleCast(a, b, smallRadius, dw, out RaycastHit hit, dumpedLength, layerMask)) {
				if (hit.distance < hitDistance) {
					hitDistance = hit.distance;
					hitPoint = hit.point;
					hitNormal = hit.normal;
				}
			}
		}

		if (hitDistance != Mathf.Infinity) {
			currLength = Mathf.Clamp(hitDistance, 0f, length);
		}
		else {
			currLength = dumpedLength;
		}

		forceY = 0;
		forceX = 0;
		forceZ = 0;

		if (hitDistance != Mathf.Infinity) {
			//Suspension
			forceY += spring * (length - currLength);
			forceY += damper * (lastLength - currLength) / dt;
			//forceY *= Vector3.Dot(up, hitNormalRes);
			//forceZ *= Vector3.Dot(fw, hitNormalRes);
			rb.AddForceAtPosition(hitNormal * (forceY * Vector3.Dot(up, hitNormal)), hitPoint, ForceMode.Force);

			//Grip
			hitVelocity = transform.InverseTransformDirection(rb.GetPointVelocity(hitPoint));
			forceX = -Mathf.Clamp(hitVelocity.x * forceY, -forceY, forceY) * sideFrictionCoeff;
			forceZ = -Mathf.Clamp(hitVelocity.z, -1f, 1f) * (axisFrictionForce + brake);
			forceZ += torque;
			rb.AddForceAtPosition(fw * forceZ + rt * forceX, hitPoint, ForceMode.Force);
		}

		//Visual rotation
		if (view != null) {
			view.localRotation *= Quaternion.Euler((hitVelocity.z / radius) * (180f / Mathf.PI) * dt, 0f, 0f);
		}
	}

	void Update() {
		if (bumper != null) {
			bumper.localPosition = Vector3.up * (length - currLength);
		}
		if (view != null) {
			view.localPosition = Vector3.up * (length - currLength);
		}
	}

	private void OnDrawGizmosSelected() {
		var up = transform.up;
		var dw = -up;
		var fw = transform.forward * radius;
		var origin = transform.position + up * length;

		var suspLength = lastLength == 0f ? length : currLength;
		var axis = origin + dw * suspLength;
		var end = origin + dw * length;

		Gizmos.color = Color.red;
		Gizmos.DrawLine(origin, axis);
		Gizmos.DrawLine(origin - fw * 0.1f, origin + fw * 0.1f);
		Gizmos.DrawLine(axis - fw, axis + fw);
		Gizmos.DrawWireSphere(axis + dw * (radius - smallRadius), smallRadius);

		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(axis, end);
		Gizmos.DrawLine(end - fw * 0.1f, end + fw * 0.1f);
	}
}
