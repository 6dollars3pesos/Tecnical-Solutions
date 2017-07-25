using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
//using Aimtec;

namespace TecnicalGangplank
{
    class Test
    {
        private Dictionary<BuffType, bool> EnabledBuffs = new Dictionary<BuffType, bool>
        {
            {BuffType.Blind, true},
            {BuffType.Stun, false},
            {BuffType.Fear, false},
            {BuffType.Taunt, false},
            {BuffType.Poison, false},
            {BuffType.Slow, false},
            {BuffType.Suppression, false},
            {BuffType.Silence, false},
            {BuffType.Snare, false}
        };

//        public static void Main(string[] args)
//        {
//            Test test = new Test();
//            test.ReflectionTest();
//        }

        public void ReflectionTest()
        {
            var field = (Dictionary<BuffType, bool>)GetType().GetField("EnabledBuffs", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
            Console.WriteLine(field[BuffType.Blind]);
            Console.ReadKey();
        }
    }

    enum BuffType
    {
        Blind,
        Stun,
        Fear,
        Taunt,
        Poison,
        Slow,
        Suppression,
        Silence,
        Snare
    }
}
