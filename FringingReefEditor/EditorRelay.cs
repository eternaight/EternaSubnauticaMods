using UnityEngine;

namespace FringingReefEditor {
    class EditorRelay : MonoBehaviour {

        private GameObject brushGizmo;
        private Player player;
        private void Start() {
            brushGizmo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            player = GetComponent<Player>();
            brushGizmo.transform.localScale = Vector3.one * 5;
        }

        private void Update() {
            if (Physics.Raycast(transform.position, Camera.main.transform.forward, out var hitInfo, 10, Voxeland.GetTerrainLayerMask())) {
                brushGizmo.transform.position = hitInfo.point;
                if (player.GetRightHandDown()) {
                    LargeWorldStreamer.main.PerformSphereEdit(hitInfo.point, 5, true, 15);
                }
            }
        }
    }
}
