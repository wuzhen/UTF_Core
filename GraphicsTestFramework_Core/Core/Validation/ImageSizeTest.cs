using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ImageSizeTest : MonoBehaviour {


	Texture2D tex;
	Camera cam;
	bool printed = false;

	// Use this for initialization
	IEnumerator Start () {
		cam = GetComponent<Camera> ();

		RenderTexture rt = new RenderTexture (cam.pixelWidth, cam.pixelHeight, 24);

		//cam.targetTexture = rt;

		Graphics.SetRenderTarget (rt);

		yield return new WaitForEndOfFrame ();

		tex = new Texture2D (cam.pixelWidth, cam.pixelHeight, TextureFormat.RGB24, false);

		tex.ReadPixels (new Rect (0, 0, cam.pixelWidth, cam.pixelHeight), 0, 0);
		tex.Apply();
		//cam.targetTexture = null;
		Graphics.SetRenderTarget (null);
	}
	
	// Update is called once per frame
	void Update () {

		if(tex != null && printed == false){
			printed = true;
			Debug.LogWarning (tex.width + " " + tex.height);
			string imgStr = System.Convert.ToBase64String (tex.EncodeToPNG ());



			//Debug.Log (imgStr);
			Debug.Log ("String Image=" + imgStr.Length + " Characters");
			byte[] imgByteArry = tex.EncodeToPNG ();
			string imgByteString = "";
			foreach(byte b in imgByteArry){
				imgByteString += b.ToString () + "|";
			}
			//Debug.Log (imgByteString);
			Debug.Log ("Byte Image=" + imgByteString.Length + " Characters");

			byte[] bytes = tex.EncodeToPNG ();
			File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);

		}


	}
}
