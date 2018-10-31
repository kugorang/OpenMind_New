#region

using UnityEngine;

#endregion

namespace Play
{
    public enum Colors
    {
        None,
        Red,
        Green,
        Blue
    }

    public class Palette : MonoBehaviour
    {
        #region Singleton

        private static Palette _sInstance;

        public static Palette GetInstance
        {
            get
            {
                if (_sInstance == null)
                    _sInstance = FindObjectOfType<Palette>();

                return _sInstance;
            }
        }

        #endregion

        
        #region -------Public Field-------

        public Color[] Colors;

        #endregion
        

        #region -------Default Method-------

        private void Start()
        {
            _sInstance = this;
        }

        #endregion

        
        #region -------Private Method-------

        private int GetRandomColorIndex()
        {
            // 초기화 되지 않은 영역은 자동으로 0으로 초기화 되기 때문에
            // 배열의 인덱스 1부터 길이 - 1까지를 영역으로 잡는다.
            return Random.Range(1, Colors.Length);
        }

        #endregion

        
        #region -------Public Method-------

        public Color GetColor(Colors color)
        {
            return Colors[(int)color];
        }

        public Color GetColor(int colorNum)
        {
            return Colors[colorNum];
        }

        public Color GetExclusionRandomColor(Colors excColor, out Colors color)
        {
            var index = GetRandomColorIndex();

            while ((Colors)index == excColor)
                index = GetRandomColorIndex();

            color = (Colors) index;

            return Colors[index];
        }
        
        public Color GetRandomColor()
        {
            return Colors[GetRandomColorIndex()];
        }

        public Color GetRandomColor(out Colors color)
        {
            var index = GetRandomColorIndex();

            color = (Colors)index;

            return Colors[index];
        }

        #endregion
    }
}