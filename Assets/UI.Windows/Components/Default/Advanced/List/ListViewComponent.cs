using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Extensions;

namespace UnityEngine.UI.Windows.Components {

	[RequireComponent(typeof(ScrollRect), typeof(VerticalLayoutGroup))]
	public class ListViewComponent : ListComponent, IListViewDataSource {

		[System.Serializable]
		public struct Range {

			//
			// Fields
			//
			public int count;
			public int from;
			
			//
			// Constructors
			//
			public Range (int fromValue, int valueCount) {

				this.from = fromValue;
				this.count = valueCount;

			}

			public int Last() {
				
				if (this.count == 0) {
					
					throw new System.InvalidOperationException("Empty range has no to()");
					
				}
				
				return (this.from + this.count - 1);
				
			}
			
			public bool Contains(int num) {
				
				return num >= this.from && num < (this.from + this.count);
				
			}
			
		}

		private const string DEFAULT_CELL_ID = "default_cell";

		[Header("List View Component")]
		[SerializeField]
		private VerticalLayoutGroup verticalLayoutGroup;

		[Header("Get Size from Rect (Optional)")]
		[Tooltip("By default size will get from source")]
		[SerializeField]
		private RectTransform specialSize;

		private int capacity;
		private UnityAction<IComponent, int> onItem;

		#region IListViewDataSource
		public override bool IsEmpty() {

			return this.isEmpty;

		}

		public int GetRowsCount(ListViewComponent view) {

			return this.capacity;

		}

		public float GetRowHeight(ListViewComponent view, int row) {

			return (this.specialSize != null) ? this.specialSize.rect.height : (this.source.transform as RectTransform).rect.height;

		}

		public WindowComponent GetRowInstance(ListViewComponent view, int row) {

			return this.GetInstance(row);

		}

		public override WindowComponent GetItem(int index) {

			return this.GetInstance(index);

		}

		public override WindowComponent GetInstance(int index) {
			
			var instance = this.GetReusableCell((this.source is IListViewItem) ? (this.source as IListViewItem).reuseIdentifier : ListViewComponent.DEFAULT_CELL_ID) as WindowComponent;
			if (instance == null) {

				instance = base.GetInstance(index);
				
			}

			if (this.onItem != null) {

				this.onItem(instance, index);

			}
			
			return instance;

		}

		public void AddItem(UnityAction<IComponent, int> onItem) {

			++this.capacity;
			this.onItem = onItem;

			this.dataSource = this;

		}

		protected override void SetItems(int capacity, UnityAction<IComponent, int> onItem) {

			this.capacity = capacity;
			this.onItem = onItem;

			this.dataSource = this;

		}
		#endregion
		
		#region Public API
		/// <summary>
		/// The data source that will feed this table view with information. Required.
		/// </summary>
		public IListViewDataSource dataSource {

			get {

				return this._dataSource;

			}
			set {

				this._dataSource = value;
				this.requiresReload = true;

			}

		}
        
		[System.Serializable]
		public class CellVisibilityChangeEvent : UnityEvent<int, bool> {}

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
		public WindowComponent GetReusableCell(string reuseIdentifier) {

			LinkedList<WindowComponent> cells;
			if (this.reusableCells.TryGetValue(reuseIdentifier, out cells) == false) {

				return null;

			}

			if (cells.Count == 0) {

				return null;

			}

			var cell = cells.First.Value;
			cells.RemoveFirst();

			return cell;

		}

		public bool isEmpty { get; private set; }

		/// <summary>
		/// Reload the table view. Manually call this if the data source changed in a way that alters the basic layout
		/// (number of rows changed, etc)
		/// </summary>
		public void ReloadData() {

			this.rowHeights = new float[this.dataSource.GetRowsCount(this)];

			this.isEmpty = this.rowHeights.Length == 0;
			this.Refresh();

			if (this.isEmpty == true) {

				this.ClearAllRows();
				return;

			}

			this.cumulativeRowHeights = new float[this.rowHeights.Length];
			this.cleanCumulativeIndex = -1;

			for (int i = 0; i < this.rowHeights.Length; ++i) {

				this.rowHeights[i] = this.dataSource.GetRowHeight(this, i);
				if (i > 0) {

					this.rowHeights[i] += verticalLayoutGroup.spacing;

				}

			}

			this.scrollRect.content.sizeDelta = new Vector2(this.scrollRect.content.sizeDelta[0], this.GetCumulativeRowHeight(this.rowHeights.Length - 1));

			this.RecalculateVisibleRowsFromScratch();
			this.requiresReload = false;

		}

