using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HoloCameraTextureMapping
{
    public class ImageTextureMapping : MonoBehaviour
    {
        public Texture2D SampleTexture;
        private ComputeBuffer buffer;
        private ComputeBuffer textureIndexBuffer;

        /*
        private List<Matrix4x4> worldToCameraMatrixList = new List<Matrix4x4>();
        private List<Matrix4x4> projectionMatrixList = new List<Matrix4x4>();
        private List<Texture2D> textureList = new List<Texture2D>();
        */

        // Use this for initialization
        void Start()
        {
            //UpdateTexture();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDestroy()
        {
            if (buffer != null)
            {
                buffer.Release();
            }
            if (textureIndexBuffer != null)
            {
                textureIndexBuffer.Release();
            }

        }

        //public void AddTextureMapping(Matrix4x4 worldToCameraMatrix, Matrix4x4 projectionMatrix, Texture2D texture)
        public void ApplyTextureMapping(List<Matrix4x4> worldToCameraMatrixList, List<Matrix4x4> projectionMatrixList, Texture2DArray textureArray)
        {
            /*
            worldToCameraMatrixList.Add(worldToCameraMatrix);
            projectionMatrixList.Add(projectionMatrix);
            textureList.Add(texture);
            */
            var mesh = GetComponent<MeshFilter>().mesh;
            var vertices = mesh.vertices;

            var uvArray = new float[vertices.Length * 2];
            var textureIndexArray = new int[vertices.Length];

            var index = 0;

            Debug.Log(vertices.Length);

            foreach (var v in vertices)
            {
                //Vector2 uv = -Vector2.one;
                Vector2 uv = Vector2.zero;
                var textureIndex = 0;

                for (int i = 0; i < worldToCameraMatrixList.Count; i++)
                {
                    var position = transform.TransformPoint(new Vector3(v.x, v.y, v.z));
                    //var position = new Vector4(v.x, v.y, v.z, 1.0f);

                    var w2c = worldToCameraMatrixList[i];
                    var pm = projectionMatrixList[i];
                    var cameraPosition = w2c.MultiplyPoint(position);

                    if(cameraPosition.z >= 0)
                    {
                        continue;
                    }

                    var projectionPosition = pm.MultiplyPoint(cameraPosition);
                    var projectionUV = new Vector2(projectionPosition.x, projectionPosition.y);
                    //Debug.Log(position);
                    //Debug.Log(projectionUV);
                    /*
                    if (projectionPosition.z <= 0)
                    {
                        continue;
                    }
                    */
                    if (float.IsNaN(projectionUV.x) || float.IsNaN(projectionUV.y))
                    {
                        continue;
                    }
                    if (Mathf.Abs(projectionUV.x) <= 1.0f && Mathf.Abs(projectionUV.y) <= 1.0f)
                    {
                        uv = 0.5f * projectionUV + 0.5f * Vector2.one;
                        textureIndex = i;

                        if ((uv.x < (1024.0 / 1280.0)) && (uv.y > (208.0 / 720.0)))
                        {
                            uv.x = uv.x * (1280.0f / 1024.0f);
                            uv.y = uv.y * (720.0f / 512.0f) - (208.0f / 512.0f);
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                        uv.x = projectionUV.x;
                        uv.y = projectionUV.y;
                    }
                    //break;
                }
                //Debug.Log("uv");
                //Debug.Log(uv);
                uvArray[2 * index] = uv.x;
                //index += 1;
                uvArray[2 * index + 1] = uv.y;


                textureIndexArray[index] = textureIndex;

                index += 1;

                /* test
                //uvArray[index] = (float)index / (vertices.Length * 2);
                uvArray[index] = (float)((v.x + 5.0) / 10.0);
                //Debug.Log(uvArray[index]);
                index += 1;
                uvArray[index] = (float)((v.z + 5.0) / 10.0);
                //uvArray[index] = (float)index / (vertices.Length * 2);
                //Debug.Log(uvArray[index]);
                index += 1;
                */
            }
            if (uvArray.Length > 0)
            {
                Debug.Log(uvArray[0]);
                Debug.Log("texture: " + textureIndexArray[0]);
            }
            //return;
            //Debug.Log(uvArray[0]);

            var material = GetComponent<Renderer>().material;

            if (vertices.Length == 0)
            {
                return;
            }
            buffer = new ComputeBuffer(vertices.Length, sizeof(float) * 2);
            buffer.SetData(uvArray);
            material.SetBuffer("_UVArray", buffer);

            textureIndexBuffer = new ComputeBuffer(vertices.Length, sizeof(int));
            textureIndexBuffer.SetData(textureIndexArray);
            material.SetBuffer("_TextureIndexArray", textureIndexBuffer);

            
            //set texture
            /*
            var width = texture.width;
            //2048;
            var height = texture.height;//1024;

            Debug.Log(SampleTexture.format);

            var textureArray = new Texture2DArray(width, height, 1, TextureFormat.DXT5, false);
            //Graphics.CopyTexture(SampleTexture, 0, 0, textureArray, 0, 0);
            Graphics.CopyTexture(texture, 0, 0, textureArray, 0, 0);
            */
            material.SetTexture("_TextureArray", textureArray);
        }
    }
}