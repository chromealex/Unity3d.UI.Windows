using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
	public class ToggleGroupExtended : UIBehaviour
	{
		//
		// Fields
		//
		[SerializeField]
		private bool m_AllowSwitchOff;

		private List<ToggleExtended> m_Toggles = new List<ToggleExtended> ();

		//
		// Properties
		//
		public bool allowSwitchOff {
			get {
				return this.m_AllowSwitchOff;
			}
			set {
				this.m_AllowSwitchOff = value;
			}
		}

		//
		// Methods
		//
		public IEnumerable<ToggleExtended> ActiveToggles ()
		{
			return from x in this.m_Toggles
					where x.isOn
				select x;
		}

		public bool AnyTogglesOn ()
		{
			return this.m_Toggles.Find ((ToggleExtended x) => x.isOn) != null;
		}

		public void NotifyToggleOn (ToggleExtended toggle)
		{
			this.ValidateToggleIsInGroup (toggle);
			for (int i = 0; i < this.m_Toggles.Count; i++) {
				if (!(this.m_Toggles [i] == toggle)) {
					this.m_Toggles [i].isOn = false;
				}
			}
		}

		public void RegisterToggle (ToggleExtended toggle)
		{
			if (!this.m_Toggles.Contains (toggle)) {
				this.m_Toggles.Add (toggle);
			}
		}

		public void SetAllTogglesOff ()
		{
			bool allowSwitchOff = this.m_AllowSwitchOff;
			this.m_AllowSwitchOff = true;
			for (int i = 0; i < this.m_Toggles.Count; i++) {
				this.m_Toggles [i].isOn = false;
			}
			this.m_AllowSwitchOff = allowSwitchOff;
		}

		public void UnregisterToggle (ToggleExtended toggle)
		{
			if (this.m_Toggles.Contains (toggle)) {
				this.m_Toggles.Remove (toggle);
			}
		}

		private void ValidateToggleIsInGroup (ToggleExtended toggle)
		{
			if (toggle == null || !this.m_Toggles.Contains (toggle)) {
				throw new ArgumentException (string.Format ("ToggleExtended {0} is not part of ToggleGroup {1}", new object[] {
					toggle,
					this
				}));
			}
		}
	}
}
