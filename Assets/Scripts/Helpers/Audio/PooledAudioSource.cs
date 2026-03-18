using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace Helpers.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class PooledAudioSource : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        
        private CancellationTokenSource _сancellationTokenSource;
        private Action<PooledAudioSource> _onRelease;

        public AudioSource AudioSource
        {
            get
            {
                if (_audioSource == null)
                    _audioSource = GetComponent<AudioSource>();

                return _audioSource;
            }
        }

        public void Play(
            AudioClip clip,
            AudioMixerGroup outputGroup,
            float volume,
            float pitch,
            float spatialBlend,
            bool loop,
            Action<PooledAudioSource> onRelease)
        {
            if (!gameObject.activeSelf) 
                gameObject.SetActive(true);

            _onRelease = onRelease;

            if (clip == null)
            {
                Release();
                return;
            }

            CancelReleaseTask();

            AudioSource.outputAudioMixerGroup = outputGroup;
            AudioSource.clip = clip;
            AudioSource.volume = volume;
            AudioSource.pitch = pitch;
            AudioSource.spatialBlend = spatialBlend;
            AudioSource.loop = loop;

            AudioSource.Play();

            if (!loop)
            {
                _сancellationTokenSource = new CancellationTokenSource();
                ReleaseAfterPlayAsync(_сancellationTokenSource.Token).Forget();
            }
        }

        public void StopAndRelease()
        {
            if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
            }

            CancelReleaseTask();
            Release();
        }

        private async UniTaskVoid ReleaseAfterPlayAsync(CancellationToken token)
        {
            var clipLength = _audioSource.clip != null ? _audioSource.clip.length : 0f;
            var pitch = Mathf.Abs(_audioSource.pitch);
            var duration = pitch > 0.001f ? clipLength / pitch : clipLength;

            if (duration > 0f)
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(duration),
                    cancellationToken: token);
            }

            if (token.IsCancellationRequested)
            {
                return;
            }

            Release();
        }

        private void Release()
        {
            CancelReleaseTask();
            _audioSource.clip = null;
            
            if (gameObject.activeSelf) 
                gameObject.SetActive(false);
            
            _onRelease?.Invoke(this);
        }

        private void CancelReleaseTask()
        {
            if (_сancellationTokenSource == null)
                return;

            _сancellationTokenSource.Cancel();
            _сancellationTokenSource.Dispose();
            _сancellationTokenSource = null;
        }

        private void OnDestroy()
        {
            CancelReleaseTask();
        }
    }
}