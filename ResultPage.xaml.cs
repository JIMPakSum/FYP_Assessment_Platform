using Accord.MachineLearning;
using Accord.Statistics.Models.Regression.Linear;
using Newtonsoft.Json;
using ScottPlot;
using SQLitedb;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Infrastructure.Pluralization;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.Json;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Xml.Linq;

namespace WillDevicesSampleApp.Net
{
    /// <summary>
    /// Interaction logic for ResultPage.xaml
    /// </summary>
    public partial class ResultPage : Page
    {
        // 3 kinds of character size for english 
        private char[] tallchar = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'b', 'd', 'f', 'h', 'k', 'l', 'i', 't' };
        private char[] shortchar = new char[] { 'a', 'c', 'e', 'm', 'n', 'o', 'r', 's', 'u', 'v', 'w', 'x', 'z' };
        private char[] descentchar = new char[] { 'g', 'j', 'p', 'q', 'y' };

        //For storing Eng height
        private List<double> tallheightlist = new List<double>();
        private List<double> shortheightlist = new List<double>();
        private List<double> descentheightlist = new List<double>();

        //For storing Chi height & width
        private List<double> chiheight = new List<double>();
        private List<double> chiwidth = new List<double>();

        private List<List<TrajectoryPoint>> _globalstrokelist = new List<List<TrajectoryPoint>>();
        private List<List<Degree>> _strokeDegreeList = new List<List<Degree>>();
        private List<List<double>> _strokeChangerateList = new List<List<double>>();
        private List<List<DegreeDiff>> _strokeDegreeDiffList = new List<List<DegreeDiff>>();
        private List<Degree> _degreelist = new List<Degree>();

        //visualize degree where x axis is time
        private List<Degree> _degreelist2 = new List<Degree>();
        private List<double> _sdlist = new List<double>();
        
        //for titlt caclulation
        private List<double> _avgdegreelist = new List<double>();

        //for smoothness
        private int _windowsize = 5;
        private double _windowthreshold = 1.5;
        private double _sdthreshold = 4;

        //tilt caluclation
        private double refheight = 0;
        private double refheightconst = 0.5;

        //array for storing character item
        private List<JiixChar> _jiixlist = new List<JiixChar>();
        private List<int> _indexOfCorrectChar = new List<int>();

        //Some const for stroke correctness calculation
        private double centroidProportionConst = 3.5;
        private double centroidDegreeThreshold = 70;
        private string strokedebug = "";

        //stroke count
        private double numbercorrectstrokechar = 0;
        private double numberofchar = 0;

        //For Exporting the inccorect stroke out
        private List<IncorrectStroke> _incorrectStrokeList = new List<IncorrectStroke>();

        private dynamic datapassed;
        private JsonObject _jiix;
        private string _WPM;
        private string _WordCorrectness;
        private string _SizeConsistency;
        private string _BaseLine;
        private string _Smoothness;
        private string _WordTilt;
        private string _StrokeCorrectness;
        private string _StrokeCount;

        private string _HandwritingAccuracy;

        //Turn WPM into marks
        private static double _ChiMarkFraction = 11;
        private static double _EngMarkFraction = 12;

        private double _AccuracyScore = 0;

        //Tool tip
        private string WPMtooltip1;
        private string WPMtooltip2;
        private string WCtooltip1;
        private string WCtooltip2;
        private string SCtooltip1;
        private string SCtooltip2;
        private string SCtooltip3;
        private string BLtooltip1;
        private string BLtooltip2;
        private string Stooltip1;
        private string Stooltip2;
        private string WTtooltip1;
        private string WTtooltip2;
        private string StCtooltip1;
        private string StCtooltip2;
        private string StCottooltip1;
        private string StCottooltip2;

        public ResultPage()
        {
            InitializeComponent();
        }

