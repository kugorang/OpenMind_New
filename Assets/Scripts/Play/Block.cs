#region

using System.Collections;
using UnityEngine;

#endregion

namespace Play
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Block : PangObject
    {
        #region -------Public Field-------

        public enum BlockState
        {
            Empty,        // before Spawn  
            Idle,         // after Spawn
            Move,
            Select,
            MatchWait,
            MatchEnd,     // Crash Event -> WAIT
            Hint
        }

        #endregion
        
        
        #region -------Private Field-------

        // SpriteRenderer mRenderer;
        // Colors mColor;

        // SpriteFader mFader;

        private static readonly Vector2 BlockScale = new Vector2(Values.BlockScale, Values.BlockScale);
        private static readonly Vector2 SelectedScale = BlockScale * .8f;
        private static MatchContainer _container;

        private IEnumerator _scaleCoroutine, _hintCoroutine;

        #endregion

        
        #region -------Property-------

        private BlockData BlockData { get; set; }
        public BlockState State { get; private set; }

        public int X
        {
            get { return BlockData.X; }
        }

        public int Y
        {
            get { return BlockData.Y; }
        }

        public Vector2 LocalPosition
        {
            get { return transform.localPosition; }
            set { transform.localPosition = value; }
        }

        #endregion


        #region -------Default Method-------

        /*
        void OnGUI()
        {
            Vector2 vec = Camera.main.WorldToScreenPoint(transform.position);
            GUI.Label(new Rect(vec.x, Screen.height - vec.y,50,80), string.Format("{0}\n{1}:{2}", mState.ToString(),_X,_Y));
        }*/

        private void Start()
        {
            Fader.Set(Render, Values.BlockChangeColorDuration);
            State = BlockState.Empty;
            
            if (!_container) 
                _container = transform.parent.GetComponent<MatchContainer>();
        }

        #endregion

        //static float speed = 10f;
      
        #region -------Private Method-------

        private IEnumerator HintCoroutine()
        {
            while (State == BlockState.Hint) 
                yield return StartCoroutine(Fader.PingPongFade(Color.white, .1f));
            
            SetColor(CurrentColor);
        }

        private IEnumerator MatchCoroutine()
        {
            Fader.FadeColor(Color.white);
            
            yield return StartCoroutine(ScaleCoroutine(Vector2.zero));
            
            State = BlockState.MatchEnd;
        }

        private IEnumerator MoveCoroutine(int x, int y, BlockState pre)
        {
            var startPos = LocalPosition;
            var endPos = new Vector2(x, y);
            var startTime = Time.time;
            //float duration = Vector2.Distance(startPos, endPos) / speed;
            const float duration = .2f;
            
            State = BlockState.Move;

            while (Time.time - startTime <= duration)
            {
                LocalPosition = Vector2.Lerp(startPos, endPos, (Time.time - startTime) / duration);
                yield return null;
            }

            State = pre;
            LocalPosition = endPos;
            SetData();
            _container.FinishedMoveEvent(this);
        }

        private IEnumerator ScaleCoroutine(Vector2 endVec)
        {
            var startTime = Time.time;
            Vector2 orgVec = transform.localScale;

            while (Time.time - startTime <= Values.BlockSpawnDuration)
            {
                transform.localScale = Vector2.Lerp(orgVec, endVec, (Time.time - startTime) / Values.BlockSpawnDuration);
                yield return null;
            }

            transform.localScale = endVec;
        }

        private IEnumerator ScaleCoroutine(Vector2 endVec, float duration)
        {
            var startTime = Time.time;
            Vector2 orgVec = transform.localScale;

            while (Time.time - startTime <= duration)
            {
                transform.localScale = Vector2.Lerp(orgVec, endVec, (Time.time - startTime) / duration);
                yield return null;
            }

            transform.localScale = endVec;
        }

        #endregion
        
        
        #region -------Public Method-------

        public void Move(int x, int y)
        {
            if (State == BlockState.Move) 
                return;
            
            if (_scaleCoroutine != null)
                StopCoroutine(_scaleCoroutine);
            
            StartCoroutine(MoveCoroutine(x, y, State));
        }

        public void On()
        {
            StopAllCoroutines();
            
            State = BlockState.Idle;
            transform.localScale = BlockScale;
        }

        public void Spawn()
        {
            if (_scaleCoroutine != null)
                StopCoroutine(_scaleCoroutine);
            
            transform.localPosition = new Vector2(X, Y);

            _scaleCoroutine = ScaleCoroutine(BlockScale);
            StartCoroutine(_scaleCoroutine);
            
            State = BlockState.Idle;
        }

        public void Hide()
        {
            if (_scaleCoroutine != null)
                StopCoroutine(_scaleCoroutine);
            
            _scaleCoroutine = ScaleCoroutine(Vector2.zero);
            StartCoroutine(_scaleCoroutine);
        }

        public void OnHint()
        {
            if (State == BlockState.Hint) 
                return;
            
            State = BlockState.Hint;

            if (_hintCoroutine != null)
                StopCoroutine(_hintCoroutine);

            _hintCoroutine = HintCoroutine();
            StartCoroutine(_hintCoroutine);
        }

        public void OffHint()
        {
            State = BlockState.Idle;
            
            if (_hintCoroutine != null)
                StopCoroutine(_hintCoroutine);
            
            SetColor(CurrentColor);
        }

        public void Match()
        {
            if (State == BlockState.MatchWait) 
                return;
            
            State = BlockState.MatchWait;

            StartCoroutine(MatchCoroutine());
            //transform.localScale = Vector2.zero;
        }

        public void SetData(int x, int y)
        {
            BlockData = new BlockData(x, y);
        }

        public void SetData(BlockData data)
        {
            BlockData = data;
        }

        public void SetPosition(int x, int y)
        {
            LocalPosition = new Vector2(x, y);
            BlockData.X = x;
            BlockData.Y = y;
        }

        public void SetData()
        {
            BlockData.X = (int)transform.localPosition.x;
            BlockData.Y = (int)transform.localPosition.y;
            
            gameObject.name = string.Format("Block({0}, {1})", BlockData.X, BlockData.Y);
        }

        public void SetColor(Colors color)
        {
            CurrentColor = color;
            Render.color = Palette.GetInstance.GetColor(color);
        }

        public void SetRandomColor()
        {
            Fader.FadeColor(Palette.GetInstance.GetRandomColor(out CurrentColor));
        }

        public void SetRandomColor(bool fade)
        {
            if (!fade)
                Render.color = Palette.GetInstance.GetRandomColor(out CurrentColor);
            else
                Fader.FadeColor(Palette.GetInstance.GetRandomColor(out CurrentColor));
        }

        public void SetExclusionRandomColor()
        {
            Fader.FadeColor(Palette.GetInstance.GetExclusionRandomColor(CurrentColor, out CurrentColor));
        }

        public void Select(out Block block)
        {
            if (State == BlockState.Select)
            {
                block = null;
                return;
            }

            block = this;
            State = BlockState.Select;
            StartCoroutine(ScaleCoroutine(SelectedScale, .1f));
        }

        public void DeSelect(out Block block)
        {
            block = null;
            State = BlockState.Idle;
            StartCoroutine(ScaleCoroutine(BlockScale, .1f));
        }

        public void Wait()
        {
            transform.localScale = BlockScale;
        }

        #endregion
    }
}