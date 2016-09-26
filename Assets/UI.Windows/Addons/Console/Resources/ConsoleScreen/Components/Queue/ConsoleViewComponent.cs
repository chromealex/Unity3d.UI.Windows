using UnityEngine;
using UnityEngine.UI.Windows;
using UnityEngine.Extensions;
using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows.Plugins.Console.Components {

	public class ConsoleViewComponent : ListViewComponent {

		private ConsoleScreen screen;
		
		public override float GetRowHeight(int row) {
			
			return this.screen.GetQueueLineHeight(row);
			
		}

		public void SetScreen(ConsoleScreen screen) {

			this.screen = screen;
			this.UpdateQueue();

		}

		public void Add() {

			this.AddItem((element, index) => {
				
				var reusable = this.screen.IsReusable(index);
				var cell = element as ConsoleCellComponent;

				cell.SetScreen(this.screen);
				cell.SetReusable(reusable);
				cell.SetText(this.screen.GetQueueText(index));

			});

			this.UpdateQueue();

		}
		
		public void UpdateQueue() {
			
			//this.Show(resetAnimation: false);
			
			if (this.screen.GetQueueCount() > 0) {
				
				this.ReloadData();
				this.scrollY = this.GetScrollYForRow(this.screen.GetQueueCount() - 1, above: false) - (this.transform as RectTransform).rect.height;
				
			}
			
		}

	}
	
}