using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>The script to control the virtual objects, which can be moved by the Pliers.</summary>
public class TestObjCollider : MonoBehaviour
{
    /// <summary>A static storage to hold the name of the shader of each TestObj, so that it can be restored after the movement is complete.</summary>
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