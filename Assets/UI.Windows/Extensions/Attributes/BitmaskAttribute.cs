using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BitMaskAttribute : PropertyAttribute
{
	public System.Type propType;
	public BitMaskAttribute(System.Type aType)
	{
		propType = aType;
	}
}

#if UNITY_EDITOR
public static class EditorExtension {
	
	public static int DrawBitMaskFieldLayout(int aMask, System.Type aType, GUIContent aLabel) {
		
		return DrawBitMaskField(new Rect(), aMask, aType, aLabel, layout: true);

	}
	
	public static int DrawBitMaskField(Rect aPosition, int aMask, System.Type aType, GUIContent aLabel) {

		return DrawBitMaskField(aPosition, aMask, aType, aLabel, layout: false);

	}

	public static int DrawBitMaskField(Rect aPosition, int aMask, System.Type aType, GUIContent aLabel, bool layout) {

		var itemNames = System.Enum.GetNames(aType);
		var itemValues = System.Enum.GetValues(aType) as int[];
		if (itemValues == null) {

			var bytes = System.Enum.GetValues(aType) as byte[];
			itemValues = new int[bytes.Length];
			for (int i = 0; i < itemValues.Length; ++i) itemValues[i] = (int)bytes[i];

		}

		if (itemValues == null) return 0;

		int val = aMask;
		int maskVal = 0;
		for(int i = 0; i < itemValues.Length; i++)
		{
			if (itemValues[i] != 0)
			{
				if ((val & itemValues[i]) == itemValues[i])
					maskVal |= 1 << i;
			}
			else if (val == 0)
				maskVal |= 1 << i;
		}

		var newMaskVal = 0;
		if (layout == true) {
			
			newMaskVal = EditorGUILayout.MaskField(aLabel, maskVal, itemNames);

		} else {

			newMaskVal = EditorGUI.MaskField(aPosition, aLabel, maskVal, itemNames);

		}

		int changes = maskVal ^ newMaskVal;
		
		for(int i = 0; i < itemValues.Length; i++)
		{
			if ((changes & (1 << i)) != 0)            // has this list item changed?
			{
				if ((newMaskVal & (1 << i)) != 0)     // has it been set?
				{
					if (itemValues[i] == 0)           // special case: if "0" is set, just set the val to 0
					{
						val = 0;
						break;
					}
					else
						val |= itemValues[i];
				}
				else                                  // it has been reset
				{
					val &= ~itemValues[i];
				}
			}
		}
		return val;
	}
}

[CustomPropertyDrawer(typeof(BitMaskAttribute))]
public class EnumBitMaskPropertyDrawer : PropertyDrawer
{
	public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
	{
		var typeAttr = attribute as BitMaskAttribute;
		// Add the actual int value behind the field name
		label.text = label.text + " (" + prop.intValue + ")";
		if (prop.hasMultipleDifferentValues == true) {

			GUILayout.Label("-");

		} else {

			prop.intValue = EditorExtension.DrawBitMaskField(position, prop.intValue, typeAttr.propType, label);
		}

	}
}
#endif