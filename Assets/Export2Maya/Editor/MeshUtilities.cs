using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class MeshUtilities {
	// --------------------------------------------------
	// Pre-Processor
	// --------------------------------------------------
	//
	// This is a convenience method. Given a MayaObject it will determine if it should
	// process it as a mesh, or a skinned mesh. Since skinned meshes are identical to
	// regular meshes, just with added skinned mesh "stuff", we reuse the mesh code because
	// its long and involved. No sense in duplicating code.
	//public static void BeginExport(MayaObject MayaObj){
	//	// If it is a mesh
	//	if(MayaObj.Type == ObjType.Mesh) ProcessMesh(MayaObj, MayaObj.MayaName, MayaObj.MayaName + "Shape", false);
	//	
	//	// If it is a skinned mesh
	//	if(MayaObj.Type == ObjType.SkinnedMesh) ProcessSkinnedMesh(MayaObj);
	//}
	
	// --------------------------------------------------
	// Process Skinned Mesh
	// --------------------------------------------------
	// 
	//public static void ProcessSkinnedMesh(MayaObject MayaObj, bool ExportNormals, bool ExportUVs, bool ExportLightmapUVs, bool ExportVertexColors, bool ExportMaterials, bool ExportTextures, float ExportScale){
	public static void ProcessSkinnedMesh(MayaObject MayaObj){
		ProcessMesh(MayaObj, MayaObj.MayaName, (MayaObj.MayaName + "Shape"), false);
		ProcessMesh(MayaObj, MayaObj.MayaName, (MayaObj.MayaName + "ShapeOrig"), true);
	}
	
	// --------------------------------------------------
	// Process Mesh
	// --------------------------------------------------
	// Given a Mesh, this will query all the mesh data, 
	// format it to Maya conventions, and write it to the file.
	//
	// Note - We must supply the ShapeName here, instead of using
	// the MayaObject.MayaName. We need this sort of setup because 
	// we have to make 2 copies of the mesh data when creating 
	// skinned meshes, the meshShape and meshShapeOrig.
	//
	// Note - For skinned meshes, the MeshShapeOrig needs
	// intermediate object set to TRUE.
	//public static void ProcessMesh(MayaObject MayaObj, bool ExportNormals, bool ExportUVs, bool ExportLightmapUVs, bool ExportVertexColors, bool ExportMaterials, bool ExportTextures, float ExportScale){
	public static void ProcessMesh(MayaObject MayaObj, string ParentName, string ShapeName, bool IntermediateObj){
		string data = "";
		
		// Get a reference to the mesh of the MayaObject depending on
		// if the MayaObj is a Mesh or SkinnedMesh
		Mesh m = new Mesh();
		if(MayaObj.Type == ObjType.Mesh) m = MayaObj.UnityObject.gameObject.GetComponent<MeshFilter>().sharedMesh;
		if(MayaObj.Type == ObjType.SkinnedMesh) m = MayaObj.UnityObject.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
			
		// Update progress bar
		Export2Maya.ProgressBarMessage(ShapeName, "Getting Mesh Data");
		
		// --------------------------------------------------
		// Gather mesh data
		// --------------------------------------------------
		Vector3[] verts = m.vertices;
		Vector2[] uvs = m.uv;
		Vector2[] uvs2 = m.uv2;
		Color[] colors = m.colors;
		Vector3[] normals = m.normals;
		int[] tris = m.triangles;
		
		// --------------------------------------------------
		// Get the Lightmap tiling and offset if Lightmap UVs exist
		// --------------------------------------------------
		Vector4 tilingOffset = new Vector4();
//!!!		if(uvs2.Length > 0) tilingOffset = MayaObj.UnityObject.gameObject.renderer.lightmapTilingOffset;
			if(uvs2.Length > 0) tilingOffset = MayaObj.UnityObject.gameObject.GetComponent<Renderer>().lightmapScaleOffset;
		
		// --------------------------------------------------
		// Perform UV and UV2 Zero Value checks here. With the
		// Tree Creator, it makes lightmap UVs all with Zero values
		// which can mess up our calculations when applying
		// the tiling and offset
		//
		// So check and see if all the UVs have zero values, if so
		// then disable UV or UV2 export
		// --------------------------------------------------
		bool ZeroUV = true;
		bool ZeroUV2 = true;
		for(int i=0; i<uvs.Length; i++){
			if(uvs[i].x != 0){
				ZeroUV = false;
				break;
			}
			if(uvs[i].y != 0){
				ZeroUV = false;
				break;
			}
		}
		for(int i=0; i<uvs2.Length; i++){
			if(uvs2[i].x != 0){
				ZeroUV2 = false;
				break;
			}
			if(uvs2[i].y != 0){
				ZeroUV2 = false;
				break;
			}
		}

		// --------------------------------------------------
		// Write shape attributes
		// --------------------------------------------------
		data += "createNode mesh -n \"" + ShapeName + "\" -p \"" + ParentName + "\";\n";
		data += "\tsetAttr -k off \".v\";\n";
		if(IntermediateObj) data += "\tsetAttr \".io\" yes;\n";	// Set intermediate object flag to true (for skinned meshes)
		data += "\tsetAttr \".vir\" yes;\n";
		data += "\tsetAttr \".vif\" yes;\n";
		
		// Write data to file
		Export2Maya.AppendToFile(data);
		data = "";
		
		// --------------------------------------------------
		// If the user has chosen to export materials
		// --------------------------------------------------
		if(Export2Maya.ExportMaterials){
			// --------------------------------------------------
			// Sub Mesh - Per face material assignment
			// --------------------------------------------------
			// If the sub mesh count is greater than 1, we have
			// per face material assignment. If this is the case, write
			// out the per face assignments
			int SubMeshCount = m.subMeshCount;
			if(SubMeshCount > 1){
				// Set the instObjGroup size to the number of sub meshes
				data += "\tsetAttr -s " + SubMeshCount + " \".iog[0].og\";\n";
				
				// Go through each sub mesh and add its face assignments
				int TotalSubMeshTris = 0;
				for(int i=0; i<SubMeshCount; i++){
					// Get the sub mesh triangle count
					int[] SubTriangles = m.GetTriangles(i);
					data += "\tsetAttr \".iog[0].og[" + i + "].gcl\" -type \"componentList\" 1 \"f[" + ((SubTriangles.Length / 3) > 1 ? (TotalSubMeshTris.ToString() + ":") : "") + (((SubTriangles.Length / 3) - 1) + TotalSubMeshTris) + "]\";\n";
					
					// Increment TotalSubMeshTris
					TotalSubMeshTris += SubTriangles.Length / 3;
				}		
			}
			
			// --------------------------------------------------
			// Material Assignment
			// --------------------------------------------------
			// Get material(s) list for the mesh
			Material[] mats = MayaObj.UnityObject.gameObject.GetComponent<Renderer>().sharedMaterials;

			// If the sub mesh count is greater than 1, then we need to handle
			// per-face assignment
			if(SubMeshCount > 1){
				// Go through sub meshes
				for(int i=0; i<SubMeshCount; i++){
					// Find the material in our MayaMaterials list
					for(int j=0; j<Export2Maya.MayaMaterials.Count; j++){
						// Note - User Error Check !!
						// If you import the mesh and manually alter the array count in inspector, this could result
						// in the submesh count being greater than the materials count. Which is bad. In this case we
						// create a new index that is based on Submesh count (i) and check if its greater than materials count.
						// if it is, we clamp it to the materials count.
						int ind = i;
						if(ind>=mats.Length) ind=mats.Length-1;
						
						// If we found the material
						if(Export2Maya.MayaMaterials[j].UnityMaterial == mats[ind]){
							// Increment GroupID counter
							Export2Maya.GroupIDCounter++;
							
							// Create GroupID node
							Export2Maya.MayaConnections += "createNode groupId -n \"groupId" + Export2Maya.GroupIDCounter + "\";\n";
							Export2Maya.MayaConnections +=		"\tsetAttr \".ihi\" 0;\n";		// Is historically interesting
							
							// Connect groupID.id > mesh.instObjGroups
							Export2Maya.MayaConnections += "connectAttr \"groupId" + Export2Maya.GroupIDCounter + ".id\" \"" + ShapeName + ".iog.og[" + i + "].gid\";\n";

							// Connect groupID.message > SG.groupNodes
							Export2Maya.MayaConnections += "connectAttr \"groupId" + Export2Maya.GroupIDCounter + ".msg\" \"" + Export2Maya.MayaMaterials[j].MayaName + "SG.gn\" -na;\n";
							
							// Connect SG.memberWireframeColor > mesh.instObjGroups
							Export2Maya.MayaConnections += "connectAttr \"" + Export2Maya.MayaMaterials[j].MayaName + "SG.mwc\" \"" + ShapeName + ".iog.og[" + i + "].gco\";\n";
							
							// Connect mesh.instObjGroups > SG.dagSetMembers
							Export2Maya.MayaConnections += "connectAttr \"" + ShapeName + ".iog.og[" + i + "]\" \"" + Export2Maya.MayaMaterials[j].MayaName + "SG.dsm\" -na;\n";
							
							// Since we found the material, break out of the loop
							break;
						}			
					}		
				}
			}
			// If the sub mesh count is 1, then we handle
			// per-object assignment
			else{
				// Find the material in our MayaMaterials list
				for(int mat=0; mat<Export2Maya.MayaMaterials.Count; mat++){
					// If we found the material
					if(Export2Maya.MayaMaterials[mat].UnityMaterial == mats[0]){
						Export2Maya.MayaConnections += "connectAttr \"" + ShapeName + ".iog\" \"" + Export2Maya.MayaMaterials[mat].MayaName + "SG.dsm\" -na;\n";
						break;
					}
				}
			}
		}
		// --------------------------------------------------
		// If the user has chosen to not export materials
		// --------------------------------------------------
		else{
			// Just assign the default lambert1 material to the object
			Export2Maya.MayaConnections += "connectAttr \"" + ShapeName + ".iog\" \":initialShadingGroup.dsm\" -na;\n";
		}
		
		// --------------------------------------------------
		// Build UV, UV2, Color and Vertex lists
		// --------------------------------------------------
		// Note - From what I've seen, the vertex list, the UV list, the UV2 list and the colors list are
		// always the same size. So as an optimization lets just pick the vertex list as our iterator and
		// fill out all the data in 1 go to save time
		//
		// The only exception is the normals since Maya stores normals per-vertex per-face, where as the
		// other lists get referenced by the face definitions
		string uvData = "";
		string uv2Data = "";
		string colorData = "";
		string vertData = "";
		
		// Update progress bar
		Export2Maya.ProgressBarMessage(ShapeName, "UVs, Colors, Vertices");
		
		// Column counter used for nicely formatting the data in the Maya file
		// NOTE - We also use it below for the edge list as well
		int ColumnCounter = 0;

		for(int i=0; i<verts.Length; i++){
			// Format UV data
			if(uvs.Length > 0){
				if(ColumnCounter == 0) uvData += "\t\t";
				uvData += uvs[i].x + " " + uvs[i].y + " ";
				if(ColumnCounter == Export2Maya.ColumnDataWidth) uvData += "\n";
			}
			
			// Format UV2 data
			if(uvs2.Length > 0){
				if(ColumnCounter == 0) uv2Data += "\t\t";
				uv2Data += ((uvs2[i].x * tilingOffset.x) + tilingOffset.z) + " " + ((uvs2[i].y * tilingOffset.y) + tilingOffset.w) + " ";
				if(ColumnCounter == Export2Maya.ColumnDataWidth) uv2Data += "\n";
			}
			
			// Format color data
			if(colors.Length > 0){
				if(ColumnCounter == 0) colorData += "\t\t";
				colorData += colors[i].r + " " + colors[i].g + " " + colors[i].b + " " + colors[i].a + " ";
				if(ColumnCounter == Export2Maya.ColumnDataWidth) colorData += "\n";
			}
	
			// Format vertex data
			if(ColumnCounter == 0) vertData += "\t\t";
			Vector3 MayaVert = MayaUtilities.MayaTranslation(verts[i]) * Export2Maya.MayaExportScale; // Multiply by export scale
			vertData += (MayaVert.x + " " + MayaVert.y + " " + MayaVert.z + " ");
			if(ColumnCounter == Export2Maya.ColumnDataWidth) vertData += "\n";
			
			// Increment column counter
			ColumnCounter++;
			if(ColumnCounter > Export2Maya.ColumnDataWidth) ColumnCounter = 0;
		}

		// --------------------------------------------------
		// Write UV data
		// --------------------------------------------------
		if(Export2Maya.ExportUVs && !ZeroUV){
			if(uvs.Length > 0){
				data += "\tsetAttr \".uvst[0].uvsn\" -type \"string\" \"map1\";\n";
				data += "\tsetAttr -s " + verts.Length + " \".uvst[0].uvsp[" + (verts.Length > 1 ? "0:" : "") + (verts.Length - 1) + "]\" -type \"float2\" \n" + uvData + ";\n";
				data += "\tsetAttr  \".cuvs\" -type \"string\" \"map1\";\n";	// Set the current uv set to the main uv set
			
				// Write data to file
				Export2Maya.AppendToFile(data);
				data = "";	
			}
		}
	
		// --------------------------------------------------
		// Write UV2 data
		// --------------------------------------------------
		if(Export2Maya.ExportLightmapUVs && !ZeroUV2){
			if(uvs2.Length > 0){
				data += "\tsetAttr \".uvst[1].uvsn\" -type \"string\" \"lightmap\";\n";
				data += "\tsetAttr -s " + verts.Length + " \".uvst[1].uvsp[" + (verts.Length > 1 ? "0:" : "") + (verts.Length - 1) + "]\" -type \"float2\" \n" + uv2Data + ";\n";
				
				// Write data to file
				Export2Maya.AppendToFile(data);
				data = "";
			}
		}
			
		// --------------------------------------------------
		// Write Color data ( RGBA )
		// --------------------------------------------------
		if(Export2Maya.ExportVertexColors){
			if(colors.Length > 0){
				// Display vertex colors on file load? ON
				data += "\tsetAttr \".dcol\" yes;\n";
				// Set which color channel to display (Ambient + Diffuse)
				data += "\tsetAttr \".dcc\" -type \"string\" \"Ambient+Diffuse\";\n";
				// Set the current color set
				data += "\tsetAttr \".ccls\" -type \"string\" \"colorSet1\";\n";
				data += "\tsetAttr \".clst[0].clsn\" -type \"string\" \"colorSet1\";\n";
				
				data += "\tsetAttr -s " + verts.Length + " \".clst[0].clsp[" + (verts.Length > 1 ? "0:" : "") + (verts.Length - 1) + "]\" " + colorData + ";\n";
				
				// Write data to file
				Export2Maya.AppendToFile(data);
				data = "";
			}
		}

		// Write data to file
		data += "\tsetAttr -s " + verts.Length + " \".vt[" + (verts.Length > 1 ? "0:" : "") + (verts.Length - 1) + "]\" \n" + vertData + ";\n";
		Export2Maya.AppendToFile(data);
		data = "";
		
		// --------------------------------------------------
		// Edge Connections
		// --------------------------------------------------
		// Since Unity stores all polygons as triangles, this will be easy.
		// We will have 3 possible edge connections per face:
		// edgeA:(vert0->vert1) edgeB:(vert1->vert2) edgeC:(vert2->vert0)
		// Note - We have to check that the edge doesn't already exist in our
		// local list before storing the edge. No duplicates allowed!
		List<MayaEdge> EdgeList = new List<MayaEdge>();		// Local edge list
		bool EdgeExists = false;
		
		// Edge Index list:
		// We will fill this guy out as we go so its ready when writing the
		// polygon face data. This will be a list of indices that point to our
		// local edge list which describe the edges of a face
		List<int> EdgeIndexList = new List<int>();
		
		// Update progress bar
		Export2Maya.ProgressBarMessage(ShapeName, "Edge List");
		
		// Go through every triangle (3 verts)
		for(int v=0; v<tris.Length; v+=3){
			// Edge A
			EdgeExists = false;	// Reset exists value
			for(int i=0; i<EdgeList.Count; i++){
				if(EdgeList[i].Match(tris[v], tris[v+1]) != 0){
					EdgeExists = true;
					EdgeIndexList.Add(i);
					break;
				}
			}
			if(!EdgeExists){
				EdgeList.Add(new MayaEdge(tris[v], tris[v+1]));
				EdgeIndexList.Add(EdgeList.Count - 1);
			}
			
			// Edge B
			EdgeExists = false;	// Reset exists value
			for(int i=0; i<EdgeList.Count; i++){
				if(EdgeList[i].Match(tris[v+1], tris[v+2]) != 0){
					EdgeExists = true;
					EdgeIndexList.Add(i);
					break;
				}
			}
			if(!EdgeExists){
				EdgeList.Add(new MayaEdge(tris[v+1], tris[v+2]));
				EdgeIndexList.Add(EdgeList.Count - 1);
			}
			
			// Edge C
			EdgeExists = false;	// Reset exists value
			for(int i=0; i<EdgeList.Count; i++){
				if(EdgeList[i].Match(tris[v+2], tris[v]) != 0){
					EdgeExists = true;
					EdgeIndexList.Add(i);
					break;
				}
			}
			if(!EdgeExists){
				EdgeList.Add(new MayaEdge(tris[v+2], tris[v]));
				EdgeIndexList.Add(EdgeList.Count - 1);
			}
		}
		
		// --------------------------------------------------
		// Combine edges into single string
		// --------------------------------------------------
		string EdgesStr = "\n";
		ColumnCounter = 0;
		for(int i=0; i<EdgeList.Count; i++){
			if(ColumnCounter == 0) EdgesStr += "\t\t";
			EdgesStr += (EdgeList[i].StartEdge + " " + EdgeList[i].EndEdge + " 0 ");
			if(ColumnCounter == Export2Maya.ColumnDataWidth) EdgesStr += "\n";
						
			// Increment column counter
			ColumnCounter++;
			if(ColumnCounter > Export2Maya.ColumnDataWidth) ColumnCounter = 0;
		}
		
		// Write data to file
		data += "\tsetAttr -s " + EdgeList.Count + " \".ed[" + (EdgeList.Count > 1 ? "0:" : "") + (EdgeList.Count - 1) + "]\" " + EdgesStr + ";\n";
		Export2Maya.AppendToFile(data);
		data = "";
		
		// --------------------------------------------------
		// Normals
		// --------------------------------------------------
		// The way Unity specifies the normals list is 1 normal per vertex
		// entry. But we need normals per-vertex per-face. So we go through the 
		// triangles list and find what vertices make up the face. We then use 
		// that to index into the normals array to find the normals per face
		if(Export2Maya.ExportNormals){
			// Update progress bar
			Export2Maya.ProgressBarMessage(ShapeName, "Normals");

			string NormalsStr = "";
			for(int v=0; v<tris.Length; v+=3){
				// Get the normals and convert them into Maya translation
				Vector3 normalA = MayaUtilities.MayaTranslation(normals[tris[v]]);
				Vector3 normalB = MayaUtilities.MayaTranslation(normals[tris[v+1]]);
				Vector3 normalC = MayaUtilities.MayaTranslation(normals[tris[v+2]]);
			
				// Not sure why this works, but we have to flip normal C and normal B for them to match
				// correctly in Maya. Weird
				NormalsStr += ("\t\t" + normalA.x + " " + normalA.y + " " + normalA.z + " ");
				NormalsStr += (normalC.x + " " + normalC.y + " " + normalC.z + " ");
				NormalsStr += (normalB.x + " " + normalB.y + " " + normalB.z);
				
				// Add a return character if not the last entry into the normals list,
				// this way the last one has the semicolon next to the number instead of the next line
				if(v+3 < tris.Length - 1) NormalsStr += "\n";
			}
			
			// Write data to file
			data += "\tsetAttr -s " + tris.Length + " \".n[0:" + (tris.Length - 1) + "]\" -type \"float3\"\n" + NormalsStr + ";\n";
			Export2Maya.AppendToFile(data);
			data = "";
		}

		// --------------------------------------------------
		// Faces
		// --------------------------------------------------
		// Now we need to tell Maya which edges make up each face. In Unity you can
		// specify the faces simply by giving 3 indexes into the vertex array, and it will build the 
		// face from that, but Maya is different. Maya needs edges specified in order to define a face.
		// So for each triangle go through the edges list and find the corresponding edges that
		// match the triangle vertices
		Export2Maya.ProgressBarMessage(ShapeName, "Faces");
		
		// --------------------------------------------------
		// 1 or more faces check!
		// --------------------------------------------------
		// For an object that has more than 1 face, Maya will list the range
		// of faces like so: [0:N] 
		// BUT! if there are objects with just 1 face, Maya will list the range
		// like so: [0]
		// So do a check here and make sure the correct format is used
		string faceFormat = ""; if((tris.Length / 3) > 1) faceFormat = "0:";
		data += "\tsetAttr -s " + (tris.Length / 3) + " \".fc[" + faceFormat + ((tris.Length / 3) - 1) + "]\" -type \"polyFaces\"\n";
		for(int i=0; i<tris.Length; i+=3){
			// --------------------------------------------------
			// Record the polygon face-edge data
			// --------------------------------------------------
			// NOTE! We reverse the order of the edge indices, from ABC to CBA
			// because Maya uses a counter-clockwise winding order for faces and Unity
			// gives us the data in clockwise winding order
			data += "\t\tf 3 " + EdgeIndexList[i+2] + " " + EdgeIndexList[i+1] + " " + EdgeIndexList[i];

			// Record the main UV data per face, if it exists and is requested.
			// Note - We don't completely reverse the order, but swap the second and 
			// last values so it displays correctly in Maya
			if(Export2Maya.ExportUVs){
				if(uvs.Length > 0) data += " mu 0 3 " + tris[i] + " " + tris[i+2] + " " + tris[i+1];
			}
			
			// Record the lightmap UV data per face, if it exists and is requested.
			// Same swapping mechanism as the main UV data
			if(Export2Maya.ExportLightmapUVs){
				if(uvs2.Length > 0) data += " mu 1 3 " + tris[i] + " " + tris[i+2] + " " + tris[i+1];
			}
		
			// Record vertex color per face, if it exists and is requested
			if(Export2Maya.ExportVertexColors){
				if(colors.Length > 0) data += " mc 0 3 " + tris[i] + " " + tris[i+2] + " " + tris[i+1];
			}

			data += "\n";
		}
		// Add trailing semicolon after face setup
		data += ";\n";
		
		// Write data to file
		Export2Maya.AppendToFile(data);
		data = "";
	}

	public static string SkinnedMeshToMel(MayaObject MayaObj){
		string data = "";
		return data;
	}
}
