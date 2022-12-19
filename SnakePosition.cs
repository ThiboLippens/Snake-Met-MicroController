

using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Project_6_12_2022
{
    public class SnakePosition
    {
        public int Row { get; }
        public int Col { get; }

        public SnakePosition(int row, int col)
        {
            Row = row;
            Col = col;
        }
        public SnakePosition Translate(Direction dir)
        {
            return new SnakePosition(Row + dir.RowOffset, Col + dir.ColOffset);
        }

        public override bool Equals(object? obj)
        {
            return obj is SnakePosition position &&
                   Row == position.Row &&
                   Col == position.Col;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Col);
        }

        public static bool operator ==(SnakePosition? left, SnakePosition? right)
        {
            return EqualityComparer<SnakePosition>.Default.Equals(left, right);
        }

        public static bool operator !=(SnakePosition? left, SnakePosition? right)
        {
            return !(left == right);
        }
    }
}
