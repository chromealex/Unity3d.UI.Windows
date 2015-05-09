using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows.Plugins.Social {

	public interface ISocialModule : ISocialPlatform {

		void Authenticate(Action<bool> callback);
		void OnLoad(ModuleSettings settings);
		
	}

	public class SocialModule : ISocialModule {
		
		protected ModuleSettings settings;

		public List<ISubModule> submodules = null;
		
		public IAuth auth = null;
		public IProfile profile = null;
		public IFriends friends = null;

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

		void ISocialPlatform.ShowAchievementsUI() {}
		
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
		
		public virtual void LoadFriends(Action<bool> callback) {
			
			
			
		}

		public virtual void LoadUsers(string[] userIDs, Action<ISocialUser[]> callback) {



		}

		public virtual void Authenticate(Action<bool> callback) {

			//this.GetSocialPlatform().Authenticate(this.GetSocialPlatform().localUser, callback);

		}
		
		public virtual void Authenticate(string token, Action<bool> callback) {
			
			//this.GetSocialPlatform().Authenticate(this.GetSocialPlatform().localUser, callback);

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

			this.submodules.Add(this.auth = new Auth<AuthSettings>(this, this.settings, this.settings.authSettings));
			this.submodules.Add(this.profile = new Profile<ProfileSettings>(this, this.settings, this.settings.standardProfileSettings));
			this.submodules.Add(this.friends = new Friends<FriendsSettings>(this, this.settings, this.settings.standardFriendsSettings));

		}

		public virtual void OnLoadUser() {
			
			// Create an empty user
			this.profile.user = new SocialUser();

		}
		
	}

}