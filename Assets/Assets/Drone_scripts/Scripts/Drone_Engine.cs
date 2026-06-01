using UnityEngine;

namespace XeroxUAV
{
    public class Drone_Engine : MonoBehaviour, IEngine
    {
        [Header("Propeller Properties")]
        [SerializeField] private Transform propeller;
        [SerializeField] private float propRotSpeed = 300f;
        [SerializeField] private bool counterClockwise = false;

        public void InitEngine()
        {
        }

        public void UpdateEngine(Rigidbody rb)
        {
            HandlePropellers();
        }

        void Update()
        {
            HandlePropellers();
        }

        void HandlePropellers()
        {
            if (!propeller) return;
            float dir = counterClockwise ? -1f : 1f;
            propeller.Rotate(Vector3.forward, propRotSpeed * dir * Time.deltaTime);
        }
    }
}