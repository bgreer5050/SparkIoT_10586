﻿#pragma checksum "C:\Users\admin\Downloads\ZumbachPi-master\ZumbachPi-master\ZumbachPi_V2.0\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "7BAF1397E1F32053EE6592BC2D2DCECA"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ZumbachPi_V2._0
{
    partial class MainPage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                {
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)(target);
                    #line 8 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Page)element1).SizeChanged += this.Page_SizeChanged;
                    #line default
                }
                break;
            case 2:
                {
                    this.grid = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 3:
                {
                    this.slider = (global::Windows.UI.Xaml.Controls.Slider)(target);
                    #line 16 "..\..\..\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Slider)this.slider).ValueChanged += this.Slider_ValueChanged;
                    #line default
                }
                break;
            case 4:
                {
                    this.webView2 = (global::Windows.UI.Xaml.Controls.WebView)(target);
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

