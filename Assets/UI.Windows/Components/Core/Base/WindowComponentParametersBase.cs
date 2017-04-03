using UnityEngine;
using System.Collections;
using System;

namespace UnityEngine.UI.Windows.Components {
	
	public static class ParameterFlag {
		
		public const long None = 0x0;
		
		public const long P1 = 0x00001;
		public const long P2 = 0x00002;
		public const long P3 = 0x00004;
		public const long P4 = 0x00008;
		
		public const long P5 = 0x00010;
		public const long P6 = 0x00020;
		public const long P7 = 0x00040;
		public const long P8 = 0x00080;
		
		public const long P9 = 0x00100;
		public const long P10 = 0x00200;
		public const long P11 = 0x00400;
		public const long P12 = 0x00800;
		
		public const long P13 = 0x01000;
		public const long P14 = 0x02000;
		public const long P15 = 0x04000;
		public const long P16 = 0x08000;
		
		public const long P17 = 0x10000;
		public const long P18 = 0x20000;
		public const long P19 = 0x40000;
		public const long P20 = 0x80000;
		
		public const long AP1 = 0x100000;
		public const long AP2 = 0x200000;
		public const long AP3 = 0x400000;
		public const long AP4 = 0x800000;
		
		public const long AP5 = 0x1000000;
		public const long AP6 = 0x2000000;
		public const long AP7 = 0x4000000;
		public const long AP8 = 0x8000000;
		
		public const long AP9 = 0x10000000;
		public const long AP10 = 0x20000000;
		public const long AP11 = 0x40000000;
		public const long AP12 = 0x80000000;
		
		public const long AP13 = 0x100000000;
		public const long AP14 = 0x200000000;
		public const long AP15 = 0x400000000;
		public const long AP16 = 0x800000000;
		
		public const long AP17 = 0x1000000000;
		public const long AP18 = 0x2000000000;
		public const long AP19 = 0x4000000000;
		public const long AP20 = 0x8000000000;
		
		public const long BP1 = 0x10000000000;
		public const long BP2 = 0x20000000000;
		public const long BP3 = 0x40000000000;
		public const long BP4 = 0x80000000000;
		
		public const long BP5 = 0x100000000000;
		public const long BP6 = 0x200000000000;
		public const long BP7 = 0x400000000000;
		public const long BP8 = 0x800000000000;
		
		public const long BP9 = 0x1000000000000;
		public const long BP10 = 0x2000000000000;
		public const long BP11 = 0x4000000000000;
		public const long BP12 = 0x8000000000000;
		
		public const long BP13 = 0x10000000000000;
		public const long BP14 = 0x20000000000000;
		public const long BP15 = 0x40000000000000;
		public const long BP16 = 0x80000000000000;
		
		public const long BP17 = 0x100000000000000;
		public const long BP18 = 0x200000000000000;
		public const long BP19 = 0x400000000000000;
		public const long BP20 = 0x800000000000000;
		
	};

	public interface IParametersEditor {

		void OnParametersGUI(Rect rect);
		float GetHeight();

	}

	public class ParamFlagAttribute : PropertyAttribute {

		public long flag;

		public ParamFlagAttribute(long flag) {

			this.flag = flag;

		}

	}

	public interface IComponentParameters {
	};

	public class WindowComponentParametersBase : MonoBehaviour, IComponentParameters/*, ISerializationCallbackReceiver*/ {

		//[HideInInspector]
		//[System.NonSerialized]
		private long flags {

			get {

				return this.OnAfterDeserialize();

			}

			set {

				this.OnBeforeSerialize(value);

			}

		}
		//[HideInInspector]
		[SerializeField]
		private int flags1;
		//[HideInInspector]
		[SerializeField]
		private int flags2;

		public void Setup(WindowComponent component) {

			// If there was no implementation

		}

		public long GetFlags() {

			return this.flags;

		}

		public bool IsChanged(long value) {

			return (this.flags & value) != 0;

		}

		public void SetChanged(long flag, bool state) {

			if (state == true) {

				this.flags |= flag;

			} else {

				//this.flags &= ~(flag);
				this.flags ^= flag;

			}

		}

		public long OnAfterDeserialize() {
			/*
			if (this.flags1 == 0 && this.flags2 == 0) {

				this.OnBeforeSerialize();

			}*/

			long flags = this.flags2;
			flags = flags << 32;
			flags = flags | (uint)this.flags1;

			return flags;

		}

		public void OnBeforeSerialize(long flags) {
			
			this.flags1 = (int)(flags & uint.MaxValue);
			this.flags2 = (int)(flags >> 32);

		}

	}

}