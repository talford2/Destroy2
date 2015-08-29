using UnityEngine;

[RequireComponent(typeof (CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float StrafeSpeed = 5f;
    public float ForwardSpeed = 5f;

    private CharacterController controller;
    private Animator meshAnimator;
    private float fallSpeed;

    private float yawTarget;
    private float pitchTarget;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        meshAnimator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        yawTarget += Input.GetAxis("Mouse X");
        pitchTarget -= Input.GetAxis("Mouse Y");
        var move = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        fallSpeed += -9.81f*Time.deltaTime;
        var velocity = transform.right*move.x*StrafeSpeed + Vector3.up*fallSpeed + transform.forward*move.z*ForwardSpeed;
        pitchTarget = Mathf.Clamp(pitchTarget, -40f, 80f);

        PlayerCamera.Current.SetTargetPitchYaw(pitchTarget, yawTarget);

        controller.Move(velocity*Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, yawTarget, 0f), 25f*Time.deltaTime);

        // Locomotion
        meshAnimator.SetFloat("Speed", move.z);
    }
}
