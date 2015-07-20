using UnityEngine;
using System.Collections;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class CMMulticastListener : MonoBehaviour
{
	private bool isFiring = false;
	private string fireURL = string.Empty;

	public int port = 5000;
	private string announce_url;

	// multicast
	private IPAddress group_address = IPAddress.Parse ("224.0.0.224");
	IPEndPoint remote_end;
	private UdpClient udp_client;

	void Start ()
	{
		StartListen ();
	}

	void Update() 
	{
		if(isFiring)
		{
			isFiring = false;
			BodyPartsCreator.Instance.CreateBodyPart (fireURL);
		}
	}
	
	void StartListen ()
	{ 
		// multicast receive setup
		remote_end = new IPEndPoint (IPAddress.Any, port);
		udp_client = new UdpClient (remote_end);
		udp_client.JoinMulticastGroup (group_address);
		
		// async callback for multicast
		udp_client.BeginReceive (new AsyncCallback (ReceiveAnnounceCallback), null);
	}

	void ReceiveAnnounceCallback (IAsyncResult ar)
	{
		// receivers package and identifies IP
		byte[] receiveBytes = udp_client.EndReceive (ar, ref remote_end);
		
		announce_url = Encoding.UTF8.GetString(receiveBytes, 0, receiveBytes.Length);
		Debug.Log ("Announce Received: " + announce_url);
		fireURL = announce_url;
		isFiring = true;
	}
}

