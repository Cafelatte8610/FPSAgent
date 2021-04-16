using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public static class MyF {
    public static bool RandomBool () {
        return Random.Range (0, 2) == 0;
    }
}

public class FPSAgent : Agent {
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

    public float searchAngle = 130f;
    public SphereCollider searchArea;
    bool searchFlag = false;

    private LineRenderer laserLine;
    int play_limit = 0;
    // bool Gunshoted = false;
    Vector3 nowAngles;
    // public float hitForce = 100f;

    // private LineRenderer laserLine;

    // void Start () { // if (MyF.RandomBool ()) {
    //     //     target.position = new Vector3 (Random.Range (-14, 14), -1.0f, 12.0f);
    //     // } else {
    //     //     target.position = new Vector3 (Random.Range (-14, 14), 2.0f, 15.0f);
    //     // }
    //     // HundGunSc.GunFire ();
    //     // ray = new Ray (transform.position, new Vector3 (0, 0, 1));
    // }

    public override void Initialize () {

        this.FPSAbody = GetComponent<Rigidbody> ();
        Agent_t.eulerAngles = Vector3.zero;
    }
    // public float hoseiti = 1.375f;

    // void Update () {
    //     Vector3 rayOrigin = fpsCamera.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 0));
    //     ray = new Ray (rayOrigin, fpsCamera.transform.forward);
    //     Vector3 t_pos = target.position;
    //     t_pos.y = t_pos.y + hoseiti;
    //     Debug.Log (DistanceToLine (ray, t_pos) * 100);
    //     Vector3 lineOrigin = fpsCamera.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 0.0f));
    //     Debug.DrawLine (lineOrigin, fpsCamera.transform.forward * 500, Color.green);
    // }

    public override void OnEpisodeBegin () {
        nowAngles = Vector3.zero;
        laserLine = GetComponent<LineRenderer> ();
        FPSAbody.velocity = Vector3.zero;
        FPSAbody.angularVelocity = Vector3.zero;
        Agent_t.eulerAngles = nowAngles;
        TargetScript.isHit = false;
        // target.localPosition = new Vector3 (Random.Range (-14, 15), -1.0f, 12.0f);
        if (MyF.RandomBool ()) {
            target.localPosition = new Vector3 (Random.Range (-14, 15), -1.0f, 12.0f);
        } else {
            target.localPosition = new Vector3 (Random.Range (-14, 15), 2.0f, 15.0f);
        }
        // Agent_t.localPosition = new Vector3 (Random.Range (-13, 14), 0.375f, Random.Range (-5, 8));

        // Agent_t.localPosition = new Vector3 (Random.Range (-13, 14), 0.375f, 0.0f);
        Agent_t.eulerAngles = Vector3.zero;
        play_limit = 0;
    }

