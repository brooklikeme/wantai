using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Windows;
namespace WanTai.DataModel
{
    public partial class OperationConfiguration
    {
        private string _IsExistsRunScript = string.Empty;
        public string IsExistsRunScript        { get; set; }
        public int StyleIndex { get; set; }
    }
}
