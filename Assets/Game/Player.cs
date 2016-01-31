using UnityEngine;
using UnityEngine.Networking;

public class Player : MonoBehaviour
{
	public float Speed = 6;
	public float RotateSpeed = 100;

	private NetworkIdentity m_ident;
	private Camera m_camera;
	private CharacterController m_char;
	private Vector3 m_lastMousePos;

	void Awake()
	{
		m_ident = GetComponent<NetworkIdentity>();
		m_camera = GetComponentInChildren<Camera>();
		m_char = GetComponent<CharacterController>();
	}

	void Start()
	{
		m_camera.gameObject.SetActive(m_ident.isLocalPlayer);
	}

	void Update()
	{
		if (m_ident.isLocalPlayer)
		{
			var vert = Input.GetAxis("Vertical");
			var horiz = Input.GetAxis("Horizontal");
			var dir = transform.TransformDirection(new Vector3(0, 0, vert));
			m_char.SimpleMove(dir * Speed);
			transform.Rotate(0, horiz * RotateSpeed * Time.deltaTime, 0);
		}
	}
}
