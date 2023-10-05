
using UdonSharp;
using UnityEngine;

public class LookAt : UdonSharpBehaviour {
    [SerializeField] private Transform target;

    public void LateUpdate() {
        transform.LookAt(target);
    }
}
