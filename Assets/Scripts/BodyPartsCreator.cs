using UnityEngine;
using System.Collections;
using System.Net;
using SimpleJSON;

public class BodyPartsCreator : MonoBehaviour {

	string json_data;
	Vector3[] Vertex;
	Vector2[] UV_MaterialDisplay;
	int[] Triangles;
	bool load_mesh = false;

	bool loading = false;

	private string jpgFilePath = "Assets/Resources/eric_selfy2.jpg";

	public GameObject BodyPartTemplate;

	//Singleton
	private BodyPartsCreator() { }
	private static object syncRoot = new Object();
	private static volatile BodyPartsCreator instance;
	public static BodyPartsCreator Instance
	{
		get
		{
			if (instance == null)
			{
				lock (syncRoot)
				{
					if (instance == null)
						instance = GameObject.Find("BodyPartsCreator").GetComponent<BodyPartsCreator>();
				}
			}
			
			return instance;
		}
	}

	public void CreateBodyPart(string url)
	{
//		url = "http://selfyscan.com/selfy/icasiihl-ykw7o5d9";

		if(loading)
		{
			Debug.Log("Sorry we are already creating");
			return;
		}
		else
		{
			loading = true;
			//read from url, get meshUrl, get jpgUrl
			string json_string = new WebClient ().DownloadString(url);
			JSONNode parseJson = JSON.Parse (json_string);

			var base_url = "http://selfyscan.com";
			var geometryUrl = base_url+parseJson["geometry"].Value;
			var textureUrl = base_url+parseJson["texture"].Value;
		
			CreateBodyPart(new System.Uri(geometryUrl), new System.Uri(textureUrl));
		}
	}

	private void CreateBodyPart(System.Uri meshUrl, System.Uri jpgUrl)
	{
		//download jpg and save into a jpg file.
		Debug.Log ("trying to load " + meshUrl.AbsoluteUri);
		WebClient webClient = new WebClient ();
		
		webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownCompleted);
		webClient.DownloadStringAsync (meshUrl);

//		Debug.Log ("trying to load " + jpgUrl.AbsoluteUri);
//		WebClient webClient2 = new WebClient ();
//		
//		webClient2.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler (JpgDownCompleted);
//		webClient2.DownloadFileAsync(jpgUrl, jpgFilePath);
	}

	void JpgDownCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
	{
		Debug.Log (e.Error.Message);


	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(load_mesh)
		{
			load_mesh = false;
			LoadMesh(Vertex, UV_MaterialDisplay, Triangles);
		}
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

	private void LoadMesh(Vector3[] vertices, Vector2[] uvs, int[] triangles)
	{
		GameObject newTemplate = Instantiate (BodyPartTemplate) as GameObject;

		Mesh mesh = newTemplate.GetComponent<MeshFilter> ().mesh;
		Material material = new Material(newTemplate.GetComponent<Renderer> ().material);

		var texture = CreateTextureFromPng ("Assets/Resources/eric_selfy.jpg");
		material.SetTexture ("_MainTex", texture );
		newTemplate.GetComponent<Renderer> ().material = material;
		
		mesh.name = "MyOwnObject";
		
		mesh.vertices = vertices; //Just do this... Use Logic...
		mesh.triangles = triangles;
		mesh.uv = uvs;
		
		mesh.RecalculateNormals ();
		mesh.Optimize ();

		loading = false;
	}

	private void LoadJsonFromFile(string fileName)
	{
		Debug.Log ("trying to load " + fileName);
		var textAsset = Resources.Load(fileName) as TextAsset;
		this.json_data = textAsset.text;
		DownCompleted (null, null);
	}

//	private void LoadJsonAndJpg(string jsonUrl, string jpg)
//	{
//		Debug.Log ("trying to load " + url);
//		WebClient webClient = new WebClient ();
//		
//		webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownCompleted);
//		webClient.DownloadStringAsync (new System.Uri(url));
//	}

	private Texture2D CreateTextureFromPng( string filename )
	{
		Texture2D newTexture = new Texture2D (0, 0);
		
		byte[] pngData = System.IO.File.ReadAllBytes (filename);
		newTexture.LoadImage ( pngData );
		
		return newTexture;
	}
}
