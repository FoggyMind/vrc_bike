using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class EventButton : UdonSharpBehaviour {
	public UdonSharpBehaviour target;
	public UdonSharpBehaviour[] targets;
	public string eventName;
	public bool sendAll = false;
	public bool sendOwner = false;

	public override void Interact() {
		if (eventName != "") {
			if (target != null) {
				if (sendAll) {
					target.SendCustomNetworkEvent(NetworkEventTarget.All, eventName);
				}
				else if (sendOwner) {
					target.SendCustomNetworkEvent(NetworkEventTarget.Owner, eventName);
				}
				else {
					target.SendCustomEvent(eventName);
				}
			}

			foreach (var it in targets) {
				if (sendAll) {
					it.SendCustomNetworkEvent(NetworkEventTarget.All, eventName);
				}
				else if (sendOwner) {
					it.SendCustomNetworkEvent(NetworkEventTarget.Owner, eventName);
				}
				else {
					it.SendCustomEvent(eventName);
				}
			}
		}
	}
}
