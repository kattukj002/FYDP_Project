using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationForce : MonoBehaviour
{
    public DigitalController elbow_controller;
    public DigitalController shoulder_abduction_controller;
    public DigitalController shoulder_flexion_controller;

    public float elbow_angle;
    public float shoulder_flexion;
    public float shoulder_abduction;
    public float upper_arm_length;
    public float lower_arm_length;
    
    private Vector3 sim_force;
    private float cached_mass = 0;
    private Vector3 collision_force = new Vector3(0,0,0);

    // Start is called before the first frame update
    void Start()
    {
        elbow_controller = new PIDController(5, 0.01f, 2, Time.fixedDeltaTime, -40);
        shoulder_abduction_controller = new PIDController(5, 0.01f, 2, Time.fixedDeltaTime, -40);
        shoulder_flexion_controller = new PIDController(5, 0.01f, 2, Time.fixedDeltaTime, -40);

        if (TryGetComponent(out Rigidbody found_rigid_body))
        {
            cached_mass = found_rigid_body.mass;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        sim_force = Physics.gravity*cached_mass + collision_force;
        applyForces(sim_force);
        collision_force.Set(0,0,0);
    }

    void OnCollisionEnter(Collision collision)
    {
        //Assume all collisions happen over one Time.fixedDeltaTime unit.
        collision_force = collision.impulse/Time.fixedDeltaTime;
    }

    void applyForces(Vector3 force)
    {
        elbow_controller.controlEffort();
        shoulder_abduction_controller.controlEffort();
        shoulder_flexion_controller.controlEffort();
    }

}