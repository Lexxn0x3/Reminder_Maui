using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Reminder
{
    public class Timetable
    {
        List<Lesson> timeTable = new List<Lesson>();

        public Timetable(List<Lesson> lessons)
        {
            timeTable = lessons;
        }

        public Timetable()
        {
            System.IO.FileInfo dataFile = new System.IO.FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @$"{System.IO.Path.DirectorySeparatorChar}Reminder App{System.IO.Path.DirectorySeparatorChar}timetable.dat");

            if (File.Exists(dataFile.FullName))
            {
                StreamReader sr = new StreamReader(dataFile.FullName);

                while (!sr.EndOfStream)
                {
                    string[] line = sr.ReadLine().Split(';');

                    int a = int.Parse(line[1]);
                    int b = int.Parse(line[2]);
                    int c = int.Parse(line[3]);

                    timeTable.Add(new Lesson(a,b,c, line[0]));

                }
                sr.Close();
            }
            else
            {
                Directory.CreateDirectory(dataFile.Directory.FullName);
                File.Create(dataFile.FullName).Close();
            }
        }

        public void SaveTimeTable()
        {
            System.IO.FileInfo dataFile = MainWindow.GetDataFile("data.dat");
            if (File.Exists(dataFile.FullName))
            {
                StreamWriter sw = new StreamWriter(dataFile.FullName);

                foreach (Lesson item in timeTable)
                {
                    sw.WriteLine(item.Name + ";" + (int)item.Day + ";" + item.Time.Hour + ";" + item.Time.Minute);
                }
                
                sw.Close();
            }
            else
            {
                Directory.CreateDirectory(dataFile.Directory.FullName);
                File.Create(dataFile.FullName).Close();
            }
        }

        public Lesson NextLesson()
        {
            DateTime curDate = DateTime.Now;

            Lesson lesson = new Lesson();
            TimeSpan span = new TimeSpan(20,0,0,0);

            foreach (Lesson item in timeTable)
            {
                TimeSpan newTimeSpan = item.TimeFromNow();

                if (newTimeSpan.Milliseconds > 0)
                {
                    if (newTimeSpan < span)
                    {
                        span = newTimeSpan;
                        lesson = item;
                    }
                }
            }
            return lesson;
        }
    }

    public struct Lesson
    {
        public DayOfWeek Day { get;}
        public TimeOnly Time { get;}
        public string Name { get;}

        public Lesson(int day, int hour, int minute, string name)
        {
            Time = new TimeOnly(hour, minute, 0);
            Day = (DayOfWeek)day;
            Name = name;
        }

        public TimeSpan TimeFromNow()
        {
            DateTime curDate = DateTime.Now;
            DateTime nextDate = new DateTime(curDate.Year, curDate.Month, curDate.Day, Time.Hour, Time.Minute, Time.Second);

            try
            {
                for (int count = 0; nextDate.DayOfWeek != Day; count++, nextDate = new DateTime(curDate.Year, curDate.Month, nextDate.Day + 1, Time.Hour, Time.Minute, Time.Second)) ;
            }
            catch (Exception)
            {

            }

            return nextDate - curDate;
        }
    }
}
