using System.Collections.Generic;

namespace Script
{
    public class MoveResult
    {
        public List<BoardSequence> BoardResult { get; private set; }
        public int Score { get; private set; }

        public MoveResult(List<BoardSequence> sequence, int score)
        {
            BoardResult = sequence;
            Score = score;
        }
    }
}