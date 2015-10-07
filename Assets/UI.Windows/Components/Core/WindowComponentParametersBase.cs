using UnityEngine;
using System.Collections;
using System;

namespace UnityEngine.UI.Windows.Components {
	
	public static class ParameterFlag {
		
		public const ulong None = 0x0;
		
		public const ulong P1 = 0x00001;
		public const ulong P2 = 0x00002;
		public const ulong P3 = 0x00004;
		public const ulong P4 = 0x00008;
		
		public const ulong P5 = 0x00010;
		public const ulong P6 = 0x00020;
		public const ulong P7 = 0x00040;
		public const ulong P8 = 0x00080;
		
		public const ulong P9 = 0x00100;
		public const ulong P10 = 0x00200;
		public const ulong P11 = 0x00400;
		public const ulong P12 = 0x00800;
		
		public const ulong P13 = 0x01000;
		public const ulong P14 = 0x02000;
		public const ulong P15 = 0x04000;
		public const ulong P16 = 0x08000;
		
		public const ulong P17 = 0x10000;
		public const ulong P18 = 0x20000;
		public const ulong P19 = 0x40000;
		public const ulong P20 = 0x80000;
		
		public const ulong AP1 = 0x100000;
		public const ulong AP2 = 0x200000;
		public const ulong AP3 = 0x400000;
		public const ulong AP4 = 0x800000;
		
		public const ulong AP5 = 0x1000000;
		public const ulong AP6 = 0x2000000;
		public const ulong AP7 = 0x4000000;
		public const ulong AP8 = 0x8000000;
		
		public const ulong AP9 = 0x10000000;
		public const ulong AP10 = 0x20000000;
		public const ulong AP11 = 0x40000000;
		public const ulong AP12 = 0x80000000;
		
		public const ulong AP13 = 0x100000000;
		public const ulong AP14 = 0x200000000;
		public const ulong AP15 = 0x400000000;
		public const ulong AP16 = 0x800000000;
		
		public const ulong AP17 = 0x1000000000;
		public const ulong AP18 = 0x2000000000;
		public const ulong AP19 = 0x4000000000;
		public const ulong AP20 = 0x8000000000;
		
		public const ulong BP1 = 0x10000000000;
		public const ulong BP2 = 0x20000000000;
		public const ulong BP3 = 0x40000000000;
		public const ulong BP4 = 0x80000000000;
		
		public const ulong BP5 = 0x100000000000;
		public const ulong BP6 = 0x200000000000;
		public const ulong BP7 = 0x400000000000;
		public const ulong BP8 = 0x800000000000;
		
		public const ulong BP9 = 0x1000000000000;
		public const ulong BP10 = 0x2000000000000;
		public const ulong BP11 = 0x4000000000000;
		public const ulong BP12 = 0x8000000000000;
		
		public const ulong BP13 = 0x10000000000000;
		public const ulong BP14 = 0x20000000000000;
		public const ulong BP15 = 0x40000000000000;
		public const ulong BP16 = 0x80000000000000;
		
		public const ulong BP17 = 0x100000000000000;
		public const ulong BP18 = 0x200000000000000;
		public const ulong BP19 = 0x400000000000000;
		public const ulong BP20 = 0x800000000000000;
		
	};

	public interface IParametersEditor {

		void OnParametersGUI(Rect rect);
		float GetHeight();

	}

	public class ParamFlagAttribute : PropertyAttribute {

		public ulong flag;

		public ParamFlagAttribute(ulong flag) {

			this.flag = flag;

		}

	}

	public interface IComponentParameters {
	};

	public class WindowComponentParametersBase : MonoBehaviour, IComponentParameters {

		[HideInInspector][SerializeField]
		//private ParameterFlag flags;
		private ulong flags;

		public void Setup(WindowComponent component) {

			// If there was no implementation

		}

		public ulong GetFlags() {

			return this.flags;

		}

		public bool IsChanged(ulong value) {

			return (this.flags & value) != 0;

		}

		public void SetChanged(ulong flag, bool state) {

			if (state == true) {

				this.flags |= flag;

			} else {

				//this.flags &= ~(flag);
				this.flags ^= flag;

			}

		}

	}

}