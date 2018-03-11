using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace DotNetCoreWebApi.Controllers
{
    [Route("api/minerstats")]
    public class MinerstatsController : Controller
    {
        const int numberOfStatsToKeep = 100;
        private static ConcurrentDictionary<string, List<MinerStats>> minerStats = new ConcurrentDictionary<string, List<MinerStats>>();

        [HttpGet("{address}")]
        public List<MinerStats> Get(string address)
        {
            minerStats.TryGetValue(address, out var resp);
            return resp;
        }

        [HttpGet("{address}/total")]
        public OperationStats GetTotal(string address)
        {
            if (string.IsNullOrEmpty(address)) return new OperationStats();
            if(!minerStats.TryGetValue(address, out var resp))
            {
                return new OperationStats();
            }
            var rigs = resp.OrderByDescending(ppp => ppp.time).GroupBy(x => x.name).Select(grp => grp.ToList());

            var rigStats = new List<RigStats>();
            foreach (var rig in rigs)
            {
                rigStats.Add(new RigStats
                {
                    name = rig.First().name,
                    temperatures = rig.Select(x => x.temperature.GetValueOrDefault()).ToList(),
                    hashrates = rig.Select(x => x.hashrate.GetValueOrDefault()).ToList(),
                    currentHashrate = rig.First().hashrate.GetValueOrDefault(),
                    currentTemperature = rig.First().temperature.GetValueOrDefault(),
                    currentDateTime = rig.First().time.ToUniversalTime()
                });
            }

            return new OperationStats
            {
                address = address,
                rigStats = rigStats,
                currentHashrate = rigStats.Sum(x => x.currentHashrate),
                currentTemperature = rigStats.First().currentTemperature,
                lastUpdate = rigStats.First().currentDateTime.ToUniversalTime()
            };
        }

        public class OperationStats
        {
            public string address;
            public decimal currentHashrate;
            public decimal currentTemperature;
            public DateTime lastUpdate;
            public List<RigStats> rigStats; 
        }

        public class RigStats
        {
            public string name;
            public List<decimal> temperatures;
            public List<decimal> hashrates;
            public decimal currentHashrate;
            public decimal currentTemperature;
            public DateTime currentDateTime;

        }

        [HttpPost]
        public void Post([FromBody]MinerStats value)
        {
            if (string.IsNullOrEmpty(value.address))
                return;

            value.time = DateTime.Now;

            minerStats.AddOrUpdate(
                value.address, 
                new List<MinerStats>() { value },
                (k, v) => {
                    v.Insert(0, value);
                    if (v.Count > numberOfStatsToKeep) v.RemoveAt(numberOfStatsToKeep);
                    return v;
                });
        }
    }

    public class MinerStats
    {
        public string address;
        public string name;
        public decimal? temperature;
        public decimal? hashrate;
        public DateTime time;
    }

}
