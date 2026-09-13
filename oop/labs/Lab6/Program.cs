using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        int choice;
        bool validChoice = false;
        do
        {
            System.Console.WriteLine("Choose input:");
            System.Console.WriteLine("1. Enter your own string");
            System.Console.WriteLine("2. Use a test string");
            validChoice = int.TryParse(Console.ReadLine(), out choice) && (choice == 1 || choice == 2);
            if (!validChoice)
            {
                System.Console.WriteLine("invalid choice");
            }
        } while (!validChoice);

        string? input = null;
        bool isNotNull = false;
        bool moreThenOneSentence = false;
        do
        {
            if (choice == 2 && input == null)
            {
                input = "В лесу родилась елочка. В лесу она росла. Зимой и летом стройная, зеленая была.";
            }
            else
            {
                System.Console.WriteLine("Input sentence: ");
                input = Console.ReadLine();
            }
            isNotNull = input != null;
            if (!isNotNull || input == "")
            {
                System.Console.WriteLine("string is null");
                continue;
            }
            
            moreThenOneSentence = Regex.IsMatch(input, @"^(?:[^.!?]*[.!?](?=[\p{L}\s]|$)){2}");
            if (!moreThenOneSentence)
            {
                System.Console.WriteLine("string contains only one sentence:");                
                continue;
            }
            var sentences = Regex.Split(input, @"(?<=[.!?])\s+");
            string result = "";
            result += sentences?[sentences.Count() - 1];
            for (int i = 1; i <= sentences?.Count() - 2; i++)
            {
                result += sentences?[i];
            }
            result += sentences?[0];    
            System.Console.WriteLine($"result: {result}");
        } while (!isNotNull || !moreThenOneSentence);      
    }
}