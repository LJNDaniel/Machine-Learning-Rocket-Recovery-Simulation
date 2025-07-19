using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class RocketAgent : Agent
{
    [SerializeField] GoalManager goalManager;
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject fThrust;
    [SerializeField] GameObject bThrust;
    [SerializeField] GameObject rThrust;
    [SerializeField] GameObject lThrust;
    [SerializeField] float mainThrust = 1;
    [SerializeField] float controlThrust = 1;
    [SerializeField] Transform startPosition;
    [SerializeField] Material failMaterial;
    [SerializeField] Material progressMaterial;
    [SerializeField] Material successMaterial;
    [SerializeField] MeshRenderer platformMesh;
    [SerializeField] GameObject LandingPad;
    int successRange = 30;
    float currentSpeedSquared;
    float timeSinceLastExecution;
    Vector3 lastPosition;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = startPosition.localPosition;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        goalManager.resetGoals();
        timeSinceLastExecution = Time.time;
        lastPosition = transform.position;
        LandingPad.gameObject.tag = "ground";
    }

    private void FixedUpdate()
    {
        if (Time.time - timeSinceLastExecution >= 3f)
        {
            if (Vector3.Magnitude(transform.position - lastPosition) <= 15f)
            {
                AddReward(-5f);
            }
            timeSinceLastExecution = Time.time;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        foreach (GameObject goal in goalManager.goals)
        {
            sensor.AddObservation(goal.transform.localPosition);
        }
        // sensor.AddObservation(transform.localPosition);
        // sensor.AddObservation(rb.velocity);
        sensor.AddObservation(LandingPad.gameObject.transform);
        // sensor.AddObservation(goalManager.goals[1].transform);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        MainThrust(actions.ContinuousActions[0]);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetKey("space") ? 1 : 0;
    }

    private void OnTriggerEnter(Collider other)
    { 
        if (other.gameObject.tag == "heightGoal")
        {
            //Debug.Log("heightGoal");
            currentSpeedSquared = rb.velocity.y * rb.velocity.y;
            if(-1f * successRange <= currentSpeedSquared && successRange >= currentSpeedSquared)
            {
                platformMesh.material = progressMaterial;
                AddReward(100f + (4000 / currentSpeedSquared)); 
                LandingPad.gameObject.tag = "landingGoal";
            }
            else
            {
                platformMesh.material = failMaterial;
                AddReward(-20f + (4000 / currentSpeedSquared));
                EndEpisode();
            }
        }
        if(other.gameObject.tag == "heightLimit")
        {
            AddReward(-75f);
            platformMesh.material = progressMaterial;
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "landingGoal")
        {
            //Debug.Log("landingGoal");
            AddReward(500f + (4000 / currentSpeedSquared));
            platformMesh.material = successMaterial;
            EndEpisode();
        }
    }

    void MainThrust(float strength)
    {
        rb.AddForce(transform.up.normalized * mainThrust * Math.Max(strength, 0), ForceMode.Force);
    }

    void ForwardThrust(float strength)
    {
        rb.AddForceAtPosition(transform.up.normalized * controlThrust * Math.Max(strength, 0), fThrust.transform.localPosition, ForceMode.Force);
    }

    void BackwardThrust(float strength)
    {
        rb.AddForceAtPosition(transform.up.normalized * controlThrust * Math.Max(strength, 0), bThrust.transform.localPosition, ForceMode.Force);
    }

    void LeftThrust(float strength)
    {
        rb.AddForceAtPosition(transform.up.normalized * controlThrust * Math.Max(strength, 0), lThrust.transform.localPosition, ForceMode.Force);
    }

    void RightThrust(float strength)
    {
        rb.AddForceAtPosition(transform.up.normalized * controlThrust * Math.Max(strength, 0), rThrust.transform.localPosition, ForceMode.Force);
    }
}