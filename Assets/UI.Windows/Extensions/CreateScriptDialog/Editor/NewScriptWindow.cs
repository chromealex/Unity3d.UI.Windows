using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Object = UnityEngine.Object;

namespace UnityEditor.UI.Windows.Extensions.Utilities {

	public class NewScriptWindow : EditorWindow {
		private const int kButtonWidth = 120;
		private const int kLabelWidth = 85;
		private const string kLanguageEditorPrefName = "NewScriptLanguage";
		private const string kTemplatePath = "CreateScriptDialog/SmartScriptTemplates";
		private const string kResourcesTemplatePath = "Resources/SmartScriptTemplates";
		private const string kMonoBehaviourName = "MonoBehaviour";
		private const string kPlainClassName = "Empty Class";
		private const string kCustomEditorClassName = "Editor";
		private const string kTempEditorClassPrefix = "E:";
		private const string kNoTemplateString = "No Template Found";
		// char array can't be const for compiler reasons but this should still be treated as such.
		private char[] kInvalidPathChars = new char[] { '<', '>', ':', '"', '|', '?', '*', (char)0 };
		private char[] kPathSepChars = new char[] { '/', '\\' };
		
		private ScriptPrescription m_ScriptPrescription;
		private string m_BaseClass;
		private string m_CustomEditorTargetClassName = string.Empty;
		private bool m_IsEditorClass = false;
		private bool m_IsCustomEditor = false;
		private bool m_FocusTextFieldNow = true;
		private GameObject m_GameObjectToAddTo;
		private string m_Directory = string.Empty;
		private Vector2 m_PreviewScroll;
		private Vector2 m_OptionsScroll;
		private bool m_ClearKeyboardControl = false;
		
		private int m_TemplateIndex;
		private string[] m_TemplateNames;

		class Styles {
			public GUIContent m_WarningContent = new GUIContent(string.Empty,
			                                                     EditorGUIUtility.LoadRequired("Builtin Skins/Icons/console.warnicon.sml.png") as Texture2D);
			public GUIStyle m_PreviewBox = new GUIStyle("OL Box");
			public GUIStyle m_PreviewTitle = new GUIStyle("OL Title");
			public GUIStyle m_LoweredBox = new GUIStyle("TextField");
			public GUIStyle m_HelpBox = new GUIStyle("helpbox");

			public Styles() {
				m_LoweredBox.padding = new RectOffset(1, 1, 1, 1);
			}
		}

		private static Styles m_Styles;

		private string GetAbsoluteBuiltinTemplatePath() {
			return Path.Combine(EditorApplication.applicationContentsPath, kResourcesTemplatePath);
		}

		private string GetAbsoluteCustomTemplatePath() {
			return Path.Combine(Application.dataPath, kTemplatePath);
		}

		private void FillTemplates(List<string> templates) {

			var assets = AssetDatabase.FindAssets("t:TextAsset");
			foreach (var asset in assets) {

				var path = AssetDatabase.GUIDToAssetPath(asset);
				var locs = Path.GetDirectoryName(path).Split('/');
				if (locs.Length > 0) {

					var loc = locs[locs.Length - 1];
					if (loc.ToLower() == "smartscripttemplates") {

						templates.Add(path);

					}

				}

			}

		}

		public string GetTemplateText(string nameWithoutExtension) {

			var assets = AssetDatabase.FindAssets("t:TextAsset");
			foreach (var asset in assets) {

				var path = AssetDatabase.GUIDToAssetPath(asset);
				if (Path.GetFileNameWithoutExtension(path) == nameWithoutExtension + "." + extension) {

					var locs = Path.GetDirectoryName(path).Split('/');
					if (locs.Length > 0) {

						var loc = locs[locs.Length - 1];
						if (loc.ToLower() == "smartscripttemplates") {

							return File.ReadAllText(path);

						}

					}

				}

			}

			return string.Empty;

		}

