using UnityEngine;

public class PlayerView : MonoBehaviour
{
	public float Height;
	public float Distance;
	public float Speed;

	public Camera Camera { get; private set; }

	private Player m_player;

	void Awake()
	{
		Camera = GetComponent<Camera>();
		Camera.tag = "MainCamera";
	}

	public void Init(Player player)
	{
		m_player = player;
		transform.position = m_player.transform.position + new Vector3(0, Height, -Distance);
	}

	void Update()
	{
		if (m_player == null)
		{
			return;
		}

		var playerPos = m_player.transform.position;
		var targetPos = playerPos + new Vector3(0, Height, -Distance);
		transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * Speed);
		transform.LookAt(playerPos);
	}
}