using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageTextureMapping : MonoBehaviour {
    public Texture2D SampleTexture;
	// Use this for initialization
	void Start () {
        var mesh = GetComponent<MeshFilter>().mesh;
        var vertices = mesh.vertices;

        var uvArray = new float[vertices.Length];
        var index = 0;
        foreach(var v in vertices)
        {
            Debug.Log(v.ToString());
            uvArray[index] = (float)index / vertices.Length;
            //TOOD
            index += 1;
        }

        var material = GetComponent<Renderer>().material;

        ComputeBuffer buffer = new ComputeBuffer(uvArray.Length, sizeof(float));
        buffer.SetData(uvArray);
        material.SetBuffer("_UVArray", buffer);

        //texture
        var width = 2048;
        var height = 1024;
        Debug.Log(SampleTexture.format);

        var textureArray = new Texture2DArray(width, height, 1, TextureFormat.DXT1, false);
        Graphics.CopyTexture(SampleTexture, 0, 0, textureArray, 0, 0);
        material.SetTexture("_TextureArray", textureArray);

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