		public string GetFunctionsPath(string baseClassName) {

			var assets = AssetDatabase.FindAssets("t:TextAsset");
			foreach (var asset in assets) {

				var path = AssetDatabase.GUIDToAssetPath(asset);
				if (Path.GetFileNameWithoutExtension(path) == baseClassName + ".functions") {

					var locs = Path.GetDirectoryName(path).Split('/');
					if (locs.Length > 0) {

						var loc = locs[locs.Length - 1];
						if (loc.ToLower() == "smartscripttemplates") {

							return path;

						}

					}

				}

			}

			return string.Empty;

		}

		private void UpdateTemplateNamesAndTemplate() {
			// Remember old selected template name
			string oldSelectedTemplateName = null;
			if (m_TemplateNames != null && m_TemplateNames.Length > 0) oldSelectedTemplateName = m_TemplateNames[m_TemplateIndex];
			
			// Get new template names
			m_TemplateNames = GetTemplateNames();
			
			// Select template
			if (m_TemplateNames.Length == 0) {
				m_ScriptPrescription.m_Template = kNoTemplateString;
				m_BaseClass = null;
			} else {
				if (oldSelectedTemplateName != null && m_TemplateNames.Contains(oldSelectedTemplateName)) m_TemplateIndex = m_TemplateNames.ToList().IndexOf(oldSelectedTemplateName);
				else m_TemplateIndex = 0;
				m_ScriptPrescription.m_Template = GetTemplate(m_TemplateNames[m_TemplateIndex]);
			}
			
			HandleBaseClass();
		}

		private void AutomaticHandlingOnChangeTemplate() {
			// Add or remove "Editor" from directory path
			if (m_IsEditorClass) {
				if (InvalidTargetPathForEditorScript()) m_Directory = Path.Combine(m_Directory, "Editor");
			} else if (m_Directory.EndsWith("Editor")) {
				m_Directory = m_Directory.Substring(0, m_Directory.Length - 6).TrimEnd(kPathSepChars);
			}
			
			// Move keyboard focus to relevant field
			if (m_IsCustomEditor) m_FocusTextFieldNow = true;
		}

		private string GetBaseClass(string templateContent) {
			string firstLine = templateContent.Substring(0, templateContent.IndexOf("\n"));
			if (firstLine.Contains("BASECLASS")) {
				string baseClass = firstLine.Substring(10).Trim();
				if (baseClass != string.Empty) return baseClass;
			}
			return null;
		}

		private bool GetFunctionIsIncluded(string baseClassName, string functionName, bool includeByDefault) {
			string prefName = "FunctionData_" + (baseClassName != null ? baseClassName + "_" : string.Empty) + functionName;
			return EditorPrefs.GetBool(prefName, includeByDefault);
		}

		private void SetFunctionIsIncluded(string baseClassName, string functionName, bool include) {
			string prefName = "FunctionData_" + (baseClassName != null ? baseClassName + "_" : string.Empty) + functionName;
			EditorPrefs.SetBool(prefName, include);
		}

