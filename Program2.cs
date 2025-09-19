using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    internal class Program
    {
        public abstract class State
        {
            protected string Name;

            public State(string name)
            {
                Name = name;
            }

            public virtual void Print()
            {
                Console.WriteLine(Name);
            }
        }

        public class Monarchy : State
        {
            protected string RulerTitle;
            protected string RulerName;
            protected int KingdomsCount;

            public Monarchy(int kingdomsCount, string rulerTitle, string rulerName, string name)
                : base(name)
            {
                KingdomsCount = kingdomsCount;
                RulerTitle = rulerTitle;
                RulerName = rulerName;
            }

            public override void Print()
            {
                Console.WriteLine($"Монархия {Name}: правит {RulerTitle} {RulerName}, королевств в составе {KingdomsCount}");
            }
        }

        public class Kingdom : Monarchy
        {
            private int ProvincesCount;
            private int Population;

            public Kingdom(int provincesCount, int population, int kingdomsCount, string rulerName, string name)
                : base(kingdomsCount, "Король", rulerName, name)
            {
                ProvincesCount = provincesCount;
                Population = population;
            }

            public override void Print()
            {
                Console.WriteLine($"Королевство {Name}: король {RulerName}, провинций {ProvincesCount}, население {Population}");
            }
        }

        public class Republic : State
        {
            private string PresidentName;
            private int ParliamentSeats;

            public Republic(string presidentName, int parliamentSeats, string name)
                : base(name)
            {
                PresidentName = presidentName;
                ParliamentSeats = parliamentSeats;
            }

            public override void Print()
            {
                Console.WriteLine($"Республика {Name}: президент {PresidentName}, мест в парламенте {ParliamentSeats}");
            }
        }

        static void Main(string[] args)
        {
            State s1 = new Kingdom(30, 12_000_000, 1, "Артур", "Авалон");
            s1.Print();

            State s2 = new Monarchy(3, "Император", "Юстиниан", "Византия");
            s2.Print();

            State s3 = new Republic("Сыров Кирилл", 100, "Новая Республика");
            s3.Print();
        }
    }
}
