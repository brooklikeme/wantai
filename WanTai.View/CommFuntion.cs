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
using System.Data.Entity;
using WanTai.DataModel;
using System.IO;
using System.Windows.Markup;
using System.Data;
using System.Reflection;
using System.Windows.Controls.Primitives;
using System.Text.RegularExpressions;
namespace WanTai.View
{
    public class CommFuntion
    {
        public static string ConvertToChinese(double x)
        {
            string s = x.ToString("#L#E#D#C#K#E#D#C#J#E#D#C#I#E#D#C#H#E#D#C#G#E#D#C#F#E#D#C#.0B0A");
            string d = Regex.Replace(s, @"((?<=-|^)[^1-9]*)|((?'z'0)[0A-E]*((?=[1-9])|(?'-z'(?=[F-L\.]|$))))|((?'b'[F-L])(?'z'0)[0A-L]*((?=[1-9])|(?'-z'(?=[\.]|$))))", "${b}${z}");
            return Regex.Replace(d, ".", delegate(Match m) { return "负个空零壹贰叁肆伍陆柒捌玖空空空空空空空分角拾佰仟万亿兆京垓秭穰"[m.Value[0] - '-'].ToString(); });
        }


        public static int GetDataGridCellRowIndex(DataGridCellInfo Cell)
        {
            object DataRow = ((System.Data.DataRowView)(Cell.Item)).Row;
            object RowID = DataRow.GetType().GetProperty("rowID", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(DataRow, null);
            if (RowID != null)
                return int.Parse(RowID.ToString());
            return 0;
        }
        public static int GetDataGridCellColumnIndex(DataGridCellInfo Cell)
        {
            return Cell.Column.DisplayIndex+1;
        }

        public static object FindName(DataGrid myDataGrid, int ColumnIndex, int RowIndex, string ControlName)
        {
            FrameworkElement item = myDataGrid.Columns[ColumnIndex].GetCellContent(myDataGrid.Items[RowIndex]);
            DataGridTemplateColumn temple = (myDataGrid.Columns[ColumnIndex] as DataGridTemplateColumn);
            return temple.CellTemplate.FindName(ControlName, item);
        }
        /// <summary>
        /// MessageBox.Show(FindCellControl<TextBlock>("txtContent").Text);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T FindCellControl<T>(string name, DataGrid dataGrid, int RowIndex,int ColumnIndex) where T : Visual
        {
            //DataRowView selectItem = dataGrid.SelectedItem as DataRowView;
            //DataGridCell cell = GetCell(dataGrid, dataGrid.SelectedIndex, 0);
            DataRowView selectItem = dataGrid.Items[RowIndex] as DataRowView;
            DataGridCell cell = GetCell(dataGrid, RowIndex, ColumnIndex);

            return FindVisualChildByName<T>(cell, name) as T;
        }

        public static T FindVisualChildByName<T>(Visual parent, string name) where T : Visual
        {
            if (parent != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i) as Visual;
                    string controlName = child.GetValue(FrameworkElement.NameProperty) as string;
                    if (controlName == name)
                    {
                        return child as T;
                    }
                    else
                    {
                        T result = FindVisualChildByName<T>(child, name);
                        if (result != null)
                            return result;
                    }
                }
            }
            return null;
        }
        public static DataGridCell GetCell(DataGrid datagrid, int rowIndex, int columnIndex)
        {
            DataGridRow rowContainer = GetRow(datagrid, rowIndex);

            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                if (cell == null)
                {
                    datagrid.ScrollIntoView(rowContainer, datagrid.Columns[columnIndex]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                }
                return cell;
            }
            return null;
        }

        public static DataGridRow GetRow(DataGrid datagrid, int columnIndex)
        {
            DataGridRow row = (DataGridRow)datagrid.ItemContainerGenerator.ContainerFromIndex(columnIndex);
            if (row == null)
            {
                datagrid.UpdateLayout();
                datagrid.ScrollIntoView(datagrid.Items[columnIndex]);
                row = (DataGridRow)datagrid.ItemContainerGenerator.ContainerFromIndex(columnIndex);
            }
            return row;
        }

        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
    }
}
