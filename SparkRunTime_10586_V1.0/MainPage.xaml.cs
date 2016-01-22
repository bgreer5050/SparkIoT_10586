using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        public static SystemState currentSystemState;
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
        }

        private void ClickMe_Click(object sender, RoutedEventArgs e)
        {
            this.HelloMessage.Text = "Hello, Windows IoT Core!";
        }
    }
}
