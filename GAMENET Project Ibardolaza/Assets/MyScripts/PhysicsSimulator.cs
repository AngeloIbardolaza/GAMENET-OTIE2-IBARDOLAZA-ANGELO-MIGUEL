using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsSimulator : MonoBehaviour
{
    PhysicsScene2D physics2D;
    PhysicsScene physics;
    bool simulatePhysics;
    bool simulatePhysics2D;

    // Start is called before the first frame update
    void Awake()
    {
        if (NetworkServer.active)
        {
            physics2D = gameObject.scene.GetPhysicsScene2D();
            physics = gameObject.scene.GetPhysicsScene();

            simulatePhysics = physics.IsValid() && physics != Physics.defaultPhysicsScene;
            simulatePhysics2D = physics2D.IsValid() && physics2D != Physics2D.defaultPhysicsScene;
        }
    }

    private void FixedUpdate()
    {
        if (!NetworkServer.active)
        {
            return;
        }

        if (simulatePhysics)
        {
            physics.Simulate(Time.fixedDeltaTime);
        }

        if (simulatePhysics2D)
        {
            physics.Simulate(Time.fixedDeltaTime);
        }
    }
}
