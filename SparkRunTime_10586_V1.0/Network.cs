using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace SparkRunTime_10586_V1._0
{
    public class Network
    {
        private string _macAddress;
        private bool _networkUp;
        public bool IsNetworkAvailable; //Use NetworkINterface.IsNetworkAvailabe

        public List<string> Errors;
        public List<string> ObjectMessages;

        public Network()
        {
            this.Errors = new List<string>();
            this.ObjectMessages = new List<string>();
        }
        public string MacAddress
        {
            get
            {
                return "NO MAC ADDRESS FOUND";
            }
            set
            {
                _macAddress = value;
            }
        }

        public bool NetworkUp
        {
            get
            {
                return NetworkInterface.GetIsNetworkAvailable();
            }
            set
            {
                _networkUp = value;
            }
        }

   


    }
}
