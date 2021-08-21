using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessNumber.Game.Core
{
    abstract class PuzzleSolverBase
    {
        public abstract Guess NextGuess();

        public abstract void SetResponse(Response result);

        public abstract void ApplyRule(Guess guess, Response result);

        public abstract int ReportGuessTimes();

        public abstract string FormatMatrix();

        public abstract string FormatGuessCandidates();

        public abstract List<Guess> GuessCandidates { get; }
        public abstract int[,] WeightMatrix { get; }
    }
}
