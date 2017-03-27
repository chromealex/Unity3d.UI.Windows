using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;
using UnityEngine.Extensions;
using UnityEngine.Events;
using UnityEngine.UI.Windows;

namespace UnityEngine.UI.Windows.Plugins.Console.Components {

	public class ConsoleCellComponent : WindowComponent {

		public ButtonComponent buttonComponent;

		private bool reusable = false;
		private ConsoleScreen screen;

		public override void OnInit() {

			base.OnInit();

			this.SetCallback(() => {
				
				this.OnClick();
				
			});

		}

		public void SetScreen(ConsoleScreen screen) {

			this.screen = screen;

		}

		public void SetReusable(bool state) {

			this.reusable = state;
			this.buttonComponent.SetEnabledState(reusable);

		}

		public void SetText(string text) {

			this.buttonComponent.SetText(text);

		}

		public void SetCallback(System.Action onAction) {

			this.buttonComponent.SetCallback(onAction);

		}

		public void OnClick() {

			if (this.reusable == true) {

				var text = this.buttonComponent.GetText();

				if (UnityEngine.Input.GetKey(KeyCode.LeftControl) == true ||
				    UnityEngine.Input.GetKey(KeyCode.RightControl) == true ||
				    UnityEngine.Input.GetKey(KeyCode.LeftCommand) == true ||
				    UnityEngine.Input.GetKey(KeyCode.RightCommand) == true) {
					
					this.screen.SetInputText(text);

				} else {

					this.screen.AddLineCmd(text);

				}

			}

		}

	}

}