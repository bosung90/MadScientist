using UnityEngine;
using System.Collections;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class CMMulticastSender : MonoBehaviour
{
	public int port = 5000;
	public AsyncCallback callback = null;
	private string announce_url;
	
	// multicast
	private IPAddress group_address = IPAddress.Parse ("224.0.0.224");
	IPEndPoint remote_end;
	private UdpClient udp_client;

	void Update() {
		if( Input.GetMouseButton(0) )  {
			this.ExampleBroadcast();
		}
	}

	void ExampleBroadcast ()
	{
		this.broadcast("http://selfyscan.com/selfy/icasiihl-ykw7o5d9");
		Debug.Log("Sending broadcast");
	}

	void broadcast(string message) {
		// multicast send setup
		udp_client = new UdpClient ();
		udp_client.JoinMulticastGroup (group_address);
		remote_end = new IPEndPoint (group_address, port);

		byte[] buffer = Encoding.ASCII.GetBytes (message);
		udp_client.Send (buffer, buffer.Length, remote_end);
	}
}

