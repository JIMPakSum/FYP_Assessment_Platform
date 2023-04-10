using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WillDevicesSampleApp.Net;

namespace SQLitedb
{
    public class EngAssessment
    {
        public int EngAssessmentId { get; set; }
        public DateTime? TimeStamp { get; set; }
        public float WPM { get; set; }
        public float CharCorrectness { get; set; }
        public float BaseLine { get; set; }
        public float SizeConsistency { get; set; }
        public float WordTilt { get; set; }
        public float Smoothness { get; set; }
        public string StudentName { get; set; }
        public int Total { get { return Convert.ToInt32((CharCorrectness + BaseLine + SizeConsistency + WordTilt + Smoothness) / 5); } }
        public string Grade { get { return ResultPage.CalculateOveralGrade((double)Convert.ToInt32((CharCorrectness + WordTilt + BaseLine + SizeConsistency + Smoothness)) / 5, (double)WPM, false); } }

    }
}
