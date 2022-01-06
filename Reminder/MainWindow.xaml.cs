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
        //static public System.IO.FileInfo dataFile =  GetDataFile("data.dat");
        Timer[] timer = new Timer[3];
        DateTime timeStart = DateTime.Now;
        TimeSpan studyTime = TimeSpan.Zero;
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

            studyTime = ReadStudyTime();
            OnTimedEventRefreshStudyTime(null, null);
            timer[2] = new System.Timers.Timer(500);
            timer[2].Elapsed += OnTimedEventRefreshStudyTime;
            timer[2].AutoReset = true;
            timer[2].Enabled = false;
        }

        List<ReminderItem> readList()
        {
            List<ReminderItem> list = new List<ReminderItem>();

            FileInfo dataFile = GetDataFile("data.dat");

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

                    if (date > DateTime.Now)
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

        static void writeList(List<ReminderItem> list)
        {
            FileInfo dataFile = GetDataFile("data.dat");

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
        private void OnTimedEventRefreshStudyTime(Object source, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                studyTime += DateTime.Now - timeStart;
                timeStart = DateTime.Now;

                studyTimeTextBlock.Text = $"You already studied {studyTime.ToString(@"hh\:mm\:ss")} today";
            });
        }

        private void OnTimedEventRefreshReminders(Object source, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                List<ReminderItem> list = readList();
                dataGrid.ItemsSource = list;

                try
                {
                    textBlock.Text = ReminderItem.daysAndWeeksRemaining(list[0]) + " remaining!!!";
                }
                catch (Exception)
                {
                    textBlock.Text = "Not really much to look forward too";
                }
                writeList(list);
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
            //timer[0].Stop();
            //timer[1].Stop();
            DragMove();
            //timer[0].Start();
            //timer[1].Start();
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

        TimeSpan ReadStudyTime()
        {
            TimeSpan timeSpan = new();
            bool newFile = false;
            FileInfo fileInfo = GetDataFile("studyTime.csv", out newFile);

            if (!newFile)
            {
                bool success = false;
                while (!success)
                {
                    try
                    {
                        StreamReader sr = new StreamReader(fileInfo.FullName);
                        string[] buffer = sr.ReadToEnd().Split('\n');

                        if (buffer.Length <= 1)
                        {
                            success = true;
                            break;
                        }

                        if (buffer[buffer.Length - 1] == "")
                            buffer = buffer[..(buffer.Length - 1)];

                        string[] lastEntry = buffer[buffer.Length - 1].Split(';');

                        if (checkIfToday(lastEntry[0], lastEntry[1], lastEntry[2]))
                        {
                            timeSpan = new TimeSpan(0, 0, (int)Convert.ToDouble(lastEntry[3]));
                        }
                        success = true;
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show(ex.Message, "IO Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
                        throw;
                    }
                }
            }

            return timeSpan;
        }

        bool checkIfToday(string day, string month, string year)
        {
            DateTime today = DateTime.Now;
            return (Convert.ToInt32(day) == today.Day && Convert.ToInt32(month) == today.Month && Convert.ToInt32(year) == today.Year) ;
        }
        void WriteStudyTime()
        {
            bool newFile;
            FileInfo dataFile = GetDataFile("studyTime.csv", out newFile);

            string[] buffer;

            StreamReader streamReader;

            bool success = false;
            while (!success)
            {
                try
                {
                    streamReader = new StreamReader(dataFile.FullName);
                    buffer = streamReader.ReadToEnd().Split('\n');
                    streamReader.Close();

                    DateTime today = DateTime.Now;

                    StreamWriter sw = new StreamWriter(dataFile.FullName);

                    string[] lastEntry;
                    if (buffer.Length > 1)
                    {
                        if (buffer[buffer.Length - 1] == "")
                            buffer = buffer[..(buffer.Length - 1)];

                        lastEntry = buffer[buffer.Length - 1].Split(';');

                        //if (Convert.ToInt32(lastEntry[0]) == today.Day && Convert.ToInt32(lastEntry[1]) == today.Month && Convert.ToInt32(lastEntry[2]) == today.Year)
                        if (checkIfToday(lastEntry[0], lastEntry[1], lastEntry[2]))
                        {
                            buffer[buffer.Length - 1] = $"{lastEntry[0]};{lastEntry[1]};{lastEntry[2]};{studyTime.TotalSeconds}";

                            for (int i = 0; i < buffer.Length; i++)
                            {

                                if (i == buffer.Length - 1)
                                    sw.Write(buffer[i].Trim());
                                else
                                    sw.WriteLine(buffer[i].Trim());
                            }
                        }
                        else
                        {
                            foreach (string str in buffer)
                            {
                                sw.WriteLine(str.Trim());
                            }

                            sw.Write($"{today.Day};{today.Month};{today.Year};{studyTime.TotalSeconds}");
                        }
                    }
                    else
                    {
                        sw.WriteLine("day;month;year;studyTimeInSeconds");
                        sw.Write($"{today.Day};{today.Month};{today.Year};{studyTime.TotalSeconds}");
                    }

                    success = true;
                    sw.Close();
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Could not open/save studyTime.csv\nCheck if the file is currently in use!\n\n{ex.Message}", "IOException", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open studyTime.csv\n\n{ex.Message}", "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw;
                }
            }
        }

        public static System.IO.FileInfo GetDataFile(string name, out bool newFile)
        {
            newFile = false;
            System.IO.FileInfo dataFile = new System.IO.FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @$"{System.IO.Path.DirectorySeparatorChar}Reminder App{System.IO.Path.DirectorySeparatorChar}{name}");
            if (!File.Exists(dataFile.FullName))
            {
                newFile = true;

                Directory.CreateDirectory(dataFile.Directory.FullName);
                File.Create(dataFile.FullName).Close();
            }

            return dataFile;
        }
        public static System.IO.FileInfo GetDataFile(string name)
        {
            System.IO.FileInfo dataFile = new System.IO.FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @$"{System.IO.Path.DirectorySeparatorChar}Reminder App{System.IO.Path.DirectorySeparatorChar}{name}");
            if (!File.Exists(dataFile.FullName))
            {

                Directory.CreateDirectory(dataFile.Directory.FullName);
                File.Create(dataFile.FullName).Close();
            }

            return dataFile;
        }

        private void Rectangle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            timer[0].Stop();
            timer[1].Stop();

            WriteStudyTime();

            this.Close();
        }

        private void studyPauseButton_Click(object sender, RoutedEventArgs e)
        {
            timer[2].Enabled = false;

            studyResumeButton.IsEnabled = true;
            studyResumeButton.IsChecked = false;
            studyPauseButton.IsEnabled = false;

            studyPauseButton.Foreground = Brushes.White;
            studyResumeButton.Foreground = Brushes.Gray;
        }

        private void studyResumeButton_Click(object sender, RoutedEventArgs e)
        {
            timeStart = DateTime.Now;
            timer[2].Enabled = true;

            studyPauseButton.IsEnabled = true;
            studyPauseButton.IsChecked = false;
            studyResumeButton.IsEnabled = false;

            studyPauseButton.Foreground = Brushes.Gray;
            studyResumeButton.Foreground = Brushes.Red;
        }
    }
}
