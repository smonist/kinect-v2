using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class BodySourceView : MonoBehaviour 
{
	public GameObject BodySourceManager;
	public GUIText GUIRightHand;
	public GUIText GUIDebug;
	public GUIText GUIDebugTwo;
	public int frameBufferCount;
	
	private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
	private BodySourceManager _BodyManager;

	Queue<Vector3>[] jointCacheQueues;
	private int jointCount;

	private Vector3 localAcceleration;
	private Vector3 globalAcceleration;

	public Vector3 pubAcceleration;

	private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
	{
		{ Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
		{ Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
		{ Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
		{ Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
		
		{ Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
		{ Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
		{ Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
		{ Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
		
		{ Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
		{ Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
		{ Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
		{ Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
		{ Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
		{ Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
		
		{ Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
		{ Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
		{ Kinect.JointType.HandRight, Kinect.JointType.WristRight },
		{ Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
		{ Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
		{ Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
		
		{ Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
		{ Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
		{ Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
		{ Kinect.JointType.Neck, Kinect.JointType.Head },
	};

	void Start ()
	{
		jointCacheQueues = new Queue<Vector3>[25];

		for (int i = 0; i < 25; i++) {
			jointCacheQueues[i] = new Queue<Vector3>();

			for (int  u = 0; u < frameBufferCount; u++) {
				jointCacheQueues[i].Enqueue(Vector3.zero);
			}
		}

		globalAcceleration.Set(0, 0, 0);
		localAcceleration.Set(0, 0, 0);
		pubAcceleration.Set (0, 0, 0);
	}
	
	void Update () 
	{
		//reset acceleration
		globalAcceleration.Set(0, 0, 0);
		localAcceleration.Set(0, 0, 0);

		if (BodySourceManager == null)
		{
			return;
		}


		_BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
		if (_BodyManager == null)
		{
			return;
		}
		
		Kinect.Body[] data = _BodyManager.GetData();
		if (data == null)
		{
			return;
		}
		
		List<ulong> trackedIds = new List<ulong>();
		foreach(var body in data)
		{
			if (body == null)
			{
				continue;
			}
			
			if(body.IsTracked)
			{
				trackedIds.Add (body.TrackingId);
			}
		}
		
		List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
		
		// First delete untracked bodies
		foreach(ulong trackingId in knownIds)
		{
			if(!trackedIds.Contains(trackingId)) //if no body is tracked
			{
				Destroy(_Bodies[trackingId]);
				_Bodies.Remove(trackingId);

				GUIRightHand.text = "joint not tracked";
				GUIDebug.text = "-";
			}
		}
		
		foreach(var body in data)
		{
			if (body == null)
			{
				continue;
			}
			
			if(body.IsTracked)
			{
				if(!_Bodies.ContainsKey(body.TrackingId))
				{
					_Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
				}

				RefreshBodyObject(body, _Bodies[body.TrackingId]);
			}
		}
		/*if (globalAcceleration.x > 0.1 | globalAcceleration.y > 0.1 | globalAcceleration.z > 0.1) {
				GUIDebugTwo.text = (globalAcceleration * 10).ToString();
		}
		else {
			GUIDebugTwo.text = "-";
		}*/

		//set public acceleration
		//pubAcceleration = globalAcceleration;
	}
	
	private GameObject CreateBodyObject(ulong id)
	{
		GameObject body = new GameObject("Body:" + id);

		/*
		for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
		{
			GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
			LineRenderer lr = jointObj.AddComponent<LineRenderer>();
			lr.SetVertexCount(2);
			lr.material = BoneMaterial;
			lr.SetWidth(0.05f, 0.05f);
			
			jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
			jointObj.name = jt.ToString();
			jointObj.transform.parent = body.transform;
		}
		*/
		return body;
	}
	
	private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
	{
		jointCount = 0;
		/*for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
		{
			Kinect.Joint sourceJoint = body.Joints[jt];
			Kinect.Joint? targetJoint = null;
			
			if(_BoneMap.ContainsKey(jt))
			{
				targetJoint = body.Joints[_BoneMap[jt]];
			}

			if (jt == Kinect.JointType.HandRight) {
				localAcceleration = (GetVector3FromJoint(sourceJoint)) - jointCacheQueues[jointCount].Peek();
				jointCacheQueues[jointCount].Dequeue();
				jointCacheQueues[jointCount].Enqueue(localAcceleration);


				//GUIRightHand.text = GetVector3FromJoint(sourceJoint).ToString("#.00");
				//GUIRightHand.text = localAcceleration.ToString("#.00");
				//pubAcceleration = localAcceleration;


				if (!V3Equal(localAcceleration, Vector3.zero)) {
					globalAcceleration += localAcceleration;
				}
			}

			//Debug.Log(jointCount + ": " + localAcceleration);


			jointCount++;
		}*/
		//GUIDebug.text = globalAcceleration.ToString("#.00");


		foreach (Kinect.Joint sourceJoint in body.Joints.Values) {
			//convert joint to vector3
			Vector3 vectorSourceJoint = GetVector3FromJoint(sourceJoint);

			localAcceleration = vectorSourceJoint - jointCacheQueues[jointCount].Peek();
			if (!V3Equal(localAcceleration, Vector3.zero)) {
				globalAcceleration += V3Abs(localAcceleration);
			}





			if (jointCount == 10) {
				GUIDebugTwo.text = localAcceleration.ToString();
			}

			jointCacheQueues[jointCount].Dequeue();
			jointCacheQueues[jointCount].Enqueue(vectorSourceJoint);

			jointCount++;
		}

		//GUIDebugTwo.text = jointCacheQueues[0].Peek().ToString();
		GUIDebug.text = globalAcceleration.ToString();



	}

	private Vector3 GetVector3FromJoint(Kinect.Joint joint)
	{
		return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
	}

	private Vector3 V3Abs(Vector3 input) {
		input.Set(Mathf.Abs(input[0]), Mathf.Abs(input[1]), Mathf.Abs(input[2]));
		return input;
	}

	private bool V3Equal(Vector3 a, Vector3 b){
		return Vector3.SqrMagnitude(a - b) < 0.0001;
	}

	private int jointToNumber(Kinect.JointType inputJoint) {
		switch (inputJoint) {
			case Kinect.JointType.SpineBase:
				return 0;
			case Kinect.JointType.SpineMid:
				return 1;
			case Kinect.JointType.Neck:
				return 2;
			case Kinect.JointType.Head:
				return 3;
			case Kinect.JointType.ShoulderLeft:
				return 4;
			case Kinect.JointType.ElbowLeft:
				return 5;
			case Kinect.JointType.WristLeft:
				return 6;
			case Kinect.JointType.HandLeft:
				return 7;
			case Kinect.JointType.ShoulderRight:
				return 8;
			case Kinect.JointType.ElbowRight:
				return 9;
			case Kinect.JointType.WristRight:
				return 10;
			case Kinect.JointType.HandRight:
				return 11;
			case Kinect.JointType.HipLeft:
				return 12;
			case Kinect.JointType.KneeLeft:
				return 13;
			case Kinect.JointType.AnkleLeft:
				return 14;
			case Kinect.JointType.FootLeft:
				return 15;
			case Kinect.JointType.HipRight:
				return 16;
			case Kinect.JointType.KneeRight:
				return 17;
			case Kinect.JointType.AnkleRight:
				return 18;
			case Kinect.JointType.FootRight:
				return 19;
			case Kinect.JointType.SpineShoulder:
				return 20;
			case Kinect.JointType.HandTipLeft:
				return 21;
			case Kinect.JointType.ThumbLeft:
				return 22;
			case Kinect.JointType.HandTipRight:
				return 23;
			case Kinect.JointType.ThumbRight:
				return 24;
			default:
				return 100;
		}
	}
}