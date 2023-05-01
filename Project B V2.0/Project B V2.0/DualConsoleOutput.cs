using System;
using System.Collections.Generic;
using System.Linq;
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
        public DualConsoleOutput(string filePath,TextWriter console, Encoding encoding = null)
        {
            if (encoding != null)
            {
                this.encoding = encoding;
            }
            this.console = console;
            File.Delete(filePath);
            this.writer = new StreamWriter(filePath,false, this.encoding);
            this.writer.AutoFlush = true;
        }
        
        public override void Write(string value)
        {
            Console.SetOut(console);
            Console.Write(value);
            Console.SetOut(this);  
            this.writer.Write(value);  
        }

        public override void WriteLine(string value)
        {
            Console.SetOut(console);
            Console.WriteLine(value);
            this.writer.WriteLine(value);
            Console.SetOut(this);
        }

        public override void Flush()
        {
            this.writer.Flush();
        }

        public override void Close()
        {
            this.writer.Close();
        }

        public void Dispose()
        {
            this.writer.Flush();
            this.writer.Close();
            this.writer.Dispose();
            base.Dispose();
        }
    }  
}
