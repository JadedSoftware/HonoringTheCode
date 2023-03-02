using System;
using System.Collections;
using Core.NonLevel_Scenes;
using DarkTonic.MasterAudio;
using UnityEngine;

namespace Core.Audio
{
    public enum PlayListTypes
    {
        MainMenu,
        GameDesign
    }
    /// <summary>
    /// Handles triggering audio
    /// </summary>
    public class AudioController : MonoSingleton<AudioController>
    {
        private AudioClipsContainer clipsContainer;
        private PlaylistController playlistController;

        public override void Init()
        {
            playlistController = MasterAudio.OnlyPlaylistController;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            clipsContainer = GetComponentInChildren<AudioClipsContainer>();
        }

        public void ChangePlaylist(PlayListTypes nextPlayList)
        {
            playlistController.ChangePlaylist(nextPlayList.ToString());
        }

        public void PlayButtonSound(ButtonTypes buttonType)
        {
            switch (buttonType)
            {
                case ButtonTypes.Start:
                    MasterAudio.PlaySound(clipsContainer.menuButtonClicked);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buttonType), buttonType, null);
            }
        }
    }
}