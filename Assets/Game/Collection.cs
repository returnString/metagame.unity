using Metagame;
using Metagame.State;
using SampleGame.Users;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class Collection<TInstance, TAdvertisedData>
	where TInstance : MetagameInstance
{
	public TAdvertisedData Data { get; private set; }

	private MetagameClient m_metagame;
	private string m_name;

	private Dictionary<string, TInstance> m_cachedInstances;

	public Collection(MetagameClient metagame, string name)
	{
		m_metagame = metagame;
		m_name = name;
		m_cachedInstances = new Dictionary<string, TInstance>();
	}

	public IEnumerator DownloadData()
	{
		var metaRef = new MetagameRef<AdvertisedResponse<TAdvertisedData>>();
		yield return m_metagame.StartCoroutine(m_metagame.GetAdvertisedData(metaRef, m_name));
		Data = metaRef.Data.Advertised;
	}

	private void CacheTask(MetagameRef<InstanceResponse<TInstance>> task, string id)
	{
		if (task.Error == null)
		{
			m_cachedInstances[id] = task.Data.Instance;
		}
	}

	public IEnumerator ApplyChange(MetagameRef<InstanceResponse<TInstance>> task, string id, params ChangeRequest[] changes)
	{
		yield return m_metagame.StartCoroutine(m_metagame.ModifyInstance(task, m_name, id, changes));
		CacheTask(task, id);
	}

	public IEnumerator DownloadInstance(MetagameRef<InstanceResponse<TInstance>> task, string id)
	{
		yield return m_metagame.StartCoroutine(m_metagame.GetInstance(task, m_name, id));
		CacheTask(task, id);
	}

	public TInstance GetInstance(string id, Func<string, TInstance> defaultInstanceCreator = null)
	{
		TInstance ret;
		if (!m_cachedInstances.TryGetValue(id, out ret) && defaultInstanceCreator != null)
		{
			ret = defaultInstanceCreator(id);
			ret.ID = id;
		}

		return ret;
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
