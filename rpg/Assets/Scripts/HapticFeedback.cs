using UnityEngine;
using UnityEngine.VR;
using System.Collections;

public class HapticFeedback : MonoBehaviour {

	private bool trigger;

	public bool GetTrigger {
		get { return this.trigger; }
	}
	
	void Update () {
		SteamVR_TrackedObject trackedObject = GetComponent<SteamVR_TrackedObject> ();
		var device = SteamVR_Controller.Input ((int)trackedObject.index);

		if (device.GetPressDown (SteamVR_Controller.ButtonMask.Trigger)) {
			Debug.Log ("トリガーを深く引いた");
			trigger = true;
		}
		if (device.GetTouchUp (SteamVR_Controller.ButtonMask.Trigger)) {
			Debug.Log ("トリガーを離した");
			trigger = false;
		}
	}
}
