namespace Services
{
    using System;

    public class Dispatcher
    {
        public void Invoke(Action action)
        {

        }
    }

    
    public class Application
    {
        public static Application Current => new Application();
    
        public Dispatcher Dispatcher => new Dispatcher();
    }

    public class OpenFileDialog
    {
        public string FileName { get;set; }
        public string DefaultExt {get;set;}
        public string Filter {get;set;}
        public bool? ShowDialog()
        {
            return false;
        }
    }
}