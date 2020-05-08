Imports CefSharp.WinForms
Imports CefSharp

Friend Class 窗体_浏览器

    Private WithEvents 浏览器 As ChromiumWebBrowser

    Private Delegate Sub 页面加载完毕_跨线程()

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(ByVal 主窗体1 As 主窗体, ByVal 链接 As String)
        InitializeComponent()
        Me.Icon = My.Resources.icon
        If CefSharp.Cef.IsInitialized = False Then
            Dim 设置 As New CefSettings
            设置.Locale = "zh-CN"
            设置.AcceptLanguageList = "zh-CN,en-US,en"
            CefSharp.Cef.Initialize(设置)
        End If
        CefSharpSettings.LegacyJavascriptBindingEnabled = True
        浏览器 = New ChromiumWebBrowser(链接) With {
            .Dock = DockStyle.Fill
        }
        浏览器.LifeSpanHandler = New LifeSpanHandler(主窗体1)
        浏览器.DownloadHandler = New DownloadHandler(主窗体1)
        浏览器的容器.Controls.Add(浏览器)
    End Sub

    Friend Sub 打开链接(ByVal 链接 As String)
        浏览器.Load(链接)
    End Sub

    Private Sub 浏览器_FrameLoadEnd(sender As Object, e As FrameLoadEndEventArgs) Handles 浏览器.FrameLoadEnd
        If e.Frame.IsMain Then 页面加载完毕()
    End Sub

    Private Sub 页面加载完毕()
        If InvokeRequired Then
            Dim d As New 页面加载完毕_跨线程(AddressOf 页面加载完毕)
            Invoke(d, New Object() {})
        Else
            Me.Text = 浏览器.Address
        End If
    End Sub

End Class