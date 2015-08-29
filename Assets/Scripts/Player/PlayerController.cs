using UnityEngine;

[RequireComponent(typeof (CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private float fallSpeed;

    private float yawTarget;
    private float pitchTarget;

    private float turnSpeed = 10f;
    private float strafeSpeed = 5f;
    private float forwardSpeed = 5f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        yawTarget += Input.GetAxis("Mouse X");
        pitchTarget -= Input.GetAxis("Mouse Y");
        var move = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        fallSpeed += -9.81f*Time.deltaTime;

        var velocity = transform.right*move.x*strafeSpeed + Vector3.up*fallSpeed + transform.forward*move.z*forwardSpeed;
        controller.Move(velocity*Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(pitchTarget, yawTarget, 0f), 25f*Time.deltaTime);
    }
}
