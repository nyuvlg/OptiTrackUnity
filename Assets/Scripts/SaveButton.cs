using UnityEngine;
using System.Collections;
using System.Xml;

public class SaveButton : Button
{
	public override void OnTriggerEnter (Collider other)
	{
		base.OnTriggerEnter (other);
		if (other.tag == "RingMarker") {
			using (XmlWriter writer = XmlWriter.Create ("MyData.xml")) {
				Debug.Log ("Saving data to MyData.xml ...");
				
				GameObject[] gos = GameObject.FindGameObjectsWithTag ("TestObj");
				writer.WriteStartElement ("GameObjects");
				foreach (GameObject go in gos) {
					Transform t = go.transform;
					writer.WriteStartElement (go.name);				
				
					writer.WriteStartElement ("Position");	
					writer.WriteStartElement ("x");
					writer.WriteValue (t.localPosition.x);
					writer.WriteEndElement ();
					writer.WriteStartElement ("y");
					writer.WriteValue (t.localPosition.y);
					writer.WriteEndElement ();
					writer.WriteStartElement ("z");
					writer.WriteValue (t.localPosition.z);
					writer.WriteEndElement ();	
					writer.WriteEndElement ();
				
					writer.WriteStartElement ("Rotation");				
					writer.WriteStartElement ("w");
					writer.WriteValue (t.localRotation.w);
					writer.WriteEndElement ();
					writer.WriteStartElement ("x");
					writer.WriteValue (t.localRotation.x);
					writer.WriteEndElement ();
					writer.WriteStartElement ("y");
					writer.WriteValue (t.localRotation.y);
					writer.WriteEndElement ();
					writer.WriteStartElement ("z");
					writer.WriteValue (t.localRotation.z);
					writer.WriteEndElement ();				
					writer.WriteEndElement ();
								
					writer.WriteStartElement ("Scale");				
					writer.WriteStartElement ("x");
					writer.WriteValue (t.localScale.x);
					writer.WriteEndElement ();
					writer.WriteStartElement ("y");
					writer.WriteValue (t.localScale.y);
					writer.WriteEndElement ();
					writer.WriteStartElement ("z");
					writer.WriteValue (t.localScale.z);
					writer.WriteEndElement ();				
					writer.WriteEndElement ();
								
					writer.WriteEndElement ();
				}
				writer.WriteEndElement ();
				writer.Flush ();
				Debug.Log ("Completed saving.");
			}
		}
	}
}