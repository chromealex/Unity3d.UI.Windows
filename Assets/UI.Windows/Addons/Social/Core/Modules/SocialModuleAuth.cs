using UnityEngine;
using System.Collections;
using System;

namespace UnityEngine.UI.Windows.Plugins.Social {
	
	[System.Serializable]
	public class AuthSettings : SubModuleSettings {
		
		public enum ProfileLoadType : byte {
			LoadAutoProfileAfterLogin = 0,
			DontLoadProfile = 1,
			LoadStandardProfile = 2,
		};
		
		public SocialQuery authQuery;

		[Header("Profile Loading")]
		public ProfileLoadType profileLoadType = ProfileLoadType.LoadAutoProfileAfterLogin;

		public SocialQuery autoProfileQuery;

		public override SocialQuery GetQuery() {

			return this.authQuery;

		}

	}

	public interface IAuth : ISubModule {
		
		string token { get; set; }
		void Authenticate(Action<bool> callback);
		void Authenticate(SocialQuery query, Action<bool> callback);
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
			
			this.Authenticate(this.settings.GetQuery(), callback);

		}
		
		public virtual void Authenticate(SocialQuery query, Action<bool> callback) {

			new SocialHttp(
				this.moduleSettings,
				query.url,
				this.moduleSettings.Prepare((this.socialModule.auth as IAuth).token, query.parameters),
				query.method,
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

			var query = settings.autoProfileQuery;
			if (type == AuthSettings.ProfileLoadType.LoadStandardProfile) {

				query = this.socialModule.GetSettings<ModuleSettings>().standardProfileSettings.GetQuery();

			}

			this.socialModule.profile.LoadInfo(
				query,
				(result) => {
				
				// Load user data
				callback(result);
				
			});
			
		}
		
	}

}