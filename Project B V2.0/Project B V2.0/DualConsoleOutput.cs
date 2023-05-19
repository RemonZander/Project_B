using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Project_B_V2._0
{
    internal class DualConsoleOutput : TextWriter
    {

        Encoding encoding = Encoding.UTF8;
        private StreamWriter writer;
        private TextWriter console;
        public override Encoding Encoding{get{return encoding;}}
        private string filepath;
        public DualConsoleOutput(string filePath,TextWriter console, Encoding? encoding = null)
        {
            if (encoding != null)
            {
                this.encoding = encoding;
            }
            this.filepath = filePath;
            this.console = console;
            File.Delete(filePath);
            this.writer = new StreamWriter(filePath,false, this.encoding);
            this.writer.AutoFlush = true;
        }
        
        public override void Write(string? value)
        {
            if (!Console.IsInputRedirected)
            {
                Console.SetOut(console);
                Console.Write(value);
            }
            Console.SetOut(this);  
            this.writer.Write(value);  
        }

        public override void WriteLine(string? value)
        {
            if (!Console.IsInputRedirected)
            {
                Console.SetOut(console);
                Console.WriteLine(value);
            }
            Console.SetOut(this);
            this.writer.WriteLine(value);
        }

        public override void WriteLine()
        {
            if (!Console.IsInputRedirected)
            {
                Console.SetOut(console);
                Console.WriteLine();
            }
            Console.SetOut(this);
            this.writer.WriteLine();
        }

        public override void Flush()
        {
            this.writer.Flush();
        }

        public override void Close()
        {
            this.writer.Close();
        }

        public new void Dispose()
        {
            this.writer.Flush();
            this.writer.Close();
            this.writer.Dispose();
            base.Dispose();
        }

        public void ReStartWriter()
        {
            this.writer = new StreamWriter(filepath, false, this.encoding);
            this.writer.AutoFlush = true;
        }
    }  
}
