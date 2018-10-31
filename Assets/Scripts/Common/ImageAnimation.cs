using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ImageAnimation : MonoBehaviour 
{
	[FormerlySerializedAs("sprites")] public Sprite[] Sprites;
	[FormerlySerializedAs("spritePerFrame")] public int SpritePerFrame = 6;
	[FormerlySerializedAs("loop")] public bool Loop = true;
	[FormerlySerializedAs("destroyOnEnd")] public bool DestroyOnEnd = false;

	private int _index = 0;
	private Image _image;
	private int _frame = 0;

	private void Awake() 
	{
		_image = GetComponent<Image> ();
	}

	private void Update () 
	{
		if (!Loop && _index == Sprites.Length) 
			return;
		
		_frame ++;
		
		if (_frame < SpritePerFrame) 
			return;
		
		_image.sprite = Sprites [_index];
		_frame = 0;
		_index ++;

		if (_index < Sprites.Length) 
			return;
		
		if (Loop) 
			_index = 0;
		
		if (DestroyOnEnd) 
			Destroy(gameObject);
	}
}