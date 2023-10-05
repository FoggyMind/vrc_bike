
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
[RequireComponent(typeof(VRC_Pickup))]
public class HandlePickup : UdonSharpBehaviour {
	[HideInInspector] public VRC_Pickup pickup;
	[HideInInspector] public bool picked = false;
	[HideInInspector] public bool use = false;
	[HideInInspector] public bool jump = false;

	public float grabDistance = 0.05f;

	private HandType hand = HandType.RIGHT;

	private Vector3 resetPos;
	private Quaternion resetRot;

	void Start() {
		pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));

		resetPos = transform.localPosition;
		resetRot = transform.localRotation;

		ResetState();
	}

	private void ResetState() {
		picked = false;
		use = false;
		jump = false;
	}

	private void ResetTransform() {
		transform.localPosition = resetPos;
		transform.localRotation = resetRot;
	}

	public override void OnPickup() {
		ResetState();

		if (pickup.currentPlayer != Networking.LocalPlayer) {
			return;
		}

		picked = true;
		hand = pickup.currentHand == VRC_Pickup.PickupHand.Left ? HandType.LEFT : HandType.RIGHT;

		var collider = GetComponent<Collider>();
		if (collider != null) {
			collider.enabled = false;
		}
	}

	public override void OnDrop() {
		ResetState();
		ResetTransform();

		var collider = GetComponent<Collider>();
		if (collider != null) {
			collider.enabled = true;
		}
	}

	public void ProcessPosition() {
		if (picked) {
			if (pickup.currentHand == VRC_Pickup.PickupHand.Left) {
				transform.position = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
			}
			else {
				transform.position = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
			}
		}
		else {
			ResetTransform();

			if (grabDistance >= Vector3.Distance(Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position, transform.position)) {
				pickup.pickupable = true;
			}
			else if (grabDistance >= Vector3.Distance(Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position, transform.position)) {
				pickup.pickupable = true;
			}
			else {
				pickup.pickupable = false;
			}
		}
	}

	public override void InputUse(bool value, VRC.Udon.Common.UdonInputEventArgs args) {
		if (picked && hand == args.handType) {
			use = value;
		}
	}

	public override void InputJump(bool value, VRC.Udon.Common.UdonInputEventArgs args) {
		if (picked && hand == args.handType) {
			jump = value;
		}
	}
}
