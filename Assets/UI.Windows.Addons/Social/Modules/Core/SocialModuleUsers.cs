using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Social.Queries;
using UnityEngine.UI.Windows.Plugins.Social.Core;

namespace UnityEngine.UI.Windows.Plugins.Social.Modules.Core {
	
	[System.Serializable]
	public class UsersSettings : SubModuleSettings {
		
		public SocialQuery loadUsersQuery;
		
		public override SocialQuery GetQuery() {
			
			return this.loadUsersQuery;
			
		}

	}

	public interface IUsers : ISubModule {
		
		void LoadInfo(string[] userIDs, Action<ISocialUser[]> callback);
		void LoadInfo(SubModuleSettings settings, string[] userIDs, Action<ISocialUser[]> callback);
		void LoadInfo(SocialQuery query, string[] userIDs, Action<ISocialUser[]> callback);

	}
	
	public class Users<TSettings> : SubModule<TSettings>, IUsers where TSettings : SubModuleSettings {

		public Users(
			SocialModule socialModule,
			ModuleSettings moduleSettings,
			TSettings settings) : base(socialModule, moduleSettings, settings) {
			
		}
		
		public virtual void LoadInfo(string[] userIDs, Action<ISocialUser[]> callback) {
			
			this.LoadInfo(this.settings, userIDs, callback);
			
		}

		public virtual void LoadInfo(SubModuleSettings settings, string[] userIDs, Action<ISocialUser[]> callback) {

			this.LoadInfo(settings.GetQuery(), userIDs, callback);

		}
		
		public virtual void LoadInfo(SocialQuery query, string[] userIDs, Action<ISocialUser[]> callback) {

			var ids = this.PrepareIds(userIDs);

			new SocialHttp(
				this.moduleSettings,
				query.url,
				this.moduleSettings.Prepare(
					(this.socialModule.auth as IAuth).token,
					query.parameters,
					new Dictionary<string, string>() { { "USER_IDS", ids } }
				),
				query.method,
				(data, result) => {
				
				if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log(result + " :: " + data);
				
				var list = this.socialModule.profile.user.ParseUsers(data);

				callback(list.ToArray());

			});
			
		}

		public virtual string PrepareIds(string[] userIDs) {

			return string.Join(",", userIDs);

		}

	}

}