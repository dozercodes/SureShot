/************************************************************************************ 
 *
 * Microsoft XNA Community Game Platform
 * Copyright (C) Microsoft Corporation. All rights reserved.
 * 
 * ===================================================================================
 * Modified by: Ohan Oda (ohan@cs.columbia.edu)
 * 
 *************************************************************************************/ 

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

using GoblinXNA.Helpers;

namespace GoblinXNA.Sounds
{
    /// <summary>
    /// A wrapper class for the XNA audio library. This class provides an easy interface to play both
    /// 2D and 3D sounds.
    /// </summary>
    public sealed class Sound
    {
        #region Member Fields
#if !WINDOWS_PHONE
        private static AudioEngine audioEngine;
        private static WaveBank waveBank;
        private static SoundBank soundBank;

        private bool initialized = false;

        private List<Cue3D> activeCues;
        private Stack<Cue3D> cuePool;
#endif

        // We don't want to poll constantly, so we set an amount of time (in seconds) of
        // how often we should poll. By default, we're going to poll every second.
        const float PollDelay = 1f;

        // A simple float for our polling timer.
        private float gameHasControlTimer;

        // We keep a member variable around to tell us if we have control of the music.
        private bool gameHasControl = false;

        // For 3D sound effect
        private AudioListener listener;
        private AudioEmitter emitter;

        private Vector3 prevListenerPos;

        private List<Sound3D> sound3Ds;
        private Song backgroundMusic;

        private Dictionary<string, SoundEffect> soundEffects;

        private static Sound sound;
        #endregion

        #region Constructors
        private Sound()
        {
            // Grab the GameHasControl as our initial value
            gameHasControl = MediaPlayer.GameHasControl;

            listener = new AudioListener();
            emitter = new AudioEmitter();

            sound3Ds = new List<Sound3D>();
            soundEffects = new Dictionary<string, SoundEffect>();
        }
        #endregion

        #region Properties

