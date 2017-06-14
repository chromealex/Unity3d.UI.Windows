using UnityEngine;
using System.Collections;

namespace ME.UAB {

	[CreateAssetMenu()]
	public class UABConfig : ScriptableObject {

		public enum CacheType : byte {
			StreamingAssets,
			Resources,
		};

		public string UAB_SERIALIZERS_NAMESPACE = "ME.UAB.Serializers";

		public CacheType UAB_CACHE_TYPE = CacheType.StreamingAssets;
		public string UAB_CACHE_PATH = "{0}/BundlesCache/{1}";
		public string UAB_EXT = "uab";
		public bool UAB_BUILD_BYTES = true;

		public string UAB_RESOURCES_PATH = "UAB/Resources";

	}

}