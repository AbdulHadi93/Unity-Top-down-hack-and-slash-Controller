using UnityEngine;
using System.Collections;

// --------------------------------------------------
// MayaName Class
// --------------------------------------------------
// Helper class for naming MayaObjects so there is no name clashing
public class MayaName {
	public string Name;
	public int Count;
	
	// --------------------------------------------------
	// Constructor
	// --------------------------------------------------
	public MayaName(string name){
		Name = name;
		Count = 0;
	}
	
	// --------------------------------------------------
	// Returns a nicely formatted name with version count
	// --------------------------------------------------
	public string GetName(){
		return (Name + "_" + Count.ToString());
	}
}