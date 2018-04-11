using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
 
// --------------------------------------------------
// Export2Maya ( Version 1.1.0 )
// By:	Michael Cook
// --------------------------------------------------
// IN PROGRESS:
//		MeshToMel class 
// COMPLETED:
//		Added scale option for export
//		Added light export
//		Added different method to create Export2Maya Menu (hopefully fixes issues some were having)
//		Added check for Submesh count being greater than material count
//		Added Unit scale for exported Maya scenes
//		Added display layer lightmap association for easy object selection in Maya
//		Added check for zero based uv's from Unity's tree creator
//		Separated classes into separate scripts for easy maintenance 
//		Created MayaUtilities class that converts between coordinates Unity -> Maya
// TO DO LIST:
//		Skinned Mesh Export
//		Better Texture Export Technique
//		Better Renaming method using DG paths
//		Option to export objects as instances instead of meshes
//		Animation Export
//		Terrain Export
//		Add check for objects that are light mapped but don't have UV2 data (plane)
 
public class Export2Maya : EditorWindow {
	// --------------------------------------------------
	// Export2Maya UI variables
	// --------------------------------------------------
	string[] MayaVersions = new string[]{ "2015", "2014", "2013.5", "2013", "2012" };
	int MayaVersionIndex = 0;
	string[] MayaUnits = new string[]{ "millimeter", "centimeter", "meter", "inch", "foot", "yard" };
	int MayaUnitsIndex = 1;
	public static float MayaExportScale = 1.0f;
	public static bool ExportNormals = true;
	public static bool ExportUVs = true;
	public static bool ExportLightmapUVs = true;
	public static bool ExportVertexColors = true;
	public static bool ExportMaterials = true;
	public static bool ExportTextures = true;

	// --------------------------------------------------
	// File Variables
	// --------------------------------------------------
	public static string filePath = "";
	public static string fileName = "";
	public static float MaxProgress = 0.0f;
	public static float CurProgress = 0.0f;
	public static string ProgressTitle = "Exporting Maya Scene File:";
	public static string ProgressMsg = "{0} ( Writing {1} )";
	public static int ColumnDataWidth = 5;				// Used to format the data in the Maya ASCII file into columns
	public static string MayaData = "";					// The Maya ASCII code involved with node creation
	public static string MayaConnections = "";			// The Maya ASCII code involved with node connections (written at end of file)
	
	// --------------------------------------------------
	// Types of Object Counters
	// --------------------------------------------------
	int NumberOfMeshes = 0;								// Number of meshes to export
	int NumberOfSkinnedMeshes = 0;						// Number of skinned meshes to export
	int NumberOfBones = 0;								// Number of bones to export
	int NumberOfTerrains = 0;							// Number of terrains to export
	int NumberOfLights = 0;								// Number of lights to export. (used in lightLinker, lightList, and the defaultLightSet)
	
	// --------------------------------------------------
	// Dependency Graph Nodes
	// --------------------------------------------------
	// Counter used to assign unique names to BindPose Nodes
	public static int BindPoseCounter;
	public static string NewBindPose(){
		BindPoseCounter++; return ("bindPose" + BindPoseCounter);
	}	
	// Counter used to assign unique names to Texture Nodes
	public static int FileTextureCounter;
	public static string NewFileTexture(){
		FileTextureCounter++; return ("file" + FileTextureCounter);
	}
	// Counter used to assign unique names to GroupID Nodes
	public static int GroupIDCounter;
	public static string NewGroupID(){
		GroupIDCounter++; return ("groupId" + GroupIDCounter);
	}	
	// Counter used to assign unique names to GroupParts Nodes
	public static int GroupPartsCounter;
	public static string NewGroupParts(){
		GroupPartsCounter++; return ("groupParts" + GroupPartsCounter);
	}	
	// Counter used to assign unique names to MaterialInfo Nodes
	public static int MaterialInfoCounter;
	public static string NewMaterialInfo(){
		MaterialInfoCounter++; return ("materialInfo" + MaterialInfoCounter);
	}
	// Counter used to assign unique names to Place2DTexture Nodes
	public static int Place2DTextureCounter;
	public static string NewPlace2DTexture(){
		Place2DTextureCounter++; return ("place2dTexture" + Place2DTextureCounter);
	}
	// Counter used to assign unique names to SkinClusterGroupID Nodes
	public static int SkinClusterGroupIDCounter;
	public static string NewSkinClusterGroupID(){
		SkinClusterGroupIDCounter++; return ("skinCluster" + SkinClusterGroupIDCounter + "GroupId");
	}
	// Counter used to assign unique names to SkinClusterGroupParts Nodes
	public static int SkinClusterGroupPartsCounter;
	public static string NewSkinClusterGroupParts(){
		SkinClusterGroupPartsCounter++; return ("skinCluster" + SkinClusterGroupPartsCounter + "GroupParts");
	}
	// Counter used to assign unique names to SkinClusterSet Nodes
	public static int SkinClusterSetCounter;
	public static string NewSkinClusterSet(){
		SkinClusterSetCounter++; return ("skinCluster" + SkinClusterSetCounter + "Set");
	}
	// Counter used to assign unique names to Tweak Nodes
	public static int TweakCounter;
	public static string NewTweak(){
		TweakCounter++; return ("tweak" + TweakCounter);
	}
	// Counter used to assign unique names to TweakSet Nodes
	public static int TweakSetCounter;
	public static string NewTweakSet(){
		TweakSetCounter++; return ("tweakSet" + TweakSetCounter);
	}
	
	// --------------------------------------------------
	// List of selected GameObjects
	// --------------------------------------------------
	GameObject[] SelectedObjects;
	
	// --------------------------------------------------
	// Object Preprocessing
	// --------------------------------------------------
	List<MayaObject> FlatObjList;						// Unconnected list of GameObjects converted to MayaObjects
	List<MayaObject> RootObjects;						// Connected list of MayaObjects
	
	// --------------------------------------------------
	// Static Node Lists
	// --------------------------------------------------
	public static List<MayaMaterial> MayaMaterials;		// Material List
	public static List<MayaFileTexture> MayaTextures;	// File Texture List
	public static List<MayaDisplayLayer> DisplayLayers;	// Display Layer List
	public static List<MayaName> MayaNameList;			// Maya Name Clashing List
	
	// --------------------------------------------------
	// Debugger UI Variables
	// --------------------------------------------------
	/*
	int DebugVert = 0;
	int DebugUV = 0;
	int DebugUV2 = 0;
	int DebugColors = 0;
	int DebugNormals = 0;
	int DebugTris = 0;
	*/
	
