using System;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Ink;
using Wacom.Devices;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using MyScript.IInk;
using MyScript.IInk.UIReferenceImplementation;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Dynamic;
using WillDevicesSampleApp.Net;
using JosePCL.Serialization;
using System.IO.Packaging;
using System.Collections.Generic;
using System.Configuration;

namespace WillDevicesSampleApp
{
    public sealed partial class RealTimeInkPage : Page, INotifyPropertyChanged
    {
        private CancellationTokenSource m_cts = new CancellationTokenSource();

        //Stroke collection used for real-time rendering 
        private StrokeCollection _strokes = new StrokeCollection();
        private double m_scale = 1.0;
        private Size m_deviceSize;
        private bool m_addNewStrokeToModel = true;
        private static float maxP = 1.402218f;
        private static float pFactor = 1.0f / (maxP - 1.0f);
        private string poninthovered;
        private Editor editor => UcEditor.Editor;
        private bool firstpoint;
        private bool pointdown;
        private dynamic datapassed;
        private string currentlanguage;
        //Timer
        private DispatcherTimer timer;
        private TimeSpan dt = new TimeSpan(0, 2, 0);
        private bool timerstart = true;
        private bool enablemyscript = true;
        private bool firstpointwhenwriting = true;

        private List<double> degreelist = new List<double>();
        //A list of point for storing a stroke
        private List<TrajectoryPoint> Stroke;
        private List<List<TrajectoryPoint>> StrokeList = new List<List<TrajectoryPoint>>();

        private float px;
        private float py;

        private int debug;



        public event PropertyChangedEventHandler PropertyChanged;

        public StrokeCollection Strokes //Used as databinding into XAML
        {
            get
            {
                return _strokes;
            }
        }

        public RealTimeInkPage()
        {
            
            this.InitializeComponent();
            
            Loaded += RealTimeInkPage_Loaded;

        }
        public RealTimeInkPage(dynamic o)
        {

            this.InitializeComponent();
            
            Loaded += RealTimeInkPage_Loaded;
            

            this.datapassed = o;

            currentlanguage = datapassed.passagestring;

        }


        private void OnLoad(object sender, RoutedEventArgs e)
        {

            try
            {
                // code that accesses the file
                if (enablemyscript)
                {
                    initializemyscript();
                    LoadTextBoxPassage(currentlanguage, "");
                }
            }
            catch (FileNotFoundException ex)
            {
                // log the error or display an error message to the user
                MessageBox.Show("The file could not be found.");
            }

            

            
            

            initialtimer();
            

        }


        private void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            m_cts.Cancel();
        }

        private async void NavigationService_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                IRealTimeInkService service = AppObjects.Instance.Device.GetService(InkDeviceService.RealTimeInk) as IRealTimeInkService;

