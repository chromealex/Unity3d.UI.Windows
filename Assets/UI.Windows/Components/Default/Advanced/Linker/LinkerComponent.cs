using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.Extensions;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Components {

	public class LinkerComponent : WindowComponent {

		public WindowComponent prefab;

		[HideInInspector]
		private WindowComponent instance;

		public T Get<T>(ref T instance) where T : WindowComponent {
			
			instance = this.instance as T;

			return instance;

		}

		public override void OnInit() {
			
			base.OnInit();

			if (this.instance != null) {

				this.instance.OnInit();

			} else if (this.prefab != null) {
				
				this.instance = this.prefab.Spawn();
				this.instance.SetParent(this, false);
				this.instance.SetTransformAs(this.prefab);
				
				this.instance.Setup(this.GetLayoutRoot());
				this.instance.Setup(this.GetWindow());
				
				this.instance.OnInit();

			}

		}
		
		public override void OnDeinit() {
			
			base.OnDeinit();

			var instance = this.instance;
			if (instance != null) instance.OnDeinit();
			
		}
		
		public override void OnShowBegin() {
			
			base.OnShowBegin();
			
			var instance = this.instance;
			if (instance != null) instance.OnShowBegin();
			
		}
		
		public override void OnShowEnd() {
			
			base.OnShowEnd();
			
			var instance = this.instance;
			if (instance != null) instance.OnShowEnd();
			
		}
		
		public override void OnHideBegin() {
			
			base.OnHideBegin();
			
			var instance = this.instance;
			if (instance != null) instance.OnHideBegin();
			
		}
		
		public override void OnHideEnd() {
			
			base.OnHideEnd();
			
			var instance = this.instance;
			if (instance != null) instance.OnHideEnd();
			
		}

	}

}