	#region Memory Cleanup
	// --------------------------------------------------
	// Clear Variables - Free Memory
	// --------------------------------------------------
	// Clears out all the variables we have to
	// free up any memory they were using
	void ResetVariables(){
		// Reset selected objects
		SelectedObjects = new GameObject[0];
		
		// Reset lists
		MayaNameList = new List<MayaName>();
		FlatObjList = new List<MayaObject>();
		RootObjects = new List<MayaObject>();
		
		NumberOfMeshes = 0;
		NumberOfSkinnedMeshes = 0;
		NumberOfBones = 0;
		NumberOfTerrains = 0;
		NumberOfLights = 0;
		
		MayaMaterials = new List<MayaMaterial>();
		MayaTextures = new List<MayaFileTexture>();
		DisplayLayers = new List<MayaDisplayLayer>();
		
		BindPoseCounter = 0;
		FileTextureCounter = 0;
		GroupIDCounter = 0;
		GroupPartsCounter = 0;
		MaterialInfoCounter = 0;
		Place2DTextureCounter = 0;
		SkinClusterGroupIDCounter = 0;
		SkinClusterGroupPartsCounter = 0;
		SkinClusterSetCounter = 0;
		TweakCounter = 0;
		TweakSetCounter = 0;
		
		MayaConnections = "";
	
		MaxProgress = 0.0f;
		CurProgress = 0.0f;
	}
	#endregion
	
	// --------------------------------------------------
	// Export2Maya - UI
	// --------------------------------------------------
	/* Old Method
	[MenuItem ("Window/Export2Maya")]
    static void Init(){
        // Get existing open window, if none then make a new one
        Export2Maya window = (Export2Maya)EditorWindow.GetWindow(typeof(Export2Maya), false, "Export2Maya");
		window.position = new Rect((Screen.width / 2) - 150, (Screen.height) / 2 + 150, 250, 290);
        window.Show();
    }
	*/
	
	void OnGUI(){
		GUILayout.Label("Maya Version:", EditorStyles.boldLabel);
			MayaVersionIndex = EditorGUILayout.Popup(MayaVersionIndex, MayaVersions, GUILayout.MaxWidth(100));
		GUILayout.Label("Maya Units:", EditorStyles.boldLabel);
			MayaUnitsIndex = EditorGUILayout.Popup(MayaUnitsIndex, MayaUnits, GUILayout.MaxWidth(100));
			MayaExportScale = EditorGUILayout.FloatField("Export Scale Multiplier:", MayaExportScale);
			if(MayaExportScale < 0.001f) MayaExportScale = 0.001f;
		GUILayout.Label("Mesh Settings:", EditorStyles.boldLabel);
			ExportNormals = EditorGUILayout.ToggleLeft(" Export Normals", ExportNormals);
			ExportUVs = EditorGUILayout.ToggleLeft(" Export UVs", ExportUVs);
			ExportLightmapUVs = EditorGUILayout.ToggleLeft(" Export Lightmap UVs", ExportLightmapUVs);
			ExportVertexColors = EditorGUILayout.ToggleLeft(" Export Vertex Colors", ExportVertexColors);
		GUILayout.Label("Material Settings:", EditorStyles.boldLabel);
			ExportMaterials = EditorGUILayout.ToggleLeft(" Export Materials", ExportMaterials);
			GUI.enabled = ExportMaterials;
			ExportTextures = EditorGUILayout.ToggleLeft(" Export Textures", ExportTextures);
			if(!ExportMaterials) ExportTextures = false;
			GUI.enabled = true;
		// Begin export
		if(GUILayout.Button("Export Selection", GUILayout.Height(22))) MayaExporter();
		GUI.enabled = false;
		GUILayout.Label("Export2Maya - ver 1.1.0", EditorStyles.miniLabel);
		GUI.enabled = true;
		
		/*
		GUILayout.Label("Mesh Debugger:", EditorStyles.boldLabel);
			GUILayout.Label("Verts: " + DebugVert, EditorStyles.boldLabel);
			GUILayout.Label("UV: " + DebugUV, EditorStyles.boldLabel);
			GUILayout.Label("UV2: " + DebugUV2, EditorStyles.boldLabel);
			GUILayout.Label("Colors: " + DebugColors, EditorStyles.boldLabel);
			GUILayout.Label("Normals: " + DebugNormals, EditorStyles.boldLabel);
			GUILayout.Label("Tris: " + DebugTris, EditorStyles.boldLabel);
		*/
    }
	
	/*
	void OnSelectionChange(){
		bool isMesh = false;
		if(Selection.gameObjects.Length > 0){
			// If it doesn't have a mesh filter, it is not a mesh
			if(Selection.gameObjects[0].GetComponent<MeshFilter>() != null){
				// Just because there is a mesh filter doesn't mean there is a mesh
				// linked to it. Check that there is actually a shared mesh
				Mesh sharedMeshCheck = Selection.gameObjects[0].GetComponent<MeshFilter>().sharedMesh;
				if(sharedMeshCheck != null){
					isMesh = true;
				}
			}
		}
		
		if(!isMesh){
			DebugVert = 0;
			DebugUV = 0;
			DebugUV2 = 0;
			DebugColors = 0;
			DebugNormals = 0;
			DebugTris = 0;
		}
		else{
			// Get the mesh data
			Mesh m = Selection.gameObjects[0].transform.GetComponent<MeshFilter>().sharedMesh;

			DebugVert = m.vertices.Length;
			DebugUV = m.uv.Length;
			DebugUV2 = m.uv2.Length;
			DebugColors = m.colors.Length;
			DebugNormals = m.normals.Length;
			DebugTris = m.triangles.Length;
		}
		
		Repaint();
	}
	*/
	
