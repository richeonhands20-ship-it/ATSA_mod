using UnityEngine;

namespace GorillaTagMod
{
    /// <summary>
    /// Creates a glowing aura effect around the player in Gorilla Tag
    /// Press 'A' to toggle the aura on/off
    /// </summary>
    public class TagAura : MonoBehaviour
    {
        private static TagAura instance;

        private const KeyCode TOGGLE_AURA_KEY = KeyCode.A;

        [SerializeField] private float auraRadius = 2f;
        [SerializeField] private float auraIntensity = 1.5f;
        [SerializeField] private Color auraColor = new Color(0f, 1f, 1f, 0.8f); // Cyan

        private bool isAuraActive = false;
        private GameObject auraObject;
        private Light auraLight;
        private ParticleSystem auraParticles;
        private Transform playerTransform;

        public static TagAura Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<TagAura>();
                    if (instance == null)
                    {
                        GameObject auraGameObject = new GameObject("TagAura");
                        instance = auraGameObject.AddComponent<TagAura>();
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
            Debug.Log("TagAura initialized!");
            Debug.Log($"Press '{TOGGLE_AURA_KEY}' to toggle the aura effect");
        }

        void Update()
        {
            HandleAuraInput();
            UpdateAuraPosition();
        }

        /// <summary>
        /// Initialize player reference
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
                Debug.Log("Player found! Ready to apply aura.");
            }
            else
            {
                Debug.LogWarning("Could not find player in scene!");
            }
        }

        /// <summary>
        /// Handle aura toggle input
        /// </summary>
        private void HandleAuraInput()
        {
            if (Input.GetKeyDown(TOGGLE_AURA_KEY))
            {
                ToggleAura();
            }
        }

        /// <summary>
        /// Toggle the aura on/off
        /// </summary>
        public void ToggleAura()
        {
            if (!isAuraActive)
            {
                ActivateAura();
            }
            else
            {
                DeactivateAura();
            }
        }

        /// <summary>
        /// Activate the aura effect
        /// </summary>
        public void ActivateAura()
        {
            if (isAuraActive || playerTransform == null)
            {
                return;
            }

            isAuraActive = true;

            // Create aura object
            auraObject = new GameObject("PlayerAura");
            auraObject.transform.SetParent(playerTransform);
            auraObject.transform.localPosition = Vector3.zero;

            // Add light component for glow effect
            auraLight = auraObject.AddComponent<Light>();
            auraLight.type = LightType.Point;
            auraLight.range = auraRadius;
            auraLight.intensity = auraIntensity;
            auraLight.color = auraColor;

            // Add a visual sphere mesh for visual feedback
            AddAuraMesh();

            // Add particle effect
            AddAuraParticles();

            Debug.Log("✨ Tag Aura ACTIVATED!");
            Debug.Log($"Aura Color: RGB({auraColor.r * 255}, {auraColor.g * 255}, {auraColor.b * 255})");
        }

        /// <summary>
        /// Deactivate the aura effect
        /// </summary>
        public void DeactivateAura()
        {
            if (!isAuraActive)
            {
                return;
            }

            isAuraActive = false;

            if (auraObject != null)
            {
                Destroy(auraObject);
            }

            Debug.Log("Tag Aura deactivated");
        }

        /// <summary>
        /// Add a visual mesh to the aura
        /// </summary>
        private void AddAuraMesh()
        {
            GameObject meshObject = new GameObject("AuraMesh");
            meshObject.transform.SetParent(auraObject.transform);
            meshObject.transform.localPosition = Vector3.zero;

            // Create a sphere to visualize the aura
            MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateSphereMesh(auraRadius, 16, 16);

            MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();

            // Create a glowing material
            Material auraMaterial = new Material(Shader.Find("Standard"));
            auraMaterial.SetFloat("_Mode", 3); // Transparent mode
            auraMaterial.color = auraColor;
            auraMaterial.SetFloat("_Glossiness", 0.3f);

            meshRenderer.material = auraMaterial;

            // Make it semi-transparent
            Color transparentColor = auraColor;
            transparentColor.a = 0.3f;
            auraMaterial.color = transparentColor;
        }

        /// <summary>
        /// Add particle effects to the aura
        /// </summary>
        private void AddAuraParticles()
        {
            GameObject particleObject = new GameObject("AuraParticles");
            particleObject.transform.SetParent(auraObject.transform);
            particleObject.transform.localPosition = Vector3.zero;

            auraParticles = particleObject.AddComponent<ParticleSystem>();

            // Configure particle system
            ParticleSystem.MainModule mainModule = auraParticles.main;
            mainModule.loop = true;
            mainModule.duration = 1f;
            mainModule.startLifetime = 2f;
            mainModule.startSpeed = 0.5f;
            mainModule.startColor = auraColor;
            mainModule.maxParticles = 50;

            // Emission
            ParticleSystem.EmissionModule emission = auraParticles.emission;
            emission.rateOverTime = 25f;

            // Shape
            ParticleSystem.ShapeModule shape = auraParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = auraRadius;

            // Size over lifetime
            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = auraParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;

            AnimationCurve sizeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            Debug.Log("Particle effect added to aura!");
        }

        /// <summary>
        /// Update aura position to follow player
        /// </summary>
        private void UpdateAuraPosition()
        {
            if (isAuraActive && auraObject != null && playerTransform != null)
            {
                auraObject.transform.position = playerTransform.position;
            }
        }

        /// <summary>
        /// Create a simple sphere mesh
        /// </summary>
        private Mesh CreateSphereMesh(float radius, int latitudes, int longitudes)
        {
            Mesh mesh = new Mesh();

            int vertexCount = (latitudes - 1) * longitudes + 2;
            Vector3[] vertices = new Vector3[vertexCount];
            int[] triangles = new int[(latitudes - 2) * longitudes * 6 + longitudes * 6];

            // Top vertex
            vertices[0] = Vector3.up * radius;

            // Create latitude rings
            for (int lat = 1; lat < latitudes; lat++)
            {
                float phi = Mathf.PI * lat / latitudes;
                float sinPhi = Mathf.Sin(phi);
                float cosPhi = Mathf.Cos(phi);

                for (int lon = 0; lon < longitudes; lon++)
                {
                    float theta = 2 * Mathf.PI * lon / longitudes;
                    float sinTheta = Mathf.Sin(theta);
                    float cosTheta = Mathf.Cos(theta);

                    int index = 1 + (lat - 1) * longitudes + lon;
                    vertices[index] = new Vector3(
                        sinPhi * cosTheta * radius,
                        cosPhi * radius,
                        sinPhi * sinTheta * radius
                    );
                }
            }

            // Bottom vertex
            vertices[vertexCount - 1] = Vector3.down * radius;

            // Create triangles
            int triIndex = 0;

            // Top cap
            for (int lon = 0; lon < longitudes; lon++)
            {
                triangles[triIndex++] = 0;
                triangles[triIndex++] = 1 + (lon + 1) % longitudes;
                triangles[triIndex++] = 1 + lon;
            }

            // Middle sections
            for (int lat = 1; lat < latitudes - 1; lat++)
            {
                for (int lon = 0; lon < longitudes; lon++)
                {
                    int a = 1 + (lat - 1) * longitudes + lon;
                    int b = 1 + (lat - 1) * longitudes + (lon + 1) % longitudes;
                    int c = 1 + lat * longitudes + lon;
                    int d = 1 + lat * longitudes + (lon + 1) % longitudes;

                    triangles[triIndex++] = a;
                    triangles[triIndex++] = c;
                    triangles[triIndex++] = b;

                    triangles[triIndex++] = b;
                    triangles[triIndex++] = c;
                    triangles[triIndex++] = d;
                }
            }

            // Bottom cap
            int bottomVertex = vertexCount - 1;
            int lastLatitudeStart = 1 + (latitudes - 2) * longitudes;

            for (int lon = 0; lon < longitudes; lon++)
            {
                triangles[triIndex++] = lastLatitudeStart + lon;
                triangles[triIndex++] = lastLatitudeStart + (lon + 1) % longitudes;
                triangles[triIndex++] = bottomVertex;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        /// <summary>
        /// Set aura color
        /// </summary>
        public void SetAuraColor(Color newColor)
        {
            auraColor = newColor;
            if (auraLight != null)
            {
                auraLight.color = newColor;
            }
            Debug.Log($"Aura color changed to: {newColor}");
        }

        /// <summary>
        /// Set aura radius
        /// </summary>
        public void SetAuraRadius(float newRadius)
        {
            auraRadius = Mathf.Clamp(newRadius, 0.5f, 5f);
            if (auraLight != null)
            {
                auraLight.range = auraRadius;
            }
            Debug.Log($"Aura radius set to: {auraRadius}");
        }

        /// <summary>
        /// Set aura intensity
        /// </summary>
        public void SetAuraIntensity(float newIntensity)
        {
            auraIntensity = Mathf.Clamp(newIntensity, 0.5f, 3f);
            if (auraLight != null)
            {
                auraLight.intensity = auraIntensity;
            }
            Debug.Log($"Aura intensity set to: {auraIntensity}");
        }

        // Getters
        public bool IsAuraActive() => isAuraActive;
        public float GetAuraRadius() => auraRadius;
        public float GetAuraIntensity() => auraIntensity;
        public Color GetAuraColor() => auraColor;
    }
}
