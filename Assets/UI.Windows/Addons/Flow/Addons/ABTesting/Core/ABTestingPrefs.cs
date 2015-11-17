using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.ABTesting.Net.Api;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.UI.Windows.Plugins.ABTesting {
	
	[System.Serializable]
	public class PrefsBase : IBaseCondition {
		
		public string name;
		[System.NonSerialized]
		public ABTestingItem item;
		
		public PrefsBase(string name) {
			
			this.name = name;
			
		}

		public bool Resolve<T, TValue>(IList list, System.Func<string, TValue> getFromPrefs, System.Func<T, TValue, bool> onResolve) {
			
			var result = false;
			
			if (PlayerPrefs.HasKey(this.name) == true) {
				
				result = true;
				TValue value = getFromPrefs(this.name);
				for (int i = 0; i < list.Count; ++i) {
					
					if (result == true) {
						
						result = onResolve((T)list[i], value);
						
					} else {
						
						break;
						
					}
					
				}
				
			}
			
			return result;

		}

		#if UNITY_EDITOR
		public virtual void OnGUI() {
			
			this.name = EditorGUILayout.TextField("Key", this.name);
			
		}
		#endif
		
	}
	
	[System.Serializable]
	public class IntPrefs : PrefsBase {

		public List<IntCondition> values = new List<IntCondition>();
		
		public IntPrefs(string name) : base(name) {}

		public IntPrefs(IntPrefsTO source) : base(source.name) {

			this.values = new List<IntCondition>();
			foreach (var item in source.values) this.values.Add(new IntCondition(item));

		}
		
		public IntPrefsTO GetTO() {
			
			var result = new IntPrefsTO();
			result.name = this.name;
			result.values = new List<IntConditionTO>();
			foreach (var item in this.values) result.values.Add(item.GetTO());
			return result;
			
		}

		public bool Resolve() {

			return this.Resolve<IntCondition, int>(this.values, (name) => PlayerPrefs.GetInt(name), (cond, value) => cond.Resolve(value));

		}

		#if UNITY_EDITOR
		public override void OnGUI() {
			
			if (this == null || this.item == null) return;
			
			base.OnGUI();
			this.item.Draw(this.name, this.values, () => {
				
				this.values.Add(new IntCondition(ConditionBase.Type.From | ConditionBase.Type.To, 0, 0));
				
			});
			
		}
		#endif
		
	}
	
	[System.Serializable]
	public class FloatPrefs : PrefsBase {

		public List<FloatCondition> values = new List<FloatCondition>();
		
		public FloatPrefs(string name) : base(name) {}

		public FloatPrefs(FloatPrefsTO source) : base(source.name) {
			
			this.values = new List<FloatCondition>();
			foreach (var item in source.values) this.values.Add(new FloatCondition(item));
			
		}
		
		public FloatPrefsTO GetTO() {
			
			var result = new FloatPrefsTO();
			result.name = this.name;
			result.values = new List<FloatConditionTO>();
			foreach (var item in this.values) result.values.Add(item.GetTO());
			return result;
			
		}

		public bool Resolve() {
			
			return this.Resolve<FloatCondition, float>(this.values, (name) => PlayerPrefs.GetFloat(name), (cond, value) => cond.Resolve(value));

		}

		#if UNITY_EDITOR
		public override void OnGUI() {
			
			if (this == null || this.item == null) return;
			
			base.OnGUI();
			this.item.Draw(this.name, this.values, () => {
				
				this.values.Add(new FloatCondition(ConditionBase.Type.From | ConditionBase.Type.To, 0f, 0f));
				
			});
			
		}
		#endif
		
	}

}