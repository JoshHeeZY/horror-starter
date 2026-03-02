using System;
using UnityEngine;

/// <summary>
/// Singleton that owns the game state machine and orchestrates all other managers.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, GameOver, Win }

    public GameState CurrentState { get; private set; }

    public event Action<GameState> OnGameStateChanged;

    [SerializeField] private JumpscareController jumpscareController;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        CurrentState = GameState.Playing;
    }

    /// <summary>
    /// Transitions to a new game state and fires the OnGameStateChanged event.
    /// </summary>
    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;

            case GameState.GameOver:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                UIManager.Instance?.ShowGameOver();
                break;

            case GameState.Win:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                UIManager.Instance?.ShowWin();
                break;
        }

        OnGameStateChanged?.Invoke(newState);
    }

    /// <summary>
    /// Called by NoteCollectionManager when a note is picked up.
    /// Forwards aggression scaling to MonsterAI.
    /// </summary>
    public void OnNoteCollected()
    {
        MonsterAI monster = FindFirstObjectByType<MonsterAI>();
        monster?.OnNoteCollected(NoteCollectionManager.Instance.NotesCollected);
    }

    /// <summary>
    /// Called by MonsterAI when the player is caught. Starts the jumpscare sequence.
    /// </summary>
    public void OnPlayerCaught()
    {
        if (CurrentState != GameState.Playing) return;
        StartCoroutine(jumpscareController.PlayJumpscare());
    }

    /// <summary>
    /// Called by NoteCollectionManager when all notes are collected.
    /// </summary>
    public void OnAllNotesCollected()
    {
        ChangeState(GameState.Win);
    }
}
