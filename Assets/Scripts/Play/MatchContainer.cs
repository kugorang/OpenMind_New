#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;

#endregion

namespace Play
{
    public class MatchContainer : MonoBehaviour
    {
        #region -------Private Field-------

        private Block _firBlock, _secBlock, _hintBlock;
        private Vector2 _preMousePos;
        private const float MoveDistance = .7f, HintDelay = 3f;
        private float _preMatchTime, _waitComboTime;
        private bool _enable;

        private IEnumerator _checkCanMatchCoroutine;

        #endregion
        
        
        #region -------Public Field-------

        public Block[,] BlockList;
        public GameObject BlockPrefab;
        public Camera Camera;
        public int Width, Height;
        public float Pad;
        
        public GameManager GameManager;

        #endregion

        
        #region -------Default Method-------

        private void Start()
        {
            BlockList = new Block[Width, Height];
            const float offset = Values.BlockScale / 2;

            transform.position = new Vector2(-(Width / 2f) + offset, -(Height / 2f) + offset);

            /*if (Screen.width > Screen.height)
            {
                const float hRatio = 1; // height / height = 1
                var wRatio = (float) Screen.width / Screen.height;

                var hViewSize = Height / (hRatio * 2);
                var wViewSize = Width / (wRatio * 2);

                /*Debug.Log("hViewSize :" + hViewSize);
                Debug.Log("wViewSize :" + wViewSize);#1#

                Camera.orthographicSize = hViewSize >= wViewSize ? hViewSize : wViewSize;
                Camera.orthographicSize += Pad;
            }
            else if (Screen.width < Screen.height)
            {
                var hRatio = (float) Screen.height / Screen.width;
                const float wRatio = 1;

                var hViewSize = Width / (wRatio * 2);
                var wViewSize = Height / (hRatio * 2);

                Camera.orthographicSize = hViewSize >= wViewSize ? hViewSize : wViewSize;
                Camera.orthographicSize += Pad;
            }
            else
            {
                Camera.orthographicSize = Width / 2f + Pad;
            }*/
            
            const float hRatio = 1; // height / height = 1
            var wRatio = (float) Screen.width / Screen.height;

            var hViewSize = Height / (hRatio * 2);
            var wViewSize = Width / (wRatio * 2);
            
            Camera.orthographicSize = hViewSize >= wViewSize ? hViewSize : wViewSize;
            Camera.orthographicSize += Pad;

            InitializeBlock();
        }