		private void HandleBaseClass() {
			if (m_TemplateNames.Length == 0) {
				m_BaseClass = null;
				return;
			}
			
			// Get base class
			m_BaseClass = GetBaseClass(m_ScriptPrescription.m_Template);
			
			// If base class was found, strip first line from template
			if (m_BaseClass != null) m_ScriptPrescription.m_Template =
					m_ScriptPrescription.m_Template.Substring(m_ScriptPrescription.m_Template.IndexOf("\n") + 1);
			
			m_IsEditorClass = IsEditorClass(m_BaseClass);
			m_IsCustomEditor = (m_BaseClass == kCustomEditorClassName);
			m_ScriptPrescription.m_StringReplacements.Clear();
			
			// Try to find function file first in custom templates folder and then in built-in
			//string functionDataFilePath = Path.Combine(GetAbsoluteCustomTemplatePath(), m_BaseClass + ".functions.txt");
			//if (!File.Exists(functionDataFilePath)) functionDataFilePath = Path.Combine(GetAbsoluteBuiltinTemplatePath(), m_BaseClass + ".functions.txt");
			var functionDataFilePath = GetFunctionsPath(m_BaseClass);

			if (!File.Exists(functionDataFilePath)) {
				m_ScriptPrescription.m_Functions = null;
			} else {
				StreamReader reader = new StreamReader(functionDataFilePath);
				List<FunctionData> functionList = new List<FunctionData>();
				int lineNr = 1;
				while (!reader.EndOfStream) {
					string functionLine = reader.ReadLine();
					string functionLineWhole = functionLine;
					try {
						if (functionLine.Substring(0, 7).ToLower() == "header ") {
							functionList.Add(new FunctionData(functionLine.Substring(7)));
							continue;
						}
						
						FunctionData function = new FunctionData();
						
						bool defaultInclude = false;
						if (functionLine.Substring(0, 8) == "DEFAULT ") {
							defaultInclude = true;
							functionLine = functionLine.Substring(8);
						}
						
						if (functionLine.Substring(0, 9) == "override ") {
							function.isVirtual = true;
							functionLine = functionLine.Substring(9);
						}
						
						string returnTypeString = GetStringUntilSeperator(ref functionLine, " ");
						function.returnType = (returnTypeString == "void" ? null : returnTypeString);
						function.name = GetStringUntilSeperator(ref functionLine, "(");
						string parameterString = GetStringUntilSeperator(ref functionLine, ")");
						if (function.returnType != null) function.returnDefault = GetStringUntilSeperator(ref functionLine, ";");
						function.comment = functionLine;
						
						string[] parameterStrings = parameterString.Split(new char[]{ ',' }, StringSplitOptions.RemoveEmptyEntries);
						List<ParameterData> parameterList = new List<ParameterData>();
						for (int i = 0; i < parameterStrings.Length; i++) {
							string[] paramSplit = parameterStrings[i].Trim().Split(' ');
							parameterList.Add(new ParameterData(paramSplit[1], paramSplit[0]));
						}
						function.parameters = parameterList.ToArray();
						
						function.include = GetFunctionIsIncluded(m_BaseClass, function.name, defaultInclude);
						
						functionList.Add(function);
					} catch (Exception e) {
						Debug.LogWarning("Malformed function line: \"" + functionLineWhole + "\"\n  at " + functionDataFilePath + ":" + lineNr + "\n" + e);
					}
					lineNr++;
				}
				m_ScriptPrescription.m_Functions = functionList.ToArray();
				
			}
		}

		private string GetStringUntilSeperator(ref string source, string sep) {
			int index = source.IndexOf(sep);
			string result = source.Substring(0, index).Trim();
			source = source.Substring(index + sep.Length).Trim(' ');
			return result;
		}

		private string GetTemplate(string nameWithoutExtension) {
			/*string path = Path.Combine(GetAbsoluteCustomTemplatePath(), nameWithoutExtension + "." + extension + ".txt");
			if (File.Exists(path)) return File.ReadAllText(path);

			path = Path.Combine(GetAbsoluteBuiltinTemplatePath(), nameWithoutExtension + "." + extension + ".txt");
			if (File.Exists(path)) return File.ReadAllText(path);
*/
			var txt = GetTemplateText(nameWithoutExtension);
			if (string.IsNullOrEmpty(txt) == false) {

				return txt;

			}

			return kNoTemplateString;
		}

		private string GetTemplateName() {
			if (m_TemplateNames.Length == 0) return kNoTemplateString;
			return m_TemplateNames[m_TemplateIndex];
		}
		
		// Custom comparer to sort templates alphabetically,
		// but put MonoBehaviour and Plain Class as the first two
		private class TemplateNameComparer : IComparer<string> {
			private int GetRank(string s) {
				if (s == kMonoBehaviourName) return 0;
				if (s == kPlainClassName) return 1;
				if (s.StartsWith(kTempEditorClassPrefix)) return 100;
				return 2;
			}

			public int Compare(string x, string y) {
				int rankX = GetRank(x);
				int rankY = GetRank(y);
				if (rankX == rankY) return x.CompareTo(y);
				else return rankX.CompareTo(rankY);
			}
		}

