using UnityEngine;
using UnityEngine.VR;
using System.Collections;

public class TriggerRight : MonoBehaviour {

	private bool trigger;
	public int num = 0;

	public bool GetTrigger {
		get { return this.trigger; }
	}

	public int GetTriggerNum {
		get { return this.num; }
	}

	public ColliderTest hap;
	bool h;

	void Update () {
		SteamVR_TrackedObject trackedObject = GetComponent<SteamVR_TrackedObject> ();
		var device = SteamVR_Controller.Input ((int)trackedObject.index);

		if (device.GetPressDown (SteamVR_Controller.ButtonMask.Trigger)) {
			Debug.Log ("トリガーを深く引いた");
			trigger = true;
			num += 1;
		}
		//Debug.Log (num);

		bool h = hap.GetHapticTrigger;
			
		
	}
}