		/// <summary>
		/// Get cell at a specific row (if active). Returns null if not.
		/// </summary>
		public WindowComponent GetCellAtRow(int row) {

			WindowComponent retVal;
			this.visibleCells.TryGetValue(row, out retVal);
			return retVal;

		}

		/// <summary>
		/// Notify the table view that one of its rows changed size
		/// </summary>
		public void NotifyCellDimensionsChanged(int row) {

			var oldHeight = this.rowHeights[row];
			this.rowHeights[row] = this.dataSource.GetRowHeight(this, row);
			this.cleanCumulativeIndex = Mathf.Min(this.cleanCumulativeIndex, row - 1);
			if (this.visibleRowRange.Contains(row) == true) {

				WindowComponent cell = this.GetCellAtRow(row);
				cell.GetComponent<LayoutElement>().preferredHeight = this.rowHeights[row];
				if (row > 0) {

					cell.GetComponent<LayoutElement>().preferredHeight -= verticalLayoutGroup.spacing;

				}

			}

			var heightDelta = this.rowHeights[row] - oldHeight;
			this.scrollRect.content.sizeDelta = new Vector2(this.scrollRect.content.sizeDelta.x, this.scrollRect.content.sizeDelta.y + heightDelta);
			this.requiresRefresh = true;

		}

		/// <summary>
		/// Get the maximum scrollable height of the table. scrollY property will never be more than this.
		/// </summary>
		public float scrollableHeight {

			get {

				return this.scrollRect.content.rect.height - (this.scrollRect.transform as RectTransform).rect.height;

			}

		}