		private string[] GetTemplateNames() {
			List<string> templates = new List<string>();
			
			// Get all file names of custom templates
			//if (Directory.Exists(GetAbsoluteCustomTemplatePath())) templates.AddRange(Directory.GetFiles(GetAbsoluteCustomTemplatePath()));
			
			// Get all file names of built-in templates
			//if (Directory.Exists(GetAbsoluteBuiltinTemplatePath())) templates.AddRange(Directory.GetFiles(GetAbsoluteBuiltinTemplatePath()));

			FillTemplates(templates);

			if (templates.Count == 0) return new string[0];
			
			// Filter and clean up list
			templates = templates
				.Distinct()
				.Where(f => (f.EndsWith("." + extension + ".txt")))
				.Select(f => Path.GetFileNameWithoutExtension(f.Substring(0, f.Length - 4)))
				.ToList();
			
			// Determine which scripts have editor class base class
			for (int i = 0; i < templates.Count; i++) {
				string templateContent = GetTemplate(templates[i]);
				if (IsEditorClass(GetBaseClass(templateContent))) templates[i] = kTempEditorClassPrefix + templates[i];
			}
			
			// Order list
			templates = templates
				.OrderBy(f => f, new TemplateNameComparer())
				.ToList();
			
			// Insert separator before first editor script template
			bool inserted = false;
			for (int i = 0; i < templates.Count; i++) {
				if (templates[i].StartsWith(kTempEditorClassPrefix)) {
					templates[i] = templates[i].Substring(kTempEditorClassPrefix.Length);
					if (!inserted) {
						templates.Insert(i, string.Empty);
						inserted = true;
					}
				}
			}
			
			// Return list
			return templates.ToArray();
		}

