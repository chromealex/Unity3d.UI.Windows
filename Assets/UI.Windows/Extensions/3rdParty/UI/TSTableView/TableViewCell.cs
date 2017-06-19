﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;

namespace Tacticsoft {

    /// <summary>
    /// The base class for cells in a TableView. ITableViewDataSource returns pointers
    /// to these objects
    /// </summary>
    public class TableViewCell : WindowComponent {

        /// <summary>
        /// TableView will cache unused cells and reuse them according to their
        /// reuse identifier. Override this to add custom cache grouping logic.
        /// </summary>
        public virtual string reuseIdentifier {

            get {

                return this.GetType().Name;

            }

        }

		public void RegisterOnView(WindowComponent container) {

			container.RegisterSubComponent(this);

		}

    }

}
