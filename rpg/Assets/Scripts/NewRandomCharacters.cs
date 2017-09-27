/// <summary>
/// 
/// </summary>

using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Animator))]  

//Name of class must be name of file as well

public class NewRandomCharacters : MonoBehaviour {

	//発生するオブジェクトをInspectorから指定する用
	public GameObject spawnObject;

	void Start () {

		// コルーチンの開始
		StartCoroutine ("Spawn");

//		// コルーチンの終了
//		StopCoroutine("Spawn");
	}

	IEnumerator Spawn() {

		for(int i = 0; i < 10; i++) {
			//自分をつけたオブジェクトの位置に、発生するオブジェクトをインスタンス化して生成する
			Instantiate(spawnObject, transform.position, Quaternion.identity);
			yield break;
		}
	}
}
