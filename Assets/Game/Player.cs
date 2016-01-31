using UnityEngine;
using UnityEngine.Networking;
using SampleGame.Users;

public class Player : NetworkBehaviour
{
	public float Speed = 6;
	public float RotateSpeed = 100;
	public float MaxInteractDistance = 10;

	private Camera m_camera;
	private CharacterController m_char;

	public User User { get; private set; }

	[SyncVar]
	private string m_name;

	[SyncVar]
	private int m_currency;

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
		LoadProfile(Collection.Users.GetInstance(Game.UserID, id => new User()));
	}

	public void LoadProfile(User user)
	{
		User = user;
		CmdSetCurrency(User.Currency);
		CmdSetName(User.ID);
	}

	[Command]
	void CmdSetCurrency(int currency)
	{
		m_currency = currency;
	}

	[Command]
	void CmdSetName(string name)
	{
		m_name = name;
	}

	void OnGUI()
	{
		var point = Camera.main.WorldToScreenPoint(transform.position + (Vector3.up * (m_char.height / 2)));
		var rect = new Rect(point.x, Screen.height - point.y - 20, 200, 200);
		GUI.Label(rect, m_name);
		rect.y += 20;
		GUI.Label(rect, "Currency: " + m_currency);
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
