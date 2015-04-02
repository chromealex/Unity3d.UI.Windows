using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CodeEditor.Text.UI.Unity.Editor.Implementation
{
	static class MissingEditorAPI
	{
		static Type inspectorStateType;

		public static bool ParentHasFocus(EditorWindow editorWindow)
		{
			return (bool)(PropertyOf(ParentOf(editorWindow), "hasFocus"));
		}
		
		public static bool IsVisible(EditorWindow editorWindow)
		{
			var parent = ParentOf(editorWindow);
			if (parent == null) return false;

			return ((EditorWindow)PropertyOf(parent, "actualView") == editorWindow);
		}
		
		public static void SwitchToPopup(this EditorWindow editorWindow) {

			//editorWindow.GetType().GetMethod("ShowWithMode").Invoke(editorWindow, ShowMode.PopupMenu);
			
		}
		
		public static void SwitchToUtility(this EditorWindow editorWindow) {
			
			//editorWindow.GetType().GetMethod("ShowWithMode").Invoke(editorWindow, ShowMode.Utility);
			
		}

		private static object ParentOf(EditorWindow editorWindow)
		{
			return FieldOf(editorWindow, "m_Parent");
		}

		private static object PropertyOf(object o, string propertyName)
		{
			var property = o.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property == null)
				throw new ArgumentException(string.Format("Can't find {0}.{1}", o, propertyName));
			return property.GetValue(o, null);
		}
		
		private static object FieldOf(object o, string fieldName)
		{
			var type = o.GetType();
			var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			if (field == null)
				throw new ArgumentException(string.Format("Can't find {0}.{1}", o, fieldName));
			return field.GetValue(o);
		}
		
		// InspectorState access
		
		public static void SetPeristedValueOfType<T>(string key, T value)
		{
			MethodInfo methodInfo = StaticMethodOf(inspectorStateType, SetterMethodNameFromType(typeof(T)));
			methodInfo.Invoke(null, new object[] { key, value });
		}
		
		public static T GetPeristedValueOfType<T>(string key, T defaultValue)
		{
			MethodInfo methodInfo = StaticMethodOf(inspectorStateType, GetterMethodNameFromType(typeof(T)));
			object result = methodInfo.Invoke(null, new object[] { key, defaultValue });
			return (T)result;		
		}
		
		static string SetterMethodNameFromType(Type type)
		{
			if (type == typeof(bool))
				return "SetBool";
			if (type == typeof(int))
				return "SetInt";
			if (type == typeof(float))
				return "SetFloat";
			if (type == typeof(Vector3))
				return "SetVector3";
			
			throw new ArgumentException(string.Format("Invalid type: '{0}' is not supported as persited type", type));
		}
		
		static string GetterMethodNameFromType(Type type)
		{
			if (type == typeof(bool))
				return "GetBool";
			if (type == typeof(int))
				return "GetInt";
			if (type == typeof(float))
				return "GetFloat";
			if (type == typeof(Vector3))
				return "GetVector3";
			
			throw new ArgumentException(string.Format("Invalid type: '{0}' is not supported as persited type", type));
		}
		
		static MissingEditorAPI()
		{
			var editorAssembly = typeof(EditorWindow).Assembly;
			inspectorStateType = editorAssembly.GetType("UnityEditor.InspectorState");
			if (inspectorStateType == null)
				Debug.LogError("Could not get type: UnityEditor.InspectorWindow");

		}
		
		/*
		private static readonly GetBoolFromCpp _getbool = (GetBoolFromCpp)DelegateForStaticMethodOf<inspectorStateType, GetBoolFromCpp>("GetBool");
		private delegate bool GetBoolFromCpp(string key);
		private static Delegate DelegateForStaticMethodOf<T, TDelegate>(string name)
		{
			return DelegateForStaticMethodOf(typeof(T), name, typeof(TDelegate));
		}
		private static Delegate DelegateForStaticMethodOf(Type type, string name, Type delegateType)
		{
			return Delegate.CreateDelegate(delegateType, StaticMethodOf(type, name, DelegateSignature(delegateType)));
		}
		private static Type[] DelegateSignature(Type delegateType)
		{
			return delegateType.GetMethod("Invoke").GetParameters().Select(_ => _.ParameterType).ToArray();
		}
*/
		private static MethodInfo StaticMethodOf(Type type, string name/*, params Type[] signature*/)
		{
			var result = type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (result == null)
				throw new ArgumentException(string.Format("Can't find method {0}.{1}({2})", type.Name, name, ""/*string.Join(", ", signature.Select(_ => _.FullName).ToArray()*/));
			return result;
		}
		
		
	}
}