using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumBitmaskExtensions {
	
	public static int CountBits(this byte enumValue) {
		
		return EnumBitmaskExtensions.GetSetBitCount((long)enumValue);
		
	}
	
	public static int CountBits(this long enumValue) {
		
		return EnumBitmaskExtensions.GetSetBitCount(enumValue);
		
	}

	public static int GetSetBitCount(long value) {

		int count = 0;
		
		while (value != 0) {

			value = value & (value - 1);

			++count;

		}

		return count;
	}

}