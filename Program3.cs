using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApp3
{
    //Классы данных
    class Institute
    {
        public string Name { get; set; }
        public List<Group> Groups { get; set; } = new List<Group>();
        public override string ToString() => Name;
    }

    class Group
    {
        public string Name { get; set; }
        public int Course { get; set; } // 1,2,3,...
        public Institute Institute { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
        public override string ToString() => $"{Name} (курс {Course}, {Institute?.Name})";
    }

    class Student
    {
        public string LastName { get; set; }
        public Group Group { get; set; }
        public List<Exam> Exams { get; set; } = new List<Exam>();

        public double AverageScore => Exams.Count == 0 ? 0 : Exams.Average(e => e.Score);

        public override string ToString()
        {
            return $"{LastName} - группа: {Group?.Name}, курс: {Group?.Course}, ин-т: {Group?.Institute?.Name}, ср.балл: {AverageScore:F2}";
        }
    }

    class Exam
    {
        public string Subject { get; set; }
        public int Score { get; set; } // 0..5 2 — двойка, 3-удовл, 4-хорошо, 5-отлично
        public override string ToString() => $"{Subject}:{Score}";
    }

    class Program
    {
        static List<Institute> institutes = new List<Institute>();

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            SeedSampleData(); // чтобы можно было сразу запустить и проверить

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Меню:");
                Console.WriteLine("1. Показать всех студентов");
                Console.WriteLine("2. Выполнить запрос 13 — удалить студентов 1-го курса с тремя двойками");
                Console.WriteLine("3. Добавить студента (быстро)");
                Console.WriteLine("4. Экспортировать текущий список студентов в файл");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите пункт: ");
                var key = Console.ReadLine();
                Console.WriteLine();

                switch (key)
                {
                    case "1": ShowAllStudents(); break;
                    case "2": ExecuteQuery13(); break;
                    case "3": QuickAddStudent(); break;
                    case "4": ExportStudentsToFile(@"students_export.txt"); break;
                    case "0": return;
                    default: Console.WriteLine("Неверный выбор."); break;
                }
            }
        }

        static void SeedSampleData()
        {
            // Создаём институт, группы, студентов — пример
            var inst = new Institute { Name = "Институт Прикладной Информатики" };
            var g1 = new Group { Name = "ПИ-11", Course = 1, Institute = inst };
            var g2 = new Group { Name = "ПИ-21", Course = 2, Institute = inst };
            inst.Groups.AddRange(new[] { g1, g2 });
            institutes.Add(inst);

            // Студенты:
            var s1 = new Student { LastName = "Иванов", Group = g1 };
            s1.Exams.AddRange(new[] {
                new Exam{Subject="Математика", Score=2},
                new Exam{Subject="Физика", Score=2},
                new Exam{Subject="Программирование", Score=2},
                new Exam{Subject="История", Score=4}
            }); // ровно 3 двойки -> должен быть удалён

            var s2 = new Student { LastName = "Петров", Group = g1 };
            s2.Exams.AddRange(new[] {
                new Exam{Subject="Математика", Score=2},
                new Exam{Subject="Физика", Score=3},
                new Exam{Subject="Программирование", Score=2},
                new Exam{Subject="История", Score=2}
            }); // тоже 3 двойки -> удалить

            var s3 = new Student { LastName = "Сидоров", Group = g1 };
            s3.Exams.AddRange(new[] {
                new Exam{Subject="Математика", Score=5},
                new Exam{Subject="Физика", Score=4},
                new Exam{Subject="Программирование", Score=4}
            }); // отличник — не удалять

            var s4 = new Student { LastName = "Козлов", Group = g2 }; // курс 2 — не трогаем
            s4.Exams.AddRange(new[] {
                new Exam{Subject="Математика", Score=2},
                new Exam{Subject="Физика", Score=2},
                new Exam{Subject="Программирование", Score=2},
            });

            g1.Students.AddRange(new[] { s1, s2, s3 });
            g2.Students.Add(s4);
        }

        static void ShowAllStudents()
        {
            foreach (var inst in institutes)
            {
                Console.WriteLine($"Институт: {inst.Name}");
                foreach (var g in inst.Groups)
                {
                    Console.WriteLine($"  Группа: {g.Name} (курс {g.Course})");
                    if (!g.Students.Any()) Console.WriteLine("    <студентов нет>");
                    foreach (var s in g.Students)
                    {
                        Console.WriteLine($"    {s.LastName} | экзамены: {string.Join(", ", s.Exams.Select(e => e.ToString()))} | ср: {s.AverageScore:F2}");
                    }
                }
            }
        }

        // Реализация запроса 13: найти студентов первого курса у которых три двойки (score==2) и удалить их, вывести сообщение и сохранить в файл лог
        static void ExecuteQuery13()
        {
            var removedList = new List<Student>();
            var logBuilder = new StringBuilder();

            foreach (var inst in institutes)
            {
                foreach (var group in inst.Groups.Where(g => g.Course == 1))
                {
                    // Найти студентов для удаления — считаем количество оценок = 2
                    var toRemove = group.Students.Where(s => s.Exams.Count(e => e.Score == 2) == 3).ToList();

                    if (toRemove.Any())
                    {
                        foreach (var s in toRemove)
                        {
                            removedList.Add(s);
                            logBuilder.AppendLine($"Удалён: {s.LastName} | Группа: {group.Name} | Институт: {inst.Name} | Кол-во двоек: {s.Exams.Count(e => e.Score == 2)}");
                        }
                        // Удаляем из группы
                        group.Students.RemoveAll(s => toRemove.Contains(s));
                    }
                }
            }

            // Вывод
            if (!removedList.Any())
            {
                Console.WriteLine("Студентов 1-го курса с ровно тремя двоечками не найдено.");
                logBuilder.AppendLine($"[{DateTime.Now}] Удалений не было.");
            }
            else
            {
                Console.WriteLine($"Удалено {removedList.Count} студентов 1-го курса с тремя двоечками. Подробности записаны в файл.");
            }

            // Сохраняем лог удаления и текущее состояние студентов в файл
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"Query13_Removed_{timestamp}.txt";
            using (var sw = new StreamWriter(filename, false, Encoding.UTF8))
            {
                sw.WriteLine("Лог выполнения запроса 13 (удаление студентов 1-го курса с ровно 3 двоечками)");
                sw.WriteLine("Время: " + DateTime.Now);
                sw.WriteLine();
                sw.WriteLine("Удалённые студенты:");
                sw.WriteLine(logBuilder.ToString());
                sw.WriteLine();
                sw.WriteLine("Текущее состояние (после удаления):");
                foreach (var inst in institutes)
                {
                    sw.WriteLine($"Институт: {inst.Name}");
                    foreach (var g in inst.Groups)
                    {
                        sw.WriteLine($"  Группа: {g.Name} (курс {g.Course})");
                        if (!g.Students.Any()) sw.WriteLine("    <студентов нет>");
                        foreach (var s in g.Students)
                        {
                            sw.WriteLine($"    {s.LastName} | экзамены: {string.Join(", ", s.Exams.Select(e => e.ToString()))} | ср: {s.AverageScore:F2}");
                        }
                    }
                }
            }

            Console.WriteLine($"Лог сохранён в файл: {filename}");
        }

        static void QuickAddStudent()
        {
            Console.Write("Фамилия: ");
            var fam = Console.ReadLine();
            Console.Write("Институт (название): ");
            var instName = Console.ReadLine();
            var inst = institutes.FirstOrDefault(i => i.Name.Equals(instName, StringComparison.OrdinalIgnoreCase));
            if (inst == null)
            {
                inst = new Institute { Name = instName };
                institutes.Add(inst);
            }

            Console.Write("Группа (название): ");
            var groupName = Console.ReadLine();
            Console.Write("Курс (число): ");
            if (!int.TryParse(Console.ReadLine(), out int course)) course = 1;

            var group = inst.Groups.FirstOrDefault(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase) && g.Course == course);
            if (group == null)
            {
                group = new Group { Name = groupName, Course = course, Institute = inst };
                inst.Groups.Add(group);
            }

            var student = new Student { LastName = fam, Group = group };
            Console.WriteLine("Добавим пару экзаменов. Вводите 'предмет оценка' или пустую строку чтобы закончить.");
            while (true)
            {
                Console.Write("exam> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) break;
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && int.TryParse(parts.Last(), out int sc))
                {
                    var subject = string.Join(" ", parts.Take(parts.Length - 1));
                    student.Exams.Add(new Exam { Subject = subject, Score = sc });
                }
                else
                {
                    Console.WriteLine("Неверный формат. Пример: Математика 2");
                }
            }

            group.Students.Add(student);
            Console.WriteLine($"Студент {student.LastName} добавлен в группу {group.Name} (курс {group.Course}, {inst.Name}).");
        }

        static void ExportStudentsToFile(string filename)
        {
            using (var sw = new StreamWriter(filename, false, Encoding.UTF8))
            {
                sw.WriteLine($"Экспорт студентов на {DateTime.Now}");
                sw.WriteLine();
                foreach (var inst in institutes)
                {
                    sw.WriteLine($"Институт: {inst.Name}");
                    foreach (var g in inst.Groups)
                    {
                        sw.WriteLine($"  Группа: {g.Name} (курс {g.Course})");
                        if (!g.Students.Any()) sw.WriteLine("    <студентов нет>");
                        foreach (var s in g.Students)
                        {
                            sw.WriteLine($"    {s.LastName} | экзамены: {string.Join(", ", s.Exams.Select(e => e.ToString()))} | ср: {s.AverageScore:F2}");
                        }
                    }
                }
            }
            Console.WriteLine($"Экспорт выполнен в файл: {filename}");
        }
    }
}
