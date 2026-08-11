using UnityEngine;
using System.Collections.Generic;

namespace GorillaTagMod
{
    /// <summary>
    /// Handles player height and arm length customization
    /// </summary>
    public class PlayerCustomization : MonoBehaviour
    {
        private static PlayerCustomization instance;

        [SerializeField] private float currentHeight = 1.7f;
        [SerializeField] private float currentArmLength = 1.0f;

        // Height settings
        private float minHeight = 0.5f;
        private float maxHeight = 3.0f;
        private float heightStep = 0.1f;

        // Arm length settings
        private float minArmLength = 0.5f;
        private float maxArmLength = 2.5f;
        private float armLengthStep = 0.1f;

        private Transform leftArm;
        private Transform rightArm;
        private Transform head;
        private Transform body;

        private Vector3 originalLeftArmScale;
        private Vector3 originalRightArmScale;
        private Vector3 originalBodyScale;

        public static PlayerCustomization Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<PlayerCustomization>();
                    if (instance == null)
                    {
                        GameObject customObject = new GameObject("PlayerCustomization");
                        instance = customObject.AddComponent<PlayerCustomization>();
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
        }

        void Start()
        {
            InitializePlayerReferences();
            ApplyCustomization();
            Debug.Log("PlayerCustomization initialized!");
        }

        void Update()
        {
            HandleHeightInput();
            HandleArmLengthInput();
        }

        /// <summary>
        /// Initialize references to player body parts
        /// </summary>
        private void InitializePlayerReferences()
        {
            // Try to find the player's body parts
            // These names might need adjustment based on your game's hierarchy
            Transform playerTransform = GameObject.Find("Player")?.transform;
            
            if (playerTransform == null)
            {
                // Try alternative names
                playerTransform = GameObject.Find("MainPlayer")?.transform;
            }

            if (playerTransform != null)
            {
                body = playerTransform;
                head = body.Find("Head") ?? body.Find("Camera");
                leftArm = body.Find("LeftHand") ?? body.Find("LeftArm");
                rightArm = body.Find("RightHand") ?? body.Find("RightArm");

                if (leftArm != null && rightArm != null)
                {
                    originalLeftArmScale = leftArm.localScale;
                    originalRightArmScale = rightArm.localScale;
                }

                if (body != null)
                {
                    originalBodyScale = body.localScale;
                }

                Debug.Log("Player references initialized successfully!");
            }
            else
            {
                Debug.LogWarning("Could not find player in scene!");
            }
        }

        /// <summary>
        /// Handle height adjustment input (Arrow Up/Down or W/S)
        /// </summary>
        private void HandleHeightInput()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                IncreaseHeight();
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                DecreaseHeight();
            }
        }

        /// <summary>
        /// Handle arm length adjustment input (Left/Right arrows or A/D)
        /// </summary>
        private void HandleArmLengthInput()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                IncreaseArmLength();
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                DecreaseArmLength();
            }
        }

        /// <summary>
        /// Increase player height
        /// </summary>
        public void IncreaseHeight()
        {
            if (currentHeight < maxHeight)
            {
                currentHeight += heightStep;
                ApplyHeight();
                Debug.Log($"Height increased to: {currentHeight:F1}m");
            }
        }

        /// <summary>
        /// Decrease player height
        /// </summary>
        public void DecreaseHeight()
        {
            if (currentHeight > minHeight)
            {
                currentHeight -= heightStep;
                ApplyHeight();
                Debug.Log($"Height decreased to: {currentHeight:F1}m");
            }
        }

        /// <summary>
        /// Increase arm length
        /// </summary>
        public void IncreaseArmLength()
        {
            if (currentArmLength < maxArmLength)
            {
                currentArmLength += armLengthStep;
                ApplyArmLength();
                Debug.Log($"Arm length increased to: {currentArmLength:F1}x");
            }
        }

        /// <summary>
        /// Decrease arm length
        /// </summary>
        public void DecreaseArmLength()
        {
            if (currentArmLength > minArmLength)
            {
                currentArmLength -= armLengthStep;
                ApplyArmLength();
                Debug.Log($"Arm length decreased to: {currentArmLength:F1}x");
            }
        }

        /// <summary>
        /// Apply all customizations
        /// </summary>
        public void ApplyCustomization()
        {
            ApplyHeight();
            ApplyArmLength();
        }

        /// <summary>
        /// Apply height scaling to the player
        /// </summary>
        private void ApplyHeight()
        {
            if (body != null)
            {
                float heightMultiplier = currentHeight / 1.7f; // 1.7m is default height
                body.localScale = originalBodyScale * heightMultiplier;
            }
        }

        /// <summary>
        /// Apply arm length scaling
        /// </summary>
        private void ApplyArmLength()
        {
            if (leftArm != null)
            {
                leftArm.localScale = originalLeftArmScale * currentArmLength;
            }

            if (rightArm != null)
            {
                rightArm.localScale = originalRightArmScale * currentArmLength;
            }
        }

        // Getters and setters
        public float GetHeight() => currentHeight;
        public float GetArmLength() => currentArmLength;

        public void SetHeight(float newHeight)
        {
            currentHeight = Mathf.Clamp(newHeight, minHeight, maxHeight);
            ApplyHeight();
        }

        public void SetArmLength(float newLength)
        {
            currentArmLength = Mathf.Clamp(newLength, minArmLength, maxArmLength);
            ApplyArmLength();
        }

        public float GetMinHeight() => minHeight;
        public float GetMaxHeight() => maxHeight;
        public float GetMinArmLength() => minArmLength;
        public float GetMaxArmLength() => maxArmLength;
    }
}
