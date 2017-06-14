using System;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace UnityEditor.UI.Windows.Extensions.Utilities {

	[Serializable]
	internal class ScriptPrescription {
		public string m_NameSpace = string.Empty;
		public string m_ClassName = string.Empty;
		public Language m_Lang = Language.CSharp;
		public string m_Template;
		public FunctionData[] m_Functions;
		public Dictionary<string, string> m_StringReplacements = new Dictionary<string, string>();
	}

	internal enum Language {
		CSharp = 0,
		//JavaScript = 1,
		//Boo = 2
	}

	internal struct FunctionData {
		public string name;
		public string returnType;
		public string returnDefault;
		public bool isVirtual;
		public ParameterData[] parameters;
		public string comment;
		public bool include;

		public FunctionData(string headerName) {
			comment = headerName;
			name = null;
			returnType = null;
			returnDefault = null;
			isVirtual = false;
			parameters = null;
			include = false;
		}
	}

	internal struct ParameterData {
		public string name;
		public string type;

		public ParameterData(string name, string type) {
			this.name = name;
			this.type = type;
		}
	}
}