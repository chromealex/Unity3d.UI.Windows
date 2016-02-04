using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Collections.Generic;
using System;
using System.Linq;

namespace UnityEngine.UI.Windows.Plugins.Services {
	
	public interface IServiceManagerBase {
		
		IEnumerator Init(System.Action onComplete);
		
	}

	public interface IServiceManager {
		
		List<IService> services { get; set; }
		void Register(IService service);
		
		string GetServiceName();
		AuthKeyPermissions GetAuthPermission();

	};

	public abstract class ServiceManagerBase : MonoBehaviour, IServiceManager, IServiceManagerBase {

		public ServiceSettings settings;
		public bool logEnabled = false;

		public List<IService> services { get; set; }

		public abstract string GetServiceName();
		public abstract AuthKeyPermissions GetAuthPermission();

		public void Register(IService service) {

			if (this.services == null) this.services = new List<IService>();
			this.services.Add(service);

		}
		
		public IEnumerator Start() {

			yield return this.StartCoroutine(this.Init());

		}

		public IEnumerator Init(System.Action onComplete = null) {

			if (this.logEnabled == true) {

				Debug.LogFormat("[ {0} ] Initializing...", this.GetServiceName());

			}

			if (FlowSystem.GetData().IsValidAuthKey(AuthKeyPermissions.Analytics) == false) {
				
				if (this.logEnabled == true) {

					Debug.LogWarningFormat("[ {0} ] Permission denied.", this.GetServiceName());

				}
				yield break;
				
			}
			
			var settings = this.settings;
			if (settings == null) yield break;

			this.OnInitialized();

			var items = settings.GetItems();
			
			foreach (var service in this.services) {
				
				for (int i = 0; i < items.Count; ++i) {
					
					var item = items[i];
					if (item.serviceName == service.GetServiceName()) {
						
						service.isActive = item.enabled;
						if (service.isActive == true) {

							yield return this.StartCoroutine(service.Auth(service.GetAuthKey(item)));
							yield return this.StartCoroutine(this.OnAfterAuth(service));

						}

					}
					
				}
				
			}
			
			if (this.logEnabled == true) {

				Debug.LogFormat("[ {0} ] Initialized", this.GetServiceName());

			}

			if (onComplete != null) onComplete.Invoke();

		}
		
		public virtual void OnInitialized() {
			
		}
		
		public virtual IEnumerator OnAfterAuth(IService service) {

			yield return false;

		}

	}

	public abstract class ServiceManager<T> : ServiceManagerBase where T : IServiceManager {

		private static T _instance;
		protected static T instance {

			get {

				return ServiceManager<T>._instance;

			}

			set {

				if (ServiceManager<T>._instance != null) return;
				ServiceManager<T>._instance = value;

			}

		}

		public void Awake() {

			ServiceManager<T>.instance = (T)(this as IServiceManager);

		}

		public static void Reinitialize(System.Action onComplete = null) {

			var instance = ServiceManager<T>.instance as IServiceManagerBase;
			(instance as MonoBehaviour).StartCoroutine(instance.Init(onComplete));

		}

		public static void ForEachService<TService>(System.Action<TService> onService) where TService : IService {
			
			#if UNITY_EDITOR
			if (Application.isPlaying == false) return;
			#endif

			var instance = ServiceManager<T>.instance;
			var list = instance.services;
			if (list == null) return;

			foreach (var serviceBase in list) {
				
				if (serviceBase.isActive == true) {
					
					onService((TService)serviceBase);
					
				}
				
			}
			
		}

	}

}