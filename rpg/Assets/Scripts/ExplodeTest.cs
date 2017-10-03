using UnityEngine;
using Exploder.Utils;
using System.Collections;

public class ExplodeTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		ExploderSingleton.ExploderInstance.ExplodeObject (gameObject);
	}
}
