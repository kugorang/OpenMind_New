using UnityEngine;
using UnityEngine.Serialization;

namespace Common
{
	public class AudioManager : MonoBehaviour
	{
		public AudioSource BGM, SFX;
		public AudioClip LogoMusic, PlayMusic, ResultMusic, BtnMusic, MatchMusic;
		
		[FormerlySerializedAs("isClear")] public bool IsClear;
		
		// Static instance of GameManager which allows it to be accessed by any other script.
		public static AudioManager Instance;              

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

		private void Start()
		{
			/*IsClear = PlayerPrefs.GetInt("Clear", 0) != 1;*/
		}

		public void MainBGMOn()
		{
			BGM.clip = LogoMusic;
			BGM.Play();
		}

		public void PlayBGMOn()
		{
			BGM.clip = PlayMusic;
			BGM.Play();
		}

		public void ResultMusicOn()
		{
			BGM.clip = ResultMusic;
			BGM.Play();
		}

		public void BtnClick()
		{
			SFX.clip = BtnMusic;
			SFX.Play();
		}

		public void MatchPuzzle()
		{
			SFX.clip = MatchMusic;
			SFX.Play();
		}
	}
}
