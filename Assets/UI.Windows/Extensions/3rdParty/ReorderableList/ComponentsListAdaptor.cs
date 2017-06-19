// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.UI.Windows.Internal.ReorderableList {

	/// <summary>
	/// Reorderable list adaptor for generic list.
	/// </summary>
	/// <remarks>
	/// <para>This adaptor can be subclassed to add special logic to item height calculation.
	/// You may want to implement a custom adaptor class where specialised functionality
	/// is needed.</para>
	/// <para>List elements which implement the <see cref="System.ICloneable"/> interface are
	/// cloned using that interface upon duplication; otherwise the item value or reference is
	/// simply copied.</para>
	/// </remarks>
	/// <typeparam name="T">Type of list element.</typeparam>
	public class ComponentsListAdaptor<T> : GenericListAdaptor<T> {

		private System.Func<int, float> getHeight;

		#region Construction

		/// <summary>
		/// Initializes a new instance of <see cref="GenericListAdaptor{T}"/>.
		/// </summary>
		/// <param name="list">The list which can be reordered.</param>
		/// <param name="itemDrawer">Callback to draw list item.</param>
		/// <param name="itemHeight">Height of list item in pixels.</param>
		public ComponentsListAdaptor(IList<T> list, ReorderableListControl.ItemDrawer<T> itemDrawer, System.Func<int, float> getHeight) : base(list, itemDrawer, itemHeight: 0f) {

			this.getHeight = getHeight;

		}

		#endregion

		#region IReorderableListAdaptor - Implementation
		public override float GetItemHeight(int index) {

			return this.getHeight(index);

		}

		#endregion

	}

}
#endif