	#region The Main Entry point
	void MayaExporter(){	
		// --------------------------------------------------
		// Reset variables
		// --------------------------------------------------
		ResetVariables();
		
		// --------------------------------------------------
		// Grab the selected objects
		// --------------------------------------------------
		SelectedObjects = Selection.gameObjects;
		 
		// --------------------------------------------------
		// Selected Objects Check
		// --------------------------------------------------
		// Check selection before doing anything
		if(SelectedObjects.Length < 1){
		//	Debug.LogWarning("Nothing selected to export!");
			EditorUtility.DisplayDialog("Nothing to Export!", "Please select the GameObjects you wish to export and try again.", "Ok");
			return;
		}
		
		// --------------------------------------------------
		// File Save Prompt
		// --------------------------------------------------
		// Prompt the user where to save the Maya file
		fileName = EditorUtility.SaveFilePanel("Export Maya Scene File", "", "", "ma");
		if(fileName == "") return;	// If they cancel then abort

		
		// Split out file name and file path
		string[] tokens = fileName.Split('/');
		filePath = "";
		for(int i=0; i<tokens.Length - 1; i++) filePath += tokens[i] + "/";
		fileName = tokens[tokens.Length - 1];
		
		// --------------------------------------------------
		// Start new file
		// --------------------------------------------------
		// Begin new Maya Scene file, add Maya Version info and
		// default cameras
		//
		// Note - If we are overwriting an existing file, any existing
		// data in the file will be erased
		StartNewFile();
		
		// --------------------------------------------------
		// Process selected GameObjects
		// --------------------------------------------------
		// Go through selection and create MayaObjects of all
		// children and parent objects
		//
		// Note - Any child objects will automatically have their
		// shape export set to TRUE. Any parents of the selected
		// objects will only have their transforms exported
		EditorUtility.DisplayProgressBar(ProgressTitle, "Converting Selection to Maya Objects", 0);
		SelectionToMayaObjects();
		
		// --------------------------------------------------
		// Find and assign the bone MayaObjects
		// --------------------------------------------------
		FindBones();
		
		// --------------------------------------------------
		// Get count of the different types of MayaObjects (mesh, skinnedmesh, light, etc)
		// --------------------------------------------------
		EditorUtility.DisplayProgressBar(ProgressTitle, "Counting Types of Maya Objects", 0);
		CountTypesOfObjects();
		
		// --------------------------------------------------
		// Connect MayaObjects
		// --------------------------------------------------
		// Go through selection again, this time populating the
		// MayaObjects connection data ( parents and children )
		EditorUtility.DisplayProgressBar(ProgressTitle, "Connecting Maya Objects", 0);
		ConnectMayaObjects();
		
		// --------------------------------------------------
		// Build root object list
		// --------------------------------------------------
		// Build a list of root only objects. Since we have the
		// parents and children connected together, we will start
		// at the root of each chain of objects and work our way down
		// recursively
		EditorUtility.DisplayProgressBar(ProgressTitle, "Building Root Object List", 0);
		for(int i=0; i<FlatObjList.Count; i++){
			// If there is no parent, it is a root object
			if(FlatObjList[i].Parent == null){
				RootObjects.Add(FlatObjList[i]);
			}
		}
		
		// --------------------------------------------------
		// Rename MayaObjects
		// --------------------------------------------------
		// Recursively go through root objects and children and
		// assign a unique Maya name to avoid name clashes
		EditorUtility.DisplayProgressBar(ProgressTitle, "Renaming Maya Objects", 0);
		for(int i=0; i<RootObjects.Count; i++){
			AssignMayaName(RootObjects[i]);
		}
		
		// --------------------------------------------------
		// Build Material List
		// --------------------------------------------------
		// Go through the FlatObjList, and find which materials
		// are assigned. If we haven't registered the material name
		// then do so
		if(ExportMaterials){
			EditorUtility.DisplayProgressBar(ProgressTitle, "Building Material List", 0);
			BuildMaterialList();
		}
		
		// --------------------------------------------------
		// Build Texture List
		// --------------------------------------------------
		// Go through the materials list and find which textures
		// are being used, and build a texture list from it
		if(ExportTextures){
			EditorUtility.DisplayProgressBar(ProgressTitle, "Building Texture List", 0);
			BuildTextureList();
		}
		
		// --------------------------------------------------
		// Build DisplayLayer List
		// --------------------------------------------------
		// Go through the FlatObjList, and check if the object
		// is set to lightmap static. If it is, create the DisplayLayer 
		// for the lightmap index and add the MayaObject to it
		EditorUtility.DisplayProgressBar(ProgressTitle, "Building Display Layer List", 0);
		for(int i=0; i<FlatObjList.Count; i++){
			// Check if the object has a MeshRenderer
			MeshRenderer MeshRender = FlatObjList[i].UnityObject.gameObject.GetComponent<MeshRenderer>();
			if(MeshRender != null){
				// Check if the mesh renderer lightmap index is set
				//		LightmapIndex -1 = Not Lightmap Static
				// 		LightmapIndex 255 = No Lightmap at all
				// 		LightmapIndex 254 = No Lightmap, but calculate GI
				// 		LightmapIndex 253 and Lower = LightmapIndex
				if(MeshRender.lightmapIndex < 254 && MeshRender.lightmapIndex > -1){
					// Find the DisplayLayer who's lightmap index matches
					int DisplayLayerIndex = -1;
					for(int d=0; d<DisplayLayers.Count; d++){
						if(DisplayLayers[d].LightmapIndex == MeshRender.lightmapIndex){
							DisplayLayerIndex = d;
							break;
						}
					}
					
					// If the DisplayLayer was not found, create it
					if(DisplayLayerIndex == -1){
						DisplayLayers.Add(new MayaDisplayLayer(MeshRender.lightmapIndex));
						
						// Update index to latest index
						DisplayLayerIndex = DisplayLayers.Count - 1;
					}
					
					// Add MayaObject to display layer MayaObject list
					DisplayLayers[DisplayLayerIndex].Objects.Add(FlatObjList[i]);
				}
			}
		}
		// Sort the DisplayLayers based on LightmapIndex property
		DisplayLayers = DisplayLayers.OrderBy(o=>o.LightmapIndex).ToList();
		
		// --------------------------------------------------
		// Process Bar initialization
		// --------------------------------------------------
		MaxProgress = FlatObjList.Count;
		
		// --------------------------------------------------
		// Write MayaObjects to File
		// --------------------------------------------------
		// Now that we have all the objects converted to MayaObjects
		// and sorted correctly, begin writing each object to disk
		for(int i=0; i<RootObjects.Count; i++){
			ProcessTransform(RootObjects[i]);
		}
		
		// --------------------------------------------------
		// Write Display Layer Manager and Display Layers
		// --------------------------------------------------
		DisplayLayerExport();
		
		// --------------------------------------------------
		// Write LightLinker
		// --------------------------------------------------
		StartLightLinker();

		// --------------------------------------------------
		// Write LightList and DefaultLightSet
		// --------------------------------------------------
		StartLightsExport();
		
		// --------------------------------------------------
		// Write renderPartition, and defaultShaderList
		// --------------------------------------------------
		if(ExportMaterials){
			StartMaterialExport();
		}
		
		// --------------------------------------------------
		// Write MayaMaterials
		// --------------------------------------------------
		if(ExportMaterials){
			for(int i=0; i<MayaMaterials.Count; i++){
				string Mat = MayaMaterials[i].GetMel();
				// Write data to file
				AppendToFile(Mat);
			}
		}
		
		// --------------------------------------------------
		// Write defaultRenderUtilityList size
		// --------------------------------------------------
		if(ExportTextures){
			StartTextureExport();
		}
		
		// --------------------------------------------------
		// Write MayaTextures
		// --------------------------------------------------
		if(ExportTextures){
			for(int i=0; i<MayaTextures.Count; i++){
				string Tex = MayaTextures[i].GetMel();
				// Write data to file
				AppendToFile(Tex);
			}
		}

		// --------------------------------------------------
		// Build Material Connections
		// --------------------------------------------------
		// We will be going through each material, and writing the
		// connections between material > textures. We will also be
		// writing connections between texture > texturePlacement nodes
		if(ExportMaterials){
			for(int i=0; i<MayaMaterials.Count; i++){
				// Check for MainTex
				if(MayaMaterials[i].MainTex != null){
					MayaConnections += "connectAttr \"" + MayaMaterials[i].MainTex.MayaFileName + ".msg\" \"" + MayaMaterials[i].MaterialInfo + ".t\" -na;\n";
					MayaConnections += "connectAttr \"" + MayaMaterials[i].MainTex.MayaFileName + ".oc\" \"" + MayaMaterials[i].MayaName + ".c\";\n";
				//	MayaConnections += "connectAttr \"" + MayaMaterials[i].MainTex.MayaFileName + ".ot\" \"" + MayaMaterials[i].MayaName + ".it\";\n";
				//	MayaConnections += "connectAttr \"" + MayaMaterials[i].MainTex.MayaFileName + ".oc\" \"" + MayaMaterials[i].MayaName + ".ambc\";\n";
				//	MayaConnections += "connectAttr \"" + MayaMaterials[i].MainTex.MayaFileName + ".oc\" \"" + MayaMaterials[i].MayaName + ".ic\";\n";
				//	MayaConnections += "connectAttr \"" + MayaMaterials[i].MainTex.MayaFileName + ".oc\" \"" + MayaMaterials[i].MayaName + ".sc\";\n";
				}
			}
		}
		
		// --------------------------------------------------
		// Write Maya Connections
		// --------------------------------------------------
		// Write data to file
		AppendToFile(MayaConnections);
		
		// --------------------------------------------------
		// Copy Textures
		// --------------------------------------------------
		// Go through our texture list and copy the textures from
		// our Unity project to the destination path
		if(ExportTextures){
			for(int i=0; i<MayaTextures.Count; i++){
				// If the file doesn't already exist in the destination path
				if(!System.IO.File.Exists(MayaTextures[i].DestinationPath)){
					FileUtil.CopyFileOrDirectory(MayaTextures[i].SourcePath, MayaTextures[i].DestinationPath);
				}
			}
		}

		// --------------------------------------------------
		// Clear Progress Bar
		// --------------------------------------------------
		EditorUtility.ClearProgressBar();

		// --------------------------------------------------
		// Clear the variable data
		// --------------------------------------------------
		ResetVariables();
	}
	#endregion
	
	
	void StartLightLinker(){
		string data = "";
		
		// !!! Note Total Amount is Number of Lights + Number of Materials !!!
		data += "createNode lightLinker -s -n \"lightLinker1\";\n";
			data += "\tsetAttr -s " + (NumberOfLights + MayaMaterials.Count) + " \".lnk\";\n";
			data += "\tsetAttr -s " + (NumberOfLights + MayaMaterials.Count) + " \".slnk\";\n";
			
		// Write data to file
		AppendToFile(data);
		data = "";
	}	
	
