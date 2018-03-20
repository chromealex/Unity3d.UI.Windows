#if !UNITY_SWITCH && !UNITY_TVOS

using UnityEngine;

namespace ME.FS.Core.Detail {

    public class FileSystemCommon : FileSystem {

        public override bool IsCacheSupported() {

            return true;

        }

        public override string GetCachePath() {

			var path = Application.persistentDataPath;
			return path;

        }

    }

}

#endif
