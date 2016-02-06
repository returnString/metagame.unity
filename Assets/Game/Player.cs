using UnityEngine;
using UnityEngine.Networking;
using SampleGame.Users;

public class Player : NetworkBehaviour
{
	public float Speed = 6;
	public float RotationSpeed = 10;
	public float MaxInteractDistance = 10;

	public GameObject PlayerView;

	private CharacterController m_char;
	private PlayerView m_view;

	private float m_maxInteractDistanceSq;

	public User User { get; private set; }

	[SyncVar]
	private string m_name;

	[SyncVar]
	private int m_currency;

	void Awake()
	{
		m_maxInteractDistanceSq = Mathf.Pow(MaxInteractDistance, 2);
		m_char = GetComponent<CharacterController>();
	}

	void Start()
	{
		if (isLocalPlayer)
		{
			m_view = Instantiate(PlayerView).GetComponent<PlayerView>();
			m_view.Init(this);
		}
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
		if (Camera.main == null)
		{
			return;
		}

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

		var vert = Input.GetAxisRaw("Vertical");
		var horiz = Input.GetAxisRaw("Horizontal");

		var baseDir = new Vector3(horiz, 0, vert);
		var dir = baseDir.normalized;
		m_char.SimpleMove(dir * Speed);

		if (baseDir.sqrMagnitude > 0.3)
		{
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * RotationSpeed);
		}

		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;
			if (Physics.Raycast(m_view.Camera.ScreenPointToRay(Input.mousePosition), out hit))
			{
				if (Vector3.SqrMagnitude(hit.point - transform.position) < m_maxInteractDistanceSq)
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
