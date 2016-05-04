using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using System.Diagnostics;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace BareBonesSparkInCloud
{
    public sealed class StartupTask : IBackgroundTask
    {

        private const int HEARTBEAT_PIN = 5;
        private GpioPin heartBeatPin;

        private const int LEDPIN_CYCLE = 6;
        private GpioPin ledCycleConfirmation;


        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // 
            // TODO: Insert code to perform background work
            //
            // If you start any asynchronous methods here, prevent the task
            // from closing prematurely by using BackgroundTaskDeferral as
            // described in http://aka.ms/backgroundtaskdeferral
            //

            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            InitGPIO();
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();
            if(gpio==null)
            {
                Debug.Write("There is no GPIO controller.");
                return;
            }

            heartBeatPin = gpio.OpenPin(HEARTBEAT_PIN);
            //heartBeatPin.DebounceTimeout = 
            if (heartBeatPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                heartBeatPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            else
                heartBeatPin.SetDriveMode(GpioPinDriveMode.Input);
            heartBeatPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
            heartBeatPin.ValueChanged += HeartBeatPin_ValueChanged;


            ledCycleConfirmation = gpio.OpenPin(LEDPIN_CYCLE);
            ledCycleConfirmation.SetDriveMode(GpioPinDriveMode.Output);



        }

        private void HeartBeatPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            Debug.WriteLine("Pin Number: {0} Value Changed.  EDGE: {1}", sender.PinNumber, args.Edge);


            if(args.Edge == GpioPinEdge.RisingEdge)
            {
                PostToCloud();
            }
        }

        private void PostToCloud()
        {
            Debug.WriteLine("POST");

            if (ledCycleConfirmation.Read() == GpioPinValue.High)
            {
                ledCycleConfirmation.Write(GpioPinValue.Low);
            }
            else
            {
                ledCycleConfirmation.Write(GpioPinValue.High);
            }

        }
    }
}
