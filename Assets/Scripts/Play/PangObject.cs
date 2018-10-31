#region

using UnityEngine;

#endregion

namespace Play
{
    public class PangObject : MonoBehaviour
    {
        #region Field

        public Colors CurrentColor;

        #endregion
        

        #region Property

        protected SpriteRenderer Render { get; set; }
        protected SpriteFader Fader { get; set; }

        #endregion
        
        
        #region Method

        private void Awake()
        {
            Render = GetComponent<SpriteRenderer>();
            Fader = GetComponent<SpriteFader>();
        }

        #endregion
    }
}