using System;
using System.Text.RegularExpressions;
using LabsUtil;


class Program
{
    static void Main(string[] args)
    {
        int choice;
        System.Console.WriteLine("Choose input:");
        System.Console.WriteLine("1. Enter your own string");
        System.Console.WriteLine("2. Use a test string");
        choice = PstuUtil.TryReadT<int>("?:", 1, 2);
        string? input = null;
        bool isNotNull = false;
        bool moreThenOneSentence = false;
        do
        {            
            if (choice == 2 && input == null)
            {
                input = "В лесу родилась елочка. В лесу она росла. Зимой и летом стройная, зеленая была.";
                System.Console.WriteLine($"Orig sentence: \n{input}");
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
            string[] reorderedSentences = new string[sentences.Length];
            reorderedSentences[0] = sentences[^1];
            for (int i = 1; i < sentences.Length; i++)
                reorderedSentences[i] = sentences[sentences.Length - i - 1];
            string result = string.Join(" ", reorderedSentences);
            System.Console.WriteLine($"result: {result}");
        } while (!isNotNull || !moreThenOneSentence);      
    }
}