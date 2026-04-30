using System.Collections.Generic;
using Data.Enums;
using Helpers.Audio;
using UnityEngine;
using UnityEngine.Audio;

namespace Services.Impl
{
    public partial class AudioService : IAudioService
    {
        private const float MinDb = -80f;
        private const string MasterVolumeParam = "MasterVolume";
        private const string MusicVolumeParam = "MusicVolume";
        private const string SfxVolumeParam = "SfxVolume";
        private const string UiVolumeParam = "UiVolume";

        private readonly AudioMixer _audioMixer;
        private readonly AudioMixerGroup _musicGroup;
        private readonly AudioMixerGroup _sfxGroup;
        private readonly AudioMixerGroup _uiGroup;
        private readonly GameObject _sfxPrefab;
        private readonly int _sfxPoolSize;

        private readonly Transform _audioRoot;
        private readonly AudioSource _musicSource;
        private readonly AudioSource _uiSource;
        private readonly List<PooledAudioSource> _sfxPool = new();
        private readonly Queue<PooledAudioSource> _availableSfx = new();

        public AudioService(
            AudioMixer audioMixer,
            AudioMixerGroup musicGroup,
            AudioMixerGroup sfxGroup,
            AudioMixerGroup uiGroup,
            GameObject sfxPrefab,
            int sfxPoolSize
        )
        {
            _audioMixer = audioMixer;
            _musicGroup = musicGroup;
            _sfxGroup = sfxGroup;
            _uiGroup = uiGroup;
            _sfxPrefab = sfxPrefab;
            _sfxPoolSize = Mathf.Max(1, sfxPoolSize);

            _audioRoot = CreateAudioRoot();
            _musicSource = Create2DSource("MusicSource", _musicGroup);
            _uiSource = Create2DSource("UiSource", _uiGroup);

            WarmupPool();
        }

        public void StopMusic()
        {
            if (_musicSource.isPlaying) 
                _musicSource.Stop();
        }

        public void StopUi()
        {
            if (_uiSource.isPlaying)
                _uiSource.Stop();
        }

        public void StopAllSfx()
        {
            for (var i = 0; i < _sfxPool.Count; i++)
            {
                var pooled = _sfxPool[i];
                if (pooled != null && pooled.gameObject.activeSelf)
                    pooled.StopAndRelease();
            }
        }

        public void StopAllAudio()
        {
            StopMusic();
            StopUi();
            StopAllSfx();
        }

        public void PauseAudio(EAudioType type)
        {
            switch (type)
            {
                case EAudioType.Music:
                    if (_musicSource.isPlaying)
                        _musicSource.Pause();
                    break;
                case EAudioType.Sfx:
                    PauseLoopSfx();
                    break;
                case EAudioType.Ui:
                    if (_uiSource.isPlaying)
                        _uiSource.Pause();
                    break;
                case EAudioType.Master:
                    if (_musicSource.isPlaying)
                        _musicSource.Pause();
                    if (_uiSource.isPlaying)
                        _uiSource.Pause();
                    PauseLoopSfx();
                    break;
            }
        }

        public void ResumeAudio(EAudioType type)
        {
            switch (type)
            {
                case EAudioType.Music:
                    _musicSource.UnPause();
                    break;
                case EAudioType.Sfx:
                    ResumeLoopSfx();
                    break;
                case EAudioType.Ui:
                    _uiSource.UnPause();
                    break;
                case EAudioType.Master:
                    _musicSource.UnPause();
                    _uiSource.UnPause();
                    ResumeLoopSfx();
                    break;
            }
        }

        public AudioSource PlayMusic(AudioClip clip, float volume = 1f, bool loop = true)
        {
            if (clip == null)
                return _musicSource;

            if (_musicSource.isPlaying && _musicSource.clip == clip)
                return _musicSource;

            _musicSource.clip = clip;
            _musicSource.volume = volume;
            _musicSource.loop = loop;
            _musicSource.Play();
            return _musicSource;
        }

        public void PlayUi(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null)
                return;

            _uiSource.pitch = pitch;
            _uiSource.PlayOneShot(clip, volume);
        }

        public AudioSource PlaySfx2D(AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false)
        {
            var pooled = GetPooled();
            pooled.transform.position = _audioRoot.position;
            pooled.Play(clip, _sfxGroup, volume, pitch, 0f, loop, ReleaseToPool);
            return pooled.AudioSource;
        }

