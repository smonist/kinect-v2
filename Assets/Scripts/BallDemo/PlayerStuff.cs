using UnityEngine;
using System.Collections;

public class PlayerStuff : MonoBehaviour {
	public GameObject BodySourceView;
	private BodySourceView _BodyView;

	private Vector3 acceleration = new Vector3(0, 0, 0);

	// Use this for initialization
	void Start () {
		_BodyView = BodySourceView.GetComponent<BodySourceView>();
		rigidbody.AddForce(new Vector3(120, 350, 80));
	}
	
	void FixedUpdate () {
		float MoveHorizontal = Input.GetAxis ("Horizontal");
		float MoveVertical = Input.GetAxis ("Vertical");
		
		Vector3 movement = new Vector3(MoveHorizontal, 0.0f, MoveVertical);
		rigidbody.AddForce(movement * 500 * Time.deltaTime);


		acceleration = _BodyView.pubAcceleration * 10;
		if (acceleration.x > 0.1 | acceleration.y > 0.1 | acceleration.z > 0.1) {
			rigidbody.AddForce(new Vector3(acceleration.x, acceleration.y, (acceleration.z * -1)));
		}
	}
}
