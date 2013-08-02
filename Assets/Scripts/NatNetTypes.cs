using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AssemblyCSharp
{
    /// <summary>
    /// The structure of each packet, sent from Tracking Tools.
    /// </summary>
	[StructLayout (LayoutKind.Sequential)]
    struct NatNetPkt
	{
        public ushort ID;
		public ushort nBytes;
		public uint frame;
		public uint nMarkerSet;
		public MarkerSet[] markerSets;
		public uint nOtherMarkers;
		public Marker[] otherMarkers;
		public uint nRigidBodies;
		public RigidBody[] rigidBodies;
		public uint nSkeletons;
		public float latency;
		public uint timeCode;
		public Marker[] ringMarkers; // The markers, which identify the pliers.
		
		public void InitMarkerSets ()
		{
			markerSets = new MarkerSet[nMarkerSet];
		}

		public void InitOtherMarkers ()
		{
			otherMarkers = new Marker[nOtherMarkers];
		}

		public void InitRigidBodies ()
		{
			rigidBodies = new RigidBody[nRigidBodies];
		}
	}

    /// <summary>The structure of each marker, sent from Tracking Tools.</summary>
	[StructLayout (LayoutKind.Sequential)]
    struct Marker
	{
		public Vec3 pos;
		
		public float SqDistTo (Marker other)
		{
			return pos.SqDistTo (other.pos);
		}

		public float DistTo (Marker other)
		{
			return pos.DistTo (other.pos);
		}

		public override bool Equals (object obj)
		{
			if (!(obj is Marker))
				return false;
			return pos.Equals (((Marker)obj).pos);
		}

		public override int GetHashCode ()
		{
			return pos.GetHashCode ();
		}

		public override string ToString ()
		{
			return pos.ToString ();
		}
	}

    /// <summary>The structure of each marker set, sent from Tracking Tools.</summary>
	struct MarkerSet
	{
		public string name;
		public uint nMarkers;
		public Marker[] markers;

		public void InitMarkers ()
		{
			markers = new Marker[nMarkers];
		}
	}

    /// <summary>The structure of each rigid body, sent from Tracking Tools.</summary>
	[StructLayout (LayoutKind.Sequential)]
    struct RigidBody
	{
		public int ID;
		public Vec3 pos;
		public Quaternion rot;
		public uint nMarkers;
		public Marker[] Markers;
		public int[] MarkerIDs;
		public float[] MarkerSizes;
		public float MeanError;

		public void InitArrays ()
		{
			Markers = new Marker[nMarkers];
			MarkerIDs = new int[nMarkers];
			MarkerSizes = new float[nMarkers];
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	struct Vec3
	{
		public float x, y, z;
		static Vec3 temp;
		
		public float SqDistTo (Vec3 b)
		{
			return (x - b.x) * (x - b.x) + (y - b.y) * (y - b.y) + (z - b.z) * (z - b.x);
		}
		
		public float DistTo (Vec3 b)
		{
			return (float)Math.Sqrt (SqDistTo (b));
		}
		
		public double Magnitude {
			get{ return Math.Sqrt (x * x + y * y + z * z);}
		}
		
		public Vector3 AsVector3 {
			get{ return new Vector3 (x, y, z);}
		}
		
		public void Set (float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;	
		}
		
		public static Vec3 fromVector3 (Vector3 vec)
		{
			temp.Set (vec.x, vec.y, vec.z);
			return temp;
		}
		
		public static Vec3 operator + (Vec3 a, Vec3 b)
		{
			temp.Set (a.x + b.x, a.y + b.y, a.z + b.z);
			return temp;
		}

		public static Vec3 operator - (Vec3 a, Vec3 b)
		{
			temp.Set (a.x - b.x, a.y - b.y, a.z - b.z);
			return temp;
		}
		
		public static Vec3 operator * (Vec3 vec, float div)
		{
			temp.Set (vec.x * div, vec.y * div, vec.z * div);
			return temp;
		}
		
		public static Vec3 operator / (Vec3 vec, float div)
		{
			temp.Set (vec.x / div, vec.y / div, vec.z / div);
			return temp;
		}
		
		public override bool Equals (object obj)
		{
			if (!(obj is Vec3))
				return false;
			Vec3 other = (Vec3)obj;
			return x.Eq (other.x) && y.Eq (other.y) && z.Eq (other.z);
		}

		public override int GetHashCode ()
		{
			return String.Format ("{0}_{1}_{2}", x.GetHashCode (), y.GetHashCode (), z.GetHashCode ()).GetHashCode ();
		}

		public override string ToString ()
		{
			return String.Format ("({0}, {1}, {2})", x, y, z);
		}
		
		public Vec3 (float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;	
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	struct Quat
	{
		public float qx, qy, qz, qw;
		
		public Quaternion AsQuaternion {
			get{ return new Quaternion (qx, qy, qz, qw);	}
		}

		public override string ToString ()
		{
			return String.Format ("({0}, {1}, {2}, {3})", qx, qy, qz, qw);
		}
		
		public Quat (float qx, float qy, float qz, float qw)
		{
			this.qx = qx;
			this.qy = qy;
			this.qz = qz;
			this.qw = qw;
		}
	}

	static class Utils
	{
		static float tol = 1e-3f;

		public static bool Eq (this float a, float b)
		{
			return Math.Abs (a - b) < tol;
		}
	}
}

