﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlRepository.Abstract;
using SqlRepository.Models;

namespace SqlRepository
{
    class HosRepository: IHosRepository
    {
        public List<DriverWorkstate> FindDriverWorkStates(int driverId)
        {
            using (var hosDbContext = new HosDBContext())
            {
                return hosDbContext.DriverWorkstates.Where(dw => dw.DriverId == driverId).ToList();
            }
        }

        public List<DriverWorkstate> FindDriverWorkStates(int driverId, DateTime from)
        {
            using (var hosDbContext = new HosDBContext())
            {
                return
                    hosDbContext.DriverWorkstates.Where(dw => dw.DriverId == driverId && dw.Timestamp > from).ToList();
            }
        }

        public List<DriverWorkstate> FindDriverWorkStates(int driverId, DateTime from, DateTime to)
        {
            using (var hosDbContext = new HosDBContext())
            {
                return
                    hosDbContext.DriverWorkstates.Where(
                        dw => dw.DriverId == driverId && dw.Timestamp > from && dw.Timestamp < to).ToList();
            }
        }

        public DriverWorkstate SaveDriverWorkstate(DriverWorkstate driverWorkstate)
        {
            try
            {
                using (var hosDbContext = new HosDBContext())
                {
                    if (driverWorkstate.DriverWorkstateId != 0)
                    {
                        hosDbContext.DriverWorkstates.Attach(driverWorkstate);
                        hosDbContext.Entry(driverWorkstate).State = EntityState.Modified;
                    }
                    else
                    {
                        hosDbContext.DriverWorkstates.Add(driverWorkstate);
                    }

                    hosDbContext.SaveChanges();
                }
                return driverWorkstate;
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public DriverSummary FindDriverSummary(int driverId, int workstateId)
        {
            using (var hosDbContext = new HosDBContext())
            {
                return
                    hosDbContext.DriverSummaries.FirstOrDefault(
                        ds => ds.DriverId == driverId && ds.WorkStateId == workstateId);
            }
        }

        public DriverSummary SaveDriverSummary(DriverSummary driverSummary)
        {
            try
            {
                using (var hosDbContext = new HosDBContext())
                {
                    var tempDriverSummary =
                        hosDbContext.DriverSummaries.FirstOrDefault(
                            ds => ds.DriverId == driverSummary.DriverId && ds.WorkStateId == ds.WorkStateId);

                    if (tempDriverSummary == null)
                    {
                        hosDbContext.DriverSummaries.Add(driverSummary);
                    }
                    else
                    {
                        hosDbContext.DriverSummaries.Attach(driverSummary);
                        hosDbContext.Entry(driverSummary).State = EntityState.Modified;
                    }

                    hosDbContext.SaveChanges();
                }
                return driverSummary;
            }
            catch (Exception exception)
            {
                return null;
            }
        }
    }
}
