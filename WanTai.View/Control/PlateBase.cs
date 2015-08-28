using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Timers;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;

namespace WanTai.View.Control
{
    public abstract class PlateBase : UserControl
    {
        private Timer timer= new Timer();
        private bool isShining = false;
    
        public PlateBase()
        {
            timer.Enabled = false;
            timer.Interval = 1000;
            timer.Elapsed += AutoShining;
        }
        #region Property        
        private List<object> relatedControls;
        public List<object> RelatedControls
        {
            get { return relatedControls; }
            set { relatedControls = value; }
        }

        public abstract Brush Color { get; set; }

        private bool correct;
        public bool Correct
        {
            get { return correct; }
            set { correct = value; }
        }

        private double needVolume;
        public double NeedVolume
        {
            get { return needVolume; }
            set { needVolume = value; }
        }

        private double actualSavedVolume;
        public double ActualSavedVolume
        {
            get { return actualSavedVolume; }
            set { actualSavedVolume = value; }
        }

        private double firstAddVolume;
        public double FirstAddVolume
        {
            get { return firstAddVolume; }
            set { firstAddVolume = value; }
        }

        public double CurrentVolume{ get; set; }
        public double CurrentActualVolume { get; set; }
        public double AddVolume{ get; set; }
        public bool IsCorrectPosition { get; set; }

        public abstract string DisplayName
        {
            get;
            set;
        }

        public string ChineseName { get; set; }
        public string EnglishName { get; set; }

        public string ContainerName { get; set; }

        public int Grid { get; set; }

        public int Position { get; set; }

        public int CarrierGrid { get; set; }

        public string BarcodePrefix { get; set; }

        public string Barcode { get; set; }

        public abstract short ItemType { get; set; }

        public  Guid ConfigurationItemID { get; set; }
        #endregion

        #region Public Methord

        public void scan()
        {
            if (ChineseName != null && !correct)
            {
                Shining();
            }
        }

        private bool TimerDisposed;
        public void ShiningPause()
        {
            if (timer != null)
            {
                timer.Stop();
            }

            UpdateControls(Brushes.DarkOrange);
            this.BorderBrush = Brushes.DarkOrange;
        }

        public void ShiningRestart()
        {
            if (timer != null)
            {
                timer.Start();
            }
            UpdateControls(Brushes.Red);
            this.BorderBrush = Brushes.Red;
        }

        public void ShiningStop()
        {
            if (timer != null)
            {
                timer.Stop();
                //timer.Dispose();
                //timer = null;
            }
            isShining = false;
            UpdateControls(Brushes.Black);
            this.BorderBrush = Brushes.Gray;
        }

        public void ShiningStop(object sender, EventArgs e)
        {
            this.ShiningStop();
        }

