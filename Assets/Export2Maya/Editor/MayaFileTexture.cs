using UnityEngine;
using System.Collections;

// --------------------------------------------------
// MayaFileTexture Class
// --------------------------------------------------
// Helper class for storing Maya file textures
public class MayaFileTexture {
	public Texture UnityTexture;
	public string MayaFileName;			// The name of the file texture as it would appear in Maya
	public string MayaBumpName;			// The name of the bump node as it would appear in Maya
	public string MayaPlacementName;	// The name of the placement node as it would appear in Maya
	public string SourcePath;			// The path of the texture file in the Unity Project folder
	public string DestinationPath;		// The desired path of the copied texture file
	public Vector2 Tiling;				// The tiling value of the texture
	public Vector2 Offset;				// The offset value of the texture
	
	// --------------------------------------------------
	// Constructor
	// --------------------------------------------------
	public MayaFileTexture(Texture t, string FileNodeName, string Place2DNodeName, string sourcePath, string destinationPath){
		UnityTexture = t;
		MayaFileName = FileNodeName;
		MayaPlacementName = Place2DNodeName;
		SourcePath = sourcePath;
		DestinationPath = destinationPath;
	}
	
	// --------------------------------------------------
	// This will return the MEL code needed to create this file texture
	// as well as the texture placement node for it
	// --------------------------------------------------
	public string GetMel(){
		string mel = "";
		
		// --------------------------------------------------
		// Create file node
		// --------------------------------------------------
		mel += "createNode file -n \"" + MayaFileName + "\";\n";
			// Alpha is luminance
			mel += "\tsetAttr \".ail\" yes;\n";
			// File texture name
			mel += "\tsetAttr \".ftn\" -type \"string\" \"" + DestinationPath + "\";\n";

		// --------------------------------------------------
		// Create texture placement
		// --------------------------------------------------
		mel += "createNode place2dTexture -n \"" + MayaPlacementName + "\";\n";
			// Repeat UV
			mel += "\tsetAttr \".re\" -type \"float2\" " + Tiling.x + " " + Tiling.y + ";\n";
			// Offset
			mel += "\tsetAttr \".of\" -type \"float2\" " + Offset.x + " " + Offset.y + ";\n";
		
		// --------------------------------------------------
		// Connect file node to defaultTextureList1
		// --------------------------------------------------
		mel += "connectAttr \"" + MayaFileName + ".msg\" \":defaultTextureList1.tx\" -na;\n";
	
		// --------------------------------------------------
		// Connect texture placement node to file node
		// --------------------------------------------------
		mel += "connectAttr \"" + MayaPlacementName + ".msg\" \":defaultRenderUtilityList1.u\" -na;\n";
		mel += "connectAttr \"" + MayaPlacementName + ".c\" \"" + MayaFileName + ".c\";\n";
		mel += "connectAttr \"" + MayaPlacementName + ".tf\" \"" + MayaFileName + ".tf\";\n";
		mel += "connectAttr \"" + MayaPlacementName + ".rf\" \"" + MayaFileName + ".rf\";\n";
		mel += "connectAttr \"" + MayaPlacementName + ".mu\" \"" + MayaFileName + ".mu\";\n";
		mel += "connectAttr \"" + MayaPlacementName + ".mv\" \"" + MayaFileName + ".mv\";\n";
		mel += "connectAttr \"" + MayaPlacementName + ".s\" \"" + MayaFileName + ".s\";\n";
		mel += "connectAttr \"" + MayaPlacementName + ".wu\" \"" + MayaFileName + ".wu\";\n";
		mel += "connectAttr \"" + MayaPlacementName + ".wv\" \"" + MayaFileName + ".wv\";\n";
		mel += "connectAttr \"" + MayaPlacementName + ".re\" \"" + MayaFileName + ".re\";\n";
		mel += "connectAttr \"" + MayaPlacementName + ".of\" \"" + MayaFileName + ".of\";\n";
		mel += "connectAttr \"" + MayaPlacementName + ".r\" \"" + MayaFileName + ".ro\";\n";
		mel += "connectAttr \"" + MayaPlacementName + ".n\" \"" + MayaFileName + ".n\";\n";
		mel += "connectAttr \"" + MayaPlacementName + ".vt1\" \"" + MayaFileName + ".vt1\";\n";
		mel += "connectAttr \"" + MayaPlacementName + ".vt2\" \"" + MayaFileName + ".vt2\";\n";
		mel += "connectAttr \"" + MayaPlacementName + ".vt3\" \"" + MayaFileName + ".vt3\";\n";
		mel += "connectAttr \"" + MayaPlacementName + ".vc1\" \"" + MayaFileName + ".vc1\";\n";
		mel += "connectAttr \"" + MayaPlacementName + ".o\" \"" + MayaFileName + ".uv\";\n";
		mel += "connectAttr \"" + MayaPlacementName + ".ofs\" \"" + MayaFileName + ".fs\";\n";
			
		return mel;
	}
}