    public override void CollectObservations (VectorSensor sensor) {
        Vector3 rayOrigin = fpsCamera.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 0));
        ray = new Ray (rayOrigin, fpsCamera.transform.forward);
        // ray = new Ray (this.transform.position, Agent_t.forward);
        sensor.AddObservation (Agent_t.position);
        sensor.AddObservation (Agent_t.eulerAngles.x);
        sensor.AddObservation (Agent_t.eulerAngles.y);
        sensor.AddObservation (searchFlag);
        if (searchFlag) {
            sensor.AddObservation (target.position);
            Vector3 t_pos = target.position;
            t_pos.y = t_pos.y + 1.4607f;
            sensor.AddObservation (DistanceToLine (ray, t_pos) * 10);
        } else {
            sensor.AddObservation (Vector3.zero);
            sensor.AddObservation (0);
        }

    }

    public float DistanceToLine (Ray ray, Vector3 point) {
        return Vector3.Cross (ray.direction, point - ray.origin).magnitude;
    }

    // int bullets = 0;

    // float ray_reward = 0;
    // int plus_limit = 0;

    private void OnTriggerStay (Collider other) {
        if (other.tag == "Target") {
            var playerDirection = other.transform.position - transform.position;
            var angle = Vector3.Angle (transform.forward, playerDirection);
            if (angle <= searchAngle) {
                searchFlag = true;
            }
        }
    }

    private void OnTriggerExit (Collider other) {
        if (other.tag == "Player") {
            searchFlag = false;
        }
    }

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

        // StartCoroutine ("ShotEffect");

        // レイを飛ばす原点（カメラの中心）
        Vector3 rayOrigin = fpsCamera.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 0));
        RaycastHit hit;

        // LineRenderer.SetPosition(index, vec3)
        laserLine.SetPosition (0, gunEnd.position); // 銃口から
        // ray origin, direction, return float, max distance
        ray = new Ray (rayOrigin, fpsCamera.transform.forward);
        // Debug.Log (DistanceToLine (ray, target.position));
        if (vectorAction[2] == 1) {
            if (Physics.Raycast (rayOrigin, fpsCamera.transform.forward, out hit, gunlength)) {
                laserLine.SetPosition (1, hit.point);
                if (hit.collider.tag == "Target") {
                    HundGunSc.GunFire ();
                    AddReward (1.0f);
                    EndEpisode ();
                } else {
                    AddReward ((5000 - play_limit) * (-0.0002f));
                    EndEpisode ();
                }
            } else {
                laserLine.SetPosition (1, rayOrigin + (fpsCamera.transform.forward * gunlength));
            }
        }

        AddReward (-0.0002f);
        // ray = new Ray (this.transform.position, Agent_t.forward);
        // if (Physics.Raycast (ray, out hit, distance)) {
        //     if (hit.collider.tag == "Target") {
        //         Debug.DrawLine (ray.origin, ray.direction * distance, Color.red);
        //         AddReward (1000.0f);
        //         HundGunSc.GunFire ();
        //         EndEpisode ();
        //     } else {
        //         Debug.DrawLine (ray.origin, ray.direction * distance, Color.blue);
        //         // AddReward (-0.1f);
        //     }
        // }

        // Vector3 target_pos = target.position;
        // target_pos.y += 0.5f;
        // float t = DistanceToLine (ray, target_pos);
        // // AddReward ((ray_reward - t) * 10);
        // ray_reward = t;

        // if (vectorAction[2] == 1) {
        //     HundGunSc.GunFire ();
        // }

        // if (TargetScript.isHit) {
        //     AddReward (1.0f);
        //     EndEpisode ();
        // }

        // if ((271 < nowAngles.x && nowAngles.x <= 360) || (0 <= nowAngles.x && nowAngles.x < 89)) {
        //     // Debug.Log ("OK");
        // } else {
        //     // Debug.Log ("But");
        //     if (89 <= nowAngles.x && nowAngles.x <= 90) {
        //         nowAngles.x = 88;
        //     } else if (270 <= nowAngles.x && nowAngles.x <= 271) {
        //         nowAngles.x = 272;
        //     }
        // }

        // if ((271 < nowAngles.y && nowAngles.y <= 360) || (0 <= nowAngles.y && nowAngles.y < 89)) {
        //     // Debug.Log ("OK");
        // } else {
        //     // Debug.Log ("But");
        //     if (89 <= nowAngles.y && nowAngles.y <= 90) {
        //         nowAngles.y = 88;
        //     } else if (270 <= nowAngles.y && nowAngles.y <= 271) {
        //         nowAngles.y = 272;
        //     }
        // }
        // Debug.Log ("x：" + nowAngles.x + " y：" + nowAngles.y);

        Agent_t.eulerAngles = nowAngles;
        play_limit++;
    }
}

// using System.Collections;
// using System.Collections.Generic;
// using Unity.MLAgents;
// using Unity.MLAgents.Sensors;
// using UnityEngine;

// public static class MyF {
//     public static bool RandomBool () {
//         return Random.Range (0, 2) == 0;
//     }
// }

// public class FPSAgent : Agent {
//     public Transform target, Agent_t;
//     Rigidbody FPSAbody;
//     public TargetScript TargetScript;
//     public HandgunScriptLPFP HundGunSc;
//     Ray ray;
//     RaycastHit hit;
//     public int distance = 25;
//     public Vector2 rotationSpeed = Vector2.one;
//     public Transform gunEnd;
//     public Camera fpsCamera;
//     public int gunlength = 25;

//     private LineRenderer laserLine;
//     int play_limit = 0;
//     // bool Gunshoted = false;
//     Vector3 nowAngles;
//     // public float hitForce = 100f;

//     // private LineRenderer laserLine;

//     void Start () { // if (MyF.RandomBool ()) {
//         //     target.position = new Vector3 (Random.Range (-14, 14), -1.0f, 12.0f);
//         // } else {
//         //     target.position = new Vector3 (Random.Range (-14, 14), 2.0f, 15.0f);
//         // }
//         // HundGunSc.GunFire ();
//         // ray = new Ray (transform.position, new Vector3 (0, 0, 1));
//     }

//     public override void Initialize () {

//         this.FPSAbody = GetComponent<Rigidbody> ();
//         Agent_t.eulerAngles = Vector3.zero;
//     }

//     void Updata () {
//         Vector3 lineOrigin = fpsCamera.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 0.0f));
//         Debug.DrawLine (lineOrigin, fpsCamera.transform.forward * 500, Color.green);
//     }

//     public override void OnEpisodeBegin () {
//         nowAngles = Vector3.zero;
//         laserLine = GetComponent<LineRenderer> ();
//         FPSAbody.velocity = Vector3.zero;
//         FPSAbody.angularVelocity = Vector3.zero;
//         Agent_t.eulerAngles = nowAngles;
//         TargetScript.isHit = false;
//         // target.localPosition = new Vector3 (Random.Range (-14, 15), -1.0f, 12.0f);
//         if (MyF.RandomBool ()) {
//             target.localPosition = new Vector3 (Random.Range (-14, 15), -1.0f, 12.0f);
//         } else {
//             target.localPosition = new Vector3 (Random.Range (-14, 15), 2.0f, 15.0f);
//         }
//         // Agent_t.localPosition = new Vector3 (Random.Range (-13, 14), 0.375f, Random.Range (-5, 8));
//         Agent_t.eulerAngles = Vector3.zero;
//         play_limit = 0;
//     }

