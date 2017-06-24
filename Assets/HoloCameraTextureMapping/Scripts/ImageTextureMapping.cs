using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HoloCameraTextureMapping
{
    public class ImageTextureMapping : MonoBehaviour
    {
        private ComputeBuffer buffer;
        private ComputeBuffer textureIndexBuffer;

        void Start()
        {
        }

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

        public void ApplyTextureMapping(List<Matrix4x4> worldToCameraMatrixList, List<Matrix4x4> projectionMatrixList, Texture2DArray textureArray)
        {
            var mesh = GetComponent<MeshFilter>().mesh;

            var vertices = mesh.vertices;
            var triangles = mesh.triangles;

            //var uvArray = new float[vertices.Length * 2];
            var uvArray = new float[vertices.Length * 2 * (worldToCameraMatrixList.Count+1)];
            var textureIndexArray = new int[vertices.Length];

            var index = 0;

            //Debug.Log(vertices.Length);
            //Vector2 uv = Vector2.one;
            //var textureIndex = 0;
            foreach (var v in vertices)
            {
                //Vector2 uv = -Vector2.one;
                //Vector2 uv = Vector2.zero;
                Vector2 uv = Vector2.one;
                var textureIndex = 0;
                var score = 0f;
                /* set default texture */
                uvArray[2 * index * (worldToCameraMatrixList.Count + 1)] = -1f;
                uvArray[2 * index * (worldToCameraMatrixList.Count + 1) + 1] = -1f;

                for (int i = 0; i < worldToCameraMatrixList.Count; i++)
                {
                    //Debug.Log(i);
                    //set default value
                    uvArray[2 * index * (worldToCameraMatrixList.Count + 1) + 2 * (i+1)] = -1f;
                    uvArray[2 * index * (worldToCameraMatrixList.Count + 1) + 2 * (i+1) + 1] = -1f;

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
                    //var projectionUV = new Vector2(projectionPosition.x/ projectionPosition.z, projectionPosition.y/ projectionPosition.z);
                    var projectionUV = new Vector2(projectionPosition.x , projectionPosition.y );
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
                    if (Mathf.Abs(projectionUV.x) <= 2.0f && Mathf.Abs(projectionUV.y) <= 2.0f)
                    {
                        uv = 0.5f * projectionUV + 0.5f * Vector2.one;

                        if ((uv.x < (1024.0 / 1280.0)) && (uv.y > (208.0 / 720.0)))
                        {
                            uv.x = uv.x * (1280.0f / 1024.0f);
                            uv.y = uv.y * (720.0f / 512.0f) - (208.0f / 512.0f);

                            var newScore = 10 - Mathf.Abs(uv.x - 0.5f) - Mathf.Abs(uv.y - 0.5f);
                            if (score <= newScore)
                            {
                                score = newScore;
                                textureIndex = i + 1;
                            }
                            
                            uvArray[2 * index * (worldToCameraMatrixList.Count + 1) + 2 * (i+1)] = uv.x;
                            uvArray[2 * index * (worldToCameraMatrixList.Count + 1) + 2 * (i+1) + 1] = uv.y;
                            continue;
                            //break;
                        }
                        else
                        {
                            uv = 0.5f * projectionUV + 0.5f * Vector2.one;
                            uv.x = uv.x * (1280.0f / 1024.0f);
                            uv.y = uv.y * (720.0f / 512.0f) - (208.0f / 512.0f);
                            uvArray[2 * index * (worldToCameraMatrixList.Count + 1) + 2 * (i+1)] = uv.x;
                            uvArray[2 * index * (worldToCameraMatrixList.Count + 1) + 2 * (i+1) + 1] = uv.y;

                            //uv.x = uv.x * (1280.0f / 1024.0f);
                            //uv.y = uv.y * (720.0f / 512.0f) - (208.0f / 512.0f);
                            continue;
                        }
                    }
                    else
                    {
                        // Debug.Log("uv");
                        //Debug.Log(uv);
                        //uv
                        continue;
                        //uv.x = 0.0f;
                        //uv.y = 0.0f;
                        //uv.x = Mathf.Clamp01(projectionUV.x);
                        //uv.y = Mathf.Clamp01(projectionUV.y);
                   }
                    //break;
                }
                
                /*
                uvArray[2 * index] = uv.x;
                uvArray[2 * index + 1] = uv.y;
                */

                textureIndexArray[index] = textureIndex;

                if(textureIndex == 1)
                {
                    //Debug.Log("hoge");
                }

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
            for (int i = 0; i < textureIndexArray.Length; i++)
            {
                if (textureIndexArray[i] != 0)
                {
                    //Debug.Log(uvArray[2 * i * (worldToCameraMatrixList.Count + 1) + 2 * textureIndexArray[i] + 1]);
                    //Debug.Log("texture: " + textureIndexArray[i]);
                    break;
                }
            }
            //return;
            //Debug.Log(uvArray[0]);

            var material = GetComponent<Renderer>().material;

            if (vertices.Length == 0)
            {
                return;
            }
            if (buffer != null)
            {
                buffer.Release();
            }
            buffer = new ComputeBuffer(vertices.Length * (worldToCameraMatrixList.Count+1), sizeof(float) * 2);
            buffer.SetData(uvArray);
            material.SetBuffer("_UVArray", buffer);

            if (textureIndexBuffer != null)
            {
                textureIndexBuffer.Release();
            }
            textureIndexBuffer = new ComputeBuffer(vertices.Length, sizeof(int));
            textureIndexBuffer.SetData(textureIndexArray);
            material.SetBuffer("_TextureIndexArray", textureIndexBuffer);

            material.SetInt("_TextureCount", worldToCameraMatrixList.Count + 1);
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