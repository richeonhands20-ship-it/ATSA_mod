using UnityEngine;
using System.Collections.Generic;

namespace GorillaTagMod
{
    /// <summary>
    /// Creates disappearing platforms that fade away after standing on them
    /// Press 'M' to toggle platform creation mode on/off
    /// Click to place platforms
    /// </summary>
    public class DisappearingPlatforms : MonoBehaviour
    {
        private static DisappearingPlatforms instance;

        private const KeyCode TOGGLE_MODE_KEY = KeyCode.M;
        private const KeyCode CREATE_KEY = KeyCode.Mouse0;

        [SerializeField] private bool isPlatformModeEnabled = false;
        [SerializeField] private float platformSize = 2f;
        [SerializeField] private float platformHeight = 0.5f;
        [SerializeField] private float disappearDelay = 3f;
        [SerializeField] private float fadeOutDuration = 1f;
        [SerializeField] private Material platformMaterial;

        private List<GameObject> activePlatforms = new List<GameObject>();
        private Transform playerTransform;
        private Camera mainCamera;
        private int platformCount = 0;

        public static DisappearingPlatforms Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<DisappearingPlatforms>();
                    if (instance == null)
                    {
                        GameObject platformObject = new GameObject("DisappearingPlatforms");
                        instance = platformObject.AddComponent<DisappearingPlatforms>();
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
            CreatePlatformMaterial();
            Debug.Log("DisappearingPlatforms initialized!");
            Debug.Log($"Press '{TOGGLE_MODE_KEY}' to toggle platform creation mode");
            Debug.Log("Status: DISABLED (Press M to enable)");
        }

        void Update()
        {
            HandleModeToggle();

            if (isPlatformModeEnabled)
            {
                HandlePlatformCreation();
                UpdateActivePlatforms();
            }
        }

        /// <summary>
        /// Initialize player and camera references
        /// </summary>
        private void InitializePlayer()
        {
            playerTransform = GameObject.Find("Player")?.transform;

            if (playerTransform == null)
            {
                playerTransform = GameObject.Find("MainPlayer")?.transform;
            }

            if (playerTransform != null)
            {
                mainCamera = playerTransform.GetComponentInChildren<Camera>();
                Debug.Log("Player and camera initialized!");
            }
            else
            {
                Debug.LogWarning("Could not find player in scene!");
            }
        }

        /// <summary>
        /// Create a material for platforms
        /// </summary>
        private void CreatePlatformMaterial()
        {
            if (platformMaterial == null)
            {
                platformMaterial = new Material(Shader.Find("Standard"));
                platformMaterial.color = new Color(0f, 1f, 1f, 0.8f); // Cyan with transparency
                platformMaterial.SetFloat("_Mode", 3); // Transparent
                platformMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                platformMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                platformMaterial.SetInt("_ZWrite", 0);
                platformMaterial.DisableKeyword("_ALPHATEST_ON");
                platformMaterial.EnableKeyword("_ALPHABLEND_ON");
                platformMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                platformMaterial.renderQueue = 3000;
            }
        }

        /// <summary>
        /// Handle platform mode toggle
        /// </summary>
        private void HandleModeToggle()
        {
            if (Input.GetKeyDown(TOGGLE_MODE_KEY))
            {
                TogglePlatformMode();
            }
        }

        /// <summary>
        /// Toggle platform creation mode on/off
        /// </summary>
        public void TogglePlatformMode()
        {
            isPlatformModeEnabled = !isPlatformModeEnabled;

            if (isPlatformModeEnabled)
            {
                Debug.Log("🛟 PLATFORM CREATION MODE ENABLED!");
                Debug.Log("Click (Left Mouse) to spawn disappearing platforms");
                Debug.Log($"Platforms disappear after {disappearDelay}s");
            }
            else
            {
                Debug.Log("Platform creation mode DISABLED");
            }
        }

        /// <summary>
        /// Handle platform creation input
        /// </summary>
        private void HandlePlatformCreation()
        {
            if (Input.GetKeyDown(CREATE_KEY) && mainCamera != null)
            {
                // Raycast from camera to find where to place platform
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                // Place platform in front of camera
                Vector3 spawnPosition = ray.origin + ray.direction * 5f;

                CreatePlatform(spawnPosition);
            }
        }

