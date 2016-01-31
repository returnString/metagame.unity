using Metagame;
using Metagame.State;
using SampleGame.Users;
using System;
using System.Collections;
using System.Reflection;

public class Collection<TInstance, TAdvertisedData>
	where TInstance : MetagameInstance
{
	public TAdvertisedData Data { get; private set; }

	private MetagameClient m_metagame;
	private string m_name;

	public Collection(MetagameClient metagame, string name)
	{
		m_metagame = metagame;
		m_name = name;
	}

	public IEnumerator DownloadData()
	{
		var metaRef = new MetagameRef<AdvertisedResponse<TAdvertisedData>>();
		yield return m_metagame.StartCoroutine(m_metagame.GetAdvertisedData(metaRef, m_name));
		Data = metaRef.Data.Advertised;
	}

	public IEnumerator ApplyChange(IMetagameTask<InstanceResponse<TInstance>> task, string id, params ChangeRequest[] changes)
	{
		yield return m_metagame.StartCoroutine(m_metagame.ModifyInstance(task, m_name, id, changes));
	}
}

public static class Collection
{
	public static Collection<User, AdvertisedUserData> Users { get; private set; }

	const BindingFlags InvokeFlags = BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance;
	const BindingFlags PropertyFlags = BindingFlags.Static | BindingFlags.Public;

	public static IEnumerator Init(MetagameClient metagame)
	{
		var props = typeof(Collection).GetProperties(PropertyFlags);
		foreach (var prop in props)
		{
			var instance = Activator.CreateInstance(prop.PropertyType, metagame, prop.Name.ToLower());
			var enumerator = (IEnumerator)prop.PropertyType.InvokeMember("DownloadData", InvokeFlags, null, instance, new object[0]);
			yield return metagame.StartCoroutine(enumerator);
			prop.SetValue(null, instance, null);
		}
	}
}
