﻿using UnityEngine;
using System.Collections;
using System;

public class WaypointCircuit : MonoBehaviour 
{
	[System.Serializable]
	public class WaypointList
	{
		public WaypointCircuit circuit;
		public Transform[] items = new Transform[0];
	}

	[HideInInspector] public WaypointList waypointList = new WaypointList();      
	[HideInInspector] public float[] distances;
	private int numPoints;
	private Vector3[] points;
	public float Length { get; private set; }
	public Transform[] Waypoints
	{
		get { return waypointList.items; }
	}
		
	private int p0n;
	private int p1n;
	private int p2n;
	private int p3n;
	private float i;
	private Vector3 P0;
	private Vector3 P1;
	private Vector3 P2;
	private Vector3 P3;
	public bool showPath = true;
	[Range(100,500)]public float pathSmoothness = 100;
	public Color pathColor = Color.yellow;

	void Awake(){
		if (Waypoints.Length > 1) {
			CachePositionsAndDistances ();
		}
		numPoints = Waypoints.Length;
	}
		
	public RoutePoint GetRoutePoint(float dist){
		Vector3 p1 = GetRoutePosition (dist);
		Vector3 p2 = GetRoutePosition (dist + 0.1f);
		Vector3 delta = p2 - p1;
		return new RoutePoint (p1, delta.normalized);
	}
		
	public Vector3 GetRoutePosition(float dist){
		int point = 0;
		if (Length == 0) {
			Length = distances [distances.Length - 1];
		}
		dist = Mathf.Repeat (dist, Length);

		while (distances [point] < dist) {
			++point;
		}
			
		p1n = ((point - 1) + numPoints) % numPoints;
		p2n = point;
		i = Mathf.InverseLerp (distances [p1n], distances [p2n], dist);
		p0n = ((point - 2) + numPoints) % numPoints;
		p3n = (point + 1) % numPoints;
		p2n = p2n % numPoints;
		P0 = points [p0n];
		P1 = points [p1n];
		P2 = points [p2n];
		P3 = points [p3n];
		return CatmullRom (P0, P1, P2, P3, i);           
	}
		
	Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float i){
		return 0.5f * ((2 * p1) + (-p0 + p2) * i + (2 * p0 - 5 * p1 + 4 * p2 - p3) * i * i + (-p0 + 3 * p1 - 3 * p2 + p3) * i * i * i);
	}
		
	void CachePositionsAndDistances(){
		points = new Vector3[Waypoints.Length + 1];
		distances = new float[Waypoints.Length + 1];
		float accumulateDistance = 0;
		for (int i = 0; i < points.Length; ++i) {
			var t1 = Waypoints [(i) % Waypoints.Length];
			var t2 = Waypoints [(i + 1) % Waypoints.Length];
			if (t1 != null && t2 != null) {
				Vector3 p1 = t1.position;
				Vector3 p2 = t2.position;
				points [i] = Waypoints [i % Waypoints.Length].position;
				distances [i] = accumulateDistance;
				accumulateDistance += (p1 - p2).magnitude;
			}
		}
	}
		
	void OnDrawGizmos(){
		DrawGizmos (showPath);
	}
		
	void DrawGizmos(bool selected){ 
		waypointList.circuit = this;
		if (Waypoints.Length > 1 && selected) {
			numPoints = Waypoints.Length;
			CachePositionsAndDistances ();
			Length = distances [distances.Length - 1];
			Gizmos.color = pathColor;
			Vector3 prev = Waypoints [0].position;

			for (int n = 0; n < Waypoints.Length; ++n) {
				if (Waypoints [n].transform != this.transform) {
					Gizmos.DrawWireSphere (Waypoints [n].position, .75f);
				}
			}
			for (float dist = 0; dist < Length - 1; dist += Length / pathSmoothness) {
				Vector3 next = GetRoutePosition (dist + 1);
				Gizmos.DrawLine (prev, next);
				prev = next;
			}
			Gizmos.DrawLine (prev, Waypoints [0].position);
		}
	}
		
	public struct RoutePoint{ 
		public Vector3 position;
		public Vector3 direction;
		public RoutePoint(Vector3 position, Vector3 direction){
			this.position = position;
			this.direction = direction;
		}
	}

	public void AddWaypointsFromChildren(){
		WaypointCircuit circuit = this;
		var children = new Transform[circuit.transform.childCount];
		int n = 0;

		foreach (Transform child in circuit.transform) {
			children [n++] = child;
		}

		Array.Sort (children, new TransformNameComparer ());
		circuit.waypointList.items = new Transform[children.Length];

		for (n = 0; n < children.Length; ++n) {
			circuit.waypointList.items [n] = children [n];
		}
	}

	public class TransformNameComparer : IComparer{
		public int Compare(object x, object y){
			return ((Transform)x).name.CompareTo (((Transform)y).name);
		}
	}
}