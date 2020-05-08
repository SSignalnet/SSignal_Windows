Imports System.IO
Imports System.Threading
Imports System.Net
Imports CefSharp.WinForms
Imports CefSharp

Public Class 窗体_下载文件

    Private Structure 下载任务_复合数据
        Dim 下载路径, 保存路径 As String
        Dim 任务编号 As Integer
    End Structure

    Friend WithEvents 浏览器 As ChromiumWebBrowser
    Dim 第几个任务, 下载进度 As Integer
    Dim 下载任务() As 下载任务_复合数据
    Dim 显示保存路径 As Boolean
    Dim 线程 As Thread

    Friend 关闭 As Boolean

    Private Delegate Sub 更新下载进度_跨线程(ByVal 进度 As Integer)
    Private Delegate Sub 下载成功或失败_跨线程(ByVal 成功 As Boolean)
    Private Delegate Sub 页面加载完毕_跨线程()

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(ByVal 下载路径1 As String, ByVal 保存路径1 As String, Optional ByVal 显示保存路径1 As Boolean = False)
        InitializeComponent()
        Me.Icon = My.Resources.icon
        第几个任务 = 1
        ReDim 下载任务(0)
        With 下载任务(0)
            .下载路径 = 下载路径1
            .保存路径 = 保存路径1
            .任务编号 = 第几个任务
        End With
        显示保存路径 = True
        Dim 本程序路径 As String = My.Application.Info.DirectoryPath
        If 本程序路径.EndsWith("\") = False Then 本程序路径 &= "\"
        浏览器 = New ChromiumWebBrowser("file://" & 本程序路径.Replace("\", "/") & "Download.html") With {
            .Dock = DockStyle.Fill
        }
        浏览器.MenuHandler = New MenuHandler
        浏览器的容器.Controls.Add(浏览器)
        Dim 绑定设置 As BindingOptions = BindingOptions.DefaultBinder
        绑定设置.CamelCaseJavascriptNames = False
        浏览器.RegisterJsObject("external", New JS接口_下载(Me), 绑定设置)
    End Sub

    Private Sub 窗体_下载文件_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = 界面文字.获取(310, Me.Text)
    End Sub

    Private Sub 浏览器_FrameLoadEnd(sender As Object, e As FrameLoadEndEventArgs) Handles 浏览器.FrameLoadEnd
        If e.Frame.IsMain Then 页面加载完毕()
    End Sub

    Private Sub 页面加载完毕()
        If InvokeRequired Then
            Dim d As New 页面加载完毕_跨线程(AddressOf 页面加载完毕)
            Invoke(d, New Object() {})
        Else
            浏览器.GetMainFrame.ExecuteJavaScriptAsync("MenuText('" & 界面文字.获取(311, "取消") & "');")
            With 下载任务(0)
                浏览器.GetMainFrame.ExecuteJavaScriptAsync("NewTask('" & .任务编号 & "', '" & 替换HTML和JS敏感字符(IIf(显示保存路径 = False, Path.GetFileName(.保存路径), .保存路径)) & "', '" & 替换HTML和JS敏感字符(.下载路径) & "', '" & 界面文字.获取(309, "等待中") & "');")
            End With
            线程 = New Thread(New ParameterizedThreadStart(AddressOf 下载))
            线程.Start(下载任务(0))
        End If
    End Sub

    Friend Sub 新下载任务(ByVal 下载路径 As String, ByVal 保存路径 As String, Optional ByVal 显示保存路径1 As Boolean = False)
        If 下载任务 Is Nothing Then
            第几个任务 += 1
            ReDim 下载任务(0)
            With 下载任务(0)
                .下载路径 = 下载路径
                .保存路径 = 保存路径
                .任务编号 = 第几个任务
                浏览器.GetMainFrame.ExecuteJavaScriptAsync("NewTask('" & .任务编号 & "', '" & 替换HTML和JS敏感字符(IIf(显示保存路径1 = False, Path.GetFileName(.保存路径), .保存路径)) & "', '" & 替换HTML和JS敏感字符(.下载路径) & "', '" & 界面文字.获取(309, "等待中") & "');")
            End With
            线程 = New Thread(New ParameterizedThreadStart(AddressOf 下载))
            线程.Start(下载任务(0))
        Else
            Dim I As Integer
            For I = 0 To 下载任务.Length - 1
                If String.Compare(下载路径, 下载任务(I).下载路径) = 0 Then Return
            Next
            第几个任务 += 1
            ReDim Preserve 下载任务(下载任务.Length)
            With 下载任务(下载任务.Length - 1)
                .下载路径 = 下载路径
                .保存路径 = 保存路径
                .任务编号 = 第几个任务
                浏览器.GetMainFrame.ExecuteJavaScriptAsync("NewTask('" & .任务编号 & "', '" & 替换HTML和JS敏感字符(IIf(显示保存路径1 = False, Path.GetFileName(.保存路径), .保存路径)) & "', '" & 替换HTML和JS敏感字符(.下载路径) & "', '" & 界面文字.获取(309, "等待中") & "');")
            End With
        End If
    End Sub

    Private Sub 下载(ByVal 参数 As Object)
        Dim 当前下载任务 As 下载任务_复合数据 = 参数
        Dim 进度 As Integer
        Dim 重试次数 As Integer
        Dim 总字节数 As Long
        Dim 收到的字节数组() As Byte
        Dim 收到的字节数, 收到的总字节数 As Integer
重试:
        收到的总字节数 = 0
        收到的字节数组 = Nothing
        下载进度 = 0
        更新下载进度(下载进度)
        Try
            Dim HTTP网络请求 As HttpWebRequest = WebRequest.Create(当前下载任务.下载路径)
            HTTP网络请求.Method = "GET"
            HTTP网络请求.Timeout = 10000
            Dim 字节数组(8191) As Byte
            Using HTTP网络回应 As HttpWebResponse = HTTP网络请求.GetResponse
                If HTTP网络回应.ContentLength > 0 Then
                    总字节数 = HTTP网络回应.ContentLength
                    ReDim 收到的字节数组(总字节数 - 1)
                    Dim 输入流 As Stream = HTTP网络回应.GetResponseStream
                    Do
                        收到的字节数 = 输入流.Read(字节数组, 0, 字节数组.Length)
                        If 收到的字节数 > 0 Then
                            Array.Copy(字节数组, 0, 收到的字节数组, 收到的总字节数, 收到的字节数)
                            收到的总字节数 += 收到的字节数
                            进度 = Int((收到的总字节数 / 总字节数) * 100)
                            If 进度 - 下载进度 >= 5 Then
                                下载进度 = 进度
                                更新下载进度(进度)
                            End If
                        Else
                            Exit Do
                        End If
                    Loop
                End If
            End Using
        Catch ex As Exception
            If 重试次数 < 2 Then
                重试次数 += 1
                GoTo 重试
            End If
        End Try
        If 收到的字节数组 IsNot Nothing Then
            If 收到的总字节数 = 收到的字节数组.Length Then
                Try
                    File.WriteAllBytes(当前下载任务.保存路径, 收到的字节数组)
                Catch ex As Exception
                    GoTo 失败
                End Try
                下载成功或失败(True)
                Return
            End If
        End If
失败:
        下载成功或失败(False)
    End Sub

    Private Sub 更新下载进度(ByVal 进度 As Integer)
        If InvokeRequired Then
            Dim d As New 更新下载进度_跨线程(AddressOf 更新下载进度)
            Invoke(d, New Object() {进度})
        Else
            浏览器.GetMainFrame.ExecuteJavaScriptAsync("ProgressChanged('" & 下载任务(0).任务编号 & "', '" & 进度 & "');")
        End If
    End Sub

    Private Sub 下载成功或失败(ByVal 成功 As Boolean)
        If InvokeRequired Then
            Dim d As New 下载成功或失败_跨线程(AddressOf 下载成功或失败)
            Invoke(d, New Object() {成功})
        Else
            If 成功 = True Then
                浏览器.GetMainFrame.ExecuteJavaScriptAsync("Succeeded('" & 下载任务(0).任务编号 & "', '" & 界面文字.获取(307, "完毕") & "');")
            Else
                浏览器.GetMainFrame.ExecuteJavaScriptAsync("Failed('" & 下载任务(0).任务编号 & "', '" & 界面文字.获取(308, "失败") & "');")
            End If
            If 下载任务.Length = 1 Then
                下载任务 = Nothing
            Else
                Dim 下载任务2(下载任务.Length - 2) As 下载任务_复合数据
                Dim I, J As Integer
                For I = 1 To 下载任务.Length - 1
                    下载任务2(J) = 下载任务(I)
                    J += 1
                Next
                下载任务 = 下载任务2
                线程 = New Thread(New ParameterizedThreadStart(AddressOf 下载))
                线程.Start(下载任务(0))
            End If
        End If
    End Sub

    Friend Sub 取消下载(ByVal 任务编号 As Integer)
        If 下载任务 Is Nothing Then Return
        Dim I As Integer
        For I = 0 To 下载任务.Length - 1
            If 下载任务(I).任务编号 = 任务编号 Then Exit For
        Next
        If I < 下载任务.Length Then
            If I = 0 Then
                If 线程 IsNot Nothing Then 线程.Abort()
            End If
            If 下载任务.Length = 1 Then
                下载任务 = Nothing
            Else
                Dim 下载任务2(下载任务.Length - 2) As 下载任务_复合数据
                Dim J, K As Integer
                For J = 0 To 下载任务.Length - 1
                    If J <> I Then
                        下载任务2(K) = 下载任务(J)
                        K += 1
                    End If
                Next
                下载任务 = 下载任务2
                线程 = New Thread(New ParameterizedThreadStart(AddressOf 下载))
                线程.Start(下载任务(0))
            End If
        End If
    End Sub

    Private Sub 窗体_下载文件_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason = CloseReason.UserClosing Then
            If 关闭 = False Then
                If 下载任务 IsNot Nothing Then
                    Me.WindowState = FormWindowState.Minimized
                Else
                    Me.Hide()
                End If
                e.Cancel = True
            End If
        End If
    End Sub

End Class
