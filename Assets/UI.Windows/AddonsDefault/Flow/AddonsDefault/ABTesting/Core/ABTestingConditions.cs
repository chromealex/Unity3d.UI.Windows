using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.ABTesting.Net.Api;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.UI.Windows.Plugins.ABTesting {

	public class Condition2Values : ConditionBase {
		
		public Condition2Values(Type type) : base(type) {}
		
		#if UNITY_EDITOR
		public override void OnContentGUI() {
			
		}
		
		public void Draw2Values(System.Action<string> onLowDraw, System.Action<string> onUpperDraw) {
			
			if (this.HasType(Type.From) == true) onLowDraw(this.lowSign);
			if (this.HasType(Type.From) == true && this.HasType(Type.To) == true) EditorGUILayout.LabelField(" AND "); 
			if (this.HasType(Type.To) == true) onUpperDraw(this.upperSign);
			
			if (this.HasType(Type.Strong) == true && this.HasType(Type.From) == false && this.HasType(Type.To) == false) {
				
				onLowDraw("Value:");
				
			}
			
			
		}
		#endif
		
	};

	[System.Serializable]
	public class EnumCondition : ConditionBase {
		
		public System.Type enumType;
		public int value;
		
		public EnumCondition(Type type, System.Type enumType, int value) : base(type) {
			
			this.enumType = enumType;
			this.value = value;
			
		}
		
		public EnumCondition(EnumConditionTO source) : base((Type)source.type) {
			
			this.value = source.value;
			
		}

		public EnumConditionTO GetTO() {
			
			return new EnumConditionTO(this.type) { value = this.value };
			
		}

		public void SetType(System.Type type) {
			
			this.enumType = type;
			
		}
		
		public bool Resolve(int value) {
			
			var result = true;
			result = ((int)value == this.value);

			return result;
			
		}

		#if UNITY_EDITOR
		public override void OnTypeGUI() {
			
			this.type = Type.Strong;
			
		}
		
		public override void OnContentGUI() {
			
			this.value = EditorGUILayout.Popup("Value: ", this.value, System.Enum.GetNames(this.enumType));
			
		}
		#endif
		
	};

	[System.Serializable]
	public class IntCondition : Condition2Values {
		
		public int fromValue;
		public int toValue;
		
		public IntCondition(Type type, int from, int to) : base(type) {
			
			this.fromValue = from;
			this.toValue = to;
			
		}
		
		public IntCondition(IntConditionTO source) : base((Type)source.type) {
			
			this.fromValue = source.from;
			this.toValue = source.to;
			
		}

		public IntConditionTO GetTO() {
			
			return new IntConditionTO(this.type) { from = this.fromValue, to = this.toValue };
			
		}

		public bool Resolve(int value) {
			
			var result = true;
			var hasStrong = this.HasType(Type.Strong);
			
			if (result == true && this.HasType(Type.From) == true) {
				
				result = (hasStrong == true) ? (value >= this.fromValue) : (value > this.fromValue);
				
			}
			
			if (result == true && this.HasType(Type.To) == true) {
				
				result = (hasStrong == true) ? (value <= this.toValue) : (value < this.toValue);
				
			}
			
			if (result == true && this.HasType(Type.To) == false && this.HasType(Type.From) == false) {
				
				result = (value == this.fromValue);
				
			}
			
			return result;
			
		}

		#if UNITY_EDITOR
		public override void OnContentGUI() {
			
			this.Draw2Values((name) => {
				
				this.fromValue = EditorGUILayout.IntField(name, this.fromValue);
				
			}, (name) => {
				
				this.toValue = EditorGUILayout.IntField(name, this.toValue);
				
			});
			
		}
		#endif
		
	};

	[System.Serializable]
	public class FloatCondition : Condition2Values {
		
		public float fromValue;
		public float toValue;
		
		public FloatCondition(Type type, float from, float to) : base(type) {
			
			this.fromValue = from;
			this.toValue = to;
			
		}
		
		public FloatCondition(FloatConditionTO source) : base((Type)source.type) {
			
			this.fromValue = source.from;
			this.toValue = source.to;
			
		}

		public FloatConditionTO GetTO() {
			
			return new FloatConditionTO(this.type) { from = this.fromValue, to = this.toValue };
			
		}

		public bool Resolve(float value) {
			
			var result = true;
			var hasStrong = this.HasType(Type.Strong);
			
			if (result == true && this.HasType(Type.From) == true) {
				
				result = (hasStrong == true) ? (value >= this.fromValue) : (value > this.fromValue);
				
			}
			
			if (result == true && this.HasType(Type.To) == true) {
				
				result = (hasStrong == true) ? (value <= this.toValue) : (value < this.toValue);
				
			}
			
			if (result == true && this.HasType(Type.To) == false && this.HasType(Type.From) == false) {
				
				result = (value == this.fromValue);
				
			}
			
			return result;
			
		}

		#if UNITY_EDITOR
		public override void OnContentGUI() {
			
			this.Draw2Values((name) => {
				
				this.fromValue = EditorGUILayout.FloatField(name, this.fromValue);
				
			}, (name) => {
				
				this.toValue = EditorGUILayout.FloatField(name, this.toValue);
				
			});
			
		}
		#endif
		
	};

	[System.Serializable]
	public class LongCondition : Condition2Values {
		
		public long fromValue;
		public long toValue;
		
		public LongCondition(Type type, long from, long to) : base(type) {
			
			this.fromValue = from;
			this.toValue = to;
			
		}

		public LongCondition(LongConditionTO source) : base((Type)source.type) {

			this.fromValue = source.from;
			this.toValue = source.to;

		}

		public LongConditionTO GetTO() {

			return new LongConditionTO(this.type) { from = this.fromValue, to = this.toValue };

		}

		public bool Resolve(long value) {

			var result = true;
			var hasStrong = this.HasType(Type.Strong);
			
			if (result == true && this.HasType(Type.From) == true) {
				
				result = (hasStrong == true) ? (value >= this.fromValue) : (value > this.fromValue);
				
			}
			
			if (result == true && this.HasType(Type.To) == true) {
				
				result = (hasStrong == true) ? (value <= this.toValue) : (value < this.toValue);
				
			}
			
			if (result == true && this.HasType(Type.To) == false && this.HasType(Type.From) == false) {
				
				result = (value == this.fromValue);
				
			}

			return result;

		}

		#if UNITY_EDITOR
		public override void OnContentGUI() {
			
			this.Draw2Values((name) => {
				
				this.fromValue = EditorGUILayout.LongField(name, this.fromValue);
				
			}, (name) => {
				
				this.toValue = EditorGUILayout.LongField(name, this.toValue);
				
			});
			
		}
		#endif
		
	};

	[System.Serializable]
	public class DoubleCondition : Condition2Values {
		
		public double fromValue;
		public double toValue;
		
		public DoubleCondition(Type type, double from, double to) : base(type) {
			
			this.fromValue = from;
			this.toValue = to;
			
		}
		
		public DoubleCondition(DoubleConditionTO source) : base((Type)source.type) {
			
			this.fromValue = source.from;
			this.toValue = source.to;
			
		}

		public DoubleConditionTO GetTO() {
			
			return new DoubleConditionTO(this.type) { from = this.fromValue, to = this.toValue };
			
		}

		public bool Resolve(double value) {
			
			var result = true;
			var hasStrong = this.HasType(Type.Strong);
			
			if (result == true && this.HasType(Type.From) == true) {
				
				result = (hasStrong == true) ? (value >= this.fromValue) : (value > this.fromValue);
				
			}
			
			if (result == true && this.HasType(Type.To) == true) {
				
				result = (hasStrong == true) ? (value <= this.toValue) : (value < this.toValue);
				
			}
			
			if (result == true && this.HasType(Type.To) == false && this.HasType(Type.From) == false) {
				
				result = (value == this.fromValue);
				
			}
			
			return result;
			
		}

		#if UNITY_EDITOR
		public override void OnContentGUI() {
			
			this.Draw2Values((name) => {
				
				this.fromValue = EditorGUILayout.DoubleField(name, this.fromValue);
				
			}, (name) => {
				
				this.toValue = EditorGUILayout.DoubleField(name, this.toValue);
				
			});
			
		}
		#endif
		
	};

	[System.Serializable]
	public abstract class ConditionBase : IBaseCondition {
		
		public enum Type : byte {
			
			None = 0x0,
			Strong = 0x1,
			From = 0x2,
			To = 0x4,
			
		};
		
		public Type type = Type.None;
		
		public string lowSign {
			
			get {
				
				if (this.HasType(Type.Strong) == true) return ">=";
				return ">";
				
			}
			
		}
		
		public string upperSign {
			
			get {
				
				if (this.HasType(Type.Strong) == true) return "<=";
				return "<";
				
			}
			
		}
		
		public ConditionBase(Type type) {
			
			this.type = type;
			
		}
		
		protected bool HasType(Type type) {
			
			return (this.type & type) != 0;
			
		}

		#if UNITY_EDITOR
		public virtual void OnTypeGUI() {
			
			GUILayout.BeginHorizontal();
			{
				
				var values = System.Enum.GetValues(typeof(Type));
				var names = System.Enum.GetNames(typeof(Type));
				for (int i = 0; i < values.Length; ++i) {
					
					var value = (Type)values.GetValue(i);
					if (value == Type.None) continue;
					
					var oldCheck = this.HasType(value);
					var check = GUILayout.Toggle(oldCheck, names[i]);
					if (oldCheck != check) {
						
						if (check == true) {
							
							this.type |= value;
							
						} else {
							
							this.type ^= value;
							
						}
						
					}
					
				}
				
			}
			GUILayout.EndHorizontal();
			
			//this.type = (Type)EditorExtension.DrawBitMaskFieldLayout((int)this.type, this.type.GetType(), new GUIContent("Keys:"));
			
		}
		
		public abstract void OnContentGUI();
		
		public void OnGUI() {
			
			this.OnTypeGUI();
			
			EditorGUIUtility.labelWidth = 50f;
			
			GUILayout.BeginVertical(EditorStyles.helpBox);
			{
				
				this.OnContentGUI();
				
			}
			GUILayout.EndVertical();
			
			EditorGUIUtilityExt.LookLikeControls();
			
		}
		#endif
		
	};

}