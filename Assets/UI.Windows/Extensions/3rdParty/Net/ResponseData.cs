namespace UnityEngine.UI.Windows.Extensions.Net {

	public interface IResponse {

		Error error { get; set; }
		bool WithError();

	}

	public class Error {

		public int code;
		public string text; // nullable
		public object obj;  // nullable

	}

	public class ResponseData<T> : IResponse {

		public int i; // 123, Iteration id (# package)
		public T data;
		public Error error { get; set; }

		public ResponseData() {}

		public ResponseData(T data) {

			this.data = data;

		}

		public T GetData() {

			return this.data;

		}

		public bool WithError() {

			return error != null;

		}

	}
}
