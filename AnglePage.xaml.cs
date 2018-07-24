using LightBuzz.Vitruvius;
using LightBuzz.Vituvius.Exergames.Kimos.WPF.Domain;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace LightBuzz.Vituvius.Exergames.Kimos.WPF
{
    /// <summary>
    /// Interaction logic for AnglePage.xaml
    /// </summary>
    public partial class AnglePage : Page
    {
        /// <summary>
        /// Folder for storage local disk
        /// </summary>
        string FOLDER_PATH = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExerGames");

        /// <summary>
        /// Folder path dynamic created based on Game and Player
        /// </summary>
        string _Data_PATH;

        /// <summary>
        /// Informations about player recorded
        /// </summary>
        Player _actualPlayer;
        List<List<Vector3>> _actual;

        /// <summary>
        /// Template for csv file recorded
        /// </summary>
        List<String> headerCsv;
        //VitruviusRecorder _recorder = new VitruviusRecorder();
        Dictionary<int, Entidade> _interestPoints;
        private bool _record = false;
        Vector3 _origin = Vector3.Zero;
        
        
        
        //Charts
        Charts _chartPage;
        SeriesCollection seriesChart1;
        LineSeries chart1LinearSeries;
        ChartValues<Double> lineValuesChart1;
        private ChartValues<double> colummValuesChart1;


        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        PlayersController _playersController;

        Boolean calibrate = false;
        
        List<List<Vector3>> _posicoes;


        public AnglePage()
        {
            //this.initiateCharts();
            InitializeComponent();
            lblInfo.Text = "Pronto!";
            _interestPoints = initializeInterestPoints();
            this.InitializeColumnsCsv();
            _sensor = KinectSensor.GetDefault();
            _posicoes = new List<List<Vector3>>();

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
                

                _playersController = new PlayersController();
                _playersController.BodyEntered += UserReporter_BodyEntered;
                _playersController.BodyLeft += UserReporter_BodyLeft;
                _playersController.Start();
            }
        }

        private Dictionary<int, Entidade> initializeInterestPoints()
        {
            Dictionary<int, Entidade> angulos = new Dictionary<int, Entidade>();
            angulos.Add((int)Angulos.CotoveloDireito, new Entidade(JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight));
            angulos.Add((int)Angulos.OmbroDireito, new Entidade(JointType.ElbowRight, JointType.ShoulderRight, JointType.SpineShoulder));
            angulos.Add((int)Angulos.PunhoDireito, new Entidade(JointType.ElbowRight, JointType.WristRight, JointType.HandRight));

            angulos.Add((int)Angulos.CotoveloEsquerdo, new Entidade(JointType.WristLeft, JointType.ElbowLeft, JointType.ShoulderLeft));
            angulos.Add((int)Angulos.OmbroEsquerdo, new Entidade(JointType.SpineShoulder, JointType.ShoulderLeft, JointType.ElbowLeft));
            angulos.Add((int)Angulos.PunhoEsquerdo, new Entidade(JointType.HandLeft, JointType.WristLeft, JointType.ElbowLeft));
            
            return angulos;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_playersController != null)
            {
                _playersController.Stop();
            }

            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void Expander_On(object sender, RoutedEventArgs e)
        {
            if (expColumn.IsExpanded == true)
            {
                expColumn.IsExpanded = false;
                expColumn.Visibility = System.Windows.Visibility.Hidden;
            }
            else {
                expColumn.IsExpanded = true;
                expColumn.Visibility = System.Windows.Visibility.Visible;
            }
            
        }

        //Update page
        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();
            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();
           
            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                
                if (frame != null)
                {
                    double a = 1.0 / frame.ColorCameraSettings.FrameInterval.TotalSeconds;
                    int b = (int)a;
                    textFps.Text = b.ToString();
                    
                    if (viewer.Visualization == Visualization.Color)
                    {
                        viewer.Image = frame.ToBitmap();
                        
                    }
                }
            }

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    var bodies = frame.Bodies();

                    _playersController.Update(bodies);

                    Body body = bodies.Closest();


                    //Verifica comando de inicio
                    if (body != null)
                    {
                        viewer.DrawBody(body);
                        // Update angles of recorded body
                        this.UpdateAngles(body);

                        if (this._record) {
                            dataRecord(body);
                        }
                        
                        if (body.IsTracked) {
                            if (this._record && !this.calibrate) {
                                this.calibrate = true;
                                var position = body.Joints[JointType.SpineBase].Position;
                                _origin = position.ToVector3();
                                Console.WriteLine("CALIBRADO NA ORIGEM: ");
                                Console.WriteLine(_origin);
                                Console.WriteLine("");
                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update body angles of interest point
        /// </summary>
        /// <param name="body"></param>
        private void UpdateAngles(Body body)
        {
            angleD1.Update(body.Joints[_interestPoints[(int)Angulos.CotoveloDireito].start],
                body.Joints[_interestPoints[(int)Angulos.CotoveloDireito].pontoInteresse], body.Joints[_interestPoints[(int)Angulos.CotoveloDireito].end], 50);
            angleD2.Update(body.Joints[_interestPoints[(int)Angulos.OmbroDireito].start],
                body.Joints[_interestPoints[(int)Angulos.OmbroDireito].pontoInteresse], body.Joints[_interestPoints[(int)Angulos.OmbroDireito].end], 50);
            angleD3.Update(body.Joints[_interestPoints[(int)Angulos.PunhoDireito].start],
                body.Joints[_interestPoints[(int)Angulos.PunhoDireito].pontoInteresse], body.Joints[_interestPoints[(int)Angulos.PunhoDireito].end], 50);

            angleE1.Update(body.Joints[_interestPoints[(int)Angulos.CotoveloEsquerdo].start],
                body.Joints[_interestPoints[(int)Angulos.CotoveloEsquerdo].pontoInteresse], body.Joints[_interestPoints[(int)Angulos.CotoveloEsquerdo].end], 50);
            angleE2.Update(body.Joints[_interestPoints[(int)Angulos.OmbroEsquerdo].start],
                body.Joints[_interestPoints[(int)Angulos.OmbroEsquerdo].pontoInteresse], body.Joints[_interestPoints[(int)Angulos.OmbroEsquerdo].end], 50);
            angleE3.Update(body.Joints[_interestPoints[(int)Angulos.PunhoEsquerdo].start],
                body.Joints[_interestPoints[(int)Angulos.PunhoEsquerdo].pontoInteresse], body.Joints[_interestPoints[(int)Angulos.PunhoEsquerdo].end], 50);

            txtAngle1.Text = ((int)angleD1.Angle).ToString();
            txtAngle2.Text = ((int)angleD2.Angle).ToString();
            txtAngle3.Text = ((int)angleD3.Angle).ToString();
            txtAngle4.Text = ((int)angleE1.Angle).ToString();
            txtAngle5.Text = ((int)angleE2.Angle).ToString();
            txtAngle6.Text = ((int)angleE3.Angle).ToString();
        }

        private void UpdateInforTextViewer(string text) {
            lblInfo.Text = text;
        }

        private void RecordClick(object sender, RoutedEventArgs e)
        {

            
            if (this._record)
            {
                this.UpdateInforTextViewer("Fim");
                this._origin = Vector3.Zero;
                this.calibrate = false;
                this.RecordData();
                _actual.Clear();

            }
            else
            {
                if (this.SaveInformationFile())
                {
                    _Data_PATH = FOLDER_PATH + "\\" + _actualPlayer._game + "\\" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + _actualPlayer._name + "\\" ;
                    lblInfo.Text = "Gravando";
                    Console.WriteLine("Gravando...");
                    Console.WriteLine("NOME: " + _actualPlayer._name);
                    Console.WriteLine("MAO DOMINANTE: "+ _actualPlayer._mao.ToString());
                    Console.WriteLine("ALTURA: " + _actualPlayer.altura);
                    Console.WriteLine("JOGO: " + _actualPlayer._game);
                    Console.WriteLine("");

                    _actual = new List<List<Vector3>>();

                    

                }
                else
                {
                    lblInfo.Text = "Infos?";
                    Console.WriteLine("Informações vazias");
                    this._record = !this._record;
                }

            }
            this._record = !this._record;

        }


        private void InitializeColumnsCsv()
        {
            headerCsv = new List<String>
            {

                //Membros Superiores
                "HandLeft.X",
                "HandLeft.Y",
                "HandLeft.Z",
                "WristLeft.X",
                "WristLeft.Y",
                "WristLeft.Z",
                "ElbowLeft.X",
                "ElbowLeft.Y",
                "ElbowLeft.Z",
                "ShoulderLeft.X",
                "ShoulderLeft.Y",
                "ShoulderLeft.Z",
                "SpineShoulder.X",
                "SpineShoulder.Y",
                "SpineShoulder.Z",

                "HandRight.X",
                "HandRight.Y",
                "HandRight.Z",
                "WristRight.X",
                "WristRight.Y",
                "WristRight.Z",
                "ElbowRight.X",
                "ElbowRight.Y",
                "ElbowRight.Z",
                "ShoulderRight.X",
                "ShoulderRight.Y",
                "ShoulderRight.Z",

                //Tronco
                "SpineMid.X",
                "SpineMid.Y",
                "SpineMid.Z",
                "SpineBase.X",
                "SpineBase.Y",
                "SpineBase.Z",

                // Membros Inferiores
                "HipLeft.X",
                "HipLeft.Y",
                "HipLeft.Z",
                "KneeLeft.X",
                "KneeLeft.Y",
                "KneeLeft.Z",
                "AnkleLeft.X",
                "AnkleLeft.Y",
                "AnkleLeft.Z",

                "HipRight.X",
                "HipRight.Y",
                "HipRight.Z",
                "KneeRight.X",
                "KneeRight.Y",
                "KneeRight.Z",
                "AnkleRight.X",
                "AnkleRight.Y",
                "AnkleRight.Z",

                "CotoveloDir",
                "OmbroDir",
                "PunhoDir",

                "CotoveloEsq",
                "OmbroEsq",
                "PunhoEsq"
            };

        }


        private void dataRecord(Body body)
        {
            var actual = new List<Vector3>();

            // Membros superiores
            actual.Add(body.Joints[JointType.HandLeft].Position.ToVector3());
            actual.Add(body.Joints[JointType.WristLeft].Position.ToVector3());
            actual.Add(body.Joints[JointType.ElbowLeft].Position.ToVector3());
            actual.Add(body.Joints[JointType.ShoulderLeft].Position.ToVector3());
            actual.Add(body.Joints[JointType.SpineShoulder].Position.ToVector3());

            actual.Add(body.Joints[JointType.HandRight].Position.ToVector3());
            actual.Add(body.Joints[JointType.WristRight].Position.ToVector3());
            actual.Add(body.Joints[JointType.ElbowRight].Position.ToVector3());
            actual.Add(body.Joints[JointType.ShoulderRight].Position.ToVector3());

            //Tronco

            actual.Add(body.Joints[JointType.SpineMid].Position.ToVector3());
            actual.Add(body.Joints[JointType.SpineBase].Position.ToVector3());


            //Membros Inferiores
            actual.Add(body.Joints[JointType.HipLeft].Position.ToVector3());
            actual.Add(body.Joints[JointType.KneeLeft].Position.ToVector3());
            actual.Add(body.Joints[JointType.AnkleLeft].Position.ToVector3());

            actual.Add(body.Joints[JointType.HipRight].Position.ToVector3());
            actual.Add(body.Joints[JointType.KneeRight].Position.ToVector3());
            actual.Add(body.Joints[JointType.AnkleRight].Position.ToVector3());

            Vector3 angleD;
            angleD.X = angleD1.Angle;
            angleD.Y = angleD2.Angle;
            angleD.Z = angleD3.Angle;

            Vector3 angleE;
            angleE.X = angleE1.Angle;
            angleE.Y = angleE2.Angle;
            angleE.Z = angleE3.Angle;

            actual.Add(angleD);
            actual.Add(angleE);

            /*
            if (count == 0)
            {
                count = Tempo;
                this.ChartUpdate(body.Joints[JointType.SpineShoulder].Position.Z, body.Joints[JointType.SpineShoulder].Position.Y);
            }
            count--;

            */


            _actual.Add(actual);
            
        }

        // Encontra referencia do corpo
        void UserReporter_BodyEntered(object sender, PlayersControllerEventArgs e)
        {
        }


        //Perde referencia do corpo
        void UserReporter_BodyLeft(object sender, PlayersControllerEventArgs e)
        {
            
            viewer.Clear();
            angleD1.Clear();
            angleD2.Clear();
            angleD3.Clear();

            angleE1.Clear();
            angleE2.Clear();
            angleE3.Clear();

            txtAngle1.Text = "-";
            txtAngle2.Text = "-";
            txtAngle3.Text = "-";
            txtAngle4.Text = "-";
            txtAngle5.Text = "-";
            txtAngle6.Text = "-";
        }


        //Corrigir depois

        private void initiateCharts()
        {
            _chartPage = new Charts();
            var host = new Window();
            host.Content = _chartPage;
            host.Show();

            seriesChart1 = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<double> { 0 }
                },
               
            };

            lineValuesChart1 = new ChartValues<double> { 0 };
            colummValuesChart1 = new ChartValues<double> { 0 };
        }

        private void ChartUpdate(double d1, double d2)
        {
            if (lineValuesChart1.Count == 30) {
                seriesChart1[0].Values.Remove(0);
            }

            //colummValuesChart1.Add(x);
            seriesChart1[0].Values.Add(d1);
            

            _chartPage.AtualizaGrafico(seriesChart1);
        }

        private bool SaveInformationFile()
        {
            if (txtName.Text != "...")
            {
                _actualPlayer = new Player();
                _actualPlayer._name = txtName.Text;
                _actualPlayer._game = txtGame.Text;
                if (checkMan.IsChecked == true)
                {
                    _actualPlayer._sexo = Sexo.Masculino;
                }
                else
                {
                    _actualPlayer._sexo = Sexo.Feminino;
                }

                if (checkLeft.IsChecked == true)
                {
                    _actualPlayer._mao = MaoDominante.Esquerda;
                }
                else
                {
                    _actualPlayer._mao = MaoDominante.Direita;
                }
                _actualPlayer.altura = Convert.ToInt32(txtAltura.Text);
                return true;
            }
            return false;
        }

        // Gravacao dos dados no arquivo csv
        private void RecordData() {
            //before your loop
            lblInfo.Text = "Salvando...";
            Console.WriteLine("Registrando Dados...");
            Console.WriteLine("Local dos Arquivos: " + _Data_PATH);


            string csvLine;
            var csv = new StringBuilder();
            var info = new StringBuilder();

            foreach (var parameter in headerCsv)
            {
                Console.Write(parameter + ",");
            }

            csv.AppendLine(string.Join(",",headerCsv.ToArray()));
            foreach (var frame in _actual)
            {
                csvLine = string.Join(",", frame.ToArray())+",";
                csv.AppendLine(csvLine);
                Console.WriteLine(csvLine);
            }

            // Save file
            if (!Directory.Exists(_Data_PATH)) {
                Directory.CreateDirectory(_Data_PATH);
            }

            File.WriteAllText(_Data_PATH+"data.csv", csv.ToString());

            //Save Info File
            
            info.AppendLine("NOME: " + _actualPlayer._name);
            info.AppendLine("MAO DOMINANTE: " + _actualPlayer._mao.ToString());
            info.AppendLine("SEXO: " + _actualPlayer._sexo.ToString());
            info.AppendLine("ALTURA: " + _actualPlayer.altura);
            info.AppendLine("JOGO: " + _actualPlayer._game);

            File.WriteAllText(_Data_PATH + "info.txt", info.ToString());

            lblInfo.Text = "Pronto!";

            
        }
        

    }
}