	void StartLightsExport(){
		string data = "";
		
		// Set Light List Size (number of lights)
		data += "select -ne :lightList1;\n";
			data += "\tsetAttr -s " + NumberOfLights + " \".l\";\n";

		// Set Default Light Set (number of lights)
		data += "select -ne :defaultLightSet;\n";
			data += "\tsetAttr -s " + NumberOfLights + " \".dsm\";\n";
			
		// Write data to file
		AppendToFile(data);
		data = "";
	}	
	
	
	
	#region MayaObject Creation
	// This will go through the SelectedObjects list and create MayaObject
	// versions of them
	void SelectionToMayaObjects(){
		// Go through selected objects
		for(int i=0; i<SelectedObjects.Length; i++){
			// Recursively find all children of selected objects
			//
			// Note - We will automatically set ExportShape to TRUE
			// for children
			ProcessChildren(SelectedObjects[i].transform);
			
			// Recursively find all parents of selected objects
			// Note - We leave ExportShape to FALSE because we only need
			// the parent transforms
			ProcessParents(SelectedObjects[i].transform);
		}
	}
	
	// Recursively process all child transforms of given transform
	void ProcessChildren(Transform t){
		// Add this GameObject to our FlatObjList
		CreateMayaObject(t, true);
		
		// Find any children of this transform and process them
		foreach(Transform child in t){
			ProcessChildren(child);
		}
	}
	
	// Recursively process all parent transforms of given transform
	void ProcessParents(Transform t){
		// Add this GameObject to our FlatObjList
		CreateMayaObject(t, false);
		
		// If transform has a parent, process it
		if(t.parent != null){
			ProcessParents(t.parent);
		}
	}
	
	// --------------------------------------------------
	// Create MayaObject
	// --------------------------------------------------
	// Given a transform, this will create a MayaObject and link
	// the transform to it ( so we know which GameObject this 
	// MayaObject references )
	//
	// Note - By default MayaObjects are set to type transform. If type is NOT
	// set to transform, that means we want to export the shape as well. We 
	// export "transforms only" for parents of selected GameObjects, since we 
	// need them to accurately place the MayaObjects in the Maya scene, but
	// don't want their shape data
	void CreateMayaObject(Transform t, bool ExportShape){
		bool ObjFound = false;
		
		// First check if the object already exists in our FlatObjList
		for(int i=0; i<FlatObjList.Count; i++){
			// OBJECT FOUND! It already exists
			if(FlatObjList[i].UnityObject == t){
				ObjFound = true;
				// We need this here, in case we already had the object but ExportShape
				// was false, this will set it to TRUE since the code below would not be
				// called
				if(FlatObjList[i].Type == ObjType.Transform){
					if(ExportShape) FlatObjList[i].GetObjType();
				}
				break;
			}
		}
		
		// If we did not find the MayaObject
		if(!ObjFound){
			// Create a new MayaObject
			MayaObject MayaObj = new MayaObject(t);
			
			// We also need to know if we need to export the shape or just the
			// transform for this object
			if(ExportShape) MayaObj.GetObjType();

			// Add this object to our flat object list
			FlatObjList.Add(MayaObj);
		}
	}
	#endregion

	// --------------------------------------------------
	// Find and assign the bone MayaObjects
	// --------------------------------------------------
	// Since Unity doesn't have "bones" we have to go through
	// every SkinnedMeshRenderer, get the GameObjects it is using
	// as bones, and mark them as bones before we export. It is
	// the only way to know which objects are bones or not
	void FindBones(){
		// Go through all the objects in the FlatObjList
		for(int i=0; i<FlatObjList.Count; i++){
			// If it is a SkinnedMeshRender
			if(FlatObjList[i].Type != ObjType.SkinnedMesh) continue;
			
			// Get the SkinnedMeshRenderer component
			SkinnedMeshRenderer sm = FlatObjList[i].UnityObject.gameObject.GetComponent<SkinnedMeshRenderer>();
			
			// Go through every bone of the SkinnedMeshRenderer
			for(int b=0; b<sm.bones.Length; b++){
				// Now go through every object in the FlatObjList again
				for(int i2=0; i2<FlatObjList.Count; i2++){
					if(FlatObjList[i2].UnityObject == sm.bones[b]) FlatObjList[i2].Type = ObjType.Bone;
				}
			}			
		}
	}
	
	// --------------------------------------------------
	// Count types of MayaObjects
	// --------------------------------------------------
	// Once we have all the selected objects converted to MayaObjects, go
	// through the FlatList and count how many of each type of object there is.
	// We do this because some nodes require the total count of certain types 
	// of objects to be defined
	void CountTypesOfObjects(){
		// First check if the object already exists in our FlatObjList
		for(int i=0; i<FlatObjList.Count; i++){
			if(FlatObjList[i].Type == ObjType.Mesh) NumberOfMeshes++;
			if(FlatObjList[i].Type == ObjType.SkinnedMesh) NumberOfSkinnedMeshes++;
			if(FlatObjList[i].Type == ObjType.Bone) NumberOfBones++;
			if(FlatObjList[i].Type == ObjType.Terrain) NumberOfTerrains++;
			if(FlatObjList[i].Type == ObjType.Light) NumberOfLights++;
		}
		
		Debug.Log("Node Counts:\nMeshes: " + NumberOfMeshes +
					"\nSkinnedMeshes: " + NumberOfSkinnedMeshes +
					"\nBones: " + NumberOfBones +
					"\nTerrains: " + NumberOfTerrains +
					"\nLights: " + NumberOfLights);
	}
	
	
	#region MayaObject Connections
	// This will go through the SelectedObjects list and the FlatObjList
	// and fill out the connection data between them (parents and children)
	void ConnectMayaObjects(){
		// Go through selected objects
		for(int i=0; i<SelectedObjects.Length; i++){
			// Recursively connect children of MayaObject
			ConnectMayaObjectChildren(Selection.gameObjects[i].transform);
			
			// Recursively connect parents of MayaObject
			ConnectMayaObjectParents(Selection.gameObjects[i].transform);
		}
	}
	