//     public override void CollectObservations (VectorSensor sensor) {
//         // ray = new Ray (this.transform.position, Agent_t.forward);
//         // sensor.AddObservation (target.position);
//         // sensor.AddObservation (Agent_t.position);
//         sensor.AddObservation (Agent_t.eulerAngles.x);
//         sensor.AddObservation (Agent_t.eulerAngles.y);
//         // sensor.AddObservation (DistanceToLine (ray, target.position));
//     }

//     public float DistanceToLine (Ray ray, Vector3 point) {
//         return Vector3.Cross (ray.direction, point - ray.origin).magnitude;
//     }

//     // int bullets = 0;

//     // float ray_reward = 0;
//     int plus_limit = 0;

//     public override void OnActionReceived (float[] vectorAction) {
//         if (play_limit > 5000) {
//             EndEpisode ();
//         }
//         Vector3 c_signal = Vector3.zero;
//         c_signal.y = vectorAction[0];
//         c_signal.x = vectorAction[1];
//         nowAngles = Agent_t.eulerAngles;
//         if (c_signal.y == 1) {
//             nowAngles.y += 0.01f * rotationSpeed.y;
//         } else if (c_signal.y == 2) {
//             nowAngles.y -= 0.01f * rotationSpeed.y;
//         }
//         if (c_signal.x == 1) {
//             nowAngles.x += 0.01f * rotationSpeed.x;
//         } else if (c_signal.y == 2) {
//             nowAngles.x -= 0.01f * rotationSpeed.x;
//         }

//         Agent_t.eulerAngles = nowAngles;

//         // StartCoroutine ("ShotEffect");

//         // レイを飛ばす原点（カメラの中心）
//         Vector3 rayOrigin = fpsCamera.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 0));
//         RaycastHit hit;

//         // LineRenderer.SetPosition(index, vec3)
//         laserLine.SetPosition (0, gunEnd.position); // 銃口から
//         // ray origin, direction, return float, max distance
//         if (Physics.Raycast (rayOrigin, fpsCamera.transform.forward, out hit, gunlength)) {
//             laserLine.SetPosition (1, hit.point);
//             if (hit.collider.tag == "Target") {
//                 AddReward (0.0001f);
//                 plus_limit++;
//             }
//             if (vectorAction[1] == 1) {
//                 if (hit.collider.tag == "Target") {
//                     HundGunSc.GunFire ();
//                     AddReward ((5000 - plus_limit) * 0.0002f);
//                     EndEpisode ();
//                 } else {
//                     AddReward ((5000 - play_limit) * (-0.0002f));
//                     EndEpisode ();
//                 }
//             }
//         } else {
//             laserLine.SetPosition (1, rayOrigin + (fpsCamera.transform.forward * gunlength));
//         }
//         AddReward (-0.0002f);
//         // ray = new Ray (this.transform.position, Agent_t.forward);
//         // if (Physics.Raycast (ray, out hit, distance)) {
//         //     if (hit.collider.tag == "Target") {
//         //         Debug.DrawLine (ray.origin, ray.direction * distance, Color.red);
//         //         AddReward (1000.0f);
//         //         HundGunSc.GunFire ();
//         //         EndEpisode ();
//         //     } else {
//         //         Debug.DrawLine (ray.origin, ray.direction * distance, Color.blue);
//         //         // AddReward (-0.1f);
//         //     }
//         // }

//         // Vector3 target_pos = target.position;
//         // target_pos.y += 0.5f;
//         // float t = DistanceToLine (ray, target_pos);
//         // // AddReward ((ray_reward - t) * 10);
//         // ray_reward = t;

//         // if (vectorAction[2] == 1) {
//         //     HundGunSc.GunFire ();
//         // }

//         // if (TargetScript.isHit) {
//         //     AddReward (1.0f);
//         //     EndEpisode ();
//         // }

//         // if ((271 < nowAngles.x && nowAngles.x <= 360) || (0 <= nowAngles.x && nowAngles.x < 89)) {
//         //     // Debug.Log ("OK");
//         // } else {
//         //     // Debug.Log ("But");
//         //     if (89 <= nowAngles.x && nowAngles.x <= 90) {
//         //         nowAngles.x = 88;
//         //     } else if (270 <= nowAngles.x && nowAngles.x <= 271) {
//         //         nowAngles.x = 272;
//         //     }
//         // }

//         // if ((271 < nowAngles.y && nowAngles.y <= 360) || (0 <= nowAngles.y && nowAngles.y < 89)) {
//         //     // Debug.Log ("OK");
//         // } else {
//         //     // Debug.Log ("But");
//         //     if (89 <= nowAngles.y && nowAngles.y <= 90) {
//         //         nowAngles.y = 88;
//         //     } else if (270 <= nowAngles.y && nowAngles.y <= 271) {
//         //         nowAngles.y = 272;
//         //     }
//         // }
//         // Debug.Log ("x：" + nowAngles.x + " y：" + nowAngles.y);

//         Agent_t.eulerAngles = nowAngles;
//         play_limit++;
//     }
// }