using UnityEngine;
using System.Collections.Generic;
using Tacticsoft;

namespace Tacticsoft.Examples
{
    //An example implementation of a class that communicates with a TableView
    public class DynamicHeightTableViewController : MonoBehaviour, ITableViewDataSource
    {
        public DynamicHeightCell m_cellPrefab;
        public TableView m_tableView;

        public int m_numRows;
        private int m_numInstancesCreated = 0;

        private Dictionary<int, float> m_customRowHeights;

        //Register as the TableView's delegate (required) and data source (optional)
        //to receive the calls
        void Start() {
            m_customRowHeights = new Dictionary<int, float>();
            m_tableView.dataSource = this;
        }

        public void SendBeer() {
            Application.OpenURL("https://www.paypal.com/cgi-bin/webscr?business=contact@tacticsoft.net&cmd=_xclick&item_name=Beer%20for%20TSTableView&currency_code=USD&amount=5.00");
        }

        #region ITableViewDataSource

        //Will be called by the TableView to know how many rows are in this table
        public int GetNumberOfRowsForTableView(TableView tableView) {
            return m_numRows;
        }

        //Will be called by the TableView to know what is the height of each row
        public float GetHeightForRowInTableView(TableView tableView, int row) {
            return GetHeightOfRow(row);
        }

        //Will be called by the TableView when a cell needs to be created for display
        public TableViewCell GetCellForRowInTableView(TableView tableView, int row) {
            DynamicHeightCell cell = tableView.GetReusableCell(m_cellPrefab.reuseIdentifier) as DynamicHeightCell;
            if (cell == null) {
                cell = (DynamicHeightCell)GameObject.Instantiate(m_cellPrefab);
                cell.name = "DynamicHeightCellInstance_" + (++m_numInstancesCreated).ToString();
                cell.onCellHeightChanged.AddListener(OnCellHeightChanged);
            }
            cell.rowNumber = row;
            cell.height = GetHeightOfRow(row);
            return cell;
        }

        #endregion

        private float GetHeightOfRow(int row) {
            if (m_customRowHeights.ContainsKey(row)) {
                return m_customRowHeights[row];
            } else {
                return m_cellPrefab.height;
            }
        }

        private void OnCellHeightChanged(int row, float newHeight) {
            if (GetHeightOfRow(row) == newHeight) {
                return;
            }
            //Debug.Log(string.Format("Cell {0} height changed to {1}", row, newHeight));
            m_customRowHeights[row] = newHeight;
            m_tableView.NotifyCellDimensionsChanged(row);
        }

    }
}
