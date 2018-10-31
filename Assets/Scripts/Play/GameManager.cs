#region

using System;
using System.Collections;
using Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Plugins.Pixelplacement.iTween;
#endregion

namespace Play
{
    public class GameManager : MonoBehaviour
    {
        public ScrollRect Rect;
        public Image GlassContent;
        public Sprite[] GlassResult;

        private bool _isComboBegin;
        private float _gapTime;
        
        public GameObject MatchContainer;

        private bool _isHalfOn;
        
        // Fade In Out
        public RawImage FadeScreen;
        public Text[] FadeText;
        public Image[] FadeRenderer;

        private readonly WaitForSeconds _loopGap = new WaitForSeconds(0.25f);
        
        // Beaker Button
        public Button BeakerBtn;

        private void Start()
        {
            FadeInOutManager.Instance.FadeIn(FadeScreen);
            
            AudioManager.Instance.PlayBGMOn();
        }

        // Use this for initialization
        public void ColorUp(Colors color, int len)
        {
            if (_isComboBegin || GlassContent.fillAmount >= 1.0f)
            {
                /*Debug.Log("Before Check : " + GlassContent.fillAmount);*/
                return;
            }

            StartCoroutine("CheckCombo");
            
            var glassColor = GlassContent.color;
            
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (color)
            {          
                case Colors.Red:
                    if (glassColor.r < 1.0f)
                        glassColor.r += (float) len / 128;
                    
                    if (glassColor.g > 0)
                        glassColor.g -= (float) len / 256;
                    
                    if (glassColor.b > 0)
                        glassColor.b -= (float) len / 256;
                    break;
                case Colors.Green:
                    if (glassColor.r > 0)
                        glassColor.r -= (float) len / 256;
                    
                    if (glassColor.g < 1.0f)
                        glassColor.g += (float) len / 128;
                    
                    if (glassColor.b > 0)
                        glassColor.b -= (float) len / 256;
                    break;
                case Colors.Blue:
                    if (glassColor.r > 0)
                        glassColor.r -= (float) len / 256;
                    
                    if (glassColor.g > 0)
                        glassColor.g -= (float) len / 256;
                    
                    if (glassColor.b < 1.0f)
                        glassColor.b += (float) len / 128;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("color", color, null);
            }
            
            GlassContent.color = glassColor;
            /*Debug.Log(glassColor);*/
            
            GlassContent.fillAmount += (float)len / 64;
            /*Debug.Log(GlassContent.fillAmount);*/

            var fillAmount = GlassContent.fillAmount;

            if (!_isHalfOn && fillAmount > 0.5f)
            {
                _isHalfOn = true;
                FadeInOutManager.Instance.FadeOut(FadeRenderer[6]);
            }

            if (fillAmount < 1.0f)
            {
                /*Debug.Log("Check FillAmount : " + fillAmount);*/
                return;
            }
            
            var maxValue = Mathf.Max(glassColor.r, glassColor.g, glassColor.b);

            if (Math.Abs(maxValue - glassColor.r) <= 0)
            {
                FadeInOutManager.Instance.FadeOut(FadeRenderer[7]);
                
                /*Debug.Log("maxValue - red : " + maxValue);*/
                
                StartCoroutine(ChangeColor(Color.red));
                /*Debug.Log("Change! Red");*/
            }
            else if (Math.Abs(maxValue - glassColor.g) <= 0)
            {
                FadeInOutManager.Instance.FadeOut(FadeRenderer[8]);
                
                /*Debug.Log("maxValue - green : " + maxValue);*/
                
                StartCoroutine(ChangeColor(Color.green));
                /*Debug.Log("Change! Green");*/
            }
            else if (Math.Abs(maxValue - glassColor.b) <= 0)
            {
                FadeInOutManager.Instance.FadeOut(FadeRenderer[9]);
                
                /*Debug.Log("maxValue - blue : " + maxValue);*/
                
                StartCoroutine(ChangeColor(Color.blue));
                /*Debug.Log("Change! Blue");*/
            }
        }

        private IEnumerator CheckCombo()
        {
            if (_isComboBegin)
            {
                _gapTime = 0.1f;
                yield break;
            }
            
            _isComboBegin = true;
            _gapTime = 0.1f;
            
            /*Debug.Log("First ComBo!");*/

            while (_gapTime > Time.deltaTime)
            {
                _gapTime -= Time.deltaTime;
                /*Debug.Log(string.Format("Combo Continue : {0}", _gapTime));*/
                yield return _loopGap;
            }

            _isComboBegin = false;
        }

