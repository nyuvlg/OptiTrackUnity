using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

public class OptitrackStream : MonoBehaviour
{
	public static Int32 trackingPort = 1511;
	public Material pliersMat;
	public GameObject pliers;
	private AsyncPkt pkt;
	private static AsyncCallback result = new AsyncCallback (asyncMethod);
	private OVRCameraController child;
	private const float dt = 10.0f;
	private const uint bSize = 100;
	private GameObject empty;
	private Color origCol;
	private const float look = 30;
	private float lookAhead = look;
	private Quaternion q2Open = Quaternion.AngleAxis (333.8647f, new Vector3 (0, 1, 0));
	private Vector3 vt2_5 = new Vector3 (-0.8855f, 0.36025f, -0.25f);
//	private	Vector3 vt3 = new Vector3 (-0.967f, 0.516f, -0.555f);
//	private	Vector3 vt2 = new Vector3 (-0.804f, 0.2245f, 0.045f);
//	private	Vector3 vt1 = new Vector3 (-0.645f, -0.065f, 0.624f);
	
	class AsyncPkt
	{
		public uint cFrames;
		public uint n;
		public byte[][] circBuffer;
		public bool stopReceive;
		public UdpClient udpClient;
		public IPEndPoint remoteIPEndPoint;

		public AsyncPkt (uint cf, uint n, bool sr)
		{
			this.cFrames = cf;
			this.n = n;
			this.stopReceive = sr;
			udpClient = new UdpClient ();
			remoteIPEndPoint = new IPEndPoint (IPAddress.Any, trackingPort);
			circBuffer = new byte[bSize][];
		}
	}
	
	[StructLayout(LayoutKind.Sequential)]
	struct PacketHeader
	{
		public ushort ID;
		public ushort bytes;
		public uint frame;
		public uint markerSetCount;
		public uint unknownMarker;
		public uint rigidBodys;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct RigidBody
	{
		public uint id;
		public Vector3 pos;
		public Quaternion Rot;
		public uint mCount;
		public float errors;
	}
	
	// Use this for initialization
	void Start ()
	{
		child = GetComponent<OVRCameraController> ();
		empty = new GameObject ("Empty");
		if (pliers == null) {
			pliers = GameObject.CreatePrimitive (PrimitiveType.Cube);
			pliers.transform.localScale = new Vector3 (0.135491f, 0.005082734f, 0.1361813f);
			pliers.AddComponent ("SphCollision");
			pliers.collider.isTrigger = false;
		} else {
			pliers.transform.localPosition = new Vector3 ();
			pliers.AddComponent<MeshRenderer> ();
		}
		pliers.transform.parent = empty.transform;
		pliers.name = "pliers";
		pliers.AddComponent<Rigidbody> ();
		pliers.rigidbody.useGravity = false;
		pliers.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
		if (pliersMat != null) {
			pliers.renderer.material = pliersMat;
		}
		
		pkt = new AsyncPkt (0, 0, false);
		UdpClient udpClient = pkt.udpClient;
		IPEndPoint remoteIPEndPoint = pkt.remoteIPEndPoint;
		
		udpClient.Client.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
		udpClient.ExclusiveAddressUse = false;

		udpClient.Client.Bind (remoteIPEndPoint);

		IPAddress multicastAddr = IPAddress.Parse ("239.255.42.99");
		udpClient.JoinMulticastGroup (multicastAddr);
		udpClient.Client.ReceiveBufferSize = 512;
		
		udpClient.BeginReceive (result, pkt);
	}
	
	private static void asyncMethod (IAsyncResult res)
	{	
		AsyncPkt pkt = (AsyncPkt)(res.AsyncState);
		pkt.cFrames++;
		
		pkt.circBuffer [pkt.n] = pkt.udpClient.EndReceive (res, ref pkt.remoteIPEndPoint);
		if (!pkt.stopReceive) {
			pkt.udpClient.BeginReceive (result, pkt);
		}
		pkt.n = (pkt.n + 1) % bSize;
	}

	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.B)) {
			lookAhead = look - lookAhead;
			Debug.Log ("The look ahead is " + lookAhead + " ms");
		}
		if (default(byte[]) == pkt.circBuffer [(pkt.n - 1 + bSize) % bSize] || default(byte[]) == pkt.circBuffer [(pkt.n - 2 + bSize) % bSize])
			return;
		
		byte[] curr = pkt.circBuffer [(pkt.n - 1 + bSize) % bSize];
		
		IntPtr dataPtr = GCHandle.Alloc (curr, GCHandleType.Pinned).AddrOfPinnedObject ();
		
		uint count = ((PacketHeader)Marshal.PtrToStructure (dataPtr, typeof(PacketHeader))).rigidBodys;
		dataPtr = new IntPtr (dataPtr.ToInt64 () + 20);
		
		RigidBody body;
		Vector3 pos;
		uint offset = 20;
		while (count-- > 0) {
			body = (RigidBody)Marshal.PtrToStructure (dataPtr, typeof(RigidBody));
			pos = xpolate (offset);
			offset += 120;
			dataPtr = new IntPtr (dataPtr.ToInt64 () + 120);
			if (body.id == 1 && child != null) {
				child.transform.localPosition = new Vector3 (pos.x, pos.y, -pos.z);
				child.transform.localRotation = body.Rot;
			} else if (body.id == 2) {
				pos *= 2.5f;
				empty.transform.localPosition = new Vector3 (pos.x, pos.y, -pos.z) + vt2_5;
				empty.transform.localRotation = body.Rot;
				float dot = Vector3.Dot (empty.transform.up, Vector3.right);
				empty.transform.localRotation = Quaternion.identity;
				if (Math.Abs (dot) > 0.01f) {
					Collider[] cls = pliers.GetComponentsInChildren<Collider> ();
					if (dot > 0) {
						foreach (var cl in cls)
							cl.isTrigger = true;
						pliers.transform.GetChild (2).localRotation = Quaternion.AngleAxis (350, Vector3.up);
					} else {
						pliers.transform.GetChild (2).localRotation = q2Open;
						foreach (var cl in cls) {
							cl.isTrigger = false;
						}
						if (empty.transform.childCount > 1) {
							for (int i = 0; i < empty.transform.childCount; i++) {
								empty.transform.GetChild (i).renderer.material.shader = Shader.Find ("Diffuse");
							}
							empty.transform.DetachChildren ();
							pliers.transform.parent = empty.transform;
						}
					}
				}
			}
		}
	}

	Vector3 xpolate (uint offset)
	{
		byte[] t_1 = pkt.circBuffer [(pkt.n - 1 + bSize) % bSize];
		byte[] t_2 = pkt.circBuffer [(pkt.n - 2 + bSize) % bSize];
		
		IntPtr dataPtr1 = GCHandle.Alloc (t_1, GCHandleType.Pinned).AddrOfPinnedObject ();
		dataPtr1 = new IntPtr (dataPtr1.ToInt64 () + offset);
		Vector3 pos_1 = ((RigidBody)Marshal.PtrToStructure (dataPtr1, typeof(RigidBody))).pos;
		
		IntPtr dataPtr2 = GCHandle.Alloc (t_2, GCHandleType.Pinned).AddrOfPinnedObject ();
		dataPtr2 = new IntPtr (dataPtr2.ToInt64 () + offset);
		Vector3 pos_2 = ((RigidBody)Marshal.PtrToStructure (dataPtr2, typeof(RigidBody))).pos;
		
		Vector3 velocity = (pos_1 - pos_2) / dt;
		return pos_1 + velocity * lookAhead;
	}
	
	void OnDestroy ()
	{
		pkt.stopReceive = true;
	}
}