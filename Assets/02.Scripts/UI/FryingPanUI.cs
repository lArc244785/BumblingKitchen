using BumblingKitchen.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace BumblingKitchen
{
    public class FryingPanUI : InGameUIBase
	{
        [SerializeField] private FryingPan _pan;
        [SerializeField] private Slider _progress;

		[SerializeField] private Sprite _sucess;
		[SerializeField] private Sprite _fail;

		private CookingRecipe _recipe;

		protected override void Awake()
		{
			base.Awake();
			_pan.OnCookingStart += Open;
			_pan.OnSettingRecipe += SetupProgress;
			_pan.OnUpdattingProgress += UpdateProgress;
			_pan.OnCookingSucess += SetFailProgress;
			_pan.OnCookingFail += Close;
			_pan.OnDoenCooked += Close;

			Close();
		}

		private void SetupProgress(CookingRecipe recipe)
		{
			_progress.minValue = 0.0f;
			_progress.maxValue = recipe.SucessProgress;
			_progress.value = 0.0f;
			_progress.fillRect.GetComponent<Image>().sprite = _sucess;
			_recipe = recipe;
		}

		private void UpdateProgress(float value)
		{
			_progress.value = value;
		}

		private void SetFailProgress()
		{
			_progress.minValue = _recipe.SucessProgress;
			_progress.maxValue = _recipe.FailProgress;
			_progress.fillRect.GetComponent<Image>().sprite = _fail;
		}

		private void Close()
		{
			Toogle(false);
		}

		private void Open()
		{
			Toogle(true);
		}

	}
}
