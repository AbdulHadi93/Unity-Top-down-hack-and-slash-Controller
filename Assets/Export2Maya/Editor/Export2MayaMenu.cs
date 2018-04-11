using UnityEngine;
using UnityEditor;
 
public class Export2MayaMenu {
    [MenuItem("Window/Export2Maya")]
    private static void LaunchUI(){
        // Get existing open window, if none then make a new one
        Export2Maya window = (Export2Maya)EditorWindow.GetWindow(typeof(Export2Maya), false, "Export2Maya");
		window.position = new Rect((Screen.width / 2) - 150, (Screen.height) / 2 + 150, 250, 300);
        window.Show();
    }
}