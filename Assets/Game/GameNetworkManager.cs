using System;
using UnityEngine.Networking;

public class GameNetworkManager : NetworkManager
{
	public event Action ClientConnected;
	public event Action ClientDisconnected;

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		base.OnClientDisconnect(conn);

		var disconnect = ClientDisconnected;
		if (disconnect != null)
		{
			disconnect();
		}
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);

		var connect = ClientConnected;
		if (connect != null)
		{
			connect();
		}
	}
}
