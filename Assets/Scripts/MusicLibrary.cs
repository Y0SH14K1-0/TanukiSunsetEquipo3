using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "MusicLibrary", menuName = "TanukiSunset/MusicLibrary", order = 1)]
public class MusicLibrary : ScriptableObject
{
    [Reorderable] public List<Song> songs;

    private Queue<Song> playlistQueue;

    public Song GetSongWithId(string id)
    {
        return songs.Where(x => x.id == id).FirstOrDefault();
    }

    public Song GetSongFromScene(string scene)
    {
        return songs.Where(x => x.scene == scene).FirstOrDefault();
    }

    public Song GetNextRandomSong()
    {
        if (playlistQueue == null || playlistQueue.Count <= 0)
        {
            playlistQueue = new Queue<Song>();

            List<Song> newSongs = new List<Song>(songs);
            newSongs.Shuffle();

            foreach (Song availabeSong in newSongs.Where(x => x.unlocked == true).ToList())
            {
                if (availabeSong.scene != SceneManager.GetActiveScene().name)
                    playlistQueue.Enqueue(availabeSong);
            }

            //playlistQueue.
        }

        return playlistQueue.Dequeue();
    }

    public void CheckForUnlockedSongs()
    {
        foreach(string songId in SaveSystem.CurrentSaveData().unlockedSongsIds)
        {
            Song song = songs.Where(x => x.id == songId).SingleOrDefault();
            if (song != null) song.unlocked = true;
        }
    }
}

[System.Serializable]
public class Song
{
    public string id;
    public string name;
    public AudioClip audioClip;
    public bool unlocked;
    public string scene;
    public int bpm;
    public bool forcePlay = false;

    public void UpdateUnlocked()
    {
        unlocked = SaveSystem.CurrentSaveData().unlockedSongsIds.Contains(id);
    }
}
