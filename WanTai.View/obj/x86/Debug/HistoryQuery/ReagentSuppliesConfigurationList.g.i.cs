﻿#pragma checksum "..\..\..\..\HistoryQuery\ReagentSuppliesConfigurationList.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "AB7A0151DD6EE8BBD0C6F599EF69F3CC"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Windows.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace WanTai.View.Configuration {
    
    
    /// <summary>
    /// ReagentSuppliesConfigurationList
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class ReagentSuppliesConfigurationList : System.Windows.Window, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 14 "..\..\..\..\HistoryQuery\ReagentSuppliesConfigurationList.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RowDefinition Grid3;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\..\HistoryQuery\ReagentSuppliesConfigurationList.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid dataGrid_view;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\..\..\HistoryQuery\ReagentSuppliesConfigurationList.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel stackPanel1;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\..\..\HistoryQuery\ReagentSuppliesConfigurationList.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button new_button;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\..\..\HistoryQuery\ReagentSuppliesConfigurationList.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button cancel;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/WanTai.View;component/historyquery/reagentsuppliesconfigurationlist.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\HistoryQuery\ReagentSuppliesConfigurationList.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 4 "..\..\..\..\HistoryQuery\ReagentSuppliesConfigurationList.xaml"
            ((WanTai.View.Configuration.ReagentSuppliesConfigurationList)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.Grid3 = ((System.Windows.Controls.RowDefinition)(target));
            return;
            case 3:
            this.dataGrid_view = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 6:
            this.stackPanel1 = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 7:
            this.new_button = ((System.Windows.Controls.Button)(target));
            
            #line 47 "..\..\..\..\HistoryQuery\ReagentSuppliesConfigurationList.xaml"
            this.new_button.Click += new System.Windows.RoutedEventHandler(this.new_button_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.cancel = ((System.Windows.Controls.Button)(target));
            
            #line 48 "..\..\..\..\HistoryQuery\ReagentSuppliesConfigurationList.xaml"
            this.cancel.Click += new System.Windows.RoutedEventHandler(this.cancel_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 4:
            
            #line 38 "..\..\..\..\HistoryQuery\ReagentSuppliesConfigurationList.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.edit_button_Click);
            
            #line default
            #line hidden
            break;
            case 5:
            
            #line 39 "..\..\..\..\HistoryQuery\ReagentSuppliesConfigurationList.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Button_Click);
            
            #line default
            #line hidden
            break;
            }
        }
    }
}

