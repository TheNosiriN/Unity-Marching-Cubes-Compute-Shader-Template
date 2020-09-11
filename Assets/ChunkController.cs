using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;


public class ChunkController : MonoBehaviour
{
		public int renderDistance = 1;
		public Vector3Int chunkSize = new Vector3Int(16,16,16);
		public int cellSize = 1;
		public float chunkLOD = 1;
		public Transform viewer;

		public ComputeShader c_DensityField, c_MarchingCubes;
		public ComputeBuffer b_DensityBuffer = null, b_NormalsBuffer = null, b_TrianglesBuffer = null, b_TrianglesBufferArgs = null;


		Vector2 viewerPosition;
		public ChunkGenerator chunkGen;


		Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
		List<Chunk> chunkMemory = new List<Chunk>();


		struct Triangle
		{
				#pragma warning disable 649
				public Vector3 vertexA;
				public Vector3 vertexB;
				public Vector3 vertexC;

				public Vector3 this [int i] {
            get {
                switch (i) {
                    case 0:
                        return vertexA;
                    case 1:
                        return vertexB;
                    default:
                        return vertexC;
                }
            }
        }
		}


		void Start()
    {
				chunkGen = new ChunkGenerator(1337, chunkSize, cellSize, chunkLOD, c_DensityField, c_MarchingCubes);

				//size = chunkSize * cellSize;

				// for (int zz = 0; zz < 1; zz++){
				// 		for (int yy = 0; yy < 1; yy++){
				// 				for (int xx = 0; xx < 1; xx++)
				// 				{
				// 						Vector3Int key = new Vector3Int(xx, yy, zz);
				//
				// 						Chunk c = new Chunk(key, cellSize, chunkSize, chunkGen);
				// 						chunks.Add(key, c);
				// 						//chunkMemory.Add(c);
				// 				}
				// 		}
				// }
    }


		void ReleaseBuffers()
		{
				b_DensityBuffer.Release();
				b_TrianglesBuffer.Release();
				b_TrianglesBufferArgs.Release();
				b_DensityBuffer = null;
				b_TrianglesBuffer = null;
				b_TrianglesBufferArgs = null;
		}


    void Update()
    {
				//viewerPosition = new Vector3(viewer.position.x, viewer.position.y, viewer.position);

				UpdateChunkMemory();
    }


		void UpdateChunkMemory()
		{
				Vector3Int viewerChunk = new Vector3Int(
						Mathf.RoundToInt(viewer.position.x/(chunkSize.x*cellSize)),
						Mathf.RoundToInt(viewer.position.y/(chunkSize.y*cellSize)),
						Mathf.RoundToInt(viewer.position.z/(chunkSize.z*cellSize))
				);

				for (int i=0; i<chunkMemory.Count; i++)
				{
						int x1 = chunkMemory[i].position.x;
						int y1 = chunkMemory[i].position.z;
						int w1 = 0;
						int x2 = viewerChunk.x - renderDistance;
						int y2 = viewerChunk.z - renderDistance;
						int w2 = renderDistance*2;

						if(
								x2 + w2 < x1 ||
								y2 + w2 < y1 ||
								x1 + w1 < x2 ||
								y1 + w1 < y2
						){
								chunkMemory[i].DeactivateChunk();
								chunkMemory.RemoveAt(i);
						}else{
								chunkMemory[i].Step();
						}
				}


				for (int zz = -renderDistance; zz <= renderDistance; zz++){
						for (int xx = -renderDistance; xx <= renderDistance; xx++)
						{
								Vector3Int key = viewerChunk + new Vector3Int(xx, 0, zz);

								if (chunks.ContainsKey(key)){
										chunks[key].ActivateChunk();
										chunkMemory.Add(chunks[key]);
								}else{
										Chunk c = new Chunk(key, cellSize, chunkSize, chunkGen);
										chunks.Add(key, c);
										chunkMemory.Add(c);
								}
						}
				}
		}




		public class Chunk
		{
				public Vector3Int position;
				GameObject chunkObject;
				Bounds bounds;
				int cellSize;
				Vector3Int chunkSize;

				MeshFilter meshFilter;
				MeshCollider meshCollider;
				MeshRenderer meshRenderer;

				float[] mapData;


				public Chunk(Vector3Int position, int cellSize, Vector3Int chunkSize, ChunkGenerator gen)
				{
						this.position = position;
						this.cellSize = cellSize;
						this.chunkSize = chunkSize;


						chunkObject = new GameObject();
						chunkObject.name = "Chunk "+position.x+"_"+position.y+"_"+position.z;

						chunkObject.transform.Translate(new Vector3(
								(position.x * chunkSize.x * cellSize),// - (chunkSize * cellSize)/2,
								(position.y * chunkSize.y * cellSize),
								(position.z * chunkSize.z * cellSize)// - (chunkSize * cellSize)/2
						));


						OnMeshDataReceived(gen.RequestMeshData(position));
				}


				void OnMeshDataReceived(MeshData data)
				{
						if (data.vertices.Length > 0){
								meshFilter = chunkObject.AddComponent<MeshFilter>();
								meshCollider = chunkObject.AddComponent<MeshCollider>();
								meshRenderer = chunkObject.AddComponent<MeshRenderer>();

								meshRenderer.receiveShadows = true;
								meshRenderer.shadowCastingMode = ShadowCastingMode.On;
								meshRenderer.material = new Material(Shader.Find("Diffuse"));

								Mesh mesh = new Mesh();

								if (data.vertices.Length > 65534){ mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; }

								mesh.vertices = data.vertices;
								mesh.triangles = data.triangles;
								mesh.normals = data.normals;
								//mesh.uv = data.uvs;
								mapData = data.mapData;

								//mesh.RecalculateBounds();
								meshFilter.mesh = mesh;
								meshCollider.sharedMesh = mesh;
						}
				}


				public void Step()
				{

				}


				public void ActivateChunk()
				{
						if (chunkObject != null){
								chunkObject.SetActive(true);
						}
				}

				public void DeactivateChunk()
				{
						if (chunkObject != null){
								chunkObject.SetActive(false);
						}
				}
		}
}
