using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MemoryManage;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MemoryGUI
{
    public class MainWindowViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public virtual void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public MainWindow? Parent { get; set; }

        private string inputMemSize = "";
        public string InputMemSize
        {
            get => inputMemSize;
            set
            {
                inputMemSize = value;
                RaisePropertyChanged(nameof(InputMemSize));
            }
        }

        private string inputStartAddr = "";
        public string InputStartAddr
        {
            get => inputStartAddr;
            set
            {
                inputStartAddr = value;
                RaisePropertyChanged(nameof(InputStartAddr));
            }
        }

        public ObservableCollection<string> AvailFitAlgs { get; } = new ObservableCollection<string>(Enum.GetNames(typeof(FitAlgorithm)));

        public string fitAlg = "";
        public string FitAlg
        {
            get => fitAlg;
            set
            {
                fitAlg = value;
                RaisePropertyChanged(nameof(FitAlg));
            }
        }

        public MemoryManager? MemoryManager { get; private set; }

        private bool isStart = false;
        public bool IsStart
        {
            get => isStart;
            set
            {
                isStart = value;
            }
        }

        private Log log;
        public string Info
        {
            get => log.Info;
            set
            {
                log.AddNewInfo(value);
                RaisePropertyChanged(nameof(Info));
            }
        }

        private string allocateMemorySize = "";
        public string AllocateMemorySize
        {
            get => allocateMemorySize;
            set
            {
                allocateMemorySize = value;
                RaisePropertyChanged(nameof(AllocateMemorySize));
            }
        }

        private string freeMemoryStartAddr = "";
        public string FreeMemoryStartAddr
        {
            get => freeMemoryStartAddr;
            set
            {
                freeMemoryStartAddr = value;
                RaisePropertyChanged(nameof(FreeMemoryStartAddr));
            }
        }

        public Command StartCommand { get; init; }
        public void CStart()
        {
            if (IsStart)
            {
                Info = "Error: The demo is running. Please reset to restart";
                return;
            }
            else
            {
                int memSize, startAddr;
                if (!int.TryParse(this.InputMemSize, out memSize))
                {
                    Info = "Error: Invalid Memory Size!";
                    return;
                }
                else if (memSize <= 0)
                {
                    Info = "Error: Memory Size should be larger than 0!";
                    return;
                }
                else if (!int.TryParse(this.InputStartAddr, out startAddr))
                {
                    Info = "Error: Invalid Start Address!";
                    return;
                }
                else
                {
                    var fit = Utils.GetFitAlgorithm(FitAlg);
                    if (fit == null)
                    {
                        Info = "Error: Invalid Fit Algorithm!";
                        return;
                    }
                    else
                    {
                        MemoryManager = new(memSize, startAddr, fit);
                        DrawMemory();
                        Info = "Successfully Starts!";
                        IsStart = true;
                    }
                }
            }
        }

        public Command ResetCommand { get; init; }
        public void CReset()
        {
            this.MemoryRectangles.Clear();
            this.MemoryManager = null;
            this.InputMemSize = "";
            this.FitAlg = "";
            this.InputStartAddr = "";
            this.AllocateMemorySize = "";
            this.FreeMemoryStartAddr = "";
            this.log.Reset();
            Info = "Reset Successfully.";
            IsStart = false;
        }

        public Command AllocateCommand { get; init; }
        public void CAllocate()
        {
            if (!IsStart)
            {
                Info = "Error: The demo hasn't started yet.";
                return;
            }
            else
            {
                if (!int.TryParse(this.AllocateMemorySize, out int memSize))
                {
                    Info = "Error: Invalid Allocate Memory Size.";
                    return;
                }
                else if (memSize <= 0)
                {
                    Info = "Error: Allocate Memory Size should be larger than 0!";
                    return;
                }
                else
                {
                    if (MemoryManager is null)
                    {
                        Info = "BUG: MemoryManager is null.";
                        return;
                    }
                    bool res = this.MemoryManager.Allocate(memSize);
                    if (res)
                    {
                        DrawMemory();
                        Info = $"Successfully allocate memory of size {memSize}.";
                        return;
                    }
                    else
                    {
                        Info = $"Failed to allocate memory of size {memSize}.";
                        return;
                    }
                }
            }
        }

        public Command FreeCommand { get; init; }
        public void CFree()
        {
            if (!IsStart)
            {
                Info = "Error: The demo hasn't started yet.";
                return;
            }
            else
            {
                if (!int.TryParse(this.FreeMemoryStartAddr, out int startAddr))
                {
                    Info = "Error: Invalid Free Memory Start Address.";
                    return;
                }
                else
                {
                    if (MemoryManager is null)
                    {
                        Info = "BUG: MemoryManager is null.";
                        return;
                    }
                    bool res = this.MemoryManager.Free(startAddr);
                    if (res)
                    {
                        DrawMemory();
                        Info = $"Successfully free memory at address {startAddr}.";
                        this.FreeMemoryStartAddr = "";
                        return;
                    }
                    else
                    {
                        Info = $"No memory Block at address {startAddr}.";
                        return;
                    }
                }
            }
        }

        public ObservableCollection<FrameworkElement> MemoryRectangles { get; set; } = new();
        public void DrawMemory()
        {
            if (MemoryManager is null)
            {
                Info = "BUG: MemoryManager is null.";
                return;
            }
            if (Parent is null)
            {
                Info = "BUG: Parent is null.";
                return;
            }
            var canvas = Parent!.MemoryCanvas;
            double scale = 10;
            MemoryRectangles.Clear();
            foreach (var allocBlock in this.MemoryManager.AllocateList)
            {
                Rectangle mRectangle = new();
                mRectangle.Width = canvas.ActualWidth;
                mRectangle.Height = allocBlock.MemorySize / (double)(this.MemoryManager.MemorySize) * canvas.ActualHeight;
                mRectangle.SetValue(Canvas.TopProperty, (allocBlock.StartAddr - MemoryManager.StartAddr) / (double)MemoryManager.MemorySize * canvas.ActualHeight);
                mRectangle.SetValue(Canvas.LeftProperty, 0.0);
                mRectangle.Fill = new SolidColorBrush(Colors.LightBlue);
                mRectangle.Stroke = new SolidColorBrush(Colors.Green);
                mRectangle.StrokeThickness = 1;
                MemoryRectangles.Add(mRectangle);

                TextBlock teBlock = new();
                teBlock.Width = canvas.ActualWidth / scale;
                teBlock.Height = teBlock.Width;
                teBlock.SetValue(Canvas.TopProperty, (allocBlock.StartAddr + allocBlock.MemorySize / 2.0 - MemoryManager.StartAddr) / (double)MemoryManager.MemorySize * canvas.ActualHeight - teBlock.Height / 4.0);
                teBlock.SetValue(Canvas.LeftProperty, (canvas.ActualWidth - teBlock.Width) / 2.0);
                teBlock.Text = Convert.ToString(allocBlock.MemorySize);
                MemoryRectangles.Add(teBlock);

                if (allocBlock.StartAddr != MemoryManager.StartAddr)
                {
                    TextBlock texBlock = new();
                    texBlock.Width = canvas.ActualWidth / scale;
                    texBlock.Height = texBlock.Width;
                    texBlock.SetValue(Canvas.TopProperty, (allocBlock.StartAddr - MemoryManager.StartAddr) / (double)MemoryManager.MemorySize * canvas.ActualHeight - texBlock.Height / 4.0);
                    texBlock.SetValue(Canvas.LeftProperty, -texBlock.Width / 2.0);
                    texBlock.Text = Convert.ToString(allocBlock.StartAddr);
                    MemoryRectangles.Add(texBlock);
                }
            }
            TextBlock tBlock = new();
            tBlock.Width = canvas.ActualWidth / scale;
            tBlock.Height = tBlock.Width;
            tBlock.SetValue(Canvas.TopProperty, -tBlock.Height / 4.0);
            tBlock.SetValue(Canvas.LeftProperty, -tBlock.Width / 2.0);
            tBlock.Text = Convert.ToString(MemoryManager.StartAddr);
            MemoryRectangles.Add(tBlock);

            tBlock = new();
            tBlock.Width = canvas.ActualWidth / scale;
            tBlock.Height = tBlock.Width;
            tBlock.SetValue(Canvas.TopProperty, canvas.ActualHeight - tBlock.Height / 4.0);
            tBlock.SetValue(Canvas.LeftProperty, -tBlock.Width / 2.0);
            tBlock.Text = Convert.ToString(MemoryManager.StartAddr + MemoryManager.MemorySize);
            MemoryRectangles.Add(tBlock);

            RaisePropertyChanged(nameof(MemoryRectangles));
        }

        public void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsStart)
            {
                DrawMemory();
            }
        }

        public MainWindowViewModel()
        {
            this.StartCommand = new Command(CStart);
            this.ResetCommand = new Command(CReset);
            this.AllocateCommand = new Command(CAllocate);
            this.FreeCommand = new Command(CFree);
            this.log = new Log(12);
        }
    }
}
