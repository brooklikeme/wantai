﻿#pragma checksum "..\..\..\..\HistoryQuery\ExperimentsViewDetail.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "D3EDAAE37CDEED26228CED0A67F299C8"
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


namespace WanTai.View.HistoryQuery {
    
    
    /// <summary>
    /// ExperimentsViewDetail
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class ExperimentsViewDetail : System.Windows.Window, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 24 "..\..\..\..\HistoryQuery\ExperimentsViewDetail.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RowDefinition Grid3;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\..\HistoryQuery\ExperimentsViewDetail.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label label1;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\..\..\HistoryQuery\ExperimentsViewDetail.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label ExperimentName_label;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\..\..\HistoryQuery\ExperimentsViewDetail.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnImportPCRResult;
        
        #line default
        #line hidden
        
        
        #line 39 "..\..\..\..\HistoryQuery\ExperimentsViewDetail.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnReagent;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\..\..\HistoryQuery\ExperimentsViewDetail.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid dataGrid_view;
        
        #line default
        #line hidden
        
        
        #line 92 "..\..\..\..\HistoryQuery\ExperimentsViewDetail.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel stackPanel1;
        
        #line default
        #line hidden
        
        
        #line 93 "..\..\..\..\HistoryQuery\ExperimentsViewDetail.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button close_button;
        
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
            System.Uri resourceLocater = new System.Uri("/WanTag;component/historyquery/experimentsviewdetail.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\HistoryQuery\ExperimentsViewDetail.xaml"
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
            
            #line 5 "..\..\..\..\HistoryQuery\ExperimentsViewDetail.xaml"
            ((WanTai.View.HistoryQuery.ExperimentsViewDetail)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.Grid3 = ((System.Windows.Controls.RowDefinition)(target));
            return;
            case 3:
            this.label1 = ((System.Windows.Controls.Label)(target));
            return;
            case 4:
            this.ExperimentName_label = ((System.Windows.Controls.Label)(target));
            return;
            case 5:
            this.btnImportPCRResult = ((System.Windows.Controls.Button)(target));
            
            #line 31 "..\..\..\..\HistoryQuery\ExperimentsViewDetail.xaml"
            this.btnImportPCRResult.Click += new System.Windows.RoutedEventHandler(this.btnImportPCRResult_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.btnReagent = ((System.Windows.Controls.Button)(target));
            
            #line 39 "..\..\..\..\HistoryQuery\ExperimentsViewDetail.xaml"
            this.btnReagent.Click += new System.Windows.RoutedEventHandler(this.btnReagent_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.dataGrid_view = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 12:
            this.stackPanel1 = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 13:
            this.close_button = ((System.Windows.Controls.Button)(target));
            
            #line 93 "..\..\..\..\HistoryQuery\ExperimentsViewDetail.xaml"
            this.close_button.Click += new System.Windows.RoutedEventHandler(this.close_button_Click);
            
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
            System.Windows.EventSetter eventSetter;
            switch (connectionId)
            {
            case 8:
            eventSetter = new System.Windows.EventSetter();
            eventSetter.Event = System.Windows.Documents.Hyperlink.ClickEvent;
            
            #line 55 "..\..\..\..\HistoryQuery\ExperimentsViewDetail.xaml"
            eventSetter.Handler = new System.Windows.RoutedEventHandler(this.OnRotationNameClick);
            
            #line default
            #line hidden
            ((System.Windows.Style)(target)).Setters.Add(eventSetter);
            break;
            case 9:
            eventSetter = new System.Windows.EventSetter();
            eventSetter.Event = System.Windows.Documents.Hyperlink.ClickEvent;
            
            #line 64 "..\..\..\..\HistoryQuery\ExperimentsViewDetail.xaml"
            eventSetter.Handler = new System.Windows.RoutedEventHandler(this.OnTubesBatchNameClick);
            
            #line default
            #line hidden
            ((System.Windows.Style)(target)).Setters.Add(eventSetter);
            break;
            case 10:
            eventSetter = new System.Windows.EventSetter();
            eventSetter.Event = System.Windows.Documents.Hyperlink.ClickEvent;
            
            #line 77 "..\..\..\..\HistoryQuery\ExperimentsViewDetail.xaml"
            eventSetter.Handler = new System.Windows.RoutedEventHandler(this.OnLogClick);
            
            #line default
            #line hidden
            ((System.Windows.Style)(target)).Setters.Add(eventSetter);
            break;
            case 11:
            eventSetter = new System.Windows.EventSetter();
            eventSetter.Event = System.Windows.Documents.Hyperlink.ClickEvent;
            
            #line 85 "..\..\..\..\HistoryQuery\ExperimentsViewDetail.xaml"
            eventSetter.Handler = new System.Windows.RoutedEventHandler(this.OnPCRTestResultClick);
            
            #line default
            #line hidden
            ((System.Windows.Style)(target)).Setters.Add(eventSetter);
            break;
            }
        }
    }
}

