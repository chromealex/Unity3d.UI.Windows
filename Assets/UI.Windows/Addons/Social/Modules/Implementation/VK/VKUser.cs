using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI.Windows.Plugins.Social.Core;

namespace UnityEngine.UI.Windows.Plugins.Social.Modules.Impl.VK {

	public class VKLocalUser : SocialUser {

		private bool _verified;
		public void SetVerified(int value) { this._verified = value == 1 ? true : false; }
		public void SetVerified(bool value) { this._verified = value; }
		public bool verified { get { return this._verified; } }
		
		//	returns 1 if the profile is verified, 0 if not. 
		//	flag, may take 1 and 0 values
		
		private bool _blacklisted;
		public void SetBlacklisted(int value) { this._blacklisted = value == 1 ? true : false; }
		public void SetBlacklisted(bool value) { this._blacklisted = value; }
		public bool blacklisted { get { return this._blacklisted; } }
		// returns 1 if a current user is in the requested user's blacklist. 
		// flag, may take 1 and 0 values
		
		private Sex _sex;
		public void SetSex(int value) { this._sex = (value == 1) ? Sex.Female : (value == 2 ? Sex.Male : Sex.NotSpecified); }
		public void SetSex(Sex value) { this._sex = value; }
		public Sex sex { get { return this._sex; } }
		// user sex.  One of three values is returned:
		/*
					1 — female;
					2 — male;
					0 — not specified.
				*/
		
		public int age { get; private set; }
		
		private string _bdate;
		public string bdate {
			get {
				return this._bdate;
			}
			private set {
				
				this._bdate = value;
				
				var splitted = value.Split('.');
				
				var year = int.Parse(splitted[2]);
				var month = int.Parse(splitted[1]);
				var day = int.Parse(splitted[0]);
				
				var bdate = new DateTime(year, month, day);
				
				DateTime today = DateTime.Today;
				this.age = today.Year - bdate.Year;
				if (bdate > today.AddYears(-this.age)) this.age--;
				
				this.SetUnderage(this.age < 18);
				
			}
		}
		//user's date of birth.  Returned as DD.MM.YYYY or DD.MM (if birth year is hidden). If the whole date is hidden, no field is returned.
		
		public int cityID { get; private set; }
		//ID of the city specified on user's page in "Contacts" section.  Returns city ID that can be used to get its name using places.getCityById method. If no city is specified or main information on the page is hidden for in privacy settings, then it returns 0.. 
		
		public int countryID { get; private set; }
		// of the country specified on user's page in "Contacts" section.  Returns country ID that can be used to get its name using places.getCountryById method. If no country is specified or main information on the page is hidden in privacy settings, then it returns 0. 
		// positive number
		// home_town	user's home town. 
		
		public Texture2D photo_50_image { get; private set; }
		public string photo_50 { get; private set; } // returns URL of square photo of the user with 50 pixels in width.  In case user does not have a photo, http://vk.com/images/camera_c.gif is returned. 
		
		public Texture2D photo_100_image { get; private set; }
		public string photo_100 { get; private set; } // 	returns URL of square photo of the user with 100 pixels in width.  In case user does not have a photo, http://vk.com/images/camera_b.gif is returned.  
		
		public Texture2D photo_200_orig_image { get; private set; }
		public string photo_200_orig { get; private set; } // 	returns URL of user's photo with 200 pixels in width.  In case user does not have a photo, http://vk.com/images/camera_a.gif is returned.  
		
		public Texture2D photo_200_image { get; private set; }
		public string photo_200 { get; private set; } // 	returns URL of square photo of the user with 200 pixels in width.  If the photo was uploaded long time ago, there can be no image of such size and in this case the reply will not include this field.  
		
		public Texture2D photo_400_orig_image { get; private set; }
		public string photo_400_orig { get; private set; } // 	returns URL of user's photo with 400 pixels in width.  If user does not have a photo of such size, reply will not include this field.  
		
		public Texture2D photo_max_image { get; private set; }
		public string photo_max { get; private set; } // 	returns URL of square photo of the user with maximum width. Can be returned a photo both 200 and 100 pixels in width.  In case user does not have a photo, http://vk.com/images/camera_b.gif is returned.  
		
		public Texture2D photo_max_orig_image { get; private set; }
		public string photo_max_orig { get; private set; } // 	returns URL of user's photo of maximum size. Can be returned a photo both 400 and 200 pixels in width.  In case user does not have a photo, http://vk.com/images/camera_a.gif is returned.  
		
