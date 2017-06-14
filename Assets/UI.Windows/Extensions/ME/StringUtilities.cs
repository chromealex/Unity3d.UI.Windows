using System.Text.RegularExpressions;
using System.Linq;
using System.Collections;

namespace ME {

	public class AlphanumComparatorFast : IComparer {
		
		public int Compare(object x, object y) {
			
			string s1 = x as string;
			if (s1 == null) {
				return 0;
			}
			string s2 = y as string;
			if (s2 == null) {
				return 0;
			}

			int len1 = s1.Length;
			int len2 = s2.Length;
			int marker1 = 0;
			int marker2 = 0;

			// Walk through two the strings with two markers.
			while (marker1 < len1 && marker2 < len2) {
				
				char ch1 = s1[marker1];
				char ch2 = s2[marker2];

				// Some buffers we can build up characters in for each chunk.
				char[] space1 = new char[len1];
				int loc1 = 0;
				char[] space2 = new char[len2];
				int loc2 = 0;

				// Walk through all following characters that are digits or
				// characters in BOTH strings starting at the appropriate marker.
				// Collect char arrays.
				do {
					
					space1[loc1++] = ch1;
					marker1++;

					if (marker1 < len1) {
						
						ch1 = s1[marker1];

					} else {
						
						break;
					}

				} while (char.IsDigit(ch1) == char.IsDigit(space1[0]));

				do {
					
					space2[loc2++] = ch2;
					marker2++;

					if (marker2 < len2) {
						
						ch2 = s2[marker2];

					}
					else {
						
						break;

					}

				} while (char.IsDigit(ch2) == char.IsDigit(space2[0]));

				// If we have collected numbers, compare them numerically.
				// Otherwise, if we have strings, compare them alphabetically.
				string str1 = new string(space1);
				string str2 = new string(space2);

				int result;

				if (char.IsDigit(space1[0]) && char.IsDigit(space2[0])) {
					
					int thisNumericChunk = int.Parse(str1);
					int thatNumericChunk = int.Parse(str2);
					result = thisNumericChunk.CompareTo(thatNumericChunk);

				}
				else {
					
					result = str1.CompareTo(str2);

				}

				if (result != 0) {
					
					return result;

				}
			}

			return len1 - len2;

		}

	}

	public static class StringUtilities {
		
		public static string UppercaseFirst(this string s) {
			
			if (string.IsNullOrEmpty(s)) return string.Empty;
			
			char[] a = s.ToCharArray();
			a[0] = char.ToUpper(a[0]);
			
			return new string(a);
			
		}
		
		public static string UppercaseWords(this string value) {

			var chars = new char[] { ' ', '.', ',', '!', '@', '#', '$', '%', '^', '&', '*', '(', '{', '[', '/', '\\' };

			char[] array = value.ToCharArray();
			// Handle the first letter in the string.
			if (array.Length >= 1) {

				if (char.IsLower(array[0])) {

					array[0] = char.ToUpper(array[0]);

				}

			}

			// Scan through the letters, checking for spaces.
			// ... Uppercase the lowercase letters following spaces.
			for (int i = 1; i < array.Length; ++i) {

				if (chars.Contains(array[i - 1]) == true) {

					if (char.IsLower(array[i])) {

						array[i] = char.ToUpper(array[i]);

					}

				}

			}

			return new string(array);

		}
		
		public static string ToSentenceCase(this string str) {
			
			return Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLower(m.Value[1]));
			
		}
		
		public static string ToPopupSentenceCase(this string str) {

			var k = 0;
			return Regex.Replace(str, "[a-z][A-Z]", m => {

				++k;

				if (k == 1) {
					
					return m.Value[0] + "/" + char.ToUpper(m.Value[1]);

				} else {

					return m.Value[0] + " " + char.ToLower(m.Value[1]);

				}

			});

		}

	}

}