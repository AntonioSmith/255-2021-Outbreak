using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Szczesniak {
    public class ForegroundRaycaster : MonoBehaviour {

        Camera cam;
        CameraTracking camTracker;

        // track things that are invisible...
        MeshRenderer hiddenObj;

        void Start() {
            cam = GetComponent<Camera>();
            camTracker = GetComponentInParent<CameraTracking>();
        }


        void Update() {
            if (hiddenObj) {
                hiddenObj.material.color = new Color(1, 1, 1, 1);
                hiddenObj = null;
            }
            DoRaycast();
        }

        void DoRaycast() {

            Vector3 vToTarget = camTracker.target.position - transform.position;
            Ray ray = new Ray(transform.position, vToTarget);

            if (Physics.Raycast(ray, out RaycastHit hit)) {
                Transform thingWeHit = hit.transform;

                if (thingWeHit != camTracker.target) {
                    MeshRenderer renderer = thingWeHit.GetComponent<MeshRenderer>();
                    //renderer.enabled = false;
                    renderer.material.color = new Color(1, 1, 1, .5f);

                    hiddenObj = renderer;
                }
            }
        }
    }
}