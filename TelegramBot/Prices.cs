public class Prices
{
    public IReadOnlyList<DatePrice> GamePrices => _gamePrices;

    private List<DatePrice> _gamePrices = new List<DatePrice>();

    public Prices(int? unlimited, int? oneMonth, int? threeMonth, int? sixMonth, int? oneYear, int? threeYear)
    {
        var Unlimited = unlimited ?? 0;
        var OneMonth = oneMonth ?? 0;
        var ThreeMonth = threeMonth ?? 0;
        var SixMonth = sixMonth ?? 0;
        var OneYear = oneYear ?? 0;
        var ThreeYear = threeYear ?? 0;

        if(!IsZero(Unlimited)) _gamePrices.Add(new DatePrice(GameDateTime.Unlimited, Unlimited));
        if (!IsZero(OneMonth)) _gamePrices.Add(new DatePrice(GameDateTime.OneMonth, OneMonth));
        if (!IsZero(ThreeMonth)) _gamePrices.Add(new DatePrice(GameDateTime.ThreeMonth, ThreeMonth));
        if (!IsZero(SixMonth)) _gamePrices.Add(new DatePrice(GameDateTime.SixMounth, SixMonth));
        if (!IsZero(OneYear)) _gamePrices.Add(new DatePrice(GameDateTime.OneYear, OneYear));
        if (!IsZero(ThreeMonth)) _gamePrices.Add(new DatePrice(GameDateTime.ThreeYear, ThreeYear));
    }

    private bool IsZero(int value)
    {
        return value == 0;
    }
}

public class DatePrice
{
    public GameDateTime DateTime { get; private set; }
    public int Amount { get; private set; }

    public DatePrice(GameDateTime dateTime, int amount)
    {
        DateTime = dateTime;
        Amount = amount;
    }
}

public enum GameDateTime
{
    Unlimited,
    OneMonth,
    ThreeMonth,
    SixMounth,
    OneYear,
    ThreeYear
}