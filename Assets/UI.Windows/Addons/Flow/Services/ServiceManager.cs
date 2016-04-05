using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Collections.Generic;
using System;
using System.Linq;

namespace UnityEngine.UI.Windows.Plugins.Services {

	public interface IServiceManagerBase : IServiceBase {
		
		IEnumerator Init(System.Action onComplete);
		
	};

	public interface IServiceManager : IServiceBase {
		
		List<IService> services { get; set; }
		void Register(IService service);

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

		public IEnumerator Init(System.Action onComplete = null) {

			if (this.logEnabled == true) {

				WindowSystemLogger.Log(this, "Initializing...");

			}

			if (FlowSystem.GetData().IsValidAuthKey(AuthKeyPermissions.Analytics) == false) {
				
				if (this.logEnabled == true) {

					WindowSystemLogger.Warning(this, "Permission denied");

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

				WindowSystemLogger.Log(this, "Initialized");

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

		public bool initializeOnStart = true;

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

		public void Start() {

			if (this.initializeOnStart == true) ServiceManager<T>.InitializeAsync();

		}
		
		public static void InitializeAsync(System.Action onComplete = null) {
			
			var instance = ServiceManager<T>.instance as IServiceManagerBase;
			(instance as MonoBehaviour).StartCoroutine(instance.Init(onComplete));
			
		}
		
		public static IEnumerator Initialize(System.Action onComplete = null) {
			
			var instance = ServiceManager<T>.instance as IServiceManagerBase;
			yield return (instance as MonoBehaviour).StartCoroutine(instance.Init(onComplete));
			
		}

		public static void ForEachService<TService>(System.Action<TService> onService) where TService : IService {
			
			#if UNITY_EDITOR
			if (Application.isPlaying == false) return;
			#endif

			var instance = ServiceManager<T>.instance;
			var list = instance.services;
			if (list == null) return;

			foreach (var serviceBase in list) {

				if (serviceBase.isActive == true && serviceBase is TService) {
					
					onService((TService)serviceBase);
					
				}
				
			}
			
		}

	}

}