        public static Sound Instance
        {
            get
            {
                if (sound == null)
                    sound = new Sound();

                return sound;
            }
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Gets the audio engine.
        /// </summary>
        public AudioEngine AudioEngine
        {
            get { return audioEngine; }
        }

        /// <summary>
        /// Gets the wave bank.
        /// </summary>
        public WaveBank WaveBank
        {
            get { return waveBank; }
        }

        /// <summary>
        /// Gets the sound bank.
        /// </summary>
        public SoundBank SoundBank
        {
            get { return soundBank; }
        }
#endif

        #endregion

        #region Public Methods

#if !WINDOWS_PHONE
        /// <summary>
        /// Initializes the audio system with the given XACT project file (.xap) compiled using
        /// Microsoft Cross-Platform Audio Creation Tool that comes with XNA Game Studio 2.0
        /// </summary>
        /// <param name="xapAssetName">The asset name of the XACT project file file</param>
        public void Initialize(String xapAssetName)
        {
            try
            {
                String name = Path.GetFileNameWithoutExtension(xapAssetName);
                audioEngine = new AudioEngine(Path.Combine(State.Content.RootDirectory + "/" +
                    State.GetSettingVariable("AudioDirectory"), name + ".xgs"));
                waveBank = new WaveBank(audioEngine, Path.Combine(State.Content.RootDirectory + 
                    "/" + State.GetSettingVariable("AudioDirectory"), "Wave Bank.xwb"));

                if (waveBank != null)
                {
                    soundBank = new SoundBank(audioEngine,
                        Path.Combine(State.Content.RootDirectory + "/" +
                        State.GetSettingVariable("AudioDirectory"), "Sound Bank.xsb"));
                }

                activeCues = new List<Cue3D>();
                cuePool = new Stack<Cue3D>();

                initialized = true;
            }
            catch (NoAudioHardwareException nahe)
            {
                Log.Write(nahe.Message);
            }
        }
#endif

        /// <summary>
        /// Event handler that is invoked when the game is activated.
        /// </summary>
        public void GameActivated(object sender, EventArgs e)
        {
            // See if we have control of the music
            gameHasControl = MediaPlayer.GameHasControl;

            // If we have control, a song we want to play, and the media player isn't playing,
            // play our song. This will happen when coming back from deactivation with certain
            // launchers (mainly the MediaPlayerLauncher) which don't automatically play/resume
            // the song for us. We can detect this case and restart the song ourselves, that way
            // the user doesn't end up with a game without background music.
            if (gameHasControl && backgroundMusic != null && MediaPlayer.State != MediaState.Playing)
                PlaySongSafe();
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Triggers a new sound.
        /// </summary>
        /// <remarks>
        /// In order to free up unnecessary memory usage, the played cue is automatically destroyed
        /// when it stops playing. 
        /// </remarks>
        /// <exception cref="GoblinException">Throws exception if this is called before Initialize(..)</exception>
        /// <param name="cueName">The name of the cue of a sound</param>
        /// <returns></returns>
        public Cue Play(String cueName)
        {
            if (!initialized)
                throw new GoblinException("Sound engine is not initialized. Call Sound.Initialize(..) first");

            Cue3D cue3D;

            if (cuePool.Count > 0)
            {
                // If possible, reuse an existing Cue instance.
                cue3D = cuePool.Pop();
            }
            else
            {
                // Otherwise we have to allocate a new one.
                cue3D = new Cue3D();
            }

            // Fill in the cue and emitter fields.
            cue3D.Cue = soundBank.GetCue(cueName);
            cue3D.Emitter = null;

            cue3D.Cue.Play();

            // Remember that this cue is now active.
            activeCues.Add(cue3D);

            return cue3D.Cue;
        }

        /// <summary>
        /// Triggers a new 3D sound
        /// </summary>
        /// <remarks>
        /// In order to free up unnecessary memory usage, the played cue is automatically destroyed
        /// when it stops playing. 
        /// </remarks>
        /// <exception cref="GoblinException">Throws exception if this is called before Initialize(..)</exception>
        /// <param name="cueName">The name of the cue of a sound</param>
        /// <param name="emitter">An IAudioEmitter object that defines the properties of the sound
        /// including position, and velocity.</param>
        /// <returns></returns>
        public Cue Play3D(String cueName, IAudioEmitter emitter)
        {
            if (!initialized)
                throw new GoblinException("Sound engine is not initialized. Call Sound.Initialize(..) first");

            Cue3D cue3D;

            if (cuePool.Count > 0)
            {
                // If possible, reuse an existing Cue3D instance.
                cue3D = cuePool.Pop();
            }
            else
            {
                // Otherwise we have to allocate a new one.
                cue3D = new Cue3D();
            }

            // Fill in the cue and emitter fields.
            cue3D.Cue = soundBank.GetCue(cueName);
            cue3D.Emitter = emitter;

            // Set the 3D position of this cue, and then play it.
            Apply3D(cue3D);

            cue3D.Cue.Play();

            // Remember that this cue is now active.
            activeCues.Add(cue3D);

            return cue3D.Cue;
        }

        /// <summary>
        /// Sets the volume of sounds in certain category
        /// </summary>
        /// <param name="categoryName">The name of the category</param>
        /// <param name="volume">The volume in dB</param>
        public void SetVolume(String categoryName, float volume)
        {
            audioEngine.GetCategory(categoryName).SetVolume(volume);
        }
#endif

        /// <summary>
        /// Triggers a new sound.
        /// </summary>
        /// <exception cref="GoblinException">Throws exception if this is called before Initialize(..)</exception>
        /// <param name="soundEffect">The loaded sound effect</param>
        /// <returns></returns>
        public SoundEffectInstance PlaySoundEffect(SoundEffect soundEffect)
        {
            SoundEffectInstance ei = soundEffect.CreateInstance();
            ei.Play();

            return ei;
        }

        /// <summary>
        /// Triggers a new sound.
        /// </summary>
        /// <exception cref="GoblinException">Throws exception if this is called before Initialize(..)</exception>
        /// <param name="soundEffectName">The name of the sound effect to play</param>
        /// <returns></returns>
        public SoundEffectInstance PlaySoundEffect(string soundEffectName)
        {
            if (!soundEffects.ContainsKey(soundEffectName))
            {
                SoundEffect effect = State.Content.Load<SoundEffect>(soundEffectName);
                effect.Name = soundEffectName;

                soundEffects.Add(soundEffectName, effect);
            }

            return PlaySoundEffect(soundEffects[soundEffectName]);
        }

        /// <summary>
        /// Triggers a new 3D sound
        /// </summary>
        /// <remarks>
        /// In order to free up unnecessary memory usage, the played cue is automatically destroyed
        /// when it stops playing. 
        /// </remarks>
        /// <exception cref="GoblinException">Throws exception if this is called before Initialize(..)</exception>
        /// <param name="soundEffect">The loaded sound effect</param>
        /// <param name="emitter">An IAudioEmitter object that defines the properties of the sound
        /// including position, and velocity.</param>
        /// <returns></returns>
        public SoundEffectInstance PlaySoundEffect3D(SoundEffect soundEffect, IAudioEmitter emitter)
        {
            SoundEffectInstance ei = soundEffect.CreateInstance();

            Sound3D sound3D = new Sound3D();
            sound3D.Sound = ei;
            sound3D.Emitter = emitter;

            sound3Ds.Add(sound3D);
            Apply3D(sound3D);

            ei.Play();

            return ei;
        }

        /// <summary>
        /// Triggers a new 3D sound
        /// </summary>
        /// <remarks>
        /// In order to free up unnecessary memory usage, the played cue is automatically destroyed
        /// when it stops playing. 
        /// </remarks>
        /// <exception cref="GoblinException">Throws exception if this is called before Initialize(..)</exception>
        /// <param name="soundEffectName">The name of the sound effect to play</param>
        /// <param name="emitter">An IAudioEmitter object that defines the properties of the sound
        /// including position, and velocity.</param>
        /// <returns></returns>
        public SoundEffectInstance PlaySoundEffect3D(string soundEffectName, IAudioEmitter emitter)
        {
            if (!soundEffects.ContainsKey(soundEffectName))
            {
                SoundEffect effect = State.Content.Load<SoundEffect>(soundEffectName);
                effect.Name = soundEffectName;

                soundEffects.Add(soundEffectName, effect);
            }

            return PlaySoundEffect3D(soundEffects[soundEffectName], emitter);
        }

        public void PlayBackgroundMusic(Song music)
        {
            backgroundMusic = music;

#if WINDOWS_PHONE
            if(gameHasControl)
#endif
                PlaySongSafe();
        }

        public void StopBackgroundMusic()
        {
            backgroundMusic = null;

#if WINDOWS_PHONE
            if (gameHasControl)
#endif
                MediaPlayer.Stop();
        }

        /// <summary>
        /// Updates the XNA audio engine
        /// </summary>
        /// <param name="elapsedTime"></param>
        internal void Update(TimeSpan elapsedTime)
        {
#if WINDOWS_PHONE
            // Update our timer
            gameHasControlTimer += (float)elapsedTime.TotalSeconds;

            // If we've passed our poll delay, we want to handle our update
            if (gameHasControlTimer >= PollDelay)
            {
                // Reset the timer back to zero
                gameHasControlTimer = 0f;

                // Check to see if we have control of the media player
                gameHasControl = MediaPlayer.GameHasControl;

                // Get the current state and song from the MediaPlayer
                MediaState currentState = MediaPlayer.State;
                Song activeSong = MediaPlayer.Queue.ActiveSong;

                // If we have control of the music...
                if (gameHasControl)
                {
                    // If we have a song that we want playing...
                    if (backgroundMusic != null)
                    {
                        // If the media player isn't playing anything...
                        if (currentState != MediaState.Playing)
                        {
                            // If the song is paused, for example because the headphones
                            // were removed, we call Resume() to continue playback.
                            if (currentState == MediaState.Paused)
                            {
                                ResumeSongSafe();
                            }

                            // Otherwise we play our desired song.
                            else
                            {
                                PlaySongSafe();
                            }
                        }
                    }

                    // If we don't have a song we want playing, we want to make sure we stop
                    // any music we may have previously had playing.
                    else
                    {
                        if (currentState != MediaState.Stopped)
                            MediaPlayer.Stop();
                    }
                }
            }
#else
            if (initialized)
            {
                // Loop over all the currently playing 3D sounds.
                int index = 0;

                while (index < activeCues.Count)
                {
                    Cue3D cue3D = activeCues[index];

                    if (cue3D.Cue.IsStopped)
                    {
                        // If the cue has stopped playing, dispose it.
                        cue3D.Cue.Dispose();

                        // Store the Cue3D instance for future reuse.
                        cuePool.Push(cue3D);

                        // Remove it from the active list.
                        activeCues.RemoveAt(index);
                    }
                    else
                    {
                        // If the cue is still playing and it's 3D, update its 3D settings.
                        if (cue3D.Emitter != null)
                            Apply3D(cue3D);

                        index++;
                    }
                }

                // Update the XACT engine.
                audioEngine.Update();
            }
#endif

            List<Sound3D> removeList = new List<Sound3D>();
            foreach (Sound3D sound3d in sound3Ds)
            {
                if (sound3d.Sound.State == SoundState.Stopped)
                    removeList.Add(sound3d);
                else
                    Apply3D(sound3d);
            }

            foreach (Sound3D sound3d in removeList)
            {
                sound3d.Sound.Dispose();
                sound3Ds.Remove(sound3d);
            }
        }

        /// <summary>
        /// Updates the position and orientation of the listener for 3D audio effect
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <param name="position">The position of the listener</param>
        /// <param name="forward">The forward vector of the listener</param>
        /// <param name="up">The up vector of the lister</param>
        internal void UpdateListener(TimeSpan elapsedTime, Vector3 position, Vector3 forward,
            Vector3 up)
        {
            listener.Position = position;
            listener.Up = up;
            listener.Forward = forward;
            listener.Velocity = (position - prevListenerPos) /
                (float)elapsedTime.TotalSeconds;
            prevListenerPos = position;
        }

        public void Dispose()
        {
            try
            {
                if(MediaPlayer.State == MediaState.Playing)
                    MediaPlayer.Stop();
            }
            catch (Exception exp) { }

            foreach (Sound3D sound3d in sound3Ds)
            {
                sound3d.Sound.Stop();
                sound3d.Sound.Dispose();
            }

            sound3Ds.Clear();

            foreach (SoundEffect effect in soundEffects.Values)
                effect.Dispose();

            soundEffects.Clear();

#if !WINDOWS_PHONE
            if (audioEngine != null)
            {
                soundBank.Dispose();
                waveBank.Dispose();
                audioEngine.Dispose();
            }
#endif
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the position and velocity settings of a 3D cue.
        /// </summary>
        private void Apply3D(Sound3D cue3D)
        {
            emitter.Position = cue3D.Emitter.Position;
            emitter.Forward = cue3D.Emitter.Forward;
            emitter.Up = cue3D.Emitter.Up;
            emitter.Velocity = cue3D.Emitter.Velocity;

            cue3D.Sound.Apply3D(listener, emitter);
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Updates the position and velocity settings of a 3D cue.
        /// </summary>
        private void Apply3D(Cue3D cue3D)
        {
            emitter.Position = cue3D.Emitter.Position;
            emitter.Forward = cue3D.Emitter.Forward;
            emitter.Up = cue3D.Emitter.Up;
            emitter.Velocity = cue3D.Emitter.Velocity;

            cue3D.Cue.Apply3D(listener, emitter);
        }
#endif

        /// <summary>
        /// Helper method to wrap MediaPlayer.Play to handle exceptions.
        /// </summary>
        private void PlaySongSafe()
        {
            // Make sure we have a song to play
            if (backgroundMusic == null)
                return;

            try
            {
                MediaPlayer.Play(backgroundMusic);
            }
            catch (InvalidOperationException)
            {
                // Media playback will fail if Zune is connected. We don't want the
                // game to crash, however, so we catch the exception.

                // Null out the song so we don't keep trying to play it. That would
                // cause us to keep catching exceptions and will likely cause the game
                // to hitch occassionally.
                backgroundMusic = null;
            }
        }

        /// <summary>
        /// Helper method to wrap MediaPlayer.Resume to handle exceptions.
        /// </summary>
        private void ResumeSongSafe()
        {
            try
            {
                MediaPlayer.Resume();
            }
            catch (InvalidOperationException)
            {
                // Media playback will fail if Zune is connected. We don't want the
                // game to crash, however, so we catch the exception.

                // Null out the song so we don't keep trying to resume it. That would
                // cause us to keep catching exceptions and will likely cause the game
                // to hitch occassionally.
                backgroundMusic = null;
            }
        }
        #endregion

        #region Private Classes
        /// <summary>
        /// Internal helper class for keeping track of an active 3D cue,
        /// and remembering which emitter object it is attached to.
        /// </summary>
        private class Sound3D
        {
            public SoundEffectInstance Sound;
            public IAudioEmitter Emitter;
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Internal helper class for keeping track of an active 3D cue,
        /// and remembering which emitter object it is attached to.
        /// </summary>
        private class Cue3D
        {
            public Cue Cue;
            public IAudioEmitter Emitter;
        }
#endif
        #endregion
    }
}
