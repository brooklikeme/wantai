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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;

using WanTai.Controller;
using WanTai.DataModel;

namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for ConfigRotation.xaml
    /// </summary>
    public partial class ConfigRotation : Page
    {
        ConfigRotationController controller = new ConfigRotationController();
        DataTable dataTable = new DataTable();
        TubesBatch tubesBatch;
        bool isBelongtoCurrentTubeBatch = true;

        public ConfigRotation()
        {
            InitializeComponent();

            dataTable.Columns.Add("Sequence", typeof(short));
            dataTable.Columns.Add("Operation", typeof(OperationConfiguration));
            dataTable.Columns.Add("OperationName", typeof(string));
            dataTable.Columns.Add("TubesBatchName", typeof(string));
            dataTable.Columns.Add("TubesBatchID", typeof(string));
            dataTable.Columns.Add("RotationName", typeof(string));
            dataTable.Columns.Add("deleteIsVisible", typeof(string));
            rotation_dataGrid.ItemsSource = dataTable.DefaultView;
        }

        private void rotation_dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //rotation_dataGrid.Items.Clear();

                errorMessage.Text = "";
                List<RotationInfo> rotationList = controller.GetCurrentRotationInfos(SessionInfo.ExperimentID);
                if (rotationList.Count() > 0)
                {
                    this.create_button.IsEnabled = false;
                    if (SessionInfo.NextButIndex == 1)
                        this.next_button.IsEnabled = true;
                    else
                        this.next_button.IsEnabled = false;
                    this.save_button.IsEnabled = false;
                    operation_viewcolumn.Visibility = System.Windows.Visibility.Visible;
                    operation_template_column.Visibility = System.Windows.Visibility.Hidden;

                    foreach (RotationInfo rotation in rotationList)
                    {
                        System.Data.DataRow dRow = dataTable.NewRow();
                        dRow["Sequence"] = dataTable.Rows.Count + 1;
                        if (rotation.TubesBatchID != null)
                        {
                            dRow["TubesBatchID"] = rotation.TubesBatchID;
                            TubesBatch _tubeBatch = controller.GetTubesBatchByID((Guid)rotation.TubesBatchID);
                            dRow["TubesBatchName"] = _tubeBatch.TubesBatchName;
                        }

                        dRow["OperationName"] = rotation.OperationName;
                        dRow["Operation"] = new OperationConfiguration() { OperationID = rotation.OperationID, OperationName = rotation.OperationName };
                        dRow["RotationName"] = rotation.RotationName;
                        dRow["deleteIsVisible"] = Visibility.Hidden.ToString();
                        dataTable.Rows.Add(dRow);
                    }
                }
                else
                {
                    this.create_button.IsEnabled = true;
                    this.next_button.IsEnabled = false;
                    this.save_button.IsEnabled = true;
                    isBelongtoCurrentTubeBatch = true;
                    operation_viewcolumn.Visibility = System.Windows.Visibility.Hidden;

                    operation_template_column.Visibility = System.Windows.Visibility.Visible;

                    operation_viewcolumn.Visibility = System.Windows.Visibility.Hidden;
                    operation_template_column.Visibility = System.Windows.Visibility.Visible;

                    tubesBatch = controller.GetLastTubesBatch();

                    List<OperationConfiguration> operationList = controller.GetDisplayedOperationConfigurations();
                    //operation_column.ItemsSource = operationList;
                    if (SessionInfo.BatchType == "A")
                    {
                        //results = results.Where(c => c.OperationSequence == 1).ToList<OperationConfiguration>();
                        CreateOperation = operationList[operationList.Count - 1];
                    }
                    else
                    {
                        CreateOperation = operationList[0];
                    }
                }

                if (SessionInfo.BatchType == "A")
                {        
                    create_button_Click(sender, e);
                    //create_button.IsEnabled = false;
                    //rotation_dataGrid.IsEnabled = false;
                }
                else
                {
                    create_button.IsEnabled = true;
                    rotation_dataGrid.IsEnabled = true;
                }
            }
            catch
            {
                MessageBox.Show("系统错误！", "系统提示！");

                //this.errorMessage.Text = "系统错误！";
            }
        }
        private OperationConfiguration CreateOperation { get; set; }
        private void create_button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.errorMessage.Text) && dataTable.Rows.Count > 0)
            {
                MessageBox.Show("必须解决完错误后，才能新建轮次！", "系统提示");
                return;
            }

            if (dataTable.Rows.Count == 5)
            {
                MessageBox.Show("最大只允许新建5个轮次！", "系统提示！");
             //   this.errorMessage.Text = "最大只允许新建5个轮次！";
                return;
            }

            if (dataTable.Rows.Count > 0)
            {
                DataRow lastRow = dataTable.Rows[dataTable.Rows.Count - 1];
                OperationConfiguration lastRowOperation = lastRow["Operation"] as OperationConfiguration;
                if (lastRowOperation == null)
                {
                    MessageBox.Show("请配置完当前轮次，再新建！", "系统提示");
                    return;
                }
                else
                {
                    //if ((int)lastRowOperation.OperationType == (int)OperationType.Grouping)
                    //{
                        isBelongtoCurrentTubeBatch = false;
                    //}
                }
            }

            System.Data.DataRow dRow = dataTable.NewRow();
            dRow["Sequence"] = dataTable.Rows.Count + 1;
            dRow["deleteIsVisible"] = Visibility.Visible.ToString();
            dRow["RotationName"] = "轮次" + dRow["Sequence"].ToString();
         //   operation_column.DisplayIndex = 1;
            dRow["Operation"] = CreateOperation;
            if (isBelongtoCurrentTubeBatch && tubesBatch != null)
            {
                dRow["TubesBatchName"] = tubesBatch.TubesBatchName;
                dRow["TubesBatchID"] = tubesBatch.TubesBatchID;
            }

            dataTable.Rows.Add(dRow);
            next_button.IsEnabled = false;
            save_button.IsEnabled = true;
        }

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            this.errorMessage.Text = "";
            int selectedIndex = rotation_dataGrid.SelectedIndex;
            int deletedSequence = int.Parse(dataTable.Rows[selectedIndex]["Sequence"].ToString());
            dataTable.Rows[selectedIndex].Delete();
            ResetSequenceNumber(deletedSequence);
            if (dataTable.Rows.Count == 0)
                save_button.IsEnabled = false;

            next_button.IsEnabled = false;
        }

        private void ResetSequenceNumber(int deletedSequence)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                int rowSequence = int.Parse(row["Sequence"].ToString());
                if (rowSequence > deletedSequence)
                {
                    row["Sequence"] = rowSequence - 1;
                    row["RotationName"] = "轮次" + row["Sequence"].ToString();
                }
            }
        }

        private void OnHyperlinkClick(object sender, RoutedEventArgs e)
        {
            int selectedIndex = rotation_dataGrid.SelectedIndex;
            string TubesBatchID = dataTable.Rows[selectedIndex]["TubesBatchID"].ToString();
            TubesDetailView detailView = new TubesDetailView();
            detailView.BatchID = new Guid(TubesBatchID);
            detailView.ShowDialog();
        }

        private void CmbOperation_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            OperationConfiguration selectedOperation = comboBox.SelectedItem as OperationConfiguration;
            if (selectedOperation == null)
                return;

            int selectedRowIndex = rotation_dataGrid.SelectedIndex;
            if (selectedRowIndex < 0)
                return;
            if (selectedRowIndex == 0)
            {
                //第一个必须是组合操作或者是混样
                if (selectedOperation != null &&
                    ((int)selectedOperation.OperationType == (int)OperationType.Single && selectedOperation.OperationSequence != 1))
                {
                    MessageBox.Show("第一个轮次的操作必须是混样或者完整操作!", "系统提示！");
                    // this.errorMessage.Text = "第一个轮次的操作必须是混样或者完整操作";
                    return;
                }
            }
            else
            {
                DataRow previous_row = dataTable.Rows[selectedRowIndex - 1];
                OperationConfiguration operation = previous_row["Operation"] as OperationConfiguration;
                if (operation != null && selectedOperation != null)
                {
                    if (string.IsNullOrEmpty(previous_row["TubesBatchName"] as string) || (selectedOperation.OperationSequence == 1 && selectedRowIndex > 0))
                    {
                        dataTable.Rows[selectedRowIndex]["TubesBatchName"] = null;
                        dataTable.Rows[selectedRowIndex]["TubesBatchID"] = null;
                        isBelongtoCurrentTubeBatch = false;
                    }
                    else
                    {
                        if (operation.OperationSequence + 1 == selectedOperation.OperationSequence)
                        {
                            isBelongtoCurrentTubeBatch = true;
                            dataTable.Rows[selectedRowIndex]["TubesBatchName"] = tubesBatch.TubesBatchName;
                            dataTable.Rows[selectedRowIndex]["TubesBatchID"] = tubesBatch.TubesBatchID;
                        }
                        else
                        {
                            isBelongtoCurrentTubeBatch = false;
                            dataTable.Rows[selectedRowIndex]["TubesBatchName"] = null;
                            dataTable.Rows[selectedRowIndex]["TubesBatchID"] = null;
                        }
                    }

                    if (selectedOperation.OperationType != (int)OperationType.Grouping && (int)operation.OperationType == (int)OperationType.Grouping && selectedOperation.OperationSequence != 1)
                    {
                        MessageBox.Show("轮次的操作必须是混样或者完整操作!", "系统提示！");

                        //   this.errorMessage.Text = "轮次的操作必须是混样或者完整操作";
                        return;
                    }
                    else if (selectedOperation.OperationType != (int)OperationType.Grouping && selectedOperation.OperationSequence != 1 && (operation.OperationSequence + 1 != selectedOperation.OperationSequence))
                    {
                        MessageBox.Show("轮次的操作必须按照顺序!", "系统提示！");
                        //  this.errorMessage.Text = "轮次的操作必须按照顺序";
                        return;
                    }
                }
            }

            this.errorMessage.Text = "";
        }

        private void save_button_Click(object sender, RoutedEventArgs e)
        {
            if (!validate())
                return;

            List<RotationInfo> rotationInfoList = new List<RotationInfo>();
            foreach (DataRow row in dataTable.Rows)
            {
                RotationInfo rotationInfo = new RotationInfo();
                rotationInfo.RotationID = WanTaiObjectService.NewSequentialGuid();
                rotationInfo.ExperimentID = SessionInfo.ExperimentID;
                if (row["TubesBatchID"] != null && !string.IsNullOrEmpty(row["TubesBatchID"].ToString()))
                {
                    rotationInfo.TubesBatchID = new Guid(row["TubesBatchID"].ToString());
                }

                if (row["RotationName"] != null && !string.IsNullOrEmpty(row["RotationName"].ToString()))
                {
                    rotationInfo.RotationName = row["RotationName"].ToString();
                }

                rotationInfo.State = (short)RotationInfoStatus.Create;
                rotationInfo.CreateTime = DateTime.Now;

                OperationConfiguration operation = row["Operation"] as OperationConfiguration;
                rotationInfo.OperationID = operation.OperationID;
                rotationInfo.OperationName = operation.OperationName;

                rotationInfo.RotationSequence = (short)row["Sequence"];
                //rotationInfo.BatchType = SessionInfo.BatchType;

                rotationInfoList.Add(rotationInfo);                
            }

            if (controller.Create(rotationInfoList))
            {
                //if (SessionInfo.PraperRotation == null)
                //{
                Guid RotationID = Guid.Empty;
                if (SessionInfo.PraperRotation != null)
                    RotationID = SessionInfo.PraperRotation.RotationID;
                SessionInfo.PraperRotation = rotationInfoList.FirstOrDefault();

                FormulaParameters formulaParameters = SessionInfo.RotationFormulaParameters[RotationID];
                SessionInfo.RotationFormulaParameters.Remove(RotationID);
                if (!SessionInfo.RotationFormulaParameters.ContainsKey(SessionInfo.PraperRotation.RotationID))
                {
                    SessionInfo.RotationFormulaParameters.Add(SessionInfo.PraperRotation.RotationID, formulaParameters);
                }
                else
                {
                    SessionInfo.RotationFormulaParameters[SessionInfo.PraperRotation.RotationID] = formulaParameters;
                }
                // }


              //  MessageBox.Show("保存成功！", "系统提示");
                if (NextStepEvent != null)
                    NextStepEvent(sender, e);

                this.next_button.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("保存失败！", "系统提示");
            }

        }

        private bool validate()
        {
            this.errorMessage.Text = "";

            if (dataTable.Rows.Count == 0)
            {
                MessageBox.Show("请至少配置一个轮次！", "系统提示");
              //  this.errorMessage.Text = "请至少配置一个轮次";
                return false;
            }

            int index = 0;
            int previous_OperationSequence = 1;
            foreach (DataRow row in dataTable.Rows)
            {
                OperationConfiguration operation = row["Operation"] as OperationConfiguration;
                if (operation == null)
                {
                    MessageBox.Show("请选择操作！", "系统提示");
                   // this.errorMessage.Text = "请选择操作";
                    return false;
                }

                if (index == 0)
                {
                    if (((int)operation.OperationType != (int)OperationType.Grouping) && operation.OperationSequence != 1)
                    {
                        MessageBox.Show("轮次的操作必须是混样或者完整操作！", "系统提示");
                       // this.errorMessage.Text = "轮次的操作必须是混样或者完整操作";
                        return false;
                    }
                }
                else if ((int)operation.OperationType == (int)OperationType.Grouping)
                {

                }
                else
                {
                    if (previous_OperationSequence == 0 && operation.OperationSequence != 1)
                    {
                        MessageBox.Show("轮次的操作必须是混样或者完整操作！", "系统提示");
                       // this.errorMessage.Text = "轮次的操作必须是混样或者完整操作";
                        return false;
                    }
                    else if (operation.OperationSequence != 1 && (operation.OperationSequence != previous_OperationSequence + 1))
                    {
                        MessageBox.Show("轮次的操作必须按照顺序！", "系统提示");
                     //   this.errorMessage.Text = "轮次的操作必须按照顺序";
                        return false;
                    }
                }

                if ((int)operation.OperationType == (int)OperationType.Grouping)
                {
                    previous_OperationSequence = 0;
                }
                else
                {
                    previous_OperationSequence = operation.OperationSequence;
                }

                index++;
            }

            return true;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            dataTable.Clear();
        }

        public event MainPage.NextStepHandler NextStepEvent;

        private void next_button_Click(object sender, RoutedEventArgs e)
        {
            if (NextStepEvent != null)
            {
                SessionInfo.NextButIndex = 2;
                NextStepEvent(sender, e);
            }
        }

        private void operation_column_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            comboBox.Items.Refresh();
        }
    }
}