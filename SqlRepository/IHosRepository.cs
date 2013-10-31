using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlRepository.Models;

namespace SqlRepository.Abstract
{
    interface IHosRepository
    {
        List<DriverWorkstate> FindDriverWorkStates(int driverId);
        List<DriverWorkstate> FindDriverWorkStates(int driverId, DateTime from);
        List<DriverWorkstate> FindDriverWorkStates(int driverId, DateTime from, DateTime to);
        DriverWorkstate SaveDriverWorkstate(DriverWorkstate driverWorkstate);

        DriverSummary FindDriverSummary(int driverId, int workstateId);
        DriverSummary SaveDriverSummary(DriverSummary driverSummary);
    }
}
