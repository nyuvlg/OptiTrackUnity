using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AssemblyCSharp
{
	public class MySph
	{		
		public uint id;
		
		public Vector3 pos{ get; internal set; }

		public float r { get; internal set; }

		public GameObject GO { get; internal set; }
		
		public bool Mark { get; set; }
		
		public void CreatePrimitive ()
		{
			if (GO == null) {
				GO = GameObject.CreatePrimitive (PrimitiveType.Sphere);
				GO.name += " " + id;	
			}
			GO.transform.localPosition = pos;
		}
		
		public MySph (Vector3 pos, float r)
		{
			this.pos = pos;
			this.r = r;
		}
		
		public override bool Equals (object obj)
		{
			if (obj is MySph) {
				MySph other = obj as MySph;
				return this.pos.Eq (other.pos) && this.r.Eq (other.r);
			}
			return false;
		}
		
		public override int GetHashCode ()
		{
			return String.Format ("{0}_{1}_{2}_{3}", pos.x.GetHashCode (), pos.y.GetHashCode (), pos.z.GetHashCode (), r.GetHashCode ()).GetHashCode ();
		}
	}
	
	public class MySphEqComp : IEqualityComparer<MySph>
	{
		public bool Equals (MySph x, MySph y)
		{
			return x.Equals (y);
		}
		
		public int GetHashCode (MySph obj)
		{
			return obj.GetHashCode ();
		}
	}
}

