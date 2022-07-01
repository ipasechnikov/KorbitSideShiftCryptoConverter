// See https://aka.ms/new-console-template for more information

internal class SideShiftPair
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public decimal Rate { get; set; }
    public string DepositCoin { get; set; } = null!;
    public string SettleCoin { get; set; } = null!;
    public string DepositNetwork { get; set; } = null!;
    public string SettleNetwork { get; set; } = null!;
}