	// Recursively connect child MayaObjects to given MayaObject.
	// Also connect the parents of each object
	void ConnectMayaObjectChildren(Transform t){
		// Get this transform MayaObject index
		int MayaObjIndex = GetMayaObjectFromTransform(t);
		
		// Get the child count of the transform
		int ChildCount = t.childCount;
		
		// Go through each child and find the MayaObject in the FlatObjList
		// and add the MayaObjects as children of this MayaObject
		//
		// Note - We are only getting the immediate children
		for(int i=0; i<ChildCount; i++){
			// Get child index
			int ChildMayaObjIndex = GetMayaObjectFromTransform(t.GetChild(i));
			
			// Add Child MayaObject to this MayaObject if it does not 
			// already exist
			bool ChildFound = false;
			for(int c=0; c<FlatObjList[MayaObjIndex].Children.Count; c++){
				if(FlatObjList[MayaObjIndex].Children[c].UnityObject == FlatObjList[ChildMayaObjIndex].UnityObject){
					ChildFound = true;
					break;
				}
			}
			// If we didn't find it, then add it
			if(!ChildFound){
				FlatObjList[MayaObjIndex].Children.Add(FlatObjList[ChildMayaObjIndex]);
			}
		}
		
		// Get the parent MayaObject index of this transform
		if(t.parent != null){
			int ParentIndex = GetMayaObjectFromTransform(t.parent);
			// Add the parent MayaObject to this MayaObject
			FlatObjList[MayaObjIndex].Parent = FlatObjList[ParentIndex];
		}
		
		// Recursively process the children now
		foreach(Transform child in t){
			ConnectMayaObjectChildren(child);
		}
	}
	
	// Recursively connect parent MayaObject to given MayaObject
	void ConnectMayaObjectParents(Transform t){
		// If this object has a parent
		if(t.parent != null){
			// Get this transform MayaObject index
			int MayaObjIndex = GetMayaObjectFromTransform(t);
			
			// Get parent transform MayaObject index
			int ParentMayaObjIndex = GetMayaObjectFromTransform(t.parent);
			
			// Add the parent MayaObject to this MayaObject
			FlatObjList[MayaObjIndex].Parent = FlatObjList[ParentMayaObjIndex];
			
			// Add this MayaObject as child to parent, if it doesn't exist already
			bool AlreadyExists = false;
			for(int i=0; i<FlatObjList[ParentMayaObjIndex].Children.Count; i++){
				if(FlatObjList[ParentMayaObjIndex].Children[i].UnityObject == t){
					AlreadyExists = true;
					break;
				}
			}
			if(!AlreadyExists){
				FlatObjList[ParentMayaObjIndex].Children.Add(FlatObjList[MayaObjIndex]);
			}
		}
		
		// Recursively process the parents now
		if(t.parent != null){
			ConnectMayaObjectParents(t.parent);
		}
	}
	
	// Given a transform, this will return an index into the FlatObjList
	// of the corresponding MayaObject
	// 
	// Note - It will return -1 if it couldn't find it
	int GetMayaObjectFromTransform(Transform t){
		int Index = -1;
		for(int i=0; i<FlatObjList.Count; i++){
			// If we found the MayaObject
			if(FlatObjList[i].UnityObject == t){
				Index = i;
				break;
			}
		}
		return Index;
	}
	#endregion
	
	#region Rename MayaObjects
	// --------------------------------------------------
	// Rename MayaObjects
	// --------------------------------------------------
	// Given a linked list of MayaObjects, this will go though and
	// make sure all names are unique by registering each name into the MayaName
	// list. If a name clash occurs, then the name will be prefixed with a number
	void AssignMayaName(MayaObject m){
		// --------------------------------------------------
		// Clean the object name
		// --------------------------------------------------
		string CleanedName =MayaUtilities.CleanName(m.UnityObject.name);
				
		// --------------------------------------------------
		// Check for name clashes
		// --------------------------------------------------
		m.MayaName = RegisterName(CleanedName);
		
		// If there are children of this MayaObject then
		// recursively assign their names as well
		for(int c=0; c<m.Children.Count; c++){
			AssignMayaName(m.Children[c]);
		}
	}
	
	// --------------------------------------------------
	// Register Name
	// --------------------------------------------------
	// Given a string name, this will search the MayaName List
	// and see if it was registered already. If it was, it 
	// increments the counter for that name. If not then it
	// registers the name
	string RegisterName(string name){
		// Check the name against our list of names
		bool NameFound = false;
		for(int i=0; i<MayaNameList.Count; i++){
			// If the object name was already registered
			if(name == MayaNameList[i].Name){
				NameFound = true;
				MayaNameList[i].Count++;
				
				// Set the name to the incremented name
				name = MayaNameList[i].GetName();
				break;
			}
		}
		// If we did not find the name, register it
		if(!NameFound){
			MayaName mn = new MayaName(name);
			MayaNameList.Add(mn);
		}
		
		// Return the name
		return name;
	}
	#endregion
	
