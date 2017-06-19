using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI.Windows.Plugins.Social.Queries;
using UnityEngine.UI.Windows.Plugins.Social.Core;

namespace UnityEngine.UI.Windows.Plugins.Social.Modules.Core {
	
	[System.Serializable]
	public class FriendsSettings : SubModuleSettings {
		
		public SocialQuery loadFriendsQuery;
		
		public override SocialQuery GetQuery() {
			
			return this.loadFriendsQuery;
			
		}

	}

	public interface IFriends : ISubModule {

		void LoadInfo(Action<bool> callback);
		void LoadInfo(SubModuleSettings settings, Action<bool> callback);
		void LoadInfo(SocialQuery query, Action<bool> callback);

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
				
				this.socialModule.profile.user.ParseFriendsData(this.socialModule, data);

				callback(result);
				
			});
			
		}

	}

}