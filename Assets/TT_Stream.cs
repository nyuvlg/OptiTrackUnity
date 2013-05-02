using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using AssemblyCSharp;
using UnityEngine;

public class TT_Stream : MonoBehaviour
{
	public GameObject Parent;
	public Int32 trackingPort = 1511;
	UdpClient udpClient = new UdpClient ();
	IPEndPoint remoteIPEndPoint;
	List<MySph> sphs = new List<MySph> ();
	Vector3 sizeScale = new Vector3 (.4f, .4f, .4f);
	static float sc = 10f;
	Vector3 posScale = new Vector3 (sc, sc, sc);
 
	[StructLayout(LayoutKind.Sequential)]
	struct PacketHeader
	{
		public ushort ID;
		public ushort bytes;
		public uint frame;
		public uint markerSetCount;
		public uint unknownMarker;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct OtherMarker
	{
		public Vector3 pos;
	};
	
	
	// Use this for initialization
	void Start ()
	{
		udpClient.ExclusiveAddressUse = false;
		remoteIPEndPoint = new IPEndPoint (IPAddress.Any, trackingPort);

		udpClient.Client.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
		udpClient.ExclusiveAddressUse = false;

		udpClient.Client.Bind (remoteIPEndPoint);

		IPAddress multicastAddr = IPAddress.Parse ("239.255.42.99");
		udpClient.JoinMulticastGroup (multicastAddr);
		
		udpClient.Client.ReceiveBufferSize = 512;
		
		GameObject lgtGO = new GameObject ("Light 1");
		lgtGO.AddComponent<Light> ();
		lgtGO.light.type = LightType.Directional;
		lgtGO.transform.position = new Vector3 (-4f, 5f, -22f);
		lgtGO.transform.rotation = Quaternion.Euler (27f, 300f, -0f);
	}

	// Update is called once per frame
	void Update ()
	{
		byte[] data = udpClient.Receive (ref remoteIPEndPoint);
		if (data.Length > 0) {
			foreach (MySph sph in sphs)
				sph.Mark = false;
			IntPtr dataPtr = GCHandle.Alloc (data, GCHandleType.Pinned).AddrOfPinnedObject ();
			uint count = ((PacketHeader)Marshal.PtrToStructure (dataPtr, typeof(PacketHeader))).unknownMarker;
			
			dataPtr = new IntPtr (dataPtr.ToInt64 () + 16);			
			
			Vector3 pos = new Vector3 ();
			MySph sp = null;
			try {
				for (uint i =0; i < count; i++) {
					pos = (((OtherMarker)Marshal.PtrToStructure (dataPtr, typeof(OtherMarker))).pos);
					dataPtr = new IntPtr (dataPtr.ToInt64 () + (12));
							
					sp = sphs.FirstOrDefault (x => x.pos.Eq (pos));
					if (sp == null)
						sphs.Add (new MySph (pos, 1.0f){Mark =true, id = i});
					else {
						sp.Mark = true;
						sp.id = i;
					}
				}
			} catch (NullReferenceException) {
			}
			deleteSphs();
		}
	}
	
	void deleteSphs ()
	{
		for (int i = 0; i < sphs.Count; i++) {
			MySph sph = sphs [i];
			if (sph.Mark == true) {
				sph.CreatePrimitive ();
				if(Parent!= null)sph.GO.transform.parent = Parent.transform;
				sph.GO.renderer.material.color = Color.white;
				sph.GO.transform.localScale = sizeScale;
				
				Vector3 v = sph.GO.transform.localPosition;
				v.Scale (posScale);
				sph.GO.transform.localPosition = v;
			} else {
				GameObject.Destroy (sph.GO);
				sphs.RemoveAt (i++);
			}
		}
	}
}

static class Utils
{
	static float tol = 1e-4f;

	public static bool Eq (this float a, float b)
	{
		return Math.Abs (a - b) <= tol;
	}
	
	public static bool isValid (this float f)
	{
		return !float.IsNaN (f) && !float.IsInfinity (f);	
	}
	
	public static bool Eq (this Vector3 u, Vector3 v)
	{
		return u.x.Eq (v.x) && u.y.Eq (v.y) && u.z.Eq (v.z);	
	}
	
	public static bool isValid (this Vector3 v)
	{
		return v.x.isValid () && v.y.isValid () && v.z.isValid ();	
	}
}