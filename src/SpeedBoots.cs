using UnityEngine;

namespace GorillaTagMod
{
    /// <summary>
    /// Provides speed boost functionality for Gorilla Tag
    /// Press 'SHIFT' to activate speed boost
    /// Press 'CTRL' to deactivate speed boost
    /// </summary>
    public class SpeedBoots : MonoBehaviour
    {
        private static SpeedBoots instance;

        [SerializeField] private float speedMultiplier = 2.0f;
        [SerializeField] private float maxSpeedBoostDuration = 10f;

        private const KeyCode ACTIVATE_KEY = KeyCode.LeftShift;
        private const KeyCode DEACTIVATE_KEY = KeyCode.LeftControl;

        private Rigidbody playerRigidbody;
        private CharacterController characterController;
        private Vector3 originalVelocity;
        private float currentSpeedBoostDuration = 0f;
        private bool isSpeedBoostActive = false;
        private float speedBoostStartTime = 0f;

        public static SpeedBoots Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SpeedBoots>();
                    if (instance == null)
                    {
                        GameObject speedBootsObject = new GameObject("SpeedBoots");
                        instance = speedBootsObject.AddComponent<SpeedBoots>();
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
            InitializePlayer();
            Debug.Log("SpeedBoots initialized!");
            Debug.Log($"Press '{ACTIVATE_KEY}' to activate speed boost (2x speed)");
            Debug.Log($"Press '{DEACTIVATE_KEY}' to deactivate speed boost");
        }

        void Update()
        {
            HandleSpeedBoostInput();
            UpdateSpeedBoost();
        }

        void FixedUpdate()
        {
            if (isSpeedBoostActive)
            {
                ApplySpeedBoost();
            }
        }

        /// <summary>
        /// Initialize player physics components
        /// </summary>
        private void InitializePlayer()
        {
            Transform playerTransform = GameObject.Find("Player")?.transform;
            
            if (playerTransform == null)
            {
                playerTransform = GameObject.Find("MainPlayer")?.transform;
            }

            if (playerTransform != null)
            {
                playerRigidbody = playerTransform.GetComponent<Rigidbody>();
                characterController = playerTransform.GetComponent<CharacterController>();

                if (playerRigidbody != null)
                {
                    Debug.Log("Found player Rigidbody!");
                }
                else if (characterController != null)
                {
                    Debug.Log("Found player CharacterController!");
                }
                else
                {
                    Debug.LogWarning("Could not find Rigidbody or CharacterController on player!");
                }
            }
            else
            {
                Debug.LogWarning("Could not find player in scene!");
            }
        }

        /// <summary>
        /// Handle speed boost activation/deactivation input
        /// </summary>
        private void HandleSpeedBoostInput()
        {
            if (Input.GetKeyDown(ACTIVATE_KEY))
            {
                ActivateSpeedBoost();
            }

            if (Input.GetKeyDown(DEACTIVATE_KEY))
            {
                DeactivateSpeedBoost();
            }
        }

        /// <summary>
        /// Activate the speed boost
        /// </summary>
        public void ActivateSpeedBoost()
        {
            if (!isSpeedBoostActive)
            {
                isSpeedBoostActive = true;
                speedBoostStartTime = Time.time;
                currentSpeedBoostDuration = maxSpeedBoostDuration;
                Debug.Log($"⚡ SPEED BOOST ACTIVATED! ({speedMultiplier}x speed for {maxSpeedBoostDuration}s)");
            }
        }

        /// <summary>
        /// Deactivate the speed boost
        /// </summary>
        public void DeactivateSpeedBoost()
        {
            if (isSpeedBoostActive)
            {
                isSpeedBoostActive = false;
                currentSpeedBoostDuration = 0f;
                Debug.Log("Speed boost deactivated");
            }
        }

        /// <summary>
        /// Update the speed boost duration
        /// </summary>
        private void UpdateSpeedBoost()
        {
            if (isSpeedBoostActive)
            {
                float elapsedTime = Time.time - speedBoostStartTime;
                currentSpeedBoostDuration = maxSpeedBoostDuration - elapsedTime;

                if (currentSpeedBoostDuration <= 0)
                {
                    DeactivateSpeedBoost();
                    Debug.Log("Speed boost duration expired!");
                }
                else if (elapsedTime % 2f < 0.1f) // Display remaining time every 2 seconds
                {
                    Debug.Log($"⚡ Speed boost active! {currentSpeedBoostDuration:F1}s remaining");
                }
            }
        }

        /// <summary>
        /// Apply speed boost to the player
        /// </summary>
        private void ApplySpeedBoost()
        {
            if (playerRigidbody != null)
            {
                // Increase velocity magnitude with speed multiplier
                Vector3 currentVelocity = playerRigidbody.velocity;
                float currentSpeed = currentVelocity.magnitude;
                
                if (currentSpeed > 0)
                {
                    Vector3 boostVelocity = currentVelocity.normalized * currentSpeed * speedMultiplier;
                    playerRigidbody.velocity = boostVelocity;
                }
            }
            else if (characterController != null && characterController.isGrounded)
            {
                // Alternative implementation for CharacterController
                // Note: CharacterController doesn't have direct velocity control
                // This is handled through movement input typically
                Debug.LogWarning("Speed boost works better with Rigidbody physics!");
            }
        }

        /// <summary>
        /// Set the speed multiplier
        /// </summary>
        public void SetSpeedMultiplier(float newMultiplier)
        {
            speedMultiplier = Mathf.Clamp(newMultiplier, 1f, 5f);
            Debug.Log($"Speed multiplier set to: {speedMultiplier}x");
        }

        /// <summary>
        /// Set the speed boost duration
        /// </summary>
        public void SetBoostDuration(float newDuration)
        {
            maxSpeedBoostDuration = Mathf.Clamp(newDuration, 1f, 60f);
            Debug.Log($"Speed boost duration set to: {maxSpeedBoostDuration}s");
        }

        // Getters
        public bool IsSpeedBoostActive() => isSpeedBoostActive;
        public float GetRemainingDuration() => currentSpeedBoostDuration;
        public float GetSpeedMultiplier() => speedMultiplier;
        public float GetMaxDuration() => maxSpeedBoostDuration;
    }
}