        private void Update()
        {
            if (!_enable)
            {
                OnContainer();
                return;
            }
            
            if (_waitComboTime > 0)
                return;

            if (Time.time - _preMatchTime >= HintDelay)
            {
                _preMatchTime = Time.time;

                if (_hintBlock && CheckMatchAtBlock(_hintBlock))
                    _hintBlock.OnHint();
                else
                {
                    SetHintBlock();
                    _hintBlock.OnHint();
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                var pos = ScreenToWorldPoint(Input.mousePosition);

                /*Debug.Log(pos);*/

                if (pos.x > -(Width / 2f) && pos.x < Width / 2f && pos.y > -(Height / 2f) && pos.y < Height / 2f)
                {
                    var x = (int) (pos.x + Width / 2f);
                    var y = (int) (pos.y + Height / 2f);

                    if (!CheckClickAble(x))
                        return;

                    ClickedEvent(x, y);
                }
            }

            if (_firBlock && !_secBlock && Input.GetMouseButton(0))
            {
                var pos = ScreenToWorldPoint(Input.mousePosition);

                if (_preMousePos.x - pos.x < -MoveDistance 
                    && _firBlock.X < Width - 1 && CheckClickAble(_firBlock.X + 1))
                {
                    BlockList[_firBlock.X + 1, _firBlock.Y].Select(out _secBlock);
                    CompleteSelect();
                }
                else if (_preMousePos.x - pos.x > MoveDistance 
                         && _firBlock.X > 0 && CheckClickAble(_firBlock.X - 1))
                {
                    BlockList[_firBlock.X - 1, _firBlock.Y].Select(out _secBlock);
                    CompleteSelect();
                }
                else if (_preMousePos.y - pos.y < -MoveDistance 
                         && _firBlock.Y < Height - 1 && CheckClickAble(_firBlock.X))
                {
                    BlockList[_firBlock.X, _firBlock.Y + 1].Select(out _secBlock);
                    CompleteSelect();
                }
                else if (_preMousePos.y - pos.y > MoveDistance 
                         && _firBlock.Y > 0 && CheckClickAble(_firBlock.X))
                {
                    BlockList[_firBlock.X, _firBlock.Y - 1].Select(out _secBlock);
                    CompleteSelect();
                }
            }

            if (Input.GetMouseButtonUp(0) && _firBlock && _secBlock) 
                CompleteSelect();
            
            // Input Click -> Get Position -> Block Enable Check -> First? Second?
        }

        #endregion

        
        #region -------Public Method-------

        public void OnContainer()
        {
            _enable = true;
            
            AllBlockPositionReset();
            AllBlockSpawn();
            AllBlockColorSet();
            
            StartCoroutine(MatchDownCoroutine());
            
            _preMatchTime = Time.time;
        }

        public void OffContainer()
        {
            _enable = false;
            StopAllCoroutines();
            AllBlockHide();
        }

        public void InitializeBlock()
        {
            for (var x = 0; x < Width; ++x)
                // TODO : check match and reset color
                for (var y = 0; y < Height; ++y)
                {
                    var go = Instantiate(BlockPrefab);
                    /*go.transform.parent = transform;*/
                    go.transform.SetParent(transform);
                    go.name = string.Format("Block({0}, {1})", x, y);
                    go.transform.localPosition = new Vector2(x, y);
    
                    var block = go.GetComponent<Block>();
                    BlockList[x, y] = block;
    
                    block.SetData(x, y);
                }
        }

        public void AllBlockHide()
        {
            if (BlockList == null)
                return;

            foreach (var block in BlockList)
            {
                block.StopAllCoroutines();
                block.Hide();
            }
        }

        public void AllBlockSpawn()
        {
            if (BlockList == null)
                return;

            foreach (var block in BlockList)
                block.Spawn();
        }

        public void AllBlockColorSet()
        {
            if (BlockList == null)
                return;

            foreach (var block in BlockList)
            {
                block.SetRandomColor();

                while (CheckMatch(block))
                {
                    /*Debug.Log(string.Format("Pos : {0}, Color : {1}", block.LocalPosition, block.CurrentColor));*/
                    block.SetRandomColor();
                }
            }
        }

        public void FinishedMoveEvent(Block block)
        {
            if (FindMatchAtBlock(block)) 
                MatchProcess();
        }

        public void AllBlockPositionReset()
        {
            for (var x = 0; x < Width; ++x)
                for (var y = 0; y < Height; ++y)
                    BlockList[x, y].SetPosition(x, y);
        }

        #endregion

        
        #region -------Private Method-------

        private void CompleteSelect()
        {
            StartCoroutine(SwapCoroutine());
            
            _firBlock = null;
            _secBlock = null;
        }

        /// <summary>
        ///     checked block click able at line(x)
        /// </summary>
        /// <param name="x">line</param>
        private bool CheckClickAble(int x)
        {
            for (var y = 0; y < Height; ++y)
                if (BlockList[x, y].State == Block.BlockState.MatchWait 
                    || BlockList[x, y].State == Block.BlockState.Move)
                    return false;

            return true;
        }

        /*/// <summary>
        ///     All Blocks in MatchContainer Color Set
        /// </summary>
        /// <param name="delay"> Between Blocks Delay </param>
        /// <returns></returns>
        private IEnumerator AllBlockColorSetWithDelay(float delay)
        {
            if (BlockList == null)
                yield break;

            foreach (var block in BlockList)
            {
                block.SetRandomColor();
                
                while (CheckMatch(block)) 
                    block.SetRandomColor();
                
                yield return new WaitForSeconds(delay);
            }
        }*/

        private const float SwapTime = .1f;

        private IEnumerator SwapCoroutine()
        {
            var a = _firBlock;
            var b = _secBlock;

            if (!CheckClickAble(a.X) || !CheckClickAble(b.X)) // before, check able swap
            {
                a.DeSelect(out a);
                b.DeSelect(out b);
                yield break;
            }

            var firPos = a.LocalPosition;
            var secPos = b.LocalPosition;

            var startTime = Time.time;

            while (Time.time - startTime <= SwapTime)
            {
                a.LocalPosition = Vector2.Lerp(firPos, secPos, (Time.time - startTime) / SwapTime);
                b.LocalPosition = Vector2.Lerp(secPos, firPos, (Time.time - startTime) / SwapTime);
                yield return null;
            }

            a.LocalPosition = secPos;
            b.LocalPosition = firPos;

            Swap(a, b);

            yield return new WaitForSeconds(.1f);

            var matchedA = FindMatchAtBlock(a);
            var matchedB = FindMatchAtBlock(b);

            if (!matchedA && !matchedB) // Don't Matched Block!!
            {
                startTime = Time.time;

                while (Time.time - startTime <= SwapTime)
                {
                    a.LocalPosition = Vector2.Lerp(secPos, firPos, (Time.time - startTime) / SwapTime);
                    b.LocalPosition = Vector2.Lerp(firPos, secPos, (Time.time - startTime) / SwapTime);
                    yield return null;
                }

                a.LocalPosition = firPos;
                b.LocalPosition = secPos;

                Swap(a, b);
            }
            
            if (!matchedA)
                a.DeSelect(out a);
            
            if (!matchedB)
                b.DeSelect(out b);

            if (_matchList.Count != 0)
                MatchProcess();
        }

        private void MatchProcess()
        {
            if (_hintBlock && _hintBlock.State == Block.BlockState.Hint) 
                _hintBlock.OffHint();
            
            _preMatchTime = Time.time;

            foreach (var data in _matchList)
            {
                var x = data.X;
                var y = data.Y;
                var length = data.Length;

                if (BlockList[x, y].State == Block.BlockState.MatchEnd)
                    continue;

                // score ++ 
                CheckComboEnd();

                switch (data.Direction)
                {
                    case MatchData.MatchDirection.Left:
                        for (var i = 0; i < length; ++i)
                            BlockList[x - i, y].Match();
                        break;
                    case MatchData.MatchDirection.Right:
                        for (var i = 0; i < length; ++i)
                            BlockList[x + i, y].Match();
                        break;
                    case MatchData.MatchDirection.Top:
                        for (var i = 0; i < length; ++i)
                            BlockList[x, y + i].Match();
                        break;
                    case MatchData.MatchDirection.Bottom:
                        for (var i = 0; i < length; ++i)
                            BlockList[x, y - i].Match();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _matchList.Clear();
        }

        private void CheckComboEnd()
        {
            if (_waitComboTime > 0)
            {
                _waitComboTime = 0.5f;
                return;
            }

            _waitComboTime = 0.5f;
            StartCoroutine("WaitComboEnd");
        }

        private IEnumerator WaitComboEnd()
        {
            while (_waitComboTime > 0)
            {
                _waitComboTime -= Time.deltaTime;

                yield return null;
            }
        }

        /*private IEnumerator MatchCoroutine()
        {
            foreach (var data in _matchList)
            {
                var x = data.X;
                var y = data.Y;
                var length = data.Length;

                switch (data.Direction)
                {
                    case MatchData.MatchDirection.Left:
                        for (var i = 0; i < length; ++i)
                            BlockList[x - i, y].Hide();
                        break;
                    case MatchData.MatchDirection.Right:
                        for (var i = 0; i < length; ++i)
                            BlockList[x + i, y].Hide();
                        break;
                    case MatchData.MatchDirection.Top:
                        for (var i = 0; i < length; ++i)
                            BlockList[x, y + i].Hide();
                        break;
                    case MatchData.MatchDirection.Bottom:
                        for (var i = 0; i < length; ++i)
                            BlockList[x, y - i].Hide();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            yield return null;
        }*/

        private void ClickedEvent(int x, int y)
        {
            if (BlockList[x, y].State == Block.BlockState.Move 
                || BlockList[x, y].State == Block.BlockState.MatchEnd)
                return;

            if (!_firBlock)
            {
                _preMousePos = ScreenToWorldPoint(Input.mousePosition);
                BlockList[x, y].Select(out _firBlock);
            }
            else if (!_secBlock && _firBlock != BlockList[x, y])
            {
                var disX = _firBlock.X - x;
                var disY = _firBlock.Y - y;

                if (Mathf.Abs(disX) > 1 || Mathf.Abs(disY) > 1)
                {
                    _firBlock.DeSelect(out _firBlock);
                    return;
                }

                BlockList[x, y].Select(out _secBlock);
            }
            else
            {
                if (_firBlock)
                    _firBlock.DeSelect(out _firBlock);

                if (_secBlock)
                    _secBlock.DeSelect(out _secBlock);
            }
        }

        private static Vector2 ScreenToWorldPoint(Vector2 pos)
        {
            // ReSharper disable once PossibleNullReferenceException
            return Camera.main.ScreenToWorldPoint(pos);
        }

        /*private void Swap()
        {
            BlockList[_firBlock.X, _firBlock.Y] = _secBlock;
            BlockList[_secBlock.X, _secBlock.Y] = _firBlock;

            _firBlock.SetData();
            _secBlock.SetData();
        }*/

        private void Swap(Block a, Block b)
        {
            BlockList[a.X, a.Y] = b;
            BlockList[b.X, b.Y] = a;

            a.SetData();
            b.SetData();
        }

        private void Swap(int ax, int ay, int bx, int by)
        {
            var tmp = BlockList[ax, ay];

            BlockList[ax, ay] = BlockList[bx, by];
            BlockList[bx, by] = tmp;
        }

        /*/// <summary>
        ///     match blocks move
        /// </summary>
        private void MatchDown()
        {
            for (var x = 0; x < Width; ++x)
            {
                var stack = new List<Block>();

                for (var y = 0; y < Height; ++y)
                {
                    var block = BlockList[x, y];

                    if (block.State == Block.BlockState.MatchEnd)
                    {
                        if (stack.Contains(block))
                            continue;

                        stack.Add(block);
                        block.LocalPosition = new Vector2(x, Height);
                    }
                    else if (stack.Count != 0)
                    {
                        Swap(x, y, x, y - stack.Count);
                        block.Move(x, y - stack.Count);
                    }
                }

                for (var i = 0; i < stack.Count; ++i)
                {
                    var block = BlockList[x, Height - i - 1];

                    block.On();
                    block.SetRandomColor();
                    block.Move(x, Height - i - 1);
                }
            }
        }*/

        /// <summary>
        ///     Is Recursive Method
        ///     Find match end blocks and swap, move
        ///     Call OnContainer() and every frame
        /// </summary>
        /// <returns></returns>
        private IEnumerator MatchDownCoroutine()
        {
            while (true)
            {
                var changed = false;
            
                for (var x = 0; x < Width; ++x)
                {
                    var stack = new List<Block>();

                    for (var y = 0; y < Height; ++y)
                    {
                        var block = BlockList[x, y];

                        if (block.State == Block.BlockState.MatchEnd)
                        {
                            if (stack.Contains(block))
                                continue;

                            stack.Add(block);
                            block.LocalPosition = new Vector2(x, Height);
                        }
                        else if (stack.Count != 0)
                        {
                            Swap(x, y, x, y - stack.Count);
                            block.Move(x, y - stack.Count);
                        
                            changed = true;
                        }
                    }

                    for (var i = 0; i < stack.Count; ++i)
                    {
                        var block = BlockList[x, Height - i - 1];

                        block.On();
                        block.SetRandomColor();
                        block.Move(x, Height - i - 1);
                    
                        changed = true;
                    }
                }

                if (changed)
                    CheckCanMatch();

                yield return null;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        /// <summary>
        ///     call count only one
        /// </summary>
        private void CheckCanMatch()
        {
            if (_checkCanMatchCoroutine != null)
                StopCoroutine(_checkCanMatchCoroutine);

            _checkCanMatchCoroutine = CheckCanMatchCoroutine();
            StartCoroutine(_checkCanMatchCoroutine);
        }

        private IEnumerator CheckCanMatchCoroutine()
        {
            while (IsMoving())
                yield return null;

            foreach (var block in BlockList)
            {
                if (!CheckMatchAtBlock(block))
                    continue;

                if (_hintBlock && _hintBlock.State != Block.BlockState.Hint)
                    _hintBlock = block;

                yield break;
            }

            AllBlockColorSet();
        }

        private void SetHintBlock()
        {
            var complete = false;
            
            foreach (var block in BlockList)
                if (CheckMatchAtBlock(block))
                {
                    _hintBlock = block;
                    complete = true;
                    break;
                }
                    
            if (!complete)
                AllBlockColorSet();
        }

        private bool IsMoving()
        {
            return BlockList.Cast<Block>().Any(block => block.State == Block.BlockState.Move);
        }

        private readonly List<MatchData> _matchList = new List<MatchData>();

        /*private bool FindMatchAtIdle()
        {
            const bool match = false;

            for (var x = 0; x < Width; ++x)
                for (var y = 0; y < Height; ++y)
                {
                }

            return match;
        }*/

        //check order : left -> right -> top -> bottom
        private bool CheckMatchAtBlock(Block block)
        {
            /*
            //left
            if (x > 0 && y < height - 2 && CompareColor(block, -1, 1, -1, 2)) return true;
            if (x > 0 && y > 1 && CompareColor(block, -1, -1, -1, -2)) return true;
            if (x > 2 && CompareColor(block, -2, 0, -3, 0)) return true;
            //right
            if (x < width - 1 && y < height - 2 && CompareColor(block, 1, 1, 1, 2)) return true;
            if (x < width - 1 && y > 1 && CompareColor(block, 1, -1, 1, -2)) return true;
            if (x < width - 3 && CompareColor(block, 2, 0, 3, 0)) return true;
            //top
            if (x < width - 2 && y < height - 1 && CompareColor(block, 1, 1, 2, 1)) return true;
            if (x > 1 && y < height - 1 && CompareColor(block, -1, 1, -2, 1)) return true;
            if (y < height - 3 && CompareColor(block, 0, 2, 0, 3)) return true;
            //bottom
            if (x < width - 2 && y > 0 && CompareColor(block, 1, -1, 2, -1)) return true;
            if (x > 1 && y > 0 && CompareColor(block, -1, -1, -2, -1)) return true;
            if (y > 2 && CompareColor(block, 0, -2, 0, -3)) return true;
             * */

            //left
            if (CompareColor(block, -1, 1, -1, 2)) 
                return true;
            
            if (CompareColor(block, -1, -1, -1, -2)) 
                return true;
            
            if (CompareColor(block, -2, 0, -3, 0)) 
                return true;
            
            //right
            if (CompareColor(block, 1, 1, 1, 2)) 
                return true;
            
            if (CompareColor(block, 1, -1, 1, -2)) 
                return true;
            
            if (CompareColor(block, 2, 0, 3, 0)) 
                return true;
            
            //top
            if (CompareColor(block, 1, 1, 2, 1)) 
                return true;
            
            if (CompareColor(block, -1, 1, -2, 1)) 
                return true;
            
            if (CompareColor(block, 0, 2, 0, 3)) 
                return true;
            
            //bottom
            if (CompareColor(block, 1, -1, 2, -1)) 
                return true;
            
            return CompareColor(block, -1, -1, -2, -1) || CompareColor(block, 0, -2, 0, -3);
        }

        private bool FindMatchAtBlock(Block block)
        {
            if (block.State == Block.BlockState.MatchEnd) 
                return false;

            //Colors color = block.CurrentColor;

            var hor = CompareColorInHorizontal(block);
            var ver = CompareColorInVertical(block);
            var left = CompareColorInPattern(MatchData.MatchDirection.Left, block);
            var top = CompareColorInPattern(MatchData.MatchDirection.Top, block);
            var right = CompareColorInPattern(MatchData.MatchDirection.Right, block);
            var bottom = CompareColorInPattern(MatchData.MatchDirection.Bottom, block);

            var match = hor || ver || left || top || right || bottom;

            if (!match) 
                return false;

            var length = 0;
            
            if (hor)
            {
                if (left && right)
                {
                    _matchList.Add(new MatchData(MatchData.MatchDirection.Right, 5, block.X - 2, block.Y));
                    length += 5;
                }
                else if (left)
                {
                    _matchList.Add(new MatchData(MatchData.MatchDirection.Right, 4, block.X - 2, block.Y));
                    length += 4;
                }
                else if (right)
                {
                    _matchList.Add(new MatchData(MatchData.MatchDirection.Right, 4, block.X - 1, block.Y));
                    length += 4;
                }
                else
                {
                    _matchList.Add(new MatchData(MatchData.MatchDirection.Right, 3, block.X - 1, block.Y));
                    length += 3;
                }
            }
            else
            {
                if (left)
                {
                    _matchList.Add(new MatchData(MatchData.MatchDirection.Left, 3, block.X, block.Y));
                    length += 3;
                }

                if (right)
                {
                    _matchList.Add(new MatchData(MatchData.MatchDirection.Right, 3, block.X, block.Y));
                    length += 3;
                }
            }

            if (ver)
            {
                if (top && bottom)
                {
                    _matchList.Add(new MatchData(MatchData.MatchDirection.Top, 5, block.X, block.Y - 2));
                    length += 5;
                }
                else if (top)
                {
                    _matchList.Add(new MatchData(MatchData.MatchDirection.Top, 4, block.X, block.Y - 1));
                    length += 4;
                }
                else if (bottom)
                {
                    _matchList.Add(new MatchData(MatchData.MatchDirection.Top, 4, block.X, block.Y - 2));
                    length += 4;
                }
                else
                {
                    _matchList.Add(new MatchData(MatchData.MatchDirection.Top, 3, block.X, block.Y - 1));
                    length += 3;
                }
            }
            else
            {
                if (top)
                {
                    _matchList.Add(new MatchData(MatchData.MatchDirection.Top, 3, block.X, block.Y));
                    length += 3;
                }

                if (bottom)
                {
                    _matchList.Add(new MatchData(MatchData.MatchDirection.Bottom, 3, block.X, block.Y));
                    length += 3;
                }
            }
            
            GameManager.ColorUp(block.CurrentColor, length);
            AudioManager.Instance.MatchPuzzle();

            return true;
        }

        private bool CompareColor(Block block, int fx, int fy, int sx, int sy)
        {
            var color = block.CurrentColor;
            var x = block.X;
            var y = block.Y;

            var x1 = x + fx;
            var y1 = y + fy;
            var x2 = x + sx;
            var y2 = y + sy;

            if (x1 < 0 || x1 > Width - 1 || x2 < 0 || x2 > Width - 1 || y1 < 0 
                || y1 > Height - 1 || y2 < 0 || y2 > Height - 1) 
                return false;

            return color == BlockList[x1, y1].CurrentColor && color == BlockList[x2, y2].CurrentColor;
        }

        /*private bool CompareColor(Colors color, int fx, int fy, int sx, int sy)
        {
            return color == BlockList[fx, fy].CurrentColor && color == BlockList[sx, sy].CurrentColor;
        }*/

        /// <summary>
        ///     Call at Blocks swap Horizontal
        /// </summary>
        /*private bool CompareColorInHorizontal(int x, int y, Colors color)
        {
            return CompareColor(color, x - 1, y) && CompareColor(color, x + 1, y);
        }*/

        private bool CompareColorInHorizontal(Block block)
        {
            var x = block.X;
            var y = block.Y;
            var color = block.CurrentColor;

            return CompareColor(color, x - 1, y) && CompareColor(color, x + 1, y);
        }

        /// <summary>
        ///     Call at Blocks swap Vertical
        /// </summary>
        private bool CompareColorInVertical(Block block)
        {
            var x = block.X;
            var y = block.Y;
            var color = block.CurrentColor;

            return CompareColor(color, x, y - 1) && CompareColor(color, x, y + 1);
        }

        /*private bool CompareColorInVertical(int x, int y, Colors color)
        {
            return CompareColor(color, x, y - 1) && CompareColor(color, x, y + 1);
        }*/

        private bool CompareColorInPattern(MatchData.MatchDirection dir, Block block)
        {
            var x = block.X;
            var y = block.Y;
            var color = block.CurrentColor;

            switch (dir)
            {
                case MatchData.MatchDirection.Left:
                    return CompareColor(color, x - 2, y) && CompareColor(color, x - 1, y);
                case MatchData.MatchDirection.Right:
                    return CompareColor(color, x + 2, y) && CompareColor(color, x + 1, y);
                case MatchData.MatchDirection.Top:
                    return CompareColor(color, x, y + 2) && CompareColor(color, x, y + 1);
                case MatchData.MatchDirection.Bottom:
                    return CompareColor(color, x, y - 2) && CompareColor(color, x, y - 1);
                default:
                    throw new ArgumentOutOfRangeException("dir", dir, null);
            }
        }

        /*private bool CompareColorInPattern(MatchData.MatchDirection dir, int x, int y, Colors color)
        {
            switch (dir)
            {
                case MatchData.MatchDirection.Left:
                    return CompareColor(color, x - 2, y) && CompareColor(color, x - 1, y);
                case MatchData.MatchDirection.Right:
                    return CompareColor(color, x + 2, y) && CompareColor(color, x + 1, y);
                case MatchData.MatchDirection.Top:
                    return CompareColor(color, x, y + 2) && CompareColor(color, x, y + 1);
                case MatchData.MatchDirection.Bottom:
                    return CompareColor(color, x, y - 2) && CompareColor(color, x, y - 1);
                default:
                    throw new ArgumentOutOfRangeException("dir", dir, null);
            }
        }

        private bool CompareColor(Color color, int fx, int fy, int sx, int sy)
        {
            return false;
        }

        private bool CompareColor(Colors a, Colors b)
        {
            return a == b;
        }

        private bool CompareColor(Colors a, PangObject b)
        {
            return a == b.CurrentColor;
        }*/

        private bool CompareColor(Colors a, int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) 
                return false;
            
            return a == BlockList[x, y].CurrentColor;
        }

        /*/// <summary>
        ///     Only Check Match at Position
        /// </summary>
        /// <param name="x">target index x</param>
        /// <param name="y">target index y</param>
        /// <returns></returns>
        private bool CheckMatch(int x, int y)
        {
            var pColor = BlockList[x, y].CurrentColor;

            //left
            if (x > 1 && pColor == BlockList[x - 1, y].CurrentColor 
                      && pColor == BlockList[x - 2, y].CurrentColor) 
                return true;
            
            //right
            if (x < Width - 2 && pColor == BlockList[x + 1, y].CurrentColor 
                              && pColor == BlockList[x + 2, y].CurrentColor) 
                return true;
            
            //top
            if (y < Height - 2 && pColor == BlockList[x, y + 1].CurrentColor 
                               && pColor == BlockList[x, y + 2].CurrentColor) 
                return true;
            
            //bottom
            return y > 1 && pColor == BlockList[x, y - 1].CurrentColor 
                         && pColor == BlockList[x, y - 2].CurrentColor;
        }*/

        private bool CheckMatch(Block block)
        {
            var pColor = block.CurrentColor;
            var x = block.X;
            var y = block.Y;

            //left
            if (x > 1 && pColor == BlockList[x - 1, y].CurrentColor
                      && pColor == BlockList[x - 2, y].CurrentColor)
            {
                /*Debug.Log(string.Format("CheckMatch (left) - Pos : {0}, Color : {1}", block.LocalPosition, pColor));*/
                return true;
            }
            
            //right
            if (x < Width - 2 && pColor == BlockList[x + 1, y].CurrentColor
                              && pColor == BlockList[x + 2, y].CurrentColor)
            {
                /*Debug.Log(string.Format("CheckMatch (right) - Pos : {0}, Color : {1}", block.LocalPosition, pColor));*/
                return true;
            }
            
            //top
            if (y < Height - 2 && pColor == BlockList[x, y + 1].CurrentColor
                               && pColor == BlockList[x, y + 2].CurrentColor)
            {
                /*Debug.Log(string.Format("CheckMatch (top) - Pos : {0}, Color : {1}", block.LocalPosition, pColor));*/
                return true;
            }
            
            //bottom
            return y > 1 && pColor == BlockList[x, y - 1].CurrentColor 
                         && pColor == BlockList[x, y - 2].CurrentColor;
            
            /*Debug.Log(string.Format("CheckMatch (bottom) - Pos : {0}, Color : {1}", block.LocalPosition, pColor));*/
        }

        #endregion
    }
}