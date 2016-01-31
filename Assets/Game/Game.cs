using Metagame;
using Metagame.Auth;
using Metagame.Matchmaking;
using Metagame.State;
using SampleGame.Users;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public enum MenuState
{
	ConnectScreen,
	Connecting,
	LoginScreen,
	LoginInProgress,
	DownloadingData,
	MatchmakingScreen,
	MatchmakingInProgress,
	SessionConnectionInProgress,
	Playing,
}

public class Game : MonoBehaviour
{
	public string MatchmakingPool = "easyPool";
	public string MetagameUrl = "ws://localhost:1337";
	public Camera MainCamera;

	public static string UserID { get; private set; }

	private MetagameClient m_metagame;
	private GameNetworkManager m_netManager;
	private MenuState m_state;

	private string m_userNameText;

	private string m_loginErrorText;
	private string m_connectErrorText;

	private string m_partyID;
	private string m_currentSessionID;
	private bool m_hosting;
	private List<string> m_badTickets;
	private Coroutine m_pingLoop;

	private string GetDefaultUsername()
	{
		return Environment.UserName + "/" + Process.GetCurrentProcess().Id;
	}

	void Start()
	{
		m_userNameText = GetDefaultUsername();
		m_badTickets = new List<string>();
		m_metagame = GetComponent<MetagameClient>();
		m_netManager = GetComponent<GameNetworkManager>();
		StartCoroutine(Connect());
	}

	void OnGUI()
	{
		switch (m_state)
		{
			case MenuState.ConnectScreen:
				{
					GUILayout.Label("Failed to connect to game servers: " + m_connectErrorText);
					if (GUILayout.Button("Reconnect"))
					{
						StartCoroutine(Connect());
					}
				}
				break;

			case MenuState.LoginScreen:
				{
					if (m_loginErrorText != null)
					{
						GUILayout.Label("An error occurred: " + m_loginErrorText);
					}

					GUILayout.Label("Please log in");
					GUILayout.Label("Username:");
					m_userNameText = GUILayout.TextField(m_userNameText);

					if (GUILayout.Button("Log in"))
					{
						m_loginErrorText = null;
						StartCoroutine(Login());
					}
				}
				break;

			case MenuState.LoginInProgress:
				{
					GUILayout.Label("Login in progress...");
				}
				break;

			case MenuState.DownloadingData:
				{
					GUILayout.Label("Downloading data...");
				}
				break;

			case MenuState.MatchmakingScreen:
				{
					if (GUILayout.Button("Matchmake"))
					{
						StartCoroutine(Matchmake());
					}

					if (GUILayout.Button("Log out"))
					{
						StartCoroutine(Logout());
					}
				}
				break;

			case MenuState.MatchmakingInProgress:
				{
					GUILayout.Label("Finding sessions...");
				}
				break;

			case MenuState.SessionConnectionInProgress:
				{
					GUILayout.Label("Connecting to session: " + m_currentSessionID);
				}
				break;

			case MenuState.Playing:
				{
					if (GUILayout.Button("Leave session: " + m_currentSessionID))
					{
						StopPlaying();
					}
				}
				break;
		}
	}

	IEnumerator Connect()
	{
		m_state = MenuState.Connecting;
		var task = new MetagameTask<ConnectResponse>();
		yield return StartCoroutine(m_metagame.Connect(task, MetagameUrl));
		if (task.Error != null)
		{
			m_connectErrorText = task.Error.Name;
			m_state = MenuState.ConnectScreen;
		}
		else
		{
			m_state = MenuState.LoginScreen;
		}
	}

	IEnumerator Login()
	{
		m_state = MenuState.LoginInProgress;
		var task = new MetagameTask<AuthResponse>();
		yield return StartCoroutine(m_metagame.AuthenticateDebug(task, m_userNameText));
		if (task.Error != null)
		{
			m_loginErrorText = task.Error.Name;
			m_state = MenuState.LoginScreen;
		}
		else
		{
			// hack to generate party IDs for debug platforms
			m_partyID = task.Data.IP.Replace('.', ',') + "|" + Process.GetCurrentProcess().Id;
			UserID = task.Data.ID;
			yield return StartCoroutine(DownloadData());
		}
	}

	IEnumerator DownloadData()
	{
		m_state = MenuState.DownloadingData;
		yield return StartCoroutine(Collection.Init(m_metagame));

		var task = new MetagameTask<InstanceResponse<User>>();
		yield return Collection.Users.DownloadInstance(task, UserID);

		m_state = MenuState.MatchmakingScreen;
	}

	IEnumerator Matchmake()
	{
		m_state = MenuState.MatchmakingInProgress;
		var task = new MetagameTask<MatchmakingSearchResponse>();
		yield return StartCoroutine(m_metagame.Matchmake(task, MatchmakingPool, m_partyID, new string[0], m_badTickets.ToArray(), new Dictionary<string, string>()));

		if (task.Error != null)
		{
			m_state = MenuState.MatchmakingScreen;
		}
		else
		{
			m_currentSessionID = task.Data.SessionID;
			m_hosting = task.Data.Action == MatchmakingSearchAction.Create;
			if (!m_hosting)
			{
				var ip = task.Data.HostPartyID.Split('|')[0].Replace(',', '.');
				m_netManager.networkAddress = ip;
				m_netManager.StartClient();
			}
			else
			{
				m_netManager.StartHost();
			}

			m_state = MenuState.SessionConnectionInProgress;
			m_netManager.ClientConnected += OnClientConnect;
			m_netManager.ClientDisconnected += OnClientDisconnect;
			m_pingLoop = StartCoroutine(PingLoop(task.Data.TTL));
		}
	}

	IEnumerator PingLoop(int ttl)
	{
		var task = new MetagameTask<MatchmakingPingResponse>();
		while (true)
		{
			yield return StartCoroutine(m_metagame.PingMatchmakingSession(task, MatchmakingPool, m_partyID, m_currentSessionID));
			yield return new WaitForSeconds(ttl / 2);
		}
	}

	void LeaveSession()
	{
		StopCoroutine(m_pingLoop);
		m_pingLoop = null;
		var task = new MetagameTask<MatchmakingLeaveResponse>();
		StartCoroutine(m_metagame.LeaveMatchmakingSession(task, MatchmakingPool, m_partyID, m_currentSessionID));
	}

	private void OnClientConnect()
	{
		StartPlaying();
	}

	private void OnClientDisconnect()
	{
		m_badTickets.Add(m_currentSessionID);
		StopPlaying();
	}

	void StartPlaying()
	{
		MainCamera.gameObject.SetActive(false);
		m_state = MenuState.Playing;
	}

	void StopPlaying()
	{
		LeaveSession();

		MainCamera.gameObject.SetActive(true);
		m_netManager.ClientConnected -= OnClientConnect;
		m_netManager.ClientDisconnected -= OnClientDisconnect;

		if (m_hosting)
		{
			m_netManager.StopHost();
		}
		else
		{
			m_netManager.StopClient();
		}

		// we've already recorded the bad session ID so we can restart matchmaking automatically if we failed to connect to a session at all
		if (m_state == MenuState.SessionConnectionInProgress)
		{
			StartCoroutine(Matchmake());
		}
		else
		{
			m_state = MenuState.MatchmakingScreen;
		}
	}

	IEnumerator Logout()
	{
		var task = new MetagameTask<AuthResponse>();
		yield return StartCoroutine(m_metagame.Logout(task));
		m_state = MenuState.LoginScreen;
	}
}
