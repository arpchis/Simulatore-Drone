using UnityEngine;

namespace DroneAgent
{
    public class LineFollower : MonoBehaviour
    {
        [Header("Linea da seguire")]
        public LineRenderer lineRenderer;

        [Header("Velocità")]
        public float velocita = 3f;
        public float velocitaRotazione = 3f;
        public float velocitaVerticale = 3f;
        public float waypointRadius = 1.5f;
        public float altezzaVolo = 1.5f;

        [Header("Spawn")]
        public Transform takeOffPad;
        public Transform landingPad;

        [Header("Propellers")]
        public Transform propeller1, propeller2, propeller3, propeller4;
        public float propRotSpeed = 500f;

        [Header("Stato")]
        public bool autoVolo = false;

        private Rigidbody rb;
        private DroneController droneController;
        private Vector3[] punti;
        private int puntoCorrente = 0;
        private bool percorsoCompletato = false;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            droneController = GetComponent<DroneController>();

            if (lineRenderer != null)
            {
                punti = new Vector3[lineRenderer.positionCount];
                lineRenderer.GetPositions(punti);

                for (int i = 0; i < punti.Length; i++)
                {
                    punti[i] = lineRenderer.transform.TransformPoint(punti[i]);
                    punti[i].y = punti[i].y + altezzaVolo;
                }

                Debug.Log($"Linea caricata con {punti.Length} punti");
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                autoVolo = !autoVolo;
                percorsoCompletato = false;
                puntoCorrente = 0;
                if (droneController != null)
                    droneController.enabled = !autoVolo;
                Debug.Log(autoVolo ? "Auto volo ON" : "Auto volo OFF");
            }

            RuotaElice();
        }

        void FixedUpdate()
        {
            if (!autoVolo) return;

            // Aggiustamento manuale altezza in auto volo
            float inputThrottle = 0f;
            if (Input.GetKey(KeyCode.Space)) inputThrottle = 1f;
            if (Input.GetKey(KeyCode.LeftShift)) inputThrottle = -1f;
            if (inputThrottle != 0f)
                rb.AddForce(Vector3.up * inputThrottle * 8f, ForceMode.Force);

            if (percorsoCompletato)
            {
                if (landingPad != null)
                    MuoviVerso(landingPad.position + Vector3.up * 1f);

                if (landingPad != null &&
                    Vector3.Distance(transform.position, landingPad.position) < waypointRadius)
                {
                    Debug.Log("Atterrato al LandingPad!");
                    autoVolo = false;
                    if (droneController != null)
                        droneController.enabled = true;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                return;
            }

            if (punti == null || punti.Length == 0) return;

            if (puntoCorrente >= punti.Length)
            {
                percorsoCompletato = true;
                Debug.Log("Percorso completato! Vado al LandingPad!");
                return;
            }

            MuoviVerso(punti[puntoCorrente]);

            float distanza = Vector3.Distance(transform.position, punti[puntoCorrente]);
            if (distanza < waypointRadius)
                puntoCorrente++;
        }

        void MuoviVerso(Vector3 targetPos)
        {
            Vector3 direzione = targetPos - transform.position;

            // Contrasta gravità
            float spintaBase = rb.mass * Physics.gravity.magnitude;
            rb.AddForce(Vector3.up * spintaBase, ForceMode.Force);

            // Movimento orizzontale
            Vector3 mossaOrizzontale = new Vector3(direzione.x, 0, direzione.z).normalized * velocita;
            rb.AddForce(mossaOrizzontale, ForceMode.Force);

            // Movimento verticale
            float diffY = direzione.y;
            rb.AddForce(Vector3.up * diffY * velocitaVerticale, ForceMode.Force);

            if (Mathf.Abs(diffY) < 0.3f)
                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x, 0, rb.linearVelocity.z);

            // Rotazione verso target
            Vector3 direzionePiana = new Vector3(direzione.x, 0, direzione.z);
            if (direzionePiana.magnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direzionePiana);
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    targetRot,
                    Time.fixedDeltaTime * velocitaRotazione
                );
            }
        }

        void RuotaElice()
        {
            if (propeller1) propeller1.Rotate(Vector3.up, propRotSpeed * Time.deltaTime);
            if (propeller2) propeller2.Rotate(Vector3.up, -propRotSpeed * Time.deltaTime);
            if (propeller3) propeller3.Rotate(Vector3.up, propRotSpeed * Time.deltaTime);
            if (propeller4) propeller4.Rotate(Vector3.up, -propRotSpeed * Time.deltaTime);
        }
    }
}