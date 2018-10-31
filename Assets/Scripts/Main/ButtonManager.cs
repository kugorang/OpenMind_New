using Common;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Main
{
	public class ButtonManager : MonoBehaviour
	{
		public GameObject CreditImg, PressText, CreditBtn, BackBtn;
		public Button StartBtn;

		[FormerlySerializedAs("before")] public SpriteRenderer Before;
		[FormerlySerializedAs("after")] public SpriteRenderer After;

		private void Start()
		{
			AudioManager.Instance.MainBGMOn();

			/*AudioManager.Instance.IsClear = false;*/

			if (!AudioManager.Instance.IsClear) 
				return;
			
			Before.gameObject.SetActive(false);
			After.gameObject.SetActive(true);
		}

		public void OnStartBtnClick()
		{
			AudioManager.Instance.BtnClick();
			
			SceneManager.LoadScene("Play");
		}

		public void OnCreditBtnClick()
		{
			PressText.SetActive(false);
			CreditBtn.SetActive(false);
			
			CreditImg.SetActive(true);
			BackBtn.SetActive(true);

			StartBtn.interactable = false;
			
			AudioManager.Instance.BtnClick();
		}

		public void OnBackBtnClick()
		{
			PressText.SetActive(true);
			CreditBtn.SetActive(true);
			
			CreditImg.SetActive(false);
			BackBtn.SetActive(false);
			
			StartBtn.interactable = true;
			
			AudioManager.Instance.BtnClick();
		}
	}
}