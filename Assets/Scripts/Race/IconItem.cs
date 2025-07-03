using UnityEngine;
using UnityEngine.UI;

public class IconItem : MonoBehaviour 
{
    public Text InfoText = null;
	private bool open = false;
    
    public void DestroyIcon(){
		Destroy (gameObject);
	}
}