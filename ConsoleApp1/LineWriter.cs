using System;
using System.Collections.Generic;
using System.Text;
namespace SVGToPrefab
{
    class LineWriter
    {
        public static List<string> console = new List<string>();
        public static bool notRead() {
            return console.Count > 0;
        }
        public static void WriteLine(string line)
        {
            System.Diagnostics.Debug.WriteLine(line);
            console.Add(line);
        }
        public static void WriteLine(object? line)
        {
            System.Diagnostics.Debug.WriteLine(line);
            console.Add(line.ToString());
        }
    }
}
