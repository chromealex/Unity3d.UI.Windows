using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.Extensions;

namespace UnityEngine.UI.Windows.Components {

	public class LinkerComponent : WindowComponent {

		public WindowComponent prefab;

		private WindowComponent instance;

		public override void OnInit() {
			
			base.OnInit();

			if (this.prefab != null) {

				this.instance = this.prefab.Spawn();
				this.instance.SetParent(this, false);
				this.instance.SetTransformAs(this.prefab);

				this.instance.OnInit();

			}

		}
		
		public override void OnDeinit() {
			
			base.OnDeinit();
			
			if (this.instance != null) this.instance.OnDeinit();
			
		}
		
		public override void OnShowBegin() {
			
			base.OnShowBegin();
			
			if (this.instance != null) this.instance.OnShowBegin();
			
		}
		
		public override void OnShowEnd() {
			
			base.OnShowEnd();
			
			if (this.instance != null) this.instance.OnShowEnd();
			
		}
		
		public override void OnHideBegin() {
			
			base.OnHideBegin();
			
			if (this.instance != null) this.instance.OnHideBegin();
			
		}
		
		public override void OnHideEnd() {
			
			base.OnHideEnd();
			
			if (this.instance != null) this.instance.OnHideEnd();
			
		}

	}

}