                if ((service != null) && service.IsStarted)
                {
                    await service.StopAsync(m_cts.Token);
                }

            }

        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        ~RealTimeInkPage()
        {
            System.Diagnostics.Debug.WriteLine("Finalizer: RealTimeInkPage");
        }

        private async void RealTimeInkPage_Loaded(object sender, RoutedEventArgs e)
        {
            IDigitalInkDevice device = AppObjects.Instance.Device;

            device.Disconnected += OnDeviceDisconnected;

            NavigationService.Navigating += NavigationService_Navigating;
            NavigationService.Navigated += NavigationService_Navigated;

            IRealTimeInkService service = device.GetService(InkDeviceService.RealTimeInk) as IRealTimeInkService;
            service.NewPage += OnNewPage; //Clear page on new page or layer
            service.NewLayer += OnNewPage;
            m_Canvas.DataContext = this;
            try
            {
                uint width = (uint)await device.GetPropertyAsync("Width", m_cts.Token);
                uint height = (uint)await device.GetPropertyAsync("Height", m_cts.Token);
                uint ptSize = (uint)await device.GetPropertyAsync("PointSize", m_cts.Token);


                m_deviceSize.Width = width;
                m_deviceSize.Height = height;

                SetCanvasScaling();

                service.StrokeStarted += Service_StrokeStarted;
                service.StrokeUpdated += Service_StrokeUpdated;
                service.StrokeEnded += Service_StrokeEnded;
                service.HoverPointReceived += Service_HoverPointReceived; ;

                if (!service.IsStarted)
                {
                    await service.StartAsync(true, m_cts.Token);
                }

            }
            catch (Exception)
            {
            }


        }

        private void NavigationService_LoadCompleted(object sender, NavigationEventArgs e)
        {
            dynamic o = new ExpandoObject();
            o = e.ExtraData;
        }

        private void Service_HoverPointReceived(object sender, HoverPointReceivedEventArgs e)
        {
            string hoverPointCoords = string.Empty;
            switch (e.Phase)
            {
                case Wacom.Ink.InputPhase.Begin:
                case Wacom.Ink.InputPhase.Move:
                    hoverPointCoords = string.Format("X:{0:0.0}, Y:{1:0.0}", e.X, e.Y);
                    break;
            }
            Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
                //Debug.WriteLine(hoverPointCoords);
            }));
        }

        #region stroke listener

        private void Service_StrokeEnded(object sender, StrokeEndedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
                var pathPart = e.PathPart;
                var data = pathPart.Data.GetEnumerator();


                //Data is stored XYW
                float x = -1;
                float y = -1;
                float w = -1;

                if (data.MoveNext())
                {
                    x = data.Current;
                }

                if (data.MoveNext())
                {
                    y = data.Current;
                }

                if (data.MoveNext())
                {
                    //Clamp to 0.0 -> 1.0
                    w = Math.Max(0.0f, Math.Min(1.0f, (data.Current - 1.0f) * pFactor));
                }

                var point = new System.Windows.Input.StylusPoint(x * m_scale, y * m_scale, w);
                
                Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                {
                    _strokes[_strokes.Count - 1].StylusPoints.Add(point);
                    NotifyPropertyChanged("Strokes");
                }));

                //Stroke point collection when stroke end
                /*
                Debug.WriteLine("StrokeEnd");
                Debug.WriteLine(_strokes[_strokes.Count-1].StylusPoints.Count);
                foreach(var ps in _strokes[_strokes.Count - 1].StylusPoints)
                {
                    Debug.WriteLine("{0},{1}",ps.X,ps.Y);
                }

                
                */

                if (pointdown)
                {
                    pointdown = false;
                    //Debug.WriteLine("PointUp");
                    //Debug.WriteLine("{0},{1}", x, y);
                    //Bind data with canvas
                    firstpointwhenwriting = true;
                    if (enablemyscript)
                    {
                        //show degree, quadrant (debug)
                        /*
                        Debug.WriteLine("{0} - {1}, {2} - {3} || degree = {4} || quadrant = {5} ", (float)point.X, px, (float)point.Y, py, UcEditor.SmartGuide.CalculateDirectionAngle((float)point.X - px, (float)point.Y - py), UcEditor.SmartGuide.CheckQuadrant(UcEditor.SmartGuide.CalculateDirectionRadian((float)point.X - px, (float)point.Y - py)));
                        Debug.WriteLine("{0},{1}", (float)point.X - px, (float)point.Y - py);
                        */
                        degreelist.Add(UcEditor.SmartGuide.CalculateDirectionAngle((float)point.X - px, (float)point.Y - py));

                        StrokeList.Add(Stroke);

                        editor.PointerUp((float)point.X, (float)point.Y, -1, 0, PointerType.PEN, -1);

                    }

                    //Raw point
                    //editor.PointerUp((float)x, (float)y, -1, 0, PointerType.PEN, -1);
                }

                m_addNewStrokeToModel = true;


            }));


        }

        private void Service_StrokeUpdated(object sender, StrokeUpdatedEventArgs e)
        {
            var pathPart = e.PathPart;
            var data = pathPart.Data.GetEnumerator();

            //Data is stored XYW
            float x = -1;
            float y = -1;
            float w = -1;

            if (data.MoveNext())
            {
                x = data.Current;
            }

            if (data.MoveNext())
            {
                y = data.Current;
            }

            if (data.MoveNext())
            {
                //Clamp to 0.0 -> 1.0
                w = Math.Max(0.0f, Math.Min(1.0f, (data.Current - 1.0f) * pFactor));
            }

            var point = new System.Windows.Input.StylusPoint(x * m_scale, y * m_scale, w);
            if (m_addNewStrokeToModel)
            {
                m_addNewStrokeToModel = false;
                var points = new System.Windows.Input.StylusPointCollection();
                points.Add(point);

                var stroke = new Stroke(points);
                stroke.DrawingAttributes = m_DrawingAttributes;

                Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                {
                    _strokes.Add(stroke);

                }));

                //retreive first point of the stroke as the pointdown 
                if (firstpoint)
                {


                    firstpoint = false;
                    pointdown = true;

                    //Debug.WriteLine("Pointer Down");
                    //Debug.WriteLine("{0},{1}",x,y);

                    //Bind data with canvas
                    if (enablemyscript)
                    {


                        editor.PointerDown(((float)point.X), (float)point.Y, -1, 0, PointerType.PEN, -1);
                    }

                    //save the first point as px, py for later use
                    px = (float)point.X;
                    py = (float)point.Y;

                    //Raw point
                    //editor.PointerDown(((float)x), (float)y, -1, 0, PointerType.PEN, -1);


                    //Start time for first point down ever made in the page.
                    if (timerstart)
                    {
                        timerstart = false;
                        timer.Start();
                    }
                }

            }
            else
            {
                Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                {
                    _strokes[_strokes.Count - 1].StylusPoints.Add(point);
                }));
                //Debug.WriteLine("Point Move");
                //Debug.WriteLine("{0},{1}", x, y);
                
                //Bind data with canvas
                if (enablemyscript)
                {
                    // the first point cannot be compared for calulating the direction
                    if (!firstpointwhenwriting)
                    {
                        //Debug
                        /*
                        Debug.WriteLine("{0} - {1}, {2} - {3} || degree = {4} || quadrant = {5} ", (float)point.X, px, (float)point.Y, py, UcEditor.SmartGuide.CalculateDirectionAngle((float)point.X - px, (float)point.Y - py), UcEditor.SmartGuide.CheckQuadrant(UcEditor.SmartGuide.CalculateDirectionRadian((float)point.X - px, (float)point.Y - py)));
                        Debug.WriteLine("{0},{1}", (float)point.X - px, (float)point.Y - py);
                        */
                        degreelist.Add(UcEditor.SmartGuide.CalculateDirectionAngle((float)point.X - px, (float)point.Y - py));

                        //set save current xy as previous xy
                        px = (float)point.X;
                        py = (float)point.Y;
                        
                    }
                    else
                    {
                        firstpointwhenwriting = false;
                    }


                    editor.PointerMove(((float)point.X), (float)point.Y, -1, 0, PointerType.PEN, -1);
                    

                }

                //Raw point
                //editor.PointerMove(((float)x), (float)y, -1, 0, PointerType.PEN, -1);

            }


        }

        private void Service_StrokeStarted(object sender, StrokeStartedEventArgs e)
        {
            m_addNewStrokeToModel = true;
            firstpoint = true;
        }

        #endregion

        private void OnNewPage(object sender, EventArgs e)
        {
            var ignore = Task.Run(() =>
            {
                _strokes.Clear();

            });
        }

        private void OnDeviceDisconnected(object sender, EventArgs e)
        {
            AppObjects.Instance.Device = null;

            MessageBox.Show($"The device {AppObjects.Instance.DeviceInfo.DeviceName} was disconnected.");

            NavigationService.Navigate(new ScanAndConnectPage());
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetCanvasScaling();
        }

        private void SetCanvasScaling()
        {
            IDigitalInkDevice device = AppObjects.Instance.Device;

            if (device != null)
            {
                double sx = m_Canvas.ActualWidth / m_deviceSize.Width;
                double sy = m_Canvas.ActualHeight / m_deviceSize.Height;
                m_scale = Math.Min(sx, sy);
            }
        }

        private void LoadTextBoxPassage(string language, string passage)
        {
            //Get Passage from xml
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string relativePath = System.IO.Path.Combine(basePath, "Passages");

            string passageaddress = relativePath +"\\" + language + ".xml";
            string xmlpath = "";
            if(language == "Chinese")
            {
                xmlpath= ConfigurationManager.AppSettings["Chinese"];
            }
            else
            {
                xmlpath = ConfigurationManager.AppSettings["English"];
            }           
            XDocument passagexml = XDocument.Load(xmlpath);
            string passagestr = passagexml.Root.Descendants().Where(x => x.Name == "Passage").First().Value.ToString();
            richtextbox.AppendText(passagestr);
            if(language == "Chinese")
            {
                richtextbox.FontSize = 36;
            }
            else
            {
                richtextbox.FontSize = 28;
            }

        }

        #region timer
        private void initialtimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            Timer.FontSize = 30;
            Timer.Text = dt.ToString(@"mm\:ss");
            
        }

        private void timer_Tick(Object s, EventArgs e)
        {
            
            

            dt = dt.Subtract(TimeSpan.FromSeconds(1));
            Timer.Text = dt.ToString(@"mm\:ss");
            if (dt == TimeSpan.Zero)
            {
                dynamic o = new ExpandoObject();
                //Do stuff for times up
                timer.Stop();
                
                // Times up event
                Debug.WriteLine("Times up");
                //Pack data into ExpandoObject to pass to next page
                o.RecognizedWords = UcEditor.SmartGuide.GetWordsRecognized();
                o.CurrentLanguage = currentlanguage;
                o.Jiix = UcEditor.SmartGuide.GetJiixExport();
                o.studentname = datapassed.studentname;

                if (editor.Part != null)
                {
                    var part = editor.Part;
                    var package = part?.Package;
                    editor.Part = null;
                    part?.Dispose();
                    package?.Dispose();
                    package = null;
                }

                NavigationService.Navigate(new ResultPage(o));
            } 
        }
        #endregion

        private void initializemyscript()
        {
            Engine engine = Engine.Create(MyScript.Certificate.MyCertificate.Bytes);

            var renderer = engine.CreateRenderer(200, 200, null);

            // Folders "conf" and "resources" are currently parts of the layout
            // (for each conf/res file of the project => properties => "Build Action = content")
            string[] confDirs = new string[1];

            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string relativePath = System.IO.Path.Combine(basePath, "conf");
            Debug.WriteLine(relativePath);
            
            confDirs[0] = relativePath;
            //default to get en_US.conf
            engine.Configuration.SetStringArray("configuration-manager.search-path", confDirs);

            //Content package save at : user\AppData\Local\MyScript\tmp
            var localFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var tempFolder = System.IO.Path.Combine(localFolder, "MyScript", "tmp");
            engine.Configuration.SetString("content-package.temp-folder", tempFolder);

            //Disable Gesture
            engine.Configuration.SetBoolean("gesture.enable", false);
            //Disable text guide
            engine.Configuration.SetBoolean("text.guides.enable", false);

            //ensure not export these to save resources
            engine.Configuration.SetBoolean("export.jiix.text.structure", false);
            engine.Configuration.SetBoolean("export.jiix.text.chars", false);

            if(currentlanguage == "Chinese")
            {
                engine.Configuration.SetString("lang", "zh_HK");
            }
            else
            {
                engine.Configuration.SetString("lang", "en_US");
            }

            // Initialize the editor with the engine
            UcEditor.Engine = engine;
            UcEditor.Initialize(this);

            UcEditor.SetInputTool(PointerTool.PEN);

            //ActivePen_Click(ActivePen, null);
            //62200 43200
            var fontMetricsProvider = new FontMetricsProvider(200, 200, 200);
            editor.SetFontMetricsProvider(fontMetricsProvider);
            editor.SetViewSize(62200, 43200);




            // Create a content package to provide the SDK with a place to work
            var package = engine.OpenPackage("text.iink", PackageOpenOption.CREATE);
            var part = package.CreatePart("Text");
            editor.Part = part;
        }

        //debug
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            dynamic o = new ExpandoObject();
            //Do stuff for times up
            timer.Stop();

            // Times up event
            Debug.WriteLine("Times up");
            //Pack data into ExpandoObject to pass to next page
            o.RecognizedWords = UcEditor.SmartGuide.GetWordsRecognized();
            o.CurrentLanguage = currentlanguage;
            o.Jiix = UcEditor.SmartGuide.GetJiixExport();

            o.studentname = datapassed.studentname;

            if (editor.Part != null)
            {
                var part = editor.Part;
                var package = part?.Package;
                editor.Part = null;
                part?.Dispose();
                package?.Dispose();
                package = null;
            }

            NavigationService.Navigate(new ResultPage(o));
        }


    }


    public class TrajectoryPoint
    {
        public double _x;
        public double _y;
        public double _t;

        public TrajectoryPoint(double x,double y, double t){

            _x = x;
            _y = y;
            _t = t;
        
        }
    }
}
