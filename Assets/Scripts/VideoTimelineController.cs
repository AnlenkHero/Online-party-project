using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class VideoChoice
{
    public VideoClip videoClip;
    public List<VideoChoice> nextChoices;
}

public class VideoTimelineController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject choicesCanvas;
    public Button choiceButtonPrefab; // Reference to the button prefab
    public Transform choicesPanel; // Panel to hold the buttons
    public List<VideoChoice> videoChoices;

    private VideoChoice currentChoice;

    void Start()
    {
        if (videoChoices.Count > 0)
        {
            PlayVideoChoice(videoChoices[0]);
        }
    }

    void PlayVideoChoice(VideoChoice choice)
    {
        currentChoice = choice;
        videoPlayer.clip = choice.videoClip;
        videoPlayer.Play();
        videoPlayer.loopPointReached += OnMovieFinished;
    }

    void OnMovieFinished(VideoPlayer vp)
    {
        vp.loopPointReached -= OnMovieFinished;
        if (currentChoice.nextChoices.Count > 0)
        {
            ShowChoices();
        }
        else
        {
            EndGame();
        }
    }

    void ShowChoices()
    {
        foreach (Transform child in choicesPanel)
        {
            Destroy(child.gameObject); // Clear existing buttons
        }

        choicesCanvas.SetActive(true);
        
        for (int i = 0; i < currentChoice.nextChoices.Count; i++)
        {
            int choiceIndex = i; // Capture index for use in lambda expression
            Button button = Instantiate(choiceButtonPrefab, choicesPanel);
            button.gameObject.SetActive(true);
            // Set button text or add image here based on the choice
            button.onClick.AddListener(() => SelectChoice(choiceIndex));
        }
    }

    public void SelectChoice(int choiceIndex)
    {
        if (choiceIndex >= 0 && choiceIndex < currentChoice.nextChoices.Count)
        {
            choicesCanvas.SetActive(false);
            PlayVideoChoice(currentChoice.nextChoices[choiceIndex]);
        }
    }

    void EndGame()
    {
        Debug.Log("Game Ended");
        choicesCanvas.SetActive(false);
        // Implement game end logic here
    }
}
