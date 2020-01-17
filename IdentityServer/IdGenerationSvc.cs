using EasyOffer.FrontEnd.Authentication.IdentityServer.Properties;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyOffer.FrontEnd.Authentication.IdentityServer
{
    public class IdGenerationSvc 
    {
        private static SnowflakeWorker _worker;

        public IConfiguration _configuration;

        public IdGenerationSvc(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public long GenerateId()
        {
            if (_worker == null)
            {
                lock (this)
                {
                    if (!int.TryParse(_configuration["Snowflake:DatacenterId"], out int datacenterId))
                    {
                        throw new InvalidOperationException(Resources.SnowflakeDatacenterIdIsMissing);
                    }

                    if (!int.TryParse(_configuration["Snowflake:ServerId"], out int serverId))
                    {
                        throw new InvalidOperationException(Resources.SnowflakeServerIdIsMissing);
                    }

                    _worker = new SnowflakeWorker(datacenterId - 1, serverId - 1);
                }
            }

            return _worker.NextId();
        }
    }

    public class SnowflakeWorker
    {
        public static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // 2019-01-01
        public const long StartTimestamp = 1546300800L;

        // 每一部分占用的位数
        private const int DatacenterIdBits = 4;
        private const int WorkerIdBits = 8;
        private const int SequenceBits = 12;

        // 每一部分的最大值
        private const long MaxDatacenterId = -1L ^ (-1L << DatacenterIdBits);
        private const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);
        private const long MaxSequence = -1L ^ (-1L << SequenceBits);

        // 每一部分向左的位移
        private const int DatacenterIdLeftShift = SequenceBits + WorkerIdBits;
        private const int WorkerIdLeftShift = SequenceBits;
        private const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;

        public long DatacenterId { get; protected set; }

        public long WorkerId { get; protected set; }

        public long Sequence { get; internal set; }

        private long _lastTimestamp = -1L;
        private readonly object _lock = new Object();

        public SnowflakeWorker(long datacenterId, long workerId, long sequence = 0L)
        {
            if (datacenterId > MaxDatacenterId || datacenterId < 0)
            {
                throw new ArgumentException(string.Format(Resources.DatacenterIdCannotGreater_X_OrLessThan0, MaxDatacenterId), nameof(datacenterId));
            }

            if (workerId > MaxWorkerId || workerId < 0)
            {
                throw new ArgumentException(string.Format(Resources.WorkerIdCannotGreater_x_OrLessThan0, MaxWorkerId), nameof(workerId));
            }

            DatacenterId = datacenterId;
            WorkerId = workerId;
            Sequence = sequence;

            //log.info($"worker starting. timestamp left shift {TimestampLeftShift}, datacenter id bits {DatacenterIdBits}, worker id bits {WorkerIdBits}, sequence bits {SequenceBits}, workerid {workerId}");
        }

        public virtual long NextId()
        {
            lock (_lock)
            {
                var timestamp = CurrentTimestamp();

                if (timestamp < _lastTimestamp)
                {
                    throw new InvalidOperationException(string.Format(Resources.ClockMovedBackwards, _lastTimestamp - timestamp));
                }

                if (_lastTimestamp == timestamp)
                {
                    Sequence = (Sequence + 1) & MaxSequence;

                    if (Sequence == 0)
                    {
                        timestamp = TilNextTime(_lastTimestamp);
                    }
                }
                else
                {
                    Sequence = 0;
                }

                _lastTimestamp = timestamp;

                var id = ((timestamp - StartTimestamp) << TimestampLeftShift)
                         | (DatacenterId << DatacenterIdLeftShift)
                         | (WorkerId << WorkerIdLeftShift)
                         | Sequence;

                return id;
            }
        }

        private long TilNextTime(long lastTimestamp)
        {
            var timestamp = CurrentTimestamp();

            while (timestamp <= lastTimestamp)
            {
                timestamp = CurrentTimestamp();
            }

            return timestamp;
        }

        protected virtual long CurrentTimestamp()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalSeconds;
        }
    }
}
