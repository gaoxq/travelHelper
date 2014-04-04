using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace Appfinal
{
    public class NetworkHelper
    {
        public static bool IsConnectedToInternet()
        {
            bool isConnected = false;

            ConnectionProfile cp = NetworkInformation.GetInternetConnectionProfile();
            if (cp != null)
            {
                NetworkConnectivityLevel cl = cp.GetNetworkConnectivityLevel();
                isConnected = (cl == NetworkConnectivityLevel.InternetAccess);
            }

            return isConnected;
        }
    }
}
