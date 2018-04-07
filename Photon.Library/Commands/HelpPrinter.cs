using Photon.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Photon.Library.Commands
{
    public class HelpPrinter
    {
        private readonly Dictionary<string, string> optionList;

        public string Title {get; set;}
        public string Description {get; set;}
        public int NameWidth {get; set;} = 20;


        public HelpPrinter(string title = null, string description = null)
        {
            this.Title = title;
            this.Description = description;

            optionList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public HelpPrinter(Type classType, string methodName)
        {
            this.Title = CommandAttribute.GetName(classType, methodName);
            this.Description = CommandAttribute.GetDescription(classType, methodName);

            optionList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public HelpPrinter Add(string name, string description)
        {
            optionList[name] = description;
            return this;
        }

        public HelpPrinter Add(Type classType)
        {
            var name = CommandAttribute.GetName(classType);
            optionList[name] = CommandAttribute.GetDescription(classType);
            return this;
        }

        public HelpPrinter Add(Type classType, string methodName)
        {
            var name = CommandAttribute.GetName(classType, methodName);
            optionList[name] = CommandAttribute.GetDescription(classType, methodName);
            return this;
        }

        public void Print()
        {
            if (!string.IsNullOrEmpty(Description)) {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  ");
                Console.WriteLine(Description);
                Console.WriteLine();
            }

            if (!string.IsNullOrEmpty(Title)) {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"  {Title} ");
            }

            foreach (var option in optionList)
                PrintAction(4, option.Key, NameWidth, option.Value);
        }

        public async Task PrintAsync()
        {
            await Task.Run(() => Print());
        }

        private static void PrintAction(int indent, string name, int nameWidth, string description)
        {
            var indentString = new string(' ', indent);

            var nameString = name
                .Truncate(nameWidth)
                .PadRight(nameWidth, ' ');

            Console.Write(indentString);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(nameString);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(description);
        }
    }
}
