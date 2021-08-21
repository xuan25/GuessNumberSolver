using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessNumber.Game.Core
{
    class Response
    {
        public int A = 0;

        public int B = 0;
        public Response()
        {
            A = 0;
            B = 0;
        }
        public Response(int a, int b)
        {
            A = a;
            B = b;
        }

        public static bool operator ==(Response a, Response b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }

            return a.A == b.A && a.B == b.B;
        }

        public static bool operator !=(Response a, Response b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            Response res = obj as Response;
            if (ReferenceEquals(this, res))
            {
                return true;
            }

            if (ReferenceEquals(res, null))
            {
                return false;
            }

            return A == res.A && B == res.B;
        }

        public override int GetHashCode()
        {
            return (A << 16) | B;
        }

        public bool IsValid()
        {
            return A != -1 && B != -1;
        }

        public override string ToString()
        {
            return $"A: {A}, B: {B}";
        }

        
    }
}
