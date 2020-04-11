## Sand Unity

Sand Unity is a Unity3D client for [Sand](https://github.com/ccadori/sand-socket), built as a UPM (Unity Package Manager).

## Installing

As long as Sand Unity is a UPM, all you need to do is to add a package to your project with this repo url.

## Usage

Using Sand Unity is very easy, add a **Sand.Client** component to any **GameObject** in your scene, and call the 
method **Connect** whenever you want to start your connection with the server.

```C#
public Sand.Client client;

public void Start() {
  // Starting the server
  client.Connect();
  
  // Adding listeners
  // Connected is called when the client has completed the handshake with the server
  client.Emitter.On("connected", OnConnected);
  // Listening to chat messages
  client.Emitter.On("chat", OnChat);
}

public void OnConnected(string data) {
  Debug.Log("Connected");
}

public void OnChat(string data) {
  Debug.Log("Received data: " + data);
}

```
