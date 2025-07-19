using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColliderScript : MonoBehaviour 
{
    GoalManager goalManager;

    private void OnTriggerEnter() {
        goalManager.hit(gameObject);
    }

    private void Start() {
        goalManager = transform.parent.GetComponent<GoalManager>();
    }
}
