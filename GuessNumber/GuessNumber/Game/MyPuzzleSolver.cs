using GuessNumber.Game.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessNumber.Game
{
    class MyPuzzleSolver : PuzzleSolverBase
    {
        public class NoSolutionException : Exception
        {
            public NoSolutionException() : base("Unable to find a proper solution.")
            {

            }
        }

        public override Guess NextGuess()
        {
            if(_guessCandidates.Count == 0)
            {
                throw new NoSolutionException();
            }

            _guessTimes++;
            _lastGuess = _guessCandidates[0];
            if (_guessTimes == 1)
                return _lastGuess;

            ulong total_times = 1;
            foreach (Guess guess in _guessCandidates)
            {
                ulong times_of_current = 1;
                for (int i = 0; i < Config.Slots; i++)
                {
                    times_of_current *= (ulong)_weightMatrix[i, guess[i]];
                }
                // find the most possible guess
                if (times_of_current > total_times)
                {
                    total_times = times_of_current;
                    _lastGuess = guess;
                }
            }
            return _lastGuess;
        }

        public override void SetResponse(Response result)
        {
            ApplyRule(_lastGuess, result);
            //// Filter possibilities
            //List<Guess> toBeRemoved = new List<Guess>();
            //foreach (Guess guess in _guessCandidates)
            //{
            //    if (!(guess.GetResult(_lastGuess) == result))
            //    {
            //        toBeRemoved.Add(guess);
            //    }
            //}

            //foreach (Guess guess in toBeRemoved)
            //{
            //    RemovePossibility(guess);
            //}

            //toBeRemoved.Clear();
        }

        public override void ApplyRule(Guess guess, Response result)
        {
            // Filter possibilities
            List<Guess> toBeRemoved = new List<Guess>();
            foreach (Guess g in _guessCandidates)
            {
                if (!(g.GetResult(guess) == result))
                {
                    toBeRemoved.Add(g);
                }
            }

            foreach (Guess g in toBeRemoved)
            {
                RemovePossibility(g);
            }

            toBeRemoved.Clear();
        }

        public override int ReportGuessTimes()
        {
            return _guessTimes;
        }

        public MyPuzzleSolver()
        {
            Init();
        }

        public override string FormatMatrix()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{_guessCandidates.Count} possibilities remain.");
            for (int i = 0; i < Config.Slots; i++)
            {
                stringBuilder.AppendLine(i.ToString());
                for (int j = 0; j < Config.Range; j++)
                {
                    stringBuilder.Append($"{j} => {_weightMatrix[i, j]} ");
                }
                stringBuilder.AppendLine();
            }
            return stringBuilder.ToString();
        }

        public override string FormatGuessCandidates()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Guess guess in _guessCandidates)
            {
                stringBuilder.Append(guess.ToString());
                stringBuilder.AppendLine();
            }
            return stringBuilder.ToString();
        }

        public override List<Guess> GuessCandidates
        {
            get
            {
                return _guessCandidates;
            }
        }

        public override int[,] WeightMatrix
        {
            get
            {
                return _weightMatrix;
            }
        }

        private void Init()
        {
            _lastGuess = null;
            _guessTimes = 0;
            _guessCandidates.Clear();
            Guess tempGuess = new Guess();
            tempGuess.Reset();
            for (int i = 0; i < Config.Slots; i++)
            {
                for (int j = 0; j < Config.Range; j++)
                {
                    _weightMatrix[i, j] = 0;
                }
            }

            // Enum all possibilities & Update weight matrix
            while (!tempGuess.IsMAX())
            {
                Guess newGuess = new Guess(tempGuess);
                AddPossibility(newGuess);
                tempGuess.NextValid();
            }
        }

        private void AddPossibility(Guess guess)
        {
            if (!guess.IsValid())
                return;

            for (int i = 0; i < Config.Slots; i++)
            {
                _weightMatrix[i, guess[i]] += 1;
            }
            _guessCandidates.Add(guess);
        }

        private void RemovePossibility(Guess guess)
        {
            if (!guess.IsValid())
                return;

            for (int i = 0; i < Config.Slots; i++)
            {
                _weightMatrix[i, guess[i]] -= 1;
            }
            _guessCandidates.Remove(guess);
        }

        private int[,] _weightMatrix = new int[Config.Slots, Config.Range];

        private List<Guess> _guessCandidates = new List<Guess>();

        private Guess _lastGuess = new Guess();

        int _guessTimes = 0;
    }
}
