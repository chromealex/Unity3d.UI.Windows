using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI.Windows.Plugins.Social.Modules.Core;
using UnityEngine.UI.Windows.Plugins.Social.Queries;

namespace UnityEngine.UI.Windows.Plugins.Social.Core {
	
	public class ModuleSettings : ScriptableObject {
		
		public virtual string GetPlatformName() {
			
			return "NoName";
			
		}
		
		public virtual string GetPlatformClassName() {
			
			return "Module";
			
		}

		[Header("Base Settings")]
		public UsersSettings standardUsersSettings;
		public FriendsSettings standardFriendsSettings;
		public ProfileSettings standardProfileSettings;
		public AuthSettings authSettings;

		public virtual HTTPParams Prepare(string token, HTTPParams parameters, Dictionary<string, string> values = null) {
			
			if (values != null) {
				
				foreach (var value in values) {
					
					foreach (var parameter in parameters.items) {
						
						if (parameter.key == value.Key) {
							
							parameter.value = value.Value;
							
						}
						
					}
					
				}
				
			}
			
			return parameters;
			
		}

		public virtual string GetPermissions() {
			
			return string.Empty;
			
		}

#if UNITY_EDITOR
		public virtual void OnInspectorGUI() {

		}
#endif

	}

	public interface ISocialModule : ISocialPlatform {

		void Authenticate(Action<bool> callback);
		void OnLoad(ModuleSettings settings);
		
	}

	public class SocialModule : ISocialModule {

		private const string NOT_IMPLEMENTED_FEATURE = "Not implemented";

		protected ModuleSettings settings;

		public List<ISubModule> submodules = null;
		
		public IAuth auth = null;
		public IProfile profile = null;
		public IFriends friends = null;
		public IUsers users = null;

		//
		// Properties
    	//
		ILocalUser ISocialPlatform.localUser {
			get {
				return this.profile.user;
			}
		}
		
		//
		// Methods
		//
		#region ACHIEVEMENTS & LEADERBOARDS
		IAchievement ISocialPlatform.CreateAchievement() { return null; }
		
		ILeaderboard ISocialPlatform.CreateLeaderboard() { return null; }
		
		void ISocialPlatform.LoadAchievementDescriptions(Action<IAchievementDescription[]> callback) {}
		
		void ISocialPlatform.LoadAchievements(Action<IAchievement[]> callback) {}
		
		bool ISocialPlatform.GetLoading(ILeaderboard board) { return false; }

		void ISocialPlatform.ShowAchievementsUI() {

		}
		
		void ISocialPlatform.ShowLeaderboardUI() {}
		#endregion

		#region SCORES
		void ISocialPlatform.LoadScores(ILeaderboard board, Action<bool> callback) {}
		
		void ISocialPlatform.LoadScores(string leaderboardID, Action<IScore[]> callback) {}

		void ISocialPlatform.ReportProgress(string achievementID, double progress, Action<bool> callback) {}
		
		void ISocialPlatform.ReportScore(long score, string board, Action<bool> callback) {}
		#endregion

		void ISocialPlatform.LoadUsers(string[] userIDs, Action<IUserProfile[]> callback) {

			this.LoadUsers(userIDs, (ISocialUser[] users) => callback(users.Cast<IUserProfile>().ToArray()));

		}
		
		void ISocialPlatform.Authenticate(ILocalUser user, Action<bool> callback) {

			this.Authenticate(callback);

		}
		
		void ISocialPlatform.LoadFriends(ILocalUser user, Action<bool> callback) {

			this.LoadFriends(callback);

		}

		/*
		public T GetSubModule<T>() where T : SubModuleSettings {

			return this.submodules.OfType<T>().FirstOrDefault();

		}

		public void LoadUserInfo(Action<bool> callback) {

			var profile = this.GetSubModule<IProfile>();
			profile.LoadInfo(callback);

		}*/

		public virtual void LoadUsers(string[] userIDs, Action<ISocialUser[]> callback) {
			
			(this.users as IUsers).LoadInfo(userIDs, callback);
			
		}
		
		public virtual void LoadFriends(Action<bool> callback) {
			
			(this.friends as IFriends).LoadInfo(callback);
			
		}

		public virtual void Authenticate(Action<bool> callback) {
			
			(this.auth as IAuth).Authenticate(callback);

		}
		
		public virtual void Authenticate(string token, Action<bool> callback) {
			
			(this.auth as IAuth).Authenticate(token, callback);

		}

		public ISocialPlatform GetSocialPlatform() {

			return this;

		}

		public T GetSettings<T>() where T : ModuleSettings {

			return this.settings as T;

		}

		public virtual void OnLoad(ModuleSettings settings) {

			this.settings = settings;

			this.submodules = new List<ISubModule>();

			this.OnLoadModules();
			this.OnLoadUser();

		}

		public virtual void OnLoadModules() {

			this.RegisterModule<IAuth>(this.auth = new Auth<AuthSettings>(this, this.settings, this.settings.authSettings));
			this.RegisterModule<IProfile>(this.profile = new Profile<ProfileSettings>(this, this.settings, this.settings.standardProfileSettings));
			this.RegisterModule<IFriends>(this.friends = new Friends<FriendsSettings>(this, this.settings, this.settings.standardFriendsSettings));
			this.RegisterModule<IUsers>(this.users = new Users<UsersSettings>(this, this.settings, this.settings.standardUsersSettings));

		}

		protected void RegisterModule<T>(T module) where T : ISubModule {

			if (this.submodules.Contains(module) == true) this.submodules.Remove(module);
			this.submodules.Add(module);

		}

		public virtual void OnLoadUser() {
			
			// Create an empty user
			this.profile.user = new SocialUser();

		}
		
	}

}