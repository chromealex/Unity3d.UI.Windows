using UnityEngine;
using System.Collections;
using System;

namespace UnityEngine.UI.Windows.Plugins.Social {
	
	public interface IProfile : ISubModule {
		
		ISocialUser user { get; set; }
		void LoadInfo(Action<bool> callback);
		void LoadInfo(SubModuleSettings settings, Action<bool> callback);
		
	}
	
	public class Profile<TSettings> : SubModule<TSettings>, IProfile where TSettings : SubModuleSettings {
		
		public ISocialUser user {
			get;
			set;
		}
		
		public Profile(
			SocialModule socialModule,
			ModuleSettings moduleSettings,
			TSettings settings) : base(socialModule, moduleSettings, settings) {
			
		}
		
		public virtual void LoadInfo(Action<bool> callback) {
			
			this.LoadInfo(this.settings, callback);
			
		}
		
		public virtual void LoadInfo(SubModuleSettings settings, Action<bool> callback) {
			
			new SocialHttp(
				this.moduleSettings,
				settings.url,
				this.moduleSettings.Prepare((this.socialModule.auth as IAuth).token, settings.parameters),
				settings.method,
				(data, result) => {
				
				Debug.Log(result + " :: " + data);
				
				this.user.ParseData(this.socialModule, data);

				callback(result);
				
			});
			
		}
		
	}

}