		private string extension {
			get {
				switch (m_ScriptPrescription.m_Lang) {
					case Language.CSharp:
						return "cs";
				/*case Language.JavaScript:
						return "js";
					case Language.Boo:
						return "boo";*/
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		[MenuItem("Component/Scripts/New Script...", false, 0)]
		private static void OpenFromComponentMenu() {
			Init();
		}

		[MenuItem("Component/Scripts/New Script...", true, 0)]
		private static bool OpenFromComponentMenuValidation() {
			return (Selection.activeObject is GameObject);
		}

		[MenuItem("Assets/Create/Script...", false, 100)]
		private static void OpenFromAssetsMenu() {
			Init();
		}

		private static void Init() {
			GetWindow<NewScriptWindow>(true, "Create Script");
		}

		public NewScriptWindow() {
			// Large initial size
			position = new Rect(50, 50, 770, 500);
			// But allow to scale down to smaller size
			minSize = new Vector2(550, 400);
			
			m_ScriptPrescription = new ScriptPrescription();
		}

		private void OnEnable() {
			//m_ScriptPrescription.m_Lang = (Language)EditorPrefs.GetInt (kLanguageEditorPrefName, 0);
			UpdateTemplateNamesAndTemplate();
			OnSelectionChange();
		}

		private void OnGUI() {
			if (m_Styles == null) m_Styles = new Styles();
			
			EditorGUIUtilityExt.LookLikeControls(85, 0f);
			
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return && CanCreate()) Create();
			
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space(10);
				
				PreviewGUI();
				GUILayout.Space(10);
				
				EditorGUILayout.BeginVertical();
				{
					OptionsGUI();
					
					GUILayout.Space(10);
					//GUILayout.FlexibleSpace ();
					
					CreateAndCancelButtonsGUI();
				}
				EditorGUILayout.EndVertical();
				
				GUILayout.Space(10);
			}
			EditorGUILayout.EndHorizontal();
			
			GUILayout.Space(10);
			
			// Clear keyboard focus if clicking a random place inside the dialog,
			// or if ClearKeyboardControl flag is set.
			if (m_ClearKeyboardControl || (Event.current.type == EventType.MouseDown && Event.current.button == 0)) {
				GUIUtility.keyboardControl = 0;
				m_ClearKeyboardControl = false;
				Repaint();
			}
		}

		private bool CanCreate() {
			return m_ScriptPrescription.m_ClassName.Length > 0 &&
			!File.Exists(TargetPath()) &&
			!ClassAlreadyExists() &&
			!ClassNameIsInvalid() &&
			!TargetClassDoesNotExist() &&
			!TargetClassIsNotValidType() &&
			!InvalidTargetPath() &&
			!InvalidTargetPathForEditorScript();
		}

		private void Create() {
			CreateScript();

			//if (CanAddComponent()) InternalEditorUtility.AddScriptComponentUnchecked(m_GameObjectToAddTo,
			//	                                                   AssetDatabase.LoadAssetAtPath(TargetPath(), typeof(MonoScript)) as MonoScript);
			
			Close();
			GUIUtility.ExitGUI();
		}

		private void CreateAndCancelButtonsGUI() {
			bool canCreate = CanCreate();

			// Create string to tell the user what the problem is
			string blockReason = string.Empty;
			if (!canCreate && m_ScriptPrescription.m_ClassName != string.Empty) {
				if (File.Exists(TargetPath())) blockReason = "A script called \"" + m_ScriptPrescription.m_ClassName + "\" already exists at that path.";
				else if (ClassAlreadyExists()) blockReason = "A class called \"" + m_ScriptPrescription.m_ClassName + "\" already exists.";
				else if (ClassNameIsInvalid()) blockReason = "The script name may only consist of a-z, A-Z, 0-9, _.";
				else if (TargetClassDoesNotExist()) if (m_CustomEditorTargetClassName == string.Empty) blockReason = "Fill in the script component to make an editor for.";
				else blockReason = "A class called \"" + m_CustomEditorTargetClassName + "\" could not be found.";
				else if (TargetClassIsNotValidType()) blockReason = "The class \"" + m_CustomEditorTargetClassName + "\" is not of type UnityEngine.Object.";
				else if (InvalidTargetPath()) blockReason = "The folder path contains invalid characters.";
				else if (InvalidTargetPathForEditorScript()) blockReason = "Editor scripts should be stored in a folder called Editor.";
			}
			
			// Warning about why the script can't be created
			if (blockReason != string.Empty) {
				m_Styles.m_WarningContent.text = blockReason;
				GUILayout.BeginHorizontal(m_Styles.m_HelpBox);
				{
					GUILayout.Label(m_Styles.m_WarningContent, EditorStyles.wordWrappedMiniLabel);
				}
				GUILayout.EndHorizontal();
			}

			// Cancel and create buttons
			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Cancel", GUILayout.Width(kButtonWidth))) {
					Close();
					GUIUtility.ExitGUI();
				}
				
				bool guiEnabledTemp = GUI.enabled;
				GUI.enabled = canCreate;
				if (GUILayout.Button(GetCreateButtonText(), GUILayout.Width(kButtonWidth))) {
					Create();
				}
				GUI.enabled = guiEnabledTemp;
			}
			GUILayout.EndHorizontal();
		}

		private bool CanAddComponent() {
			return (m_GameObjectToAddTo != null && m_BaseClass == kMonoBehaviourName);
		}

		private void OptionsGUI() {
			EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
			{
				GUILayout.BeginHorizontal();
				{
					NameGUI();
					LanguageGUI();
				}
				GUILayout.EndHorizontal();
				
				GUILayout.Space(10);
				
				TargetPathGUI();
				
				GUILayout.Space(20);
				
				TemplateSelectionGUI();
				
				if (GetTemplateName() == kMonoBehaviourName) {
					GUILayout.Space(10);
					AttachToGUI();
				}
				
				if (m_IsCustomEditor) {
					GUILayout.Space(10);
					CustomEditorTargetClassNameGUI();
				}
				
				GUILayout.Space(10);
				
				FunctionsGUI();
			}
			EditorGUILayout.EndVertical();
		}

		private bool FunctionHeader(string header, bool expandedByDefault) {
			GUILayout.Space(5);
			bool expanded = GetFunctionIsIncluded(m_BaseClass, header, expandedByDefault);
			bool expandedNew = GUILayout.Toggle(expanded, header, EditorStyles.foldout);
			if (expandedNew != expanded) SetFunctionIsIncluded(m_BaseClass, header, expandedNew);
			return expandedNew;
		}

