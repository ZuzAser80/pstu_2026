namespace LabsUtil;

public class PstuUtil
{
    public static T TryReadT<T>(string prompt) where T : IParsable<T>
    {
        System.Console.WriteLine(prompt);
        T res;
        bool isParsed = false;
        do
        {                           
            isParsed = T.TryParse(Console.ReadLine(), null, out res);
            if (!isParsed || res == null)
            {
                System.Console.WriteLine("wrong input try again (it didn't parse)");
            }            
        } while (!isParsed || res == null);
        return res;
    }

    public static T TryReadT<T>(string prompt, T[] allowed_values) where T : IParsable<T>
    {
        System.Console.WriteLine(prompt);
        T res;
        bool isValid;
        do
        {
            isValid = T.TryParse(Console.ReadLine(), null, out res)
                      && res != null
                      && allowed_values.Contains(res);
            if (!isValid)
            {
                System.Console.WriteLine("wrong input try again (parse error or not allowed value)");
            }
        } while (!isValid);
        return res;
    }
}