using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CubeScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		this.GetComponent<Rigidbody>().isKinematic = true;
		StartCoroutine(fall());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator fall()
	{
		yield return new WaitForSeconds(60f);
		turnOnGravity();
	}

	public void turnOnGravity()
	{
		this.GetComponent<Rigidbody>().isKinematic = false;

	}
}
