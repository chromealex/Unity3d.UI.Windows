
namespace UnityEngine.UI.Windows.Components {

	public class DotsComponent : ListComponent {

		public int minCount = 2;
		private CarouselComponent sourceList;
		private IButtonComponent previousDot;

		public void OnSelect(int index) {

			if (this.previousDot != null) this.previousDot.SetEnabled();

			var button = this.GetItem<IButtonComponent>(index);
			if (button != null) {
				
				this.previousDot = button;
				button.SetDisabled();

			}

		}

		public void Refresh(CarouselComponent sourceList) {

			this.sourceList = sourceList;

			var items = this.sourceList.GetItems();
			if (items.Count <= this.minCount) {

				this.Hide();
				return;

			}

			this.SetItems<IButtonComponent>(items.Count, (item, index) => {

				item.SetCallback(() => this.OnClickElement(index));
				item.SetEnabled();

			});
			this.OnSelect(this.sourceList.GetCurrentIndex());

		}

		private void OnClickElement(int index) {

			this.sourceList.MoveTo(index);

		}

	}

}

