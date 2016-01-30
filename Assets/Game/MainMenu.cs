using Metagame;
using Metagame.Auth;
using System.Collections;
using UnityEngine;

public enum MenuState
{
	NotConnected,
	Connecting,
	LoginRequired,
	LoginInProgress,
	DownloadingData,
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

	private GameObject m_player;

	void Start()
	{
		m_metagame = FindObjectOfType<MetagameClient>();
		StartCoroutine(Connect());
	}

	void OnGUI()
	{
		switch (m_state)
		{
			case MenuState.NotConnected:
				{
					GUILayout.Label("Failed to connect to game servers: " + m_connectErrorText);
					if (GUILayout.Button("Reconnect"))
					{
						StartCoroutine(Connect());
					}
				}
				break;

			case MenuState.LoginRequired:
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
			m_state = MenuState.NotConnected;
		}
		else
		{
			m_state = MenuState.LoginRequired;
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
			m_state = MenuState.LoginRequired;
		}
		else
		{
			yield return StartCoroutine(DownloadData());
		}
	}

	IEnumerator DownloadData()
	{
		m_state = MenuState.DownloadingData;
		yield return StartCoroutine(Collection.Init(m_metagame));
		m_state = MenuState.Playing;
		StartPlaying();
  }

	void StartPlaying()
	{
		m_player = Instantiate(PlayerPrefab);
	}

	void StopPlaying()
	{
		Destroy(m_player);
  }

	IEnumerator Logout()
	{
		StopPlaying();
		var metaRef = new MetagameRef<AuthResponse>();
		yield return StartCoroutine(m_metagame.Logout(metaRef));
		m_state = MenuState.LoginRequired;
  }
}
