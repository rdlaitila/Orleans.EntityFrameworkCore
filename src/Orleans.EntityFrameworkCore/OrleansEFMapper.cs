using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Orleans.EntityFrameworkCore
{
    /// <summary>
    /// </summary>
    internal static class OrleansEFMapper
    {
        /// <summary>
        /// Maps the specified source.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="dst">The DST.</param>
        /// <returns></returns>
        internal static MembershipTableData Map(
            List<OrleansEFMembership> src,
            MembershipTableData dst = null
        )
        {
            var entries = src.Select(a =>
            {
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
                        ).Select(b =>
                        {
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

        /// <summary>
        /// Maps the specified source.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="dst">The DST.</param>
        /// <returns></returns>
        internal static OrleansEFMembership Map(
            MembershipEntry src,
            OrleansEFMembership dst = null
        )
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

        /// <summary>
        /// Maps the specified source address.
        /// </summary>
        /// <param name="srcAddress">The source address.</param>
        /// <param name="srcPort">The source port.</param>
        /// <param name="srcGeneration">The source generation.</param>
        /// <param name="dst">The DST.</param>
        /// <returns></returns>
        internal static SiloAddress Map(
            string srcAddress,
            int srcPort,
            int srcGeneration,
            SiloAddress dst = null
        )
        {
            dst = dst ?? SiloAddress.New(
                new IPEndPoint(IPAddress.Parse(srcAddress), srcPort),
                srcGeneration
            );
            return dst;
        }

        /// <summary>
        /// Maps the specified source.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="dst">The DST.</param>
        /// <returns></returns>
        internal static ReminderTableData Map(
            IGrainReferenceConverter converter,
            List<OrleansEFReminder> src,
            ReminderTableData dst = null
        )
        {
            var entries = src
                .Select(a => Map(converter, a))
                .ToList();

            dst = dst ?? new ReminderTableData(entries);

            return dst;
        }

        /// <summary>
        /// Maps the specified converter.
        /// </summary>
        /// <param name="converter">The converter.</param>
        /// <param name="src">The source.</param>
        /// <param name="dst">The DST.</param>
        /// <returns></returns>
        internal static ReminderEntry Map(
            IGrainReferenceConverter converter,
            OrleansEFReminder src,
            ReminderEntry dst = null
        )
        {
            dst = dst ?? new ReminderEntry();

            dst.ETag = src.ETag;
            dst.GrainRef = converter.GetGrainFromKeyString(src.GrainId);
            dst.Period = TimeSpan.FromMilliseconds(src.Period);
            dst.ReminderName = src.ReminderName;
            dst.StartAt = src.StartTime;

            return dst;
        }

        /// <summary>
        /// Maps the specified source.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="dst">The DST.</param>
        /// <returns></returns>
        internal static OrleansEFReminder Map(
            ReminderEntry src,
            OrleansEFReminder dst = null,
            string serviceId = null
        )
        {
            dst = dst ?? new OrleansEFReminder();

            dst.GrainId = src.GrainRef.ToKeyString();
            dst.ETag = src.ETag;
            dst.GrainHash = (int)src.GrainRef.GetUniformHashCode();
            dst.Period = src.Period.Milliseconds;
            dst.StartTime = src.StartAt;
            dst.ReminderName = src.ReminderName;
            dst.ServiceId = serviceId;

            return dst;
        }
    }
}