        public ResultPage(dynamic o)
        {
            datapassed = o;

            _jiix = o.Jiix;
            //_strokelist = o.StrokeList;
            // Turn jiix into trajectorypoint class
            GetStrokeListFromJiix(_jiix);
            // Caclulate the degree from the stroke list
            GetDegreeList(_globalstrokelist);
            
            string CurrentLanguage = datapassed.CurrentLanguage;
            string PassageStr = LoadTextBoxPassage(CurrentLanguage, "");
            InitializeComponent();

            if (CurrentLanguage == "Chinese")
            {
                //Chinese marks
                _WPM = CountChiWPM(datapassed.RecognizedWords).ToString();
                _WordCorrectness = CompareChiPassage(datapassed.RecognizedWords, PassageStr).ToString();
                _SizeConsistency = CalculateChiSize(_jiix).ToString();
                _BaseLine = CalculateBaseline(_jiix).ToString();
                _Smoothness = CalculateSmoothness().ToString();
                _StrokeCorrectness = CalculateStrokeCorrectness().ToString();
                _StrokeCount = CalculateStrokeCount().ToString();

                //Hide tilt, shows strokes
                WordTiltGrid.Visibility = Visibility.Collapsed;
                StrokeCountGrid.Visibility = Visibility.Visible;
                StrokeOrderGrid.Visibility = Visibility.Visible;

                StrokeTesting();
                this.StrokeOrder.Text = _StrokeCorrectness;
                this.StrokeCount.Text = _StrokeCount;

                _AccuracyScore = CalculateChiHandwritingAccuracy();


                //add to db
                SaveDataToDatabase(true);

            }
            else
            {
                //English marks
                _WPM = CountEngWPM(datapassed.RecognizedWords).ToString();
                _WordCorrectness = CompareEngPassage(datapassed.RecognizedWords, PassageStr).ToString();
                _SizeConsistency = CalculateEngSize(_jiix).ToString();
                _BaseLine = CalculateBaseline(_jiix).ToString();
                _Smoothness = CalculateSmoothness().ToString();
                _WordTilt = CalculateTilt().ToString();

                _AccuracyScore = CalculateEngHandwritingAccuracy();


                //Show tilt, hide strokes
                WordTiltGrid.Visibility = Visibility.Visible;
                StrokeCountGrid.Visibility = Visibility.Collapsed;
                StrokeOrderGrid.Visibility = Visibility.Collapsed;

                //add to db
                SaveDataToDatabase(false);
            }


            //Set text here
            this.WPM.Text = _WPM;
            this.CharCorrectness.Text = _WordCorrectness;
            this.SizeConsistency.Text = _SizeConsistency;
            this.Baseline.Text = _BaseLine;
            this.Smoothness.Text = _Smoothness;
            this.WordTilt.Text = _WordTilt;

            //Set text for the left big section
            this.AccuracyScore.Text = Convert.ToInt32(_AccuracyScore).ToString();            

            //set tooltip
            this.CCToolTip.ToolTip = "Correct words: " + WCtooltip1 + " Total words: " + WCtooltip2;
            this.BLToolTip.ToolTip = "Avg Slop:" + BLtooltip1;
            this.SToolTip.ToolTip = "Pre-normalized mark: " + Stooltip1;
            if (CurrentLanguage == "Chinese")
            {
                this.SCToolTip.ToolTip = "Height cov: " + SCtooltip1 + " Width cov:" + SCtooltip2;
                this.StrokeOrderGrid.ToolTip = "Correct Stroke: " + StCtooltip1 + " Total Stroke: " + StCtooltip2;
                this.StrokeCountGrid.ToolTip = "Correct Stroke Written: " + StCottooltip1 + " Total Character in Passage: " + StCottooltip2;
                this.Grade.Text = CalculateOveralGrade(_AccuracyScore, Convert.ToDouble(_WPM), true);
            }
            else
            {
                this.SCToolTip.ToolTip = "Tall cov: " + SCtooltip1 + " Short cov:" + SCtooltip2 + " Descent cov:" + SCtooltip3;
                this.WordTiltGrid.ToolTip = "Avg tilt difference from 90 degree :" + WTtooltip1;
                this.Grade.Text = CalculateOveralGrade(_AccuracyScore, Convert.ToDouble(_WPM),false);
            }
            
            

            //signalplotgrah(_rateofchangelist.ToArray());
            //plotgraph2(_strokeDegreeDiffList);
            //plotgraph(_sdlist);

            /*
            foreach (Degree item in _degreelist)
            {
                Debug.WriteLine("degree: " + item._degree.ToString());
                Debug.WriteLine("dt: " + item._dt.ToString());
            }
            */

            
            //Debug.WriteLine("ref:" + refheight);
            

            //Debug.WriteLine("length " + _strokeDegreeList[0].Select(x=>x._l).Sum().ToString());

            

            //degree plot debug
            //plotgrah(generateyarray(_degreelist, 1) ,generateyarray(_degreelist,0) );

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AssessmentPlatormMainPage());
        }

        private string LoadTextBoxPassage(string language, string passage)
        {
            //Get Passage from xml
            string passageaddress = "Passages\\" + language + ".xml";
            XDocument passagexml = XDocument.Load(passageaddress);
            string passagestr = passagexml.Root.Descendants().Where(x => x.Name == "Passage").First().Value.ToString();

            return passagestr;

        }
        private bool IsPuncuation(char character)
        {
            char[] delimiters = new char[] { ' ', '\r', '\n', ',', '.', '?', '\'', '，', '。' };
            foreach (var delimiter in delimiters)
            {
                if (delimiter == character)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsDescent(char character)
        {
            foreach (var delimiter in descentchar)
            {
                if (delimiter == character)
                {
                    return true;
                }
            }
            return false;
        }

        #region WPM
        private int CountEngWPM(string wordsrec)
        {
            if(wordsrec == null)
            {
                return 0;
            }

            int WPM;
            //Count total words
            char[] delimiters = new char[] { ' ', '\r', '\n', ',', '.', '?', '\'' };
            int count = wordsrec.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length;
            WPM = count / 2;

            return WPM;
        }
        private int CountChiWPM(string wordsrec)
        {
            int WPM;
            int count = 0;
            //Count total words
            foreach (char c in wordsrec)
            {
                if (c.Equals(char.Parse(" ")) || c.Equals(char.Parse("，")) || c.Equals(char.Parse("。")))
                {
                    continue;
                }
                else
                {
                    count++;
                }

            }
            WPM = count / 2;

            return WPM;
        }
        #endregion

        #region PassageCompare
        private int CompareEngPassage(string wordsrec, string passagestr)
        {


            char[] delimiters = new char[] { ' ', '\r', '\n', ',', '.', '?', '\'', '，', '。' };
            List<string> wordsreclist = new List<string>();
            List<string> passagestrlist = new List<string>();
            wordsreclist = wordsrec.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).ToList();
            passagestrlist = passagestr.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).ToList();
            Debug.WriteLine(wordsrec);
            Debug.WriteLine(passagestr);
            int i = 0;
            int wordswritten = wordsreclist.Count;

            int correctwords = 0;
            foreach (string word in wordsreclist)
            {
                if (string.Equals(word, passagestrlist[i]))
                {
                    correctwords++;
                }
                i++;
            }

            if (wordswritten == 0)
            {
                return 0;
            }

            WCtooltip1 = correctwords.ToString();
            WCtooltip2 = wordswritten.ToString();

            int correctness = (int)Math.Round((double)(100 * correctwords) / wordswritten);

            return correctness;
        }



        private int CompareChiPassage(string wordsrec, string passagestr)
        {
            if (wordsrec == "")
            {
                return 0;
            }

            Debug.WriteLine(wordsrec);
            Debug.WriteLine(passagestr);
            int i = 0;
            int wordswritten = 0;
            int correctwords = 0;


            foreach (char c in wordsrec)
            {
                if (c.Equals(char.Parse("，")) || c.Equals(char.Parse("。")))
                {
                    i++;
                    continue;
                }
                else if(c.Equals(char.Parse(" ")))
                {
                    continue;
                }
                else if (char.Equals(c, passagestr[i]))
                {
                    _indexOfCorrectChar.Add(i);
                    correctwords++;
                    wordswritten++;
                }
                else
                {
                    wordswritten++;
                }

                i++;
            }

            if (wordswritten == 0)
            {
                return 0;
            }

            WCtooltip1 = correctwords.ToString();
            WCtooltip2 = wordswritten.ToString();

            int correctness = (int)Math.Round((double)(100 * correctwords) / wordswritten);

            return correctness;
        }

        #endregion

        #region Size

        //Accept a double list and return ahe coefficient of variation
        private double CalculateCoefficientOfVariation(List<double> DoubleList)
        {
            if (DoubleList.Count != 0)
            {
                var average = DoubleList.Average();
                var sd = CalculateSDFromAList(DoubleList);
                var cov = (sd / average) * 100;
                //Debug.WriteLine("cov" + cov.ToString());
                return (100 - cov);
            }
            else
            {
                return 0;
            }

        }

        //Accept a double list and return standard deviation
        private double CalculateSDFromAList(List<double> DoubleList)
        {
            if (DoubleList.Count != 0)
            {
                var average = DoubleList.Average();
                var differentsum = DoubleList.Sum(a => (a - average) * (a - average));
                var sd = Math.Sqrt(differentsum / DoubleList.Count());
                return sd;
            }
            else
            {
                return 0;
            }
        }
        

        private int CalculateEngSize(JsonObject jixx)
        {
            int sizeconsistencymark = 0;

            JsonValue jiixWords_ = null;

            //jiixWords_ set -> single jixxWord
            if (_jiix?.TryGetValue("chars", out jiixWords_) ?? false)
            {
                var jiixWords = (JsonArray)jiixWords_;

                // inside each word
                foreach (var jiixWord_ in jiixWords)
                {
                    var jiixWord = (JsonObject)jiixWord_;

                    char CharacterRecognized;

                    if (char.TryParse(jiixWord["label"], out CharacterRecognized) && IsPuncuation(CharacterRecognized))
                    {
                        continue;
                    }

                    var CharacterHeight = jiixWord["bounding-box"]["height"];

                    EngCategorize(CharacterRecognized, CharacterHeight);


                }
            }
            tallheightlist.ForEach(a => Debug.Write(a + " "));
            Debug.WriteLine("");
            shortheightlist.ForEach(a => Debug.Write(a + " "));
            Debug.WriteLine("");
            descentheightlist.ForEach(a => Debug.Write(a + " "));

            // set reference height for calculating tilt
            if(tallheightlist.Count > 0)
            {
                refheight = tallheightlist.Average() * refheightconst;
            }
            else if(descentheightlist.Count > 0)
            {
                refheight = descentheightlist.Average() * refheightconst;
            }

            SCtooltip1 = CalculateCoefficientOfVariation(tallheightlist).ToString();
            SCtooltip2 = CalculateCoefficientOfVariation(shortheightlist).ToString();
            SCtooltip3 = CalculateCoefficientOfVariation(descentheightlist).ToString();

            sizeconsistencymark = Convert.ToInt32((CalculateCoefficientOfVariation(tallheightlist) + CalculateCoefficientOfVariation(shortheightlist) + CalculateCoefficientOfVariation(descentheightlist)) / 3);



            return sizeconsistencymark;


        }

        private int CalculateChiSize(JsonObject jixx)
        {
            int sizeconsistencymark = 0;

            JsonValue jiixWords_ = null;

            //jiixWords_ set -> single jixxWord
            if (_jiix?.TryGetValue("chars", out jiixWords_) ?? false)
            {
                var jiixWords = (JsonArray)jiixWords_;

                // inside each word
                foreach (var jiixWord_ in jiixWords)
                {
                    var jiixWord = (JsonObject)jiixWord_;

                    char CharacterRecognized;

                    if (char.TryParse(jiixWord["label"], out CharacterRecognized) && IsPuncuation(CharacterRecognized))
                    {
                        continue;
                    }

                    chiheight.Add(jiixWord["bounding-box"]["height"]);
                    chiwidth.Add(jiixWord["bounding-box"]["width"]);


                }
            }
            chiheight.ForEach(a => Debug.Write(a + " "));
            Debug.WriteLine("");
            chiwidth.ForEach(a => Debug.Write(a + " "));
            Debug.WriteLine("");

            //set ref height
            if(chiheight.Count > 0)
            {
                refheight = chiheight.Average();
            }

            SCtooltip1 = CalculateCoefficientOfVariation(chiheight).ToString();
            SCtooltip2 = CalculateCoefficientOfVariation(chiwidth).ToString();
            sizeconsistencymark = Convert.ToInt32((CalculateCoefficientOfVariation(chiheight) + CalculateCoefficientOfVariation(chiwidth)) / 2);



            return sizeconsistencymark;


        }

        private void EngCategorize(char c, double height)
        {
            if (tallchar.Contains(c))
            {
                tallheightlist.Add(height);
            }
            else if (shortchar.Contains(c))
            {
                shortheightlist.Add(height);
            }
            else if (descentchar.Contains(c))
            {
                descentheightlist.Add(height);
            }
        }

        #endregion


        #region Baseline
        private int CalculateBaseline(JsonObject jixx)
        {
            // this controls the y intercept of the equation, it is set to 0.01 for y = 100 when x is 0
            double a = 0.01;
            // this controls the slop of the equation, the more is n, the steeper the downward slope of the equation.
            double n = 6;

            double baselinemark = 0;

            JsonValue jiixWords_ = null;


            //jiixWords_ set -> single jixxWord
            if (_jiix?.TryGetValue("chars", out jiixWords_) ?? false)
            {
                var jiixWords = (JsonArray)jiixWords_;

                dynamic o = new ExpandoObject();
                dynamic textlinelist = new List<ExpandoObject>();

                List<double> textlineY = new List<double>();
                List<double> textlineX = new List<double>();

                // inside each word
                foreach (var jiixWord_ in jiixWords)
                {
                    var jiixWord = (JsonObject)jiixWord_;

                    char CharacterRecognized;

                    if ((char.TryParse(jiixWord["label"], out CharacterRecognized) && IsPuncuation(CharacterRecognized) && !CharacterRecognized.Equals('\n')) || IsDescent(CharacterRecognized))
                    {
                        continue;
                    }

                    // is line break
                    if (CharacterRecognized.Equals('\n'))
                    {
                        o = new ExpandoObject();
                        // add current text line Y coordinates into a dynamic list
                        o.X = textlineX;
                        o.Y = textlineY;
                        textlinelist.Add(o);
                        // clear textline list
                        textlineY = new List<double>();
                        textlineX = new List<double>();
                    }
                    else
                    {
                        //add y coordinate to textline list
                        double height = jiixWord["bounding-box"]["height"];
                        double leftrightbox = jiixWord["bounding-box"]["y"];
                        var textboxminima = height + leftrightbox;
                        var textboxX = jiixWord["bounding-box"]["x"];
                        textlineY.Add(textboxminima);
                        textlineX.Add(textboxX);
                    }



                }

                // add last line to the textline list
                o = new ExpandoObject();
                o.X = textlineX;
                o.Y = textlineY;
                textlinelist.Add(o);

                
                double averageslope = GetAverageSlopFromLinearRegression(textlinelist);
                BLtooltip1 = averageslope.ToString();

                //whole equation 1/a(x+100a)^n
                var powered = Math.Pow((Math.Abs(averageslope) + 100 * a), n); //(x+100a)^n
                baselinemark = 1 / (a * powered);
                Debug.WriteLine(baselinemark);


            }
            return Convert.ToInt32(baselinemark);
        }


        private double GetAverageSlopFromLinearRegression(List<ExpandoObject> textlinelist)
        {
            int textlinecount = textlinelist.Count;
            if(textlinecount == 0)
            {
                return 0;
            }
            List<double> slopelist = new List<double>();
            foreach (dynamic o in textlinelist)
            {
                double[] X = o.X.ToArray();
                double[] Y = o.Y.ToArray();
                double[] iY = new double[Y.Length];
                for (int i = 0; i < Y.Length; i++)
                {
                    iY[i] = -Y[i];
                }
                if(X.Length != 0)
                {
                    OrdinaryLeastSquares ols = new OrdinaryLeastSquares();
                    SimpleLinearRegression regression = ols.Learn(X, Y);
                    slopelist.Add(Math.Abs(regression.Slope));
                }


                /*
                this.WpfPlot.Plot.AddScatter(X, iY);
                this.WpfPlot.Plot.YAxis.TickLabelNotation(invertSign: true);
                this.WpfPlot.Refresh();
                */

            }
            if(slopelist.Count > 0)
            {
                return slopelist.Average();
            }
            else
            {
                return 0;
            }
            
        }

        #endregion


        #region Graph

        private void Show_Graph(object sender, RoutedEventArgs e)
        {
            if (WpfPlot.Visibility == Visibility.Hidden)
            {
                WpfPlot.Visibility = Visibility.Visible;
            }
            else
            {
                WpfPlot.Visibility = Visibility.Hidden;
            }

        }

        private void signalplotgrah(double[] y)
        {
            this.WpfPlot.Plot.AddSignal(y,1);
            this.WpfPlot.Plot.YAxis.TickLabelNotation(invertSign: false);
            this.WpfPlot.Refresh();
        }

        private void plotgraph2(List<List<double>> _strokediffratelist)
        {
            List<double> _rate = _strokediffratelist[0];
            double[] xs = Enumerable.Range(0, _rate.Count).Select(i => (double)i).ToArray();
            double[] ys = _rate.ToArray();

            this.WpfPlot.Plot.AddScatter(xs, ys);
            this.WpfPlot.Refresh();
        }

        private void plotgraph(List<double> list)
        {
            double[] xs = Enumerable.Range(0, list.Count).Select(i => (double)i).ToArray();
            double[] ys = list.ToArray();

            this.WpfPlot.Plot.AddScatter(xs, ys);
            this.WpfPlot.Refresh();
        }


        private double[] generatexarray(double[] l ){
            
            var length = l.Length;
            double[] xarray = new double [length];
            for (int i = 0; i < length; i++) {
                xarray[i] = i;

            }
            return xarray;
        }

        private double[] generateyarray(List<Degree> dl, int mode) {
            var length = dl.Count;
            double[] xarray = new double[length];
            int i = 0;
            double t = 0;
            foreach(Degree degree in dl)
            {

                if(mode == 0)
                {
                    xarray[i] = degree._degree;
                }
                else
                {
                    xarray[i] = degree._dt;
                }

                i++;
                
            }
            return xarray;
        }

        #endregion

        private void GetDegreeList(List<List<TrajectoryPoint>> _strokelist)
        {

            float px = 0;
            float py = 0;
            double pt = 0;
            double accumulatetime = 0;

            foreach (List<TrajectoryPoint> points in _strokelist) {

                //Each Stroke
                bool firstpoint = true;



                _degreelist = new List<Degree>();


                foreach (TrajectoryPoint point in points) {

                    

                    if (firstpoint)
                    {
                        //save first point for px,py,pt
                        px = (float)point._x;
                        py = (float)point._y;
                        pt = point._t;
                        firstpoint = false;
                    }
                    else
                    {
                        

                        double degree = CalculateDirectionAngle((float)point._x - px, (float)point._y - py);
                        //discard abnomral degree recorded in the pad, usually integer degree
                        if (Math.Floor(degree) == degree)
                        {
                            continue;
                        }

                        accumulatetime = point._t;

                        double dt = point._t - pt;

                        //Debug.WriteLine("degree: " + degree + " dt : " + dt + " at " + accumulatetime) ;
                        
                        //discard abnormal time
                        if(dt == 0)
                        {
                            dt = 0.5;
                            accumulatetime += 0.5;
                        }

                        double distance = Math.Sqrt((Math.Pow(point._x - px, 2) + Math.Pow(point._y - py, 2)));
                        _degreelist.Add(new Degree(degree, dt, accumulatetime, distance));
                        //_degreelist2.Add(new Degree(degree, point._t.Millisecond));
                        
                        px = (float)point._x;
                        py = (float)point._y;
                        pt = point._t;
                    }
                }

                //add degree list of this stroke in the list
                _strokeDegreeList.Add(_degreelist);
            }

            //Calculate Change rate with the strokedegree list that newly created
            foreach(List<Degree> localdegreelist in _strokeDegreeList)
            {

                List<DegreeDiff> _degreedifflist = new List<DegreeDiff>();

                
                for (int i = 1; i < localdegreelist.Count; i++)
                {
                    var dx = Math.Abs(localdegreelist[i]._degree - localdegreelist[i - 1]._degree);
                    var l = localdegreelist[i]._l;
                    if (dx == 0)
                    {
                        _degreedifflist.Add(new DegreeDiff(0,0));
                        continue;
                    }

                    _degreedifflist.Add(new DegreeDiff(dx, l));
                }

                _strokeDegreeDiffList.Add(_degreedifflist);

            }

            //_rateofchangelist
        }

        #region Degree diff
        public class DegreeDiff
        {
            public double _degreediff { get; set; }
            public double _l { get; set; }

            public DegreeDiff(double degreediff, double _l)
            {
                this._degreediff = degreediff;
                this._l = _l;
            }
        }
        #endregion

        #region Degree Class
        public class Degree
        {
            //change rate
            public double _degree { get; set; }
            //time difference
            public double _dt { get; set; }

            //accumulated time
            public double _at { get; set; }

            //length
            public double _l { get; set; }

            public Degree(double degree, double dt, double at, double length)
            {
                _degree = degree;
                _dt = dt;
                _at = at;
                _l = length;

            }
        }
        #endregion

        #region JiixChar

        public class JiixChar
        {
            public string _label { get; set; }
            public List<List<Degree>> _strokes { get; set; }

            public JiixChar(string Label, List<List<Degree>> Strokes)
            {
                this._label = Label;
                this._strokes = Strokes;
            }
        }

        #endregion

        #region DirectionCalculation
        public double CalculateDirectionAngle(float dx, float dy)
        {
            return (Math.Atan2(dy, dx)) * 180 / Math.PI;
        }

        public double CalculateDirectionRadian(float dx, float dy)
        {
            return (Math.Atan2(dy, dx));
        }


        //because y axis is inverted in the pad, quadrant will be swapped
        public int CheckQuadrant(double radian)
        {
            if (radian > 0 && radian < Math.PI / 2)
            {
                return 4;
            }
            else if (radian > Math.PI / 2 && radian <= Math.PI)
            {
                return 3;
            }
            else if (radian < -Math.PI / 2 && radian >= -Math.PI)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }
        #endregion

        #region Smoothness

        //For each stroke, calculate abonormal changes
        //maybe don't use rate of change
        private int CalculateSmoothness()
        {
            var totalnumberofstrokes = _globalstrokelist.Count;

            var index = 0;

            double smoothness = 0;
            double sumtotaldata = _strokeDegreeDiffList.Select(x => x.Count).Sum();
            List<double> smothnesslist = new List<double>();
            List<double> locallengthlist = new List<double>();
            double countedlength = 0;
            double originalmark = 0;

            foreach (List<DegreeDiff> difflist in _strokeDegreeDiffList)
            {
                // stroke that more than window size
                int countedstroke = 0;
                // total datat = how many entry in the changerate list
                double totaldata = difflist.Select(x => x._l).Sum();
                double totaldatacount = difflist.Count();
                // abort stroke that lesser than window size - 1
                if (totaldatacount <= (_windowsize - 1))
                {
                    
                    continue;
                }

                countedstroke++;
                countedlength += totaldata;

                List<double> _abnormalchangeslist = CalculateAbonormalChangesMk2(difflist, _windowsize, _windowthreshold);

                double numberofabnormalchanges = _abnormalchangeslist.Sum();
                Debug.WriteLine("total: " + totaldata + " abnormal: " + numberofabnormalchanges);
                //var totallength = _strokeDegreeDiffList.Select(x => x.Select(j => j._l).Sum()).Sum();
                double ws = (double)_windowsize;
                double localabruptionmark = ((numberofabnormalchanges) / (totaldata));
                double smoothnessmark = 1 - localabruptionmark;
                smothnesslist.Add(smoothnessmark);
                locallengthlist.Add(totaldata);
            }
            // normalize the marks
            for(int i = 0 ; i < smothnesslist.Count; i++)
            {
                var proportion = locallengthlist[i] / countedlength;
                originalmark += smothnesslist[i] * proportion;
            }

            originalmark *= 100;
            Stooltip1 = originalmark.ToString();
            //normalize the mark
            if (originalmark < 70)
            {
                return 0;
            }
            if (originalmark < 90)
            {
                return Convert.ToInt32((originalmark - 70) / 20 *100);
            }
            else
            {
                return 100;
            }

        }

        //Scrolling window for looping all index
        private List<int> CalculateAbonormalChanges(List<double> data, int windowsize, double threshold)
        {
            List<int> changes = new List<int>();

            for (int i = windowsize; i < data.Count; i++)
            {
                var currentindex = i - windowsize;
                // getrange  -> subsection of the list
                List<double> window = data.GetRange(currentindex, windowsize);

                var sd = CalculateSDFromAList(window);

                

                double windowthres = window.Average() +  threshold * sd;


                Debug.WriteLine(" ");
                window.ForEach(p => Debug.Write(String.Format("{0:0.##}", p) + " "));
                Debug.Write(" Sd " + sd);
                Debug.Write(" thres " + windowthres);


                bool changeDetected = false;

                    for (int j = 0; j < window.Count; j++)
                    {
                        if (window[j] > windowthres)
                        {
                            changeDetected = true;
                            Debug.Write(" HIT ");


                            break;
                        }
                    }
                


                if (changeDetected)
                {
                    changes.Add(currentindex);
                }
            }

            return changes;
        }

        private List<double> CalculateAbonormalChangesMk2(List<DegreeDiff> data, int windowsize, double threshold)
        {
            List<double> changes = new List<double>();


            for (int i = windowsize; i < data.Count; i++)
            {
                var currentindex = i - windowsize;
                // getrange  -> subsection of the list
                List<DegreeDiff> range = data.GetRange(currentindex, windowsize);
                List<double> window = range.Select(x=>x._degreediff).ToList();
                var sd = CalculateSDFromAList(window);


                //Debug.WriteLine(" ");
                //window.ForEach(p => Debug.Write(String.Format("{0:0.##}", p) + " "));
                //Debug.Write(" Sd " + sd);
                //Debug.Write(" thres " + windowthres);


                bool changeDetected = false;
                if(sd >= 30 )
                {
                    continue;
                }

                if(sd >= _sdthreshold)
                {
                    changeDetected = true;
                }

                _sdlist.Add(sd);

                if (changeDetected)
                {
                    
                    changes.Add(data[currentindex]._l);
                }
            }

            return changes;
        }
        #endregion

        //loop through stroke degree list
        private int CalculateTilt()
        {
            
            foreach(List<Degree> stroke in _strokeDegreeList)
            {

                double tiltthreshold = 30;     

                int i = 0;

                List<Degree> currentdegree = new List<Degree>();

                Degree pd = null;

                foreach (Degree degree in stroke)
                {                                 
                    
                    if (currentdegree.Count == 0)
                    {
                        // put the first degree in the list if matches the condition , and start record the pd
                        if (Math.Abs(degree._degree) <= 110 && Math.Abs(degree._degree) >= 70)
                        {
                            currentdegree.Add(degree);
                            pd = degree;
                            continue;
                        }
                        continue;

                    }
                    else
                    {
                        //this difference of the current degree is lesesr than the threshold
                        if((Math.Abs(degree._degree) <= 110 && Math.Abs(degree._degree) >= 70) && IssameSign(degree, pd))
                        {
                            currentdegree.Add(degree);
                            pd = degree;
                        }
                        else
                        {
                            
                            // this degree is out of the threshold, discard all the recorded degree
                            currentdegree.Clear();
                            // give it another chance by peeking the next degree?
                            

                        }


                    }

                    //Debug.WriteLine("sum :" + currentdegree.Select(x => x._l).Sum() + " ref :" + refheight);
                    if(currentdegree.Select(x => x._l).Sum() >= refheight && refheight != 0 )
                    {
                        // if the accumulated height already hit the reference height, stop the loop and add the adverage degree.
                        _avgdegreelist.Add(currentdegree.Select(x => x._degree).Average());
                        //Debug.WriteLine("refHit :" + currentdegree.Select(x => x._degree).Average());
                        break;
                    }
                }


            }

            double tiltness = 0;

            //Caclulate the tiltness mark, loop through all avg degree to see how it tilt from 90 degree

            if(_avgdegreelist.Count != 0)
            {
                tiltness = (_avgdegreelist.Select(x => Math.Abs(Math.Abs(x) - 90 )).Average()) / 20;
                WTtooltip1 = _avgdegreelist.Select(x => Math.Abs(Math.Abs(x) - 90)).Average().ToString();
            }

            



            return Convert.ToInt32( (1 -tiltness) * 100);
        }

        private bool IssameSign(Degree d1, Degree d2)
        {
            return ((d1._degree < 0) == (d2._degree < 0));
        }

        private void GetStrokeListFromJiix(JsonObject jiix)
        {
            double baselinemark = 0;

            JsonValue jiixWords_ = null;


            //jiixWords_ set -> single jixxWord
            if (_jiix?.TryGetValue("chars", out jiixWords_) ?? false)
            {
                var jiixWords = (JsonArray)jiixWords_;

                dynamic o = new ExpandoObject();
                dynamic textlinelist = new List<ExpandoObject>();

                List<double> textlineY = new List<double>();
                List<double> textlineX = new List<double>();
                              
                // inside each word
                foreach (JsonObject jiixWord_ in jiixWords)
                {
                    //Avoid no key found
                    JsonValue jiixCharitems_ = null;

                    JiixChar jiixchar = null;

                    string CharacterRecognized = jiixWord_["label"];

                    if (jiixWord_.TryGetValue("items", out jiixCharitems_))
                    {

                        // local stroke list for storing strokes trajectory for each character
                        List<List<TrajectoryPoint>> _localstrokelist = new List<List<TrajectoryPoint>>();

                        JsonArray jiixCharitems = (JsonArray)jiixCharitems_;

                        //in each item (stroke)....
                        foreach (var jiixCharitem in jiixCharitems)
                        {
                            List<TrajectoryPoint> stroke = new List<TrajectoryPoint>();

                            // loop through all T,X,Y data for Trajectory point
                            for (int i = 0; i < jiixCharitem["T"].Count; i++)
                            {
                                double X = jiixCharitem["X"][i];
                                double Y = jiixCharitem["Y"][i];
                                double T = jiixCharitem["T"][i];
                                stroke.Add(new TrajectoryPoint(X, Y, T));
                            }

                            _globalstrokelist.Add(stroke);
                            _localstrokelist.Add(stroke);

                        }

                        float px = 0;
                        float py = 0;
                        double pt = 0;
                        double accumulatetime = 0;
                        List<List<Degree>> StrokeDegreeList = new List<List<Degree>>();

                        foreach (List<TrajectoryPoint> points in _localstrokelist)
                        {

                            //Each Stroke
                            bool firstpoint = true;

                            _degreelist = new List<Degree>();

                            foreach (TrajectoryPoint point in points)
                            {

                                if (firstpoint)
                                {
                                    //save first point for px,py,pt
                                    px = (float)point._x;
                                    py = (float)point._y;
                                    pt = point._t;
                                    firstpoint = false;
                                }
                                else
                                {

                                    double degree = CalculateDirectionAngle((float)point._x - px, (float)point._y - py);
                                    //discard abnomral degree recorded in the pad, usually integer degree
                                    if (Math.Floor(degree) == degree)
                                    {
                                        continue;
                                    }

                                    accumulatetime = point._t;

                                    double dt = point._t - pt;

                                    //discard abnormal time
                                    if (dt == 0)
                                    {
                                        dt = 0.5;
                                        accumulatetime += 0.5;
                                    }

                                    double distance = Math.Sqrt((Math.Pow(point._x - px, 2) + Math.Pow(point._y - py, 2)));
                                    _degreelist.Add(new Degree(degree, dt, accumulatetime, distance));
                                    //_degreelist2.Add(new Degree(degree, point._t.Millisecond));

                                    px = (float)point._x;
                                    py = (float)point._y;
                                    pt = point._t;
                                }
                            }

                            //add degree list of this stroke in the list
                            StrokeDegreeList.Add(_degreelist);
                        }

                        //Actually bad coding here... Duplicated operateion of getdegree function...
                        jiixchar = new JiixChar(CharacterRecognized, StrokeDegreeList);
                        _jiixlist.Add(jiixchar);
                    }
                    


                }



            }
        }

        #region ChineseStroke Calculation

        private void StrokeTesting()
        {
            Debug.WriteLine("Identified Stroke Sequence: ");
            foreach (List<Degree> stroke in _strokeDegreeList)
            {
                //strokedebug += IdentifyStroke(stroke);            
                Debug.Write(IdentifyStroke(stroke) + " ");
            }

            
        }
        // Chinese stroke 
        private int CalculateStrokeCorrectness()
        {

            double TotalStrokes = 0;
            double CorrectStrokes = 0;

            // get the strokelist for the correct character
            foreach(int i in _indexOfCorrectChar)
            {
                JiixChar jiixChar = _jiixlist[i];

                string character = jiixChar._label;

                numberofchar++;

                //Get the stroke sequence string from database

                string strokesequence = "";
                using (var db = new WritingsDbContext())
                {
                    strokesequence = db.StrokeDatas.Where(x => x.Character == character).Select(x => x.StrokeSequence).FirstOrDefault().ToString();
                    TotalStrokes += strokesequence.Length;
                }

                string identifiedstrokesequence = "";

                //Identify strokes from the raw degree data
                foreach (List<Degree> stroke in jiixChar._strokes)
                {
                    identifiedstrokesequence += IdentifyStroke(stroke);
                }
                

                if (identifiedstrokesequence.Length == strokesequence.Length)
                {
                    numbercorrectstrokechar++;
                }

                //check correct stroke by comparing the stroke sequence
                for(int j =0; j< identifiedstrokesequence.Length; j++)
                {
                    
                    if(j == strokesequence.Length)
                    {
                        //ensure array won't out of bound
                        break;
                    }
                    if (identifiedstrokesequence[j].Equals(strokesequence[j]))
                    {
                        CorrectStrokes++;
                    }
                }

                //save inccorrect strokes
                if(identifiedstrokesequence != strokesequence)
                {
                    _incorrectStrokeList.Add(new IncorrectStroke(character, strokesequence, identifiedstrokesequence));
                }

                Debug.WriteLine("Correct: " + CorrectStrokes + "Total : " + TotalStrokes);
            }

            if(TotalStrokes == 0)
            {
                return 0;
            }
            else
            {
                double strokecorrectness = CorrectStrokes / TotalStrokes;
                StCtooltip1 = CorrectStrokes.ToString();
                StCtooltip2 = TotalStrokes.ToString();
                return Convert.ToInt32(strokecorrectness * 100);
            }
            
        }

        private int CalculateStrokeCount()
        {
            if(numberofchar == 0)
            {
                return 0;
            }
            else
            {
                double strokecount = numbercorrectstrokechar / numberofchar;
                StCottooltip1 = numbercorrectstrokechar.ToString();
                StCottooltip2 = numberofchar.ToString();
                return Convert.ToInt32(strokecount * 100);
            }

        }

        private void SaveDataToDatabase(bool IsChinese)
        {
            if (IsChinese)
            {
                ChAssessment chAssessment = new ChAssessment();                
                if (string.IsNullOrEmpty(datapassed.studentname))
                {
                    chAssessment.StudentName = "Unspecified";
                }
                else
                {
                    chAssessment.StudentName = datapassed.studentname;
                }
                chAssessment.WPM = float.Parse(_WPM);
                chAssessment.StrokeOrder = float.Parse(_StrokeCorrectness);
                chAssessment.StrokeCount = float.Parse(_StrokeCount);
                chAssessment.CharCorrectness = float.Parse(_WordCorrectness);
                chAssessment.SizeConsistency = float.Parse(_SizeConsistency);
                chAssessment.Smoothness = float.Parse(_Smoothness);
                chAssessment.BaseLine = float.Parse(_BaseLine);
                chAssessment.TimeStamp = DateTime.Now;

                string jsonString = JsonConvert.SerializeObject(_incorrectStrokeList);

                chAssessment.IncorrectStroke = jsonString;

                //Add data into db
                using (var db = new WritingsDbContext())
                {

                    db.ChAssessments.Add(chAssessment);
                    db.SaveChanges();
                }
            }
            else
            {
                EngAssessment engAssessment = new EngAssessment();
                if (string.IsNullOrEmpty(datapassed.studentname))
                {
                    engAssessment.StudentName = "Unspecified";
                }
                else
                {
                    engAssessment.StudentName = datapassed.studentname;
                }
                engAssessment.WPM = float.Parse(_WPM);
                engAssessment.WordTilt = float.Parse(_WordTilt);
                engAssessment.CharCorrectness = float.Parse(_WordCorrectness);
                engAssessment.SizeConsistency = float.Parse(_SizeConsistency);
                engAssessment.Smoothness = float.Parse(_Smoothness);
                engAssessment.BaseLine = float.Parse(_BaseLine);
                engAssessment.TimeStamp = DateTime.Now;

                using (var db = new WritingsDbContext())
                {

                    db.EngAssessments.Add(engAssessment);
                    db.SaveChanges();
                }
            }
        }

        private string IdentifyStroke(List<Degree> stroke)
        {
            int totaltrajectroypoint = stroke.Count;
            double totallength = stroke.Select(x => x._l).Sum();
            double _L_RCLUE = 0;
            double _U_DClue = 0;
            double _TR_BLClue = 0;
            double _TL_DRClue = 0;

            
            double[][] aes = stroke.Skip(5).Select(x => new double[] {x._degree, x._l}).ToArray();
            if(aes.GetLength(0) <= 1)
            {
                return "6"; //wild card number for data that cannot be cluster, e.g: too less dataset
            }
            KMeans kmeans = new KMeans(k: 2);
            // Compute and retrieve the data centroids
            var clusters = kmeans.Learn(aes);

            // Use the centroids to parition all the data
            int[] labels = clusters.Decide(aes);

            double[][] centroids = clusters.Centroids;
            double errors = kmeans.Error;

            //Calculate centroid proportion, proportion large -> they are not hugely related -> might be some noises.
            double lengthcentroid1 = centroids[0][1]; // for column: 0 = degree, 1 = length
            double lengthcentroid2 = centroids[1][1];
            double centroidProportion = 0;
            if(lengthcentroid1 > lengthcentroid2)
            {
                centroidProportion = lengthcentroid1/ lengthcentroid2;
            }
            else
            {
                centroidProportion = lengthcentroid2 / lengthcentroid1;
            }

            double centroidDegreeDifference = Math.Abs(centroids[0][0] - centroids[1][0]);

            if(centroidDegreeDifference > 180)
            {
                centroidDegreeDifference = 360 - centroidDegreeDifference; 
            }

            //the degree difference is out of the threshold, and the length are not related -> it might be a 折
            if(centroidDegreeDifference >= centroidDegreeThreshold && centroidProportion <= centroidProportionConst)
            {
                return "5";
            }
            else
            {
                foreach (Degree strokePoint in stroke)
                {
                    if (L_RClue(strokePoint._degree))
                    {
                        _L_RCLUE += strokePoint._l;
                    }
                    else if (U_DClue(strokePoint._degree))
                    {
                        _U_DClue += strokePoint._l;
                    }
                    else if (TR_BLClue(strokePoint._degree))
                    {
                        _TR_BLClue += strokePoint._l;
                    }
                    else if (TL_DRClue(strokePoint._degree))
                    {
                        _TL_DRClue += strokePoint._l;
                    }
                }

                Debug.WriteLine(_TL_DRClue);
                Debug.WriteLine(totallength);

                if (_L_RCLUE / totallength >= 0.6)
                {
                    return "1";
                }
                else if (_U_DClue / totallength >= 0.6)
                {
                    return "2";
                }
                else if (_TR_BLClue / totallength >= 0.6)
                {
                    return "3";
                }
                else if (_TL_DRClue / totallength >= 0.6)
                {
                    return "4";
                }
                else
                {
                    return "0";
                }

            }



        }

        // "橫" 1
        private bool L_RClue(double Degree)
        {
            if(Math.Abs(Degree) <= 25) //0+-25
            {
                return true;
            }
            else if (Degree <= -25 && Degree >= -60)
            {
                //bottom left to top right 
                return true;
            }
            else
            {
                return false;
            }
        }
        
        //"棟" 2
        private bool U_DClue(double Degree)
        {
            if(Math.Abs(Degree-90) <= 20)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // "撇" 3
        private bool TR_BLClue(double Degree)
        {
            if(Math.Abs(Degree - 135) <= 30)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //"點" 4
        private bool TL_DRClue(double Degree)
        {
            Debug.WriteLine(Degree);
            if (Math.Abs(Degree - 45) <=20) // 45 +- 20
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public class IncorrectStroke
        {
            public string Label { get; set; }

            public string CorrectSequence { get; set; }

            public string WrittenSequence { get; set; }

            public IncorrectStroke(string label, string correctSequence, string writtenSequence)
            {
                Label = label;
                CorrectSequence = correctSequence;
                WrittenSequence = writtenSequence;
            }
        }

        #endregion


        /*
                    private string _WPM;
        private string _WordCorrectness;
        private string _SizeConsistency;
        private string _BaseLine;
        private string _Smoothness;
        private string _WordTilt;
        private string _StrokeCorrectness;
        private string _StrokeCount;
         */
        private double CalculateChiHandwritingAccuracy()
        {
            return ((double.Parse(_WordCorrectness) + double.Parse(_SizeConsistency)  + double.Parse(_BaseLine) + double.Parse(_Smoothness) + double.Parse(_StrokeCorrectness) + double.Parse(_StrokeCount)) / 6);
        }

        private double CalculateEngHandwritingAccuracy()
        {
            return ((double.Parse(_WordCorrectness) + double.Parse(_SizeConsistency)  + double.Parse(_BaseLine) + double.Parse(_Smoothness) + double.Parse(_WordTilt) ) / 5);
        }

        public static string CalculateOveralGrade(double AccuracyScore, double WPM , bool IsChinese)
        {
            //double AccuracyScore = Convert.ToDouble(this.AccuracyScore.Text);
            //double WPM = Convert.ToDouble(this.WPM.Text);
            double WPMScore = 0;
            if (IsChinese)
            {
                if(WPM > _ChiMarkFraction)
                {
                    WPMScore = 100;
                }
                else
                {
                    WPMScore = WPM / _ChiMarkFraction * 100;
                }           
            }
            else
            {
                if( WPM > _EngMarkFraction)
                {
                    WPMScore = 100;
                }
                else
                {
                    WPMScore = WPM / _EngMarkFraction * 100;
                }

            }

            double TotalScore = (AccuracyScore + WPMScore) /2;

            if(TotalScore <= 100 && TotalScore > 85)
            {
                return "A";
            }
            else if (TotalScore <= 85 && TotalScore > 60)
            {
                return "B";
            }
            else if(TotalScore <= 60 && TotalScore > 50)
            {
                return "C";
            }
            else if(TotalScore <= 50 && TotalScore > 40)
            {
                return "D";
            }
            else if(TotalScore <= 40)
            {
                return "F";
            }
            return "U";
        }


    }
}
