using System;

namespace IOTServer.StatisticData
{
	public class StatisticElement {
		public uint UnixTime { get; private set; }
		public long BytesCount { get; private set; }

		public StatisticElement(long bytesCount) {
			BytesCount = bytesCount;
			UnixTime = (uint)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
		}

		public override string ToString() {
			return string.Format("[StatisticData: UnixTime={0}, BytesCount={1}]", UnixTime, BytesCount);
		}
	}
}
