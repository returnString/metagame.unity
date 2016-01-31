using UnityEngine;
using UnityEngine.Networking;

public class Player : MonoBehaviour
{
	private NetworkIdentity m_ident;
	private Camera m_camera;

	void Awake()
	{
		m_ident = GetComponent<NetworkIdentity>();
		m_camera = GetComponentInChildren<Camera>();
	}

	void Start()
	{
		m_camera.gameObject.SetActive(m_ident.isLocalPlayer);
	}
}
