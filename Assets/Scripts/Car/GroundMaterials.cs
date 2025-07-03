using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GroundMaterials : MonoBehaviour 
{
	public enum SurfaceType { OnRoad, OffRoad};

	[System.Serializable]
	public class TerrainSurface
	{
		public string surfaceName;
		public GroundMaterials.SurfaceType surfaceType;         
		public Texture2D texture;
		public GameObject skidParticle;
		public AudioClip skidSound;
		public bool allowSkidmark;          
	}

	[System.Serializable]
	public class PhysicMaterialSurface
	{
		public string surfaceName;
		public GroundMaterials.SurfaceType surfaceType;         
		public PhysicMaterial physicMaterial;
		public GameObject skidParticle;
		public AudioClip skidSound;
		public bool allowSkidmark; 
	}

	[Header("Terrain Surface")]
	public List<TerrainSurface> terrainSurfaceTypes = new List<TerrainSurface>();

	[Header("Physic Material Surface")]
	public List<PhysicMaterialSurface> physicMaterialSurface = new List<PhysicMaterialSurface>();
}