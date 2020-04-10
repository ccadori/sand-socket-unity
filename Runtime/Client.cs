using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;
using UnityEngine;

public struct Message
{
	public string eventName;
	public string message;

	public Message(string eventName, string message)
	{
		this.eventName = eventName;
		this.message = message;
	}
}

public class Client : MonoBehaviour
{
	public string ID { get; private set; }
	public EventEmitter Emitter { get; private set; }

	public float queueReadingRate = 0.1f;
	public string host = "localhost";
	public int port = 3000;
	public char[] eventDelimiter = ("#e#").ToCharArray();
	public char[] packetDelimiter = ("\n").ToCharArray();

	private TcpClient socketConnection;
	private Thread clientReceiveThread;
	private Reader reader;
	private List<Message> queuedMessages;
	private Coroutine queueReadingRoutine;

	private void Awake()
	{
		Emitter = new EventEmitter();
		Emitter.On("handshake", OnHandshake);
	}

	public void Connect()
	{
		try
		{
			reader = new Reader(OnReadLine, packetDelimiter);
			queuedMessages = new List<Message>();

			clientReceiveThread = new Thread(new ThreadStart(Reading));
			clientReceiveThread.IsBackground = true;
			clientReceiveThread.Start();

			queueReadingRoutine = StartCoroutine(ReadingQueuedMessages());
		}
		catch (Exception e)
		{
			Debug.LogError("On client connect exception " + e);
		}
	}
  
	private IEnumerator ReadingQueuedMessages()
	{
		while (true)
		{
			lock(queuedMessages)
			{
				if (queuedMessages.Count > 0) {
					foreach (Message message in queuedMessages)
					{
						Emitter.Emit(message.eventName, message.message);
					}
					queuedMessages.Clear();
				}
			}
			
			yield return new WaitForSeconds(queueReadingRate);
		}
	}

	private void Reading()
	{
		try
		{
			socketConnection = new TcpClient(host, port);
			byte[] bytes = new byte[1024];

			while (true)
			{
				using (NetworkStream stream = socketConnection.GetStream())
				{
					int dataLength;
					while ((dataLength = stream.Read(bytes, 0, bytes.Length)) != 0)
					{
						byte[] incommingData = new byte[dataLength];
						Array.Copy(bytes, 0, incommingData, 0, dataLength);
						reader.OnReceiveData(Encoding.ASCII.GetString(incommingData));
					}
				}
			}
		}
		catch (SocketException socketException)
		{
			Debug.LogError("Socket exception: " + socketException);
		}
	}

	private void OnHandshake(string data)
	{
		ID = data;
		Emitter.Emit("connected", "");
		Write("handshake", "");
	}

	private void OnReadLine(string data)
	{
		string[] splitedData = data.Split(eventDelimiter);
		
		lock (queuedMessages)
		{
			queuedMessages.Add(new Message(splitedData[0], splitedData[2]));
		}
	}

	public void WriteJSON(string eventName, object payload)
	{
		Write(eventName, JsonUtility.ToJson(payload));
	}

	public void Write(string eventName, string payload)
	{
		if (socketConnection == null)
		{
			return;
		}
		try
		{
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite)
			{
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(eventName + "@@" + payload + "\n");
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}

	private void OnDestroy()
	{
		clientReceiveThread.Abort();
		socketConnection.Close();
		StopCoroutine(queueReadingRoutine);
	}
}
