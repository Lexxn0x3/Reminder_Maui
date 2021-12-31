using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Timers;

namespace Reminder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static public System.IO.FileInfo dataFile = new System.IO.FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @$"{System.IO.Path.DirectorySeparatorChar}Reminder App{System.IO.Path.DirectorySeparatorChar}data.dat");
        Timer[] timer = new Timer[2];
        public MainWindow()
        {
            InitializeComponent();

            OnTimedEventRefreshReminders(null, null);
            timer[0] = new System.Timers.Timer(3600000);
            timer[0].Elapsed += OnTimedEventRefreshReminders;
            timer[0].AutoReset = true;
            timer[0].Enabled = true;

            OnTimedEventRefreshUpcommingLesson(null, null);
            timer[1] = new System.Timers.Timer(500);
            timer[1].Elapsed += OnTimedEventRefreshUpcommingLesson;
            timer[1].AutoReset = true;
            timer[1].Enabled = true;

        }

        List<ReminderItem> readList()
        {
            List<ReminderItem> list = new List<ReminderItem>();

            if (File.Exists(dataFile.FullName))
            {
                StreamReader sr = new StreamReader(dataFile.FullName);

                while (!sr.EndOfStream)
                {
                    string[] line = sr.ReadLine().Split(';');

                    int a = int.Parse(line[3]);
                    int b = int.Parse(line[2]);
                    int c = int.Parse(line[1]);
                    DateTime date = new DateTime(a, b, c);

                    list.Add(new ReminderItem(date, line[0]));
                }
                sr.Close();
            }
            else
            {
                Directory.CreateDirectory(dataFile.Directory.FullName);
                File.Create(dataFile.FullName).Close();
            }
            return list;
        }

        void writeList(List<ReminderItem> list)
        {
            if (!File.Exists(dataFile.FullName))
            {
                Directory.CreateDirectory(dataFile.Directory.FullName);
                File.Create(dataFile.FullName).Close();
            }
            StreamWriter streamWriter = new StreamWriter(dataFile.FullName);

            foreach (ReminderItem item in list)
            {
                streamWriter.WriteLine(item.ToString());
            }

            streamWriter.Close();
        }

        private void OnTimedEventRefreshReminders(Object source, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                List<ReminderItem> list = readList();
                dataGrid.ItemsSource = list;
                
                textBlock.Text = ReminderItem.daysAndWeeksRemaining(list[0]) + " remaining!!!";
            });
        }
        private void OnTimedEventRefreshUpcommingLesson(Object source, ElapsedEventArgs e)
        {
            Timetable timetable = new Timetable();

            this.Dispatcher.Invoke(() =>
            {
                Lesson nextLesson = timetable.NextLesson();
                string days = "";
                if (nextLesson.TimeFromNow().Days != 0)
                    days += nextLesson.TimeFromNow().Days.ToString() + " days and ";
                upcomingLesson.Text = $"Upcoming lesson:\n{nextLesson.Name} at {nextLesson.Time} in {days}{nextLesson.TimeFromNow().ToString(@"hh\:mm\:ss")}";
            });
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            timer[0].Stop();
            timer[1].Stop();
            DragMove();
            timer[0].Start();
            timer[1].Start();
        }

        private void Rectangle_PreviewMouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            if (this.Topmost)
            {
                this.Topmost = false;
                topMostBtn.Fill = new SolidColorBrush(System.Windows.Media.Colors.Yellow);
            }
            else
            {
                this.Topmost = true;
                topMostBtn.Fill = new SolidColorBrush(System.Windows.Media.Colors.LimeGreen);
            }            
        }

        private void Rectangle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            timer[0].Stop();
            timer[1].Stop();

            this.Close();
        }
    }
}
