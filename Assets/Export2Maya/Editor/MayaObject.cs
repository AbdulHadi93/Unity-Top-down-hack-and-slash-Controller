using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// --------------------------------------------------
// MayaObject Class
// --------------------------------------------------
// Helper class for processing and storing Unity objects as Maya objects
public class MayaObject {
	public MayaObject Parent;					// The parent transform of this MayaObject
	public List<MayaObject> Children;			// List of transforms parented under this object
	
	public Transform UnityObject;				// The Unity GameObject this Maya Object refers to
	public string MayaName;						// The name that will be written to the Maya file
	
	public ObjType Type;						// The type of object the GameObject is
	
	// --------------------------------------------------
	// Constructor
	// --------------------------------------------------
	public MayaObject(Transform t){
		UnityObject = t;						// Automatically set the UnityObject to given transform
		Type = ObjType.Transform;				// Default ObjType to Transform
		Children = new List<MayaObject>();		// Create a new list ready to populate with children
	}
	
	// --------------------------------------------------
	// Determine GameObject type
	// --------------------------------------------------
	// All MayaObjects default to transform only (no shape node).
	// When requested, this will examine what components are attached to
	// the GameObject and determine what type of object this MayaObject is.
	// This will also effectively signal to the exporter that we want to export
	// the shape data as well.
	public void GetObjType(){
		// --------------------------------------------------
		// Mesh Check
		// --------------------------------------------------
		MeshFilter MeshCheck = UnityObject.gameObject.GetComponent<MeshFilter>();
		if(MeshCheck != null){
			// Just because there is a mesh filter attached to it doesn't mean there is a mesh
			// linked to it. Check that there is actually a shared mesh
			Mesh SharedMeshCheck = MeshCheck.sharedMesh;
			if(SharedMeshCheck != null){
				// Finally, there might be a mesh, but no data (procedural meshes), so check that there is
				// actually data . . . sheesh! LOL
				if(SharedMeshCheck.vertices.Length > 2) Type = ObjType.Mesh;
			}
		}
		
		// --------------------------------------------------
		// Skinned Mesh Check
		// --------------------------------------------------
		SkinnedMeshRenderer SkinnedMeshCheck = UnityObject.gameObject.GetComponent<SkinnedMeshRenderer>();
		if(SkinnedMeshCheck != null){
			// Just because there is a mesh filter it doesn't mean there is a mesh
			// linked to it. Check that there is actually a shared mesh
			Mesh SharedMeshCheck = SkinnedMeshCheck.sharedMesh;
			if(SharedMeshCheck != null) Type = ObjType.SkinnedMesh;
		}
		
		// --------------------------------------------------
		// Light Check
		// --------------------------------------------------
		Light LightCheck = UnityObject.gameObject.GetComponent<Light>();
		if(LightCheck != null){
			Type = ObjType.Light;
		}
	}
}

// --------------------------------------------------
// Enumerator for storing what type of object this MayaObject is
// --------------------------------------------------
public enum ObjType {
	Transform,
	Mesh,
	SkinnedMesh,
	Bone,
	Terrain,
	Light	
};