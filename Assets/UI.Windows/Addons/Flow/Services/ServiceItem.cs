using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Collections.Generic;
using System;
using System.Linq;

namespace UnityEngine.UI.Windows.Plugins.Services {

	public abstract class ServiceItem {

		[System.NonSerialized]
		public bool processing;
		
		public bool enabled;
		public string serviceName;
		public string authKey;

		public ServiceItem() {}

	};

}