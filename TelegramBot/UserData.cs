using Telegram_Bot;

class UserData
{
    public List<Game> _games = new();
    public List<QA> _qaList = new();

    public bool _isSelectGame = false;
    public bool _isRegistrationInput = false;
    public bool _isInput = false;
    public bool _isPay = false;
    public bool _isSendPayment = false;
    public bool _isPreCheckout = false;
    public bool _isGetQuestion = false;
    public bool _isPaymentInsturction = false;
    public bool _isBlockMenu = false;
    public bool _isGenerateLicense = false;
    public bool _isGenerateLicenseAdmin = false;
    public bool _isGetDistributionMessage = false;

    public Game _gameToBuy;
    public Game _gamePack;
    public RegestrationState _regestrationState = RegestrationState.empty;
    public RegestrationState _adminRegestrationState = RegestrationState.empty;

    public string _username;
    public string _phone;
    public string _email;
    public string _hardwareID;
    public string _transactionKey;
    public int _price;
    public string _distributionText;

    public void ResetDate()
    {
        _isSelectGame = false;
        _isGetQuestion = false;
        _gameToBuy = null;
        _gamePack = null;
        _isPaymentInsturction = false;
        _isRegistrationInput = false;
        _games.Clear();
        _transactionKey = null;
        _hardwareID = null;
        _isGenerateLicenseAdmin = false;
        _phone = null;
        _username = null;
        _isGetDistributionMessage = false;
        _isGenerateLicense=false;
        _email = null;
        _distributionText = null;

        _regestrationState = RegestrationState.empty;
        _adminRegestrationState = RegestrationState.empty;
    }
}