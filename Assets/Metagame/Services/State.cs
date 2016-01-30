using Newtonsoft.Json;
using System.Collections;

namespace Metagame.State
{
	public class AdvertisedResponse<T>
	{
		public T Advertised { get; set; }
	}

	public class InstanceResponse<T>
		where T : MetagameInstance
	{
		public T Instance { get; set; }
	}

	public class MetagameInstance
	{
		[JsonProperty(PropertyName = "_id")]
		public string ID { get; set; }
	}

	public static class MetagameStateExtensions
	{
		public static IEnumerator GetAdvertisedData<T>(this MetagameClient metagame, IMetagameTask<AdvertisedResponse<T>> task, string collection)
		{
			var request = new { collection };
			return metagame.Send(task, "/state/advertised", request);
		}

		public static IEnumerator GetInstance<T>(this MetagameClient metagame, IMetagameTask<InstanceResponse<T>> task, string collection, string id)
			where T : MetagameInstance
		{
			var request = new { collection, id };
			return metagame.Send(task, "/state/instance", request);
		}
	}
}