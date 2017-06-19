using System;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace UnityEditor.UI.Windows.Extensions.Utilities {

	internal class NewScriptGenerator {
		private const int kCommentWrapLength = 35;
		
		private TextWriter m_Writer;
		private string m_Text;
		private ScriptPrescription m_ScriptPrescription;
		private string m_Indentation;
		private int m_IndentLevel = 0;

		private int IndentLevel {
			get {
				return m_IndentLevel;
			}
			set {
				m_IndentLevel = value;
				m_Indentation = String.Empty;
				for (int i = 0; i < m_IndentLevel; i++) m_Indentation += "	";
			}
		}

		private string ClassName {
			get {
				if (m_ScriptPrescription.m_ClassName != string.Empty) return m_ScriptPrescription.m_ClassName;
				return "Example";
			}
		}

		public NewScriptGenerator(ScriptPrescription scriptPrescription) {
			m_ScriptPrescription = scriptPrescription;
		}

		public override string ToString() {
			m_Text = m_ScriptPrescription.m_Template;
			m_Writer = new StringWriter();
			m_Writer.NewLine = "\n";
			
			// Make sure all line endings are Unix (Mac OS X) format
			m_Text = Regex.Replace(m_Text, @"\r\n?", delegate(Match m) {
				return "\n";
			});
			
			// Class Name
			m_Text = m_Text.Replace("$ClassName", ClassName);
			m_Text = m_Text.Replace("$NicifiedClassName", ObjectNames.NicifyVariableName(ClassName));
			
			// Other replacements
			foreach (KeyValuePair<string, string> kvp in m_ScriptPrescription.m_StringReplacements) m_Text = m_Text.Replace(kvp.Key, kvp.Value);
			
			// Functions
			// Find $Functions keyword including leading tabs
			Match match = Regex.Match(m_Text, @"(\t*)\$Functions");
			if (match.Success) {
				// Set indent level to number of tabs before $Functions keyword
				IndentLevel = match.Groups[1].Value.Length;
				bool hasFunctions = false;
				if (m_ScriptPrescription.m_Functions != null) {
					foreach (var function in m_ScriptPrescription.m_Functions.Where (f => f.include)) {
						WriteFunction(function);
						WriteBlankLine();
						hasFunctions = true;
					}
					
					// Replace $Functions keyword plus newline with generated functions text
					if (hasFunctions) m_Text = m_Text.Replace(match.Value + "\n", m_Writer.ToString());
				}
				
				if (!hasFunctions) {
					/*if (m_ScriptPrescription.m_Lang == Language.Boo && !m_Text.Contains ("def"))
						// Replace $Functions keyword with "pass" if no functions in Boo
						m_Text = m_Text.Replace (match.Value, m_Indentation + "pass");
					else*/
					// Otherwise just remove $Functions keyword plus newline
					m_Text = m_Text.Replace(match.Value + "\n", string.Empty);
				}
			}
			
			// Put curly vraces on new line if specified in editor prefs
			if (EditorPrefs.GetBool("CurlyBracesOnNewLine")) PutCurveBracesOnNewLine();
			
			// Return the text of the script
			return m_Text;
		}

		private void PutCurveBracesOnNewLine() {
			m_Text = Regex.Replace(m_Text, @"(\t*)(.*) {\n((\t*)\n(\t*))?", delegate(Match match) {
				return match.Groups[1].Value + match.Groups[2].Value + "\n" + match.Groups[1].Value + "{\n" +
				(match.Groups[4].Value == match.Groups[5].Value ? match.Groups[4].Value : match.Groups[3].Value);
			});
		}

		private void WriteBlankLine() {
			m_Writer.WriteLine(m_Indentation);
		}

		private void WriteComment(string comment) {
			int index = 0;
			while (true) {
				if (comment.Length <= index + kCommentWrapLength) {
					m_Writer.WriteLine(m_Indentation + "// " + comment.Substring(index));
					break;
				} else {
					int wrapIndex = comment.IndexOf(' ', index + kCommentWrapLength);
					if (wrapIndex < 0) {
						m_Writer.WriteLine(m_Indentation + "// " + comment.Substring(index));
						break;
					} else {
						m_Writer.WriteLine(m_Indentation + "// " + comment.Substring(index, wrapIndex - index));
						index = wrapIndex + 1;
					}
				}
			}
		}

		private string TranslateTypeToJavascript(string typeInCSharp) {
			return typeInCSharp.Replace("bool", "boolean").Replace("string", "String").Replace("Object",
			                                                                                      "UnityEngine.Object");
		}

		private string TranslateTypeToBoo(string typeInCSharp) {
			return typeInCSharp.Replace("float", "single");
		}

		private void WriteFunction(FunctionData function) {
			string paramString = string.Empty;
			string overrideString;
			string returnTypeString;
			string functionContentString;
			string paramStringWithoutTypes = string.Empty;
			
			switch (m_ScriptPrescription.m_Lang) {
			/*case Language.JavaScript:
				// Comment
				WriteComment (function.comment);
				
				// Function header
				for (int i=0; i<function.parameters.Length; i++)
				{
					paramString += function.parameters[i].name + " : " + TranslateTypeToJavascript (function.parameters[i].type);
					if (i < function.parameters.Length-1)
						paramString += ", ";
				}
				overrideString = (function.isVirtual ? "override " : string.Empty);
				returnTypeString = (function.returnType == null ? " " : " : " + TranslateTypeToJavascript (function.returnType) + " ");
				m_Writer.WriteLine (m_Indentation + overrideString + "function " + function.name + " (" + paramString + ")" + returnTypeString + "{");
				
				// Function content
				IndentLevel++;
				functionContentString = (function.returnType == null ? string.Empty : function.returnDefault + ";");
				m_Writer.WriteLine (m_Indentation + functionContentString);
				IndentLevel--;
				m_Writer.WriteLine (m_Indentation + "}");
				
				break;*/
				
				case Language.CSharp:
					
					// Comment
					WriteComment(function.comment);
				
					// Function header
					for (int i = 0; i < function.parameters.Length; i++) {
						
						paramString += function.parameters[i].type + " " + function.parameters[i].name;
						paramStringWithoutTypes += function.parameters[i].name;
						if (i < function.parameters.Length - 1) {

							paramString += ", ";
							paramStringWithoutTypes += ", ";

						}

					}
					overrideString = (function.isVirtual ? "public override " : string.Empty);
					returnTypeString = (function.returnType == null ? "void " : function.returnType + " ");
					m_Writer.WriteLine(m_Indentation + overrideString + returnTypeString + function.name + "(" + paramString + ") {");
				
					// Function content
					IndentLevel++;
					{

						if (function.isVirtual == true) {

							WriteBlankLine();
							m_Writer.WriteLine(m_Indentation + "base." + function.name + "(" + paramStringWithoutTypes + ");");
							WriteBlankLine();

						}

						functionContentString = (function.returnType == null ? string.Empty : function.returnDefault + ";");
						m_Writer.WriteLine(m_Indentation + functionContentString);
						WriteBlankLine();

					}
					IndentLevel--;
					m_Writer.WriteLine(m_Indentation + "}");
				
					break;
				
			/*case Language.Boo:
				// Comment
				WriteComment (function.comment);
				
				// Function header
				for (int i=0; i<function.parameters.Length; i++)
				{
					paramString += function.parameters[i].name + " as " + TranslateTypeToBoo (function.parameters[i].type);
					if (i < function.parameters.Length-1)
						paramString += ", ";
				}
				overrideString = (function.isVirtual ? "public override " : string.Empty);
				returnTypeString = (function.returnType == null ? string.Empty : " as " + TranslateTypeToJavascript (function.returnType));
				m_Writer.WriteLine (m_Indentation + overrideString + "def " + function.name + " (" + paramString + ")" + returnTypeString + ":");
				
				// Function content
				IndentLevel++;
				functionContentString = (function.returnType == null ? "pass" : function.returnDefault);
				m_Writer.WriteLine (m_Indentation + functionContentString);
				IndentLevel--;
				
				break;*/
			}
		}
	}
}