	#region Transforms
	// --------------------------------------------------
	// Process Transform
	// --------------------------------------------------
	// Given a MayaObject, this will query all the transform data and record it
	void ProcessTransform(MayaObject MayaObj){
		// If the MayaObj is a Bone, do Bone export (bones dont have shapes, or transforms, only bones)
		if(MayaObj.Type == ObjType.Bone){
			ProcessBone(MayaObj);
		}
		// Otherwise do normal Transform export
		else{
			string data = "";
			
			// Update progress bar
			EditorUtility.DisplayProgressBar(ProgressTitle, string.Format(ProgressMsg, MayaObj.MayaName, "Transform"), CurProgress/MaxProgress);
				
			// ------------------------------
			// First process the transform
			// ------------------------------
			// Get the local translation as Maya translation
			Vector3 translate = MayaUtilities.MayaTranslation(MayaObj.UnityObject.localPosition) * MayaExportScale;
			
			// Get the local rotation as Maya rotation
			Vector3 rotate = MayaUtilities.MayaRotation(MayaObj.UnityObject.localRotation.eulerAngles);
			// LIGHT CHECK - Unity lights point down positive Z, while Maya lights point down negative Z
			// So we have to check if its a light and if it is, adjust the rotation
			if(MayaObj.Type == ObjType.Light){
				Vector3 NewRotate = MayaObj.UnityObject.localRotation.eulerAngles;
				NewRotate.x += 180;
				rotate = MayaUtilities.MayaRotation(NewRotate);
			}
			
			// Get the local scale
			Vector3 scale = MayaObj.UnityObject.localScale;
			
			// If transform has no parent
			if(MayaObj.Parent == null) data += "createNode transform -n \"" + MayaObj.MayaName + "\";\n";
			else data += "createNode transform -n \"" + MayaObj.MayaName + "\" -p \"" + MayaObj.Parent.MayaName + "\";\n";
			
			// Add transformation data
			data += "\tsetAttr \".t\" -type \"double3\"" + translate.x + " " + translate.y + " " + translate.z + ";\n";
			data += "\tsetAttr \".r\" -type \"double3\"" + rotate.x + " " + rotate.y + " " + rotate.z + ";\n";
			data += "\tsetAttr \".s\" -type \"double3\"" + scale.x + " " + scale.y + " " + scale.z + ";\n";
			
			// Set rotation order to ZXY instead of XYZ
			data += "\tsetAttr \".ro\" 2;\n";
			
			// Write data to file
			AppendToFile(data);
			data = "";
			
			// --------------------------------------------------
			// Process Shape?
			// --------------------------------------------------
			// If the shape is set to process, handle it
			if(MayaObj.Type == ObjType.Mesh) MeshUtilities.ProcessMesh(MayaObj, MayaObj.MayaName, (MayaObj.MayaName + "Shape"), false);
		//	if(MayaObj.Type == ObjType.SkinnedMesh) MeshUtilities.ProcessSkinnedMesh(MayaObj);
			if(MayaObj.Type == ObjType.Light) ProcessLight(MayaObj);
			//if(MayaObj.Type == ObjType.Terrain) ProcessTerrain(MayaObj);
			
			// --------------------------------------------------
			// Recursive Child Search
			// --------------------------------------------------
			foreach(MayaObject child in MayaObj.Children){
				ProcessTransform(child);
			}
			
			// --------------------------------------------------
			// Progress Bar update
			// --------------------------------------------------
			CurProgress += 1;
		}
	}
	#endregion

	
	
	
	#region Bones
	void ProcessBone(MayaObject MayaObj){
		string data = "";
		
		// Get bone parent
		string boneParent = "";
		if(MayaObj.Parent != null) boneParent = " -p \"" + MayaObj.Parent.MayaName + "\"";
		data += "createNode joint -n \"" + MayaObj.MayaName + "\"" + boneParent + ";\n";
		
		// Write data to file
		AppendToFile(data);
		data = "";
		
		// --------------------------------------------------
		// Recursive Child Search
		// --------------------------------------------------
		foreach(MayaObject child in MayaObj.Children){
			ProcessTransform(child);
		}
		
		// --------------------------------------------------
		// Progress Bar update
		// --------------------------------------------------
		CurProgress += 1;
	}	
	#endregion
	
	
	
	#region Lights
	// --------------------------------------------------
	// Process Light
	// --------------------------------------------------
	// Given a MayaObject, this will query all the light
	// data, format it to Maya conventions, and write it
	// to the file
	void ProcessLight(MayaObject MayaObj){
		string data = "";
		
		// Get a reference to the light component of the MayaObject
		Light L = MayaObj.UnityObject.gameObject.GetComponent<Light>();
			
		// Update progress bar
		EditorUtility.DisplayProgressBar(ProgressTitle, string.Format(ProgressMsg, MayaObj.MayaName, "Getting Light Data"), CurProgress/MaxProgress);
		
		// Determine which type of light it is (spot, directional, point, area)
		string lightType = "";
		if(L.type == LightType.Spot) lightType = "spot";
		if(L.type == LightType.Directional) lightType = "directional";
		if(L.type == LightType.Point) lightType = "point";
		if(L.type == LightType.Area) lightType = "area";
		
		// Get light color
		Color LightColor = L.color;
		
		// Get light intensity
		float LightIntensity = L.intensity;
		
		// Get shadows ON / OFF
		string LightShadow = "on";
		if(L.shadows == LightShadows.None) LightShadow = "off";		
		
		// Update progress bar
		EditorUtility.DisplayProgressBar(ProgressTitle, string.Format(ProgressMsg, MayaObj.MayaName, "Writing Light Data"), CurProgress/MaxProgress);
		
		// --------------------------------------------------
		// Write shape attributes
		// --------------------------------------------------
		data += "createNode " + lightType + "Light -n \"" + MayaObj.MayaName + "Shape\" -p \"" + MayaObj.MayaName + "\";\n";
		data += "\tsetAttr -k off \".v\";\n";
		data += "\tsetAttr \".cl\" -type \"float3\" " + LightColor.r + " " + LightColor.g + " " + LightColor.b + ";\n";
		data += "\tsetAttr \".in\" " + LightIntensity + ";\n";
		data += "\tsetAttr \".urs\" " + LightShadow + ";\n";
		// Set decay rate to linear for all lights except spotlight
		if(lightType != "directional"){
			data += "\tsetAttr \".de\" 1;\n";
		}
		if(lightType == "spot"){
			// Set cone angle
			data += "\tsetAttr \".ca\" " + L.spotAngle + ";\n";
			// Set penumbra angle
			data += "\tsetAttr \".pa\" 5;\n";
		}
		
		// --------------------------------------------------
		// Write Maya Connections
		// --------------------------------------------------
		MayaConnections += "connectAttr \"" + MayaObj.MayaName + "Shape.ltd\" \":lightList1.l\" -na;\n";
		MayaConnections += "connectAttr \"" + MayaObj.MayaName + ".iog\" \":defaultLightSet.dsm\" -na;\n";
		
		// Write data to file
		AppendToFile(data);
		data = "";
	}
	#endregion
	
	#region DisplayLayers
	void DisplayLayerExport(){
		string data = "";
		
		data += "createNode displayLayerManager -n \"layerManager\";\n";
			string LayerOrder = "";
			for(int i=0; i<DisplayLayers.Count; i++){
				LayerOrder += (i+1) + " ";
			}
			data += "\tsetAttr -s " + DisplayLayers.Count + " \".dli[" + (DisplayLayers.Count > 1 ? "1:" : "") + (DisplayLayers.Count) + "]\" " + LayerOrder + ";\n";
			data += "\tsetAttr -s " + DisplayLayers.Count + " \".dli\";\n";
		data += "connectAttr \"layerManager.dli[0]\" \"defaultLayer.id\";\n";
			
		// Write data to file
		AppendToFile(data);
		data = "";
		
		// Write out each display layer and connections
		for(int i=0; i<DisplayLayers.Count; i++){
			data += "createNode displayLayer -n \"Lightmap_Layer_" + i + "\";\n";
				data += "\tsetAttr \".do\" " + (i+1) + ";\n";
			for(int j=0; j<DisplayLayers[i].Objects.Count; j++){
				data += "connectAttr \"Lightmap_Layer_" + i + ".di\" \"" + DisplayLayers[i].Objects[j].MayaName + ".do\";\n";
			}
			data += "connectAttr \"layerManager.dli[" + (i+1) + "]\" \"Lightmap_Layer_" + i + ".id\";\n";
			
			// Write data to file
			AppendToFile(data);
			data = "";
		}
	}
	#endregion
	
