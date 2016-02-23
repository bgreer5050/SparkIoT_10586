﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SparkRunTime_10586_V1._0
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        /// <summary>
        /// Main Classes ***********************************************************************************
        /// </summary>
        public Network network;
        public Configuration configuration;
        public Controller controller;
        public VM.ViewModel viewModel;

        public SparkQueue sparkQueue;
        public PowerOuttageHandler powerHandler;
        //***************************************************************************************************


        /// <summary>
        /// Data Collection Variables **********************************************************************
        /// </summary>
        public static DateTime timeOfSystemStartup;
        public static DateTime timeOfLastSystemStateChange;
        public static DateTime timeOfLastHeartbeat;
        public static long numberOfHeartBeatsSinceLastStateChange;
        public static long totalNumberOfCycles;
        public static long totalRuntimeMilliseconds;
        public static  SystemState currentSystemState;
        //****************************************************************************************************


        /// <summary>
        /// INPUT AND OUTPUT PIN DECLARATIONS **********************************************
        /// </summary>
        private const int HEARTBEAT_PIN = 5;
        private GpioPin heartBeatPin;

        //***********************************************************************************



        /// <summary>
        /// Timers **********************************************************************
        /// </summary>
        private DispatcherTimer timer;
        private DispatcherTimer timerDateTime;
        private static System.Threading.Timer systemStateMonitor;
        //************************************************************************************

        /// <summary>
        /// UI VARIABLES **********************************************************************
        /// </summary>
        private string Test = "";
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush yellowBrush = new SolidColorBrush(Windows.UI.Colors.Yellow);
        private SolidColorBrush greenBrush = new SolidColorBrush(Windows.UI.Colors.Green);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);

        //**************************************************************************************

        /// <summary>
        /// SHARED VARIABLES FOR UI UPDATE **********************************************************************
        /// </summary>
        public CycleLights cycleLights;

        //**************************************************************************************

        /// <summary>
        /// Flag Variables *********************************************************************
        /// </summary>
        public static bool blnDateReceivedFromServer = false;

        //**************************************************************************************

        /// <summary>
        /// Testing Variables *******************************************************************
        /// </summary>
        int inputCounter = 0;
        int outPutCounter = 0;
        //***************************************************************************************

        // TODO How to use this ?
        System.Threading.SynchronizationContext _uiSyncContext;

        public MainPage()
        {
            this.InitializeComponent();

            string powerOuttagePost = PowerOuttageHandler.CheckLog().Result;
            SetUpMisc(powerOuttagePost);

            _uiSyncContext = System.Threading.SynchronizationContext.Current;

            DateTime dt = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
            txtblockTime.Text = dt.Hour.ToString() + ":" + dt.Minute.ToString(); // ToString("HH:mm:ss");

            timerDateTime = new DispatcherTimer();
            timerDateTime.Interval = TimeSpan.FromMilliseconds(300);
            //timerDateTime.Tick += TimerDateTime_Tick;
            timerDateTime.Tick += TimerDateTime_Tick1;
            timerDateTime.Start();

            setUpSystem();
            setUpBoardIO();


            controller = new Controller();
            configuration = new Configuration();
            powerHandler = new PowerOuttageHandler(configuration);
            network = new Network();

            Utilities.SparkEmail.Send(this.configuration.AssetNumber + " Starting ");


            viewModel = new VM.ViewModel(controller, configuration, cycleLights, network, sparkQueue, this);

            //sparkQueue = new SparkQueue();
            sparkQueue.DataReadyForPickUp += SparkQueue_DataReadyForPickUp;
        }

        private async void SetUpMisc(string strPowerOuttagePost)
        {
            sparkQueue = new SparkQueue();
            sparkQueue.itializeClass().Wait();
            if (strPowerOuttagePost.Length > 5)
            {
                sparkQueue.Enqueue(strPowerOuttagePost);
            }

        }
        private System.Threading.SemaphoreSlim _semaphore = new System.Threading.SemaphoreSlim(1);
        private async void SparkQueue_DataReadyForPickUp(object sender, EventArgs e)
        {
            _semaphore.Wait();
            bool postSucceed = await ProcessOutboundQueue();
            Debug.WriteLine("DATA READY FOR PICKUP NOT IMPLEMENTED");
            if (postSucceed)
            {
                await sparkQueue.Dequeue();
            }
            _semaphore.Release();
        }

        private async Task<bool> ProcessOutboundQueue()
        {
            string postData = "";
            bool blnPostSuccessful = false;
            // TODO Decide how many times I should try to post until we cancel ???
            StringBuilder sb = new StringBuilder();
            postData = sparkQueue.Peek();
            Debug.WriteLine("Post Data: " + postData);

            if (postData != null && postData.Length > 10)
            {
                HttpClient client = new HttpClient();
                //var body = String.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=notify.windows.com", MyCredentials, MyCredentials2);
                var body = String.Format(postData);
                StringContent theContent = new StringContent(body, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpResponseMessage aResponse;
                try
                {
                    aResponse = await client.PostAsync(new Uri(@"http://sparkhub.metal-matic.com/api/MachineRunState/Record"), theContent);
                }
                catch (HttpRequestException ex)
                {
                    // TODO Update View Model Errors

                    Debug.WriteLine(ex.Message);
                    viewModel.Errors.Add("L197" + ex.Message);
                    return false;
                }

                Debug.WriteLine(aResponse.StatusCode);
                if (aResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //   sparkQueue.Dequeue();
                    return true;
                }

                Debug.WriteLine("Post Status Code: " + aResponse.StatusCode.ToString());
            }
            else
            {
                blnPostSuccessful = false;
            }
            return blnPostSuccessful;

        }

        private async void btnTest_Click(object sender, RoutedEventArgs e)
        {
            int result = await AccessTheWebAsync();

        }

        async Task<int> AccessTheWebAsync()
        {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

            Task<string> getStringTask = client.GetStringAsync(@"http://yahoo.com");
            string urlContents = await getStringTask;

            return urlContents.Length;

        }

        private void btnSetMachineStatusRUN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSetMachineStatusDOWN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TimerDateTime_Tick1(object sender, object e)
        {
            //BELOW IS WHEN WE ARE IN BENCH TESTING MODE ONLY.  IT SIMULATES HEARTBEATS.
            //if (OutPutHeartBeatPinTesting.Read() == GpioPinValue.High)
            //{
            //    OutPutHeartBeatPinTesting.Write(GpioPinValue.Low);
            //}
            //else if(DateTime.Now.Second > 30)
            //{
            //    OutPutHeartBeatPinTesting.Write(GpioPinValue.High);
            //    outPutCounter += 1;
            //}

            txtblockTime.Text = "Time Of Day: " + DateTime.Now.TimeOfDay.ToString();

            Debug.WriteLine(inputCounter.ToString());
            Debug.WriteLine(outPutCounter.ToString());
        }
        private void stateMonitorCheck(object state)
        {
            //Debug.WriteLine("QUEUE SIZE:" + sparkQueue.Count.ToString());

            var currTime = DateTime.Now;
            Debug.WriteLine(DateTime.Now.ToString());

            //Debug.Print("Checking if Machine State = Running");
            //Debug.Print("System State: " + currentSystemState.ToString()); 

            if (currentSystemState == SystemState.RUNNING)
            {

                var timeSinceLastHeartbeat = getMillisecondsSinceLastHeartBeat(currTime);
                if ((configuration.CycleLengthMs * configuration.GracePeriodMultiple) < timeSinceLastHeartbeat)
                {
                   // Debug.WriteLine("Cycle Length:" + configuration.CycleLengthMs.ToString() + )
                    //Debug.Print("Change State to Down");
                    setSystemStateToDown(currTime, configuration, sparkQueue);
                }
            }
        }

        private void setUpSystem()
        {
            cycleLights = new CycleLights();
            //timeOfSystemStartup = DateTime.UtcNow;
            //txtblockTime.Text = timeOfSystemStartup.time ToShortDateString();
            txtSystemStartTime.Text = DateTime.Now.TimeOfDay.ToString();
            currentSystemState = SystemState.DOWN;

            systemStateMonitor = new System.Threading.Timer(stateMonitorCheck, configuration, 1000, 1000);
        }

        private void setUpBoardIO()
        {
            //The OutPutHeartBeatPin will be set to high.  When the isolation relay
            //on the machine closes our OutPutHearBeatPin will send 5v to the hearBeatPin


            var gpioController = GpioController.GetDefault();

            if (gpioController == null)
            {
                Utilities.SparkEmail.Send(configuration.AssetNumber + " NO GPIO CONTROLLER.");
                return;
            }


            heartBeatPin = gpioController.OpenPin(HEARTBEAT_PIN);

            // Check if input pull-up resistors are supported
            if (heartBeatPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                heartBeatPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            else
                heartBeatPin.SetDriveMode(GpioPinDriveMode.Input);

            // Set a debounce timeout to filter out switch bounce noise from a button press
            heartBeatPin.DebounceTimeout = TimeSpan.FromMilliseconds(12);
            heartBeatPin.ValueChanged += HeartBeatPin_ValueChanged;

        }

        private void HeartBeatPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            Test = "HEART BEAT RECEIVED";
            if (args.Edge == GpioPinEdge.FallingEdge)
            {
                inputCounter += 1;
                handleHeartBeat(DateTime.Now, controller, configuration, sparkQueue);
            }
            Debug.WriteLine("***********************************");
            Debug.WriteLine(args.Edge.ToString());
            Debug.WriteLine("***********************************");

        }

        private void TimerUpdateUI_Tick(object sender, object e)
        {
            UpdateUI();
        }


        private void UpdateUI()
        {
            viewModel.BindBusinessLayerToViewModel();

            if (viewModel.CycleLights.GreenOn) { greenLED.Fill = greenBrush; } else { greenLED.Fill = grayBrush; }

            if (viewModel.CycleLights.YellowON) { yellowLED.Fill = yellowBrush; } else { yellowLED.Fill = grayBrush; }

            if (viewModel.CycleLights.RedON) { redLED.Fill = redBrush; } else { redLED.Fill = grayBrush; }



            txtCycleCount.Text = viewModel.TotalNumberOfCycles.ToString();
            Debug.WriteLine("TOTAL CYCLE COUNT: " + MainPage.totalNumberOfCycles.ToString());
            Debug.WriteLine("TOTAL CYCLE COUNT: " + viewModel.TotalNumberOfCycles.ToString());
            //*****************************************************************************************
            //************************    Update Cycle Lights **************************

            if (currentSystemState == SystemState.RUNNING)
            {
                cycleLights.GreenOn = true;
                cycleLights.YellowON = false;
                cycleLights.RedON = false;

                TimeSpan ts = DateTime.Now - timeOfLastSystemStateChange;
                if (ts.Milliseconds > configuration.CycleLengthMs)
                {
                    cycleLights.YellowON = false;
                }
            }
            else
            {
                cycleLights.GreenOn = false;
                cycleLights.YellowON = false;
                cycleLights.RedON = true;
            }


            //*****************************************************************************************
            //************************    Get Object Errors To Place In View **************************
            listViewErrors.Items.Clear();

            //We create a seperate list to hold our errors.  If our collection is modified
            //while we are iterating, we will get an error.


            string[] strControllerErrors = viewModel.Controller.Errors.ToArray();
            string[] strConfigurationErrors = viewModel.Configuration.Errors.ToArray();
            string[] strNetworkErrors = viewModel.Network.Errors.ToArray();
            string[] strSparQueueErrors = viewModel.SparkQueue.Errors.ToArray();


            for (var _x = 0; _x < strControllerErrors.Length; _x++)
            {
                listViewErrors.Items.Add(strControllerErrors[_x]);
            }

            for (var _x = 0; _x < strConfigurationErrors.Length; _x++)
            {
                listViewErrors.Items.Add(strConfigurationErrors[_x]);
            }

            for (var _x = 0; _x < strNetworkErrors.Length; _x++)
            {
                listViewErrors.Items.Add(strNetworkErrors[_x]);
            }

            for (var _x = 0; _x < strSparQueueErrors.Length; _x++)
            {
                listViewErrors.Items.Add(strSparQueueErrors[_x]);
            }



            //*********************************************************************************************




            switch (viewModel.Cluster.LedInbound)
            {
                case TroubleshootingCluster.Color.Gray:
                    ledInbound.Fill = grayBrush;
                    break;

                case TroubleshootingCluster.Color.Red:
                    ledInbound.Fill = redBrush;
                    break;

                case TroubleshootingCluster.Color.Yellow:
                    ledInbound.Fill = yellowBrush;
                    break;

                case TroubleshootingCluster.Color.Green:
                    ledInbound.Fill = greenBrush;
                    break;

                default:
                    ledInbound.Fill = grayBrush;
                    break;
            }


            switch (viewModel.Cluster.LedOutbound)
            {
                case TroubleshootingCluster.Color.Gray:
                    ledOutbound.Fill = grayBrush;
                    break;

                case TroubleshootingCluster.Color.Red:
                    ledOutbound.Fill = redBrush;
                    break;

                case TroubleshootingCluster.Color.Yellow:
                    ledOutbound.Fill = yellowBrush;
                    break;

                default:
                    ledOutbound.Fill = grayBrush;
                    break;
            }


            switch (viewModel.Cluster.LedPosting)
            {
                case TroubleshootingCluster.Color.Gray:
                    ledPosting.Fill = grayBrush;
                    break;

                case TroubleshootingCluster.Color.Red:
                    ledPosting.Fill = redBrush;
                    break;

                case TroubleshootingCluster.Color.Yellow:
                    ledPosting.Fill = yellowBrush;
                    break;

                default:
                    ledPosting.Fill = grayBrush;
                    break;
            }

            switch (viewModel.Cluster.LedAux1)
            {
                case TroubleshootingCluster.Color.Gray:
                    ledAux1.Fill = grayBrush;
                    break;

                case TroubleshootingCluster.Color.Red:
                    ledAux1.Fill = redBrush;
                    break;

                case TroubleshootingCluster.Color.Yellow:
                    ledAux1.Fill = yellowBrush;
                    break;

                default:
                    ledAux1.Fill = grayBrush;
                    break;
            }

            switch (viewModel.Cluster.LedAux2)
            {
                case TroubleshootingCluster.Color.Gray:
                    ledAux2.Fill = grayBrush;
                    break;

                case TroubleshootingCluster.Color.Red:
                    ledAux2.Fill = redBrush;
                    break;

                case TroubleshootingCluster.Color.Yellow:
                    ledAux2.Fill = yellowBrush;
                    break;

                default:
                    ledAux2.Fill = grayBrush;
                    break;
            }


            switch (viewModel.Cluster.LedAux3)
            {
                case TroubleshootingCluster.Color.Gray:
                    ledAux3.Fill = grayBrush;
                    break;

                case TroubleshootingCluster.Color.Red:
                    ledAux3.Fill = redBrush;
                    break;

                case TroubleshootingCluster.Color.Yellow:
                    ledAux3.Fill = yellowBrush;
                    break;

                default:
                    ledAux3.Fill = grayBrush;
                    break;
            }

            switch (viewModel.Cluster.LedAux4)
            {
                case TroubleshootingCluster.Color.Gray:
                    ledAux4.Fill = grayBrush;
                    break;

                case TroubleshootingCluster.Color.Red:
                    ledAux4.Fill = redBrush;
                    break;

                case TroubleshootingCluster.Color.Yellow:
                    ledAux4.Fill = yellowBrush;
                    break;

                default:
                    ledAux4.Fill = grayBrush;
                    break;
            }






            //listViewErrors.Background = redBrush;
            //listViewErrors.Foreground = greenBrush;
        }
        private static long getMillisecondsSinceLastStateChange(DateTime time)
        {
            TimeSpan ts = time - timeOfLastSystemStateChange;
            return ts.Ticks / TimeSpan.TicksPerMillisecond;
        }
        private static long getMillisecondsSinceLastHeartBeat(DateTime time)
        {
            TimeSpan ts = time - timeOfLastHeartbeat;
            return ts.Ticks / TimeSpan.TicksPerMillisecond;
        }



        private static void handleHeartBeat(DateTime time, Controller controller, Configuration config, SparkQueue sparkQueue)
        {

            TimeSpan ts = time - timeOfLastHeartbeat;
            if (ts.TotalMilliseconds > 7000)
            {
                Debug.WriteLine("HANDLING HEART BEAT !");
                Debug.WriteLine("HANDLING HEART BEAT !");
                Debug.WriteLine("HANDLING HEART BEAT !");


                totalRuntimeMilliseconds += getMillisecondsSinceLastHeartBeat(time);
                totalNumberOfCycles++;
                numberOfHeartBeatsSinceLastStateChange++;
                timeOfLastHeartbeat = time;



                double totalMillisecondsSinceLastCycle = ts.Ticks / 10000.0;

                if (currentSystemState == SystemState.DOWN &&
                numberOfHeartBeatsSinceLastStateChange >= config.HeartbeatsRequiredToChangeState &&
                totalMillisecondsSinceLastCycle < (config.CycleLengthMs * 2.0))
                {
                    setSystemSateToRun(time, config, sparkQueue);
                }
            }
            else
            {
                Debug.WriteLine("NOT HANDLING HEART BEAT!!!!!!!!!!!!!!!!!!!!!!");
            }

        }

        private static void setSystemSateToRun(DateTime time, Configuration config, SparkQueue sparkQueue)
        {
            // TODO Conside doing something with an output here

            Debug.WriteLine("RUN RUN RUN RUN");

            currentSystemState = SystemState.RUNNING;
            timeOfLastSystemStateChange = time;
            numberOfHeartBeatsSinceLastStateChange = 0;
            MachineEvent evt = new MachineEvent { AssetID = config.AssetNumber, state = "RUNNING", ticks = DateTime.Now.Ticks.ToString() };
            sparkQueue.Enqueue(@"assetID=" + config.AssetNumber + "&state=" + evt.state + "&ticks=" + evt.ticks.ToString());

            //+ "&blnNetworkUp=" + network.NetworkUp.ToString() + "&blntimeupdated=" + Program.time.TimeUpdated.ToString());
        }




        private static void setSystemStateToDown(DateTime time, Configuration config, SparkQueue sparkQueue)
        {

            Debug.WriteLine("DOWN DOWN DOWN DOWN");

            // TODO Conside doing something with an output here

            currentSystemState = SystemState.DOWN;
            timeOfLastSystemStateChange = time;
            numberOfHeartBeatsSinceLastStateChange = 0;

            MachineEvent evt = new MachineEvent { AssetID = config.AssetNumber, state = "DOWN", ticks = DateTime.Now.Ticks.ToString() };
            //sparkQueue.Enqueue(@"assetID=" + config.AssetNumber + "&state=" + evt.state + "&ticks=" + evt.ticks.ToString() + "&blnNetworkUp=" + network.NetworkUp.ToString() + "&blntimeupdated=" + Program.time.TimeUpdated.ToString());

            sparkQueue.Enqueue(@"assetID=" + config.AssetNumber + "&state=" + evt.state + "&ticks=" + evt.ticks.ToString());

            //Thread threadRecordStateInMachineStateLog = new Thread(new ThreadStart(UpdateMachineStateOnSDCard));
            //threadRecordStateInMachineStateLog.Start();
        }


        public enum SystemState
        {
            DOWN = 0,
            RUNNING
        };

        public struct MachineEvent
        {
            public string AssetID { get; set; }
            public string state { get; set; }
            public string ticks { get; set; }
        }
        //private void ClickMe_Click(object sender, RoutedEventArgs e)
        //{
        //    this.HelloMessage.Text = "Hello, Windows IoT Core!";
        //}
    }
}
