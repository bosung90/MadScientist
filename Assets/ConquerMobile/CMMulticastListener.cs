using UnityEngine;
using System.Collections;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class CMMulticastListener : MonoBehaviour
{
	public string station_id="";
	private int server_port = 5000;
	private string server_ip;
	
	// multicast
	private int startup_port = 5100;
	private IPAddress group_address = IPAddress.Parse ("224.0.0.224");
	private UdpClient udp_client;
	private IPEndPoint remote_end;
	private NetworkConnectionError init_status=NetworkConnectionError.NoError;
	
	void Start ()
	{
		// loaded elsewhere
		if (station_id == "GameServer")
			StartGameServer ();
		else
			StartGameClient ();
	}
	
	void StartGameServer ()
	{
		// the Unity3d way to become a server
		init_status = Network.InitializeServer (10, server_port, false);
		Debug.Log ("status: " + init_status);
		
		StartBroadcast ();
	}
	
	void StartGameClient ()
	{
		// multicast receive setup
		remote_end = new IPEndPoint (IPAddress.Any, startup_port);
		udp_client = new UdpClient (remote_end);
		udp_client.JoinMulticastGroup (group_address);
		
		// async callback for multicast
		udp_client.BeginReceive (new AsyncCallback (ServerLookup), null);
		
		MakeConnection ();
	}
	
	IEnumerator MakeConnection ()
	{
		// continues after we get server's address
		while (server_ip.Length==0)
			yield return null;
		
		while (Network.peerType == NetworkPeerType.Disconnected)
		{
			Debug.Log ("connecting: " + server_ip +":"+ server_port);
			
			// the Unity3d way to connect to a server
			NetworkConnectionError error;
			error = Network.Connect (server_ip, server_port);
			
			Debug.Log ("status: " + error);
			yield return new WaitForSeconds (1);
		}
	}
	
	/******* broadcast functions *******/
	void ServerLookup (IAsyncResult ar)
	{
		// receivers package and identifies IP
		byte[] receiveBytes = udp_client.EndReceive (ar, ref remote_end);
		
		server_ip = remote_end.Address.ToString ();
		Debug.Log ("Server: " + server_ip);
	}
	
	IEnumerator StartBroadcast ()
	{
		// multicast send setup
		udp_client = new UdpClient ();
		udp_client.JoinMulticastGroup (group_address);
		remote_end = new IPEndPoint (group_address, startup_port);
		
		// sends multicast
		while (true)
		{
			byte[] buffer = Encoding.ASCII.GetBytes ("GameServer");
			udp_client.Send (buffer, buffer.Length, remote_end);
			
			yield return new WaitForSeconds (1);
		}
	}
}