	#region MayaMaterials
	// --------------------------------------------------
	// Build MayaMaterial List
	// --------------------------------------------------
	// This will go through and create MayaMaterial equivalents
	// of the Materials it finds on all the MayaObjects in the FlatObjList
	void BuildMaterialList(){
		// Go through each MayaObject
		for(int i=0; i<FlatObjList.Count; i++){
			// Perform checks before operating on the MayaObj
			if(FlatObjList[i].Type != ObjType.Mesh) continue;

			// Get materials
			Material[] mats = FlatObjList[i].UnityObject.gameObject.GetComponent<Renderer>().sharedMaterials;
			
			// Go through each material and check if we already have it in
			// our MayaMaterials list. If we don't, then add it
			bool MatFound = false;
			for(int m=0; m<mats.Length; m++){
				for(int c=0; c<MayaMaterials.Count; c++){
					// If we found the material
					if(MayaMaterials[c].UnityMaterial == mats[m]){
						MatFound = true;
						break;
					}
				}
				
				// If we did not find the material
				if(!MatFound){
					// Create the material
					MayaMaterial NewMat = new MayaMaterial();
					
					// Clean the name
					// Note - For some reason the materials names have (Instance) in
					// it. If I find a better way to get the real name, we can remove
					// the part that strips it off.
					string NewName = MayaUtilities.CleanName(mats[m].name.Replace(" (Instance)",""));
					
					// Register the name
					NewName = RegisterName(NewName);
					
					// Set the MayaMaterial variables
					NewMat.MayaName = NewName;
					NewMat.UnityMaterial = mats[m];
					NewMat.MaterialInfo = "materialInfo"+MaterialInfoCounter;
					MaterialInfoCounter++;
					
					// Add the material to the list
					MayaMaterials.Add(NewMat);
				}
			}
		}
	}
	
	// --------------------------------------------------
	// Start Material Export
	// --------------------------------------------------
	// This will write out the lightLinker, renderPartition
	// and defaultShaderList. We need these when dealing with
	// materials
	void StartMaterialExport(){
		string data = "";
		
		// --------------------------------------------------
		// Create light linker
		// --------------------------------------------------
		// Note - We set these values to the number of materials + 1
		// (or in this case, we can just use the size of the MayaMaterials list)
		// because the initial particleCloud1 shader gets included into this list
		data += "createNode lightLinker -s -n \"lightLinker1\";\n";
			data += "\tsetAttr -s " + MayaMaterials.Count + " \".lnk\";\n";
			data += "\tsetAttr -s " + MayaMaterials.Count + " \".slnk\";\n";

		// --------------------------------------------------
		// Set render partition size
		// --------------------------------------------------
		// Note - Same thing as light linker
		data += "select -ne :renderPartition;\n";
			data += "\tsetAttr -s " + MayaMaterials.Count + " \".st\";\n";
			
		// --------------------------------------------------
		// Set default shader list size
		// --------------------------------------------------
		// Note - Same thing as light linker
		data += "select -ne :defaultShaderList1;\n";
			data += "\tsetAttr -s " + MayaMaterials.Count + " \".s\";\n";	
	
		// Write data to file
		AppendToFile(data);
		data = "";
	}
	#endregion
	
	#region Textures
	// --------------------------------------------------
	// Build Texture List
	// --------------------------------------------------
	// This will go through the materials list and build a
	// file texture list from it
	void BuildTextureList(){
		// Go through each MayaMaterial
		for(int i=0; i<MayaMaterials.Count; i++){
			// _MainTex
			Texture MainTex = MayaMaterials[i].UnityMaterial.GetTexture("_MainTex");
			if(MainTex){
				// Check that this texture doesn't already exist in our list
				bool found = false;
				for(int t=0; t<MayaTextures.Count; t++){
					if(MayaTextures[t].UnityTexture == MainTex){
						found = true;
						
						// Add a link to this texture on the material
						MayaMaterials[i].MainTex = MayaTextures[t];
						
						break;
					}
				}
				// If it doesn't then make it
				if(!found){
					// Get current path to texture
					string currentTexturePath = GetFullTexturePath(MainTex);
					
					// Get texture asset name
					string assetName = GetTextureName(MainTex);
				
					// Create MayaFileTexture
					MayaFileTexture mayaTexture = new MayaFileTexture(MainTex, NewFileTexture(), NewPlace2DTexture(), currentTexturePath, filePath + assetName);
					
					// Add texture tiling
					mayaTexture.Tiling = MayaMaterials[i].UnityMaterial.GetTextureScale("_MainTex");
					
					// Add texture offset
					mayaTexture.Offset = MayaMaterials[i].UnityMaterial.GetTextureOffset("_MainTex");

					// Add the MayaFileTexture to our list
					MayaTextures.Add(mayaTexture);
					
					// Add a link to this texture on the material
					MayaMaterials[i].MainTex = mayaTexture;
					
					// Increment file texture counter
					FileTextureCounter++;
				}
			}
		}
	}
	
	// --------------------------------------------------
	// Get Full Texture Path
	// --------------------------------------------------
	// Given a Texture, this will return the full file path
	string GetFullTexturePath(Texture t){
		// Get the application data path
		// Note - This will be a path to the assets folder
		string dataPath = Application.dataPath;
		string[] tokens = dataPath.Split('/');
		dataPath = "";
		// Go through and rebuild path
		// Note - We go through Length - 1 since we want
		// to strip off the extra Assets token
		for(int i=0; i<tokens.Length - 1; i++){
			dataPath += tokens[i] + "/";
		}
		
		// Get the local path
		// Note - this will be a path FROM the assets folder
		// to the texture
		string localPath = AssetDatabase.GetAssetPath(t);
		
		// Return the full texture path
		return (dataPath + localPath);
	}
	
	// --------------------------------------------------
	// Get Texture Name
	// --------------------------------------------------
	// Given a Texture, this will return the name of the texture asset
	string GetTextureName(Texture t){
		// Get the local path
		// Note - this will be a path FROM the assets folder
		// to the texture
		string localPath = AssetDatabase.GetAssetPath(t);
		
		// Split the string
		string[] tokens = localPath.Split('/');
		
		return tokens[tokens.Length - 1];
	}
	
	// --------------------------------------------------
	// Start Texture Export
	// --------------------------------------------------
	// This will write out the defaultRenderUtilityList and set
	// the size of it to our number of File Textures + Bump Map Nodes
	void StartTextureExport(){
		string data = "";
		
		data += "select -ne :defaultTextureList1;\n";

		// The defaultRenderUtilityList contains all texture placement nodes
		// as well as all bump nodes. Set this value to the total combined number of
		// these nodes
		data += "select -ne :defaultRenderUtilityList1;\n";
			data += "\tsetAttr -s " + MayaTextures.Count + " \".u\";\n";
	
		// Write data to file
		AppendToFile(data);
		data = "";
	}
	#endregion
	
