using System.ComponentModel.DataAnnotations;
using System.Data.SQLite;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Environment = System.Environment;

internal static class SQL
{
    public static string CONNECTION_STRING
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return @"Data Source = " + Environment.CurrentDirectory + @"\DataBase\SecretDataBase.db; Version = 3;";
            }
            else
                return @"Data Source = SecretDataBase.db;";
        }
    }

    public static void RegistrateUser(string userId, string username, string number, string email)
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"insert into Users (User_ID, Username, Number, Email) values('{userId}','{username}','{number}', '{email}')";
            command.ExecuteNonQuery();
        }
    }

    public static void RegistrateOrder(string userId, string hardwareId, string licenseKey, int price, string gameName,
        int monthsCount, string date, string transactionKey)
    {
        string username = null;
        string number = null;
        string email = null;

        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            
            command.CommandText = $"SELECT * FROM Users WHERE User_ID LIKE '{userId}'";
            var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                username = reader.GetString(2);
                number = reader.GetString(3);
                email = reader.GetString(4);
            }
        }

        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;

            if (username == null || number == null || email == null) throw new NullReferenceException();

            command.CommandText = $"insert into ClosedOrders (User_ID, Username, Number, Email, HardwareID, LicenseKey, Price, GameName, MonthsCount, Date, TransactionKey) values('{userId}','{username}','{number}','{email}','{hardwareId}', '{licenseKey}','{price}','{gameName}','{monthsCount}','{date}','{transactionKey}')";
            command.ExecuteNonQuery();
        }
    }

    public static void AdminRegistrateOrder(string username, string number, string email,  string hardwareId, string licenseKey, int price, string gameName, int monthsCount, string date)
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;

            command.CommandText = $"insert into ClosedOrders (Username, Number, Email, HardwareID, LicenseKey, Price, GameName, MonthsCount, Date) values('{username}','{number}','{email}','{hardwareId}', '{licenseKey}','{price}','{gameName}','{monthsCount}','{date}')";
            command.ExecuteNonQuery();
        }
    }

    public static bool IsUserExist(string userId)
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"select 1 from Users where User_ID like '{userId}'";
            return command.ExecuteScalar() != null;
        }
    }

    public static bool IsUserIDExist(string userId)
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"select 1 from Users_ID where User_ID like '{userId}'";
            return command.ExecuteScalar() != null;
        }
    }

    public static void RegistrateUserID(string userId)
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"insert into Users_ID (User_ID) values('{userId}')";
            command.ExecuteNonQuery();
        }
    }

    public static List<Game> GetGames()
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            var games = new List<Game>();
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"SELECT * FROM Games";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                games.Add(new Game(reader.GetString(1),reader.GetString(2), reader.GetString(3), reader.GetInt32(4) == 1));
            }

            return games;
        }
    }

    public static List<QA> GetQA()
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            var qa = new List<QA>();
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"SELECT * FROM QA";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                qa.Add(new QA(reader.GetString(1), reader.GetString(2)));
            }

            return qa;
        }
    }

    public static Prices GetPrices(string GameName)
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"SELECT * FROM Prices WHERE GameName LIKE '{GameName}' OR PackName LIKE '{GameName}'";
            var reader = command.ExecuteReader();
            Prices gamePrices = null;
            while (reader.Read())
            {
                gamePrices = new Prices(reader.GetInt32(4), reader.GetInt32(5), reader.GetInt32(6), reader.GetInt32(7), reader.GetInt32(8), reader.GetInt32(9));
            }
            return gamePrices;
        }
    }

    public static string GetSetupLink(string gameName)
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"SELECT * FROM Products WHERE GameName LIKE '{gameName}' OR PackName LIKE '{gameName}'";
            var reader = command.ExecuteReader();
            string link = null;
            while (reader.Read())
            {
                link = reader.GetString(1);
            }
            return link;
        }
    }

    public static string GetGameDemoLink(string gameName)
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"SELECT * FROM Products WHERE GameName LIKE '{gameName}' OR PackName LIKE '{gameName}'";
            var reader = command.ExecuteReader();
            string link = null;
            while (reader.Read())
            {
                link = reader.GetString(6);
            }
            return link;
        }
    }

    public static List<string> GetListUsersId()
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"SELECT User_ID FROM Users_ID";
            var reader = command.ExecuteReader();
            List<string> usersId = new();
            while (reader.Read())
            {
                usersId.Add(reader.GetString(0));
            }
            return usersId;
        }
    }

    public static List<Game> GetGamePacks(string gameName)
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            var games = new List<Game>();
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"SELECT * FROM GamePacks where GameName like '{gameName}'";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                games.Add(new Game(reader.GetString(2), reader.GetString(3), reader.GetString(4), false));
            }

            return games;
        }
    }

    public static List<Game> GetNewGames()
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            var games = new List<Game>();
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"SELECT * FROM NewGames";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                games.Add(new Game(reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetInt32(4) == 1));
            }

            return games;
        }
    }

    public static string GetUserEmail(string userId)
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"SELECT Email FROM Users WHERE User_ID LIKE '{userId}'";
            var reader = command.ExecuteReader();
            string userEmail = null;
            while (reader.Read())
            {
                userEmail = reader.GetString(0);
            }

            return userEmail;
        }
    }

    public static void GetOpenCloseKeys(string gameName, out string openKey, out string closeKey)
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            var games = new List<Game>();
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"SELECT * FROM Products WHERE GameName LIKE '{gameName}' OR PackName LIKE '{gameName}'";
            var reader = command.ExecuteReader();
            openKey = null;
            closeKey = null;

            while (reader.Read())
            {
                openKey = reader.GetString(2);
                closeKey = reader.GetString(3);
            }
        }
    }

    public static List<string> GetNewsMessages()
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"SELECT * FROM NewsMessage";
            var reader = command.ExecuteReader();
            List<string> messages = new ();
            while (reader.Read())
            {
                 messages.Add(reader.GetString(1));
            }
            return messages;
        }
    }

    public static List<Admin> GetAdmins()
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            var admins = new List<Admin>();
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"SELECT * FROM Admins";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                admins.Add(new Admin(reader.GetString(1), reader.GetString(2)));
            }

            return admins;
        }
    }

    public static void GetCompanyContacts(out string phone, out string mail, out string website, out string companyName)
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"SELECT * FROM Contacts";
            var reader = command.ExecuteReader();

            phone = null;
            mail = null;
            website = null;
            companyName = null;
            while (reader.Read())
            {
                phone = reader.GetString(1);
                mail = reader.GetString(2);
                website = reader.GetString(3);
                companyName = reader.GetString(4);
            }
        }
    }

    public static string GetTelegramLink()
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"SELECT TelegramLink FROM Contacts";
            var reader = command.ExecuteReader();
            string link = null;
            while (reader.Read())
            {
                link = reader.GetString(0);
            }

            return link;
        }
    }

    public static string GetWhatsAppLink()
    {
        using (var connection = new SQLiteConnection(CONNECTION_STRING))
        {
            connection.Open();
            var command = new SQLiteCommand();
            command.Connection = connection;
            command.CommandText = $"SELECT WhatsAppLink FROM Contacts";
            var reader = command.ExecuteReader();
            string link = null;
            while (reader.Read())
            {
                link = reader.GetString(0);
            }

            return link;
        }
    }
}