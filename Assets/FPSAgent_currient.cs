using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public static class MyF_c {
    public static bool RandomBool () {
        return Random.Range (0, 2) == 0;
    }
}

public class FPSAgent_currient : Agent {
    public Transform target, Agent_t;
    Rigidbody FPSAbody;
    public TargetScript TargetScript;
    public HandgunScriptLPFP HundGunSc;
    Ray ray;
    RaycastHit hit;
    public int distance = 25;
    public Vector2 rotationSpeed = Vector2.one;
    public Transform gunEnd;
    public Camera fpsCamera;
    public int gunlength = 25;

    private LineRenderer laserLine;
    int play_limit = 0;
    // bool Gunshoted = false;
    Vector3 nowAngles;
    public SphereCollider searchArea;
    public float searchAngle = 130f;
    float scanReward, findReward, hitReward, shotRewerd, stepReward;

    // bool target_stay = false;

    public override void Initialize () {

        this.FPSAbody = GetComponent<Rigidbody> ();
        Agent_t.eulerAngles = Vector3.zero;
    }

    void Updata () {
        Vector3 lineOrigin = fpsCamera.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 0.0f));
        Debug.DrawLine (lineOrigin, fpsCamera.transform.forward * 500, Color.green);
    }
    public void SetEnvParameters () {
        EnvironmentParameters envParameters = Academy.Instance.EnvironmentParameters;
        this.scanReward = envParameters.GetWithDefault ("scan_reward", 1.0f);
        this.findReward = envParameters.GetWithDefault ("find_reward", 1.0f);
        this.hitReward = envParameters.GetWithDefault ("hit_rewerd", 0.0f);
        this.shotRewerd = envParameters.GetWithDefault ("shot_reward", 0.0f);
        this.stepReward = envParameters.GetWithDefault ("step_reward", -0.0002f);
    }

    public override void OnEpisodeBegin () {
        nowAngles = Vector3.zero;
        laserLine = GetComponent<LineRenderer> ();
        FPSAbody.velocity = Vector3.zero;
        FPSAbody.angularVelocity = Vector3.zero;
        Agent_t.eulerAngles = nowAngles;
        TargetScript.isHit = false;
        // target.localPosition = new Vector3 (Random.Range (-14, 15), -1.0f, 12.0f);
        if (this.scanReward == 2.0f && MyF_c.RandomBool ()) {
            target.localPosition = new Vector3 (Random.Range (-14, 15), 2.0f, 15.0f);
        } else {
            target.localPosition = new Vector3 (Random.Range (-14, 15), -1.0f, 12.0f);
        }
        // Agent_t.localPosition = new Vector3 (Random.Range (-13, 14), 0.375f, Random.Range (-5, 8));
        Agent_t.eulerAngles = Vector3.zero;
        play_limit = 0;
        // 環境パラメタの設定
        SetEnvParameters ();
    }

    public float DistanceToLine (Ray ray, Vector3 point) {
        return Vector3.Cross (ray.direction, point - ray.origin).magnitude;
    }

    // private void OnTriggerStay (Collider other) {
    //     if (other.tag == "Target") {
    //         var playerDirection = other.transform.position - transform.position;
    //         //　敵の前方からの主人公の方向
    //         var angle = Vector3.Angle (transform.forward, playerDirection);
    //         //　サーチする角度内だったら発見
    //         if (angle <= searchAngle) {
    //             target_stay = true;
    //             // onlyForwardSearchEnemy.SetState (OnlyForwardSearchEnemy.EnemyState.Chase, other.transform);
    //         }
    //     } else {
    //         target_stay = false;
    //     }
    // }

    // float ray_reward = 0;

    public override void OnActionReceived (float[] vectorAction) {
        if (play_limit > 5000) {
            EndEpisode ();
        }
        Vector3 c_signal = Vector3.zero;
        c_signal.y = vectorAction[0];
        c_signal.x = vectorAction[1];
        nowAngles = Agent_t.eulerAngles;
        if (c_signal.y == 1) {
            nowAngles.y += 0.01f * rotationSpeed.y;
        } else if (c_signal.y == 2) {
            nowAngles.y -= 0.01f * rotationSpeed.y;
        }
        if (c_signal.x == 1) {
            nowAngles.x += 0.01f * rotationSpeed.x;
        } else if (c_signal.y == 2) {
            nowAngles.x -= 0.01f * rotationSpeed.x;
        }

        Agent_t.eulerAngles = nowAngles;

        if (this.hitReward > 0) {

            if (vectorAction[2] == 1) {
                AddReward (this.shotRewerd);
                Vector3 rayOrigin = fpsCamera.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 0));
                RaycastHit hit;
                HundGunSc.GunFire ();
                // LineRenderer.SetPosition(index, vec3)
                laserLine.SetPosition (0, gunEnd.position); // 銃口から
                // ray origin, direction, return float, max distance
                if (Physics.Raycast (rayOrigin, fpsCamera.transform.forward, out hit, gunlength)) {
                    laserLine.SetPosition (1, hit.point);
                    if (this.findReward > 0 && hit.collider.tag == "Target") {
                        AddReward (this.findReward);
                        EndEpisode ();
                    }
                    // } else {
                    //     AddReward ((5000 - play_limit) * (this.stepReward));
                    //     EndEpisode ();
                    // }
                } else {
                    laserLine.SetPosition (1, rayOrigin + (fpsCamera.transform.forward * gunlength));
                }
            }
        } else {
            Vector3 rayOrigin = fpsCamera.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 0));
            RaycastHit hit;
            // LineRenderer.SetPosition(index, vec3)
            laserLine.SetPosition (0, gunEnd.position); // 銃口から
            // ray origin, direction, return float, max distance
            if (Physics.Raycast (rayOrigin, fpsCamera.transform.forward, out hit, gunlength)) {
                laserLine.SetPosition (1, hit.point);
                if (this.findReward > 0 && hit.collider.tag == "Target") {
                    AddReward (this.findReward);
                    EndEpisode ();
                }
                // } else {
                //     AddReward ((5000 - play_limit) * (this.stepReward));
                //     EndEpisode ();
                // }
            } else {
                laserLine.SetPosition (1, rayOrigin + (fpsCamera.transform.forward * gunlength));
            }
        }

        if (TargetScript.isHit) {
            AddReward (this.hitReward);
            EndEpisode ();
        }

        AddReward (this.stepReward);
        Agent_t.eulerAngles = nowAngles;
        play_limit++;
    }

}