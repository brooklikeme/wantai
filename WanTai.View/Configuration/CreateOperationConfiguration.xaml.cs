using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using WanTai.DataModel;
using WanTai.Controller.Configuration;

namespace WanTai.View.Configuration
{
    /// <summary>
    /// Interaction logic for CreateOperationConfiguration.xaml
    /// </summary>
    public partial class CreateOperationConfiguration : Window
    {
        public static string[] OperationTypeName = {"单一操作", "组合操作"};
        OperationConfigurationController controller = new OperationConfigurationController();
        string editOperationId;
        string editOperation_ScriptFileName;
        string scriptPath = System.IO.Path.GetFullPath(WanTai.Common.Configuration.GetEvoScriptFileLocation());

        public CreateOperationConfiguration()
        {
            InitializeComponent();
        }

        public void SetEditedOperationId(string operationId)
        {
            editOperationId = operationId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach(string name in OperationTypeName)
            {
                this.type_comboBox.Items.Add(name);
            }

            InitField();
        }

        private void InitField()
        {
            if (!string.IsNullOrEmpty(editOperationId))
            {
                OperationConfiguration item = controller.GetOperation(new Guid(editOperationId));
                
                this.name_textBox.Text = item.OperationName;
                this.type_comboBox.SelectedIndex = item.OperationType;
                string scriptName = item.ScriptFileName;
                if (!string.IsNullOrEmpty(scriptName))
                {
                    string[] scripts =  scriptName.Split(',');
                    foreach(string script in scripts)
                    {
                        files_listBox.Items.Add(script);
                    }
                }

                this.editOperation_ScriptFileName = item.ScriptFileName;
                this.sequence_textBox.Text = item.OperationSequence.ToString();
                this.display_checkBox.IsChecked = item.DisplayFlag;
                this.runTime_textBox.Text = item.RunTime.ToString();
                this.startFileName_textBox.Text = item.StartOperationFileName;
                this.endFileName_textBox.Text = item.EndOperationFileName;

                if (type_comboBox.SelectedIndex == (int)OperationType.Grouping)
                {
                    operationList_stackPanel.Children.Clear();
                    List<OperationConfiguration> operationList = controller.GetAllOperations();
                    foreach (OperationConfiguration operation in operationList)
                    {
                        CheckBox operationCheckbox = new CheckBox() { Content = operation.OperationName, DataContext = operation, Margin = new Thickness(10, 10, 0, 0) };
                        if (item.SubOperationIDs.Contains(operation.OperationID.ToString()))
                        {
                            operationCheckbox.IsChecked = true;
                        }

                        operationCheckbox.Click += new RoutedEventHandler(operationCheckbox_Click);
                        operationList_stackPanel.Children.Add(operationCheckbox);
                    }
                }
            }
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            if (!validate())
            {
                return;
            }

            OperationConfiguration config = new OperationConfiguration();            
            config.OperationName = name_textBox.Text;            
            config.OperationType = (short)((OperationType)type_comboBox.SelectedIndex);
            config.OperationSequence = short.Parse(sequence_textBox.Text);
            config.DisplayFlag = display_checkBox.IsChecked;
            if (!string.IsNullOrEmpty(runTime_textBox.Text))
            {
                config.RunTime = int.Parse(runTime_textBox.Text);
            }

            if (files_listBox.Items.Count > 0)
            {
                string allscripts = string.Empty;
                foreach (string script in files_listBox.Items)
                {
                    if (!string.IsNullOrEmpty(editOperationId) && editOperation_ScriptFileName.Contains(script))
                    {
                        allscripts = allscripts + "," + script;
                    }
                    else
                    {
                        string fileName = System.IO.Path.GetFileName(script);

                        if (!System.IO.File.Exists(scriptPath + fileName))
                        {
                            System.IO.File.Copy(script, scriptPath + fileName);
                        }
                        else
                        {
                            fileName = System.IO.Path.GetFileNameWithoutExtension(fileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + System.IO.Path.GetExtension(fileName);
                            System.IO.File.Copy(script, scriptPath + fileName);
                        }

                        allscripts = allscripts + "," + fileName;
                    }
                }
                if (allscripts.StartsWith(","))
                    allscripts = allscripts.Substring(1);

                config.ScriptFileName = allscripts;
            }

            if (type_comboBox.SelectedIndex == (int)OperationType.Grouping)
            {
                string subOperationIds = string.Empty;
                foreach (UIElement element in operationList_stackPanel.Children)
                {
                    if (element is CheckBox && (bool)((CheckBox)element).IsChecked)
                    {
                        OperationConfiguration operation = (OperationConfiguration)((CheckBox)element).DataContext;
                        if (string.IsNullOrEmpty(subOperationIds))
                        {
                            subOperationIds = operation.OperationID.ToString();
                        }
                        else
                        {
                            subOperationIds = subOperationIds + "," + operation.OperationID.ToString();
                        }
                    }
                }

                config.SubOperationIDs = subOperationIds;
            }
            else
            {
                config.StartOperationFileName = startFileName_textBox.Text;
                config.EndOperationFileName = endFileName_textBox.Text;
            }

            bool result = false;
            if (string.IsNullOrEmpty(editOperationId))
            {
                config.OperationID = WanTaiObjectService.NewSequentialGuid();
                config.ActiveStatus = true;
                result = controller.Create(config);
                WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "新建操作：" + config.OperationName + " " +(result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);
            
            }
            else
            {
                result = controller.EditOperation(new Guid(editOperationId), config);
                WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "修改操作：" + config.OperationName + " " +(result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);
            }

            
            if (result)
            {
                MessageBox.Show("保存成功", "系统提示");
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("保存失败", "系统提示");
            }
            this.Close();
        }

        private bool validate()
        {
            if (string.IsNullOrEmpty(name_textBox.Text))
            {
                MessageBox.Show("请输入名称", "系统提示");
                name_textBox.Focus();
                return false;
            }

            if (type_comboBox.SelectedIndex < 0)
            {
                MessageBox.Show("请选择类型", "系统提示");
                type_comboBox.Focus();
                return false;
            }
            
            if (type_comboBox.SelectedIndex == (int)OperationType.Grouping)
            {
                bool hasSelected = false;
                foreach (UIElement element in operationList_stackPanel.Children)
                {
                    if (element is CheckBox && (bool)((CheckBox)element).IsChecked)
                    {
                        hasSelected = true;
                        break;
                    }
                }

                if (!hasSelected)
                {
                    MessageBox.Show("请选择子操作", "系统提示");
                    return false;
                }
            } 

            //if (type_comboBox.SelectedIndex == (int)OperationType.Single && files_listBox.Items.Count==0)
            //{
            //    MessageBox.Show("请选择脚本文件上传", "系统提示");
            //    files_listBox.Focus();
            //    return false;
            //}

            if (files_listBox.Items.Count > 0)
            {
                foreach (string script in files_listBox.Items)
                {
                    if (!System.IO.Path.GetExtension(script).Equals(".esc", StringComparison.CurrentCultureIgnoreCase))
                    {
                        MessageBox.Show("请选择正确的脚本文件上传", "系统提示");
                        files_listBox.Focus();
                        return false;
                    }
                }
                
            }

            int outnum = 0;
            if (string.IsNullOrEmpty(sequence_textBox.Text) || !int.TryParse(sequence_textBox.Text, out outnum))
            {
                MessageBox.Show("请填写正确的序号，必须为数字", "系统提示");
                sequence_textBox.Focus();
                return false;
            }

            if (!string.IsNullOrEmpty(runTime_textBox.Text) && !int.TryParse(runTime_textBox.Text, out outnum))
            {
                MessageBox.Show("请填写正确的预计运行时间，必须为数字", "系统提示");
                runTime_textBox.Focus();
                return false;
            }

            //if (!controller.IsOperationSequenceNotExist(int.Parse(sequence_textBox.Text), type_comboBox.SelectedIndex, (editOperationId ==null ? Guid.Empty:new Guid(editOperationId))))
            //{
            //    MessageBox.Show("序号已经被其他操作使用，请输入其他的数字");
            //    sequence_textBox.Focus();
            //    return false;
            //}

            return true;
        }

        private void selectFile_button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Filter = "EVO Script files|*.esc";
            fileDialog.Multiselect = true;
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] fileNames = fileDialog.FileNames;
                if (fileNames != null && fileNames.Length > 0)
                {
                    foreach (string fileName in fileNames)
                    {
                        files_listBox.Items.Add(fileName);
                    }
                }                          
            }
        }

        private void type_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            if (type_comboBox.SelectedIndex == (int)OperationType.Grouping)
            {
                this.startFileName_textBox.Visibility = System.Windows.Visibility.Hidden;
                this.endFileName_textBox.Visibility = System.Windows.Visibility.Hidden;

                List<OperationConfiguration> operationList = controller.GetAllOperations();
                foreach (OperationConfiguration operation in operationList)
                {
                    CheckBox operationCheckbox = new CheckBox() { Content = operation.OperationName, DataContext = operation, Margin = new Thickness(10, 10, 0, 0) };
                    operationCheckbox.Click += new RoutedEventHandler(operationCheckbox_Click);
                    operationList_stackPanel.Children.Add(operationCheckbox);

                }
            }
            else if (type_comboBox.SelectedIndex == (int)OperationType.Single)
            {
                operationList_stackPanel.Children.Clear();
                this.startFileName_textBox.Visibility = System.Windows.Visibility.Visible;
                this.endFileName_textBox.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void operationCheckbox_Click(object sender, RoutedEventArgs e)
        {
            string name = string.Empty;
            int runTime = 0;
            foreach (UIElement element in operationList_stackPanel.Children)
            {
                if (element is CheckBox && (bool)((CheckBox)element).IsChecked)
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        name = ((CheckBox)element).Content.ToString();
                        if (((OperationConfiguration)((CheckBox)element).DataContext).RunTime != null)
                        {
                            runTime = (int)((OperationConfiguration)((CheckBox)element).DataContext).RunTime;
                        }
                    }
                    else
                    {
                        name = name + "+" + ((CheckBox)element).Content;
                        runTime = runTime + (int)((OperationConfiguration)((CheckBox)element).DataContext).RunTime;
                    }
                }
            }

            this.name_textBox.Text = name;
            this.runTime_textBox.Text = runTime.ToString();
        }

        private void deleteFile_button_Click(object sender, RoutedEventArgs e)
        {
            if (files_listBox.SelectedItem != null && !string.IsNullOrEmpty(files_listBox.SelectedItem.ToString()))
            {
                files_listBox.Items.RemoveAt(files_listBox.SelectedIndex);
            }
        }
    }
}
