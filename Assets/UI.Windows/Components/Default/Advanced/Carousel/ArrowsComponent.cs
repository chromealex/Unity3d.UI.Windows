
namespace UnityEngine.UI.Windows.Components {

	public class ArrowsComponent : WindowComponent {

		public ButtonComponent prevButton;
		public ButtonComponent nextButton;
		private CarouselComponent sourceList;

		public void OnSelect(int index) {

			this.prevButton.SetEnabledState(index > 0);
			this.nextButton.SetEnabledState(index < this.sourceList.Count() - 1);

		}

		public void Refresh(CarouselComponent sourceList) {

			this.sourceList = sourceList;

			this.prevButton.SetCallback(this.OnPrevClick);
			this.nextButton.SetCallback(this.OnNextClick);

			this.OnSelect(this.sourceList.GetCurrentIndex());

		}

		private void OnPrevClick() {

			this.sourceList.MovePrev();

		}

		private void OnNextClick() {

			this.sourceList.MoveNext();

		}

	}

}

