using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour 
{
	public enum CheckpointType {Speedtrap, TimeCheckpoint};
	public CheckpointType checkpointType;
	public float timeToAdd = 10f;
}