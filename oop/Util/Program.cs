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
                System.Console.WriteLine("wrong input try again \n(it didn't parse)");
            }            
        } while (!isParsed || res == null);
        return res;
    } 
    public static T TryReadT<T>(string prompt, T min_allowed, T max_allowed) where T : IParsable<T>, IComparable
    {        
        T res;
        bool isValid;
        do
        {
            res = TryReadT<T>(prompt);
            isValid = min_allowed.CompareTo(res) <= 0 && max_allowed.CompareTo(res) >= 0;
            if (!isValid)
            {
                System.Console.WriteLine($"allowed range: from {min_allowed} to {max_allowed} inclusive.");
            }
        } while (!isValid);
        return res;
    }
}

public class PstuMenu
{
    public PstuMenu(List<Tuple<String, List<MenuItem>>> tuples)
    {
        foreach (var a in tuples)
        {
            
        }
    }
}

public class MenuItem { 
    public String content; 
    public Action onSelect;
    public MenuItem(String s, Action a)
    {
        content = s;
        onSelect = a;
    }
 }