        public void ShiningStop(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.ShiningStop();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="code">1 位置错误；2 类型错误；3 量不足</param>
        /// <returns></returns>
        public abstract void Check(int code);
        #endregion

        #region Private Methord

        private void Shining()
        {            
            timer.Enabled = true;
            timer.Start();
            isShining = true;
            this.BorderBrush = Brushes.Red;
            flag = true;
            if (RelatedControls != null)
            {
                foreach (object tb in RelatedControls)
                {
                    if (tb is TextBlock)
                        ((TextBlock)tb).Foreground = Brushes.Red;
                    if (tb is TextBox)
                        ((TextBox)tb).Foreground = Brushes.Red;
                }
            }
        }

        public void ShiningWithGreen()
        {
            timer.Enabled = false;
            timer.Elapsed -= AutoShining;
            timer.Elapsed += AutoShiningWithGreen;
            timer.Enabled = true;
            timer.Start();
            isShining = true;
            //this.Color = new SolidColorBrush(Colors.Green);
            //this.Background = new SolidColorBrush(Colors.Green);
            this.BorderBrush = Brushes.LawnGreen;
            flag = true;
        }

        private bool flag = true;
        private void AutoShining(object sender, ElapsedEventArgs e)
        {
            if (flag)
            {
                //UpdateControlsInThread(Brushes.Red);
                if (!this.Dispatcher.CheckAccess())
                {
                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(delegate()
                    {
                        this.Foreground = Brushes.Gray;
                        this.BorderBrush = Brushes.Red;
                        // 修改警告颜色 js
                        ((Plate)this).GridPlate.Background = Brushes.Red;
                        ((Plate)this).txtPlateName.Background = Brushes.Red;
                        if (RelatedControls != null)
                        {
                            foreach (object tb in RelatedControls)
                            {
                                if (tb is TextBlock)
                                    ((TextBlock)tb).Foreground = Brushes.Red;
                                if (tb is TextBox)
                                    ((TextBox)tb).Foreground = Brushes.Red;
                            }
                        }
                    }));
                }
            }
            else
            {
                //UpdateControlsInThread(Brushes.Black);
                if (!this.Dispatcher.CheckAccess())
                {
                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(delegate()
                    {
                        this.Foreground = Brushes.Gray;
                        this.BorderBrush = Brushes.White;
                        // 修改警告颜色 js
                        ((Plate)this).GridPlate.Background = Brushes.White;
                        ((Plate)this).txtPlateName.Background = Brushes.White;
                        ((Plate)this).txtPlateName.FontWeight = FontWeights.Bold;
                        if (RelatedControls != null)
                        {
                            foreach (object tb in RelatedControls)
                            {
                                if (tb is TextBlock)
                                    ((TextBlock)tb).Foreground = Brushes.Black;
                                if (tb is TextBox)
                                    ((TextBox)tb).Foreground = Brushes.Black;
                            }
                        }
                    }));
                }
            }
            flag = !flag;
        }

        private void AutoShiningWithGreen(object sender, ElapsedEventArgs e)
        {
            if (flag)
            {
                //UpdateControlsInThread(Brushes.Red);
                if (!this.Dispatcher.CheckAccess())
                {
                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(delegate()
                        {
                            //this.BorderBrush = Brushes.LawnGreen;        
                            // 修改警告颜色 js
                            ((Plate)this).GridPlate.Background = Brushes.LawnGreen;
                            ((Plate)this).txtPlateName.Background = Brushes.LawnGreen;
                        }));
                }
            }
            else
            {
                //UpdateControlsInThread(Brushes.Black);
                if (!this.Dispatcher.CheckAccess())
                {
                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(delegate()
                        {
                            //this.BorderBrush = Brushes.Gray;  
                            ((Plate)this).GridPlate.Background = Brushes.White;
                            ((Plate)this).txtPlateName.Background = Brushes.White;
                        }));
                }
            }
            flag = !flag;
        }

        private void UpdateControlsInThread(Brush color)
        {
            if (RelatedControls == null) return;
            foreach (System.Windows.Controls.Control c in RelatedControls)
            {
                if (!c.Dispatcher.CheckAccess())
                {
                    c.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(delegate { c.Foreground = color; }));
                }
            }
        }

        private void UpdateControls(Brush color)
        {
            if (RelatedControls == null) return;
            foreach (object c in RelatedControls)
            {
                if (c is TextBlock)
                    ((TextBlock)c).Foreground = color;
                if (c is TextBox)
                    ((TextBox)c).Foreground = color;
            }
        }


        protected void RectangleTest_MouseUp(object sender, MouseButtonEventArgs e)
        {
            UpdateControls(Brushes.DarkOrange);
            this.BorderBrush = Brushes.DarkOrange;
        }

        protected void RectangleTest_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isShining)
            {
                UpdateControls(Brushes.Red);
                this.BorderBrush = Brushes.Red;
            }
            else
            {
                UpdateControls(Brushes.Black);
                this.BorderBrush = Brushes.Gray;
            }
        }
        #endregion
    }
}
