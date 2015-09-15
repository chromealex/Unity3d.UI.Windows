using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Components {

	public interface IListViewDataSource {

		/// <summary>
		/// Gets the rows count.
		/// </summary>
		/// <returns>The rows count.</returns>
		/// <param name="view">View.</param>
		int GetRowsCount(ListViewComponent view);
        
		/// <summary>
		/// Gets the height of the row.
		/// </summary>
		/// <returns>The row height.</returns>
		/// <param name="view">View.</param>
		/// <param name="row">Row.</param>
		float GetRowHeight(ListViewComponent view, int row);

		/// <summary>
		/// Gets the cell instance.
		/// </summary>
		/// <returns>The cell instance.</returns>
		/// <param name="view">View.</param>
		/// <param name="row">Row.</param>
		WindowComponent GetRowInstance(ListViewComponent view, int row);

	}

	public interface IListViewItem {

		string reuseIdentifier {
			
			get;
			set;

		}

	}

}

