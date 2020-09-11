using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
		public int chunkSize = 100;
		public int cellSize = 1;

		public float octaves = 5;
		public float amplitude = 10;
		public float frequency = 0.05f;
		public float roughness = 2;
		public float persistence = 0.5f;
		public float warpAmplitude = 10;
		public float warpFrequency = 0.04f;
		public float terraceHeight = 1;

		MeshFilter meshFilter;
		MeshCollider meshCollider;
		Vector3[] vertices;
		int[] triangles;
		Vector2[] uvs;


		void Start()
		{
				meshFilter = GetComponent<MeshFilter>();
				meshCollider = GetComponent<MeshCollider>();

				vertices = new Vector3[(chunkSize+1) * (chunkSize+1)];
				triangles = new int[chunkSize * chunkSize * 6];
				uvs = new Vector2[(chunkSize+1) * (chunkSize+1)];

				CreateMeshData();
				BuildMesh();

				transform.Translate(-(chunkSize * cellSize)/2, 0, -(chunkSize * cellSize)/2);
		}


		float getDensity(float x, float y)
		{
				float density = 0;

				float f = frequency;
				float a = amplitude;
				//terracing: density += (-yVal + val + yVal % terraceHeight) + floor;

				for (var i=0; i<octaves; i++)
				{
						//warping world coords
						float warp = Mathf.PerlinNoise(x*warpFrequency, y*warpFrequency);
						x += warp * warpAmplitude;
						y += warp * warpAmplitude;

						//setting density
						float val = Mathf.PerlinNoise(x*frequency, y*frequency) * amplitude;
						density += val;
						f *= roughness;
						a *= persistence;
				}

				return density;
		}


		void CreateMeshData()
		{
				int v1 = 0;
				int v2 = 0;
				int t = 0;

				for (int x = 0; x <= chunkSize; x++){
						for (int y = 0; y <= chunkSize; y++)
						{
								float xx = transform.position.x + (float)x;
								float yy = transform.position.z + (float)y;

								vertices[v1] = new Vector3(x*cellSize, getDensity(xx, yy), y*cellSize);
								uvs[v1] = new Vector2((float)x / chunkSize, (float)y / chunkSize);
								v1++;


								if (x < chunkSize && y < chunkSize){
										triangles[t] = v2;
										triangles[t+4] = triangles[t+1] = v2 + 1;
										triangles[t+3] = triangles[t+2] = v2 + chunkSize + 1;
										triangles[t+5] = v2 + chunkSize + 2;

										v2++;
										t+=6;
								}
						}
						if (x < chunkSize){ v2++; }
				}
		}


		void BuildMesh()
		{
				Mesh mesh = new Mesh();
				mesh.vertices = vertices;
				mesh.triangles = triangles;
				mesh.uv = uvs;

				mesh.RecalculateBounds();
				mesh.RecalculateNormals();
				meshFilter.mesh = mesh;
				meshCollider.sharedMesh = mesh;
		}


		// flat shaded

		// List<Vector3> vertices = new List<Vector3>();
		// List<int> triangles = new List<int>();
		// List<Vector2> uvs = new List<Vector2>();

		// float[,] terrainMap;
		// terrainMap = new float[width + 1, height + 1];

		// void CreateMeshData()
		// {
		// 		int index = 0;
		//
		// 		for (uint x = 0; x < width; x++){
		// 				for (uint y = 0; y < height; y++)
		// 				{
		// 						vertices.Add(new Vector3((0 + x) * cellSize, terrainMap[x, y], (0 + y) * cellSize));
		// 						vertices.Add(new Vector3((1 + x) * cellSize, terrainMap[x+1, y], (0 + y) * cellSize));
		// 						vertices.Add(new Vector3((0 + x) * cellSize, terrainMap[x, y+1], (1 + y) * cellSize));
		// 						vertices.Add(new Vector3((1 + x) * cellSize, terrainMap[x+1, y+1], (1 + y) * cellSize));
		//
		// 						triangles.Add(0 + index);
		// 						triangles.Add(2 + index);
		// 						triangles.Add(1 + index);
		// 						triangles.Add(2 + index);
		// 						triangles.Add(3 + index);
		// 						triangles.Add(1 + index);
		//
		// 						uvs.Add(new Vector2((0 + x) / width, (0 + y) / height));
		// 						uvs.Add(new Vector2((1 + x) / width, (0 + y) / height));
		// 						uvs.Add(new Vector2((0 + x) / width, (1 + y) / height));
		// 						uvs.Add(new Vector2((1 + x) / width, (1 + y) / height));
		//
		// 						index += 4;
		// 				}
		// 		}
		// }


		// void ClearMeshData()
		// {
		// 		vertices.Clear();
		// 		triangles.Clear();
		// 		uvs.Clear();
		// }
}
