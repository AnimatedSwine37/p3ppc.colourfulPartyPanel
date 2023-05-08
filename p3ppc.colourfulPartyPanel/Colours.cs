using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace p3ppc.colourfulPartyPanel
{
    public class Colours
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Colour
        {
            public byte R;
            public byte G;
            public byte B;
            public byte A;
        }

        internal static Colour[] memberColours = new Colour[] 
        {
            new Colour() { R = 213, G = 143, B = 171, A = 255 }, // Yukari
            new Colour() { R = 228, G = 222, B = 138, A = 255 }, // Aigis
            new Colour() { R = 222, G = 95, B = 104, A = 255 }, // Mitsuru
            new Colour() { R = 87, G = 122, B = 162, A = 255 }, // Junpei
            new Colour() { R = 255, G = 255, B = 255, A = 255 }, // Fuuka (Unused Obviously)
            new Colour() { R = 195, G = 0, B = 19, A = 255 }, // Akihiko
            new Colour() { R = 255, G = 144, B = 91, A = 255 }, // Ken
            new Colour() { R = 166, G = 38, B = 89, A = 255 }, // Shinjiro
            new Colour() { R = 173, G = 173, B = 173, A = 255 }, // Koromaru
        };

        internal static Colour maleColour = new Colour() { R = 134, G = 166, B = 191, A = 255 };
        internal static Colour femaleColour = new Colour() { R = 134, G = 166, B = 191, A = 255 };
    }
}
