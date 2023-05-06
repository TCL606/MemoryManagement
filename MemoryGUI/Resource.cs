using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;
using MemoryManage;

namespace MemoryGUI
{
    public enum FitAlgorithm
    {
        FirstFit = 1,
        NextFit = 2,
        BestFit = 3,
        WorstFit = 4
    }

    public static class Utils
    {
        public static Fit? GetFitAlgorithm(string name)
        {
            switch (name)
            {
                case "FirstFit": return new FirstFit();
                case "NextFit": return new NextFit();
                case "BestFit": return new BestFit();
                case "WorstFit": return new WorstFit();
                default: return null;
            }
        }
    }

    public class Command : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => exeFunc();

        private Action exeFunc;

        public Command(Action exe)
        {
            this.exeFunc = exe;
        }
    }

    public class Log
    {
        private int maxMsgNum;

        public string Info => Display();

        private Queue<string> InfoQueue { get; set; }

        public Log(int maxMsgNum)
        {
            this.maxMsgNum = maxMsgNum;
            InfoQueue = new();
        }

        public void AddNewInfo(string newInfo)
        {
            if (InfoQueue.Count < maxMsgNum)
                InfoQueue.Enqueue(newInfo);
            else
            {
                InfoQueue.Dequeue();
                InfoQueue.Enqueue(newInfo);
            }
        }

        public void Reset()
        {
            this.InfoQueue.Clear();
        }

        public string Display()
        {
            string res = "";
            foreach (var item in InfoQueue)
            {
                res = item + "\n" + res;
            }
            return res;
        }
    }

}
