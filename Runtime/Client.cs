using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.IO;

namespace Sand
{
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
		public bool useTLS = false;
		public bool leaveInnerStreamOpen = false;
		public bool validateCert = true;
		private string eventDelimiter = "#e#";
		private string packetDelimiter = "\n";

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

		public void Disconnect()
        	{
			try
			{
				if (queueReadingRoutine != null)
					StopCoroutine(queueReadingRoutine);

				if (clientReceiveThread != null)
					clientReceiveThread.Abort();

				if (socketConnection != null)
					socketConnection.Close();
			}
			catch (Exception ex)
            		{
				Debug.LogError("Disconnecting Client Failed.");
            		}
        	}

		private IEnumerator ReadingQueuedMessages()
		{
			while (true)
			{
				lock (queuedMessages)
				{
					if (queuedMessages.Count > 0)
					{
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

				Stream stream = useTLS ? GetSSLStream(socketConnection) : GetStream(socketConnection);

				if (stream == null) return;

				while (true)
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
			catch (SocketException socketException)
			{
				Debug.LogError("Socket exception: " + socketException);
			}
		}

		private Stream GetStream(TcpClient client)
		{
			return client.GetStream();
		}

		private Stream GetSSLStream(TcpClient client)
		{
			SslStream stream = new SslStream(
				socketConnection.GetStream(),
				leaveInnerStreamOpen,
				new RemoteCertificateValidationCallback(ValidateServerCertificate),
				null
			);

			try
			{
				stream.AuthenticateAsClient(host);

				return stream;
			}
			catch (AuthenticationException e)
			{
				Console.WriteLine("Exception: {0}", e.Message);

				if (e.InnerException != null)
				{
					Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
				}

				Console.WriteLine("Authentication failed - closing the connection.");
				
				client.Close();

				return null;
			}
		}

		public bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (!validateCert) 
				return true;
			
			if (sslPolicyErrors == SslPolicyErrors.None)
				return true;

			return false;
		}

		private void OnHandshake(string data)
		{
			ID = data;
			Emitter.Emit("connected", "");
			Write("handshake", "");
		}

		private void OnReadLine(string data)
		{
			string[] splitedData = Regex.Split(data, eventDelimiter);

			lock (queuedMessages)
			{
				queuedMessages.Add(new Message(splitedData[0], splitedData[1]));
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
					byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(eventName + eventDelimiter + payload + packetDelimiter);
					stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				}
			}
			catch (SocketException socketException)
			{
				Debug.Log("Socket exception: " + socketException);
			}
		}

		public void SetDelimiters(string packetDelimiter, string eventDelimiter)
		{
			this.packetDelimiter = packetDelimiter;
			this.eventDelimiter = eventDelimiter;
			reader.delimiter = packetDelimiter;
		}

		private void OnDestroy()
		{
			if (clientReceiveThread != null)
				clientReceiveThread.Abort();
			
			if (socketConnection != null)
				socketConnection.Close();

			if (queueReadingRoutine != null)
				StopCoroutine(queueReadingRoutine);
		}
	}
}
