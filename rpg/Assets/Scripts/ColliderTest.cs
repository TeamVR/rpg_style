using UnityEngine;
using Exploder.Utils;
using System.Collections;

public class ColliderTest : MonoBehaviour {

	public float lifeTime = 3.5f;
	float time = 0f;

	public bool hapticFlg = false;

	public bool GetHapticTrigger {
		get { return this.hapticFlg; }
	}

	// Use this for initialization
	void Start () {
		time = 0;
	}
	
	// Update is called once per frame
	void Update () {

		time += Time.deltaTime;
	
	}

	void OnCollisionEnter(Collision col) {
        ExploderSingleton.ExploderInstance.ExplodeObject(gameObject);
        hapticFlg = true;
        //Destroy(this.gameObject, 2.0f);

	}
}
