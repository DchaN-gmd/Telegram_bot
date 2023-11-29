public class Game
{
    public string Name { get; private set; }

    public string Desciption { get; private set; }

    public bool IsPack { get; private set; }

    public string ImageName { get; private set; }

    public DatePrice DatePrice { get; private set; }

    public Game(string name, string description, string imageName, bool isPack)
    {
        Name = name;
        Desciption = description;
        ImageName = imageName;
        IsPack = isPack;
    }

    public void SetDatePrice(DatePrice datePrice)
    {
        DatePrice = datePrice; 
    }
}