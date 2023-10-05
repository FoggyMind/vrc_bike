
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
[RequireComponent(typeof(VRCStation))]
public class CarSeat : UdonSharpBehaviour {
	public UdonSharpBehaviour carSystem;
	private VRCStation station;
	private Collider stationCollider;

	[HideInInspector]
	public bool seated = false;

	void Start() {
		seated = false;
		station = (VRCStation)GetComponent(typeof(VRCStation));
		stationCollider = GetComponent<Collider>();
	}

	public override void Interact() {
		station.UseStation(Networking.LocalPlayer);
	}

	public void ExitStation() {
		station.ExitStation(Networking.LocalPlayer);
	}

	public override void OnStationEntered(VRC.SDKBase.VRCPlayerApi player) {
		if (player != Networking.LocalPlayer) {
			return;
		}

		Networking.SetOwner(Networking.LocalPlayer, gameObject);

		seated = true;
		stationCollider.enabled = false;
		
		carSystem.SendCustomEvent("OnSeat");
	}

	public override void OnStationExited(VRC.SDKBase.VRCPlayerApi player) {
		if (player != Networking.LocalPlayer) {
			return;
		}

		seated = false;
		stationCollider.enabled = true;
		carSystem.SendCustomEvent("OnLeave");
	}
}
