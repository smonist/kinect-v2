using UnityEngine;
using System.Collections;

public class Ulrichs_Block : MonoBehaviour {
	public GameObject BodySourceView;
	private BodySourceView _BodyView;
	
	private Vector3 acceleration = new Vector3(0, 0, 0);
	
	// Use this for initialization
	void Start () {
		_BodyView = BodySourceView.GetComponent<BodySourceView>();
	}
	
	// Update is called once per frame
	void Update () {
		acceleration = _BodyView.pubAcceleration;
		
		
		if (acceleration.x > 0.1 | acceleration.y > 0.1 | acceleration.z > 0.1) {
			transform.localScale += new Vector3 (0.02F, 0, 0);
		}
		else {
			if (transform.lossyScale [0] > 0) {
				transform.localScale -= new Vector3 (0.01F, 0, 0);
			}
		}
	}
}