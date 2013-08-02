using UnityEngine;
using System.Collections;
using System.Xml;

/// <summary>The script attached to any button, which loads the data from a file.</summary>
public class LoadButton : Button
{
	public override void OnTriggerEnter (Collider other)
	{
		base.OnTriggerEnter (other);
		if (other.tag == "RingMarker") {
			Vector3 pos = new Vector3 (), sc = new Vector3 ();
			Quaternion rot = new Quaternion ();
			using (XmlReader reader = XmlReader.Create ("MyData.xml", null)) {
				Debug.Log ("Loading data from MyData.xml ... ");
				GameObject[] gos = GameObject.FindGameObjectsWithTag ("TestObj");
				int i = 0;
				reader.ReadStartElement ();
				while (reader.IsStartElement() == true) {
					reader.ReadStartElement ();
				
					reader.ReadStartElement ("Position");
					pos.x = reader.ReadElementContentAsFloat ();
					pos.y = reader.ReadElementContentAsFloat ();
					pos.z = reader.ReadElementContentAsFloat ();
					reader.ReadEndElement ();
			
					reader.ReadStartElement ("Rotation");
					rot.w = reader.ReadElementContentAsFloat ();
					rot.x = reader.ReadElementContentAsFloat ();
					rot.y = reader.ReadElementContentAsFloat ();
					rot.z = reader.ReadElementContentAsFloat ();
					reader.ReadEndElement ();
			
					reader.ReadStartElement ("Scale");
					sc.x = reader.ReadElementContentAsFloat ();
					sc.y = reader.ReadElementContentAsFloat ();
					sc.z = reader.ReadElementContentAsFloat ();
					reader.ReadEndElement ();
			
					reader.ReadEndElement ();
				
					gos [i].transform.localPosition = pos;
					gos [i].transform.localRotation = rot;
					gos [i].transform.localScale = sc;
					i++;
				}
				reader.ReadEndElement ();
				Debug.Log ("Completed loading.");
			}
		}
	}
}