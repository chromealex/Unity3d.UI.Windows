using UnityEngine;
using System.Collections;
using System;

namespace UnityEngine.UI.Windows.Plugins.Social {
	
	public interface IFriends : ISubModule {

		void LoadInfo(Action<bool> callback);
		void LoadInfo(SubModuleSettings settings, Action<bool> callback);
		
	}
	
	public class Friends<TSettings> : SubModule<TSettings>, IFriends where TSettings : SubModuleSettings {

		public Friends(
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
				
				this.socialModule.profile.user.ParseFriendsData(this.socialModule, data);
				
			});
			
		}
		
	}

}