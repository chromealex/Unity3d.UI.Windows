using UnityEngine;

namespace UnityEngine.UI.Windows.Plugins.GameData {

	[System.Serializable]
	public class GDFloat {
		
		public string key;
		
		public GDFloat(string key) {
			
			this.key = key;
			
		}
		
		public bool IsNone() {
			
			return this.key == null || string.IsNullOrEmpty(this.key.Trim());
			
		}

		public float Get() {

			return GameDataSystem.Get(this);

		}
		
	}
	
	[System.Serializable]
	public class GDInt {
		
		public string key;
		
		public GDInt(string key) {
			
			this.key = key;
			
		}
		
		public bool IsNone() {
			
			return this.key == null || string.IsNullOrEmpty(this.key.Trim());
			
		}
		
		public int Get() {
			
			return GameDataSystem.Get(this);
			
		}
		
	}
	
	[System.Serializable]
	public class GDBool {
		
		public string key;
		
		public GDBool(string key) {
			
			this.key = key;
			
		}
		
		public bool IsNone() {
			
			return this.key == null || string.IsNullOrEmpty(this.key.Trim());
			
		}
		
		public bool Get() {
			
			return GameDataSystem.Get(this);
			
		}
		
	}

    [System.Serializable]
    public class GDEnum {

        public string key;

        public GDEnum(string key) {

            this.key = key;

        }

        public bool IsNone() {

            return this.key == null || string.IsNullOrEmpty(this.key.Trim());

        }

        public int Get() {

            return GameDataSystem.Get(this);

        }

        public T Get<T>() where T : struct, System.IComparable {

            return (T)(object)this.Get();

        }

    }

}