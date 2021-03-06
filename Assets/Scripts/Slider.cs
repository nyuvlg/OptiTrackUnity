using UnityEngine;
using System.Collections;

/// <summary>The script to control the behavior of all Sliders</summary>
public class Slider : MonoBehaviour, IUIControl
{
    /// <summary>The minimum value on the slider.</summary>
	public float Min = 0;
    /// <summary>The minimum value on the slider.</summary>
    public float Max = 10;
    /// <summary>The minimum value on the slider.</summary>
    public float Curr = 5;

	public enum SType
	{
		Horizontal,
		Vertical,
		Z		
	};
	public SType SliderType = SType.Horizontal;
	private SliderBall ball; // The moving ball of the slider
	private float pValue; // The parametric value on the slider
	private GameObject handle; // The stationary handle of the slider
	private Transform ballXForm; // The transform of the ball
	private static Transform t = null; // The local, to hold the value of the parent transform of the sliderball.
	
	public static void SetTransform (Transform trans)
	{
		t = trans;
	}
	
	public void OnPliersOpen ()
	{
		ball.OnPliersOpen ();
	}
	
	void Start ()
	{
		handle = this.gameObject;
		ballXForm = handle.transform.GetChild (0).GetChild (0);
		ball = ballXForm.gameObject.GetComponent ("SliderBall") as SliderBall;
	}
	
	void Update ()
	{
		if (ball.IsMoving == true) {			
			ballXForm.position = t.localPosition;
			Curr = Min + Max * pValue / (2 * handle.transform.localScale.y);
		}
		clampBallPos ();
	}		
	
    /// <summary>Clamps the position of the ball to the limits of the slider.</summary>
	private void clampBallPos ()
	{
		Vector3 pos = ballXForm.localPosition;
		float clamp = handle.transform.localScale.y / 1.0f;
		switch (SliderType) {
		case SType.Horizontal:
			pValue = pos.x = Mathf.Clamp (pos.x, -clamp, clamp);
			pos.y = 0;
			pos.z = 0;
			break;
			
		case SType.Vertical:
			pos.x = 0;
			pValue = pos.y = Mathf.Clamp (pos.y, -clamp, clamp);
			pos.z = 0;
			break;			
			
		case SType.Z:
			pos.x = 0;
			pos.y = 0;
			pValue = pos.z = Mathf.Clamp (pos.z, -clamp, clamp);
			break;
			
		}
		ballXForm.localPosition = pos;
	}
}