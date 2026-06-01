using UnityEngine;
using System.Collections;

namespace DroneAgent
{
    public class DroneController : MonoBehaviour
    {
        [Header("Fisica")]
        public float massa = 1f;
        public float dragLineare = 2f;
        public float dragAngolare = 2f;

        [Header("Potenza motori")]
        public float potenzaThrottle = 12f;
        public float velocitaOrizzontale = 5f;
        public float velocitaYaw = 80f;

        [Header("Inclinazione visiva")]
        public float maxInclinazione = 20f;
        public float velocitaInclinazione = 5f;

        [Header("Propellers")]
        public Transform propeller1, propeller2, propeller3, propeller4;
        public float propRotSpeed = 500f;

        [Header("Spawn")]
        public Transform takeOffPad;

        private Rigidbody rb;
        private float inputThrottle;
        private float inputX;
        private float inputZ;
        private float inputYaw;
        private bool isFlipping = false;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.mass = massa;
            rb.linearDamping = dragLineare;
            rb.angularDamping = dragAngolare;
            ResetDrone();
        }

        void Update()
        {
            LeggiInput();
            RuotaElice();
            if (Input.GetKeyDown(KeyCode.R)) ResetDrone();
            if (Input.GetKeyDown(KeyCode.F) && !isFlipping)
                StartCoroutine(FlipAvanti());
        }

        void FixedUpdate()
        {
            if (isFlipping) return;
            ApplicaFisica();
        }

        void LeggiInput()
        {
            inputThrottle = 0f;
            if (Input.GetKey(KeyCode.Space)) inputThrottle = 1f;
            if (Input.GetKey(KeyCode.LeftShift)) inputThrottle = -1f;

            inputZ = Input.GetAxis("Vertical");
            inputX = Input.GetAxis("Horizontal");

            inputYaw = 0f;
            if (Input.GetKey(KeyCode.Q)) inputYaw = -1f;
            if (Input.GetKey(KeyCode.E)) inputYaw = 1f;
        }

        void ApplicaFisica()
        {
            // 1. CONTRASTA GRAVITÀ sempre verticale pura
            float spintaBase = rb.mass * Physics.gravity.magnitude;
            rb.AddForce(Vector3.up * spintaBase, ForceMode.Force);

            // 2. THROTTLE separato — solo su/giù
            rb.AddForce(Vector3.up * inputThrottle * potenzaThrottle, ForceMode.Force);

            // 3. MOVIMENTO ORIZZONTALE puro — forza nel piano XZ
            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 moveForce = (forward * inputZ + right * inputX) * velocitaOrizzontale;
            rb.AddForce(moveForce, ForceMode.Force);

            // 4. YAW
            rb.AddTorque(Vector3.up * inputYaw * velocitaYaw * Mathf.Deg2Rad,
                         ForceMode.Force);

            // 5. INCLINAZIONE VISIVA
            float targetPitch = -inputZ * maxInclinazione;
            float targetRoll = -inputX * maxInclinazione;

            Quaternion targetRot = Quaternion.Euler(
                targetPitch,
                transform.eulerAngles.y,
                targetRoll
            );
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRot,
                Time.fixedDeltaTime * velocitaInclinazione
            );
        }

        IEnumerator FlipAvanti()
        {
            isFlipping = true;
            float durata = 0.5f;
            float tempo = 0f;
            float velocitaFlip = 360f / durata;

            // Spinta verso l'alto durante il flip
            rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);

            while (tempo < durata)
            {
                float rotazione = velocitaFlip * Time.deltaTime;
                transform.Rotate(Vector3.right, rotazione, Space.World);
                tempo += Time.deltaTime;
                yield return null;
            }

            // Raddrizza il drone
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            rb.angularVelocity = Vector3.zero;
            isFlipping = false;
        }

        void RuotaElice()
        {
            if (propeller1) propeller1.Rotate(Vector3.up, propRotSpeed * Time.deltaTime);
            if (propeller2) propeller2.Rotate(Vector3.up, -propRotSpeed * Time.deltaTime);
            if (propeller3) propeller3.Rotate(Vector3.up, propRotSpeed * Time.deltaTime);
            if (propeller4) propeller4.Rotate(Vector3.up, -propRotSpeed * Time.deltaTime);
        }

        public void ResetDrone()
        {
            Vector3 startPos = takeOffPad != null
                ? takeOffPad.position + Vector3.up * 1f
                : new Vector3(25.78757f, 1f, 26.21754f);

            transform.position = startPos;
            transform.rotation = Quaternion.identity;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            isFlipping = false;
        }
    }
}