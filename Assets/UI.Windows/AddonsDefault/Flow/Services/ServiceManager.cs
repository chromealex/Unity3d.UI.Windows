using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UI.Windows.Extensions.Net;

namespace UnityEngine.UI.Windows.Plugins.Services {

	public interface IServiceManagerBase : IServiceBase {

		System.Collections.IEnumerator Init(System.Action onComplete);
		System.Collections.IEnumerator InitLate(System.Action onComplete);
		
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

		public T GetSettings<T>() where T : ServiceSettings {

			return (T)this.settings;

		}

		public void Register(IService service) {

			if (this.services == null) this.services = new List<IService>();
			this.services.Add(service);

		}

		public System.Collections.IEnumerator Init(System.Action onComplete = null) {

			if (this.logEnabled == true) {

				WindowSystemLogger.Log(this, "Initializing...");

			}

			if (FlowSystem.GetData().IsValidAuthKey(this.GetAuthPermission()) == false) {
				
				WindowSystemLogger.Warning(this, "Permission denied");

				if (onComplete != null) onComplete.Invoke();

				yield break;
				
			}
			
			var settings = this.settings;
			if (settings == null) {

				if (onComplete != null) onComplete.Invoke();

				yield break;

			}

			this.OnInitialized();

			if (this.services != null) {

				var items = settings.GetItems();

				foreach (var service in this.services) {
					
					for (int i = 0; i < items.Count; ++i) {
						
						var item = items[i];
						if (item.serviceName == service.GetServiceName()) {

							var isSupported = service.IsSupported();
							service.isActive = (item.enabled == true && isSupported == true);
							if (service.isActive == true && isSupported == true) {
								
								yield return this.StartCoroutine(service.Auth(service.GetAuthKey(item), item));
								yield return this.StartCoroutine(this.OnAfterAuth(service));

							}

						}
						
					}
					
				}

			}

			if (this.logEnabled == true) {

				WindowSystemLogger.Log(this, "Initialized");

			}

			if (onComplete != null) onComplete.Invoke();

		}

		public System.Collections.IEnumerator InitLate(System.Action onComplete = null) {

			this.OnInitializedLate();

			yield return 0;

			if (onComplete != null) onComplete.Invoke();

		}

		public virtual void OnInitialized() {

		}

		public virtual void OnInitializedLate() {

		}

		public virtual System.Collections.IEnumerator OnAfterAuth(IService service) {

			yield return 0;

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

	    protected virtual void OnDestory() {

	        ServiceManager<T>._instance = default(T);

	    }

		public void Awake() {

			ServiceManager<T>.instance = (T)(this as IServiceManager);

		}

		public void Start() {

			if (this.initializeOnStart == true) ServiceManager<T>.InitializeAsync();

		}

		public void OnEnable() {

			if (this.initializeOnStart == true) ServiceManager<T>.InitializeLateAsync();

		}

		public static T GetInstance() {

			return ServiceManager<T>.instance;

		}

		public static void InitializeAsync(System.Action onComplete = null) {

			var instance = ServiceManager<T>.instance as IServiceManagerBase;
			(instance as MonoBehaviour).StartCoroutine(instance.Init(onComplete));

		}

		public static System.Collections.IEnumerator Initialize(System.Action onComplete = null) {
			
			var instance = ServiceManager<T>.instance as IServiceManagerBase;
			yield return (instance as MonoBehaviour).StartCoroutine(instance.Init(onComplete));

		}

		public static void InitializeLateAsync(System.Action onComplete = null) {

			var instance = ServiceManager<T>.instance as IServiceManagerBase;
			(instance as MonoBehaviour).StartCoroutine(instance.InitLate(onComplete));

		}

		public static System.Collections.IEnumerator InitializeLate(System.Action onComplete = null) {

			var instance = ServiceManager<T>.instance as IServiceManagerBase;
			yield return (instance as MonoBehaviour).StartCoroutine(instance.InitLate(onComplete));

		}

		public static List<TService> GetServices<TService>() where TService : IService {

			return ServiceManager<T>.instance.services.Cast<TService>().ToList();

		}

		public static void ForEachService<TService>(System.Action<TService> onService, bool activeOnly = true) where TService : IService {
			
			#if UNITY_EDITOR
			if (Application.isPlaying == false) return;
			#endif

			var instance = ServiceManager<T>.instance;
			var list = instance.services;
			if (list == null) return;

			foreach (var serviceBase in list) {

				if ((activeOnly == false || serviceBase.isActive == true) && serviceBase is TService) {

					onService.Invoke((TService)serviceBase);
					
				}
				
			}
			
		}

	}

}