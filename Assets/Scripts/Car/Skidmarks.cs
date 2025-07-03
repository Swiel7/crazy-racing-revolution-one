using UnityEngine;
using System.Collections;

public class Skidmarks : MonoBehaviour 
{
	class MarkSection
	{
		public Vector3 pos = Vector3.zero;
		public Vector3 normal = Vector3.zero;
		public Vector4 tangent = Vector4.zero;
		public Vector3 posl = Vector3.zero;
		public Vector3 posr = Vector3.zero;
		public byte intensity;
		public int lastIndex;
	};
		
	public Material skidmarksMaterial;
	public int maxMarks = 1024;
	public float markWidth = 0.35f;
	public float groundOffset = 0.02f;  
	public float minDistance = 1.0f; 
	int markIndex;
	MarkSection[] skidmarks;
	Mesh marksMesh;
	MeshRenderer mr;
	MeshFilter mf;
	Vector3[] vertices;
	Vector3[] normals;
	Vector4[] tangents;
	Color32[] colors;
	Vector2[] uvs;
	int[] triangles;
	bool updated;
	bool haveSetBounds;

	protected void Start(){
		skidmarks = new MarkSection[maxMarks];
		for (int i = 0; i < maxMarks; i++) {
			skidmarks [i] = new MarkSection ();
		}
		mf = GetComponent<MeshFilter> ();
		mr = GetComponent<MeshRenderer> ();

		if (mr == null) {
			mr = gameObject.AddComponent<MeshRenderer> ();
		}

		if (mf == null) {
			mf = gameObject.AddComponent<MeshFilter> ();
		}

		marksMesh = new Mesh ();
		marksMesh.MarkDynamic ();
		mf.sharedMesh = marksMesh;
		vertices = new Vector3[maxMarks * 4];
		normals = new Vector3[maxMarks * 4];
		tangents = new Vector4[maxMarks * 4];
		colors = new Color32[maxMarks * 4];
		uvs = new Vector2[maxMarks * 4];
		triangles = new int[maxMarks * 6];

		mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		mr.material = skidmarksMaterial;
		mr.receiveShadows = false;
		mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
	}

	protected void LateUpdate(){
		if (!updated)
			return;

		updated = false;
		marksMesh.vertices = vertices;
		marksMesh.normals = normals;
		marksMesh.tangents = tangents;
		marksMesh.triangles = triangles;
		marksMesh.colors32 = colors;
		marksMesh.uv = uvs;

		if (!haveSetBounds) {
			marksMesh.bounds = new Bounds (new Vector3 (0, 0, 0), new Vector3 (10000, 10000, 10000));
			haveSetBounds = true;
		}
		mf.sharedMesh = marksMesh;
	}

	public int AddSkidMark(Vector3 pos, Vector3 normal, float intensity, int lastIndex){
		if (intensity > 1)
			intensity = 1f;
		else if (intensity < 0)
			return -1;
		
		if (lastIndex > 0) {
			float sqrDistance = (pos - skidmarks [lastIndex].pos).sqrMagnitude;
			if (sqrDistance < minDistance * minDistance) {
				return lastIndex;
			}
		}

		MarkSection curSection = skidmarks[markIndex];
		curSection.pos = pos + normal * groundOffset;
		curSection.normal = normal;
		curSection.intensity = (byte)(intensity * 255f);
		curSection.lastIndex = lastIndex;

		if (lastIndex != -1) {
			MarkSection lastSection = skidmarks [lastIndex];
			Vector3 dir = (curSection.pos - lastSection.pos);
			Vector3 xDir = Vector3.Cross (dir, normal).normalized;

			curSection.posl = curSection.pos + xDir * markWidth * 0.5f;
			curSection.posr = curSection.pos - xDir * markWidth * 0.5f;
			curSection.tangent = new Vector4 (xDir.x, xDir.y, xDir.z, 1);

			if (lastSection.lastIndex == -1) {
				lastSection.tangent = curSection.tangent;
				lastSection.posl = curSection.pos + xDir * markWidth * 0.5f;
				lastSection.posr = curSection.pos - xDir * markWidth * 0.5f;
			}
		}
		UpdateSkidmarksMesh();
		int curIndex = markIndex;
		markIndex = ++markIndex % maxMarks;
		return curIndex;
	}
		
	void UpdateSkidmarksMesh(){
		MarkSection curr = skidmarks [markIndex];
		if (curr.lastIndex == -1)
			return;

		MarkSection last = skidmarks [curr.lastIndex];
		vertices [markIndex * 4 + 0] = last.posl;
		vertices [markIndex * 4 + 1] = last.posr;
		vertices [markIndex * 4 + 2] = curr.posl;
		vertices [markIndex * 4 + 3] = curr.posr;

		normals [markIndex * 4 + 0] = last.normal;
		normals [markIndex * 4 + 1] = last.normal;
		normals [markIndex * 4 + 2] = curr.normal;
		normals [markIndex * 4 + 3] = curr.normal;

		tangents [markIndex * 4 + 0] = last.tangent;
		tangents [markIndex * 4 + 1] = last.tangent;
		tangents [markIndex * 4 + 2] = curr.tangent;
		tangents [markIndex * 4 + 3] = curr.tangent;

		colors [markIndex * 4 + 0] = new Color32 (0, 0, 0, last.intensity);
		colors [markIndex * 4 + 1] = new Color32 (0, 0, 0, last.intensity);
		colors [markIndex * 4 + 2] = new Color32 (0, 0, 0, curr.intensity);
		colors [markIndex * 4 + 3] = new Color32 (0, 0, 0, curr.intensity);

		uvs [markIndex * 4 + 0] = new Vector2 (0, 0);
		uvs [markIndex * 4 + 1] = new Vector2 (1, 0);
		uvs [markIndex * 4 + 2] = new Vector2 (0, 1);
		uvs [markIndex * 4 + 3] = new Vector2 (1, 1);

		triangles [markIndex * 6 + 0] = markIndex * 4 + 0;
		triangles [markIndex * 6 + 2] = markIndex * 4 + 1;
		triangles [markIndex * 6 + 1] = markIndex * 4 + 2;

		triangles [markIndex * 6 + 3] = markIndex * 4 + 2;
		triangles [markIndex * 6 + 5] = markIndex * 4 + 1;
		triangles [markIndex * 6 + 4] = markIndex * 4 + 3;

		updated = true;
	}
}