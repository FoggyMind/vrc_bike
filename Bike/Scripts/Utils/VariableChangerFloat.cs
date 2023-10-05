
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class VariableChangerFloat : UdonSharpBehaviour {
	public UdonBehaviour target;
	public string variable;
	public float deltaA = 0;
	public float deltaB = 0;
	public Text text;
	public string textFormat = "F1";

	private float value = 0;

	void Start() {
		GetValue();
	}

	private void OnEnable() {
		GetValue();
	}

	public void IncrementA() {
		value += deltaA;
		SetValue();
	}

	public void IncrementB() {
		value += deltaB;
		SetValue();
	}

	public void DecrementA() {
		value -= deltaA;
		SetValue();
	}

	public void DecrementB() {
		value -= deltaB;
		SetValue();
	}

	public void GetValue() {
		if (text) {
			text.text = string.Empty;
		}

		if (target) {
			var obj = target.GetProgramVariable(variable);
			if (obj != null) {
				value = (float)obj;
				SetText();
			}
		}
	}

	void SetValue() {
		if (target) {
			if (target.GetProgramVariable(variable) != null) {
				target.SetProgramVariable(variable, value);
				SetText();
			}
		}
	}

	void SetText() {
		if (text) {
			text.text = value.ToString(textFormat);
		}
	}
}