		private void FunctionsGUI() {
			if (m_ScriptPrescription.m_Functions == null) {
				GUILayout.FlexibleSpace();
				return;
			}

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Label("Functions", GUILayout.Width(kLabelWidth - 4));
				
				EditorGUILayout.BeginVertical(m_Styles.m_LoweredBox);
				m_OptionsScroll = EditorGUILayout.BeginScrollView(m_OptionsScroll);
				{
					bool expanded = FunctionHeader("General", true);
					
					for (int i = 0; i < m_ScriptPrescription.m_Functions.Length; i++) {
						FunctionData func = m_ScriptPrescription.m_Functions[i];
						
						if (func.name == null) {
							expanded = FunctionHeader(func.comment, false);
						} else if (expanded) {
							Rect toggleRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toggle);
							toggleRect.x += 15;
							toggleRect.width -= 15;
							bool include = GUI.Toggle(toggleRect, func.include, new GUIContent(func.name, func.comment));
							if (include != func.include) {
								m_ScriptPrescription.m_Functions[i].include = include;
								SetFunctionIsIncluded(m_BaseClass, func.name, include);
							}
						}
					}
				}
				EditorGUILayout.EndScrollView();
				EditorGUILayout.EndVertical();
				
			}
			EditorGUILayout.EndHorizontal();
		}

		private void AttachToGUI() {
			GUILayout.BeginHorizontal();
			{
				m_GameObjectToAddTo = EditorGUILayout.ObjectField("Attach to", m_GameObjectToAddTo, typeof(GameObject), true) as GameObject;

				if (ClearButton()) m_GameObjectToAddTo = null;
			}
			GUILayout.EndHorizontal();
			
			HelpField("Click a GameObject or Prefab to select.");
		}

		private void SetClassNameBasedOnTargetClassName() {
			if (m_CustomEditorTargetClassName == string.Empty) m_ScriptPrescription.m_ClassName = string.Empty;
			else m_ScriptPrescription.m_ClassName = m_CustomEditorTargetClassName + "Editor";
		}

		private void CustomEditorTargetClassNameGUI() {
			GUI.SetNextControlName("CustomEditorTargetClassNameField");
			
			string newName = EditorGUILayout.TextField("Editor for", m_CustomEditorTargetClassName);
			m_ScriptPrescription.m_StringReplacements["$TargetClassName"] = newName;
			if (newName != m_CustomEditorTargetClassName) {
				m_CustomEditorTargetClassName = newName;
				SetClassNameBasedOnTargetClassName();
			}
			
			if (m_FocusTextFieldNow && Event.current.type == EventType.repaint) {
				GUI.FocusControl("CustomEditorTargetClassNameField");
				m_FocusTextFieldNow = false;
				Repaint();
			}
			
			HelpField("Script component to make an editor for.");
		}

		private void TargetPathGUI() {

			var changedDir = false;

			GUI.enabled = false;
			var m_Directory = EditorGUILayout.TextField("Save in", this.m_Directory, GUILayout.ExpandWidth(true));
			if (m_Directory != this.m_Directory) {

				changedDir = true;

			}

			GUI.enabled = true;
			HelpField("Click a folder in the Project view to select.");

			EditorGUILayout.Space();
			m_ScriptPrescription.m_NameSpace = EditorGUILayout.TextField("Namespace", m_ScriptPrescription.m_NameSpace, GUILayout.ExpandWidth(true));

			var targetNamespace = m_ScriptPrescription.m_NameSpace;

			if (string.IsNullOrEmpty(m_ScriptPrescription.m_NameSpace) == true || changedDir == true) {

				var project = UnityEditor.UI.Windows.Plugins.Flow.Flow.FindProjectForPath("Assets", m_Directory);
				if (project != null) {

					var dir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(project)).Replace("Assets/", string.Empty).Replace("Assets\\", string.Empty);

					targetNamespace = m_Directory
						.Replace(dir + "/", string.Empty)
						.Replace(dir + "\\", string.Empty)
						.Replace("/", ".")
						.Replace("\\", ".");
					
				} else {

					targetNamespace = "Namespace";

				}

			}

			m_ScriptPrescription.m_StringReplacements["$Namespace"] = targetNamespace;
			m_ScriptPrescription.m_NameSpace = targetNamespace;

		}

		private bool ClearButton() {
			return GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.Width(40));
		}

		private void TemplateSelectionGUI() {
			m_TemplateIndex = Mathf.Clamp(m_TemplateIndex, 0, m_TemplateNames.Length - 1);
			var names = (string[])m_TemplateNames.Clone();
			for (int i = 0; i < names.Length; ++i) names[i] = names[i].Replace(" ", "/");
			int templateIndexNew = EditorGUILayout.Popup("Template", m_TemplateIndex, names);
			if (templateIndexNew != m_TemplateIndex) {
				m_TemplateIndex = templateIndexNew;
				UpdateTemplateNamesAndTemplate();
				AutomaticHandlingOnChangeTemplate();
			}
		}

		private void NameGUI() {
			GUI.SetNextControlName("ScriptNameField");
			m_ScriptPrescription.m_ClassName = EditorGUILayout.TextField("Name", m_ScriptPrescription.m_ClassName);
			
			if (m_FocusTextFieldNow && !m_IsCustomEditor && Event.current.type == EventType.repaint) {
				GUI.FocusControl("ScriptNameField");
				m_FocusTextFieldNow = false;
			}
		}

		private void LanguageGUI() {
			var langNew = (Language)EditorGUILayout.EnumPopup(m_ScriptPrescription.m_Lang, GUILayout.Width(80));

			if (langNew != m_ScriptPrescription.m_Lang) {
				m_ScriptPrescription.m_Lang = langNew;
				EditorPrefs.SetInt(kLanguageEditorPrefName, (int)langNew);
				UpdateTemplateNamesAndTemplate();
				AutomaticHandlingOnChangeTemplate();
			}
		}

		private void PreviewGUI() {
			EditorGUILayout.BeginVertical(GUILayout.Width(Mathf.Max(position.width * 0.4f, position.width - 380f)));
			{
				// Reserve room for preview title
				Rect previewHeaderRect = GUILayoutUtility.GetRect(new GUIContent("Preview"), m_Styles.m_PreviewTitle);
				
				// Secret! Toggle curly braces on new line when double clicking the script preview title
				Event evt = Event.current;
				if (evt.type == EventType.MouseDown && evt.clickCount == 2 && previewHeaderRect.Contains(evt.mousePosition)) {
					EditorPrefs.SetBool("CurlyBracesOnNewLine", !EditorPrefs.GetBool("CurlyBracesOnNewLine"));
					Repaint();
				}

				// Preview scroll view
				m_PreviewScroll = EditorGUILayout.BeginScrollView(m_PreviewScroll, m_Styles.m_PreviewBox);
				{
					EditorGUILayout.BeginHorizontal();
					{
						// Tiny space since style has no padding in right side
						GUILayout.Space(5);
			
						// Preview text itself
						string previewStr = new NewScriptGenerator(m_ScriptPrescription).ToString();
						Rect r = GUILayoutUtility.GetRect(
							         new GUIContent(previewStr),
							         EditorStyles.miniLabel,
							         GUILayout.ExpandWidth(true),
							         GUILayout.ExpandHeight(true));
						EditorGUI.SelectableLabel(r, previewStr, EditorStyles.miniLabel);
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndScrollView();

				// Draw preview title after box itself because otherwise the top row
				// of pixels of the slider will overlap with the title
				GUI.Label(previewHeaderRect, new GUIContent("Preview"), m_Styles.m_PreviewTitle);
				
				GUILayout.Space(4);
			}
			EditorGUILayout.EndVertical();
		}

		private bool InvalidTargetPath() {
			if (m_Directory.IndexOfAny(kInvalidPathChars) >= 0) return true;
			if (TargetDir().Split(kPathSepChars, StringSplitOptions.None).Contains(string.Empty)) return true;
			return false;
		}

		private bool InvalidTargetPathForEditorScript() {
			return m_IsEditorClass && !m_Directory.ToLower().Split(kPathSepChars).Contains("editor");
		}

		private bool IsFolder(Object obj) {
			return Directory.Exists(AssetDatabase.GetAssetPath(obj));
		}

		private void HelpField(string helpText) {
			GUILayout.BeginHorizontal();
			GUILayout.Label(string.Empty, GUILayout.Width(kLabelWidth - 4));
			GUILayout.Label(helpText, m_Styles.m_HelpBox);
			GUILayout.EndHorizontal();
		}

		private string TargetPath() {
			return Path.Combine(TargetDir(), m_ScriptPrescription.m_ClassName + "." + extension);
		}

		private string TargetDir() {
			return Path.Combine("Assets", m_Directory.Trim(kPathSepChars));
		}

		private bool ClassNameIsInvalid() {
			return !System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(
				m_ScriptPrescription.m_ClassName);
		}

		private bool ClassExists(string className) {
			return AppDomain.CurrentDomain.GetAssemblies()
				.Any(a => a.GetType(className, false) != null);
		}

		private bool ClassAlreadyExists() {
			if (m_ScriptPrescription.m_ClassName == string.Empty) return false;
			return ClassExists(m_ScriptPrescription.m_ClassName);
		}

		private bool TargetClassDoesNotExist() {
			if (!m_IsCustomEditor) return false;
			if (m_CustomEditorTargetClassName == string.Empty) return true;
			return !ClassExists(m_CustomEditorTargetClassName);
		}

		private bool TargetClassIsNotValidType() {
			if (!m_IsCustomEditor) return false;
			if (m_CustomEditorTargetClassName == string.Empty) return true;
			return AppDomain.CurrentDomain.GetAssemblies()
				.All(a => !typeof(UnityEngine.Object).IsAssignableFrom(a.GetType(m_CustomEditorTargetClassName, false)));
		}

		private string GetCreateButtonText() {
			return CanAddComponent() ? "Create and Attach" : "Create";
		}

		private void CreateScript() {
			if (!Directory.Exists(TargetDir())) Directory.CreateDirectory(TargetDir());

			var writer = new StreamWriter(TargetPath());
			writer.Write(new NewScriptGenerator(m_ScriptPrescription).ToString());
			writer.Close();
			writer.Dispose();
			AssetDatabase.Refresh();
		}

		private void OnSelectionChange() {
			m_ClearKeyboardControl = true;
			
			if (Selection.activeObject == null) return;
			
			if (IsFolder(Selection.activeObject)) {
				m_Directory = AssetPathWithoutAssetPrefix(Selection.activeObject);
				if (m_IsEditorClass && InvalidTargetPathForEditorScript()) {
					m_Directory = Path.Combine(m_Directory, "Editor");
				}
			} else if (Selection.activeGameObject != null) {
				m_GameObjectToAddTo = Selection.activeGameObject;
			} else if (m_IsCustomEditor && Selection.activeObject is MonoScript) {
				m_CustomEditorTargetClassName = Selection.activeObject.name;
				SetClassNameBasedOnTargetClassName();
			}
				
			Repaint();
		}

		private string AssetPathWithoutAssetPrefix(Object obj) {
			return AssetDatabase.GetAssetPath(obj).Substring(7);
		}

		private bool IsEditorClass(string className) {
			if (className == null) return false;
			return GetAllClasses("UnityEditor").Contains(className);
		}

		/// Method to populate a list with all the class in the namespace provided by the user
		static List<string> GetAllClasses(string nameSpace) {
			// Get the UnityEditor assembly
			Assembly asm = Assembly.GetAssembly(typeof(Editor));
			
			// Create a list for the namespaces
			List<string> namespaceList = new List<string>();
			
			// Create a list that will hold all the classes the suplied namespace is executing
			List<string> returnList = new List<string>();
			
			foreach (Type type in asm.GetTypes ()) {
				if (type.Namespace == nameSpace) namespaceList.Add(type.Name);
			}
			
			// Now loop through all the classes returned above and add them to our classesName list
			foreach (String className in namespaceList) returnList.Add(className);
			
			return returnList;
		}
	}

}