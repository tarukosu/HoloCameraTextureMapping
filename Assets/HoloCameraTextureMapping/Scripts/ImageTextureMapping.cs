using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageTextureMapping : MonoBehaviour {
    public Texture2D SampleTexture;
    private ComputeBuffer buffer;

    // Use this for initialization
    void Start () {
        var mesh = GetComponent<MeshFilter>().mesh;
        var vertices = mesh.vertices;

        var uvArray = new float[vertices.Length * 2];
        var index = 0;
        foreach(var v in vertices)
        {
            //TOOD
            Debug.Log(v.ToString());

            //uvArray[index] = (float)index / (vertices.Length * 2);
            uvArray[index] = (float)((v.x + 5.0) / 10.0);
            Debug.Log(uvArray[index]);
            index += 1;
            uvArray[index] = (float)((v.z + 5.0) / 10.0);
            //uvArray[index] = (float)index / (vertices.Length * 2);
            Debug.Log(uvArray[index]);
            index += 1;
        }
        //Debug.Log(uvArray[0]);

        var material = GetComponent<Renderer>().material;

        buffer = new ComputeBuffer(vertices.Length, sizeof(float) * 2);
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

    private void OnDestroy()
    {
        if (buffer != null)
        {
            buffer.Release();
        }
    }
}
