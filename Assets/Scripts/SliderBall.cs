using UnityEngine;
using System.Collections;

/// <summary>
/// The behavior of the slider ball of a slider
/// </summary>
public class SliderBall : MonoBehaviour, IUIControl
{	
    /// <summary>Is the slider ball moving?</summary>
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
