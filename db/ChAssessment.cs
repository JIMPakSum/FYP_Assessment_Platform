using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WillDevicesSampleApp.Net;

namespace SQLitedb
{
    public class ChAssessment
    {
        public int ChAssessmentId { get; set; }
        public DateTime? TimeStamp { get; set; }
        public float WPM { get; set; }
        public float CharCorrectness { get; set; }
        public float StrokeCount { get; set; }
        public float StrokeOrder { get; set; }
        public float BaseLine { get; set; }
        public float SizeConsistency { get; set; }
        public float Smoothness { get; set; }
        public string StudentName { get; set; }
        public string IncorrectStroke { get; set; }
        public int Total{ get { return Convert.ToInt32((CharCorrectness + StrokeCount + StrokeOrder + BaseLine + SizeConsistency + Smoothness)) / 6; }  }
        public string Grade { get {return ResultPage.CalculateOveralGrade((double)Convert.ToInt32((CharCorrectness + StrokeCount + StrokeOrder + BaseLine + SizeConsistency + Smoothness)) / 6, (double)WPM, true) ; } }
    }
}
