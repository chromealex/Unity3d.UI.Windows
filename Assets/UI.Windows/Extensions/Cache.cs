using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Extensions {

	public class Cache {

		public int[] values = null;

		private bool isEmpty = true;
		
		public void Fill<T>(List<T> sources, System.Func<T, int, int> getKey, System.Func<T, int, int> getValue) {
			
			var maxKey = 0;
			for (int i = 0; i < sources.Count; ++i) {
				
				var key = getKey(sources[i], i);
				if (maxKey < key) maxKey = key;
				
			}
			
			this.values = new int[maxKey + 1];
			for (int i = 0; i < sources.Count; ++i) {
				
				this.values[getKey(sources[i], i)] = getValue(sources[i], i);
				
			}
			
			this.isEmpty = false;
			
		}
		
		public void Fill<T>(T[] sources, System.Func<T, int, int> getKey, System.Func<T, int, int> getValue) {
			
			var maxKey = 0;
			for (int i = 0; i < sources.Length; ++i) {
				
				var key = getKey(sources[i], i);
				if (maxKey < key) maxKey = key;
				
			}
			
			this.values = new int[maxKey + 1];
			for (int i = 0; i < sources.Length; ++i) {
				
				this.values[getKey(sources[i], i)] = getValue(sources[i], i);
				
			}
			
			this.isEmpty = false;
			
		}

		public void Clear() {
		
			this.values = null;
			this.isEmpty = true;
		
		}

		public int GetValue(int key) {

			if (key >= this.values.Length) return -1;
			return this.values[key];

		}

		public bool IsEmpty() {

			return this.isEmpty;

		}

	}

	public class Cache<T0, T1> {

		public List<T0> keys = new List<T0>();
		public List<T1> values = new List<T1>();

		private bool isEmpty = true;

		public void Fill<T>(List<T> sources, System.Func<T, int, T0> getKey, System.Func<T, int, T1> getValue) {

			this.keys.Clear();
			this.values.Clear();

			for (int i = 0; i < sources.Count; ++i) {

				this.keys.Add(getKey(sources[i], i));
				this.values.Add(getValue(sources[i], i));

			}

			this.isEmpty = false;

		}

		public T1 GetValue(T0 key) {

			var index = this.keys.IndexOf(key);
			if (index < 0) return default(T1);

			return this.values[index];

		}

		public bool IsEmpty() {

			return this.isEmpty;

		}

	}

}