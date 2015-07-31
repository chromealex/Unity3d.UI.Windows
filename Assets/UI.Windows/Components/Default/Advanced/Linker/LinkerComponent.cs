using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.Extensions;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Components {

	public class LinkerComponent : WindowComponent {

		[ComponentChooser]
		public WindowComponent prefab;

		[HideInInspector]
		private WindowComponent instance;

		public T Get<T>(ref T instance) where T : IComponent {
			
			instance = this.Get<T>();

			return instance;

		}

		public T Get<T>() where T : IComponent {

			if (this.instance == null) {

				this.InitInstance();

			}

			return (T)(this.instance as IComponent);

		}
		
		public void InitPool(int capacity) {
			
			this.prefab.CreatePool(capacity, this.transform);
			if (this.prefab is LinkerComponent) {
				
				(this.prefab as LinkerComponent).InitPool(capacity);
				
			}
			
		}

		public override void OnInit() {
			
			base.OnInit();

			this.InitInstance();

		}

		private void InitInstance() {

			if (this.instance != null) {

				this.instance.OnInit();

			} else if (this.prefab != null) {

				this.instance = this.prefab.Spawn();
				this.instance.SetParent(this, false);
				this.instance.SetTransformAs(this.prefab);
				
				this.instance.Setup(this.GetLayoutRoot());
				this.instance.Setup(this.GetWindow());

				this.RegisterSubComponent(this.instance);

				//this.instance.OnInit();

			}

		}
		/*
		public override void OnDeinit() {
			
			base.OnDeinit();

			var instance = this.instance;
			if (instance != null) instance.OnDeinit();
			
		}
		
		public override void OnShowBegin(System.Action callback, bool resetAnimation = true) {
			
			var counter = 0;
			System.Action callbackItem = () => {
				
				++counter;
				if (counter < 2) return;
				
				if (callback != null) callback();
				
			};
			
			base.OnShowBegin(callbackItem, resetAnimation);
			this.instance.OnShowBegin(callbackItem, resetAnimation);

		}
		
		public override void OnShowEnd() {
			
			base.OnShowEnd();
			
			var instance = this.instance;
			if (instance != null) instance.OnShowEnd();
			
		}
		
		public override void OnHideBegin(System.Action callback, bool immediately = false) {
			
			var counter = 0;
			System.Action callbackItem = () => {
				
				++counter;
				if (counter < 2) return;
				
				if (callback != null) callback();
				
			};
			
			base.OnHideBegin(callbackItem, immediately);
			this.instance.OnHideBegin(callbackItem, immediately);

		}
		
		public override void OnHideEnd() {
			
			base.OnHideEnd();
			
			var instance = this.instance;
			if (instance != null) instance.OnHideEnd();
			
		}*/

	}

}