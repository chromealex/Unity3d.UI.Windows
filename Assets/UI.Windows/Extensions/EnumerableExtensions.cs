using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumerableExtensions {
	
	public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) {
		
		Random rng = new Random(UnityEngine.Random.Range(0, 9999999));
		T[] sourceArray = source.ToArray();
		
		for (int n = 0; n < sourceArray.Length; n++) {
			
			int k = rng.Next(n, sourceArray.Length);
			yield return sourceArray[k];
			
			sourceArray[k] = sourceArray[n];
			
		}
		
	}
	
	public static void Shuffle<T>(this T[] source) {
		
		Random rng = new Random(UnityEngine.Random.Range(0, 9999999));
		T[] sourceArray = source;
		
		for (int n = 0; n < sourceArray.Length; n++) {
			
			int k = rng.Next(n, sourceArray.Length);
			sourceArray[k] = sourceArray[n];
			
		}

	}

}