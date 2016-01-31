using System.Collections;

namespace Metagame.Matchmaking
{
	public enum MatchmakingSearchAction
	{
		Create,
		Join,
	}

	public class MatchmakingSearchResponse
	{
		public MatchmakingSearchAction Action { get; set; }
		public string SessionID { get; set; }
		public string HostPartyID { get; set; }
	}

	public class MatchmakingPingResponse
	{
	}

	public class MatchmakingLeaveResponse
	{
	}

	public static class MetagameMatchmakingExtensions
	{
		public static IEnumerator Matchmake<TSessionValues>(this MetagameClient metagame, IMetagameTask<MatchmakingSearchResponse> task,
			string pool, string partyID, string[] members, string[] excludedSessions, TSessionValues sessionValues)
		{
			var request = new { pool, partyID, members, sessionValues, excludedSessions };
			return metagame.Send(task, "/matchmaking/search", request);
		}

		public static IEnumerator PingMatchmakingSession(this MetagameClient metagame, IMetagameTask<MatchmakingPingResponse> task,
			string pool, string partyID, string sessionID)
		{
			var request = new { pool, partyID, sessionID };
			return metagame.Send(task, "/matchmaking/ping", request);
		}

		public static IEnumerator LeaveMatchmakingSession(this MetagameClient metagame, IMetagameTask<MatchmakingLeaveResponse> task,
			string pool, string partyID, string sessionID)
		{
			var request = new { pool, partyID, sessionID };
			return metagame.Send(task, "/matchmaking/leave", request);
		}
	}
}
