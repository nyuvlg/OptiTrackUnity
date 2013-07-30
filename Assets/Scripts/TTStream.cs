using UnityEngine;
using System.Collections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace AssemblyCSharp
{
	class TTStream : MonoBehaviour
	{
		public static int trackingPort = 1511;
		public Material RMMat = null;
		private GameObject RM1;
		private GameObject RM2;
		private static AsyncCallback result = new AsyncCallback (asyncMethod);
		private OVRCameraController ovrCamera;
		private const float dt = 10.0f;
		private const uint bSize = 100;
		private Color origCol;
		private const float lookTime = 15;
		private float lookAhead = lookTime;
		private Vector3 vt2_5 = new Vector3 (-0.8855f, 0.36025f, -0.25f);
		private AsyncPkt aPkt;
		private NatNetPkt curr;
		private NatNetPkt t1 = new NatNetPkt ();
		private NatNetPkt t2 = new NatNetPkt ();
		private List<Marker> others = new List<Marker> ();
		private GameObject mp;
		private GameObject empty;
		private Color orig;
		
		private class AsyncPkt
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
		
		private void Start ()
		{
			ovrCamera = GetComponent<OVRCameraController> ();
			RM1 = new GameObject ("RingMarker1");
			RM2 = new GameObject ("RingMarker2");
			
			mp = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			if (RMMat != null)
				mp.renderer.material = RMMat;
			empty = new GameObject ("Empty");
			Slider.SetTransform (empty.transform);
			mp.transform.parent = empty.transform;
			mp.name = "MidPtSph";
			mp.transform.localScale = new Vector3 (0.03f, 0.03f, 0.03f);
			mp.tag = "RingMarker";
			mp.AddComponent ("TestObjCollider");
			mp.AddComponent<Rigidbody> ();
			mp.collider.isTrigger = false;
			mp.rigidbody.useGravity = false;
			mp.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
			
			curr = new NatNetPkt ();
			aPkt = new AsyncPkt (0, 0, false);
			UdpClient udpClient = aPkt.udpClient;
			IPEndPoint remoteIPEndPoint = aPkt.remoteIPEndPoint;

			udpClient.Client.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			udpClient.ExclusiveAddressUse = false;

			udpClient.Client.Bind (remoteIPEndPoint);

			IPAddress multicastAddr = IPAddress.Parse ("239.255.42.99");
			udpClient.JoinMulticastGroup (multicastAddr);
			udpClient.Client.ReceiveBufferSize = 512;

			udpClient.BeginReceive (result, aPkt);
			orig = mp.renderer.material.color;
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

		private void Update ()
		{
			if (Input.GetKeyDown (KeyCode.B)) {
				lookAhead = lookTime - lookAhead;
				Debug.Log ("The look ahead is " + lookAhead + " ms");
			}
			if (default (byte[]) == aPkt.circBuffer [(aPkt.n - 1 + bSize) % bSize]
				|| default (byte[]) == aPkt.circBuffer [(aPkt.n - 2 + bSize) % bSize]
				|| default (byte[]) == aPkt.circBuffer [(aPkt.n - 3 + bSize) % bSize]
				)
				return;

			loadPacket (aPkt.circBuffer [(aPkt.n - 1 + bSize) % bSize], ref curr);
			loadPacket (aPkt.circBuffer [(aPkt.n - 2 + bSize) % bSize], ref t1);
			loadPacket (aPkt.circBuffer [(aPkt.n - 3 + bSize) % bSize], ref t2);
			
			xpolate (ref curr.rigidBodies [0].pos, t1.rigidBodies [0].pos, t2.rigidBodies [0].pos);
			curr.rigidBodies [0].pos.z = -curr.rigidBodies [0].pos.z;
			
			ovrCamera.transform.localPosition = curr.rigidBodies [0].pos.AsVector3;
			ovrCamera.transform.rotation = curr.rigidBodies [0].rot;
			
			if (curr.ringMarkers.Length == 1) {
				if (t1.ringMarkers.Length > 0 && t2.ringMarkers.Length > 0)
					xpolate (ref curr.ringMarkers [0].pos, t1.ringMarkers [0].pos, t2.ringMarkers [0].pos);
				curr.ringMarkers [0].pos.z = - curr.ringMarkers [0].pos.z;
				RM1.transform.localPosition = RM2.transform.localPosition = curr.ringMarkers [0].pos.AsVector3 * 2.5f + vt2_5;
			} else if (curr.ringMarkers.Length > 1) {
				if (t1.ringMarkers.Length > 0 && t2.ringMarkers.Length > 0)
					xpolate (ref curr.ringMarkers [0].pos, t1.ringMarkers [0].pos, t2.ringMarkers [0].pos);
				curr.ringMarkers [0].pos.z = - curr.ringMarkers [0].pos.z;
				RM1.transform.localPosition = curr.ringMarkers [0].pos.AsVector3 * 2.5f + vt2_5;
				
				if (t1.ringMarkers.Length > 1 && t2.ringMarkers.Length > 1)
					xpolate (ref curr.ringMarkers [1].pos, t1.ringMarkers [1].pos, t2.ringMarkers [1].pos);
				curr.ringMarkers [1].pos.z = - curr.ringMarkers [1].pos.z;
				RM2.transform.localPosition = curr.ringMarkers [1].pos.AsVector3 * 2.5f + vt2_5;
			}
			empty.transform.localPosition = RM1.transform.localPosition / 2 + RM2.transform.localPosition / 2;
			
			float dist = Vector3.Distance (RM1.transform.localPosition, RM2.transform.localPosition);
			if (dist < 0.097f) {
				Color c = mp.renderer.material.color;
				c.b = 0.5f;
				mp.renderer.material.color = c;
				mp.collider.isTrigger = true;
			} else {
				mp.renderer.material.color = orig;
				mp.collider.isTrigger = false;
				GameObject.FindGameObjectsWithTag ("UIControl").ToList ().ForEach (x => (x.GetComponent ("IUIControl") as IUIControl).OnPliersOpen ());
				if (empty.transform.childCount > 1) {
					for (int i = 1; i < empty.transform.childCount; i++) {
						Transform t = empty.transform.GetChild (i);
						t.renderer.material.shader = Shader.Find (TestObjCollider.TestObjShaders [t.name]);
						if (t.rigidbody != null)
							t.rigidbody.useGravity = true;
					}
					empty.transform.DetachChildren ();
					mp.transform.parent = empty.transform;
				}
			}
		}
		
		private void xpolate (ref Vec3 pos, Vec3 v1, Vec3 v2)
		{
			Vec3 vel = (v1 - v2) / dt;
			if (vel.Magnitude < 1e-3)
				vel.Set (0, 0, 0);
			pos = v1 + vel * lookAhead;
		}

		private void loadPacket (byte[] curr, ref NatNetPkt pkt)
		{
			IntPtr ptr = GCHandle.Alloc (curr, GCHandleType.Pinned).AddrOfPinnedObject ();

			readPtrToObj (ref ptr, ref pkt.ID);
			readPtrToObj (ref ptr, ref pkt.nBytes);
			readPtrToObj (ref ptr, ref pkt.frame);
			readPtrToObj (ref ptr, ref pkt.nMarkerSet);

			pkt.InitMarkerSets ();
			for (int i = 0; i < pkt.nMarkerSet; i++) {
				pkt.markerSets [i].name = Marshal.PtrToStringAnsi (ptr);
				ptr = new IntPtr (ptr.ToInt64 () + pkt.markerSets [i].name.Length + 1);

				readPtrToObj (ref ptr, ref pkt.markerSets [i].nMarkers);
				pkt.markerSets [i].InitMarkers ();
				for (int j = 0; j < pkt.markerSets[i].nMarkers; j++) {
					readPtrToObj (ref ptr, ref pkt.markerSets [i].markers [j]);
				}
			}

			readPtrToObj (ref ptr, ref pkt.nOtherMarkers);

			pkt.InitOtherMarkers ();
			for (int i = 0; i < pkt.nOtherMarkers; i++) {
				readPtrToObj (ref ptr, ref pkt.otherMarkers [i]);
			}

			readPtrToObj (ref ptr, ref pkt.nRigidBodies);

			pkt.InitRigidBodies ();
			for (int i = 0; i < pkt.nRigidBodies; i++) {
				readPtrToObj (ref ptr, ref pkt.rigidBodies [i].ID);
				readPtrToObj (ref ptr, ref pkt.rigidBodies [i].pos);
				readPtrToObj (ref ptr, ref pkt.rigidBodies [i].rot);
				readPtrToObj (ref ptr, ref pkt.rigidBodies [i].nMarkers);

				pkt.rigidBodies [i].InitArrays ();
				for (int j = 0; j < pkt.rigidBodies[i].nMarkers; j++) {
					readPtrToObj (ref ptr, ref pkt.rigidBodies [i].Markers [j]);
				}
				for (int j = 0; j < pkt.rigidBodies[i].nMarkers; j++) {
					readPtrToObj (ref ptr, ref pkt.rigidBodies [i].MarkerIDs [j]);
				}
				for (int j = 0; j < pkt.rigidBodies[i].nMarkers; j++) {
					readPtrToObj (ref ptr, ref pkt.rigidBodies [i].MarkerSizes [j]);
				}
				readPtrToObj (ref ptr, ref pkt.rigidBodies [i].MeanError);
			}
			readPtrToObj (ref ptr, ref pkt.nSkeletons);
			readPtrToObj (ref ptr, ref pkt.latency);
			readPtrToObj (ref ptr, ref pkt.timeCode);
			
			pkt.ringMarkers = new Marker[2];
			others.Clear ();
			foreach (Marker m in pkt.otherMarkers) {
				if (!pkt.rigidBodies [0].Markers.Contains (m))
					others.Add (m);
			}
			Vec3 pos = pkt.rigidBodies [0].pos;
			pkt.ringMarkers = others.OrderByDescending (x => x.pos.SqDistTo (pos)).Select ((m, idx) => new { m, idx }).Where (x => x.idx < 2).Select (x => x.m).ToArray ();
			
		}

		private void readPtrToObj<T> (ref IntPtr ptr, ref T obj)
		{
			obj = (T)Marshal.PtrToStructure (ptr, typeof(T));
			ptr = new IntPtr (ptr.ToInt64 () + Marshal.SizeOf (obj));
		}

		private void OnDestroy ()
		{
			aPkt.stopReceive = true;
		}
	}
}