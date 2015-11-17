using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Collections.Generic;
using System;
using System.Linq;

namespace UnityEngine.UI.Windows.Plugins.Services {

	public abstract class ServiceSettings : ScriptableObject {

		public void AddService<T>(IService service) where T : ServiceItem, new() {
			
			var item = new T();
			item.serviceName = service.GetServiceName();
			this.AddService(item);
			
		}
		
		public bool HasService(string serviceName) {

			var items = this.GetItems();
			for (int i = 0; i < items.Count; ++i) {
				
				if (items[i].serviceName == serviceName) return true;
				
			}
			
			return false;
			
		}

		public virtual void SetChanged() {}

		public abstract void AddService(ServiceItem item);
		public abstract List<ServiceItem> GetItems();

	}

}