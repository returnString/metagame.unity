using Metagame;
using Metagame.Auth;
using Metagame.Matchmaking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;

public enum MenuState
{
	ConnectScreen,
	Connecting,
	LoginScreen,
	LoginInProgress,
	DownloadingData,
	MatchmakingScreen,
	MatchmakingInProgress,
	Playing,
}

public class MainMenu : MonoBehaviour
{
	public string MetagameUrl = "ws://localhost:1337";
	public GameObject PlayerPrefab;

	private MetagameClient m_metagame;
	private MenuState m_state;

	private string m_userNameText = string.Empty;

	private string m_loginErrorText;
	private string m_connectErrorText;

	private string m_partyID;
	private string m_joinedSessionID;
	private bool m_hosting;

	void Start()
	{
		m_metagame = FindObjectOfType<MetagameClient>();
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
		yield return StartCoroutine(m_metagame.Matchmake(metaRef, "easyPool", m_partyID, new string[0], new Dictionary<string, string>()));

		if (metaRef.Error != null)
		{
			m_state = MenuState.MatchmakingScreen;
		}
		else
		{
			m_joinedSessionID = metaRef.Data.SessionID;

			if (metaRef.Data.Action == MatchmakingSearchAction.Join)
			{
				var ip = metaRef.Data.HostPartyID.Split('|')[0].Replace(',', '.');
				NetworkManager.singleton.networkAddress = ip;
				NetworkManager.singleton.StartClient();
				m_hosting = false;
			}
			else
			{
				m_hosting = true;
				NetworkManager.singleton.StartHost();
      }

			StartPlaying();
		}
	}

	void StartPlaying()
	{
		m_state = MenuState.Playing;
	}

	void StopPlaying()
	{
		if (m_hosting)
		{
			NetworkManager.singleton.StopHost();
		}
		else
		{
			NetworkManager.singleton.StopClient();
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
