using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Wheels : MonoBehaviour 
{
	private GroundMaterials groundMaterials;
	private Texture2D currentTexture;
	private PhysicMaterial currentPhysicMaterial;
	private WheelCollider wheelCollider;
	private WheelHit wheelHit;
	private Skidmarks skidmarks;
	private int lastSkid;
	private Terrain terrain;
	private TerrainData terrainData;
	private SplatPrototype[] splatPrototypes;
	private AudioSource skidAudioSource;
	[HideInInspector] public bool shouldEmit = false;
	private ParticleSystem particleToEmit;
	private bool useSkidmarks;
	private List<string> wheelParticleChildren = new List<string>();

	void Start(){
		groundMaterials = GameObject.FindObjectOfType(typeof(GroundMaterials)) as GroundMaterials;
		SetupWheelComponents();
		GetTerrainInfo();
	}
		
	void SetupWheelComponents(){
		if (!wheelCollider) {
			wheelCollider = GetComponent<WheelCollider> ();
		}
		skidmarks = GameObject.FindObjectOfType (typeof(Skidmarks)) as Skidmarks;
		skidAudioSource = GetComponent<AudioSource> ();
		skidAudioSource.spatialBlend = 1f;
		skidAudioSource.loop = true;
		skidAudioSource.outputAudioMixerGroup = SoundManager.instance.raceMixerGroup;

		if (groundMaterials.terrainSurfaceTypes.Count > 0) {
			for (int i = 0; i < groundMaterials.terrainSurfaceTypes.Count; i++) {
				if (!wheelParticleChildren.Contains (groundMaterials.terrainSurfaceTypes [i].skidParticle.transform.name + "(Clone)")) {
					GameObject particle = (GameObject)Instantiate (groundMaterials.terrainSurfaceTypes [i].skidParticle, transform.position, Quaternion.identity);
					particle.transform.parent = transform;
					var em = particle.GetComponent<ParticleSystem> ().emission;
					em.enabled = false;
					wheelParticleChildren.Add (particle.transform.name);
				}
			}
		}

		if (groundMaterials.physicMaterialSurface.Count > 0) {
			for (int i = 0; i < groundMaterials.physicMaterialSurface.Count; i++) {
				if (!wheelParticleChildren.Contains (groundMaterials.physicMaterialSurface [i].skidParticle.transform.name + "(Clone)")) {
					GameObject particle = (GameObject)Instantiate (groundMaterials.physicMaterialSurface [i].skidParticle, transform.position, Quaternion.identity);
					particle.transform.parent = transform;
					var em = particle.GetComponent<ParticleSystem> ().emission;
					em.enabled = false;
					wheelParticleChildren.Add (particle.transform.name);
				}
			}
		}
	}

	void Update(){
		if (skidAudioSource && !skidAudioSource.isPlaying) {
			skidAudioSource.Play ();
		}
		wheelCollider.GetGroundHit (out wheelHit);
		PhysicMaterialSurfaceDetection ();
		TerrainSurfaceDetection ();
		Emit ();
	}

	void PhysicMaterialSurfaceDetection(){
		if (wheelHit.collider) {
			if (wheelHit.collider.material) {
				currentPhysicMaterial = wheelHit.collider.material;
				for (int i = 0; i < groundMaterials.physicMaterialSurface.Count; i++) {
					if (currentPhysicMaterial.name.Replace (" (Instance)", "") == groundMaterials.physicMaterialSurface [i].physicMaterial.name) {
						skidAudioSource.clip = groundMaterials.physicMaterialSurface [i].skidSound;
						foreach (Transform t in transform) {
							if (t.name == groundMaterials.physicMaterialSurface [i].skidParticle.name + "(Clone)") {
								particleToEmit = t.GetComponent<ParticleSystem> ();
							}
						}
						if (groundMaterials.physicMaterialSurface [i].surfaceType == GroundMaterials.SurfaceType.OffRoad) {
							if (wheelCollider.attachedRigidbody.velocity.magnitude > 5f) {
								shouldEmit = true;
								skidAudioSource.volume = 0.5f;
							}
						}
						useSkidmarks = groundMaterials.physicMaterialSurface [i].allowSkidmark;
					}
				}
			}
		}
	}

	void TerrainSurfaceDetection(){
		if (terrain == null)
			return;
		if (wheelHit.collider) {
			if (wheelHit.collider.GetComponent<Terrain> ()) {
				currentTexture = splatPrototypes [GetTerrainTexture (transform.position)].texture;
			} else {
				currentTexture = null;
			}
		}
			
		for (int i = 0; i < groundMaterials.terrainSurfaceTypes.Count; i++) {
			if (currentTexture == groundMaterials.terrainSurfaceTypes [i].texture) {
				skidAudioSource.clip = groundMaterials.terrainSurfaceTypes [i].skidSound;

				foreach (Transform t in transform) {
					if (t.name == groundMaterials.terrainSurfaceTypes [i].skidParticle.name + "(Clone)") {
						particleToEmit = t.GetComponent<ParticleSystem> ();
					}
				}
				if (groundMaterials.terrainSurfaceTypes [i].surfaceType == GroundMaterials.SurfaceType.OffRoad) {
					if (wheelCollider.attachedRigidbody.velocity.magnitude > 5f) { 
						shouldEmit = true; 
						skidAudioSource.volume = 0.5f;
					}
				}
				useSkidmarks = groundMaterials.terrainSurfaceTypes [i].allowSkidmark;
			}
		}
	}

	public void Emit(){
		if (shouldEmit && wheelCollider.isGrounded) {
			particleToEmit.Emit (1);
			Vector3 skidPoint = wheelHit.point + (wheelCollider.attachedRigidbody.velocity * Time.fixedDeltaTime);

			if (useSkidmarks) {
				lastSkid = skidmarks.AddSkidMark (skidPoint, wheelHit.normal, 0.5f, lastSkid);
			} else {
				lastSkid = -1;
			}
			skidAudioSource.volume = Mathf.Abs (wheelHit.sidewaysSlip) + Mathf.Abs (wheelHit.forwardSlip) + 0.5f;
		} else {
			lastSkid = -1;
			skidAudioSource.volume -= Time.deltaTime;
		}
	}
		
	private void GetTerrainInfo(){
		if (Terrain.activeTerrain) {
			terrain = Terrain.activeTerrain;
			terrainData = terrain.terrainData;
			splatPrototypes = terrain.terrainData.splatPrototypes;
		}
	}

	private float[] GetTextureMix(Vector3 worldPos){
		terrain = Terrain.activeTerrain;
		terrainData = terrain.terrainData;
		Vector3 terrainPos = terrain.transform.position;
		int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
		int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);
		float[,,] splatmapData = terrainData.GetAlphamaps (mapX, mapZ, 1, 1);
		float[] cellMix = new float[splatmapData.GetUpperBound (2) + 1];

		for (int n = 0; n < cellMix.Length; ++n) {
			cellMix [n] = splatmapData [0, 0, n];
		}
		return cellMix;
	}

	private int GetTerrainTexture(Vector3 worldPos){
		float[] mix = GetTextureMix (worldPos);
		float maxMix = 0;
		int maxIndex = 0;

		for (int n = 0; n < mix.Length; ++n) {
			if (mix [n] > maxMix) {
				maxIndex = n;
				maxMix = mix [n];
			}
		}
		return maxIndex;
	}
}