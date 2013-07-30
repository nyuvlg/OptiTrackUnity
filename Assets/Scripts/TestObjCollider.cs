using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestObjCollider : MonoBehaviour
{
	public static Dictionary<string, string> TestObjShaders = new Dictionary<string, string> ();

	public void OnTriggerEnter (Collider other)
	{
		switch (other.tag) {
		case "TestObj":
			if (other.transform.parent != null)
				return;
			other.transform.parent = this.transform.parent; 
			if (other.rigidbody != null)
				other.rigidbody.useGravity = false;
			TestObjShaders [other.name] = other.renderer.material.shader.name;
			other.renderer.material.shader = Shader.Find ("Transparent/Diffuse");
			break;
		}
	}
}