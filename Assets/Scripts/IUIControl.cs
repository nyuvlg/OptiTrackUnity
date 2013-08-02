using UnityEngine;
using System.Collections;

/// <summary>The interface which every UI Control must derive from.</summary>
public interface IUIControl
{
    /// <summary>Call this when the pliers are opened </summary>
	void OnPliersOpen () ;
}
