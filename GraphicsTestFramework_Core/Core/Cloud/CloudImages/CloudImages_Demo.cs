using UnityEngine;
using System.Collections;
using System;

public class CloudImages_Demo : MonoBehaviour
{
	Rect texturePos = new Rect(10, 10, 500, 500);
	Texture2D text2d;
	string cloudFileID = "";
	
	void OnEnable()
	{
		// Suscribe for catching cloud responses.
		CloudImagesConnector.Instance.responseCallback.AddListener(ParseData);
	}
	
	void OnDisable()
	{
		// Remove listeners.
		CloudImagesConnector.Instance.responseCallback.RemoveListener(ParseData);
	}
	
	protected void OnGUI()
	{
		if (GUI.Button(new Rect(550, 10, 150, 30), "Load PNG from File"))
		{
			LoadPNGFromFile(Application.dataPath + "/CloudImages/UFO.png");
		}
		
		GUI.enabled = text2d != null;
		if (GUI.Button(new Rect(550, 50, 150, 30), "Image to Cloud"))
		{
			CloudImagesConnector.PersistImage(text2d, "TextureFile");
		}
		
		GUI.enabled = true;
		
		if (GUI.Button(new Rect(550, 90, 150, 30), "Screenshot to Cloud"))
		{
			StartCoroutine(TakeScreenshot(false));
		}
		
		GUI.Label(new Rect(550, 140, 150, 25), "Google Drive file id:");
		cloudFileID = GUI.TextField(new Rect(550, 160, 150, 25), cloudFileID);
		
		GUI.enabled = !string.IsNullOrEmpty(cloudFileID);
		if (GUI.Button(new Rect(550, 190, 150, 30), "Get From Cloud"))
		{
			CloudImagesConnector.RequestImage(cloudFileID);
		}
		
		if (text2d == null)
		{
			return;
		}
		
		GUI.DrawTexture(texturePos, text2d);
	}
	
	void ParseData(string responseType, string response)
	{
		if(responseType == "IMAGE_")
		{
			byte[] decodedBytes = Convert.FromBase64String(response);
			Texture2D tex = new Texture2D(2, 2);
			tex.LoadImage(decodedBytes, false);
			text2d = tex;
		}
		
		if (responseType == "IMAGE_SAVED")
		{
			// Do anything you need with the returning Drive file id.
		}
	}
	
	IEnumerator TakeScreenshot(bool usePNG = true)
	{
		yield return new WaitForEndOfFrame();

		Texture2D texture = new Texture2D(Screen.width, Screen.height);
		texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		texture.Apply();
		
		CloudImagesConnector.PersistImage(texture, "Screenshot");
		
		DestroyObject(texture);
	}
	
	public void LoadPNGFromFile(string filePath)
	{
		byte[] fileData;
		if (System.IO.File.Exists(filePath))
		{
			fileData = System.IO.File.ReadAllBytes(filePath);
			text2d = new Texture2D(2, 2);
			text2d.LoadImage(fileData);
		}
 }
}
