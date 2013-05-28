using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using AssemblyCSharp;
using UnityEngine;

public class OptitrackStream : MonoBehaviour
{
	private static int count = 4;
	public OVRCameraController Child;
	public Vector3 posScale = new Vector3 (100f, 100f, 100f);
	public Int32 trackingPort = 1511;
	UdpClient udpClient = new UdpClient ();
	IPEndPoint remoteIPEndPoint;
	
	GameObject rb;
 
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
		public uint rb;
		public Vector3 Pos;
		public UnityEngine.Quaternion Rot;
		public uint mCount;
		public float errors;
	}
	
	
	// Use this for initialization
	void Start ()
	{
		rb = new GameObject ("RigidBody");
		udpClient.ExclusiveAddressUse = false;
		remoteIPEndPoint = new IPEndPoint (IPAddress.Any, trackingPort);

		udpClient.Client.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
		udpClient.ExclusiveAddressUse = false;

		udpClient.Client.Bind (remoteIPEndPoint);

		IPAddress multicastAddr = IPAddress.Parse ("239.255.42.99");
		udpClient.JoinMulticastGroup (multicastAddr);
		
		udpClient.Client.ReceiveBufferSize = 128;
	}

	// Update is called once per frame
	void Update ()
	{
		byte[] data = udpClient.Receive (ref remoteIPEndPoint);
		IntPtr dataPtr = GCHandle.Alloc (data, GCHandleType.Pinned).AddrOfPinnedObject ();
		uint count1 = ((PacketHeader)Marshal.PtrToStructure (dataPtr, typeof(PacketHeader))).unknownMarker;
			
		dataPtr = new IntPtr (dataPtr.ToInt64 () + 20);
		
		for (uint i =  0; i < count; i++) {
			RigidBody body = (RigidBody)Marshal.PtrToStructure (dataPtr, typeof(RigidBody));
			dataPtr = new IntPtr (dataPtr.ToInt64 () + 40);
				
			if (body.rb == 1) {
				body.Pos *= 3;
				Child.transform.localPosition = new Vector3(body.Pos.x, body.Pos.y, -body.Pos.z);
				Child.transform.localRotation = body.Rot;
				Child.transform.localScale = posScale;
			}
		}
	}
}
