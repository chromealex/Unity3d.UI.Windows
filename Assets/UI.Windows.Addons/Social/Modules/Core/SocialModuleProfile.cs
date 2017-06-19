using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI.Windows.Plugins.Social.Queries;
using UnityEngine.UI.Windows.Plugins.Social.Core;

namespace UnityEngine.UI.Windows.Plugins.Social.Modules.Core {
	
	[System.Serializable]
	public class ProfileSettings : SubModuleSettings {
		
		public SocialQuery loadProfileQuery;
		
		public override SocialQuery GetQuery() {
			
			return this.loadProfileQuery;
			
		}

	}

	public interface IProfile : ISubModule {
		
		ISocialUser user { get; set; }
		void LoadInfo(Action<bool> callback);
		void LoadInfo(SubModuleSettings settings, Action<bool> callback);
		void LoadInfo(SocialQuery query, Action<bool> callback);
		
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
			
			this.LoadInfo(settings.GetQuery(), callback);

		}

		public virtual void LoadInfo(SocialQuery query, Action<bool> callback) {

			new SocialHttp(
				this.moduleSettings,
				query.url,
				this.moduleSettings.Prepare((this.socialModule.auth as IAuth).token, query.parameters),
				query.method,
				(data, result) => {
				
				Debug.Log(result + " :: " + data);
				
				this.user.ParseData(this.socialModule, data);

				callback(result);
				
			});
			
		}
		
	}

}