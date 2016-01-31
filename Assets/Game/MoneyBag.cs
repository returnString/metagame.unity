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
		var metaRef = new MetagameRef<InstanceResponse<User>>();
		var changeRequest = new ChangeRequest("grantCurrencyInsecure", new { currency = Value });
		yield return StartCoroutine(Collection.Users.ApplyChange(metaRef, player.User.ID, changeRequest));
		m_applying = false;

		if (metaRef.Error == null)
		{
			player.LoadProfile(metaRef.Data.Instance);
			gameObject.SetActive(false);
		}
	}
}