using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace BumblingKitchen
{
    public class InitailzeSound : MonoBehaviour
    {
        [SerializeField] AudioMixer _mixer;
        void Start()
        {
			UpdateMixer();
		}

        private void UpdateMixer()
        {
			float _masterVolume;
			float _bgmVolume;
			float _sfxVolume;


			if (PlayerPrefs.HasKey("MasterVolume") == true)
			{
				_masterVolume = PlayerPrefs.GetFloat("MasterVolume");
			}
			else
			{
				_masterVolume = 1.0f;
			}

			if (PlayerPrefs.HasKey("BgmVolume") == true)
			{
				_bgmVolume = PlayerPrefs.GetFloat("BgmVolume");
			}
			else
			{
				_bgmVolume = 1.0f;
			}


			if (PlayerPrefs.HasKey("SfxVolume") == true)
			{
				_sfxVolume = PlayerPrefs.GetFloat("SfxVolume");
			}
			else
			{
				_sfxVolume = 1.0f;
			}

			float master = Mathf.Log10(_masterVolume) * 20.0f;
			float bgm = Mathf.Log10(_bgmVolume) * 20.0f;
			float sfx = Mathf.Log10(_sfxVolume) * 20.0f;

			_mixer.SetFloat("Master", master);
            _mixer.SetFloat("BGM", bgm);
            _mixer.SetFloat("SFX", sfx);
        }
    }
}
