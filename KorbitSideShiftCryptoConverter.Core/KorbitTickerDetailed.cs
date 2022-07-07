// See https://aka.ms/new-console-template for more information

namespace KorbitSideShiftCryptoConverter.Core
{
    internal class KorbitTickerDetailed
    {
        public long Timestamp { get; set; }
        public decimal Last { get; set; }
        public decimal Open { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
        public decimal Low { get; set; }
        public decimal High { get; set; }
        public decimal Volume { get; set; }
        public decimal Change { get; set; }
        public double ChangePercent { get; set; }
    }
}