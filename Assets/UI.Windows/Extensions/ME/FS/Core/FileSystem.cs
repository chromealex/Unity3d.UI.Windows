using ME.FS.Core.Detail;
using ME.FS.Lib;

namespace ME.FS.Core {

#if UNITY_SWITCH
    using FileSystemImpl = FileSystemSwitch;
#elif UNITY_TVOS
    using FileSystemImpl = FileSystemTvOS;
#else
    using FileSystemImpl = FileSystemCommon;
#endif

    public abstract class FileSystem : Singleton<FileSystem, FileSystemImpl> {

        protected FileSystem() {

        }

        protected override void Destroy() {

        }

        public abstract bool IsCacheSupported();
        public abstract string GetCachePath();

    }

}