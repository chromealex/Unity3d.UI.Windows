namespace UnityEngine.UI.Windows {

	public class ResourceAsyncOperation : YieldInstruction, System.IDisposable {

		public ResourceAsyncOperation() {}

		~ResourceAsyncOperation() {

			this.Dispose();

		}

		public int priority { get; set; }
		public bool isDone { get; private set; }
		public float progress { get; private set; }
		public object asset { get; private set; }
		public bool broke { get; private set; }

		internal void SetValues(bool isDone, float progress, object asset) {

			this.isDone = isDone;
			this.progress = progress;
			this.asset = asset;

		}

		public void Break() {

			this.broke = true;

		}

		public void Dispose() {

			this.asset = null;
			System.GC.SuppressFinalize(this);

		}

	}

	public static class Instantiator {

		public static void Destroy<T>(T instance) where T : Object {

			if (instance != null) {

				if (instance is Component) {

					Object.Destroy((instance as Component).gameObject);

				} else {

					Object.Destroy(instance);

				}

			}

		}

		public static T Instantiate<T>(T source, bool createInstance = true) where T : Object {

			if (createInstance == false) return source;

			if (source != null) {

				var instance = Object.Instantiate<T>(source);
				if (instance is Component) (instance as Component).gameObject.SetActive(false);
				return instance;

			}

			return default(T);

		}

	}

}