using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp4
{
    // Интерфейсы
    interface IPrintable
    {
        void PrintInfo(); // показать краткую информацию
    }
    interface IStatistics
    {
        void ShowAverage();   // показать средний балл
        void ShowTwosCount(); // показать количество двоек
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
        public int Score { get; set; } // Оценки
        public override string ToString() { return string.Format("{0}:{1}", Subject, Score); }
    }
    // Реализация интерфейсов
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

        // --- IPrintable ---
        public void PrintInfo()
        {
            Console.WriteLine(string.Format("{0} | группа: {1} | курс: {2} | ср: {3:F2}",
                LastName,
                Group != null ? Group.Name : "-",
                Group != null ? Group.Course.ToString() : "-",
                AverageScore));
        }

        // --- IStatistics ---
        public void ShowAverage()
        {
            Console.WriteLine(string.Format("[{0}] Средний балл: {1:F2}", LastName, AverageScore));
        }

        public void ShowTwosCount()
        {
            int twos = Exams.Count(e => e.Score == 2);
            Console.WriteLine(string.Format("[{0}] Кол-во двоек: {1}", LastName, twos));
        }

        // --- IExamManager ---
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
    // Многоадресный делегат
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

        // 13 — удалить студентов 1-го курса с ровно 3 двойками
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

            // Многоадресный делегат
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
