
namespace ME {

	public static class StringUtilities {

		public static string UppercaseFirst(this string s) {

			if (string.IsNullOrEmpty(s)) return string.Empty;

			char[] a = s.ToCharArray();
			a[0] = char.ToUpper(a[0]);

			return new string(a);

		}

	}

}