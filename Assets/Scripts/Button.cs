using UnityEngine;
using System.Collections;

/// <summary> Every Button class must derive from this base abstract class</summary>
public abstract class Button : MonoBehaviour, IUIControl
{
    Color orig ; // The original color of the button, in the defaults state.
	bool iOn = false; // Is the button switched on?

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