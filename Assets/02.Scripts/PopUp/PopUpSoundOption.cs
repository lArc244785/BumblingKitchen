using BumblingKitchen.PopUp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace BumblingKitchen.PopUp
{
    public class PopUpSoundOption : PopUpBase
    {
		[SerializeField] private AudioMixer _mixer;
		[SerializeField] private Slider _master;
		[SerializeField] private Slider _bgm;
		[SerializeField] private Slider _sfx;
		[SerializeField] private Button _cancle;
		[SerializeField] private Button _apply;

		private float _masterVolume;
		private float _bgmVolume;
		private float _sfxVolume;

		private float _prevMasterVolume;
		private float _prevBgmVolume;
		private float _prevSfxVolume;

		private void Awake()
		{
			if(PlayerPrefs.HasKey("MasterVolume") == true)
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

			_master.value = _masterVolume;
			_master.onValueChanged.AddListener((value) =>
			{
				_masterVolume = value;
				UpdateMixer();
			});

			_bgm.value = _bgmVolume;
			_bgm.onValueChanged.AddListener((value) =>
			{
				_bgmVolume = value;
				UpdateMixer();
			});

			_sfx.value = _sfxVolume;
			_sfx.onValueChanged.AddListener((value) =>
			{
				_sfxVolume = value;
				UpdateMixer();
			});

			_cancle.onClick.AddListener(OnCancle);
			_apply.onClick.AddListener(OnApply);

			_prevMasterVolume = _masterVolume;
			_prevBgmVolume = _bgmVolume;
			_prevSfxVolume = _sfxVolume;
		}

		private void OnCancle()
		{
			_masterVolume = _prevMasterVolume;
			_bgmVolume = _prevBgmVolume;
			_sfxVolume = _prevSfxVolume;
			UpdateMixer();
			Close();
		}

		private void OnApply()
		{
			PlayerPrefs.SetFloat("MasterVolume", _masterVolume);
			PlayerPrefs.SetFloat("BgmVolume", _bgmVolume);
			PlayerPrefs.SetFloat("SfxVolume", _sfxVolume);
			Close();
		}

		private void UpdateMixer()
		{
			Debug.Log("UpDate Mixer");
			float master = Mathf.Log10(_masterVolume) * 20.0f;
			float bgm = Mathf.Log10(_bgmVolume) * 20.0f;
			float sfx = Mathf.Log10(_sfxVolume) * 20.0f;

			_mixer.SetFloat("Master", master);
			_mixer.SetFloat("BGM", bgm);
			_mixer.SetFloat("SFX", sfx);
		}
	}
}
