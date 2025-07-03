using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Animator))]
public class Driver : MonoBehaviour
{
	private Animator animator;
	private Car_Controller carController;
	public bool enableDriver = true;

	[Header("Driver Targets")]
	public Transform rightHandTarget = null;
	public Transform leftHandTarget = null;
	public Transform leftFootTarget = null;
	public Transform rightFootTarget = null;
	public Transform driverLookTarget = null;

	[Header("Hand Rotation References")]
	public Transform leftHandRef;
	public Transform rightHandRef;

	[Header("Driver Position")]
	public Vector3 seatPosition;
	private float initialDriverLookX;
	[HideInInspector]public float steer;

	void Awake(){
		animator = GetComponent<Animator> ();
		carController = transform.parent.GetComponent<Car_Controller> ();
		initialDriverLookX = driverLookTarget.localPosition.x;
	}

	void Update(){
		transform.localPosition = seatPosition;
		HeadLook ();
	}

	void LateUpdate(){
		if (rightHandRef == null || leftHandRef == null)
			return;

		Transform[] Lfingers = animator.GetBoneTransform (HumanBodyBones.LeftHand).GetComponentsInChildren<Transform> ();
		Transform[] Rfingers = animator.GetBoneTransform (HumanBodyBones.RightHand).GetComponentsInChildren<Transform> ();
		Transform[] LfingersRef = leftHandRef.GetComponentsInChildren<Transform> ();
		Transform[] RfingersRef = rightHandRef.GetComponentsInChildren<Transform> ();
		for (int i = 1; i < Lfingers.Length; i++) {
			Lfingers [i].localRotation = LfingersRef [i].localRotation;
			Rfingers [i].localRotation = RfingersRef [i].localRotation;
		}
	}

	void HeadLook(){
		steer = carController.steerInput;
		Vector3 newLook = driverLookTarget.localPosition;
		newLook.x = steer + initialDriverLookX;
		driverLookTarget.localPosition = Vector3.Lerp (driverLookTarget.localPosition, newLook, Time.deltaTime * 3f);
	}

	void OnAnimatorIK(){
		if (animator) {
			if (enableDriver) {
				if (driverLookTarget != null) {
					animator.SetLookAtWeight (1);
					animator.SetLookAtPosition (driverLookTarget.position);
				}
				if (rightHandTarget != null && leftHandTarget != null) {
					animator.SetIKPositionWeight (AvatarIKGoal.RightHand, 1);
					animator.SetIKRotationWeight (AvatarIKGoal.RightHand, 1);
					animator.SetIKPosition (AvatarIKGoal.RightHand, rightHandTarget.position);
					animator.SetIKRotation (AvatarIKGoal.RightHand, rightHandTarget.rotation);

					animator.SetIKPositionWeight (AvatarIKGoal.LeftHand, 1);
					animator.SetIKRotationWeight (AvatarIKGoal.LeftHand, 1);
					animator.SetIKPosition (AvatarIKGoal.LeftHand, leftHandTarget.position);
					animator.SetIKRotation (AvatarIKGoal.LeftHand, leftHandTarget.rotation);
				}
				if (rightFootTarget != null && leftFootTarget != null) {
					animator.SetIKPositionWeight (AvatarIKGoal.LeftFoot, 1);
					animator.SetIKRotationWeight (AvatarIKGoal.LeftFoot, 1);
					animator.SetIKPosition (AvatarIKGoal.LeftFoot, leftFootTarget.position);
					animator.SetIKRotation (AvatarIKGoal.LeftFoot, leftFootTarget.rotation);

					animator.SetIKPositionWeight (AvatarIKGoal.RightFoot, 1);
					animator.SetIKRotationWeight (AvatarIKGoal.RightFoot, 1);
					animator.SetIKPosition (AvatarIKGoal.RightFoot, rightFootTarget.position);
					animator.SetIKRotation (AvatarIKGoal.RightFoot, rightFootTarget.rotation);
				}
			} else {
				animator.SetIKPositionWeight (AvatarIKGoal.RightHand, 0);
				animator.SetIKRotationWeight (AvatarIKGoal.RightHand, 0);
				animator.SetIKPositionWeight (AvatarIKGoal.LeftHand, 0);
				animator.SetIKRotationWeight (AvatarIKGoal.LeftHand, 0);
				animator.SetLookAtWeight (0);
			}
		}
	}
}