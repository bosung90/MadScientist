using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using SimpleJSON;

public class CreateSampleMesh : MonoBehaviour {

	public string mesh_data_url;

	string json_data;

	Vector3[] Vertex;
	Vector2[] UV_MaterialDisplay;
	int[] Triangles;

	bool load_mesh = false;

	Texture2D	CreateTextureFromPng( string filename )
	{
		Texture2D newTexture = new Texture2D (0, 0);

		byte[] pngData = System.IO.File.ReadAllBytes (filename);
		newTexture.LoadImage ( pngData );

		return newTexture;
	}

	public void LoadJsonString(string url)
	{
		Debug.Log ("trying to load " + url);
		WebClient webClient = new WebClient ();

		webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownCompleted);
		webClient.DownloadStringAsync (new System.Uri(url));
	}

	public void LoadJsonFromFile(string fileName)
	{
		Debug.Log ("trying to load " + fileName);
		var textAsset = Resources.Load(fileName) as TextAsset;
		this.json_data = textAsset.text;
		DownCompleted (null, null);
	}

	void Update () 
	{
		if(load_mesh)
		{
			load_mesh = false;
			LoadMesh(Vertex, UV_MaterialDisplay, Triangles);
		}

		Input.GetMouseButton (0);
	}

	void LoadMesh(Vector3[] vertices, Vector2[] uvs, int[] triangles)
	{
		Mesh mesh = GetComponent<MeshFilter> ().mesh;
		Material material = new Material(GetComponent<Renderer> ().material);

		var texture = CreateTextureFromPng ("Assets/Resources/eric_selfy.jpg");
		material.SetTexture ("_MainTex", texture );
		GetComponent<Renderer> ().material = material;

		mesh.name = "MyOwnObject";
		
		mesh.vertices = vertices; //Just do this... Use Logic...
		mesh.triangles = triangles;
		mesh.uv = uvs;
		
		mesh.RecalculateNormals ();
		mesh.Optimize ();
	}

	void DownCompleted(object sender, DownloadStringCompletedEventArgs e)
	{
		if(e!=null)
			this.json_data = e.Result;

		JSONNode parseJson = JSON.Parse (json_data);
		
		var verticesObjArr = parseJson["vertices"].AsArray;
		var uvObjArr = parseJson["uv"].AsArray;
		var facesObjArr = parseJson["faces"].AsArray;
		
		float[] verticesArr = new float[verticesObjArr.Count];
		float[] uvArr = new float[uvObjArr.Count];
		int[] facesArr = new int[facesObjArr.Count];
		
		for(int v = 0; v<verticesArr.Length; v++)
		{
			verticesArr[v] = verticesObjArr[v].AsFloat;
		}
		for(int u = 0; u<uvArr.Length; u++)
		{
			uvArr[u] = uvObjArr[u].AsFloat;
		}
		for(int f = 0; f<facesArr.Length; f++)
		{
			facesArr[f] = facesObjArr[f].AsInt;
		}
		
		int numVertices = verticesArr.Length / 3;
		Vector3[] Vertex = new Vector3[numVertices];
		
		for(int v=0; v<numVertices; v++)
		{
			Vertex[v] = new Vector3(verticesArr[0+ v*3], verticesArr[1 + v*3], verticesArr[2+ v*3]);
		}
		
		int numUVs = uvArr.Length / 2;
		
		Vector2[] UV_MaterialDisplay = new Vector2[numUVs];
		
		for(int u=0; u<numUVs; u++)
		{
			UV_MaterialDisplay[u] = new Vector2(uvArr[0+ u*2], uvArr[1 + u*2]);
		}
		
		int[] Triangles = facesArr;
		this.Vertex = Vertex;
		this.UV_MaterialDisplay = UV_MaterialDisplay;
		this.Triangles = Triangles;
		load_mesh = true;
	}
	void Start () {
		if(mesh_data_url.StartsWith("http"))
			LoadJsonString (mesh_data_url);
		else
			LoadJsonFromFile (mesh_data_url);
	}
}