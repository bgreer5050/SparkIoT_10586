﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkRunTime_10586_V1._0.VM
{
    public class ViewModel
    {
        /// <summary>
        /// Create all of the objects and values we will need for our View on ctor
        /// </summary>
        public ViewModel(Controller controller, Configuration configuration, CycleLights cycleLights, Network network, SparkQueue sparkQueue, MainPage startPage)
        {
            this.CycleLights = cycleLights;
            this.TotalNumberOfCycles = 0;
            this.TicksSinceLastCycle = 0;
            this.NumberOfHeartBeatsSinceLastStateChange = 0;
            this.TotalRunTimeMilliSeconds = 0;
            List<string> errorsList = new List<string>();
            this.Errors = errorsList;
            this.Cluster = new TroubleshootingCluster();
            this.Controller = controller;
            this.Configuration = configuration;
            this.Network = network;
            this.SparkQueue = sparkQueue;
            this.StartPage = startPage;
        }

        public CycleLights CycleLights { get; set; }
        public TroubleshootingCluster Cluster { get; set; }
        public long TotalNumberOfCycles { get; set; }
        public long TicksSinceLastCycle { get; set; }
        public long NumberOfHeartBeatsSinceLastStateChange { get; set; }
        public List<string> Errors { get; set; }
        public long TotalRunTimeMilliSeconds { get; set; }

        public DateTime SystemTime
        {
            get { return DateTime.Now.ToLocalTime(); }
            set { }
        }
        public DateTime SystemStartupTime { get; set; }
        public DateTime TimeOfLastHeartBeat { get; set; }
        public DateTime TimeOfLastSystemStateChange { get; set; }
        public double TotalMilliSecondsSinceLastCycle
        {
            get
            {
                TimeSpan ts = DateTime.Now.Date - this.TimeOfLastHeartBeat;
                return ts.Ticks / 1000.0;
            }
            set
            {
                TotalMilliSecondsSinceLastCycle = value;
            }
        }

        public double SecondsSinceLastCycle
        {
            get
            {
                return this.TotalMilliSecondsSinceLastCycle / 1000.0;
            }
            set
            {

            }

        }
        public int MyProperty { get; set; }
        public SparkRunTime_10586_V1._0.MainPage.SystemState CurrentSystemState { get; private set; }
        public Controller Controller { get; private set; }
        public Configuration Configuration { get; private set; }
        public Network Network { get; private set; }
        public SparkQueue SparkQueue { get; private set; }
        public MainPage StartPage { get; private set; }

        public void BindBusinessLayerToViewModel()
        {
            this.SystemStartupTime = MainPage.timeOfSystemStartup;
            this.SystemTime = DateTime.Now;
            this.TimeOfLastSystemStateChange = MainPage.timeOfLastSystemStateChange;
            this.TimeOfLastHeartBeat = MainPage.timeOfLastHeartbeat;
            this.NumberOfHeartBeatsSinceLastStateChange = MainPage.numberOfHeartBeatsSinceLastStateChange;
            this.TotalNumberOfCycles = MainPage.totalNumberOfCycles;
            this.TotalRunTimeMilliSeconds = MainPage.totalRuntimeMilliseconds;
            this.CurrentSystemState = MainPage.currentSystemState;
            this.CycleLights = StartPage.cycleLights;
            this.TotalNumberOfCycles = MainPage.totalNumberOfCycles;

            //find all references to cycle lights
            //    AND CYCLE COUNT
        }
    }
}
