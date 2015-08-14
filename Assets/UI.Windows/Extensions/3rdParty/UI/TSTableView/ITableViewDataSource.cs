using UnityEngine;
using System.Collections;

namespace Tacticsoft
{
    /// <summary>
    /// An interface for a data source to a TableView
    /// </summary>
    public interface ITableViewDataSource
    {
        /// <summary>
        /// Get the number of rows that a certain table should display
        /// </summary>
        int GetNumberOfRowsForTableView(TableView tableView);
        
        /// <summary>
        /// Get the height of a row of a certain cell in the table view
        /// </summary>
        float GetHeightForRowInTableView(TableView tableView, int row);

        /// <summary>
        /// Create a cell for a certain row in a table view.
        /// Callers should use tableView.GetReusableCell to cache objects
        /// </summary>
        TableViewCell GetCellForRowInTableView(TableView tableView, int row);
    }
}

