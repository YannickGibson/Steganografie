using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace Steganografie
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("*example: " +
                Environment.NewLine + "***stega --hide \"Zikmund je neznaboh\" anime.png" +
                Environment.NewLine + "***stega --show anime.png" + 
                Environment.NewLine + "***leave" + Environment.NewLine
                );

            string[] input;

            while (true)
            {
                bool end = false;


                string s = Console.ReadLine();
                //string s = "stega --hide \"Zikmund je neznaboh\" anime.png";
                //string s = "stega --show anime.png";

                string[] split = CommandSplit(s);
                string command = split[0];

                bool nothing = false;
                Result result = null;// will be overwritten
                try
                {
                    switch (command)
                    {
                        case "stega":
                            result = Steganography(split.Skip(1).ToArray());
                            break;
                        case "leave":
                            end = true;
                            break;
                        case "":
                            if (split.Length==0)// User only presses enter 
                                nothing = true;
                            break;
                    }
                }
                catch(Exception e)
                {
                    result = new Result() { error = "true", message = $"Error occured {e}" };
                }
                if (!nothing)
                {
                    if (result == null)
                    {
                        Console.WriteLine("Unknown command: \"" + command + "\"");
                    }
                    else if (result.error == null)
                    {
                        Console.WriteLine(result.message);
                    }
                    else
                    {
                        Console.WriteLine("Error: " + result.message);
                    }
                }



                if (end == true)
                {
                    break;
                }
            }

        }
        class Result
        {
            public string message;
            public string error;
        }
        static private Result Steganography(string[] args)
        {
            Result res = new Result();
            if (args[0] == "--show" || args[0] == "-s" && args.Length == 2)
            {
                string fileName = args[1];
                Image img = Image.FromFile(fileName);
                Bitmap bitmap = new Bitmap(img);
                int width = bitmap.Width;
                int x;
                int y;
                StringBuilder sb = new StringBuilder();
                bool messageFound = false;
                for (int i = 0; i < bitmap.Width*bitmap.Height; i++)
                {
                    x = i % width;
                    y = i / width;
                    Color pixelColor = bitmap.GetPixel(x, y);


                    if (pixelColor.A == 69 && pixelColor.R == 70 && pixelColor.G == 70 && pixelColor.B == 70)
                    {
                        messageFound = true;
                        break;
                    }
                    sb.Append((char)pixelColor.A);
                }
                string resMessage = "";
                if (messageFound)
                {
                    resMessage = $"Hidden text in the alpha channel is: \"{sb}\"";
                }
                else
                {
                    resMessage = "Message is not hidden here";
                }

                res = new Result() { message = resMessage };
                img.Dispose();
                img = null;
            }
            else if (args[0] == "--hide" || args[0] == "-h" && args.Length == 3)
            {
                string message = args[1].Substring(1, args[1].Length - 2);// "hello" => hello
                string fileName = args[2];

                StringBuilder sb = new StringBuilder();
                Image img = Image.FromFile(fileName); // saving so i can dispose it later
                Bitmap bitmap = new Bitmap(img);
                int width = bitmap.Width;
                int x = 0;
                int y = 0;
                int i = 0;
                for (; i < message.Length; i++)
                {
                    x = i % width;
                    y = i / width;
                    Color pixelColor = bitmap.GetPixel(x, y);
                    bitmap.SetPixel(x, y, Color.FromArgb(message[i], pixelColor.R, pixelColor.G, pixelColor.B));
                    pixelColor = bitmap.GetPixel(x, y);
                    sb.Append((char)pixelColor.A);
                }
                // Adding stop sign to the next pixel
                bitmap.SetPixel((x + 1)%width, (y + 1)/width, Color.FromArgb(69, 70, 70, 70));

                img.Dispose();
                img = null;
                bitmap.Save(fileName, ImageFormat.Png);

                res = new Result() { message = $"\"{message}\" was inserted in the alpha channel" };
            }
            else
            {
                res = new Result() { error="yes", message = $"'stega {string.Join(" ", args)}' is not a correct command" };
            }


            return res;

        }
        static private string[] CommandSplit(string s)
    {


        // Check For Quotes
        List<int> indexes = null;
        for (int i = 2; i < s.Length; i++) // start from 1 because the shortest command can be 1 char + space
        {
            if (s[i] == '"')
            {
                if (indexes == null) // lazy inicialization
                    indexes = new List<int>();
                // Before or after must be space depending if its first or last " . also on the end its not needed
                if (indexes.Count % 2 == 0 && s[i - 1] == ' ' ||
                    indexes.Count % 2 == 1 && (i == s.Length - 1 || s[i + 1] == ' '))
                    indexes.Add(i);
            }
        }

        string[] split = null;

        if (indexes != null && indexes.Count >= 2)// 2 or more quotes
        {
            List<string> stringArguments = new List<string>();
            List<int> argumentStartIndex = new List<int>();
            string inputCopy = s;
            for (int i = 0; i < indexes.Count / 2; i++)
            {// remove strings from copy
                int indexesIndex = 2 * i;
                if (indexesIndex + 1 < indexes.Count)
                {
                    int ind1 = indexes[indexesIndex];
                    int ind2 = indexes[indexesIndex + 1];

                    // Take out string in quotes with quotes and the one space on the left
                    string theString = inputCopy.Substring(ind1,
                        ind2 - ind1 + 1);
                    argumentStartIndex.Add(ind1);


                    stringArguments.Add(theString); // skip first char - space

                    // now i caan finally remove the string
                    // We are removing whitespace
                    inputCopy = inputCopy.Remove(ind1 - 1,
                        ind2 - ind1 + 2);// + 2 because of śpace from left and to get last quote
                }
            }
            List<string> withoutQuotes = new List<string>(inputCopy.Split(" "));
            int withoutQuotesIndexSum = 0;
            int insertedIndex = 0;
            int listIndex = 0;
            for (int i = 0; i < withoutQuotes.Count; i++)
            {
                withoutQuotesIndexSum += withoutQuotes[i].Length + 1;

                // if x == start Index of argument
                // - because im not counting one space before the first  command, which obviously isnt there
                if (withoutQuotesIndexSum >= argumentStartIndex[insertedIndex])
                {
                    string insertion = stringArguments[insertedIndex]; // Example: ' "My name is Tom"' 
                    withoutQuotes.Insert(listIndex + 1, insertion);
                    withoutQuotesIndexSum += insertion.Length;
                    i += 1; // Next argument (cuz we modify looped array)
                    insertedIndex += 1;
                }
                listIndex += 1;
                if (insertedIndex > argumentStartIndex.Count - 1)
                {
                    break;
                }
            }

            split = withoutQuotes.ToArray();
        }
        else
        {
            split = s.Split(" ");
        }
        return split;
    }
    }
}
