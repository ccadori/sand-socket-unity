[![openupm](https://img.shields.io/npm/v/com.ccadori.sandsocketunity?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.ccadori.sandsocketunity/)

## Sand Unity

Sand Unity is a Unity3D client for [Sand](https://github.com/ccadori/sand-socket), built as a UPM (Unity Package Manager).

## Installing

As long as Sand Unity is a UPM, all you need to do is to add a package to your project with this repo url (dont forget the .git extension).

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

## Examples

For full examples with server and client please visit [this repository](https://github.com/ccadori/sand-socket-examples).

## Delimiters

If you changed the package or event delimiter in the server, you'll need to change it in the client too.

```C#
public Sand.Client client;

public void Start() {
  // Starting the server
  client.Connect();
  client.SetDelimiters("packetDelimiter", "eventDelimiter");
}
```

## Using TLS

You can easely use TLS in your client by defining the Client's property "useTLS" as true.

```C#
public Sand.Client client;

public void Start() {
  // Starting the server
  client.useTLS = true;
  client.Connect();
}
```

PS: If you are using a self-signed certificate in your server, you'll have to define the Client's property "validateCert" as false, because Unity by default prevents this type of authentication triggering a cert error.

```C#
public Sand.Client client;

public void Start() {
  // Starting the server
  client.useTLS = true;
  client.validateCert = false;
  client.Connect();
}
```

## Roadmap

- Use bytes instead of text as data format.
