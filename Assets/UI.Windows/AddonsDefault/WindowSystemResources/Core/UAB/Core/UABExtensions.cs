using System.Linq;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

namespace ME.UAB {

	public class BundleImporter {

		public static string GetBundleName(UnityEngine.Object obj) {

			#if UNITY_EDITOR
			var path = UnityEditor.AssetDatabase.GetAssetPath(obj);
			var baseImporter = UnityEditor.AssetImporter.GetAtPath(path);
			if (baseImporter != null) {

				if (string.IsNullOrEmpty(baseImporter.assetBundleName) == false) {

					return baseImporter.assetBundleName;

				}

			}

			var sPath = path.Split(System.IO.Path.DirectorySeparatorChar);
			if (sPath != null && sPath.Length > 0) {

				var rootPath = sPath[0];
				for (int i = 1; i < sPath.Length; ++i) {

					var importer = UnityEditor.AssetImporter.GetAtPath(rootPath);
					if (importer != null) {

						if (string.IsNullOrEmpty(importer.assetBundleName) == false) {

							return importer.assetBundleName;

						}

					}

					rootPath += System.IO.Path.DirectorySeparatorChar + sPath[i];

				}

			}
			#endif

			return string.Empty;

		}

	}

	public static class TypeExtensions {
		/// <summary>
		/// Determine whether a type is simple (String, Decimal, DateTime, etc) 
		/// or complex (i.e. custom class with public properties and methods).
		/// </summary>
		/// <see cref="http://stackoverflow.com/questions/2442534/how-to-test-if-type-is-primitive"/>
		public static bool IsSimpleType(this System.Type type) {

			return
				type.IsPrimitive ||
				new System.Type[] { 
				typeof(System.String),
				typeof(System.Decimal),
				typeof(System.DateTime),
				typeof(System.DateTimeOffset),
				typeof(System.TimeSpan),
				typeof(System.Guid)
			}.Contains(type) ||
			System.Convert.GetTypeCode(type) != System.TypeCode.Object;

		}

		public static bool IsArray(this object o) {
			
			bool isGenericList = false;
			var oType = o.GetType();
			if (oType.IsArray == true) return true;
			if (oType is IEnumerable == true) return true;
			if (oType is IList == true) return true;
			if (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>))) isGenericList = true;

			return isGenericList;

		}

		public static bool IsGenericList(this object o) {

			bool isGenericList = false;
			var oType = o.GetType();
			if (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>))) isGenericList = true;

			return isGenericList;

		}

		public static bool IsEnumerableType(this System.Type type) {

			return (type.GetInterface("IEnumerable") != null);

		}

		public static System.Type GetEnumerableType(this System.Type type) {

			if (type == null)
				throw new System.ArgumentNullException("type");

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				return type.GetGenericArguments()[0];

			var iface = (from i in type.GetInterfaces()
				where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
				select i).FirstOrDefault();

			if (iface == null)
				throw new System.ArgumentException("Does not represent an enumerable type.", "type");

			return GetEnumerableType(iface);

		}

		public static FieldInfo[] GetAllFields(this System.Type t) {

			if (t == null) return new FieldInfo[0];

			BindingFlags flags = 
				BindingFlags.Public |
				BindingFlags.NonPublic | 
				BindingFlags.Instance | 
				BindingFlags.DeclaredOnly;

			var fields = t.GetFields(flags);
			var baseFields = t.BaseType.GetAllFields();
			return fields.Concat(baseFields).ToArray();

		}

		public static PropertyInfo[] GetAllProperties(this System.Type t) {

			if (t == null) return new PropertyInfo[0];

			BindingFlags flags = 
				BindingFlags.Public |
				BindingFlags.NonPublic | 
				BindingFlags.SetProperty |
				BindingFlags.Instance | 
				BindingFlags.DeclaredOnly;

			var fields = t.GetProperties(flags);
			var baseFields = t.BaseType.GetAllProperties();
			return fields.Concat(baseFields).ToArray();

		}

	}

}