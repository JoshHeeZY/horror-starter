using System;
using UnityEngine;

/// <summary>
/// Singleton that tracks note collection progress, notifies GameManager,
/// and triggers monster aggression scaling.
/// </summary>
public class NoteCollectionManager : MonoBehaviour
{
    public static NoteCollectionManager Instance { get; private set; }

    public int NotesCollected { get; private set; }

    [Tooltip("Total number of notes in the scene. Set to 8.")]
    public int TotalNotes = 8;

    public event Action<int> OnNoteCountChanged;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Registers a collected note, fires events, and checks the win condition.
    /// </summary>
    public void CollectNote(NotePickup note)
    {
        NotesCollected++;

        OnNoteCountChanged?.Invoke(NotesCollected);
        UIManager.Instance?.UpdateNoteCounter(NotesCollected, TotalNotes);
        GameManager.Instance?.OnNoteCollected();

        if (NotesCollected >= TotalNotes)
        {
            GameManager.Instance?.OnAllNotesCollected();
        }
    }
}
