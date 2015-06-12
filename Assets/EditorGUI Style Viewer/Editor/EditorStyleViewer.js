class EditorStyleViewer extends EditorWindow{
	
	var scrollPosition = new Vector2(0,0);
	var search : String = "";
	
	@MenuItem("Window/Editor Style Viewer")
	
	static function Init(){
		
		var window : EditorStyleViewer = EditorWindow.GetWindow(EditorStyleViewer);
	}
	
	function OnGUI(){
		
		GUILayout.BeginHorizontal("HelpBox");
		GUILayout.Label("Click a Sample to copy its Name to your Clipboard","MiniBoldLabel");
		GUILayout.FlexibleSpace();
		GUILayout.Label("Search:");
		search = EditorGUILayout.TextField(search);
		
		GUILayout.EndHorizontal();
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		
		for(var style : GUIStyle in GUI.skin.customStyles){
			
			if(style.name.ToLower().Contains(search.ToLower())){
			GUILayout.BeginHorizontal("PopupCurveSwatchBackground");
			GUILayout.Space(7);
			if(GUILayout.Button(style.name,style)){
				
				EditorGUIUtility.systemCopyBuffer = "\""+style.name+"\"";
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.SelectableLabel("\""+style.name+"\"");
			GUILayout.EndHorizontal();
			GUILayout.Space(11);
			}
		}
		
		GUILayout.EndScrollView();
	}
}