using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;

namespace Tacticsoft
{
    /// <summary>
    /// A reusable table for for (vertical) tables. API inspired by Cocoa's UITableView
    /// Hierarchy structure should be :
    /// GameObject + TableView (this) + Mask + Scroll Rect (point to child)
    /// - Child GameObject + Vertical Layout Group
    /// This class should be after Unity's internal UI components in the Script Execution Order
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class TableView : MonoBehaviour
    {

        #region Public API
        /// <summary>
        /// The data source that will feed this table view with information. Required.
        /// </summary>
        public ITableViewDataSource dataSource
        {
            get { return m_dataSource; }
            set { m_dataSource = value; m_requiresReload = true; }
        }
        
        [System.Serializable]
        public class CellVisibilityChangeEvent : UnityEvent<int, bool> { }
        /// <summary>
        /// This event will be called when a cell's visibility changes
        /// First param (int) is the row index, second param (bool) is whether or not it is visible
        /// </summary>
        public CellVisibilityChangeEvent onCellVisibilityChanged;

        /// <summary>
        /// Get a cell that is no longer in use for reusing
        /// </summary>
        /// <param name="reuseIdentifier">The identifier for the cell type</param>
        /// <returns>A prepared cell if available, null if none</returns>
        public TableViewCell GetReusableCell(string reuseIdentifier) {
            LinkedList<TableViewCell> cells;
            if (!m_reusableCells.TryGetValue(reuseIdentifier, out cells)) {
                return null;
            }
            if (cells.Count == 0) {
                return null;
            }
            TableViewCell cell = cells.First.Value;
            cells.RemoveFirst();
            return cell;
        }

        public bool isEmpty { get; private set; }

        /// <summary>
        /// Reload the table view. Manually call this if the data source changed in a way that alters the basic layout
        /// (number of rows changed, etc)
        /// </summary>
        public void ReloadData() {
            m_rowHeights = new float[m_dataSource.GetNumberOfRowsForTableView(this)];
            this.isEmpty = m_rowHeights.Length == 0;
            if (this.isEmpty) {
                ClearAllRows();
                return;
            }
            m_cumulativeRowHeights = new float[m_rowHeights.Length];
            m_cleanCumulativeIndex = -1;

            for (int i = 0; i < m_rowHeights.Length; i++) {
                m_rowHeights[i] = m_dataSource.GetHeightForRowInTableView(this, i);
                if (i > 0) {
                    m_rowHeights[i] += m_verticalLayoutGroup.spacing;
                }
            }

            m_scrollRect.content.sizeDelta = new Vector2(m_scrollRect.content.sizeDelta[0], 
                GetCumulativeRowHeight(m_rowHeights.Length - 1));

            RecalculateVisibleRowsFromScratch();
            m_requiresReload = false;
        }

        /// <summary>
        /// Get cell at a specific row (if active). Returns null if not.
        /// </summary>
        public TableViewCell GetCellAtRow(int row)
        {
            TableViewCell retVal = null;
            m_visibleCells.TryGetValue(row, out retVal);
            return retVal;
        }

        /// <summary>
        /// Get the range of the currently visible rows
        /// </summary>
        public Range visibleRowRange {
            get { return m_visibleRowRange; }
        }

        /// <summary>
        /// Notify the table view that one of its rows changed size
        /// </summary>
        public void NotifyCellDimensionsChanged(int row) {
            float oldHeight = m_rowHeights[row];
            m_rowHeights[row] = m_dataSource.GetHeightForRowInTableView(this, row);
            m_cleanCumulativeIndex = Mathf.Min(m_cleanCumulativeIndex, row - 1);
            if (m_visibleRowRange.Contains(row)) {
                TableViewCell cell = GetCellAtRow(row);
                cell.GetComponent<LayoutElement>().preferredHeight = m_rowHeights[row];
                if (row > 0) {
                    cell.GetComponent<LayoutElement>().preferredHeight -= m_verticalLayoutGroup.spacing;
                }
            }
            float heightDelta = m_rowHeights[row] - oldHeight;
            m_scrollRect.content.sizeDelta = new Vector2(m_scrollRect.content.sizeDelta.x,
                m_scrollRect.content.sizeDelta.y + heightDelta);
            m_requiresRefresh = true;
        }

        /// <summary>
        /// Get the maximum scrollable height of the table. scrollY property will never be more than this.
        /// </summary>
        public float scrollableHeight {
            get {
                return m_scrollRect.content.rect.height - (this.transform as RectTransform).rect.height;
            }
        }

        /// <summary>
        /// Get or set the current scrolling position of the table
        /// </summary>
        public float scrollY {
            get {
                return m_scrollY;
            }
            set {
                if (this.isEmpty) {
                    return;
                }
                value = Mathf.Clamp(value, 0, GetScrollYForRow(m_rowHeights.Length - 1, true));
                if (m_scrollY != value) {
                    m_scrollY = value;
                    m_requiresRefresh = true;
                    float relativeScroll = value / this.scrollableHeight;
                    m_scrollRect.verticalNormalizedPosition = 1 - relativeScroll;
                }
            }
        }

        /// <summary>
        /// Get the y that the table would need to scroll to to have a certain row at the top
        /// </summary>
        /// <param name="row">The desired row</param>
        /// <param name="above">Should the top of the table be above the row or below the row?</param>
        /// <returns>The y position to scroll to, can be used with scrollY property</returns>
        public float GetScrollYForRow(int row, bool above) {
            float retVal = GetCumulativeRowHeight(row);
            if (above) {
                retVal -= m_rowHeights[row];
            }
            return retVal;
        }

        #endregion

        #region Private implementation

        private ITableViewDataSource m_dataSource;
        private bool m_requiresReload;

        private VerticalLayoutGroup m_verticalLayoutGroup;
        private ScrollRect m_scrollRect;
        private LayoutElement m_topPadding;
        private LayoutElement m_bottomPadding;

        private float[] m_rowHeights;
        //cumulative[i] = sum(rowHeights[j] for 0 <= j <= i)
        private float[] m_cumulativeRowHeights;
        private int m_cleanCumulativeIndex;

        private Dictionary<int, TableViewCell> m_visibleCells;
        private Range m_visibleRowRange;

        private RectTransform m_reusableCellContainer;
        private Dictionary<string, LinkedList<TableViewCell>> m_reusableCells;

        private float m_scrollY;

        private bool m_requiresRefresh;

        private void ScrollViewValueChanged(Vector2 newScrollValue) {
            float relativeScroll = 1 - newScrollValue.y;
            m_scrollY = relativeScroll * scrollableHeight;
            m_requiresRefresh = true;
            //Debug.Log(m_scrollY.ToString(("0.00")));
        }

        private void RecalculateVisibleRowsFromScratch() {
            ClearAllRows();
            SetInitialVisibleRows();
        }

        private void ClearAllRows() {
            while (m_visibleCells.Count > 0) {
                HideRow(false);
            }
            m_visibleRowRange = new Range(0, 0);
        }

        void Awake()
        {
            isEmpty = true;
            m_scrollRect = GetComponent<ScrollRect>();
            m_verticalLayoutGroup = GetComponentInChildren<VerticalLayoutGroup>();
            m_topPadding = CreateEmptyPaddingElement("TopPadding");
            m_topPadding.transform.SetParent(m_scrollRect.content, false);
            m_bottomPadding = CreateEmptyPaddingElement("Bottom");
            m_bottomPadding.transform.SetParent(m_scrollRect.content, false);
            m_visibleCells = new Dictionary<int, TableViewCell>();

            m_reusableCellContainer = new GameObject("ReusableCells", typeof(RectTransform)).GetComponent<RectTransform>();
            m_reusableCellContainer.SetParent(this.transform, false);
            m_reusableCellContainer.gameObject.SetActive(false);
            m_reusableCells = new Dictionary<string, LinkedList<TableViewCell>>();
        }
        
        void Update()
        {
            if (m_requiresReload) {
                ReloadData();
            }
        }

        void LateUpdate() {
            if (m_requiresRefresh) {
                RefreshVisibleRows();
            }
        }

        void OnEnable() {
            m_scrollRect.onValueChanged.AddListener(ScrollViewValueChanged);
        }

        void OnDisable() {
            m_scrollRect.onValueChanged.RemoveListener(ScrollViewValueChanged);
        }
        
        private Range CalculateCurrentVisibleRowRange()
        {
            float startY = m_scrollY;
            float endY = m_scrollY + (this.transform as RectTransform).rect.height;
            int startIndex = FindIndexOfRowAtY(startY);
            int endIndex = FindIndexOfRowAtY(endY);
            return new Range(startIndex, endIndex - startIndex + 1);
        }

        private void SetInitialVisibleRows()
        {
            Range visibleRows = CalculateCurrentVisibleRowRange();
            for (int i = 0; i < visibleRows.count; i++)
            {
                AddRow(visibleRows.from + i, true);
            }
            m_visibleRowRange = visibleRows;
            UpdatePaddingElements();
        }

        private void AddRow(int row, bool atEnd)
        {
            TableViewCell newCell = m_dataSource.GetCellForRowInTableView(this, row);
            newCell.transform.SetParent(m_scrollRect.content, false);

            LayoutElement layoutElement = newCell.GetComponent<LayoutElement>();
            if (layoutElement == null) {
                layoutElement = newCell.gameObject.AddComponent<LayoutElement>();
            }
            layoutElement.preferredHeight = m_rowHeights[row];
            if (row > 0) {
                layoutElement.preferredHeight -= m_verticalLayoutGroup.spacing;
            }
            
            m_visibleCells[row] = newCell;
            if (atEnd) {
                newCell.transform.SetSiblingIndex(m_scrollRect.content.childCount - 2); //One before bottom padding
            } else {
                newCell.transform.SetSiblingIndex(1); //One after the top padding
            }
            this.onCellVisibilityChanged.Invoke(row, true);
        }

        private void RefreshVisibleRows()
        {
            m_requiresRefresh = false;

            if (this.isEmpty) {
                return;
            }

            Range newVisibleRows = CalculateCurrentVisibleRowRange();
            int oldTo = m_visibleRowRange.Last();
            int newTo = newVisibleRows.Last();

            if (newVisibleRows.from > oldTo || newTo < m_visibleRowRange.from) {
                //We jumped to a completely different segment this frame, destroy all and recreate
				RecalculateVisibleRowsFromScratch();
                return;
            }

            //Remove rows that disappeared to the top
            for (int i = m_visibleRowRange.from; i < newVisibleRows.from; i++)
            {
                HideRow(false);
            }
            //Remove rows that disappeared to the bottom
            for (int i = newTo; i < oldTo; i++)
            {
                HideRow(true);
            }
            //Add rows that appeared on top
            for (int i = m_visibleRowRange.from - 1; i >= newVisibleRows.from; i--) {
                AddRow(i, false);
            }
            //Add rows that appeared on bottom
            for (int i = oldTo + 1; i <= newTo; i++) {
                AddRow(i, true);
            }
            m_visibleRowRange = newVisibleRows;
            UpdatePaddingElements();
        }

        private void UpdatePaddingElements() {
            float hiddenElementsHeightSum = 0;
            for (int i = 0; i < m_visibleRowRange.from; i++) {
                hiddenElementsHeightSum += m_rowHeights[i];
            }
            m_topPadding.preferredHeight = hiddenElementsHeightSum;
            m_topPadding.gameObject.SetActive(m_topPadding.preferredHeight > 0);
            for (int i = m_visibleRowRange.from; i <= m_visibleRowRange.Last(); i++) {
                hiddenElementsHeightSum += m_rowHeights[i];
            }
            float bottomPaddingHeight = m_scrollRect.content.rect.height - hiddenElementsHeightSum;
            m_bottomPadding.preferredHeight = bottomPaddingHeight - m_verticalLayoutGroup.spacing;
            m_bottomPadding.gameObject.SetActive(m_bottomPadding.preferredHeight > 0);
        }

        private void HideRow(bool last)
        {
            //Debug.Log("Hiding row at scroll y " + m_scrollY.ToString("0.00"));

            int row = last ? m_visibleRowRange.Last() : m_visibleRowRange.from;
            TableViewCell removedCell = m_visibleCells[row];
            StoreCellForReuse(removedCell);
            m_visibleCells.Remove(row);
            m_visibleRowRange.count -= 1;
            if (!last) {
                m_visibleRowRange.from += 1;
            } 
            this.onCellVisibilityChanged.Invoke(row, false);
        }

        private LayoutElement CreateEmptyPaddingElement(string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
            LayoutElement le = go.GetComponent<LayoutElement>();
            return le;
        }

        private int FindIndexOfRowAtY(float y) {
            //TODO : Binary search if inside clean cumulative row height area, else walk until found.
            return FindIndexOfRowAtY(y, 0, m_cumulativeRowHeights.Length - 1);
        }

        private int FindIndexOfRowAtY(float y, int startIndex, int endIndex) {
            if (startIndex >= endIndex) {
                return startIndex;
            }
            int midIndex = (startIndex + endIndex) / 2;
            if (GetCumulativeRowHeight(midIndex) >= y) {
                return FindIndexOfRowAtY(y, startIndex, midIndex);
            } else {
                return FindIndexOfRowAtY(y, midIndex + 1, endIndex);
            }
        }

        private float GetCumulativeRowHeight(int row) {
            while (m_cleanCumulativeIndex < row) {
                m_cleanCumulativeIndex++;
                //Debug.Log("Cumulative index : " + m_cleanCumulativeIndex.ToString());
                m_cumulativeRowHeights[m_cleanCumulativeIndex] = m_rowHeights[m_cleanCumulativeIndex];
                if (m_cleanCumulativeIndex > 0) {
                    m_cumulativeRowHeights[m_cleanCumulativeIndex] += m_cumulativeRowHeights[m_cleanCumulativeIndex - 1];
                } 
            }
            return m_cumulativeRowHeights[row];
        }

        private void StoreCellForReuse(TableViewCell cell) {
            string reuseIdentifier = cell.reuseIdentifier;
            
            if (string.IsNullOrEmpty(reuseIdentifier)) {
                GameObject.Destroy(cell.gameObject);
                return;
            }

            if (!m_reusableCells.ContainsKey(reuseIdentifier)) {
                m_reusableCells.Add(reuseIdentifier, new LinkedList<TableViewCell>());
            }
            m_reusableCells[reuseIdentifier].AddLast(cell);
            cell.transform.SetParent(m_reusableCellContainer, false);
        }

        #endregion


        
    }

    internal static class RangeExtensions
    {
        public static int Last(this Range range)
        {
            if (range.count == 0)
            {
                throw new System.InvalidOperationException("Empty range has no to()");
            }
            return (range.from + range.count - 1);
        }

        public static bool Contains(this Range range, int num) {
            return num >= range.from && num < (range.from + range.count);
        }
    }
}
