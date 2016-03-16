using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;

namespace UnityEngine.UI.Windows.Components {

	public class DecoratorComponent : ImageComponent {

		public override void OnShowBegin() {

			base.OnShowBegin();

			if (this.IsMovie() == true) {

				if (this.GetPlayOnShow() == true) this.Play();

			}

		}

	}

}