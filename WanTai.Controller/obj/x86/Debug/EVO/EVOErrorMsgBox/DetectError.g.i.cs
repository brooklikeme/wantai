﻿#pragma checksum "..\..\..\..\..\EVO\EVOErrorMsgBox\DetectError.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "8B5CCA2CB98078197CD12846851D8E4D"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
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


namespace WanTai.Controller.EVO.EVOErrorMsgBox {
    
    
    /// <summary>
    /// DetectError
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class DetectError : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 23 "..\..\..\..\..\EVO\EVOErrorMsgBox\DetectError.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid dataGrid_view;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\..\..\..\EVO\EVOErrorMsgBox\DetectError.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel stackPanel1;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\..\..\..\EVO\EVOErrorMsgBox\DetectError.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnRetryDetect;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\..\..\..\EVO\EVOErrorMsgBox\DetectError.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnMoveZMax;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\..\..\..\EVO\EVOErrorMsgBox\DetectError.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnPipetNothing;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\..\..\..\EVO\EVOErrorMsgBox\DetectError.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnAspAir;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\..\..\..\EVO\EVOErrorMsgBox\DetectError.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnAbort;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\..\..\..\EVO\EVOErrorMsgBox\DetectError.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnMoveZTravel;
        
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
            System.Uri resourceLocater = new System.Uri("/WanTai.Controller;component/evo/evoerrormsgbox/detecterror.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\EVO\EVOErrorMsgBox\DetectError.xaml"
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
            this.dataGrid_view = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 2:
            this.stackPanel1 = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 3:
            this.btnRetryDetect = ((System.Windows.Controls.Button)(target));
            
            #line 31 "..\..\..\..\..\EVO\EVOErrorMsgBox\DetectError.xaml"
            this.btnRetryDetect.Click += new System.Windows.RoutedEventHandler(this.btnRetryDetect_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.btnMoveZMax = ((System.Windows.Controls.Button)(target));
            
            #line 32 "..\..\..\..\..\EVO\EVOErrorMsgBox\DetectError.xaml"
            this.btnMoveZMax.Click += new System.Windows.RoutedEventHandler(this.btnMoveZMax_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.btnPipetNothing = ((System.Windows.Controls.Button)(target));
            
            #line 33 "..\..\..\..\..\EVO\EVOErrorMsgBox\DetectError.xaml"
            this.btnPipetNothing.Click += new System.Windows.RoutedEventHandler(this.btnPipetNothing_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.btnAspAir = ((System.Windows.Controls.Button)(target));
            
            #line 34 "..\..\..\..\..\EVO\EVOErrorMsgBox\DetectError.xaml"
            this.btnAspAir.Click += new System.Windows.RoutedEventHandler(this.btnAspAir_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.btnAbort = ((System.Windows.Controls.Button)(target));
            
            #line 35 "..\..\..\..\..\EVO\EVOErrorMsgBox\DetectError.xaml"
            this.btnAbort.Click += new System.Windows.RoutedEventHandler(this.btnAbort_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.btnMoveZTravel = ((System.Windows.Controls.Button)(target));
            
            #line 36 "..\..\..\..\..\EVO\EVOErrorMsgBox\DetectError.xaml"
            this.btnMoveZTravel.Click += new System.Windows.RoutedEventHandler(this.btnMoveZTravel_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