        public void PlaySfx3D(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
        {
            var pooled = GetPooled();
            pooled.transform.position = position;
            pooled.Play(clip, _sfxGroup, volume, pitch, 1f, false, ReleaseToPool);
        }

        public void SetAudioVolume(EAudioType type, float value)
        {
            if (_audioSelectionParameters != null && _audioSelectionParameters.VolumeSetup != null)
            {
                switch (type)
                {
                    case EAudioType.Master:
                        _audioSelectionParameters.SetMasterVolume(value);
                        break;
                    case EAudioType.Music:
                        _audioSelectionParameters.SetMusicVolume(value);
                        break;
                    case EAudioType.Sfx:
                        _audioSelectionParameters.SetSfxVolume(value);
                        break;
                    case EAudioType.Ui:
                        _audioSelectionParameters.SetUiVolume(value);
                        break;
                }
            }

            if (type != EAudioType.Master)
            {
                if (type == EAudioType.Music)
                    UpdateMusicVolume();
                return;
            }

            if (_audioMixer == null)
                return;

            var param = GetParamName(type);
            var volume = SetValueVolume(value);
            _audioMixer.SetFloat(param, volume);
        }

        public void TransitionSnapshot(string snapshotName, float time)
        {
            if (_audioMixer == null || string.IsNullOrWhiteSpace(snapshotName))
                return;

            var snapshot = _audioMixer.FindSnapshot(snapshotName);
            if (snapshot == null)
                return;

            snapshot.TransitionTo(time);
        }

        private Transform CreateAudioRoot()
        {
            var root = new GameObject("AudioRoot");
            Object.DontDestroyOnLoad(root);
            return root.transform;
        }

        private AudioSource Create2DSource(string name, AudioMixerGroup group)
        {
            var unitSource = new GameObject(name);
            unitSource.transform.SetParent(_audioRoot, false);
            var source = unitSource.AddComponent<AudioSource>();
            source.spatialBlend = 0f;
            source.loop = false;
            source.playOnAwake = false;
            source.outputAudioMixerGroup = group;
            return source;
        }

        private void WarmupPool()
        {
            for (var i = 0; i < _sfxPoolSize; i++)
            {
                var pooled = CreatePooledInstance();
                _sfxPool.Add(pooled);
                _availableSfx.Enqueue(pooled);
            }
        }

        private PooledAudioSource GetPooled()
        {
            if (_availableSfx.Count > 0)
                return _availableSfx.Dequeue();

            var pooled = CreatePooledInstance();
            _sfxPool.Add(pooled);
            return pooled;
        }

        private PooledAudioSource CreatePooledInstance()
        {
            var instance = _sfxPrefab != null
                ? Object.Instantiate(_sfxPrefab, _audioRoot)
                : new GameObject("SoundFx_Pooled");
            
            instance.name = _sfxPrefab != null ? $"{_sfxPrefab.name}_Pooled" : "SoundFx_Pooled";
            
            if (instance.transform.parent != _audioRoot)
                instance.transform.SetParent(_audioRoot, false);
            
            if (instance.activeSelf) 
                instance.SetActive(false);

            var pooled = instance.GetComponent<PooledAudioSource>();
            if (pooled == null) 
                pooled = instance.AddComponent<PooledAudioSource>();

            var source = instance.GetComponent<AudioSource>();
            if (source == null) 
                source = instance.AddComponent<AudioSource>();

            source.playOnAwake = false;
            return pooled;
        }

        private void ReleaseToPool(PooledAudioSource pooled)
        {
            if (pooled == null)
                return;

            pooled.transform.position = _audioRoot.position;
            _availableSfx.Enqueue(pooled);
        }

        private void PauseLoopSfx()
        {
            for (var i = 0; i < _sfxPool.Count; i++)
            {
                var pooled = _sfxPool[i];
                if (pooled == null || !pooled.gameObject.activeSelf)
                    continue;

                var source = pooled.AudioSource;
                if (source != null && source.loop && source.isPlaying)
                    source.Pause();
            }
        }

        private void ResumeLoopSfx()
        {
            for (var i = 0; i < _sfxPool.Count; i++)
            {
                var pooled = _sfxPool[i];
                if (pooled == null || !pooled.gameObject.activeSelf)
                    continue;

                var source = pooled.AudioSource;
                if (source != null && source.loop)
                    source.UnPause();
            }
        }

        private static float SetValueVolume(float value)
        {
            if (value <= 0f)
                return MinDb;

            return Mathf.Clamp(Mathf.Log10(value) * 20f, MinDb, 0f);
        }

        private static string GetParamName(EAudioType type)
        {
            return type switch
            {
                EAudioType.Master => MasterVolumeParam,
                EAudioType.Music => MusicVolumeParam,
                EAudioType.Sfx => SfxVolumeParam,
                EAudioType.Ui => UiVolumeParam,
                _ => MasterVolumeParam
            };
        }
    }
}