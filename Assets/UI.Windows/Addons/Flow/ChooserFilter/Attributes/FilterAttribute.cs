using UnityEngine;
using System;

public class FilterAttribute : PropertyAttribute {
	
	public readonly Type type;
	
	public FilterAttribute(Type type) {
		
		this.type = type;

	}

}
