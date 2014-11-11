using UnityEngine;
using System.Collections;

public class PlayerStuff : MonoBehaviour {

	// Use this for initialization
	void Start () {
		rigidbody.AddForce(new Vector3(120, 350, 80));
	}
	
	void FixedUpdate () {
		float MoveHorizontal = Input.GetAxis ("Horizontal");
		float MoveVertical = Input.GetAxis ("Vertical");
		
		Vector3 movement = new Vector3(MoveHorizontal, 0.0f, MoveVertical);
		
		rigidbody.AddForce(movement * 500 * Time.deltaTime);
	}
}
