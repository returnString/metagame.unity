using UnityEngine;
using UnityEngine.Networking;

public class Player : MonoBehaviour
{
	public float Speed = 6;
	public float RotateSpeed = 100;
	public float MaxInteractDistance = 10;

	private NetworkIdentity m_ident;
	private Camera m_camera;
	private CharacterController m_char;

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

			if (Input.GetMouseButtonDown(0))
			{
				RaycastHit hit;
				if (Physics.Raycast(m_camera.ScreenPointToRay(Input.mousePosition), out hit, MaxInteractDistance))
				{
					var interactive = hit.collider.gameObject.GetComponent<Interactive>();
					if (interactive != null)
					{
						interactive.OnLocalPlayerInteract(this);
					}
				}
			}
		}
	}
}
