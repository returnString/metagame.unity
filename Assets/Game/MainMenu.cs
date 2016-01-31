using Metagame;
using Metagame.Auth;
using Metagame.Matchmaking;
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

public class MainMenu : MonoBehaviour
{
	public string MetagameUrl = "ws://localhost:1337";
	public Camera MainCamera;

	private MetagameClient m_metagame;
	private GameNetworkManager m_netManager;
	private MenuState m_state;

	private string m_userNameText = string.Empty;

	private string m_loginErrorText;
	private string m_connectErrorText;

	private string m_partyID;
	private string m_joinedSessionID;
	private bool m_hosting;
	private List<string> m_badTickets;

	void Start()
	{
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
				}
				break;

			case MenuState.MatchmakingInProgress:
				{
					GUILayout.Label("Finding sessions...");
				}
				break;

			case MenuState.SessionConnectionInProgress:
				{
					GUILayout.Label("Connecting to session...");
				}
				break;

			case MenuState.Playing:
				{
					if (GUILayout.Button("Log out"))
					{
						StartCoroutine(Logout());
					}
				}
				break;
		}
	}

	IEnumerator Connect()
	{
		m_state = MenuState.Connecting;
		var metaRef = new MetagameRef<ConnectResponse>();
		yield return StartCoroutine(m_metagame.Connect(metaRef, MetagameUrl));
		if (metaRef.Error != null)
		{
			m_connectErrorText = metaRef.Error.Name;
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
		var metaRef = new MetagameRef<AuthResponse>();
		yield return StartCoroutine(m_metagame.Authenticate(metaRef, m_userNameText));
		if (metaRef.Error != null)
		{
			m_loginErrorText = metaRef.Error.Name;
			m_state = MenuState.LoginScreen;
		}
		else
		{
			// hack to generate party IDs for debug platforms
			m_partyID = metaRef.Data.IP.Replace('.', ',') + "|" + Process.GetCurrentProcess().Id;
			yield return StartCoroutine(DownloadData());
		}
	}

	IEnumerator DownloadData()
	{
		m_state = MenuState.DownloadingData;
		yield return StartCoroutine(Collection.Init(m_metagame));
		m_state = MenuState.MatchmakingScreen;
	}

	IEnumerator Matchmake()
	{
		m_state = MenuState.MatchmakingInProgress;
		var metaRef = new MetagameRef<MatchmakingSearchResponse>();
		yield return StartCoroutine(m_metagame.Matchmake(metaRef, "easyPool", m_partyID, new string[0], m_badTickets.ToArray(), new Dictionary<string, string>()));

		if (metaRef.Error != null)
		{
			m_state = MenuState.MatchmakingScreen;
		}
		else
		{
			m_joinedSessionID = metaRef.Data.SessionID;
			m_hosting = metaRef.Data.Action == MatchmakingSearchAction.Create;
			if (!m_hosting)
			{
				var ip = metaRef.Data.HostPartyID.Split('|')[0].Replace(',', '.');
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
		}
	}

	private void OnClientConnect()
	{
		StartPlaying();
	}

	private void OnClientDisconnect()
	{
		m_badTickets.Add(m_joinedSessionID);
		StopPlaying();
	}

	void StartPlaying()
	{
		MainCamera.gameObject.SetActive(false);
		m_state = MenuState.Playing;
	}

	void StopPlaying()
	{
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
		StopPlaying();
		var metaRef = new MetagameRef<AuthResponse>();
		yield return StartCoroutine(m_metagame.Logout(metaRef));
		m_state = MenuState.LoginScreen;
	}
}
