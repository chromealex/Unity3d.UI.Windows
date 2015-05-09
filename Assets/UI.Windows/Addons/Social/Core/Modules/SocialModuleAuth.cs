using UnityEngine;
using System.Collections;
using System;

namespace UnityEngine.UI.Windows.Plugins.Social {
	
	public interface IAuth : ISubModule {
		
		string token { get; set; }
		void Authenticate(Action<bool> callback);
		void Authenticate(string token, Action<bool> callback);
		
	}
	
	public class Auth<TSettings> : SubModule<TSettings>, IAuth where TSettings : SubModuleSettings {
		
		public string token {
			set;
			get;
		}
		
		public Auth(
			SocialModule socialModule,
			ModuleSettings moduleSettings,
			TSettings settings) : base(socialModule, moduleSettings, settings) {
			
		}
		
		public virtual void Authenticate(Action<bool> callback) {
			
			new SocialHttp(
				this.moduleSettings,
				this.settings.url,
				this.moduleSettings.Prepare((this.socialModule.auth as IAuth).token, this.settings.parameters),
				this.settings.method,
				(data, result) => {
				
				// TODO: Parse token here
				Debug.Log(result + " :: " + data);

				// Add call auth with token
				// this.Authenticate(token, callback);
				
			});
			
		}
		
		public virtual void Authenticate(string token, Action<bool> callback) {
			
			this.token = token;
			
			var settings = this.GetSettings() as AuthSettings;
			var type = settings.profileLoadType;
			if (type == AuthSettings.ProfileLoadType.DontLoadProfile) {
				
				callback(true);
				return;
				
			}

			var subModuleSettings = settings.autoProfileSettings as SubModuleSettings;
			if (type == AuthSettings.ProfileLoadType.LoadStandardProfile) subModuleSettings = this.socialModule.GetSettings<ModuleSettings>().standardProfileSettings as SubModuleSettings;

			this.socialModule.profile.LoadInfo(
				subModuleSettings,
				(result) => {
				
				// Load user data
				callback(result);
				
			});
			
		}
		
	}

}