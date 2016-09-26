using UnityEngine;
using UnityEngine.UI.Windows;

namespace ExampleProject.Gameplay.GameplayView.Components {

	public class GameplayComponent : WindowComponent {

		public Material material;

		public void SetColor(Color color) {

			this.material.color = color;

		}

	}

}