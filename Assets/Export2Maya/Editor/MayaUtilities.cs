using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// --------------------------------------------------
// MayaUtilities Class
// --------------------------------------------------
// Contains methods to convert translations and rotations between 
// coordinate systems, clean illegal characters out of Maya names, etc
public static class MayaUtilities {
	// --------------------------------------------------
	// Unity -> Maya Translation Conversion
	// --------------------------------------------------
	// Given a Vector3 translation, this will convert it to Maya translation
	public static Vector3 MayaTranslation(Vector3 t){
		return new Vector3(-t.x, t.y, t.z);
	}
	
	// --------------------------------------------------
	// Unity -> Maya Rotation Conversion
	// --------------------------------------------------
	// Given a Vector3 euler rotation, this will convert it to Maya rotation
	public static Vector3 MayaRotation(Vector3 r){
		return new Vector3(r.x, -r.y, -r.z);
	}
	
	// --------------------------------------------------
	// Clean Name
	// --------------------------------------------------
	// Given a string, this will remove illegal characters so
	// it fits within Maya naming conventions
	public static string CleanName(string name){
		// We have to strip out any illegal characters from the name
		// a-z A-Z 0-9 and _ are the only accepted characters
		List<string> CleanedName = new List<string>();
		
		// Convert the name into an array of char
		char[] array = name.ToCharArray();

		// We will be moving backwards through the name, removing numbers
		// and underscores. Once we find a letter, we stop removing numbers
		bool removeNumbers = true;
		for(int i=(array.Length - 1); i>-1; i--){
			if(char.IsLetter(array[i]) && array[i] != '_') removeNumbers = false;
			if(removeNumbers){
				if(char.IsUpper(array[i]) || char.IsLower(array[i])) CleanedName.Add(array[i].ToString());
			}
			else{
				if(char.IsLetterOrDigit(array[i]) || array[i] == '_') CleanedName.Add(array[i].ToString());
			}
		}
		// Since we went backwards through the name, it will be reversed. We will have
		// to reverse the result to make it correct
		CleanedName.Reverse();
		
		// Convert the char array into a string again
		return string.Join("", CleanedName.ToArray());	
	}
}