	#region File Writing
	// --------------------------------------------------
	// Begin writing to a file
	// --------------------------------------------------
	// Note - First we pass the file name to the StreamWriter and tell it to NOT
	// append data. This will then erase any contents that were previously inside
	// the file
	void StartNewFile(){
		// Erase the file contents
		using (StreamWriter writer = new StreamWriter(filePath + fileName, false)){
			writer.Write("");
		}
		
		string data = "";
		
		// --------------------------------------------------
		// Adds Maya Scene File Header Info
		// --------------------------------------------------
		// This is required for any Maya Scene File.
		// This is also where you specify which version of Maya
		// the scene file should open on
		data += "//Maya ASCII " + MayaVersions[MayaVersionIndex] + " scene\n";
		data += "requires maya \"" + MayaVersions[MayaVersionIndex] + "\";\n";
		data += "currentUnit -l " + MayaUnits[MayaUnitsIndex] + " -a degree -t film;\n";
		data += "fileInfo \"application\" \"maya\";\n";
		
		// --------------------------------------------------
		// Adds Maya Default Cameras (persp, front, top, side)
		// --------------------------------------------------
		// You don't NEED this, but if you don't the organization in the Outliner
		// becomes very confusing, with all the objects listed before the cameras.
		// This will force the cameras to be created first and makes the objects
		// show up in the correct order
		data += "createNode transform -s -n \"persp\";\n";
			data += "\tsetAttr \".v\" no;\n";
			data += "\tsetAttr \".t\" -type \"double3\" 57 43 57 ;\n";
			data += "\tsetAttr \".r\" -type \"double3\" -28.076862662266123 44.999999999999986 8.9959671327898901e-015 ;\n";
		data += "createNode camera -s -n \"perspShape\" -p \"persp\";\n";
			data += "\tsetAttr -k off \".v\" no;\n";
			data += "\tsetAttr \".fl\" 34.999999999999993;\n";
			data += "\tsetAttr \".fcp\" 1000;\n";
			data += "\tsetAttr \".coi\" 91.361917668140052;\n";
			data += "\tsetAttr \".imn\" -type \"string\" \"persp\";\n";
			data += "\tsetAttr \".den\" -type \"string\" \"persp_depth\";\n";
			data += "\tsetAttr \".man\" -type \"string\" \"persp_mask\";\n";
			data += "\tsetAttr \".hc\" -type \"string\" \"viewSet -p %camera\";\n";
		data += "createNode transform -s -n \"top\";\n";
			data += "\tsetAttr \".v\" no;\n";
			data += "\tsetAttr \".t\" -type \"double3\" 0 100.1 0 ;\n";
			data += "\tsetAttr \".r\" -type \"double3\" -89.999999999999986 0 0 ;\n";
		data += "createNode camera -s -n \"topShape\" -p \"top\";\n";
			data += "\tsetAttr -k off \".v\" no;\n";
			data += "\tsetAttr \".rnd\" no;\n";
			data += "\tsetAttr \".fcp\" 1000;\n";
			data += "\tsetAttr \".coi\" 100.1;\n";
			data += "\tsetAttr \".ow\" 30;\n";
			data += "\tsetAttr \".imn\" -type \"string\" \"top\";\n";
			data += "\tsetAttr \".den\" -type \"string\" \"top_depth\";\n";
			data += "\tsetAttr \".man\" -type \"string\" \"top_mask\";\n";
			data += "\tsetAttr \".hc\" -type \"string\" \"viewSet -t %camera\";\n";
			data += "\tsetAttr \".o\" yes;\n";
		data += "createNode transform -s -n \"front\";\n";
			data += "\tsetAttr \".v\" no;\n";
			data += "\tsetAttr \".t\" -type \"double3\" 0 0 100.1 ;\n";
		data += "createNode camera -s -n \"frontShape\" -p \"front\";\n";
			data += "\tsetAttr -k off \".v\" no;\n";
			data += "\tsetAttr \".rnd\" no;\n";
			data += "\tsetAttr \".fcp\" 1000;\n";
			data += "\tsetAttr \".coi\" 100.1;\n";
			data += "\tsetAttr \".ow\" 30;\n";
			data += "\tsetAttr \".imn\" -type \"string\" \"front\";\n";
			data += "\tsetAttr \".den\" -type \"string\" \"front_depth\";\n";
			data += "\tsetAttr \".man\" -type \"string\" \"front_mask\";\n";
			data += "\tsetAttr \".hc\" -type \"string\" \"viewSet -f %camera\";\n";
			data += "\tsetAttr \".o\" yes;\n";
		data += "createNode transform -s -n \"side\";\n";
			data += "\tsetAttr \".v\" no;\n";
			data += "\tsetAttr \".t\" -type \"double3\" 100.1 0 0 ;\n";
			data += "\tsetAttr \".r\" -type \"double3\" 0 89.999999999999986 0 ;\n";
		data += "createNode camera -s -n \"sideShape\" -p \"side\";\n";
			data += "\tsetAttr -k off \".v\" no;\n";
			data += "\tsetAttr \".rnd\" no;\n";
			data += "\tsetAttr \".fcp\" 1000;\n";
			data += "\tsetAttr \".coi\" 100.1;\n";
			data += "\tsetAttr \".ow\" 30;\n";
			data += "\tsetAttr \".imn\" -type \"string\" \"side\";\n";
			data += "\tsetAttr \".den\" -type \"string\" \"side_depth\";\n";
			data += "\tsetAttr \".man\" -type \"string\" \"side_mask\";\n";
			data += "\tsetAttr \".hc\" -type \"string\" \"viewSet -s %camera\";\n";
			data += "\tsetAttr \".o\" yes;\n";
			
		// Write data to file
		AppendToFile(data);
		data = "";
	}
	
	// --------------------------------------------------
	// Write to File - Append Data
	// --------------------------------------------------
	// When writing a file, this will append data to the file
	public static void AppendToFile(string s){
		using (StreamWriter writer = new StreamWriter(filePath + fileName, true)){
			writer.Write(s);
		}
	}
	#endregion
	
	#region Progress Bar Update
	// --------------------------------------------------
	// Update Progress Bar Message
	// --------------------------------------------------
	// Convienence method to easy update the text of the progress bar
	public static void ProgressBarMessage(string ObjName, string Message){
		// Update progress bar
		EditorUtility.DisplayProgressBar(ProgressTitle, string.Format(ProgressMsg, ObjName, Message), CurProgress / MaxProgress);
	}
	#endregion

	#region Mesh Debugger
	void MeshDebuger(){
		// Get the mesh data
		Mesh m = Selection.gameObjects[0].transform.GetComponent<MeshFilter>().sharedMesh;
		
		Vector3[] verts = m.vertices; 
		Vector2[] uvs = m.uv;
		Vector2[] uvs2 = m.uv2;
		Color[] colors = m.colors;
		Vector3[] normals = m.normals;
		int[] tris = m.triangles;
		
		Debug.Log("---------- Number of verts: " + verts.Length);
		Debug.Log("---------- Number of uvs: " + uvs.Length);
		Debug.Log("---------- Number of uvs2: " + uvs2.Length);
		Debug.Log("---------- Number of colors: " + colors.Length);
		Debug.Log("---------- Number of normals: " + normals.Length);
		Debug.Log("---------- Number of tris: " + tris.Length);
		
		for(int i=0; i<uvs2.Length; i++){
			Debug.Log("UV " + i + " Value: " + uvs2[i]);
		}
	}
	#endregion
}