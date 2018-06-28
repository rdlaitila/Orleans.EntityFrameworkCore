using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Orleans;
using Orleans.Runtime;

namespace Orleans.EntityFrameworkCore
{
    public static class OrleansEFMapper
    {
        public static MembershipTableData Map(List<OrleansEFMembership> src, MembershipTableData dst = null)
        {
            var entries = src.Select(a => {
                return new Tuple<MembershipEntry, string>(
                    new MembershipEntry
                    {
                        SiloAddress = Map(
                            a.Address,
                            (int)a.Port,
                            a.Generation
                        ),
                        Status = (SiloStatus)a.Status,
                        ProxyPort = (int)a.ProxyPort,
                        HostName = a.HostName,
                        SiloName = a.SiloName,
                        StartTime = a.StartTime,
                        IAmAliveTime = a.IAmAliveTime,
                        UpdateZone = a.UpdateZone,
                        FaultZone = a.FaultZone,
                        SuspectTimes = a.SuspectTimes?.Split(";").Where(b =>
                            !string.IsNullOrWhiteSpace(b)
                        ).Select(b => {
                            var split = b.Split("::");
                            return new Tuple<SiloAddress, DateTime>(
                                SiloAddress.FromParsableString(split[0]),
                                DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(split[1])).UtcDateTime
                            );
                        }).ToList(),
                    },
                    a.DeploymentId
                );
            }).ToList();

            dst = new MembershipTableData(
                entries,
                new TableVersion(1, "whatever")
            );

            return dst;
        }

        public static OrleansEFMembership Map(MembershipEntry src, OrleansEFMembership dst = null)
        {
            dst = dst ?? new OrleansEFMembership();

            dst.SiloName = src.SiloName;
            dst.StartTime = src.StartTime;
            dst.Status = (int)src.Status;
            dst.ProxyPort = src.ProxyPort;
            dst.HostName = src.HostName;
            dst.IAmAliveTime = src.IAmAliveTime;
            dst.UpdateZone = src.UpdateZone;
            dst.FaultZone = src.FaultZone;
            dst.SuspectTimes = string.Join(";", src.SuspectTimes.Select(a =>
                $"{a.Item1.ToParsableString()}::{new DateTimeOffset(a.Item2).ToUnixTimeMilliseconds()}"
            ).ToArray());

            return dst;
        }

        public static SiloAddress Map(string srcAddress, int srcPort, int srcGeneration, SiloAddress dst = null)
        {
            dst = dst ?? SiloAddress.New(
                new IPEndPoint(IPAddress.Parse(srcAddress), srcPort),
                srcGeneration
            );
            return dst;
        }
    }
}