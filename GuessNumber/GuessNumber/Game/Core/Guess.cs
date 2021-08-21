using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessNumber.Game.Core
{
    class Guess: IEnumerable<int>
    {
        int[] _array = new int[Config.Slots];

        void Next()
        {
            int i = 0;
            bool next;
            do
            {
                _array[i]++;
                next = false;
                if (_array[i] == Config.Range)
                {
                    _array[i] = 0;
                    next = true;
                }
                i++;
            } while (next);
        }

        public void NextValid()
        {
            Next();
            while (!IsValid() && !IsMAX())
            {
                Next();
            }
        }

        public void Reset()
        {
            for (int i = 0; i < 4; i++)
            { 
                _array[i] = 0;
            }
            NextValid();
        }

        //public bool IsValid()
        //{
        //    for (int i = 0; i < 4; i++)
        //    {
        //        if (i < 0 && i >= 10)
        //            return false;
        //        for (int j = i + 1; j < 4; j++)
        //        {
        //            if (_array[i] == _array[j])
        //            {
        //                return false;
        //            }
        //        }
        //    }
        //    return true;
        //}

        public bool IsValid()
        {
            // First digit cannot be 0
            if (_array[0] == 0)
                return false;

            for (int i = 0; i < Config.Slots; i++)
            {
                // Range 0-9
                if (_array[i] < 0 && _array[i] >= Config.Range)
                    return false;
                // No duplicate
                for (int j = i + 1; j < Config.Slots; j++)
                {
                    if (_array[i] == _array[j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool IsMAX()
        {
            for (int i = 0; i < Config.Slots; i++)
            {
                if (_array[i] != Config.Range - 1)
                    return false;
            }
            return true;
        }

        public int this[int idx]
        {
            get
            {
                return _array[idx];
            }
            set
            {
                _array[idx] = value;
            }
        }

        public Guess()
        {
            Reset();
        }

        public Guess(Guess src)
        {
            for (int i = 0; i < Config.Slots; i++)
            {
                _array[i] = src[i];
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < Config.Slots; i++)
            {
                stringBuilder.Append(_array[i]);
                stringBuilder.Append(" ");
            }
            return stringBuilder.ToString();
        }

        public Response GetResult(Guess guess)
        {
            int A = 0, B = 0;
            for (int i = 0; i < Config.Slots; i++)
            {
                if (guess[i] == _array[i])
                {
                    A++;
                }
                else
                {
                    for (int j = 0; j < Config.Slots; j++)
                    {
                        if (j == i) continue;
                        else
                        {
                            if (guess[i] == _array[j])
                            {
                                B++;
                                break;
                            }
                        }
                    }
                }
            }
            return new Response(A, B);
        }

        public IEnumerator<int> GetEnumerator()
        {
            return ((IEnumerable<int>)_array).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _array.GetEnumerator();
        }
    }
}
