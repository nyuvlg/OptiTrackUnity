using UnityEngine;
using System.Collections;

public abstract class Button : MonoBehaviour, IUIControl
{
	Color orig ;
	bool iOn = false;

	public virtual void OnTriggerEnter (Collider other)
	{
		orig = this.renderer.material.color;
		Color c = orig;
		c.g = 0.5f;
		this.renderer.material.color = c;
		iOn = true;
	}
	
	public virtual void OnTriggerExit (Collider other)
	{
		OnPliersOpen ();
		iOn = false;
	}
	
	public void OnPliersOpen ()
	{
		if (iOn == true)
			this.renderer.material.color = orig;
	}
}