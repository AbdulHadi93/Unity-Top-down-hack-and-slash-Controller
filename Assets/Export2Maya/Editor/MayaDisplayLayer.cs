using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// --------------------------------------------------
// MayaDisplayLayer Class
// --------------------------------------------------
// Used to manage display layers 
public class MayaDisplayLayer {
	// --------------------------------------------------
	// The index of the lightmap this DisplayLayer refers to
	// --------------------------------------------------
	public int LightmapIndex = 0;
	
	// --------------------------------------------------
	// List of MayaObjects that belong to this display layer
	// --------------------------------------------------
	public List<MayaObject> Objects;
	
	// --------------------------------------------------
	// Constructor
	// --------------------------------------------------
	public MayaDisplayLayer(int Index){
		// Initialize the LightmapIndex
		LightmapIndex = Index;
		
		// Initialize the MayaObject list
		Objects = new List<MayaObject>();
	}
}
