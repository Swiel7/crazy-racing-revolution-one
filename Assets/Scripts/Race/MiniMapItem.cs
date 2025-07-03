using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MiniMapItem : MonoBehaviour 
{
    [Header("Target")]
    public GameObject GraphicPrefab = null;
    public Transform Target = null;
    public Vector3 OffSet = Vector3.zero;

    [Header("Icon")]
    public Sprite Icon = null;
    public Color IconColor = new Color(1, 1, 1, 0.9f);
    public float Size = 20;

    [Header("Settings")]
    public bool OffScreen = true;
    public float BorderOffScreen = 0.01f;
    public float OffScreenSize = 10;
    private Image Graphic = null;
    private RectTransform RectRoot;
    private GameObject cacheItem = null;
    
    void Start(){
		if (MiniMap.MapUIRoot != null) {
			CreateIcon ();
		}
	}
		
    void CreateIcon(){
		cacheItem = Instantiate (GraphicPrefab) as GameObject;
		cacheItem.name = this.name + " (Pointer)";
		RectRoot = MiniMap.MapUIRoot;
		Graphic = cacheItem.GetComponent<Image> ();
		if (Icon != null) {
			Graphic.sprite = Icon;
			Graphic.color = IconColor;
		}
		cacheItem.transform.SetParent (RectRoot.transform, false);
		Graphic.GetComponent<RectTransform> ().anchoredPosition = Vector2.zero;
		if (Target == null) {
			Target = this.GetComponent<Transform> ();
		}
		cacheItem.tag = Target.tag;
		if (cacheItem.tag == "Opponent") {
			SettingsMenu.instance.aiPointers.Add (cacheItem);
		}
	}
    
    void FixedUpdate(){
		if (Target == null)
			return;
		if (Graphic == null)
			return;
		if (!this.cacheItem.transform.root.GetComponentInChildren<MiniMap> ())
			return;
		
		RectTransform rt = Graphic.GetComponent<RectTransform> ();
		Vector3 CorrectPosition = TargetPosition + OffSet;
		Vector2 vp2 = MiniMap.MiniMapCamera.WorldToViewportPoint (CorrectPosition);
		Vector2 position = new Vector2 ((vp2.x * RectRoot.sizeDelta.x) - (RectRoot.sizeDelta.x * 0.5f),
			                   (vp2.y * RectRoot.sizeDelta.y) - (RectRoot.sizeDelta.y * 0.5f));
		if (OffScreen) {
			position.x = Mathf.Clamp (position.x, -((RectRoot.sizeDelta.x * 0.5f) - BorderOffScreen), ((RectRoot.sizeDelta.x * 0.5f) - BorderOffScreen));
			position.y = Mathf.Clamp (position.y, -((RectRoot.sizeDelta.y * 0.5f) - BorderOffScreen), ((RectRoot.sizeDelta.y * 0.5f) - BorderOffScreen));
		}
   
		float size = Size;
		Vector3 screenPos = Vector3.zero;
		Vector3 forward = Target.position - m_miniMap.TargetPosition;
		Vector3 cameraRelativeDir = MiniMap.MiniMapCamera.transform.InverseTransformDirection (forward);
		cameraRelativeDir.z = 0;
		cameraRelativeDir = cameraRelativeDir.normalized / 2;
		float posPositiveX = Mathf.Abs (position.x);
		float relativePositiveX = Mathf.Abs ((0.5f + (cameraRelativeDir.x * m_miniMap.CompassSize)));
		if (posPositiveX >= relativePositiveX) {
			screenPos.x = 0.5f + (cameraRelativeDir.x * m_miniMap.CompassSize);
			screenPos.y = 0.5f + (cameraRelativeDir.y * m_miniMap.CompassSize);
			position = screenPos;
			size = OffScreenSize;
		} else {
			size = Size;
		}
        
		rt.anchoredPosition = position;
		rt.sizeDelta = Vector2.Lerp (rt.sizeDelta, new Vector2 (size, size), Time.deltaTime * 8);
		Quaternion r = Quaternion.identity;
		r.x = Target.rotation.x;
		rt.localRotation = r;
	}
		
    public void DestroyItem(){
		if (Graphic == null)
			return;
		Graphic.GetComponent<IconItem> ().DestroyIcon ();
	}
    
    public void HideItem(){
		if (cacheItem != null) {
			cacheItem.SetActive (false);
		}
	}
    
    public void ShowItem(){
		if (cacheItem != null) {
			cacheItem.SetActive (true);
		}
	}
		
    public Vector3 TargetPosition {
		get {
			if (Target == null) {
				return Vector3.zero;
			}
			return new Vector3 (Target.position.x, 0, Target.position.z);
		}
	}

    private MiniMap _minimap = null;
    private MiniMap m_miniMap {
		get {
			if (_minimap == null) {
				_minimap = this.cacheItem.transform.root.GetComponentInChildren<MiniMap> ();
			}
			return _minimap;
		}
	}
}