using UnityEngine;
using System.Collections;

// --------------------------------------------------
// MayaEdge Class
// --------------------------------------------------
// Helper class for storing Maya Edge data
public class MayaEdge {
	public int StartEdge;
	public int EndEdge;
	
	// --------------------------------------------------
	// Constructor
	// --------------------------------------------------
	public MayaEdge(int A, int B){
		StartEdge = A;
		EndEdge = B;
	}
	
	// --------------------------------------------------
	// Given two indices, this will return if the edge matches
	// 0:	No Match
	// 1:	Match
	// -1:	Reversed Match
	// --------------------------------------------------
	public int Match(int A, int B){
		// Set match to false
		int result = 0;
		
		// Check for straight match
		if(A == StartEdge && B == EndEdge) result = 1;
		
		// Check for reversed match
		if(A == EndEdge && B == StartEdge) result = -1;
		
		// Return the match result
		return result;
	}
}