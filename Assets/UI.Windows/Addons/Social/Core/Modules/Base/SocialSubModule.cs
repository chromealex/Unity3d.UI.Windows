using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Plugins.Social {
	
	public class SubModuleSettings {

		public virtual SocialQuery GetQuery() { return null; }

	}

	public interface ISubModule {
		
	}
	
	public class SubModule<TSettings> : ISubModule where TSettings : SubModuleSettings {
		
		protected readonly SocialModule socialModule;
		protected readonly ModuleSettings moduleSettings;
		
		protected TSettings settings;
		
		public SubModule(
			SocialModule socialModule,
			ModuleSettings moduleSettings,
			TSettings settings) {
			
			this.socialModule = socialModule;
			this.moduleSettings = moduleSettings;
			this.settings = settings;
			
		}
		
		public TSettings GetSettings() {
			
			return this.settings;
			
		}
		
	}

}