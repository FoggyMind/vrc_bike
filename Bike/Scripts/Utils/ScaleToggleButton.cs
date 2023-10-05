
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ScaleToggleButton : UdonSharpBehaviour {

	public GameObject target;

	public override void Interact() {
		Toggle();
	}

	public void Toggle() {
		if (target) {
			var scale = target.transform.localScale.x == 0.0f ? 1.0f : 0.0f;
			target.transform.localScale = new Vector3(scale, scale, scale);
		}
	}

	public void ToggleTrue() {
		if (target) {
			target.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		}
	}

	public void ToggleFalse() {
		if (target) {
			target.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
		}
	}
}
