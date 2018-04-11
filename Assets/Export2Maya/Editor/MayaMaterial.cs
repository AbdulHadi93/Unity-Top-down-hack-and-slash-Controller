using UnityEngine;
using System.Collections;

// --------------------------------------------------
// MayaMaterial Class
// --------------------------------------------------
// Helper class for storing Maya materials
public class MayaMaterial {
	public Material UnityMaterial;
	public string MayaName;
	
	// Every material needs a materialInfo node.
	// As a convenience we store the name here so
	// we can access it later
	public string MaterialInfo;
	
	// Store references to MayaTextures for the material
	// Note - We use this when setting up connections
	// from texture to material
	public MayaFileTexture MainTex;
	// _Bump
	// _Emissive
	
	// --------------------------------------------------
	// This will return the MEL code needed to create this material
	// and the materialInfo node 
	// --------------------------------------------------
	public string GetMel(){
		string mel = "";
		
		mel += "createNode blinn -n \"" + MayaName + "\";\n";
			// Set color
//			mel += "\tsetAttr \".c\" -type \"float3\" 1 0 0;";
		mel += "createNode shadingEngine -n \"" + MayaName + "SG\";\n";
			mel += "\tsetAttr \".ihi\" 0;\n";		// Is historically interesting
			mel += "\tsetAttr \".ro\" yes;\n";		// Renderable Only Set
		// Create MaterialInfo Node
		mel += "createNode materialInfo -n \"" + MaterialInfo + "\";\n";
		// Link the light linker, SG, and default light set together
		mel += "relationship \"link\" \":lightLinker1\" \"" + MayaName + "SG.message\" \":defaultLightSet.message\";\n";
		// Shadow Link the light linker, SG, and default light set together
		mel += "relationship \"shadowLink\" \":lightLinker1\" \"" + MayaName + "SG.message\" \":defaultLightSet.message\";\n";
		// Connect shader.outColor > SG.surfaceShader
		mel += "connectAttr \"" + MayaName + ".oc\" \"" + MayaName + "SG.ss\";\n";
		// Connect SG.message > MaterialInfo.shadingGroup
		mel += "connectAttr \"" + MayaName + "SG.msg\" \"" + MaterialInfo + ".sg\";\n";
		// Connect shader.message > materialInfo.message
		mel += "connectAttr \"" + MayaName + ".msg\" \"" + MaterialInfo + ".m\";\n";
		// Connect SG.partition > renderPartition.sets
		mel += "connectAttr \"" + MayaName + "SG.pa\" \":renderPartition.st\" -na;\n";
		// Connect shader.message > defaultShaderList.shaders
		mel += "connectAttr \"" + MayaName + ".msg\" \":defaultShaderList1.s\" -na;\n";
		
		return mel;	
	}
}