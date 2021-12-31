using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reminder
{
    public class ReminderItem
    {
        private DateTime Date;
        public string DateString { get;}
        public string Name { get; }
        public int DaysRemaining { get;}
        public string DaysAndWeeks { get;}

        public ReminderItem(DateTime date, string name)
        {
            this.Date = date.Date;
            this.DateString = date.ToString("dd.MM.yyy");
            this.Name = name;
            this.DaysRemaining = getDaysRemaining();
            this.DaysAndWeeks = daysAndWeeksRemaining();
        }

        public int getDaysRemaining()
        {
            return (Date - DateTime.Today).Days;
        }

        public static string daysAndWeeksRemaining(ReminderItem reminder)
        {
            return $"{reminder.DaysRemaining / 7,0} weeks and {reminder.DaysRemaining % 7} days";
        }

        string daysAndWeeksRemaining()
        {
            return $"{DaysRemaining / 7,0} weeks and {DaysRemaining % 7} days";
        }

        public override string ToString()
        {
            return Name + ";" + Date.Day + ";" + Date.Month + ";" + Date.Year;
        }
    }
}
