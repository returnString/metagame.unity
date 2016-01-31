using System.Collections;

namespace Metagame.Auth
{
	public class AuthResponse
	{
		public string IP { get; set; }
	}

	public static class MetagameAuthExtensions
	{
		public static IEnumerator Authenticate(this MetagameClient metagame, IMetagameTask<AuthResponse> task, object key, string client = "game")
		{
			var request = new { client, key };
			return metagame.Send(task, "/auth/login", request);
		}

		public static IEnumerator Logout(this MetagameClient metagame, IMetagameTask<AuthResponse> task)
		{
			return metagame.Send(task, "/auth/logout", false);
		}
	}
}