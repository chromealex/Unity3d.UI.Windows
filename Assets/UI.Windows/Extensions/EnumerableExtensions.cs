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
	
	public static T[] Shuffle<T>(this T[] source) {
		
		Random rng = new Random(UnityEngine.Random.Range(0, 9999999));
		var sourceArray = source.ToList();
		
		for (var i = 0; i < source.Length; ++i) {
			
			var k = rng.Next(0, sourceArray.Count);
			source[i] = sourceArray[k];
			sourceArray.RemoveAt(k);
			
		}

		return source;
		
	}
	
	public static T[] Reverse<T>(this T[] source) {

		var copy = new T[source.Length];
		for (int k = 0, i = source.Length - 1; i >= 0; --i, ++k) {

			copy[i] = source[k];

		}

		return copy;

	}

}