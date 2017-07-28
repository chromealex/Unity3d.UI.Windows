using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ME {

	public class Coroutines : MonoBehaviour {

		[System.Serializable]
		public class Item {

			public int tag;
			public List<IEnumerator> list;

		}

		//public int currentCount;
		//public Dictionary<int, List<IEnumerator>> list = new Dictionary<int, List<IEnumerator>>();
		public List<Item> list = new List<Item>();
		private static Coroutines instance;

		public void Awake() {
			
			Coroutines.instance = this;

		}
		/*
		public void Start() {

			//for (int i = 0; i < 1000000; ++i) {

				Coroutines.Run(this.Test());

			//}

		}

		private IEnumerator<byte> Test() {

			Debug.Log("Test start");
			yield return 0;

			var c = this.TestInner();
			while (c.MoveNext() == true) yield return 0;
			Debug.Log("Test end");

		}

		private IEnumerator<byte> TestInner() {

			Debug.Log("Inner start");
			yield return 0;
			Debug.Log("Inner end");
			yield return 0;

		}*/

		public void LateUpdate() {

			var dic = Coroutines.instance.list;
			/*var e = list.GetEnumerator();
			while (e.MoveNext() == true) {

				if (e.Current != null) {

					e.Current.MoveNext();

				}*/

			//var e = dic.GetEnumerator();
			//while (e.MoveNext() == true) {

			for (int j = 0; j < dic.Count; ++j) {

				var list = dic[j].list;
				for (int i = list.Count - 1; i >= 0; --i) {

					//if (list[i] == null) continue;

					if (list[i].MoveNext() == false) {

						list.RemoveAt(i);

					}

				}

			}

			// run accumulate next frame
			var frame = Time.frameCount;
			if (Coroutines.nextFrameDelegateList != null) {

				foreach (var item in Coroutines.nextFrameDelegateList) {

					var values = item.Value;
					for (int i = 0; i < values.Count; ++i) {

						var val = values[i];
						if (frame > val.frame) {

							values[i].Call();
							values.RemoveAt(i);
							--i;

						}

					}

				}

			}

		}

		/*
		 * API
		 */
		public static IEnumerator<byte> WaitForSeconds(float time) {

			var timer = 0f;
			while (timer < time) {

				timer += Time.deltaTime;
				yield return 0;

			}

		}

		public class DelegateItem {

			public int frame;
			public System.Delegate del;
			public object[] parameters;

			public void Call() {

				this.del.DynamicInvoke(this.parameters);

			}

		}

		private static Dictionary<object, List<DelegateItem>> nextFrameDelegateList = new Dictionary<object, List<DelegateItem>>();
		public static void AccumulateRunNextFrame(object tag, System.Delegate[] delegates, params object[] parameters) {

			var frame = Time.frameCount;

			List<DelegateItem> list;
			if (nextFrameDelegateList.TryGetValue(tag, out list) == true) {

				for (int i = 0; i < delegates.Length; ++i) {

					var del = delegates[i];
					if (list.Any(x => x.frame == frame && x.del == del) == false) {

						list.Add(new DelegateItem() { frame = frame, del = del, parameters = parameters });

					}

				}

			} else {

				list = new List<DelegateItem>();
				for (int i = 0; i < delegates.Length; ++i) {

					var del = delegates[i];
					if (list.Any(x => x.frame == frame && x.del == del) == false) {

						list.Add(new DelegateItem() { frame = frame, del = del, parameters = parameters });

					}

				}

				nextFrameDelegateList.Add(tag, list);

			}

		}

		public static IEnumerator<byte> WaitForNextFrame(System.Action callback) {

			yield return 0;

			if (callback != null) callback.Invoke();

		}

		public static System.Collections.Generic.IEnumerator<byte> RunNextFrame(System.Action callback) {

			return Coroutines.Run(Coroutines.WaitForNextFrame(callback));

		}

		public static System.Collections.Generic.IEnumerator<byte> Run(System.Collections.Generic.IEnumerator<byte> coroutine, Object tag = null) {

			if (coroutine.MoveNext() == true) {

				var key = (tag == null ? 0 : tag.GetInstanceID());
				List<IEnumerator> list = null;
				for (int i = 0; i < Coroutines.instance.list.Count; ++i) {

					var item = Coroutines.instance.list[i];
					if (item.tag == key) {

						list = item.list;

					}

				}

				if (list != null) {

					list.Add(coroutine);

				} else {
					
					Coroutines.instance.list.Add(new Item() { tag = key, list = new List<IEnumerator>() { coroutine }});

				}

			}

			return coroutine;

		}

		public static void Stop(System.Collections.Generic.IEnumerator<byte> coroutine) {

			if (Coroutines.instance == null) return;

			for (int i = 0; i < Coroutines.instance.list.Count; ++i) {

				var item = Coroutines.instance.list[i];
				if (item.list.Contains(coroutine) == true) {

					item.list.Remove(coroutine);
					return;

				}

			}

		}

		public static void StopAll(Object tag) {

			if (Coroutines.instance == null) return;

			if (tag != null) {

				var key = tag.GetInstanceID();
				for (int i = 0; i < Coroutines.instance.list.Count; ++i) {

					var item = Coroutines.instance.list[i];
					if (item.tag == key) {

						Coroutines.instance.list.RemoveAt(i);
						return;

					}

				}

			}

		}

		public static void StopAll() {

			Coroutines.instance.list.Clear();

		}

	}

}