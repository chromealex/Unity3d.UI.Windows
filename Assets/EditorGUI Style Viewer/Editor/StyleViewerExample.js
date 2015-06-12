class StyleViewerExample extends EditorWindow{
	
	var popup : int = 0;
	var toggles : boolean[] = new boolean[5];
	
	@MenuItem("Window/Style Viewer Example")
	
	static function Init(){
		
		var window : StyleViewerExample = EditorWindow.GetWindow(StyleViewerExample);
	}
	
	function OnGUI(){
	
		GUILayout.Label("  Options","LargeLabel");
		
		GUILayout.BeginVertical("HelpBox");
		GUILayout.BeginHorizontal();
		
		EditorGUILayout.TextField("Search","","ToolbarSeachTextField");
		GUILayout.Button("Close","ToolbarSeachCancelButton");
		
		GUILayout.Space(20);
		
		GUILayout.Button("minibuttonleft","minibuttonleft");
		GUILayout.Button("minibuttonright","minibuttonright");
		
		GUILayout.EndHorizontal();
		
		GUILayout.Space(10);
		if(GUILayout.Button("Advanced","MiniToolbarButton")) toggles[2] = !toggles[2];
		if(toggles[2]){
			
			GUILayout.BeginVertical("ProgressBarBack");
			GUILayout.BeginHorizontal();
			
			GUILayout.BeginVertical();
			GUILayout.Label("WhiteMiniLabel","WhiteMiniLabel");
			GUILayout.Label("ErrorLabel","ErrorLabel");
			GUILayout.EndVertical();
			
			GUILayout.BeginVertical("GroupBox");
			
			GUILayout.BeginHorizontal();
			GUILayout.Button("OL Plus","OL Plus");
			GUILayout.Button("OL Minus","OL Minus");
			GUILayout.EndHorizontal();
			GUILayout.Space(10);
			
			GUILayout.Button("OL Title","OL Title");
			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			GUILayout.Button("OL Titleleft","OL Titleleft");
			GUILayout.Button("OL Titlemid","OL Titlemid");
			GUILayout.Button("OL Titleright","OL Titleright");
			GUILayout.EndHorizontal();
			
			GUILayout.EndVertical();
			
			GUILayout.EndHorizontal();
			GUILayout.Space(20);
			GUILayout.EndVertical();
			
			GUILayout.BeginHorizontal("GroupBox");
			GUILayout.Label("CN EntryInfo","CN EntryInfo");
			GUILayout.Label("CN EntryWarn","CN EntryWarn");
			GUILayout.Label("CN EntryError","CN EntryError");
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
	}
}