
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

public class CarSeatSettingsUI : UdonSharpBehaviour {
	public Transform seatPosition;

	public Slider heightSlider;
	public float heightMin = -0.5f;
	public float heightMax = 0.5f;

	public Slider distanceSlider;
	public float distanceMin = -0.15f;
	public float distanceMax = 0.15f;
	
	public Slider angleSlider;
	public float angleMin = -10.0f;
	public float angleMax = 10.0f;

	private float height;
	private float distance;
	private float angle;


	void Start() {
		heightSlider.minValue = heightMin;
		heightSlider.maxValue = heightMax;
		heightSlider.value = 0.5f * (heightMin + heightMax);
		height = heightSlider.value;

		distanceSlider.minValue = distanceMin;
		distanceSlider.maxValue = distanceMax;
		distanceSlider.value = 0.5f * (distanceMin + distanceMax);
		distance = distanceSlider.value;

		angleSlider.minValue = angleMin;
		angleSlider.maxValue = angleMax;
		angleSlider.value = 0.5f * (angleMin + angleMax);
		angle = angleSlider.value;

		Apply();
	}

	public void ChangeSeatHeight() {
		height = heightSlider.value;
		Apply();
	}

	public void ChangeSeatDistance() {
		distance = distanceSlider.value;
		Apply();
	}
	
	public void ChangeSeatAngle() {
		angle = angleSlider.value;
		Apply();
	}

	private void Apply() {
		seatPosition.localPosition = new Vector3(seatPosition.localPosition.x, height, distance);

		var euler = seatPosition.localRotation.eulerAngles;
		seatPosition.localRotation = Quaternion.Euler(angle, euler.y, euler.z);
	}
}
