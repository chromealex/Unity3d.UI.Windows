using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Components {

	public interface IListViewHandler {

		void OnItemBecomeVisible();
		void OnItemBecomeInvisible();

	}

	public interface IListViewDataSource {

		/// <summary>
		/// Gets the rows count.
		/// </summary>
		/// <returns>The rows count.</returns>
		int GetRowsCount();
        
		/// <summary>
		/// Gets the height of the row.
		/// </summary>
		/// <returns>The row height.</returns>
		/// <param name="row">Row.</param>
		float GetRowHeight(int row);

		/// <summary>
		/// Gets the cell instance.
		/// </summary>
		/// <returns>The cell instance.</returns>
		/// <param name="row">Row.</param>
		WindowComponent GetRowInstance(int row);

	}

	public interface IListViewItem {

		string reuseIdentifier {
			get;
		}

	}

}

