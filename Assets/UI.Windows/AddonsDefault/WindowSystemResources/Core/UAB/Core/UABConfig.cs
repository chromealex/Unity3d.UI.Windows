using UnityEngine;
using System.Collections;

namespace ME.UAB {

	[CreateAssetMenu()]
	public class UABConfig : ScriptableObject {

		public string UAB_SERIALIZERS_NAMESPACE = "ME.UAB.Serializers";

		public string UAB_CACHE_PATH = "{0}/BundlesCache/{1}";
		public string UAB_EXT = "uab";

		public string UAB_RESOURCES_PATH = "UAB/Resources";

	}

}