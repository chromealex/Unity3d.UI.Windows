using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI.Windows.Plugins.FlowCompiler;

/*
 * Template can contains:
 * 
 * {NAMESPACE_NAME}
 * {CLASS_NAME}
 * {BASE_CLASS_NAME}
 * {CLASS_NAME_WITH_NAMESPACE}
 */
using System.Text.RegularExpressions;
using UnityEngine.UI.Windows.Plugins.Flow;

namespace UnityEngine.UI.Windows.Plugins.FlowCompiler {

	public static class FlowTemplateGenerator {

		public static string ReplaceText(string text, Tpl.Info oldInfo, Tpl.Info newInfo) {

			var file = Resources.Load("UI.Windows/Templates/TemplateReplaceRules") as TextAsset;
			if (file == null) {
				
				Debug.LogError("Template Loading Error: Could not load template 'TemplateReplaceRules'");
				
				return text;

			}

			var path = AssetDatabase.GetAssetPath(file);
			var replaceRules = System.IO.File.ReadAllLines(path);

			//text = text.Replace(oldInfo.baseNamespace, newInfo.baseNamespace);
			
			for (int i = 0; i < replaceRules.Length; ++i) {
				
				// Prepare rule
				var replace = replaceRules[i];
				replace = replace.Replace(" ", @"\s?");
				replace = oldInfo.Replace(replace, (r) => r.Replace("{VARIABLE}", "([a-zA-Z]+[a-zA-Z0-9]*)"));
				
				var replacement = replaceRules[i];
				replacement = newInfo.Replace(replacement, (r) => r.Replace("{VARIABLE}", "$1"));
				
				var pattern = @"" + replace + "";
				
				var rgx = new Regex(pattern, RegexOptions.IgnoreCase);
				text = rgx.Replace(text, replacement);
				
			}
			
			return text;
			
		}

		public static string GenerateWindowLayoutBaseClass( string className, string classNamespace, string transitionMethods ) {

			var file = Resources.Load( "UI.Windows/Templates/TemplateBaseClass" ) as TextAsset;
			if ( file == null ) {

				Debug.LogError( "Template Loading Error: Could not load template 'TemplateBaseClass'" );

				return null;
			}

			return file.text.Replace( "{NAMESPACE_NAME}", classNamespace )
							.Replace( "{CLASS_NAME}", className )
							.Replace( "{TRANSITION_METHODS}", transitionMethods );
		}

		public static string GenerateWindowLayoutDerivedClass( string className, string baseClassName, string classNamespace ) {

			var file = Resources.Load( "UI.Windows/Templates/TemplateDerivedClass" ) as TextAsset;
			if ( file == null ) {

				Debug.LogError( "Template Loading Error: Could not load template 'TemplateDerivedClass'" );

				return null;
			}

			return file.text.Replace( "{NAMESPACE_NAME}", classNamespace )
							.Replace( "{CLASS_NAME}", className )
							.Replace( "{BASE_CLASS_NAME}", baseClassName );
		}
		
		public static string GenerateWindowLayoutTransitionMethod(FlowWindow from, FlowWindow to, string targetClassName, string targetClassNameWithNamespace) {
			
			var file = Resources.Load("UI.Windows/Templates/TemplateTransitionMethod") as TextAsset;
			if (file == null) {
				
				Debug.LogError("Template Loading Error: Could not load template 'TemplateTransitionMethod'");
				
				return null;

			}
			
			return file.text.Replace("{CLASS_NAME}", targetClassName)
							.Replace("{FLOW_FROM_ID}", from.id.ToString())
							.Replace("{FLOW_TO_ID}", to.id.ToString())
							.Replace("{CLASS_NAME_WITH_NAMESPACE}", targetClassNameWithNamespace);

		}
		
		public static string GenerateWindowLayoutTransitionMethodDefault() {
			
			var file = Resources.Load( "UI.Windows/Templates/TemplateDefaultTransitionMethod" ) as TextAsset;
			if ( file == null ) {
				
				Debug.LogError( "Template Loading Error: Could not load template 'TemplateDefaultTransitionMethod'" );
				
				return null;
			}
			
			return file.text;
		}
	}

}