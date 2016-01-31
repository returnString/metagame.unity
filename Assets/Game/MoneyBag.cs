using Metagame;
using Metagame.State;
using System.Collections;
using SampleGame.Users;

public class MoneyBag : Interactive
{
	public int Value = 100;

	private bool m_applying;

	public override void OnLocalPlayerInteract(Player player)
	{
		StartCoroutine(AddMoney(player));
	}

	IEnumerator AddMoney(Player player)
	{
		if (m_applying)
		{
			yield break;
		}

		m_applying = true;
		var task = new MetagameTask<InstanceResponse<User>>();
		var changeRequest = new ChangeRequest("grantCurrencyInsecure", new { currency = Value });
		yield return StartCoroutine(Collection.Users.ApplyChange(task, player.User.ID, changeRequest));
		m_applying = false;

		if (task.Error == null)
		{
			player.LoadProfile(task.Data.Instance);
			gameObject.SetActive(false);
		}
	}
}