using UnityEngine;
using System.Collections;
using Tacticsoft;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Tacticsoft.Examples
{
    //Inherit from TableViewCell instead of MonoBehavior to use the GameObject
    //containing this component as a cell in a TableView
    public class DynamicHeightCell : TableViewCell
    {
        public Text m_rowNumberText;
        public Slider m_cellHeightSlider;

        public int rowNumber { get; set; }

        [System.Serializable]
        public class CellHeightChangedEvent : UnityEvent<int, float> { }
        public CellHeightChangedEvent onCellHeightChanged;

        void Update() {
            m_rowNumberText.text = "Row " + rowNumber.ToString();
        }

        public void SliderValueChanged(float value) {
            onCellHeightChanged.Invoke(rowNumber, value);
        }

        public float height {
            get { return m_cellHeightSlider.value; }
            set { m_cellHeightSlider.value = value; }
        }


    }
}
