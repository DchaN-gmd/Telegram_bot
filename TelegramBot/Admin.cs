internal class Admin
{
    public string ID { get; private set; }
    public string Name { get; private set; }

    public Admin(string id, string name)
    {
        ID = id;
        Name = name;
    }
}