        /// <summary>
        /// Create a disappearing platform at the specified position
        /// </summary>
        private void CreatePlatform(Vector3 position)
        {
            platformCount++;
            GameObject platformObj = new GameObject($"Platform_{platformCount}");
            platformObj.transform.position = position;

            // Create cube for platform
            GameObject cubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubeObj.name = "PlatformMesh";
            cubeObj.transform.SetParent(platformObj.transform);
            cubeObj.transform.localPosition = Vector3.zero;
            cubeObj.transform.localScale = new Vector3(platformSize, platformHeight, platformSize);

            // Remove collider from primitive (we'll add our own)
            Collider primCollider = cubeObj.GetComponent<Collider>();
            if (primCollider != null)
            {
                DestroyImmediate(primCollider);
            }

            // Set material
            MeshRenderer renderer = cubeObj.GetComponent<MeshRenderer>();
            renderer.material = platformMaterial;

            // Add collider to parent
            BoxCollider boxCollider = platformObj.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(platformSize, platformHeight, platformSize);
            boxCollider.isTrigger = false;

            // Add rigidbody
            Rigidbody rb = platformObj.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;

            // Add platform script
            DisappearingPlatformScript platformScript = platformObj.AddComponent<DisappearingPlatformScript>();
            platformScript.Initialize(disappearDelay, fadeOutDuration, renderer);

            activePlatforms.Add(platformObj);

            Debug.Log($"✅ Platform created at {position}. Will disappear in {disappearDelay}s");
        }

        /// <summary>
        /// Update active platforms - remove destroyed ones
        /// </summary>
        private void UpdateActivePlatforms()
        {
            activePlatforms.RemoveAll(p => p == null);
        }

        /// <summary>
        /// Clear all active platforms
        /// </summary>
        public void ClearAllPlatforms()
        {
            foreach (GameObject platform in activePlatforms)
            {
                if (platform != null)
                {
                    Destroy(platform);
                }
            }
            activePlatforms.Clear();
            Debug.Log("All platforms cleared!");
        }

        // Getters
        public bool IsPlatformModeEnabled() => isPlatformModeEnabled;
        public int GetActivePlatformCount() => activePlatforms.Count;
        public float GetPlatformSize() => platformSize;
        public float GetDisappearDelay() => disappearDelay;

        /// <summary>
        /// Set disappear delay
        /// </summary>
        public void SetDisappearDelay(float newDelay)
        {
            disappearDelay = Mathf.Clamp(newDelay, 0.5f, 30f);
            Debug.Log($"Disappear delay set to: {disappearDelay}s");
        }

        /// <summary>
        /// Set fade out duration
        /// </summary>
        public void SetFadeOutDuration(float newDuration)
        {
            fadeOutDuration = Mathf.Clamp(newDuration, 0.1f, 5f);
            Debug.Log($"Fade out duration set to: {fadeOutDuration}s");
        }

        /// <summary>
        /// Set platform size
        /// </summary>
        public void SetPlatformSize(float newSize)
        {
            platformSize = Mathf.Clamp(newSize, 0.5f, 5f);
            Debug.Log($"Platform size set to: {platformSize}");
        }
    }

    /// <summary>
    /// Individual platform behavior script
    /// </summary>
    public class DisappearingPlatformScript : MonoBehaviour
    {
        private float disappearDelay = 3f;
        private float fadeOutDuration = 1f;
        private float creationTime = 0f;
        private MeshRenderer meshRenderer;
        private bool hasBeenTouched = false;
        private float touchTime = 0f;

        public void Initialize(float delay, float fadeDuration, MeshRenderer renderer)
        {
            disappearDelay = delay;
            fadeOutDuration = fadeDuration;
            meshRenderer = renderer;
            creationTime = Time.time;
        }

        void Start()
        {
            if (meshRenderer == null)
            {
                meshRenderer = GetComponentInChildren<MeshRenderer>();
            }
        }

        void Update()
        {
            // Check if player is standing on this platform
            if (!hasBeenTouched && IsPlayerOnPlatform())
            {
                hasBeenTouched = true;
                touchTime = Time.time;
                Debug.Log($"⚠️ Platform touched! Will disappear in {disappearDelay}s");
            }

            // Handle disappearance
            if (hasBeenTouched)
            {
                float timeSinceTouched = Time.time - touchTime;

                // Check if we should start fading
                if (timeSinceTouched >= disappearDelay)
                {
                    float fadeTime = timeSinceTouched - disappearDelay;

                    if (fadeTime >= fadeOutDuration)
                    {
                        // Completely disappeared
                        Destroy(gameObject);
                    }
                    else
                    {
                        // Fade out
                        FadeOut(fadeTime / fadeOutDuration);
                    }
                }
            }
        }

        /// <summary>
        /// Check if player is standing on this platform
        /// </summary>
        private bool IsPlayerOnPlatform()
        {
            Collider[] colliders = Physics.OverlapBox(
                transform.position,
                transform.localScale * 0.5f
            );

            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.name.Contains("Player") || 
                    collider.gameObject.name.Contains("Gorilla") ||
                    collider.gameObject.name.Contains("MainPlayer"))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Fade out the platform
        /// </summary>
        private void FadeOut(float fadeProgress)
        {
            if (meshRenderer != null)
            {
                Color color = meshRenderer.material.color;
                color.a = Mathf.Lerp(0.8f, 0f, fadeProgress);
                meshRenderer.material.color = color;
            }
        }
    }
}
