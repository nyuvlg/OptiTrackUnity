using UnityEngine;
using System.Collections;

public class SliderBall : MonoBehaviour, IUIControl
{	
	public bool IsMoving{ get; private set; }
	
	public void OnPliersOpen ()
	{
		IsMoving = false;
	}
	
	void OnTriggerEnter (Collider other)
	{ 
		
		if (other.tag == "RingMarker") {
			IsMoving = true;
		}
	}
}
