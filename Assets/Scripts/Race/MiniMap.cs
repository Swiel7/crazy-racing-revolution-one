using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MiniMap : MonoBehaviour
{
    public Camera MMCamera = null;
    public RectTransform MMUIRoot = null;
    public Image PlayerIcon = null;
    public float height = 60;
    [Range(1, 15)] public float LerpHeight = 8;
    [Range(25, 500)] public float CompassSize = 175f;
    [Range(1, 15)] public float LerpRotation = 8;
    public Vector3 MiniMapPosition = Vector2.zero;
    public Vector3 MiniMapRotation = Vector3.zero;
	public Vector2 MiniMapSize = Vector2.zero;
    public float LerpTransition = 7;
	private Transform target;
    public static Camera MiniMapCamera = null;
	public static RectTransform MapUIRoot = null;

    void Awake(){
		MiniMapCamera = MMCamera;
		MapUIRoot = MMUIRoot;
	}
    
    void Update(){
		if (!target && GameObject.FindGameObjectWithTag ("Player")) {
			target = GameObject.FindGameObjectWithTag ("Player").transform;
		}
		PositionControl ();
		RotationControl ();
		MapSize ();
	}
   
    void PositionControl(){
		Vector3 p = m_Transform.position;
		p.x = target.position.x;
		p.z = target.position.z;
		p.y = (85 / 2) + (target.position.y * 2);
		m_Transform.position = p;
	}
    
    void RotationControl(){
		RectTransform rt = PlayerIcon.GetComponent<RectTransform> ();
		Vector3 e = m_Transform.eulerAngles;
		e.y = target.eulerAngles.y;
		rt.rotation = Quaternion.identity;
		if (m_Transform.eulerAngles.y != e.y) {
			float d = e.y - m_Transform.eulerAngles.y;

			if (d > 180 || d < -180) {
				m_Transform.eulerAngles = e;
			}
		}
		m_Transform.eulerAngles = Vector3.Lerp (this.transform.eulerAngles, e, Time.deltaTime * LerpRotation);
	}
   
    void MapSize(){
		RectTransform rt = MMUIRoot;
		rt.sizeDelta = Vector2.Lerp (rt.sizeDelta, MiniMapSize, Time.deltaTime * LerpTransition);
		rt.anchoredPosition = Vector3.Lerp (rt.anchoredPosition, MiniMapPosition, Time.deltaTime * LerpTransition);
		rt.localEulerAngles = Vector3.Lerp (rt.localEulerAngles, MiniMapRotation, Time.deltaTime * LerpTransition);
		MMCamera.orthographicSize = Mathf.Lerp (MMCamera.orthographicSize, height, Time.deltaTime * LerpHeight);
	}
		
    public Vector3 TargetPosition {
		get {
			Vector3 v = Vector3.zero;
			if (target != null) {
				v = target.transform.position;
			}
			return v;
		}
	}

    private Transform t;
    private Transform m_Transform {
		get {
			if (t == null) {
				t = this.GetComponent<Transform> ();
			}
			return t;
		}
	}
}