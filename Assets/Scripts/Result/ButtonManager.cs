using System.Collections;
using Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Result
{
	public class ButtonManager : MonoBehaviour
	{
		[FormerlySerializedAs("rawImage")] public RawImage RawImage;

		private void Start()
		{
			AudioManager.Instance.ResultMusicOn();
			AudioManager.Instance.IsClear = true;
			
			FadeInOutManager.Instance.FadeIn(RawImage);
		}

		public void OnHomeBtnClick()
		{
			FadeInOutManager.Instance.FadeOut(RawImage);
			AudioManager.Instance.BtnClick();

			StartCoroutine("Wait");
		}

		private IEnumerator Wait()
		{
			yield return new WaitForSeconds(3.1f);
			
			SceneManager.LoadScene("Main");
		}

		private void OnDisable()
		{
			StopCoroutine("Wait");
		}
	}
}
