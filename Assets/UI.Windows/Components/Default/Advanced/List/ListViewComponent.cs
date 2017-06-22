using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Extensions;
using UnityEngine.UI.Extensions;

namespace UnityEngine.UI.Windows.Components {

	//[RequireComponent(typeof(ScrollRect))]
	public class ListViewComponent : ListComponent, IListViewDataSource, ILayoutController {
		
		[System.Serializable]
		public class CellVisibilityChangeEvent : ME.Events.SimpleEvent<int, bool> {}

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
					
					//throw new System.InvalidOperationException("Empty range has no to()");
					return this.from;

				}
				
				return (this.from + this.count - 1);
				
			}
			
			public bool Contains(int num) {
				
				return num >= this.from && num < (this.from + this.count);
				
			}

			public override string ToString() {

				return string.Format("{1} - {0}", this.count, this.from);

			}
			
		}

		private const string DEFAULT_CELL_ID = "default_cell";

		[Header("Offsets")]
		public float offsetStartY = 0f;
		public float offsetEndY = 0f;

		[Header("Get Size from Rect (Optional)")]
		[Tooltip("By default size will get from source")]
		[SerializeField]
		private RectTransform specialSize;

		private int capacity;
		private System.Action<IComponent, int> onItem;

		/// <summary>
		/// This event will be called when a cell's visibility changes
		/// First param (int) is the row index, second param (bool) is whether or not it is visible
		/// </summary>
		public CellVisibilityChangeEvent onCellVisibilityChanged = new CellVisibilityChangeEvent();


		#region ILayoutController
		public void SetLayoutHorizontal() {

			// TODO: ?
			this.requiresReload = true;

		}
		
		public void SetLayoutVertical() {

			// TODO: ?
			this.requiresReload = true;

		}
		#endregion

		#region IListViewDataSource
		public override bool IsEmpty() {

			return this.isEmpty;

		}

		public virtual int GetRowsCount() {

			return this.capacity;

		}

		public override IListComponent ListMoveUp(int count = 1) {

			var newValue = this.scrollY;
			newValue -= (this.GetRowHeight(0) + this.GetRowSpacing()) * count;
			if (newValue < 0f) newValue = 0f;

			this.scrollY = newValue;

			return this;

		}

		public override IListComponent ListMoveDown(int count = 1) {

			var newValue = this.scrollY;
			newValue += (this.GetRowHeight(0) + this.GetRowSpacing()) * count;
			var maxHeight = this.scrollableHeight;
			if (newValue >= maxHeight) newValue = maxHeight;

			this.scrollY = newValue;

			return this;

		}

		public override IListComponent ListMoveRight(int count = 1) {

			var newValue = this.scrollX;
			newValue -= (this.GetRowWidth(0) + this.GetRowSpacing()) * count;
			//var maxWidth = this.scrollableWidth;
			if (newValue < 0f) newValue = 0f;

			this.scrollX = newValue;

			return this;

		}

		public override IListComponent ListMoveLeft(int count = 1) {

			var newValue = this.scrollX;
			newValue += (this.GetRowWidth(0) + this.GetRowSpacing()) * count;
			var maxWidth = this.scrollableWidth;
			if (newValue >= maxWidth) newValue = maxWidth;

			this.scrollX = newValue;

			return this;

		}

		public override float GetRowWidth(int row) {

			var index = row;
			if (index >= 0) {

				return (this.specialSize != null) ? this.specialSize.rect.width : (this.source.transform as RectTransform).rect.width;

			}

			return (this.source.transform as RectTransform).rect.width;

		}

		public override float GetRowHeight(int row) {

			var index = row;
			if (index >= 0) {

				return (this.specialSize != null) ? this.specialSize.rect.height : (this.source.transform as RectTransform).rect.height;

			}

			return (this.source.transform as RectTransform).rect.height;

		}

		public WindowComponent GetRowInstance(int row) {

			return this.GetInstance(row);

		}

		public override WindowComponent GetItem(int index, bool linkerLookup = true) {

			if (index >= 0 && index < this.list.Count) {

				return base.GetItem(index, linkerLookup);

			}

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

		public void AddItem(System.Action<IComponent, int> onItem) {

			++this.capacity;
			this.onItem = onItem;

			this.dataSource = this;

		}

		protected override void SetItems(int capacity, System.Action<IComponent, int> onItem) {

			this.capacity = capacity;
			this.onItem = onItem;

			this.dataSource = this;

			this.ReloadData();

		}

		/// <summary>
		/// Updates the cell.
		/// Update action onItem will fired with null IComponent. Be sure you'd check it for null.
		/// </summary>
		/// <param name="index">Cell index in your data array.</param>
		public void UpdateCell(int index) {

			this.onItem(null, index);

		}
		#endregion
		
		#region Public API
		private int maxOutputCount = -1;
		public void SetMaxOutputCount(int max = -1, System.Action callback = null) {

			System.Action callbackInner = () => {
				
				this.maxOutputCount = max;
				this.ReloadData();
				
				if (callback != null) callback();

			};

			callbackInner();

		}

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

			var count = this.dataSource.GetRowsCount();
			this.isEmpty = (count == 0);

			if (this.isEmpty == true) {

				this.ClearAllRows();
				this.Refresh(withNoElements: true);
				return;

			}

			if (this.rowHeights == null || count != this.rowHeights.Length) {

				this.rowHeights = new float[count];
				this.cumulativeRowHeights = new float[count];

			}

			this.cleanCumulativeIndex = -1;

			for (int i = 0; i < count; ++i) {

				this.rowHeights[i] = this.dataSource.GetRowHeight(i);
				if (i > 0) {

					this.rowHeights[i] += this.GetRowSpacing();

				}

			}

			this.scrollRect.content.sizeDelta = new Vector2(this.scrollRect.content.sizeDelta[0], this.GetCumulativeRowHeight(count - 1));

			this.RecalculateVisibleRowsFromScratch();
			this.requiresReload = false;
			
			this.Refresh(withNoElements: true);

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
			this.rowHeights[row] = this.dataSource.GetRowHeight(row);
			this.cleanCumulativeIndex = Mathf.Min(this.cleanCumulativeIndex, row - 1);
			if (this.visibleRowRange.Contains(row) == true) {

				WindowComponent cell = this.GetCellAtRow(row);
				var le = cell.GetComponent<LayoutElement>();
				le.preferredHeight = this.rowHeights[row];
				if (row > 0) {

					le.preferredHeight -= this.GetRowSpacing();

				}
				le.minHeight = le.preferredHeight;

			}

			var heightDelta = this.rowHeights[row] - oldHeight;
			this.scrollRect.content.sizeDelta = new Vector2(this.scrollRect.content.sizeDelta.x, this.scrollRect.content.sizeDelta.y + heightDelta);
			this.requiresRefresh = true;

		}

		public float scrollableWidth {

			get {

				return this.scrollRect.content.rect.width - (this.scrollRect.transform as RectTransform).rect.width;

			}

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
		public float scrollX {

			get {

				return this._scrollX;

			}

			set {

				if (this.isEmpty == true) {

					return;

				}

				value = Mathf.Clamp(value, 0f, this.GetScrollXForRow(this.rowHeights.Length - 1, true));
				if (this._scrollX != value) {

					this._scrollX = value;
					this.requiresRefresh = true;
					var relativeScroll = value / this.scrollableWidth;
					this.scrollRect.horizontalNormalizedPosition = 1f - relativeScroll;

				}

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

				value = Mathf.Round(Mathf.Clamp(value, 0f, this.GetScrollYForRow(this.rowHeights.Length - 1, true)) * 100f) / 100f;
				if (this._scrollY != value) {
					
					this._scrollY = value;
					this.requiresRefresh = true;
					var relativeScroll = value / this.scrollableHeight;
					this.scrollRect.verticalNormalizedPosition = 1f - relativeScroll;

				}

			}

		}

		public float GetScrollXForRow(int row, bool above) {

			var retVal = this.GetCumulativeRowHeight(row);
			if (above == true) {

				retVal -= this.rowHeights[row];

			}

			return retVal;

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
		private float _scrollX;
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
			this.RefreshVisibleRows();
			//Debug.Log(this.scrollY.ToString(("0.00")));

		}

		private void RecalculateVisibleRowsFromScratch() {

			this.ClearAllRows();
			this.SetInitialVisibleRows();

		}

		private void ClearAllRows() {

			if (this.visibleCells != null) {

				while (this.visibleCells.Count > 0) {

					this.HideRow(false);

				}

			}

			this.visibleRowRange = new Range(0, 0);

		}

		public override void OnInit() {

			base.OnInit();

			this.Init();
			
			this.scrollRect.onValueChanged.AddListener(this.ScrollViewValueChanged);

		}

		public override void OnDeinit(System.Action callback) {

			base.OnDeinit(callback);

			this.scrollRect.onValueChanged.RemoveListener(this.ScrollViewValueChanged);

		}

		private void Init() {

			this.isEmpty = true;
			this.topPadding = this.CreateEmptyPaddingElement("TopPadding");
			this.topPadding.transform.SetParent(this.scrollRect.content, false);
			this.bottomPadding = this.CreateEmptyPaddingElement("BottomPadding");
			this.bottomPadding.transform.SetParent(this.scrollRect.content, false);
			this.visibleCells = new Dictionary<int, WindowComponent>();

			this.reusableCellContainer = new GameObject("ReusableCells", typeof(RectTransform)).GetComponent<RectTransform>();
			this.reusableCellContainer.SetParent(this.scrollRect.transform, false);
			this.reusableCellContainer.gameObject.SetActive(true);
			this.reusableCellContainer.localScale = Vector3.zero;
			this.reusableCells = new Dictionary<string, LinkedList<WindowComponent>>();

			if (this.top != null) this.top.Hide(immediately: true);
			if (this.bottom != null) this.bottom.Hide(immediately: true);

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

		private Range CalculateCurrentVisibleRowRange(int max = -2) {

			var startY = this.scrollY - this.offsetStartY;
			var endY = this.scrollY + (this.scrollRect.transform as RectTransform).rect.height + this.offsetEndY;
			var startIndex = this.FindIndexOfRowAtY(startY);
			var endIndex = this.FindIndexOfRowAtY(endY);

			var visibleRows = new Range(startIndex, endIndex - startIndex + 1);

			if (max == -2) {

				max = this.maxOutputCount;

			}

			if (max > -1) {

				var count = this.cumulativeRowHeights.Length;
				visibleRows.from = Mathf.Max(0, count - max);
				visibleRows.count = Mathf.Min(count, max);

			}

			return visibleRows;

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

			var newCell = this.dataSource.GetRowInstance(row);
			newCell.transform.SetParent(scrollRect.content, false);
			newCell.transform.localScale = Vector3.one;

			var layoutElement = newCell.GetComponent<LayoutElement>();
			if (layoutElement == null) {

				layoutElement = newCell.gameObject.AddComponent<LayoutElement>();

			}

			layoutElement.preferredHeight = this.rowHeights[row];
			if (row > 0) {

				layoutElement.preferredHeight -= this.GetRowSpacing();

			}
			layoutElement.minHeight = layoutElement.preferredHeight;
            
			this.visibleCells[row] = newCell;

			if (atEnd == true) {

				newCell.transform.SetSiblingIndex(this.scrollRect.content.childCount - 2); //One before bottom padding

			} else {

				newCell.transform.SetSiblingIndex(1); //One after the top padding

			}

			//this.bottomPadding.transform.SetSiblingIndex(this.Count());

			this.onCellVisibilityChanged.Invoke(row, true);

			if (newCell is IListViewHandler) {

				(newCell as IListViewHandler).OnItemBecomeVisible();

			}

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
			var topEnabled = this.topPadding.preferredHeight > 0f;
			this.topPadding.gameObject.SetActive(topEnabled);
			if (this.top != null) this.top.ShowHide(topEnabled);

			for (var i = this.visibleRowRange.from; i <= this.visibleRowRange.Last(); ++i) {

				hiddenElementsHeightSum += this.rowHeights[i];

			}

			var bottomPaddingHeight = this.scrollRect.content.rect.height - hiddenElementsHeightSum;
			this.bottomPadding.preferredHeight = bottomPaddingHeight - this.GetRowSpacing();
			var bottomEnabled = this.bottomPadding.preferredHeight > 0f;
			this.bottomPadding.gameObject.SetActive(bottomEnabled);
			if (this.bottom != null) this.bottom.ShowHide(bottomEnabled);

		}

		private void HideRow(bool last) {

			var row = last ? this.visibleRowRange.Last() : this.visibleRowRange.from;
			if (this.visibleCells.ContainsKey(row) == true) {
				
				var removedCell = this.visibleCells[row];
				this.StoreCellForReuse(removedCell);
				this.visibleCells.Remove(row);
				this.visibleRowRange.count -= 1;

				if (last == false) {

					this.visibleRowRange.from += 1;

				} 

				this.onCellVisibilityChanged.Invoke(row, false);

				if (removedCell is IListViewHandler) {

					(removedCell as IListViewHandler).OnItemBecomeInvisible();

				}

			}

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

			if (cell != null) cell.transform.SetParent(this.reusableCellContainer, false);
			//this.reusableCellContainer.localScale = Vector3.zero;
			/*if (cell != null) {

				cell.transform.localScale = Vector3.zero;

			}*/

		}
        #endregion

	}

}
