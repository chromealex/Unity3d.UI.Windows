
namespace UnityEngine.UI.Windows.Plugins.GameData {

	[System.Serializable]
	public class GDFloat {
		
		public string key;
	    private float? cache;
		
		public GDFloat(string key) {
			
			this.key = key;
			
		}
		
		public bool IsNone() {
			
			return this.key == null || string.IsNullOrEmpty(this.key.Trim());
			
		}

		public float Get() {

            #if UNITY_EDITOR
		    if (Application.isPlaying == false) {

                return GameDataSystem.Get(this);

		    }
            #endif

            if (this.cache.HasValue == true) return this.cache.Value;

            this.cache = GameDataSystem.Get(this);
		    return this.cache.Value;

		}

	    public void ForceSet(float value) {

	        this.cache = value;

	    }
		
	}
	
	[System.Serializable]
	public class GDInt {
		
		public string key;
        private int? cache;

        public GDInt(string key) {
			
			this.key = key;
			
		}
		
		public bool IsNone() {
			
			return this.key == null || string.IsNullOrEmpty(this.key.Trim());
			
		}

        public int Get() {

        #if UNITY_EDITOR
            if (Application.isPlaying == false) {

                return GameDataSystem.Get(this);

            }
        #endif

            if (this.cache.HasValue == true) return this.cache.Value;

            this.cache = GameDataSystem.Get(this);
            return this.cache.Value;

        }

        public void ForceSet(int value) {

            this.cache = value;

        }

    }

    [System.Serializable]
	public class GDBool {
		
		public string key;
        private bool? cache;
		
		public GDBool(string key) {
			
			this.key = key;
			
		}
		
		public bool IsNone() {
			
			return this.key == null || string.IsNullOrEmpty(this.key.Trim());
			
		}

        public bool Get() {

#if UNITY_EDITOR
            if (Application.isPlaying == false) {

                return GameDataSystem.Get(this);

            }
#endif

            if (this.cache.HasValue == true) return this.cache.Value;

            this.cache = GameDataSystem.Get(this);
            return this.cache.Value;

        }

        public void ForceSet(bool value) {

            this.cache = value;

        }

    }

    [System.Serializable]
    public class GDEnum {

        public string key;
        private int? cache;

        public GDEnum(string key) {

            this.key = key;

        }

        public bool IsNone() {

            return this.key == null || string.IsNullOrEmpty(this.key.Trim());

        }

        public int Get() {

#if UNITY_EDITOR
            if (Application.isPlaying == false) {

                return GameDataSystem.Get(this);

            }
#endif

            if (this.cache.HasValue == true)
                return this.cache.Value;

            this.cache = GameDataSystem.Get(this);
            return this.cache.Value;

        }

        public T Get<T>() where T : struct, System.IComparable {

            return (T)(object)this.Get();

        }

    }

}