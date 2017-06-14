
using System.Text.RegularExpressions;namespace UnityEngine.UI.Windows {

	[System.Serializable]
	public struct VersionCheck {

		public bool enabled;
		public Version version;

	}
	
	[System.Serializable]
	public struct VersionCrc {

		public int crc;

		public VersionCrc(Version version) {

			this.crc = (int)ME.Utilities.GetHash(version.ToString());

		}

		public VersionCrc(string version) : this(new Version(version)) {

		}

		public override bool Equals(object obj) {
			
			var version = (VersionCrc)obj;
			return version == this;
			
		}
		
		public override int GetHashCode() {
			
			return this.crc;
			
		}
		
		public static bool operator ==(Version v1, VersionCrc v2) {
			
			return ME.Utilities.GetHash(v1.ToString()) == v2.crc;
			
		}
		
		public static bool operator !=(Version v1, VersionCrc v2) {
			
			return !(v1 == v2);
			
		}
		
		public static bool operator ==(VersionCrc v1, Version v2) {
			
			return ME.Utilities.GetHash(v2.ToString()) == v1.crc;
			
		}
		
		public static bool operator !=(VersionCrc v1, Version v2) {
			
			return !(v1 == v2);
			
		}

		public static bool operator ==(VersionCrc v1, VersionCrc v2) {
			
			return v1.crc == v2.crc;
			
		}
		
		public static bool operator !=(VersionCrc v1, VersionCrc v2) {
			
			return !(v1 == v2);
			
		}

	}

	[System.Serializable]
	public struct Version {
		
		public static class Type {

			public const string Alpha = "a";
			public const string Beta = "b";
			public const string RC = "rc";
			public const string RTM = "rtm";
			public const string GA = "ga";
			public const string Production = "";

			public static string[] GetValues() {

				return new string[] { Type.Alpha, Type.Beta, Type.RC, Type.RTM, Type.GA, Type.Production };

			}
			
			public static int GetIndex(string value) {

				var index = -1;
				switch (value) {
					
					case Type.Alpha:
						index = 0;
						break;
						
					case Type.Beta:
						index = 1;
						break;
						
					case Type.RC:
						index = 2;
						break;
						
					case Type.RTM:
						index = 3;
						break;
						
					case Type.GA:
						index = 4;
						break;
						
					case Type.Production:
						index = 5;
						break;

				}

				return index;

			}

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

		public Version(string value) {

			var pattern = @"^(\d+?)\.(\d+?)\.(\d+?)([a-z]*)$";
			var matches = Regex.Match(value.Trim(), pattern, RegexOptions.IgnoreCase);
			var groups = matches.Groups;
			if (groups.Count == 5) {
				
				this.major = int.Parse(groups[1].Value);
				this.minor = int.Parse(groups[2].Value);
				this.release = int.Parse(groups[3].Value);
				this.type = groups[4].Value;

			} else {

				this.major = 0;
				this.minor = 0;
				this.release = 0;
				this.type = string.Empty;

			}

		}
		
		public static Version minValue {
			
			get {
				
				return new Version(0, 0, 0, Type.Alpha);
				
			}
			
		}

		public static Version maxValue {

			get {

				return new Version(int.MaxValue, int.MaxValue, int.MaxValue, Type.Production);

			}

		}

		public int ToInt() {

			return this.major + this.minor + this.release + Type.GetIndex(this.type);

		}

		public override string ToString() {

			return string.Format("{0}.{1}.{2}{3}", this.major, this.minor, this.release, this.type);

		}
		
		public string ToSmallString() {
			
			return string.Format("{0}{1}{2}{3}", this.major, this.minor, this.release, this.type);
			
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

		public static readonly Version BUNDLE_VERSION = new Version(1, 0, 7, Version.Type.Alpha);

	}

}