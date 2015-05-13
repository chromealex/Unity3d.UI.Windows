using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI.Windows.Plugins.Social.Core;

namespace UnityEngine.UI.Windows.Plugins.Social.Modules.Impl.FB {

	public class FBLocalUser : SocialUser {

		public int age;

		private string _bdate;
		public string bdate {
			get {
				return this._bdate;
			}
			private set {

				if (string.IsNullOrEmpty(value) == true) return;

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
		// The person's birthday. This is a fixed format string, like MM/DD/YYYY. However, people can control who can see the year they were born separately from the month and day so this string can be only the year (YYYY) or the month + day (MM/DD)

		public List<UserDevice> devices;
		// The list of devices the person is using. This will return only iOS and Android devices
		
		private Sex _sex;
		public void SetSex(string value) { this._sex = (value == "female") ? Sex.Female : (value == "male" ? Sex.Male : Sex.NotSpecified); }
		public void SetSex(Sex value) { this._sex = value; }
		public Sex sex { get { return this._sex; } }
		// The gender selected by this person, male or female. This value will be omitted if the gender is set to a custom value

		public string about;
		// The About Me section of this person's profile
		
		public string bio;
		// The person's bio

		public string email;
		// The person's primary email address listed on their profile. This field will not be returned if no valid email address is available

		public bool installed;

		public bool is_verified;
		// People with large numbers of followers can have the authenticity of their identity manually verified by Facebook. This field indicates whether the person's profile is verified in this way. This is distinct from the verified field

		public string link;
		// A link to the person's Timeline

		public string locale;
		// The person's locale

		public string political;
		// The person's political views

		public string relationship_status;
		// The person's relationship status

		public string religion;
		// The person's religion

		public string quotes;
		// The person's favorite quotes

		public string third_party_id;
		// A string containing an anonymous, but unique identifier for the person. You can use this identifier with third parties

		public float timezone;
		// float (min: -24) (max: 24)
		// The person's current timezone offset from UTC

		public string token_for_business;
		// A token that is the same across a business's apps. Access to this token requires that the person be logged into your app. This token will change if the business owning the app changes

		public bool verified;
		// Indicates whether the account has been verified. This is distinct from the is_verified field. Someone is considered verified if they take any of the following actions:
		/*
		Register for mobile
		Confirm their account via SMS
		Enter a valid credit card
		*/

		public string website;
		// person's website

		public Texture2D photo_50_image { get; private set; }
		public string photo_50 { get; private set; }
		// avatar

		public bool photoIsDefault;
		// Is avatar default?

		protected override void OnUserParse() {
			
			var data = new JSONObject(this.userData);
			this.ParseData(data);

		}
		
		protected override void OnFriendsParse() {

			var friends = this.ParseUsers(this.friendsData);
			foreach (var friend in friends) (friend as SocialUser).SetIsFriend(true);
			this.SetFriends(friends.ToArray());

		}
		
		public override List<ISocialUser> ParseUsers(string inputData) {
			
			var result = new List<ISocialUser>();

			var data = new JSONObject(inputData);

			var field = data.GetField("data");
			if (field.IsArray == true) {

				foreach (var item in field.list) {
					
					var user = new FBLocalUser();
					user.socialModule = this.socialModule;
					user.userData = item.ToString();
					user.ParseData(item);

					result.Add(user);
					
				}

			}

			return result;
			
		}

		private void ParseData(JSONObject item) {
			
			var settings = this.socialModule.GetSettings<FBSettings>();
			var mainPhoto = settings.profileMainPhoto;

			var picture = item.GetField("picture");
			if (picture.isContainer == true) {

				picture = picture.GetField("data");
				if (picture.isContainer == true) {

					var url = picture.GetField("url").str;
					this.photoIsDefault = picture.GetField("is_silhouette").b;
					SocialSystem.LoadImage(mainPhoto == "photo_50", this.photo_50 = url, (texture, result) => this.photo_50_image = texture as Texture2D, (texture) => this.SetImage(texture as Texture2D));

				}

			}

			var items = item.GetField("devices");
			if (items != null && items.IsArray == true) {

				this.devices = new List<UserDevice>();
				foreach (var device in items.list) {

					this.devices.Add(new UserDevice(device));

				}

			}

			this.SetSex(item.GetField("gender").str);
			this.SetUserID(item.GetField("id").str);
			this.SetUserName(item.GetField("name").str);

			this.bdate = item.GetField("birthday").str;
			
			this.about = item.GetField("about").str;
			this.bio = item.GetField("bio").str;
			this.installed = item.GetField("installed").b;
			this.is_verified = item.GetField("is_verified").b;
			this.link = item.GetField("link").str;
			this.locale = item.GetField("locale").str;
			this.political = item.GetField("political").str;
			this.relationship_status = item.GetField("relationship_status").str;
			this.religion = item.GetField("religion").str;
			this.quotes = item.GetField("quotes").str;
			this.third_party_id = item.GetField("third_party_id").str;
			this.timezone = item.GetField("timezone").f;
			this.token_for_business = item.GetField("token_for_business").str;
			this.verified = item.GetField("verified").b;
			this.website = item.GetField("website").str;

			this.SetAuthenticated(true);
			this.SetState(UnityEngine.SocialPlatforms.UserState.Online);

		}
		
	}

}