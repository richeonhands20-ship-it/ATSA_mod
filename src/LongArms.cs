using UnityEngine;

namespace GorillaTagMod
{
    /// <summary>
    /// Long Arms Mod - Extend your arm reach for better gameplay
    /// Adjustable arm length from 0.5x to 3x normal length
    /// </summary>
    public class LongArms : MonoBehaviour
    {
        private static LongArms instance;

        private const KeyCode TOGGLE_KEY = KeyCode.A;
        private const KeyCode INCREASE_KEY = KeyCode.E;
        private const KeyCode DECREASE_KEY = KeyCode.Q;

        [SerializeField] private bool isLongArmsEnabled = false;
        [SerializeField] private float armLengthMultiplier = 1.5f;
        [SerializeField] private float minArmLength = 0.5f;
        [SerializeField] private float maxArmLength = 3f;
        [SerializeField] private float adjustmentSpeed = 0.1f;

        private Transform leftHandTransform;
        private Transform rightHandTransform;
        private Transform playerTransform;

        private Vector3 originalLeftHandLocalPos;
        private Vector3 originalRightHandLocalPos;

        public static LongArms Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<LongArms>();
                    if (instance == null)
                    {
                        GameObject longArmsObject = new GameObject("LongArms");
                        instance = longArmsObject.AddComponent<LongArms>();
                    }
                }
                return instance;
            }
        }

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            InitializeHands();
            Debug.Log("LongArms initialized!");
            Debug.Log($"Press '{TOGGLE_KEY}' to toggle long arms");
            Debug.Log($"Press '{INCREASE_KEY}' to increase arm length");
            Debug.Log($"Press '{DECREASE_KEY}' to decrease arm length");
            Debug.Log("Status: DISABLED (Press A to enable)");
        }

        void Update()
        {
            HandleInput();

            if (isLongArmsEnabled)
            {
                ApplyLongArms();
            }
        }

        /// <summary>
        /// Initialize hand transforms
        /// </summary>
        private void InitializeHands()
        {
            // Try to find OVRCameraRig for VR hands
            OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
            if (cameraRig != null)
            {
                leftHandTransform = cameraRig.leftHandAnchor;
                rightHandTransform = cameraRig.rightHandAnchor;
                playerTransform = cameraRig.centerEyeAnchor;

                if (leftHandTransform != null)
                {
                    originalLeftHandLocalPos = leftHandTransform.localPosition;
                }
                if (rightHandTransform != null)
                {
                    originalRightHandLocalPos = rightHandTransform.localPosition;
                }

                Debug.Log("Hand transforms initialized from OVRCameraRig!");
                return;
            }

            // Fallback: Find hands by name
            Transform[] allTransforms = FindObjectsOfType<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t.name.Contains("LeftHand") || t.name.Contains("Left Hand"))
                {
                    leftHandTransform = t;
                    originalLeftHandLocalPos = t.localPosition;
                }
                if (t.name.Contains("RightHand") || t.name.Contains("Right Hand"))
                {
                    rightHandTransform = t;
                    originalRightHandLocalPos = t.localPosition;
                }
            }

            if (leftHandTransform != null && rightHandTransform != null)
            {
                Debug.Log("Hand transforms found by name!");
            }
            else
            {
                Debug.LogWarning("Could not find hand transforms!");
            }

            // Find player
            playerTransform = GameObject.Find("Player")?.transform;
            if (playerTransform == null)
            {
                playerTransform = GameObject.Find("MainPlayer")?.transform;
            }
            if (playerTransform == null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    playerTransform = mainCamera.transform;
                }
            }
        }

        /// <summary>
        /// Handle input for arm length controls
        /// </summary>
        private void HandleInput()
        {
            if (Input.GetKeyDown(TOGGLE_KEY))
            {
                ToggleLongArms();
            }

            if (isLongArmsEnabled)
            {
                if (Input.GetKeyDown(INCREASE_KEY))
                {
                    IncreaseArmLength();
                }

                if (Input.GetKeyDown(DECREASE_KEY))
                {
                    DecreaseArmLength();
                }
            }
        }

        /// <summary>
        /// Toggle long arms on/off
        /// </summary>
        public void ToggleLongArms()
        {
            isLongArmsEnabled = !isLongArmsEnabled;

            if (isLongArmsEnabled)
            {
                Debug.Log($"💪 LONG ARMS ENABLED! Arm length: {armLengthMultiplier}x");
                Debug.Log($"Press '{INCREASE_KEY}' to increase | Press '{DECREASE_KEY}' to decrease");
            }
            else
            {
                Debug.Log("Long arms DISABLED - arms returned to normal length");
                ResetArmLength();
            }
        }

        /// <summary>
        /// Increase arm length
        /// </summary>
        private void IncreaseArmLength()
        {
            armLengthMultiplier = Mathf.Min(armLengthMultiplier + adjustmentSpeed, maxArmLength);
            Debug.Log($"Arm length increased: {armLengthMultiplier:F2}x");
        }

        /// <summary>
        /// Decrease arm length
        /// </summary>
        private void DecreaseArmLength()
        {
            armLengthMultiplier = Mathf.Max(armLengthMultiplier - adjustmentSpeed, minArmLength);
            Debug.Log($"Arm length decreased: {armLengthMultiplier:F2}x");
        }

        /// <summary>
        /// Apply long arms effect by stretching hand positions
        /// </summary>
        private void ApplyLongArms()
        {
            if (playerTransform == null)
            {
                return;
            }

            // Extend left hand
            if (leftHandTransform != null)
            {
                Vector3 handDirection = (leftHandTransform.position - playerTransform.position).normalized;
                float baseDistance = Vector3.Distance(leftHandTransform.position, playerTransform.position);
                float extendedDistance = baseDistance * armLengthMultiplier;

                leftHandTransform.position = playerTransform.position + handDirection * extendedDistance;
            }

            // Extend right hand
            if (rightHandTransform != null)
            {
                Vector3 handDirection = (rightHandTransform.position - playerTransform.position).normalized;
                float baseDistance = Vector3.Distance(rightHandTransform.position, playerTransform.position);
                float extendedDistance = baseDistance * armLengthMultiplier;

                rightHandTransform.position = playerTransform.position + handDirection * extendedDistance;
            }
        }

        /// <summary>
        /// Reset arms to normal length
        /// </summary>
        private void ResetArmLength()
        {
            armLengthMultiplier = 1.5f;

            if (leftHandTransform != null)
            {
                leftHandTransform.localPosition = originalLeftHandLocalPos;
            }

            if (rightHandTransform != null)
            {
                rightHandTransform.localPosition = originalRightHandLocalPos;
            }
        }

        // Getters
        public bool IsLongArmsEnabled() => isLongArmsEnabled;
        public float GetArmLengthMultiplier() => armLengthMultiplier;

        /// <summary>
        /// Set arm length multiplier
        /// </summary>
        public void SetArmLength(float newMultiplier)
        {
            armLengthMultiplier = Mathf.Clamp(newMultiplier, minArmLength, maxArmLength);
            Debug.Log($"Arm length set to: {armLengthMultiplier:F2}x");
        }
    }
}
