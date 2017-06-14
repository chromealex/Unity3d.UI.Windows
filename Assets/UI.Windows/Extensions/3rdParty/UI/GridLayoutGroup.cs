using UnityEngine;
using System.Collections;

namespace ME {

	public class GridLayoutGroup : UnityEngine.UI.GridLayoutGroup {
		
		public bool stretchWidth = false;
		public bool stretchHeight = false;
		
		public override void SetLayoutHorizontal() {
			
			if (this.stretchWidth == true && this.constraint == Constraint.FixedColumnCount) {
				
				var rect = this.rectTransform.rect;
				var cellSize = this.cellSize;
				cellSize.x = (rect.width - (this.constraintCount - 1) * this.spacing.x - this.padding.right - this.padding.left) / (float)this.constraintCount;
				this.cellSize = cellSize;
				
			}
			
			base.SetLayoutHorizontal();
			
		}
		
		public override void SetLayoutVertical() {
			
			if (this.stretchHeight == true && this.constraint == Constraint.FixedRowCount) {
				
				var rect = this.rectTransform.rect;
				var cellSize = this.cellSize;
				cellSize.y = (rect.height - (this.constraintCount - 1) * this.spacing.y - this.padding.top - this.padding.bottom) / (float)this.constraintCount;
				this.cellSize = cellSize;
				
			}
			
			base.SetLayoutVertical();
			
		}

	}

}