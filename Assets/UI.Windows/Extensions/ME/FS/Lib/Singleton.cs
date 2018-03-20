using UnityEngine.Assertions;

namespace ME.FS.Lib {

    /*public abstract class Singleton<T> where T : Singleton<T>, new() {

        public static T instance { get; private set; }

        public static void Init() {

            Assert.IsNull(Singleton<T>.instance);
            Singleton<T>.instance = new T();

        }

        public static void Term() {

            Assert.IsNotNull(Singleton<T>.instance);
            Singleton<T>.instance.Destroy();
            Singleton<T>.instance = null;

        }

        protected abstract void Destroy();

    }*/

    public abstract class Singleton<TBase, TFinal> where TBase : Singleton<TBase, TFinal> where TFinal : TBase, new() {

		private static TBase _instance;
		public static TBase instance {
			
			get {
				
				if (_instance == null) Singleton<TBase, TFinal>.Init();
				return _instance;

			}

		}

        public static void Init() {

            Assert.IsNull(Singleton<TBase, TFinal>._instance);
            Singleton<TBase, TFinal>._instance = new TFinal();

        }

        public static void Term() {

            Assert.IsNotNull(Singleton<TBase, TFinal>._instance);
            Singleton<TBase, TFinal>._instance.Destroy();
            Singleton<TBase, TFinal>._instance = null;

        }

        protected abstract void Destroy();

    }

}

