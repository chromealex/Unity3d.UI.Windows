using UnityEngine;
using System.Collections;

public static class FlowTemplateGenerator {

	public static string GenerateWindowLayoutBaseClass( string className, string classNamespace, string transitionMethods ) {

		var file = Resources.Load( "UI.Windows/Templates/TemplateScreen" ) as TextAsset;
		if ( file == null ) {

			Debug.LogError( "Template Loading Error: Could not load template 'TemplateScreen'" );

			return null;
		}

		return file.text.Replace( "{NAMESPACE_NAME}", classNamespace )
						.Replace( "{CLASS_NAME}", className )
						.Replace( "{TRANSITION_METHODS}", transitionMethods );
	}

	public static string GenerateWindowLayoutTransitionMethod( string targetClassName, string targetClassNameWithNamespace ) {

		var file = Resources.Load( "UI.Windows/Templates/TemplateTransitionMethod" ) as TextAsset;
		if ( file == null ) {

			Debug.LogError( "Template Loading Error: Could not load template 'TemplateTransitionMethod'" );

			return null;
		}

		return file.text.Replace( "{CLASS_NAME}", targetClassName )
						.Replace( "{CLASS_NAME_WITH_NAMESPACE}", targetClassNameWithNamespace );
	}
}