        private IEnumerator ChangeColor(Color resultColor)
        {
            while (GlassContent.color != resultColor)
            {
                var glassColor = GlassContent.color;
                
                if (resultColor == Color.red)
                {
                    if (glassColor.r > 1.0f)
                    {
                        glassColor.r = 1.0f;
                    }
                    else if (glassColor.r < 1.0f)
                    {
                        glassColor.r += (float) 1 / 256;

                        if (glassColor.r > 1.0f)
                        {
                            /*Debug.Log("red overflow");*/
                            glassColor.r = 1.0f;
                        }
                    }

                    if (glassColor.g > 0)
                        glassColor.g -= (float) 1 / 256;
                    
                    if (glassColor.b > 0)
                        glassColor.b -= (float) 1 / 256;
                }
                else if (resultColor == Color.green)
                {
                    if (glassColor.r > 0)
                        glassColor.r -= (float) 1 / 256;

                    if (glassColor.g > 1.0f)
                    {
                        glassColor.g = 1.0f;
                    }
                    else if (glassColor.g < 1.0f)
                    {
                        glassColor.g += (float) 1 / 256;

                        if (glassColor.g > 1.0f)
                        {
                            /*Debug.Log("green overflow");*/
                            glassColor.g = 1.0f;
                        }
                    }

                    if (glassColor.b > 0)
                        glassColor.b -= (float) 1 / 256;
                }
                else if (resultColor == Color.blue)
                {
                    if (glassColor.r > 0)
                        glassColor.r -= (float) 1 / 256;
                    
                    if (glassColor.g > 0)
                        glassColor.g -= (float) 1 / 256;

                    if (glassColor.b > 1.0f)
                    {
                        glassColor.b = 1.0f;
                    }
                    else if (glassColor.b < 1.0f)
                    {
                        glassColor.b += (float) 1 / 256;
                        
                        if (glassColor.b > 1.0f)
                        {
                            /*Debug.Log("blue overflow");*/
                            glassColor.b = 1.0f;
                        }
                    }
                }
                
                GlassContent.color = glassColor;
                /*Debug.Log(glassColor);*/
                
                yield return new WaitForSeconds(0.02f);
            }
            
            /*Debug.Log("Change Finish");*/

            if (resultColor == Color.red)
            {
                GlassContent.sprite = GlassResult[0];
                /*Debug.Log("red");*/
            }
            else if (resultColor == Color.green)
            {
                GlassContent.sprite = GlassResult[1];
                /*Debug.Log("green");*/
            }
            else if (resultColor == Color.blue)
            {
                GlassContent.sprite = GlassResult[2];
                /*Debug.Log("blue");*/
            }

            BeakerBtn.interactable = true;
            /*Debug.Log("interactable true");*/
        }

        public void FixScreen(Vector2 pos)
        {
            if (pos != Vector2.zero)
                return;
            
            Rect.vertical = false;
            MatchContainer.SetActive(true);
        }

        public void FadeInPanel(Vector2 pos)
        {
            var posY = pos.y;
            
            if (posY <= 0.104f)
                FadeInOutManager.Instance.FadeOut(FadeRenderer[5]);
            else if (posY <= 0.19f)
                FadeInOutManager.Instance.FadeOut(FadeRenderer[4]);
            else if (posY <= 0.33f)
                FadeInOutManager.Instance.FadeOut(FadeText[3]);
            else if (posY <= 0.498f)
                FadeInOutManager.Instance.FadeOut(FadeRenderer[3]);
            else if (posY <= 0.555f)
                FadeInOutManager.Instance.FadeOut(FadeText[2]);
            else if (posY <= 0.636f)
                FadeInOutManager.Instance.FadeOut(FadeRenderer[2]);
            else if (posY <= 0.694f)
                FadeInOutManager.Instance.FadeOut(FadeText[1]);
            else if (posY <= 0.775f)
                FadeInOutManager.Instance.FadeOut(FadeRenderer[1]);
            else if (posY <= 0.832f)
                FadeInOutManager.Instance.FadeOut(FadeText[0]);
            else if (posY <= 0.913f)
                FadeInOutManager.Instance.FadeOut(FadeRenderer[0]);
        }

        public void OnBeakerBtnClick()
        {
            iTween.ShakePosition(BeakerBtn.gameObject, new Vector3(10, 10, 10), 1.0f);
            AudioManager.Instance.BtnClick();
            
            StartCoroutine("Shake");
        }

        private IEnumerator Shake()
        {
            FadeInOutManager.Instance.FadeOut(FadeScreen);
            
            yield return new WaitForSeconds(3.1f);
            
            SceneManager.LoadScene("Result");
        }

        /*private void OnDisable()
        {
            StopAllCoroutines();
        }*/
    }
}