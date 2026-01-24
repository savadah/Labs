using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp4
{
    interface IPrintable { void PrintInfo(); }
    interface IStatistics
    {
        void ShowAverage();
        void ShowTwosCount();
    }
    interface IExamManager
    {
        void AddExam(Exam exam);
        bool RemoveExam(string subject);
    }

    class Institute
    {
        public string Name { get; set; }
        public List<Group> Groups { get; set; } = new List<Group>();
        public override string ToString() { return Name; }
    }

    class Group
    {
        public string Name { get; set; }
        public int Course { get; set; }
        public Institute Institute { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
        public override string ToString() { return string.Format("{0} (курс {1})", Name, Course); }
    }

    class Exam
    {
        public string Subject { get; set; }
        public int Score { get; set; }
        public override string ToString() { return string.Format("{0}:{1}", Subject, Score); }
    }

    class Student : IPrintable, IStatistics, IExamManager
    {
        public string LastName { get; set; }
        public Group Group { get; set; }
        public List<Exam> Exams { get; set; } = new List<Exam>();

        public double AverageScore
        {
            get
            {
                if (Exams.Count == 0) return 0;
                return Exams.Average(e => e.Score);
            }
        }

        public void PrintInfo()
        {
            Console.WriteLine(string.Format("{0} | группа: {1} | курс: {2} | ср: {3:F2}",
                LastName,
                Group != null ? Group.Name : "-",
                Group != null ? Group.Course.ToString() : "-",
                AverageScore));
        }

        public void ShowAverage()
        {
            Console.WriteLine(string.Format("[{0}] Средний балл: {1:F2}", LastName, AverageScore));
        }

        public void ShowTwosCount()
        {
            int twos = Exams.Count(e => e.Score == 2);
            Console.WriteLine(string.Format("[{0}] Кол-во двоек: {1}", LastName, twos));
        }

        public void AddExam(Exam exam)
        {
            Exams.Add(exam);
            Console.WriteLine(string.Format("Добавлен экзамен {0}:{1} для {2}", exam.Subject, exam.Score, LastName));
        }

        public bool RemoveExam(string subject)
        {
            Exam ex = Exams.FirstOrDefault(e => e.Subject.Equals(subject, StringComparison.OrdinalIgnoreCase));
            if (ex == null) return false;
            Exams.Remove(ex);
            Console.WriteLine(string.Format("Удалён экзамен {0} у {1}", subject, LastName));
            return true;
        }
    }

    delegate void StudentAction();

    class DataStore
    {
        public List<Institute> Institutes { get; } = new List<Institute>();

        public void Seed()
        {
            Institute inst = new Institute { Name = "Институт" };
            Group g1 = new Group { Name = "ГруппаA", Course = 1, Institute = inst };
            Group g2 = new Group { Name = "ГруппаB", Course = 2, Institute = inst };
            inst.Groups.Add(g1);
            inst.Groups.Add(g2);
            Institutes.Add(inst);

            Student s1 = new Student { LastName = "Иванов", Group = g1 };
            s1.Exams.AddRange(new Exam[]
            {
                new Exam{Subject="Математика", Score=2},
                new Exam{Subject="Физика", Score=2},
                new Exam{Subject="Программирование", Score=2},
                new Exam{Subject="История", Score=4}
            });

            Student s2 = new Student { LastName = "Петров", Group = g1 };
            s2.Exams.AddRange(new Exam[]
            {
                new Exam{Subject="Математика", Score=5},
                new Exam{Subject="Физика", Score=4},
                new Exam{Subject="Программирование", Score=4},
                new Exam{Subject="История", Score=4}
            });

            Student s3 = new Student { LastName = "Козлов", Group = g2 };
            s3.Exams.AddRange(new Exam[]
            {
                new Exam{Subject="Математика", Score=2},
                new Exam{Subject="Физика", Score=2},
                new Exam{Subject="Программирование", Score=2}
            });

            g1.Students.Add(s1);
            g1.Students.Add(s2);
            g2.Students.Add(s3);
        }

        public List<Student> ExecuteQuery13_RemoveFirstCourseWithExactlyThreeTwos()
        {
            List<Student> removed = new List<Student>();
            foreach (Institute inst in Institutes)
            {
                foreach (Group g in inst.Groups.Where(x => x.Course == 1))
                {
                    List<Student> toRemove = g.Students.Where(s => s.Exams.Count(e => e.Score == 2) == 3).ToList();
                    if (toRemove.Count > 0)
                    {
                        removed.AddRange(toRemove);
                        g.Students.RemoveAll(s => toRemove.Contains(s));
                    }
                }
            }
            return removed;
        }
    }

    // обработка ошибок через событие + наследование (переопределение события)

    class LabErrorEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }
        public string Context { get; private set; }
        public DateTime Time { get; private set; }

        public LabErrorEventArgs(Exception ex, string context)
        {
            Exception = ex;
            Context = context;
            Time = DateTime.Now;
        }
    }

    class ErrorSource
    {
        protected EventHandler<LabErrorEventArgs> handlers;

        public virtual event EventHandler<LabErrorEventArgs> ErrorHappened
        {
            add { handlers += value; }
            remove { handlers -= value; }
        }

        protected virtual void OnError(Exception ex, string context)
        {
            if (handlers != null)
                handlers(this, new LabErrorEventArgs(ex, context));
        }
    }

    class Lab4ErrorSource : ErrorSource
    {
        // "Переопределили событие" (add/remove) через наследование
        public override event EventHandler<LabErrorEventArgs> ErrorHappened
        {
            add
            {
                Console.WriteLine("Подписка на обработку ошибок ЛР4.");
                handlers += value;
            }
            remove { handlers -= value; }
        }

        protected override void OnError(Exception ex, string context)
        {
            string nice = context;

            if (ex is StackOverflowException) nice += " | StackOverflowException";
            else if (ex is ArrayTypeMismatchException) nice += " | ArrayTypeMismatchException";
            else if (ex is DivideByZeroException) nice += " | DivideByZeroException";
            else if (ex is IndexOutOfRangeException) nice += " | IndexOutOfRangeException";
            else if (ex is InvalidCastException) nice += " | InvalidCastException";
            else if (ex is OutOfMemoryException) nice += " | OutOfMemoryException";
            else if (ex is OverflowException) nice += " | OverflowException";
            else nice += " | " + ex.GetType().Name;

            base.OnError(ex, nice);
        }
    }

    class Lab4Runner : Lab4ErrorSource
    {
        public void RunAll()
        {
            Try(() => SimulateStackOverflow(), "Демо stack overflow (симуляция)");
            Try(() => SimulateArrayTypeMismatch(), "Демо неправильного типа в массиве");
            Try(() => SimulateDivideByZero(), "Демо деления на ноль");
            Try(() => SimulateIndexOutOfRange(), "Демо выхода за границы массива");
            Try(() => SimulateInvalidCast(), "Демо неверного приведения типа");
            Try(() => SimulateOutOfMemory(), "Демо нехватки памяти (симуляция)");
            Try(() => SimulateOverflow(), "Демо переполнения (checked)");
        }

        void Try(Action action, string context)
        {
            try { action(); }
            catch (Exception ex) { OnError(ex, context); }
        }

        void SimulateStackOverflow()
        {
            // Реальный StackOverflowException обычно не ловится, поэтому для ЛР делаем симуляцию:
            throw new StackOverflowException("Симуляция StackOverflowException");
        }

        void SimulateArrayTypeMismatch()
        {
            string[] arr = new string[1];
            object[] obj = arr;     // ковариантность массивов
            obj[0] = 123;           // ArrayTypeMismatchException
        }

        void SimulateDivideByZero()
        {
            int a = 10, b = 0;
            int c = a / b;          // DivideByZeroException
        }

        void SimulateIndexOutOfRange()
        {
            int[] a = new int[2];
            int x = a[10];          // IndexOutOfRangeException
        }

        void SimulateInvalidCast()
        {
            object o = "abc";
            int x = (int)o;         // InvalidCastException
        }

        void SimulateOutOfMemory()
        {
            // реальную память лучше не убивать, просто симулируем
            throw new OutOfMemoryException("Симуляция OutOfMemoryException");
        }

        void SimulateOverflow()
        {
            checked
            {
                int x = int.MaxValue;
                x = x + 1;          // OverflowException
            }
        }
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            DataStore store = new DataStore();
            store.Seed();

            Console.WriteLine("Список студентов до запроса 13");
            PrintAll(store);

            Console.WriteLine("\nДемонстрация интерфейсов и делегата");
            Student demoStudent = store.Institutes.First().Groups.First().Students.First();

            IPrintable ip = demoStudent;
            IStatistics istat = demoStudent;
            IExamManager imgr = demoStudent;

            ip.PrintInfo();
            istat.ShowAverage();
            istat.ShowTwosCount();

            StudentAction actions = demoStudent.PrintInfo;
            actions += demoStudent.ShowAverage;
            actions += demoStudent.ShowTwosCount;

            Console.WriteLine("\nВызов делегата (все методы подряд):");
            actions();

            Console.WriteLine("\nДобавлю экзамен и снова вызову делегат:");
            imgr.AddExam(new Exam { Subject = "Английский", Score = 3 });
            actions();

            Console.WriteLine("\nВыполним запрос 13");
            List<Student> removed = store.ExecuteQuery13_RemoveFirstCourseWithExactlyThreeTwos();
            Console.WriteLine("Удалено студентов: " + removed.Count);
            foreach (Student s in removed)
                Console.WriteLine("  Удалён: " + s.LastName);

            Console.WriteLine("\nСписок студентов после запроса 13");
            PrintAll(store);

            //демонстрация обработки ошибок 
            Console.WriteLine("\nЛР4: обработка ошибок через событие");
            Lab4Runner runner = new Lab4Runner();
            runner.ErrorHappened += OnLabError; // подписка на событие (переопределённое)
            runner.RunAll();
        }

        static void OnLabError(object sender, LabErrorEventArgs e)
        {
            Console.WriteLine(string.Format("!! Ошибка: {0}\n   Контекст: {1}\n   Сообщение: {2}\n",
                e.Exception.GetType().Name,
                e.Context,
                e.Exception.Message));
        }

        static void PrintAll(DataStore store)
        {
            foreach (Institute inst in store.Institutes)
            {
                Console.WriteLine("Институт: " + inst.Name);
                foreach (Group g in inst.Groups)
                {
                    Console.WriteLine("  " + g);
                    if (g.Students.Count == 0)
                        Console.WriteLine("    <студентов нет>");
                    foreach (Student s in g.Students)
                    {
                        string exams = string.Join(", ", s.Exams.Select(e => e.ToString()));
                        Console.WriteLine(string.Format("    {0} | экзамены: {1} | ср: {2:F2}", s.LastName, exams, s.AverageScore));
                    }
                }
            }
        }
    }
}
