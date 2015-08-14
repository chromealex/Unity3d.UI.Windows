using UnityEngine;
using System.Collections;
using Tacticsoft;
using UnityEngine.UI;

namespace Tacticsoft.Examples
{
    //Inherit from TableViewCell instead of MonoBehavior to use the GameObject
    //containing this component as a cell in a TableView
    public class VisibleCounterCell : TableViewCell
    {
        public Text m_rowNumberText;
        public Text m_visibleCountText;

        public void SetRowNumber(int rowNumber) {
            m_rowNumberText.text = "Row " + rowNumber.ToString();
        }

        private int m_numTimesBecameVisible;
        public void NotifyBecameVisible() {
            m_numTimesBecameVisible++;
            m_visibleCountText.text = "# rows this cell showed : " + m_numTimesBecameVisible.ToString();
        }

    }
}
