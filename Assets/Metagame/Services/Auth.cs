using System.Collections;

namespace Metagame.Auth
{
	public class AuthResponse
	{
		public string IP { get; set; }
		public string ID { get; set; }
	}

	public static class MetagameAuthExtensions
	{
		public static IEnumerator AuthenticateDebug(this MetagameClient metagame, MetagameTask<AuthResponse> task, string userID, string client = "game")
		{
			var request = new { client, userID };
			return metagame.MetagameSend(task, "/auth/login", request);
		}

		public static IEnumerator Logout(this MetagameClient metagame, MetagameTask<AuthResponse> task)
		{
			return metagame.MetagameSend(task, "/auth/logout", false);
		}
	}
}