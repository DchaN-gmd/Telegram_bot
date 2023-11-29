public class QA
{
    public string Question { get; private set; }

    public string Answer { get; private set; }


    public QA(string question, string answer)
    {
        Question = question;
        Answer = answer;
    }
}