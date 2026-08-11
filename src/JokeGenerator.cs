using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace GorillaTagMod
{
    /// <summary>
    /// Generates random jokes using the JokeAPI external service
    /// Press 'J' to display a random joke in the console
    /// </summary>
    public class JokeGenerator : MonoBehaviour
    {
        private static JokeGenerator instance;
        
        private const string JOKE_API_URL = "https://v2.jokeapi.dev/joke/Any?format=json";
        private const KeyCode JOKE_KEY = KeyCode.J;

        private bool isLoadingJoke = false;
        private float lastJokeTime = -5f;
        private float jokeRequestDelay = 1f; // Prevent spam requests

        public static JokeGenerator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<JokeGenerator>();
                    if (instance == null)
                    {
                        GameObject jokeObject = new GameObject("JokeGenerator");
                        instance = jokeObject.AddComponent<JokeGenerator>();
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
            Debug.Log("JokeGenerator initialized! Press 'J' to get a random joke.");
        }

        void Update()
        {
            if (Input.GetKeyDown(JOKE_KEY) && !isLoadingJoke && Time.time - lastJokeTime >= jokeRequestDelay)
            {
                StartCoroutine(FetchJoke());
                lastJokeTime = Time.time;
            }
        }

        /// <summary>
        /// Fetches a random joke from the JokeAPI
        /// </summary>
        private IEnumerator FetchJoke()
        {
            isLoadingJoke = true;
            Debug.Log("Fetching joke...");

            using (UnityWebRequest request = UnityWebRequest.Get(JOKE_API_URL))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    ParseAndDisplayJoke(jsonResponse);
                }
                else
                {
                    Debug.LogError($"Failed to fetch joke: {request.error}");
                    DisplayFallbackJoke();
                }
            }

            isLoadingJoke = false;
        }

        /// <summary>
        /// Parses the JSON response and displays the joke
        /// </summary>
        private void ParseAndDisplayJoke(string jsonResponse)
        {
            try
            {
                JokeData jokeData = JsonUtility.FromJson<JokeData>(jsonResponse);

                if (jokeData.type == "single")
                {
                    // Single-line joke
                    DisplayJoke(jokeData.joke);
                }
                else if (jokeData.type == "twopart")
                {
                    // Two-part joke (setup and delivery)
                    string fullJoke = $"Setup: {jokeData.setup}\n\nPunchline: {jokeData.delivery}";
                    DisplayJoke(fullJoke);
                }
                else
                {
                    Debug.LogWarning("Unknown joke type: " + jokeData.type);
                    DisplayFallbackJoke();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error parsing joke JSON: {ex.Message}");
                DisplayFallbackJoke();
            }
        }

        /// <summary>
        /// Displays a joke to the console with formatting
        /// </summary>
        private void DisplayJoke(string joke)
        {
            Debug.Log("╔════════════════════════════════════════╗");
            Debug.Log("║           🤣 RANDOM JOKE 🤣           ║");
            Debug.Log("╠════════════════════════════════════════╣");
            Debug.Log($"║ {joke}");
            Debug.Log("╚════════════════════════════════════════╝");
        }

        /// <summary>
        /// Displays a fallback joke if API fails
        /// </summary>
        private void DisplayFallbackJoke()
        {
            string[] fallbackJokes = new string[]
            {
                "Why did the chicken cross the road?\nTo get to the other side!",
                "What do you call a bear with no teeth?\nA gummy bear!",
                "Why don't scientists trust atoms?\nBecause they make up everything!",
                "What did the ocean say to the beach?\nNothing, it just waved!",
                "Why did the scarecrow win an award?\nBecause he was outstanding in his field!",
                "What do you call a fake noodle?\nAn impasta!",
                "Why don't eggs tell jokes?\nThey'd crack up!",
                "What do you call a sleeping bull?\nA bulldozer!"
            };

            string randomFallback = fallbackJokes[Random.Range(0, fallbackJokes.Length)];
            DisplayJoke(randomFallback);
        }

        /// <summary>
        /// JSON structure for JokeAPI response
        /// </summary>
        [System.Serializable]
        private class JokeData
        {
            public string type;
            public string joke; // For single-line jokes
            public string setup; // For two-part jokes
            public string delivery; // For two-part jokes
        }
    }
}
