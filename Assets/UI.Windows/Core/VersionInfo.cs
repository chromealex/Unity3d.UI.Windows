
namespace UnityEngine.UI.Windows {
	
	[System.Serializable]
	public struct Version {
		
		public static class Type {

			public const string Alpha = "a";
			public const string Beta = "b";
			public const string RC = "rc";
			public const string RTM = "rtm";
			public const string GA = "ga";
			public const string Production = "";

		}
		
		public int major;
		public int minor;
		public int release;
		public string type;

		public Version(int major, int minor, int release, string type) {

			this.major = major;
			this.minor = minor;
			this.release = release;
			this.type = type;

		}

		public override string ToString() {

			return string.Format("{0}.{1}.{2}{3}", this.major, this.minor, this.release, this.type.ToString());

		}
		
		public string ToSmallString() {
			
			return string.Format("{0}{1}{2}{3}", this.major, this.minor, this.release, this.type.ToString());
			
		}
		
		public string ToSmallWithoutTypeString() {
			
			return string.Format("{0}{1}{2}", this.major, this.minor, this.release);
			
		}

		public override bool Equals(object obj) {

			var version = (Version)obj;
			return version == this;

		}

		public override int GetHashCode() {

			return this.major + this.major + this.release + this.type.GetHashCode();

		}
		
		private Version Increase() {
			
			++this.release;
			if (this.release > 9) {
				
				this.release = 0;
				
				++this.minor;
				if (this.minor > 9) {
					
					this.minor = 0;
					
					++this.major;
					
				}
				
			}
			
			return new Version(this.major, this.minor, this.release, this.type);
			
		}
		
		private Version Decrease() {
			
			--this.release;
			if (this.release < 0) {
				
				this.release = 9;
				
				--this.minor;
				if (this.minor < 0) {
					
					this.minor = 9;
					
					--this.major;
					if (this.major < 0) {

						this.major = 0;

					}

				}
				
			}
			
			return new Version(this.major, this.minor, this.release, this.type);
			
		}

		public static Version operator +(Version v, int value) {
			
			if (value < 0) value = 0;
			for (int i = 0; i < value; ++i) {
				
				v.Increase();
				
			}
			
			return v;
			
		}
		
		public static Version operator -(Version v, int value) {

			if (value < 0) value = 0;
			for (int i = 0; i < value; ++i) {
				
				v.Decrease();
				
			}
			
			return v;
			
		}

		public static bool operator <(Version v1, Version v2) {

			if (v1.major < v2.major) {

				return true;

			} else if (v1.major == v2.major) {

				if (v1.minor < v2.minor) {

					return true;

				} else if (v1.minor == v2.minor) {

					if (v1.release < v2.release) {

						return true;

					}

				}

			}

			return false;

		}
		
		public static bool operator >(Version v1, Version v2) {
			
			if (v1.major > v2.major) {
				
				return true;
				
			} else if (v1.major == v2.major) {
				
				if (v1.minor > v2.minor) {
					
					return true;
					
				} else if (v1.minor == v2.minor) {
					
					if (v1.release > v2.release) {
						
						return true;
						
					}
					
				}
				
			}
			
			return false;

		}
		
		public static bool operator ==(Version v1, Version v2) {
			
			return v1.major == v2.major &&
				v1.minor == v2.minor &&
				v1.release == v2.release;
			
		}
		
		public static bool operator !=(Version v1, Version v2) {
			
			return v1.major != v2.major ||
				v1.minor != v2.minor ||
				v1.release != v2.release;
			
		}
		
		public static bool operator <=(Version v1, Version v2) {
			
			return v1 == v2 || v1 < v2;
			
		}
		
		public static bool operator >=(Version v1, Version v2) {
			
			return v1 == v2 || v1 > v2;
			
		}

	}

	public class VersionInfo {
		
		public const string DOWNLOAD_LINK = "https://github.com/chromealex/Unity3d.UI.Windows";
		public const string GETKEY_LINK = "http://unity3dwindows.com";
		public const string DESCRIPTION = "Version {0}. MIT license Alex Feer <chrome.alex@gmail.com>";

		public static readonly Version BUNDLE_VERSION = new Version(1, 0, 5, Version.Type.Alpha);

	}

}