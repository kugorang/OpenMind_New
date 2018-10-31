#region

using System.Collections;
using UnityEngine;

#endregion

namespace Play
{
    public class SpriteFader : MonoBehaviour
    {
        #region Field

        private SpriteRenderer _renderer;
        private float _delay;
        private IEnumerator _fadeCoroutine;

        #endregion
        
        
        #region Private Method

        private IEnumerator FadeCoroutine(Color color)
        {
            var orgColor = _renderer.color;
            var startTime = Time.time;

            while (Time.time - startTime <= _delay)
            {
                _renderer.color = Color.Lerp(orgColor, color, (Time.time - startTime) / _delay);
                yield return null;
            }

            _renderer.color = color;
        }

        #endregion

        
        #region Public Method

        public void Set(SpriteRenderer render, float delay)
        {
            _renderer = render;
            _delay = delay;
        }

        public void FadeColor(Color color)
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            _fadeCoroutine = FadeCoroutine(color);
            StartCoroutine(_fadeCoroutine);
        }

        public IEnumerator PingPongFade(Color color, float delay)
        {
            var orgColor = _renderer.color;
            var startTime = Time.time;
            
            while (Time.time - startTime <= _delay)
            {
                _renderer.color = Color.Lerp(orgColor, color, (Time.time - startTime) / _delay);
                yield return null;
            }

            startTime = Time.time;

            while (Time.time - startTime <= _delay)
            {
                _renderer.color = Color.Lerp(color, orgColor, (Time.time - startTime) / _delay);
                yield return null;
            }

            _renderer.color = orgColor;
        }

        #endregion
    }
}