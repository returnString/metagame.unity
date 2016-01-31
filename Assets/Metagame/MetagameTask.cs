
namespace Metagame
{
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

	public class MetagameTask<TData>
	{
		public TData Data { get { return m_response.Data; } }
		public MetagameError Error { get { return m_response.Error; } }

		private MetagameResponse<TData> m_response;

		public void OnClientError(MetagameClientError error)
		{
			m_response = new MetagameResponse<TData>
			{
				Error = new MetagameError { Name = "client/" + error }
			};
		}

		public void OnResponse(MetagameResponse<TData> response)
		{
			m_response = response;
		}

		public void Reset()
		{
			m_response = null;
		}
	}
}
