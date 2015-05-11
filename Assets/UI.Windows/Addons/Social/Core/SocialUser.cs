using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;
using System;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Plugins.Social.Core {
	
	public interface ISocialUser : ILocalUser {
		
		string userData {
			get;
			set;
		}

		//
		// Methods
		//
		void ParseData(SocialModule socialModule, string fulldata);
		
		void ParseFriendsData(SocialModule socialModule, string fulldata);

		new void Authenticate(Action<bool> callback);
		
		new void LoadFriends(Action<bool> callback);

		List<ISocialUser> ParseUsers(string inputData);

	}
	
	public class SocialUser : LocalUser, ISocialUser {
		
		protected SocialModule socialModule;
		
		public string userData {
			get;
			set;
		}
		
		public string friendsData {
			get;
			set;
		}

		public void ParseData(SocialModule socialModule, string fulldata) {
			
			this.socialModule = socialModule;
			this.userData = fulldata;
			
			this.OnUserParse();
			
		}

		public void ParseFriendsData(SocialModule socialModule, string friendsData) {
			
			this.socialModule = socialModule;
			this.friendsData = friendsData;
			
			this.OnFriendsParse();

		}

		new public void Authenticate(Action<bool> callback) {

			if (this.authenticated == true) {

				callback(true);

			} else {
				
				// Auth
				(this.socialModule as ISocialPlatform).Authenticate(this, callback);

			}

		}
		
		new public void LoadFriends(Action<bool> callback) {

			if (this.authenticated == false) {

				callback(false);

			} else {

				// Load friends
				(this.socialModule as ISocialPlatform).LoadFriends(this, callback);

			}

		}

		public virtual List<ISocialUser> ParseUsers(string inputData) {

			return null;

		}

		protected virtual void OnUserParse() {}
		protected virtual void OnFriendsParse() {}

	}

}