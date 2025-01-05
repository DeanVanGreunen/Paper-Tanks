using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client
{
    public struct Achievement
    {
        public string name;
        public string description;

        public override bool Equals(object obj)
        {
            return ( (Achievement) obj ).name == name;
        }

        public override int GetHashCode()
        {
            return name.ToCharArray().GetHashCode();
        }

        public static bool operator ==(Achievement left, Achievement right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Achievement left, Achievement right)
        {
            return !( left == right );
        }
    }
}
