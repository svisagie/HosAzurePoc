using System.Runtime.Serialization;
using System.ServiceModel;

namespace HosUI.Models
{
    [DataContract]
    public class DriverData
    {
        [DataMember]
        public int DriverId { get; set; }
        [DataMember]
        public string DriverName { get; set; }
    }

    [ServiceContract]
    public interface IDriverContract
    {
        [OperationContract]
        DriverData GetDriver(int driverId);
    }

    public interface IDriverChannel : IDriverContract, IClientChannel { }
}