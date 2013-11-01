using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using Microsoft.ServiceBus;

namespace HosDriverService
{
    [DataContract]
    public class DriverData
    {
        [DataMember]
        public int DriverId { get; set; }
        [DataMember]
        public string DriverName { get; set; }
    }

    public sealed class Drivers
    {
        static readonly Drivers _instance = new Drivers();
        public List<DriverData> DriversList = new List<DriverData>();

        public static Drivers Instance
        {
            get { return _instance; }
        }

        private Drivers()
        {
            DriversList.Clear();
            for (int i = 0; i < 1000; i++)
                DriversList.Add(new DriverData { DriverId = i, DriverName = "Driver" + i });
        }
    }

    [ServiceContract]
    public interface IDriverContract
    {
        [OperationContract]
        DriverData GetDriver(int driverId);
    }

    [ServiceBehavior]
    class DriverService : IDriverContract
    {
        public DriverData GetDriver(int driverId)
        {
            var driver = Drivers.Instance.DriversList.FirstOrDefault(d => d.DriverId == driverId);
            Console.WriteLine("Getting driver for id: {0}... returning {1}", driverId, driver.DriverName);
            return driver;
        }
    }

    public interface IDriverChannel : IDriverContract, IClientChannel { }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Initialising the Service Bus ");

            string serviceNamespace = "hospoc";
            string issuerName = "owner";
            string issuerSecret = "mEiPrhMsF+gtEuyAHXJvt9zwNH8OiFiuAI9j0W+qzbo=";

            TransportClientEndpointBehavior sharedSecretServicebusCredential = new TransportClientEndpointBehavior();
            sharedSecretServicebusCredential.TokenProvider = TokenProvider.CreateSharedSecretTokenProvider(issuerName, issuerSecret);

            Uri address = ServiceBusEnvironment.CreateServiceUri("sb", serviceNamespace, "DriverService");

            ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;
            ServiceHost host = new ServiceHost(typeof(DriverService), address);
            IEndpointBehavior serviceRegistrySettings = new ServiceRegistrySettings(DiscoveryType.Public);
            foreach (ServiceEndpoint endpoint in host.Description.Endpoints)
            {
                endpoint.Behaviors.Add(serviceRegistrySettings);
                endpoint.Behaviors.Add(sharedSecretServicebusCredential);
            }

            host.Open();
            Console.WriteLine("Service address: {0}", address);
            Console.WriteLine("Press any key to stop");
            Console.ReadLine();

            host.Close();
        }
    }
}
