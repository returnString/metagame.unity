using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
	public float Speed = 6;
	public float RotateSpeed = 100;
	public float MaxInteractDistance = 10;

	private Camera m_camera;
	private CharacterController m_char;

	[SyncVar]
	private string m_name;

	void Awake()
	{
		m_camera = GetComponentInChildren<Camera>();
		m_camera.tag = "MainCamera";
		m_char = GetComponent<CharacterController>();
	}

	void Start()
	{
		m_camera.gameObject.SetActive(isLocalPlayer);
	}

	public override void OnStartLocalPlayer()
	{
		CmdSetName(Game.UserID);
	}

	[Command]
	void CmdSetName(string name)
	{
		m_name = name;
	}

	void OnGUI()
	{
		var point = Camera.main.WorldToScreenPoint(transform.position + (Vector3.up * (m_char.height / 2)));
		GUI.Label(new Rect(point.x, Screen.height - point.y, 200, 200), m_name);
	}

	void Update()
	{
		if (!isLocalPlayer)
		{
			return;
		}

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
