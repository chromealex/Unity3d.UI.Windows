#if UNITY_TVOS

using UnityEngine;

namespace ME.FS.Core.Detail {

    public class FileSystemTvOS : FileSystem {

        public override bool IsCacheSupported() {

            return true;

        }

        public override string GetCachePath() {

            return Application.temporaryCachePath;

        }

    }

}

#endif
