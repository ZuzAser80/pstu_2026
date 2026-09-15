namespace LabsUtil;

public class PstuUtil
{
    public static T TryReadT<T>(string prompt) where T : IParsable<T>
    {
        System.Console.WriteLine(prompt);
        T res;
        var r = Console.ReadLine();
        bool isParsed = false;
        do
        {               
            isParsed = T.TryParse(r, null, out res);
            if (!isParsed || res == null)
            {
                System.Console.WriteLine("wrong input try again (it didn't parse)");
            }
        } while (!isParsed || res == null);
        return res;
    }
}