		private int onlineState {
			set {
				this.SetState(value == 1 ? UnityEngine.SocialPlatforms.UserState.Online : UnityEngine.SocialPlatforms.UserState.Offline);
			}
		}
		// information whether the user is online.  Returned values: 1 — online, 0 — offline.  If user utilizes a mobile application or site mobile version, it returns online_mobile additional field that includes 1. With that, in case of application, online_app additional field is returned with application ID.  
		// flag, can be 1 or 0
		
		private bool _hasMobile;
		public void SetMobile(int value) { this._hasMobile = value == 1 ? true : false; }
		public void SetMobile(bool value) { this._hasMobile = value; }
		public bool hasMobile { get { return this._hasMobile; } }
		//Information whether the user's mobile phone number is available.  Returned values: 1 — available, 0 — not available.  We recommend you to use it prior to call of secure.sendSMSNotification method.  
		//flag, can be 1 or 0
		
		public string mobilePhone { private set; get; }
		public string homePhone { private set; get; }
		/*contacts	information about user's phone numbers.  If data are available and not hidden in privacy settings, the following fields are returned:
				mobile_phone — user's mobile phone number (only for standalone applications);
				home_phone — user's additional phone number.*/
		
		public string site { private set; get; }
		// site	returns a website address from a user profile.
		
		protected override void OnUserParse() {
			
			var data = new JSONObject(this.userData);
			
			var field = data.GetField("response");
			if (field.IsArray == true) {
				
				var item = field.list[0];
				
				this.ParseData(item);
				
			}
			
		}
		
		protected override void OnFriendsParse() {

			var friends = this.ParseUsers(this.friendsData);
			foreach (var friend in friends) (friend as SocialUser).SetIsFriend(true);
			this.SetFriends(friends.ToArray());

		}
		
		public override List<ISocialUser> ParseUsers(string inputData) {
			
			var result = new List<ISocialUser>();

			var data = new JSONObject(inputData);

			var field = data.GetField("response");
			if (field.IsArray == true) {

				foreach (var item in field.list) {
					
					var user = new VKLocalUser();
					user.socialModule = this.socialModule;
					user.userData = item.ToString();
					user.ParseData(item);

					result.Add(user);
					
				}

			}

			return result;
			
		}

		private void ParseData(JSONObject item) {
			
			this.SetSex((int)item.GetField("sex").n);
			this.SetUserID(item.GetField("uid").d.ToString());
			this.SetUserName(string.Format("{0} {1}", item.GetField("first_name").str, item.GetField("last_name").str));
			
			this.onlineState = (int)item.GetField("online").n;
			
			this.SetAuthenticated(true);
			this.SetState(UnityEngine.SocialPlatforms.UserState.Online);
			
			var settings = this.socialModule.GetSettings<VKSettings>();
			var mainPhoto = settings.profileMainPhoto;
			
			SocialSystem.LoadImage(mainPhoto == "photo_50", this.photo_50 = item.GetField("photo_50").str,				(texture, result) => this.photo_50_image = texture as Texture2D, (texture) => this.SetImage(texture as Texture2D));
			SocialSystem.LoadImage(mainPhoto == "photo_100", this.photo_100 = item.GetField("photo_100").str,				(texture, result) => this.photo_100_image = texture as Texture2D, (texture) => this.SetImage(texture as Texture2D));
			SocialSystem.LoadImage(mainPhoto == "photo_200_orig", this.photo_200_orig = item.GetField("photo_200_orig").str,	(texture, result) => this.photo_200_orig_image = texture as Texture2D, (texture) => this.SetImage(texture as Texture2D));
			SocialSystem.LoadImage(mainPhoto == "photo_200", this.photo_200 = item.GetField("photo_200").str,				(texture, result) => this.photo_200_image = texture as Texture2D, (texture) => this.SetImage(texture as Texture2D));
			SocialSystem.LoadImage(mainPhoto == "photo_400_orig", this.photo_400_orig = item.GetField("photo_400_orig").str,	(texture, result) => this.photo_400_orig_image = texture as Texture2D, (texture) => this.SetImage(texture as Texture2D));
			SocialSystem.LoadImage(mainPhoto == "photo_max", this.photo_max = item.GetField("photo_max").str,				(texture, result) => this.photo_max_image = texture as Texture2D, (texture) => this.SetImage(texture as Texture2D));
			SocialSystem.LoadImage(mainPhoto == "photo_max_orig", this.photo_max_orig = item.GetField("photo_max_orig").str,	(texture, result) => this.photo_max_orig_image = texture as Texture2D, (texture) => this.SetImage(texture as Texture2D));

		}
		
	}

}