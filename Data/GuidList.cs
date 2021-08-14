using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AltFiguraServer.Data
{
    public class GuidList : List<Guid>
    {
        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Count; i++)
            {
                if (i > 0)
                    sb.Append(';');
                sb.Append(this[i]);
            }
            return sb.ToString();
        }

        public static GuidList FromString(string str)
        {
            var list = new GuidList();
            foreach (string part in str.Split(';'))
            {
                if (part.Length == 32)
                    list.Add(Guid.Parse(part));
            }
            return list;
        }
    }
}