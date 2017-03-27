using UnityEngine;
using System;

namespace UnityEngine.UI.Windows.Extensions {

	public class FilterAttribute : PropertyAttribute {
		
		public readonly Type type;
		
		public FilterAttribute(Type type) {
			
			this.type = type;

		}

	}

}