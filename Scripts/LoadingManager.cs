using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager instance;

    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI loadingText;

    // Tracks loading tasks: object -> completed?
    private Dictionary<GameObject, bool> loadingTasks = new Dictionary<GameObject, bool>();

    private float displayedProgress = 0f;
    private bool finishing = false;

    [SerializeField] private float minLoadTime = 2f; // Minimum time for slider to fill

    private float loadStartTime;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void StartLoading(GameObject ob)
    {
        if (!loadingTasks.ContainsKey(ob))
        {
            loadingTasks.Add(ob, false);
            Debug.Log($"Started loading task for {ob.name}");
            loadStartTime = Time.time; // Reset start time when first task starts
        }
    }

    public void StopLoading(GameObject ob)
    {
        if (loadingTasks.ContainsKey(ob))
        {
            loadingTasks[ob] = true;
            Debug.Log($"Completed task for {ob.name}");
        }
    }

    void Update()
    {
        if (finishing) return;

        // Calculate target progress based on tasks completed
        int totalTasks = loadingTasks.Count;
        int completedTasks = 0;
        foreach (var task in loadingTasks)
            if (task.Value) completedTasks++;

        float taskProgress = totalTasks == 0 ? 1f : (float)completedTasks / totalTasks;

        // Ensure minimum load time is respected
        float timeProgress = Mathf.Clamp01((Time.time - loadStartTime) / minLoadTime);

        float targetProgress = Mathf.Max(taskProgress, timeProgress);

        // Smoothly animate slider
        float fillSpeed = 1.5f; // Adjust for smoothness
        displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, Time.deltaTime * fillSpeed);

        slider.value = displayedProgress;
        loadingText.text = $"Loading... {Mathf.RoundToInt(displayedProgress * 100)}%";

        // Finish loading when slider visually reaches 100%
        if (completedTasks == totalTasks && Mathf.Approximately(displayedProgress, 1f))
        {
            StartCoroutine(FinishLoading());
        }
    }

    private IEnumerator FinishLoading()
    {
        finishing = true;

        loadingText.text = "Loading Complete!";
        slider.value = 1f;

        yield return new WaitForSeconds(1f); // Keep message visible

        loadingTasks.Clear();
        slider.value = 0f;

        if (GameManager.instance != null)
            GameManager.instance.ActiveGameContainers();

        Destroy(gameObject);
    }
}
