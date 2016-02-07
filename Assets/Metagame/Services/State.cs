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
		[JsonProperty("_id")]
		public string ID { get; set; }
	}

	public class ChangeRequest
	{
		[JsonProperty("name")]
		public string Name;
		[JsonProperty("params")]
		public object Params;

		public ChangeRequest(string name, object changeParams)
		{
			Name = name;
			Params = changeParams;
		}
	}

	public static class MetagameStateExtensions
	{
		public static IEnumerator GetAdvertisedData<T>(this MetagameClient metagame, MetagameTask<AdvertisedResponse<T>> task, string collection)
		{
			var request = new { collection };
			return metagame.MetagameSend(task, "/state/advertised", request);
		}

		public static IEnumerator GetInstance<T>(this MetagameClient metagame, MetagameTask<InstanceResponse<T>> task, string collection, string id)
			where T : MetagameInstance
		{
			var request = new { collection, id };
			return metagame.MetagameSend(task, "/state/instance", request);
		}

		public static IEnumerator ModifyInstance<T>(this MetagameClient metagame, MetagameTask<InstanceResponse<T>> task, string collection, string id, params ChangeRequest[] changes)
			where T : MetagameInstance
		{
			var request = new { collection, id, changes };
			return metagame.MetagameSend(task, "/state/modify", request);
		}
	}
}