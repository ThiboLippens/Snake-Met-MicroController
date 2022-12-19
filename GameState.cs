
using System;
using System.Collections.Generic;
using System.Printing;
using System.Runtime.CompilerServices;

namespace Project_6_12_2022
{
    internal class GameState
    {
        public int Rows { get; }
        public int Cols { get; }
        public GridValue[,] Grid { get; }
        public Direction Dir { get; private set; } //53.34
        public int Score { get; private set; }
        public bool GameOver { get; private set; }

        private readonly LinkedList<Direction> dirChanges = new LinkedList<Direction>();
        private readonly LinkedList<SnakePosition> snakePositions = new LinkedList<SnakePosition>();
        private readonly Random random = new Random();

        public GameState(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Grid = new GridValue[rows, cols];
            Dir = Direction.Right;

            AddSnake();
            AddFood();
        }
        private void AddSnake()
        {
            int r = Rows / 2;

            for(int c =1; c <= 2; c++)
            {
                Grid[r, c] = GridValue.Snake;
                snakePositions.AddFirst(new SnakePosition(r, c));
            }
        }
        private IEnumerable<SnakePosition> EmptyPositions()
        {
            for(int r = 0; r < Rows; r++)
            {
                for(int c =0; c < Cols; c++)
                {
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new SnakePosition(r, c);
                    }
                }
            }
        }

        private void AddFood()
        {
            List < SnakePosition> empty = new List<SnakePosition> (EmptyPositions());

            if(empty.Count == 0)
            {
                return;
            }

            SnakePosition pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Col] = GridValue.Food;
        }

        public SnakePosition HeadPosition()
        {
            return snakePositions.First.Value;
        }

        public SnakePosition TailPosition()
        {
            return snakePositions.Last.Value;
        }
        public IEnumerable<SnakePosition> SnakePositions()
        {
            return snakePositions;
        }
        private void AddHead(SnakePosition pos)
        {
            snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Col] = GridValue.Snake;
        }
        private void RemoveTail()
        {
            SnakePosition tail = snakePositions.Last.Value;
            Grid[tail.Row, tail.Col] = GridValue.Empty;
            snakePositions.RemoveLast();
        }

        private Direction GetLastDirection()
        {
            if(dirChanges.Count == 0)
            {
                return Dir;
            }

            return dirChanges.Last.Value;
        }

        private bool CanChangeLastDirection(Direction newDir)
        {
            if(dirChanges.Count == 2)
            {
                return false;
            }

            Direction lastDir = GetLastDirection();
            return newDir != lastDir && newDir != lastDir.Opposite();
        }

        public void ChangeDirection(Direction dir)
        {
            if (CanChangeLastDirection(dir))
            {
                dirChanges.AddLast(dir);
            }
        }

        private bool OutsideGrid(SnakePosition pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Cols;
        }
        private GridValue WillHit(SnakePosition newHeadPos)
        {
            if (OutsideGrid(newHeadPos))
            {
                return GridValue.Outside;
            }
            if(newHeadPos == TailPosition())
            {
                return GridValue.Empty;
            }

            return Grid[newHeadPos.Row, newHeadPos.Col];
        }

        public void Move()
        {
            if(dirChanges.Count > 0)
            {
                Dir = dirChanges.First.Value;
                dirChanges.RemoveFirst();
            }

            SnakePosition newHeadPos = HeadPosition().Translate(Dir);
            GridValue hit = WillHit(newHeadPos);

            if(hit == GridValue.Outside || hit == GridValue.Snake)
            {
                GameOver = true;
            }
            else if(hit == GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPos);
            }
            else if(hit == GridValue.Food)
            {
                AddHead(newHeadPos);
                Score++;
                AddFood();
            }
        }
    }
}