		/// <summary>
		/// Get or set the current scrolling position of the table
		/// </summary>
		public float scrollY {

			get {

				return this._scrollY;

			}

			set {

				if (this.isEmpty == true) {

					return;

				}

				value = Mathf.Clamp(value, 0, this.GetScrollYForRow(this.rowHeights.Length - 1, true));
				if (this._scrollY != value) {

					this._scrollY = value;
					this.requiresRefresh = true;
					var relativeScroll = value / this.scrollableHeight;
					this.scrollRect.verticalNormalizedPosition = 1 - relativeScroll;

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

			var retVal = this.GetCumulativeRowHeight(row);
			if (above == true) {

				retVal -= this.rowHeights[row];

			}

			return retVal;

		}

        #endregion

        #region Private implementation

		private IListViewDataSource _dataSource;
		private float _scrollY;

		private bool requiresReload;
		private LayoutElement topPadding;
		private LayoutElement bottomPadding;
		private float[] rowHeights;
		//cumulative[i] = sum(rowHeights[j] for 0 <= j <= i)
		private float[] cumulativeRowHeights = new float[0];
		private int cleanCumulativeIndex;
		private Dictionary<int, WindowComponent> visibleCells;
		private Range visibleRowRange;
		private RectTransform reusableCellContainer;
		private Dictionary<string, LinkedList<WindowComponent>> reusableCells;
		private bool requiresRefresh;

		private void ScrollViewValueChanged(Vector2 newScrollValue) {

			var relativeScroll = 1f - newScrollValue.y;
			this.scrollY = relativeScroll * scrollableHeight;
			this.requiresRefresh = true;
			//Debug.Log(this.scrollY.ToString(("0.00")));

		}

		private void RecalculateVisibleRowsFromScratch() {

			this.ClearAllRows();
			this.SetInitialVisibleRows();

		}

		private void ClearAllRows() {

			while (this.visibleCells.Count > 0) {

				this.HideRow(false);

			}

			this.visibleRowRange = new Range(0, 0);

		}

		public override void OnInit() {

			base.OnInit();

			this.Init();
			
			this.scrollRect.onValueChanged.AddListener(this.ScrollViewValueChanged);

		}

		public override void OnDeinit() {

			base.OnDeinit();

			this.scrollRect.onValueChanged.RemoveListener(this.ScrollViewValueChanged);

		}

		private void Init() {

			this.isEmpty = true;
			this.topPadding = this.CreateEmptyPaddingElement("TopPadding");
			this.topPadding.transform.SetParent(this.scrollRect.content, false);
			this.bottomPadding = this.CreateEmptyPaddingElement("Bottom");
			this.bottomPadding.transform.SetParent(this.scrollRect.content, false);
			this.visibleCells = new Dictionary<int, WindowComponent>();

			this.reusableCellContainer = new GameObject("ReusableCells", typeof(RectTransform)).GetComponent<RectTransform>();
			this.reusableCellContainer.SetParent(this.scrollRect.transform, false);
			this.reusableCellContainer.gameObject.SetActive(false);
			this.reusableCells = new Dictionary<string, LinkedList<WindowComponent>>();

			this.dataSource = this;

		}
        
		public virtual void Update() {

			if (this.requiresReload == true) {

				this.ReloadData();

			}

		}

		public virtual void LateUpdate() {

			if (this.requiresRefresh == true) {

				this.RefreshVisibleRows();

			}

		}

		private Range CalculateCurrentVisibleRowRange() {

			var startY = this.scrollY;
			var endY = this.scrollY + (this.scrollRect.transform as RectTransform).rect.height;
			var startIndex = this.FindIndexOfRowAtY(startY);
			var endIndex = this.FindIndexOfRowAtY(endY);

			return new Range(startIndex, endIndex - startIndex + 1);

		}

		private void SetInitialVisibleRows() {

			var visibleRows = this.CalculateCurrentVisibleRowRange();
			for (int i = 0; i < visibleRows.count; ++i) {

				this.AddRow(visibleRows.from + i, true);

			}

			this.visibleRowRange = visibleRows;
			this.UpdatePaddingElements();

		}

		private void AddRow(int row, bool atEnd) {

			var newCell = this.dataSource.GetRowInstance(this, row);
			newCell.transform.SetParent(scrollRect.content, false);

			var layoutElement = newCell.GetComponent<LayoutElement>();
			if (layoutElement == null) {

				layoutElement = newCell.gameObject.AddComponent<LayoutElement>();

			}

			layoutElement.preferredHeight = this.rowHeights[row];
			if (row > 0) {

				layoutElement.preferredHeight -= this.verticalLayoutGroup.spacing;

			}
            
			this.visibleCells[row] = newCell;

			if (atEnd == true) {

				newCell.transform.SetSiblingIndex(this.scrollRect.content.childCount - 2); //One before bottom padding

			} else {

				newCell.transform.SetSiblingIndex(1); //One after the top padding

			}

			//this.bottomPadding.transform.SetSiblingIndex(this.Count());

			this.onCellVisibilityChanged.Invoke(row, true);

		}

		private void RefreshVisibleRows() {

			this.requiresRefresh = false;

			if (this.isEmpty == true) {

				return;

			}

			var newVisibleRows = this.CalculateCurrentVisibleRowRange();
			var oldTo = this.visibleRowRange.Last();
			var newTo = newVisibleRows.Last();

			if (newVisibleRows.from > oldTo || newTo < this.visibleRowRange.from) {

				//We jumped to a completely different segment this frame, destroy all and recreate
				this.RecalculateVisibleRowsFromScratch();
				return;

			}

			//Remove rows that disappeared to the top
			for (var i = this.visibleRowRange.from; i < newVisibleRows.from; ++i) {

				this.HideRow(false);

			}

			//Remove rows that disappeared to the bottom
			for (var i = newTo; i < oldTo; ++i) {

				this.HideRow(true);

			}

			//Add rows that appeared on top
			for (var i = this.visibleRowRange.from - 1; i >= newVisibleRows.from; --i) {

				this.AddRow(i, false);

			}

			//Add rows that appeared on bottom
			for (var i = oldTo + 1; i <= newTo; ++i) {

				this.AddRow(i, true);

			}

			this.visibleRowRange = newVisibleRows;
			this.UpdatePaddingElements();

		}

		private void UpdatePaddingElements() {

			var hiddenElementsHeightSum = 0f;
			for (var i = 0; i < this.visibleRowRange.from; ++i) {

				hiddenElementsHeightSum += this.rowHeights[i];

			}

			this.topPadding.preferredHeight = hiddenElementsHeightSum;
			this.topPadding.gameObject.SetActive(this.topPadding.preferredHeight > 0f);

			for (var i = this.visibleRowRange.from; i <= this.visibleRowRange.Last(); ++i) {

				hiddenElementsHeightSum += this.rowHeights[i];

			}

			var bottomPaddingHeight = this.scrollRect.content.rect.height - hiddenElementsHeightSum;
			this.bottomPadding.preferredHeight = bottomPaddingHeight - this.verticalLayoutGroup.spacing;
			this.bottomPadding.gameObject.SetActive(this.bottomPadding.preferredHeight > 0f);

		}

		private void HideRow(bool last) {

			//Debug.Log("Hiding row at scroll y " + this.scrollY.ToString("0.00"));

			var row = last ? this.visibleRowRange.Last() : this.visibleRowRange.from;
			var removedCell = this.visibleCells[row];
			this.StoreCellForReuse(removedCell);
			this.visibleCells.Remove(row);
			this.visibleRowRange.count -= 1;

			if (last == false) {

				this.visibleRowRange.from += 1;

			} 

			this.onCellVisibilityChanged.Invoke(row, false);

		}

		private LayoutElement CreateEmptyPaddingElement(string name) {

			var go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
			var le = go.GetComponent<LayoutElement>();

			return le;

		}

		private int FindIndexOfRowAtY(float y) {

			//TODO : Binary search if inside clean cumulative row height area, else walk until found.
			return this.FindIndexOfRowAtY(y, 0, this.cumulativeRowHeights.Length - 1);

		}

		private int FindIndexOfRowAtY(float y, int startIndex, int endIndex) {

			if (endIndex < 0) {

				endIndex = 0;

			}

			if (startIndex >= endIndex) {

				return startIndex;

			}

			var midIndex = (startIndex + endIndex) / 2;
			if (this.GetCumulativeRowHeight(midIndex) >= y) {

				return this.FindIndexOfRowAtY(y, startIndex, midIndex);

			} else {

				return this.FindIndexOfRowAtY(y, midIndex + 1, endIndex);

			}

		}

		private float GetCumulativeRowHeight(int row) {

			if (row < 0) {

				row = 0;

			}

			if (this.cumulativeRowHeights.Length == 0) {

				return 0f;

			}

			while (this.cleanCumulativeIndex < row) {

				++this.cleanCumulativeIndex;
				//Debug.Log("Cumulative index : " + this.cleanCumulativeIndex.ToString());
				this.cumulativeRowHeights[this.cleanCumulativeIndex] = this.rowHeights[this.cleanCumulativeIndex];
				if (this.cleanCumulativeIndex > 0) {

					this.cumulativeRowHeights[this.cleanCumulativeIndex] += this.cumulativeRowHeights[this.cleanCumulativeIndex - 1];

				}

			}

			return this.cumulativeRowHeights[row];

		}

		private void StoreCellForReuse(WindowComponent cell) {

			var reuseIdentifier = (cell is IListViewItem) ? (cell as IListViewItem).reuseIdentifier : ListViewComponent.DEFAULT_CELL_ID;

			if (this.reusableCells.ContainsKey(reuseIdentifier) == false) {

				this.reusableCells.Add(reuseIdentifier, new LinkedList<WindowComponent>());

			}
			this.reusableCells[reuseIdentifier].AddLast(cell);

			cell.transform.SetParent(this.reusableCellContainer, false);

		}
        #endregion

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();

			if (Application.isPlaying == true) return;

			var items = this.GetComponentsInChildren<VerticalLayoutGroup>();
			if (items != null && items.Length > 0) this.verticalLayoutGroup = items[0];

		}
		#endif

	}

}
