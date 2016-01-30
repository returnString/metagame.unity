using Metagame.Utils;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using WebSocketSharp;

namespace Metagame
{
	public class ConnectResponse
	{
	}

	public class MetagameClient : MonoBehaviour
	{
		private WebSocket m_socket;
		private bool m_connected;
		private string m_connectError;

		private Dictionary<string, string> m_responses;
		private ReaderWriterLockSlim m_responseLock;

		private void Log(string format, params object[] args)
		{
			var message = string.Format(format, args);
			var currentThread = Thread.CurrentThread;
			Debug.LogFormat("[{0}] {1}", currentThread.ManagedThreadId, message);
		}

		void Awake()
		{
			m_responses = new Dictionary<string, string>();
			m_responseLock = new ReaderWriterLockSlim();
		}

		public IEnumerator Connect(IMetagameTask<ConnectResponse> task, string url)
		{
			task.Reset();

			if (m_socket != null)
			{
				Log("Closing existing metagame socket");

				m_socket.CloseAsync();

				while (m_socket.IsAlive)
				{
					yield return null;
				}
			}

			m_socket = new WebSocket(url);
			m_connectError = null;
			m_connected = false;

			// TODO: include game-embedded CAs
			m_socket.ServerCertificateValidationCallback = (s, cert, chain, errors) => false;

			m_socket.OnOpen += (s, e) =>
			{
				m_connected = true;
				Log("Metagame connection established");
			};

			m_socket.OnError += (s, e) =>
			{
				m_connectError = e.Message;
				Log("Metagame connection failed: {0}", m_connectError);
			};

			var correlationExample = new { correlation = "" };
			m_socket.OnMessage += (s, e) =>
			{
				var temp = JsonConvert.DeserializeAnonymousType(e.Data, correlationExample);
				Log("Got correlation {0}, {1} chars: {2}", temp.correlation, e.Data.Length, e.Data);

				using (m_responseLock.Write())
				{
					m_responses[temp.correlation] = e.Data;
				}
			};

			Log("Connecting to {0}", url);
			m_socket.ConnectAsync();

			while (!m_connected && m_connectError == null)
			{
				yield return null;
			}

			if (m_connectError != null)
			{
				task.OnClientError(MetagameClientError.NotConnected);
			}
			else
			{
				task.OnResponse(new MetagameResponse<ConnectResponse> { Data = new ConnectResponse() });
			}
		}

		public IEnumerator Send<TData, TRequestData>(IMetagameTask<TData> task, string path, TRequestData @params)
		{
			task.Reset();

			if (!m_socket.IsAlive)
			{
				task.OnClientError(MetagameClientError.NotConnected);
				yield break;
			}

			var correlation = Guid.NewGuid().ToString("N");

			var obj = new
			{
				path,
				@params,
				correlation,
			};

			var json = JsonConvert.SerializeObject(obj);

			Log("Sending {0}, {1} chars: {2}", correlation, json.Length, json);

			var done = false;
			var writeSucceeded = false;

			m_socket.SendAsync(json, ok =>
			{
				done = true;
				writeSucceeded = ok;
			});

			// FIXME: is this read allowed to be cached in release? find nice sync primitive to replace
			while (!done)
			{
				yield return null;
			}

			if (!writeSucceeded)
			{
				task.OnClientError(MetagameClientError.SendFailed);
				yield break;
			}

			// TODO: Client timeout on correlated receive
			while (true)
			{
				string response;
				using (m_responseLock.Read())
				{
					m_responses.TryGetValue(correlation, out response);
				}

				if (response == null)
				{
					yield return null;
				}
				else
				{
					using (m_responseLock.Write())
					{
						m_responses.Remove(correlation);
					}

					task.OnResponse(JsonConvert.DeserializeObject<MetagameResponse<TData>>(response));
					break;
				}
			}
		}
	}
}
