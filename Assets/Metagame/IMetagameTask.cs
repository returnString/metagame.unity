
namespace Metagame
{
	public interface IMetagameTask<TData>
	{
		void OnClientError(MetagameClientError error);
		void OnResponse(MetagameResponse<TData> response);

		void Reset();
	}

	public class MetagameError
	{
		public string Name { get; set; }
	}

	public class MetagameResponse<TData>
	{
		public TData Data { get; set; }
		public MetagameError Error { get; set; }
	}

	public enum MetagameClientError
	{
		NotConnected,
		SendFailed,
	}
}
