using System;
using UnityEngine;

public class CubeMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    
    private MatchReplayAgent MatchReplayAgent;

    private void Awake()
    {
        MatchReplayAgent = GetComponent<MatchReplayAgent>();
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (horizontalInput == 0f && verticalInput == 0) return;
        
        Vector3 movement = new Vector3(horizontalInput, 0.0f, verticalInput) * moveSpeed * Time.deltaTime;

        transform.Translate(movement, Space.World);
        transform.forward = movement;

        if (Input.GetKeyDown(KeyCode.E))
        {
            MatchReplayAgent.OnElimination(transform.position);
            transform.position = new Vector3(0f, 0.5f, 0f);
        }
    }
}
