using Metagame.State;
using System.Collections.Generic;

namespace SampleGame.Users
{
	public class PurchaseableItem
	{
		public int Cost { get; set; }
	}

	public class AdvertisedUserData
	{
		public Dictionary<string, PurchaseableItem> Items { get; set; }
  }

	public class User : MetagameInstance
	{
		public int Currency { get; set; }
		public string[] Items { get; set; }
	}
}
