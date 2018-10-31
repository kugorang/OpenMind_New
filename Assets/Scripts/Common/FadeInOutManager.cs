using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Common
{
	public class FadeInOutManager : MonoBehaviour 
	{
		// Static instance of GameManager which allows it to be accessed by any other script.
		public static FadeInOutManager Instance;
		
		//Awake is always called before any Start functions
		private void Awake()
		{
			//Check if instance already exists
			if (Instance == null)
			{        
				//if not, set instance to this
				Instance = this;
			}
			//If instance already exists and it's not this:
			else if (Instance != this)
			{        
				//Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
				Destroy(gameObject);    
			}

			//Sets this to not be destroyed when reloading scene
			DontDestroyOnLoad(gameObject);
		}

		public void FadeIn(Graphic graphic)
		{
			StartCoroutine("GraphicFadeIn", graphic);
		}
		
		public void FadeIn(SpriteRenderer spriteRenderer)
		{
			StartCoroutine("GraphicFadeIn", spriteRenderer);
		}

		private IEnumerator GraphicFadeIn(Graphic graphic)
		{
			var color = graphic.color;
			var alpha = color.a;

			while (alpha > 0f)
			{
				color.a = alpha -= 0.02f;
				
				if (graphic == null)
					yield break;
					
				graphic.color = color;
				
				yield return new WaitForSeconds(0.06f);
			}
			
			graphic.gameObject.SetActive(false);
		}
		
		private IEnumerator GraphicFadeIn(SpriteRenderer spriteRenderer)
		{
			var color = spriteRenderer.color;
			var alpha = color.a;

			while (alpha > 0f)
			{
				color.a = alpha -= 0.02f;

				if (spriteRenderer == null)
					yield break;
				
				spriteRenderer.color = color;
				
				yield return new WaitForSeconds(0.06f);
			}
			
			spriteRenderer.gameObject.SetActive(false);
		}

		public void FadeOut(Graphic graphic)
		{
			StartCoroutine("GraphicFadeOut", graphic);
		}
		
		public void FadeOut(SpriteRenderer spriteRenderer)
		{
			StartCoroutine("GraphicFadeOut", spriteRenderer);
		}

		private IEnumerator GraphicFadeOut(Graphic graphic)
		{
			graphic.gameObject.SetActive(true);
			
			var color = graphic.color;
			var alpha = color.a;

			while (alpha < 1.0f)
			{
				color.a = alpha += 0.02f;

				if (graphic == null)
					yield break;
				
				graphic.color = color;
				
				yield return new WaitForSeconds(0.06f);
			}
		}
		
		private IEnumerator GraphicFadeOut(SpriteRenderer spriteRenderer)
		{
			spriteRenderer.gameObject.SetActive(true);
			
			var color = spriteRenderer.color;
			var alpha = color.a;

			while (alpha < 1.0f)
			{
				color.a = alpha += 0.02f;

				if (spriteRenderer == null)
					yield break;
				
				spriteRenderer.color = color;
				
				yield return new WaitForSeconds(0.06f);
			}
		}
	}
}
