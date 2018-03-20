#if UNITY_SWITCH

namespace ME.FS.Core.Detail {

    public class FileSystemSwitch : FileSystem {

        private const bool enableCacheStorage = false;
        private const bool enableTemporaryStorage = false;

        private const string cacheStoragePath = "cache:";
        private const string temporaryStoragePath = "temp:";

        private static string MountPathToName(string mountPath) {

            return mountPath.Substring(0, mountPath.IndexOf(':'));

        }

        public FileSystemSwitch() {

            if (FileSystemSwitch.enableCacheStorage == true) {

                nn.fs.CacheStorage.Mount(FileSystemSwitch.MountPathToName(FileSystemSwitch.cacheStoragePath));

            }
            if (FileSystemSwitch.enableTemporaryStorage == true) {

                nn.fs.TemporaryStorage.Mount(FileSystemSwitch.MountPathToName(FileSystemSwitch.temporaryStoragePath));

            }

        }

        protected override void Destroy() {

            if (FileSystemSwitch.enableTemporaryStorage == true) {

                nn.fs.FileSystem.Unmount(FileSystemSwitch.MountPathToName(FileSystemSwitch.temporaryStoragePath));

            }

            if (FileSystemSwitch.enableCacheStorage == true) {

                nn.fs.FileSystem.Unmount(FileSystemSwitch.MountPathToName(FileSystemSwitch.cacheStoragePath));

            }

        }

        public override bool IsCacheSupported() {

            return FileSystemSwitch.enableCacheStorage || FileSystemSwitch.enableTemporaryStorage;

        }

        public override string GetCachePath() {

            if (FileSystemSwitch.enableCacheStorage == true) {

                return FileSystemSwitch.cacheStoragePath;

            } else if (FileSystemSwitch.enableTemporaryStorage == true) {

                return FileSystemSwitch.temporaryStoragePath;

            } else {

                return "";

            }

        }

    }

}

#endif
