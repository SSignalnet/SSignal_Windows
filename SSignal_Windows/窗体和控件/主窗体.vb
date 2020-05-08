Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Net
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports CefSharp.WinForms
Imports CefSharp

Public Class 主窗体

    Private Structure 最近讯友_复合数据
        Dim 英语讯宝地址 As String
        Dim 群编号 As Long
        Dim 新讯宝数量 As Short
    End Structure

    Private WithEvents 浏览器_讯友录 As ChromiumWebBrowser

    Friend 聊天控件() As 控件_聊天
    Friend 聊天控件数 As Integer
    Dim 当前聊天控件 As 控件_聊天
    Dim 主控机器人 As 类_机器人_主控
    Dim 系统管理机器人 As 类_机器人_系统管理
    Friend 正在发送的讯宝 As 类_要发送的讯宝
    Dim 线程_发送讯宝 As Thread
    Friend 关闭, 保存凭据 As Boolean
    Dim 当前讯友录() As Object
    Friend WithEvents 录音类 As 类_音频_录制
    Friend WithEvents 播音类 As 类_音频_播放
    Friend 当前录音控件, 当前播音控件 As 控件_聊天
    Friend 下载文件的窗体 As 窗体_下载文件
    Private WithEvents 浏览器窗体 As 窗体_浏览器

    Private Delegate Sub 页面加载完毕_跨线程()
    Private Delegate Sub 发送完毕_跨线程(ByVal 继续 As Boolean)
    Private Delegate Sub 等待确认_跨线程()

    Public Sub New()
        InitializeComponent()
        Me.Icon = My.Resources.icon
        If My.Settings.WindowLeft < 0 Then My.Settings.WindowLeft = 0
        If My.Settings.WindowTop < 0 Then My.Settings.WindowTop = 0
        Dim 屏幕高宽 As Size = My.Computer.Screen.WorkingArea.Size
        If My.Settings.WindowLeft + Me.Width <= 屏幕高宽.Width Then
            If My.Settings.WindowTop + Me.Height <= 屏幕高宽.Height Then
                Me.Location = New Point(My.Settings.WindowLeft, My.Settings.WindowTop)
            Else
                Me.Location = New Point(My.Settings.WindowLeft, 屏幕高宽.Height - Me.Height)
            End If
        ElseIf My.Settings.WindowTop + Me.Height <= 屏幕高宽.Height Then
            Me.Location = New Point(屏幕高宽.Width - Me.Width, My.Settings.WindowTop)
        Else
            Me.Location = New Point(屏幕高宽.Width - Me.Width, 屏幕高宽.Height - Me.Height)
        End If
        Dim 本程序路径 As String = My.Application.Info.DirectoryPath
        If 本程序路径.EndsWith("\") = False Then 本程序路径 &= "\"
        If CefSharp.Cef.IsInitialized = False Then
            Dim 设置 As New CefSettings
            设置.Locale = "zh-CN"
            设置.AcceptLanguageList = "zh-CN,en-US,en"
            CefSharp.Cef.Initialize(设置)
        End If
        CefSharpSettings.LegacyJavascriptBindingEnabled = True
        浏览器_讯友录 = New ChromiumWebBrowser("file://" & 本程序路径.Replace("\", "/") & "Contacts.html") With {
            .Dock = DockStyle.Fill
        }
        浏览器_讯友录.MenuHandler = New MenuHandler
        讯友录的容器.Controls.Add(浏览器_讯友录)
        Dim 绑定设置 As BindingOptions = BindingOptions.DefaultBinder
        绑定设置.CamelCaseJavascriptNames = False
        浏览器_讯友录.RegisterJsObject("external", New JS接口_讯友录(Me), 绑定设置)
    End Sub

    Private Sub 主窗体_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim 本程序路径 As String = My.Application.Info.DirectoryPath
        If 本程序路径.EndsWith("\") = False Then 本程序路径 &= "\"

        Select Case My.Application.Culture.ThreeLetterISOLanguageName
            Case 语言代码_中文
                界面文字 = New 类_界面文字()
            Case Else
                界面文字 = New 类_界面文字(My.Resources.eng)
        End Select
        Me.Text = 界面文字.获取(0, Me.Text)
        任务栏小图标.Text = Me.Text
        任务栏小图标.Icon = Me.Icon

        Dim 类 As New 类_载入任务名称
        类 = Nothing

        当前用户 = New 类_用户
        当前用户.编号 = My.Settings.UserID
        当前用户.凭据_中心服务器 = My.Settings.Credential
        当前用户.域名_英语 = My.Settings.Domain
        If 测试 = False Then
            My.Settings.UserID = 0
            My.Settings.Credential = ""
            My.Settings.Domain = ""
            My.Settings.Save()
        End If

        文件保存对话框.InitialDirectory = My.Computer.FileSystem.SpecialDirectories.MyDocuments

        Call 添加聊天控件(机器人id_主控)
    End Sub

    Private Sub 浏览器_讯友录_FrameLoadEnd(sender As Object, e As FrameLoadEndEventArgs) Handles 浏览器_讯友录.FrameLoadEnd
        If e.Frame.IsMain Then 页面加载完毕()
    End Sub

    Private Sub 页面加载完毕()
        If InvokeRequired Then
            Dim d As New 页面加载完毕_跨线程(AddressOf 页面加载完毕)
            Invoke(d, New Object() {})
        Else
            Dim html As String = "<div class='RedNotify' style='display:none;'></div>" & 界面文字.获取(142, "讯宝机器人") & "<br><span class='Address'>" & 保留用户名_机器人 & 讯宝地址标识 & 讯宝网络域名_本国语 & "<br>" & 保留用户名_robot & 讯宝地址标识 & 讯宝网络域名_英语 & "</span>"
            浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("ShowRobot1(""" & html & """);")
            显示主控机器人聊天窗体()
        End If
    End Sub

    Private Sub 定时器_心跳_Tick(sender As Object, e As EventArgs) Handles 定时器_心跳.Tick
        Const 最大秒数 As Short = 600
        Dim I, J As Integer
        For I = 0 To 聊天控件数 - 1
            If 聊天控件(I).Equals(当前聊天控件) = False Then
                With 聊天控件(I)
                    If TypeOf .机器人 IsNot 类_机器人_主控 Then
                        .不活动秒数 += 定时器_心跳.Interval / 1000
                        If .不活动秒数 >= 最大秒数 Then
                            J += 1
                        End If
                    End If
                End With
            End If
        Next
        If J > 0 Then
            Dim 聊天控件2(聊天控件数 - J - 1) As 控件_聊天
            Dim 聊天控件数2 As Integer
            For I = 0 To 聊天控件数 - 1
                If 聊天控件(I).不活动秒数 < 最大秒数 Then
                    聊天控件2(聊天控件数2) = 聊天控件(I)
                    聊天控件数2 += 1
                Else
                    聊天控件(I).Dispose()
                    聊天控件(I) = Nothing
                End If
            Next
            聊天控件数 = 0
            聊天控件 = 聊天控件2
            聊天控件数 = 聊天控件数2
        End If
        If 主控机器人.心跳确认时间 > 0 Then
            If DateDiff(DateInterval.Second, Date.FromBinary(主控机器人.心跳确认时间), Date.Now) > 120 AndAlso 线程_发送讯宝 Is Nothing Then
                If 定时器_等待确认.Enabled = False Then
                    主控机器人.启动访问线程_传送服务器()
                End If
                Return
            End If
        End If
        If 正在发送的讯宝 Is Nothing Then
            If 网络连接器 IsNot Nothing Then
                正在发送的讯宝 = New 类_要发送的讯宝
                正在发送的讯宝.讯宝指令 = 讯宝指令_常量集合.无
跳转点1:
                If 当前用户.讯宝发送序号 < Long.MaxValue Then
                    当前用户.讯宝发送序号 += 1
                End If
                正在发送的讯宝.发送序号 = 当前用户.讯宝发送序号
                If 线程_发送讯宝 Is Nothing Then
                    线程_发送讯宝 = New Thread(New ThreadStart(AddressOf 发送))
                    线程_发送讯宝.Start()
                End If
                If 当前用户.加入的大聊天群 IsNot Nothing Then Call 检查大聊天群是否有新消息()
            Else
                主控机器人.自检()
            End If
        ElseIf 定时器_等待确认.Enabled = False Then
            GoTo 跳转点1
        End If
    End Sub

    Friend Sub 点击某一讯友(ByVal id As String)
        Dim 英语讯宝地址 As String = Nothing
        Dim 子域名 As String = Nothing
        Dim 群编号 As Long
        Select Case id.Substring(0, 1)
            Case "c"
                英语讯宝地址 = CType(当前讯友录(CInt(id.Substring(1))), 类_讯友).英语讯宝地址
            Case "s"
                With CType(当前讯友录(CInt(id.Substring(1))), 类_聊天群_小)
                    英语讯宝地址 = .群主.英语讯宝地址
                    群编号 = .编号
                End With
            Case "l"
                With CType(当前讯友录(CInt(id.Substring(1))), 类_聊天群_大)
                    子域名 = .子域名
                    群编号 = .编号
                End With
            Case "r"
                英语讯宝地址 = id
            Case "d"
                显示主控机器人聊天窗体()
                If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.黑域 Then
                    主控机器人.移除黑域(CType(当前讯友录(CInt(id.Substring(1))), 域名_复合数据))
                Else
                    主控机器人.移除白域(CType(当前讯友录(CInt(id.Substring(1))), 域名_复合数据))
                End If
                Return
            Case Else
                Return
        End Select
        Dim I As Integer
        For I = 0 To 聊天控件数 - 1
            With 聊天控件(I).聊天对象
                If .小聊天群 IsNot Nothing Then
                    If String.IsNullOrEmpty(子域名) = True Then
                        If .小聊天群.编号 = 群编号 AndAlso String.Compare(.讯友或群主.英语讯宝地址, 英语讯宝地址) = 0 Then
                            Exit For
                        End If
                    End If
                ElseIf .大聊天群 IsNot Nothing Then
                    If String.IsNullOrEmpty(子域名) = False Then
                        If .大聊天群.编号 = 群编号 AndAlso String.Compare(.大聊天群.子域名, 子域名) = 0 Then
                            Exit For
                        End If
                    End If
                End If
            End With
        Next
        If I < 聊天控件数 Then
            If 聊天控件(I).Equals(当前聊天控件) = False Then
                聊天控件(I).Show()
                If 当前聊天控件.Visible = True Then 当前聊天控件.Hide()
                当前聊天控件 = 聊天控件(I)
            Else
                If 当前聊天控件.Visible = False Then 当前聊天控件.Show()
            End If
            当前聊天控件.输入框.Focus()
            If 当前聊天控件.机器人 IsNot Nothing AndAlso TypeOf 当前聊天控件.机器人 Is 类_机器人_主控 Then
                With CType(当前聊天控件.机器人, 类_机器人_主控)
                    If .从未自检 Then
                        .自检()
                        .从未自检 = False
                    End If
                End With
            End If
            If I > 0 Then
                For I = I To 1 Step -1
                    聊天控件(I) = 聊天控件(I - 1)
                Next
                聊天控件(0) = 当前聊天控件
            End If
            当前聊天控件.不活动秒数 = 0
        Else
            Call 添加聊天控件(id)
        End If
        With 当前聊天控件.聊天对象
            If .小聊天群 IsNot Nothing Then
                If 群编号 = 0 Then
                    If .讯友或群主.新讯宝数量 <> 0 Then
                        .讯友或群主.新讯宝数量 = 0
                        数据库_更新新讯宝数量(.讯友或群主.英语讯宝地址, 群编号, 0)
                        浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("NewSSNumber('" & 获取id(.讯友或群主.英语讯宝地址) & "', '0');")
                    End If
                Else
                    If .小聊天群.新讯宝数量 <> 0 Then
                        .小聊天群.新讯宝数量 = 0
                        数据库_更新新讯宝数量(.讯友或群主.英语讯宝地址, 群编号, 0)
                        浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("NewSSNumber('" & 获取id(.讯友或群主.英语讯宝地址, 群编号) & "', '0');")
                    End If
                End If
            ElseIf .大聊天群 IsNot Nothing Then
                If .大聊天群.新讯宝数量 <> 0 Then
                    .大聊天群.新讯宝数量 = 0
                    数据库_更新新讯宝数量(.大聊天群.子域名, 群编号, 0)
                    浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("NewSSNumber('" & 获取id(.大聊天群.子域名, 群编号) & "', '0');")
                End If
            End If
        End With
    End Sub

    Private Sub 添加聊天控件(ByVal id As String)
        Dim 聊天对象 As New 类_聊天对象
        Select Case id.Substring(0, 1)
            Case "c"
                聊天对象.小聊天群 = New 类_聊天群_小
                聊天对象.讯友或群主 = CType(当前讯友录(CInt(id.Substring(1))), 类_讯友)
            Case "s"
                聊天对象.小聊天群 = CType(当前讯友录(CInt(id.Substring(1))), 类_聊天群_小)
                聊天对象.讯友或群主 = 聊天对象.小聊天群.群主
            Case "l"
                聊天对象.大聊天群 = CType(当前讯友录(CInt(id.Substring(1))), 类_聊天群_大)
            Case Else
                Select Case id
                    Case 机器人id_主控
                        聊天对象.讯友或群主 = New 类_讯友
                        聊天对象.讯友或群主.英语讯宝地址 = 机器人id_主控
                        聊天对象.小聊天群 = New 类_聊天群_小
                    Case 机器人id_系统管理
                        聊天对象.讯友或群主 = New 类_讯友
                        聊天对象.讯友或群主.英语讯宝地址 = 机器人id_系统管理
                        聊天对象.小聊天群 = New 类_聊天群_小
                    Case Else
                        Return
                End Select
        End Select
        添加聊天控件(聊天对象)
    End Sub

    Friend Sub 添加聊天控件(ByVal 聊天对象 As 类_聊天对象)
        If 当前聊天控件 IsNot Nothing Then
            If 当前聊天控件.Visible = True Then
                当前聊天控件.Hide()
                My.Application.DoEvents()
            End If
        End If
        If 聊天控件 Is Nothing Then
            ReDim 聊天控件(4)
            聊天控件数 = 0
        Else
            If 聊天控件.Length = 聊天控件数 Then ReDim Preserve 聊天控件(聊天控件数 * 2 - 1)
            Dim I As Integer
            For I = 聊天控件数 - 1 To 0 Step -1
                聊天控件(I + 1) = 聊天控件(I)
            Next
        End If
        Dim 控件 As New 控件_聊天(Me, 聊天对象)
        聊天控件(0) = 控件
        聊天控件数 += 1
        控件.Dock = DockStyle.Fill
        聊天控件的容器.Controls.Add(控件)
        If 聊天对象.小聊天群 IsNot Nothing Then
            Select Case 聊天对象.讯友或群主.英语讯宝地址
                Case 机器人id_主控
                    主控机器人 = New 类_机器人_主控(Me, 控件)
                    控件.机器人 = 主控机器人
                    当前用户.主控机器人 = 主控机器人
                Case 机器人id_系统管理
                    系统管理机器人 = New 类_机器人_系统管理(Me, 控件)
                    控件.机器人 = 系统管理机器人
                Case Else
                    If 聊天对象.小聊天群.编号 = 0 Then
                        控件.机器人 = New 类_机器人_一对一(Me, 控件)
                    Else
                        控件.机器人 = New 类_机器人_小聊天群(Me, 控件)
                    End If
            End Select
        ElseIf 聊天对象.大聊天群 IsNot Nothing Then
            控件.机器人 = New 类_机器人_大聊天群(Me, 控件)
        End If
        控件.载入任务名称()
        当前聊天控件 = 控件
        当前聊天控件.输入框.Focus()
    End Sub

    Friend Sub 显示可选范围()
        Dim 讯友标签() As String = Nothing
        Dim 讯友标签数, I As Integer
        If 当前用户.讯友目录 IsNot Nothing Then
            Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
            ReDim 讯友标签(讯友目录.Length * 2 - 1)
            For I = 0 To 讯友目录.Length - 1
                With 讯友目录(I)
                    收集标签(.标签一, 讯友标签, 讯友标签数)
                    收集标签(.标签二, 讯友标签, 讯友标签数)
                End With
            Next
            If 讯友标签数 > 0 Then
                If 讯友标签数 < 讯友标签.Length Then ReDim Preserve 讯友标签(讯友标签数 - 1)
                Array.Sort(讯友标签)
            End If
        End If
        Dim 变长文本 As StringBuilder
        If 讯友标签数 > 0 Then
            变长文本 = New StringBuilder(500 * (讯友标签数 + 4))
        Else
            变长文本 = New StringBuilder(500 * 4)
        End If
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write(生成讯友录范围html(讯友录显示范围_常量集合.最近))
        文本写入器.Write(生成讯友录范围html(讯友录显示范围_常量集合.讯友))
        文本写入器.Write(生成讯友录范围html(讯友录显示范围_常量集合.聊天群))
        文本写入器.Write(生成讯友录范围html(讯友录显示范围_常量集合.黑名单))
        文本写入器.Write(生成讯友录范围html(讯友录显示范围_常量集合.黑域))
        文本写入器.Write(生成讯友录范围html(讯友录显示范围_常量集合.白域))
        If 讯友标签数 > 0 Then
            For I = 0 To 讯友标签数 - 1
                文本写入器.Write(生成讯友录范围html(讯友录显示范围_常量集合.某标签, 讯友标签(I)))
            Next
        End If
        文本写入器.Close()
        浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("ShowRange(""" & 文本写入器.ToString & """);")
    End Sub

    Private Function 生成讯友录范围html(ByVal 范围 As 讯友录显示范围_常量集合, Optional ByVal 标签 As String = Nothing) As String
        Dim id As String
        Select Case 范围
            Case 讯友录显示范围_常量集合.最近 : id = "lately"
            Case 讯友录显示范围_常量集合.讯友 : id = "contacts"
            Case 讯友录显示范围_常量集合.聊天群 : id = "groups"
            Case 讯友录显示范围_常量集合.某标签 : id = "tag_" & 标签
            Case 讯友录显示范围_常量集合.黑名单 : id = "blacklist"
            Case 讯友录显示范围_常量集合.黑域 : id = "blackdomains"
            Case 讯友录显示范围_常量集合.白域 : id = "whitedomains"
            Case Else : id = "contacts"
        End Select
        Return "<div id='" & id & "' onclick='ClickARange(\""" & id & "\"")' onmouseover='OnMouseOver(\""" & id & "\"")' onmouseout='OnMouseOut(\""" & id & "\"")' class='Range'>" & 获取范围的名称(范围, 标签) & "</div>"
    End Function

    Friend Sub 点击某一范围(ByVal id As String)
        Select Case id
            Case "lately" : 刷新讯友录(讯友录显示范围_常量集合.最近)
            Case "contacts" : 刷新讯友录(讯友录显示范围_常量集合.讯友)
            Case "groups" : 刷新讯友录(讯友录显示范围_常量集合.聊天群)
            Case "blacklist" : 刷新讯友录(讯友录显示范围_常量集合.黑名单)
            Case "blackdomains" : 刷新讯友录(讯友录显示范围_常量集合.黑域)
            Case "whitedomains" : 刷新讯友录(讯友录显示范围_常量集合.白域)
            Case Else
                If id.StartsWith("tag_") Then
                    刷新讯友录(讯友录显示范围_常量集合.某标签, id.Substring(4))
                End If
        End Select
    End Sub

    Friend Sub 发送讯宝(Optional ByVal 继续 As Boolean = False)
        If 继续 = False Then
            If 网络连接器 Is Nothing Then
                主控机器人.启动访问线程_传送服务器()
                Return
            End If
            If 定时器_等待确认.Enabled = True Then
                Return
            End If
        End If
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As 类_列添加器
            Dim 筛选器 As 类_筛选器
            If 继续 AndAlso 正在发送的讯宝.讯宝指令 <> 讯宝指令_常量集合.无 Then
                If 正在发送的讯宝.讯宝指令 < 讯宝指令_常量集合.视频通话请求 Then
                    Dim 列添加器_新数据 As New 类_列添加器
                    列添加器_新数据.添加列_用于插入数据("发送序号", 正在发送的讯宝.发送序号)
                    列添加器_新数据.添加列_用于插入数据("发送时间", Date.UtcNow.Ticks)
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于筛选器("存储时间", 筛选方式_常量集合.等于, 正在发送的讯宝.存储时间)
                    筛选器 = New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    Dim 指令2 As 类_数据库指令_更新数据
                    If 正在发送的讯宝.群编号 = 0 Then
                        指令2 = New 类_数据库指令_更新数据(副数据库, "一对一讯宝", 列添加器_新数据, 筛选器, "#存储时间")
                    Else
                        指令2 = New 类_数据库指令_更新数据(副数据库, "小聊天群讯宝", 列添加器_新数据, 筛选器, "#存储时间")
                    End If
                    If 指令2.执行() = 0 Then
                        Dim I As Integer
                        For I = 0 To 聊天控件数 - 1
                            With 聊天控件(I).聊天对象
                                If .小聊天群 IsNot Nothing Then
                                    If .小聊天群.编号 = 正在发送的讯宝.群编号 AndAlso String.Compare(.讯友或群主.英语讯宝地址, 正在发送的讯宝.讯宝地址) = 0 Then
                                        Exit For
                                    End If
                                End If
                            End With
                        Next
                        Dim 机器人 As 类_机器人
                        If I < 聊天控件数 Then
                            机器人 = 聊天控件(I).机器人
                        Else
                            机器人 = 主控机器人
                        End If
                        If 正在发送的讯宝.群编号 = 0 Then
                            数据库_保存要发送的一对一讯宝(机器人, 正在发送的讯宝.讯宝地址, Date.UtcNow.Ticks, 讯宝指令_常量集合.撤回, 正在发送的讯宝.发送序号)
                        Else
                            数据库_保存要发送的小聊天群讯宝(机器人, 正在发送的讯宝.讯宝地址, 正在发送的讯宝.群编号, Date.UtcNow.Ticks, 讯宝指令_常量集合.撤回, 正在发送的讯宝.发送序号)
                        End If
                    End If
                Else
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于筛选器("存储时间", 筛选方式_常量集合.等于, 正在发送的讯宝.存储时间)
                    筛选器 = New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    Dim 指令2 As 类_数据库指令_删除数据
                    If 正在发送的讯宝.群编号 = 0 Then
                        指令2 = New 类_数据库指令_删除数据(副数据库, "一对一讯宝", 筛选器, "#存储时间")
                    Else
                        指令2 = New 类_数据库指令_删除数据(副数据库, "小聊天群讯宝", 筛选器, "#存储时间")
                    End If
                    指令2.执行()
                    If 正在发送的讯宝.文本库号 > 0 Then
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 正在发送的讯宝.文本编号)
                        筛选器 = New 类_筛选器
                        筛选器.添加一组筛选条件(列添加器)
                        指令2 = New 类_数据库指令_删除数据(副数据库, 正在发送的讯宝.文本库号 & "库", 筛选器, 主键索引名)
                        指令2.执行()
                    End If
                End If
            End If
            Dim 一对一讯宝 As New 类_要发送的讯宝
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"讯宝地址", "指令", "文本库号", "文本编号", "宽度", "高度", "秒数", "存储时间"})
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "一对一讯宝", Nothing, 1, 列添加器, , "#存储时间")
            读取器 = 指令.执行()
            While 读取器.读取
                With 一对一讯宝
                    .讯宝地址 = 读取器(0)
                    .讯宝指令 = 读取器(1)
                    .文本库号 = 读取器(2)
                    .文本编号 = 读取器(3)
                    .宽度 = 读取器(4)
                    .高度 = 读取器(5)
                    .秒数 = 读取器(6)
                    .存储时间 = 读取器(7)
                End With
                Exit While
            End While
            读取器.关闭()
            Dim 群讯宝 As New 类_要发送的讯宝
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"群主讯宝地址", "群编号", "指令", "文本库号", "文本编号", "宽度", "高度", "秒数", "存储时间"})
            指令 = New 类_数据库指令_请求获取数据(副数据库, "小聊天群讯宝", Nothing, 1, 列添加器, , "#存储时间")
            读取器 = 指令.执行()
            While 读取器.读取
                With 群讯宝
                    .讯宝地址 = 读取器(0)
                    .群编号 = 读取器(1)
                    .讯宝指令 = 读取器(2)
                    .文本库号 = 读取器(3)
                    .文本编号 = 读取器(4)
                    .宽度 = 读取器(5)
                    .高度 = 读取器(6)
                    .秒数 = 读取器(7)
                    .存储时间 = 读取器(8)
                End With
                Exit While
            End While
            读取器.关闭()
            If 一对一讯宝.存储时间 > 0 Then
                If 群讯宝.存储时间 > 0 Then
                    If 一对一讯宝.存储时间 <= 群讯宝.存储时间 Then
                        正在发送的讯宝 = 一对一讯宝
                    Else
                        正在发送的讯宝 = 群讯宝
                    End If
                Else
                    正在发送的讯宝 = 一对一讯宝
                End If
            ElseIf 群讯宝.存储时间 > 0 AndAlso 群讯宝.群编号 > 0 Then
                正在发送的讯宝 = 群讯宝
            Else
                正在发送的讯宝 = Nothing
                线程_发送讯宝 = Nothing
                Return
            End If
            If 正在发送的讯宝.文本库号 > 0 Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 正在发送的讯宝.文本编号)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("文本")
                指令 = New 类_数据库指令_请求获取数据(副数据库, 正在发送的讯宝.文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
                读取器 = 指令.执行()
                While 读取器.读取
                    正在发送的讯宝.文本 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
            End If
            If 当前用户.讯宝发送序号 < Long.MaxValue Then
                当前用户.讯宝发送序号 += 1
            End If
            正在发送的讯宝.发送序号 = 当前用户.讯宝发送序号
            Select Case 正在发送的讯宝.讯宝指令
                Case 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频, 讯宝指令_常量集合.发送文件, 讯宝指令_常量集合.修改图标
                    If File.Exists(正在发送的讯宝.文本) Then
                        正在发送的讯宝.文件字节数组 = File.ReadAllBytes(正在发送的讯宝.文本)
                        If 正在发送的讯宝.讯宝指令 = 讯宝指令_常量集合.发送短视频 Then
                            正在发送的讯宝.视频预览图片数据 = File.ReadAllBytes(正在发送的讯宝.文本 & ".jpg")
                        End If
                    End If
            End Select
        Catch ex As Exception
            正在发送的讯宝 = Nothing
            线程_发送讯宝 = Nothing
            If 读取器 IsNot Nothing Then 读取器.关闭()
            主控机器人.说(ex.Message)
            Return
        End Try
        If 线程_发送讯宝 Is Nothing Then
            线程_发送讯宝 = New Thread(New ThreadStart(AddressOf 发送))
            线程_发送讯宝.Start()
        End If
    End Sub

    Private Sub 发送()
        Try
            Dim SS包生成器 As New 类_SS包生成器()
            With 正在发送的讯宝
                SS包生成器.添加_有标签("序号", .发送序号)
                SS包生成器.添加_有标签("指令", .讯宝指令)
                If .讯宝指令 <> 讯宝指令_常量集合.无 Then
                    SS包生成器.添加_有标签("地址", .讯宝地址)
                    If .群编号 > 0 Then
                        SS包生成器.添加_有标签("群编号", .群编号)
                    End If
                    Select Case .讯宝指令
                        Case 讯宝指令_常量集合.确认收到, 讯宝指令_常量集合.发送文字, 讯宝指令_常量集合.撤回, 讯宝指令_常量集合.邀请加入小聊天群,
                                 讯宝指令_常量集合.删减聊天群成员, 讯宝指令_常量集合.修改聊天群名称, 讯宝指令_常量集合.创建小聊天群,
                                 讯宝指令_常量集合.添加黑域, 讯宝指令_常量集合.添加白域, 讯宝指令_常量集合.移除黑域,
                                 讯宝指令_常量集合.移除白域, 讯宝指令_常量集合.邀请加入大聊天群, 讯宝指令_常量集合.退出大聊天群,
                                 讯宝指令_常量集合.删除讯友, 讯宝指令_常量集合.给讯友添加标签, 讯宝指令_常量集合.移除讯友标签,
                                 讯宝指令_常量集合.修改讯友备注, 讯宝指令_常量集合.拉黑取消拉黑讯友, 讯宝指令_常量集合.重命名讯友标签
                            If String.IsNullOrEmpty(.文本) Then GoTo 跳转点
                            SS包生成器.添加_有标签("文本", .文本)
                        Case 讯宝指令_常量集合.发送语音
                            If String.IsNullOrEmpty(.文本) Then GoTo 跳转点
                            If .文件字节数组 Is Nothing Then GoTo 跳转点
                            SS包生成器.添加_有标签("文本", Path.GetExtension(.文本).Replace(".", ""))
                            SS包生成器.添加_有标签("秒数", .秒数)
                            SS包生成器.添加_有标签("文件", .文件字节数组)
                        Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                            If .宽度 < 1 OrElse .宽度 > 最大值_常量集合.讯宝预览图片宽高_像素 Then GoTo 跳转点
                            If .高度 < 1 OrElse .高度 > 最大值_常量集合.讯宝预览图片宽高_像素 Then GoTo 跳转点
                            If String.IsNullOrEmpty(.文本) Then GoTo 跳转点
                            If .文件字节数组 Is Nothing Then GoTo 跳转点
                            SS包生成器.添加_有标签("文本", Path.GetExtension(.文本).Replace(".", ""))
                            If .讯宝指令 = 讯宝指令_常量集合.发送短视频 Then
                                If .视频预览图片数据 Is Nothing Then GoTo 跳转点
                                SS包生成器.添加_有标签("预览", .视频预览图片数据)
                            End If
                            SS包生成器.添加_有标签("宽度", .宽度)
                            SS包生成器.添加_有标签("高度", .高度)
                            SS包生成器.添加_有标签("文件", .文件字节数组)
                        Case 讯宝指令_常量集合.发送文件
                            If String.IsNullOrEmpty(.文本) Then GoTo 跳转点
                            If .文件字节数组 Is Nothing Then GoTo 跳转点
                            SS包生成器.添加_有标签("文本", Path.GetFileName(.文本))
                            SS包生成器.添加_有标签("文件", .文件字节数组)
                        Case 讯宝指令_常量集合.获取小聊天群成员列表
                            If String.IsNullOrEmpty(.文本) = False Then
                                SS包生成器.添加_有标签("文本", .文本)
                            End If
                        Case 讯宝指令_常量集合.修改图标
                            If .文件字节数组 Is Nothing Then GoTo 跳转点
                            SS包生成器.添加_有标签("文件", .文件字节数组)
                    End Select
                End If
            End With

            'If 正在发送的讯宝.讯宝指令 < 讯宝指令_常量集合.视频通话请求 Then
            '    Try
            '        Dim 流写入器 As StreamWriter = File.AppendText("D:\who2.txt")
            '        流写入器.WriteLine(正在发送的讯宝.讯宝指令 & "_" & 正在发送的讯宝.文本 & " (" & 正在发送的讯宝.发送序号 & ")")
            '        流写入器.Flush()
            '        流写入器.Close()
            '    Catch ex As Exception
            '    End Try
            'End If

            If SS包生成器.发送SS包(网络连接器, 当前用户.AES加密器) = True Then
                等待确认()
            Else
                主控机器人.关闭网络连接器()
                发送完毕(False)
            End If
        Catch ex As Exception
            主控机器人.关闭网络连接器()
            发送完毕(False)
        End Try
        Return
跳转点:
        发送完毕(True)
    End Sub

    Friend Sub 发送完毕(ByVal 继续 As Boolean)
        If InvokeRequired Then
            Dim d As New 发送完毕_跨线程(AddressOf 发送完毕)
            Invoke(d, New Object() {继续})
        Else
            线程_发送讯宝 = Nothing
            If 继续 Then
                发送讯宝(True)
            Else
                正在发送的讯宝 = Nothing
            End If
        End If
    End Sub

    Private Sub 等待确认()
        If InvokeRequired Then
            Dim d As New 等待确认_跨线程(AddressOf 等待确认)
            Invoke(d, New Object() {})
        Else
            线程_发送讯宝 = Nothing
            定时器_等待确认.Stop()
            定时器_等待确认.Start()
        End If
    End Sub

    Private Sub 定时器_等待确认_Tick(sender As Object, e As EventArgs) Handles 定时器_等待确认.Tick
        定时器_等待确认.Stop()
        If 线程_发送讯宝 Is Nothing Then
            主控机器人.关闭网络连接器()
        End If
    End Sub

    Friend Sub 显示收到的讯友讯宝(ByVal 发送者英语讯宝地址 As String, ByVal 群编号 As Byte,
                         ByVal 群主英语讯宝地址 As String, ByVal 时间 As Long, ByVal 序号 As Long,
                         ByVal 讯宝指令 As 讯宝指令_常量集合, ByVal 文本 As String, ByVal 宽度 As Short,
                         ByVal 高度 As Short, ByVal 秒数 As Byte, ByVal 刷新 As Boolean)
        Dim I As Integer
        If 群编号 = 0 Then
            For I = 0 To 聊天控件数 - 1
                With 聊天控件(I).聊天对象
                    If .小聊天群 IsNot Nothing Then
                        If .小聊天群.编号 = 0 AndAlso String.Compare(.讯友或群主.英语讯宝地址, 发送者英语讯宝地址) = 0 Then
                            Exit For
                        End If
                    End If
                End With
            Next
        Else
            For I = 0 To 聊天控件数 - 1
                With 聊天控件(I).聊天对象
                    If .小聊天群 IsNot Nothing Then
                        If .小聊天群.编号 = 群编号 AndAlso String.Compare(.讯友或群主.英语讯宝地址, 群主英语讯宝地址) = 0 Then
                            Exit For
                        End If
                    End If
                End With
            Next
        End If
        If I < 聊天控件数 Then
            Dim 某一控件 As 控件_聊天 = 聊天控件(I)
            Select Case 讯宝指令
                Case 讯宝指令_常量集合.某人加入聊天群
                    某一控件.机器人.说(界面文字.获取(175, "#% 加入了本群。", (New String() {替换HTML和JS敏感字符(文本)})), 时间)
                Case 讯宝指令_常量集合.退出小聊天群
                    某一控件.机器人.说(界面文字.获取(178, "#% 离开了本群。", (New String() {替换HTML和JS敏感字符(文本)})), 时间)
                Case 讯宝指令_常量集合.删减聊天群成员
                    某一控件.机器人.说(界面文字.获取(190, "群主让 #% 离开了本群。", (New String() {替换HTML和JS敏感字符(文本)})), 时间)
                Case 讯宝指令_常量集合.修改聊天群名称
                    某一控件.机器人.说(界面文字.获取(185, "本群名称更改为 #%。", (New String() {替换HTML和JS敏感字符(文本)})), 时间)
                Case Else
                    某一控件.讯友说(发送者英语讯宝地址, 时间, 序号, 讯宝指令, 文本, 宽度, 高度, 秒数, False)
            End Select
            If I > 0 Then
                For I = I To 1 Step -1
                    聊天控件(I) = 聊天控件(I - 1)
                Next
                聊天控件(0) = 某一控件
            End If
            If Not 某一控件.Equals(当前聊天控件) OrElse Me.Visible = False OrElse Me.WindowState = FormWindowState.Minimized Then
                If 刷新 Then
                    刷新讯友录(讯友录显示范围_常量集合.最近)
                ElseIf 当前用户.讯友录当前显示范围 <> 讯友录显示范围_常量集合.最近 Then
                    刷新讯友录(讯友录显示范围_常量集合.最近)
                End If
                If 群编号 = 0 Then
                    With 某一控件.聊天对象.讯友或群主
                        If .新讯宝数量 < 999 Then
                            .新讯宝数量 += 1
                            数据库_更新新讯宝数量(.英语讯宝地址, 群编号, .新讯宝数量)
                            浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("NewSSNumber('" & 获取id(发送者英语讯宝地址) & "', '" & .新讯宝数量 & "');")
                        End If
                    End With
                Else
                    With 某一控件.聊天对象.小聊天群
                        If .新讯宝数量 < 999 Then
                            .新讯宝数量 += 1
                            数据库_更新新讯宝数量(.群主.英语讯宝地址, 群编号, .新讯宝数量)
                            浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("NewSSNumber('" & 获取id(群主英语讯宝地址, 群编号) & "', '" & .新讯宝数量 & "');")
                        End If
                    End With
                End If
            Else
                If 刷新 AndAlso 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.最近 Then
                    刷新讯友录()
                End If
            End If
        Else
            If 刷新 Then
                刷新讯友录(讯友录显示范围_常量集合.最近)
            ElseIf 当前用户.讯友录当前显示范围 <> 讯友录显示范围_常量集合.最近 Then
                刷新讯友录(讯友录显示范围_常量集合.最近)
            End If
            If 群编号 = 0 Then
                Dim 某一讯友 As 类_讯友 = 当前用户.查找讯友(发送者英语讯宝地址)
                If 某一讯友 IsNot Nothing Then
                    If 某一讯友.新讯宝数量 < 999 Then
                        某一讯友.新讯宝数量 += 1
                        数据库_更新新讯宝数量(某一讯友.英语讯宝地址, 群编号, 某一讯友.新讯宝数量)
                        浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("NewSSNumber('" & 获取id(发送者英语讯宝地址) & "', '" & 某一讯友.新讯宝数量 & "');")
                    End If
                End If
            Else
                Dim 某一群 As 类_聊天群_小 = 当前用户.查找小聊天群(群主英语讯宝地址, 群编号)
                If 某一群 IsNot Nothing Then
                    If 某一群.新讯宝数量 < 999 Then
                        某一群.新讯宝数量 += 1
                        数据库_更新新讯宝数量(某一群.群主.英语讯宝地址, 群编号, 某一群.新讯宝数量)
                        浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("NewSSNumber('" & 获取id(群主英语讯宝地址, 群编号) & "', '" & 某一群.新讯宝数量 & "');")
                    End If
                End If
            End If
        End If
    End Sub

    Friend Sub 显示同步的讯宝(ByVal 群编号 As Byte, ByVal 群主英语讯宝地址 As String, ByVal 时间 As Long,
                         ByVal 序号 As Long, ByVal 讯宝指令 As 讯宝指令_常量集合, ByVal 文本 As String, ByVal 宽度 As Short,
                         ByVal 高度 As Short, ByVal 秒数 As Byte, ByVal 刷新 As Boolean)
        Dim I As Integer
        If 群编号 = 0 Then
            For I = 0 To 聊天控件数 - 1
                With 聊天控件(I).聊天对象
                    If .小聊天群 IsNot Nothing Then
                        If .小聊天群.编号 = 0 AndAlso String.Compare(.讯友或群主.英语讯宝地址, 群主英语讯宝地址) = 0 Then
                            Exit For
                        End If
                    End If
                End With
            Next
        Else
            For I = 0 To 聊天控件数 - 1
                With 聊天控件(I).聊天对象
                    If .小聊天群 IsNot Nothing Then
                        If .小聊天群.编号 = 群编号 AndAlso String.Compare(.讯友或群主.英语讯宝地址, 群主英语讯宝地址) = 0 Then
                            Exit For
                        End If
                    End If
                End With
            Next
        End If
        If I < 聊天控件数 Then
            Dim 某一控件 As 控件_聊天 = 聊天控件(I)
            某一控件.显示同步的讯宝(时间, 讯宝指令, 文本, 宽度, 高度, 秒数)
            If I > 0 Then
                For I = I To 1 Step -1
                    聊天控件(I) = 聊天控件(I - 1)
                Next
                聊天控件(0) = 某一控件
            End If
            If Not 某一控件.Equals(当前聊天控件) Then
                If 刷新 Then
                    刷新讯友录(讯友录显示范围_常量集合.最近)
                ElseIf 当前用户.讯友录当前显示范围 <> 讯友录显示范围_常量集合.最近 Then
                    刷新讯友录(讯友录显示范围_常量集合.最近)
                End If
            Else
                If 刷新 AndAlso 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.最近 Then
                    刷新讯友录()
                End If
            End If
        Else
            If 刷新 Then
                刷新讯友录(讯友录显示范围_常量集合.最近)
            ElseIf 当前用户.讯友录当前显示范围 <> 讯友录显示范围_常量集合.最近 Then
                刷新讯友录(讯友录显示范围_常量集合.最近)
            End If
        End If
    End Sub

    Friend Sub 显示收到的陌生人讯宝(ByVal 英语讯宝地址 As String, ByVal 时间 As Long, ByVal 序号 As Long, ByVal 讯宝指令 As 讯宝指令_常量集合, ByVal 文本 As String)
        If Not 主控机器人.聊天控件.Equals(当前聊天控件) Then
            With 主控机器人.聊天控件.聊天对象.讯友或群主
                If .新讯宝数量 < 999 Then
                    .新讯宝数量 += 1
                    数据库_更新新讯宝数量(.英语讯宝地址, 0, .新讯宝数量)
                    浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("NewSSNumber('" & 机器人id_主控 & "', '" & .新讯宝数量 & "');")
                End If
            End With
        End If
        主控机器人.聊天控件.陌生人说(英语讯宝地址, 时间, 序号, 讯宝指令, 文本)
    End Sub

    Friend Sub 提示讯宝发送失败(ByVal 发送者英语讯宝地址 As String, ByVal 群编号 As Byte, ByVal 群主英语讯宝地址 As String,
                        ByVal 讯宝指令 As 讯宝指令_常量集合, ByVal 发送序号 As Long, ByVal 讯宝文本 As String)
        Dim I As Integer
        If 讯宝指令 <> 讯宝指令_常量集合.被邀请加入大聊天群者未添加我为讯友 Then
            If 群编号 = 0 Then
                For I = 0 To 聊天控件数 - 1
                    With 聊天控件(I).聊天对象
                        If .小聊天群 IsNot Nothing Then
                            If .小聊天群.编号 = 0 AndAlso String.Compare(.讯友或群主.英语讯宝地址, 发送者英语讯宝地址) = 0 Then
                                Exit For
                            End If
                        End If
                    End With
                Next
            Else
                For I = 0 To 聊天控件数 - 1
                    With 聊天控件(I).聊天对象
                        If .小聊天群 IsNot Nothing Then
                            If .小聊天群.编号 = 群编号 AndAlso String.Compare(.讯友或群主.英语讯宝地址, 群主英语讯宝地址) = 0 Then
                                Exit For
                            End If
                        End If
                    End With
                Next
            End If
        Else
            Dim 子域名 As String = Nothing
            Dim 大聊天群编号 As Long
            Dim SS解读器 As New 类_SS包解读器()
            Try
                SS解读器.解读纯文本(讯宝文本)
                SS解读器.读取_有标签("D", 子域名)
                SS解读器.读取_有标签("I", 大聊天群编号)
            Catch ex As Exception
                Return
            End Try
            For I = 0 To 聊天控件数 - 1
                With 聊天控件(I).聊天对象
                    If .大聊天群 IsNot Nothing Then
                        If .大聊天群.编号 = 大聊天群编号 AndAlso String.Compare(.大聊天群.子域名, 子域名) = 0 Then
                            Exit For
                        End If
                    End If
                End With
            Next
        End If
        If I < 聊天控件数 Then
            Dim 读取器 As 类_读取器_外部 = Nothing
            Try
                Dim 指令 As 类_数据库指令_请求获取数据
                Dim 列添加器 As New 类_列添加器
                Dim 存储时间 As Long
                Dim 文本库号 As Short
                Dim 文本编号 As Long
                Dim 筛选器 As 类_筛选器
                If TypeOf 聊天控件(I).机器人 Is 类_机器人_一对一 OrElse 讯宝指令 = 讯宝指令_常量集合.被邀请加入小聊天群者未添加我为讯友 OrElse 讯宝指令 = 讯宝指令_常量集合.被邀请加入大聊天群者未添加我为讯友 Then
                    列添加器.添加列_用于筛选器("讯宝地址", 筛选方式_常量集合.等于, 发送者英语讯宝地址)
                    列添加器.添加列_用于筛选器("是接收者", 筛选方式_常量集合.等于, True)
                    列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, 发送序号)
                    筛选器 = New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于获取数据(New String() {"文本库号", "文本编号", "存储时间"})
                    指令 = New 类_数据库指令_请求获取数据(副数据库, "一对一讯宝", 筛选器, 1, 列添加器, , "#地址是接收者发送序号")
                Else
                    列添加器.添加列_用于筛选器("群主讯宝地址", 筛选方式_常量集合.等于, 群主英语讯宝地址)
                    列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
                    列添加器.添加列_用于筛选器("发送者讯宝地址", 筛选方式_常量集合.等于, 发送者英语讯宝地址)
                    列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, 发送序号)
                    筛选器 = New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于获取数据(New String() {"文本库号", "文本编号", "存储时间"})
                    指令 = New 类_数据库指令_请求获取数据(副数据库, "小聊天群讯宝", 筛选器, 1, 列添加器, , "#群主编号发送者发送序号")
                End If
                读取器 = 指令.执行()
                While 读取器.读取
                    文本库号 = 读取器(0)
                    文本编号 = 读取器(1)
                    存储时间 = 读取器(2)
                    Exit While
                End While
                读取器.关闭()
                If TypeOf 聊天控件(I).机器人 Is 类_机器人_一对一 OrElse 讯宝指令 = 讯宝指令_常量集合.被邀请加入小聊天群者未添加我为讯友 OrElse 讯宝指令 = 讯宝指令_常量集合.被邀请加入大聊天群者未添加我为讯友 Then
                    列添加器.添加列_用于筛选器("讯宝地址", 筛选方式_常量集合.等于, 发送者英语讯宝地址)
                    列添加器.添加列_用于筛选器("是接收者", 筛选方式_常量集合.等于, True)
                    列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, 发送序号)
                    列添加器.添加列_用于筛选器("存储时间", 筛选方式_常量集合.等于, 存储时间)
                    筛选器 = New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    Dim 指令2 As New 类_数据库指令_删除数据(副数据库, "一对一讯宝", 筛选器, "#地址是接收者发送序号")
                    指令2.执行()
                Else
                    列添加器.添加列_用于筛选器("群主讯宝地址", 筛选方式_常量集合.等于, 群主英语讯宝地址)
                    列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
                    列添加器.添加列_用于筛选器("发送者讯宝地址", 筛选方式_常量集合.等于, 发送者英语讯宝地址)
                    列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, 发送序号)
                    列添加器.添加列_用于筛选器("存储时间", 筛选方式_常量集合.等于, 存储时间)
                    筛选器 = New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    Dim 指令2 As New 类_数据库指令_删除数据(副数据库, "小聊天群讯宝", 筛选器, "#群主编号发送者发送序号")
                    指令2.执行()
                End If
                If 文本库号 > 0 Then
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 文本编号)
                    筛选器 = New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    Dim 指令3 As New 类_数据库指令_删除数据(副数据库, 文本库号 & "库", 筛选器, 主键索引名)
                    指令3.执行()
                End If
            Catch ex As Exception
                If 读取器 IsNot Nothing Then 读取器.关闭()
                主控机器人.说(ex.Message)
            End Try
            Dim 文本 As String
            Select Case 讯宝指令
                Case 讯宝指令_常量集合.已是群成员 : 文本 = 界面文字.获取(89, "他/她已加入当前聊天群。")
                Case 讯宝指令_常量集合.不是群成员 : 文本 = 界面文字.获取(83, "你不是当前聊天群的成员。")
                Case 讯宝指令_常量集合.群成员数量已达上限 : 文本 = 界面文字.获取(171, "群成员数量已达上限。")
                Case 讯宝指令_常量集合.对方未添加我为讯友, 讯宝指令_常量集合.被邀请加入小聊天群者未添加我为讯友,
                    讯宝指令_常量集合.被邀请加入大聊天群者未添加我为讯友
                    文本 = 界面文字.获取(153, "发送失败。#%未添加你为讯友。", New String() {发送者英语讯宝地址})
                Case 讯宝指令_常量集合.对方把我拉黑了 : 文本 = 界面文字.获取(52, "发送失败。你已被其列入黑名单。")
                Case 讯宝指令_常量集合.讯宝地址不存在 : 文本 = 界面文字.获取(131, "发送失败。讯宝地址已不存在。")
                Case 讯宝指令_常量集合.群里没有成员 : 文本 = 界面文字.获取(170, "除你之外，聊天群里没有其他人。")
                Case 讯宝指令_常量集合.群里还有成员 : 文本 = 界面文字.获取(182, "无法解散还有成员的群。")
                Case 讯宝指令_常量集合.加入的群数量已达上限 : 文本 = 界面文字.获取(174, "你加入的小聊天群数量已达上限。")
                Case 讯宝指令_常量集合.本小时发送的讯宝数量已达上限 : 文本 = 界面文字.获取(266, "本小时发送的讯宝数量已达上限。")
                Case 讯宝指令_常量集合.今日发送的讯宝数量已达上限 : 文本 = 界面文字.获取(267, "今日发送的讯宝数量已达上限。")
                Case 讯宝指令_常量集合.数据传送失败 : 文本 = 界面文字.获取(198, "数据传送失败。")
                Case 讯宝指令_常量集合.HTTP数据错误 : 文本 = 界面文字.获取(200, "HTTP数据错误。")
                Case 讯宝指令_常量集合.目标服务器程序出错 : 文本 = 界面文字.获取(201, "目标服务器程序出错。")
                Case Else : 文本 = 界面文字.获取(108, "出错 #%", New Object() {讯宝指令})
            End Select
            聊天控件(I).机器人.说(文本)
        End If
    End Sub

    Friend Sub 显示收到的大聊天群讯宝(ByVal 子域名 As String, ByVal 有新讯宝的群() As 有新讯宝的群_复合数据, ByVal 刷新 As Boolean)
        Dim 群编号 As Long
        Dim I, J, K As Integer
        For I = 0 To 有新讯宝的群.Length - 1
            With 有新讯宝的群(I)
                群编号 = .编号
                For J = 0 To 聊天控件数 - 1
                    With 聊天控件(J).聊天对象
                        If .大聊天群 IsNot Nothing Then
                            If .大聊天群.编号 = 群编号 AndAlso String.Compare(.大聊天群.子域名, 子域名) = 0 Then
                                Exit For
                            End If
                        End If
                    End With
                Next
                If J < 聊天控件数 Then
                    Dim 某一控件 As 控件_聊天 = 聊天控件(J)
                    If .撤回的讯宝数量 > 0 Then
                        For K = 0 To .撤回的讯宝数量 - 1
                            某一控件.浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("RemoveSS('" & .撤回的讯宝(K) & "');")
                        Next
                    End If
                    某一控件.加载大聊天群新讯宝(.时间)
                    If J > 0 Then
                        For J = J To 1 Step -1
                            聊天控件(J) = 聊天控件(J - 1)
                        Next
                        聊天控件(0) = 某一控件
                    End If
                    If Not 某一控件.Equals(当前聊天控件) OrElse Me.Visible = False OrElse Me.WindowState = FormWindowState.Minimized Then
                        If 刷新 Then
                            刷新讯友录(讯友录显示范围_常量集合.最近)
                        ElseIf 当前用户.讯友录当前显示范围 <> 讯友录显示范围_常量集合.最近 Then
                            刷新讯友录(讯友录显示范围_常量集合.最近)
                        End If
                        Dim 某一群 As 类_聊天群_大 = 当前用户.查找大聊天群(子域名, 群编号)
                        If 某一群 IsNot Nothing Then
                            If 某一群.新讯宝数量 + .新讯宝数量 < 1000 Then
                                某一群.新讯宝数量 += .新讯宝数量
                                数据库_更新新讯宝数量(子域名, 群编号, 某一群.新讯宝数量)
                                浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("NewSSNumber('" & 获取id(子域名, 群编号) & "', '" & 某一群.新讯宝数量 & "');")
                            End If
                        End If
                    Else
                        If 刷新 AndAlso 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.最近 Then
                            刷新讯友录()
                        End If
                    End If
                Else
                    If 刷新 Then
                        刷新讯友录(讯友录显示范围_常量集合.最近)
                    ElseIf 当前用户.讯友录当前显示范围 <> 讯友录显示范围_常量集合.最近 Then
                        刷新讯友录(讯友录显示范围_常量集合.最近)
                    End If
                    Dim 某一群 As 类_聊天群_大 = 当前用户.查找大聊天群(子域名, 群编号)
                    If 某一群 IsNot Nothing Then
                        If 某一群.新讯宝数量 + .新讯宝数量 < 1000 Then
                            某一群.新讯宝数量 += .新讯宝数量
                            数据库_更新新讯宝数量(子域名, 群编号, 某一群.新讯宝数量)
                            浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("NewSSNumber('" & 获取id(子域名, 群编号) & "', '" & 某一群.新讯宝数量 & "');")
                        End If
                    End If
                End If
            End With
        Next
    End Sub

    Friend Sub 事件同步(ByVal 同步事件 As 同步事件_常量集合, ByVal SS包解读器 As 类_SS包解读器)
        Dim 英语讯宝地址 As String = Nothing
        SS包解读器.读取_有标签("英语讯宝地址", 英语讯宝地址)
        If String.IsNullOrEmpty(英语讯宝地址) = False Then
            Dim I As Integer
            For I = 0 To 聊天控件数 - 1
                With 聊天控件(I).聊天对象
                    If .小聊天群 IsNot Nothing Then
                        If .小聊天群.编号 = 0 AndAlso String.Compare(.讯友或群主.英语讯宝地址, 英语讯宝地址) = 0 Then
                            Exit For
                        End If
                    End If
                End With
            Next
            If I < 聊天控件数 Then
                Select Case 同步事件
                    Case 同步事件_常量集合.讯友添加标签
                        CType(聊天控件(I).机器人, 类_机器人_一对一).添加标签成功(SS包解读器)
                    Case 同步事件_常量集合.讯友移除标签
                        CType(聊天控件(I).机器人, 类_机器人_一对一).移除标签成功(SS包解读器)
                    Case 同步事件_常量集合.修改讯友备注
                        CType(聊天控件(I).机器人, 类_机器人_一对一).修改备注成功(SS包解读器)
                    Case 同步事件_常量集合.拉黑讯友
                        CType(聊天控件(I).机器人, 类_机器人_一对一).拉黑讯友成功(SS包解读器)
                End Select
            ElseIf 当前用户.讯友目录 IsNot Nothing Then
                Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
                For I = 0 To 讯友目录.Length - 1
                    If String.Compare(讯友目录(I).英语讯宝地址, 英语讯宝地址) = 0 Then Exit For
                Next
                If I < 讯友目录.Length Then
                    Dim 讯友录更新时间 As Long
                    SS包解读器.读取_有标签("时间", 讯友录更新时间)
                    Select Case 同步事件
                        Case 同步事件_常量集合.讯友添加标签
                            Dim 标签 As String = Nothing
                            SS包解读器.读取_有标签("标签名称", 标签)
                            With 讯友目录(I)
                                If String.IsNullOrEmpty(.标签一) Then
                                    .标签一 = 标签
                                ElseIf String.IsNullOrEmpty(.标签二) Then
                                    .标签二 = 标签
                                End If
                            End With
                        Case 同步事件_常量集合.讯友移除标签
                            Dim 标签名称 As String = Nothing
                            SS包解读器.读取_有标签("标签名称", 标签名称)
                            With 讯友目录(I)
                                If String.Compare(.标签一, 标签名称, True) = 0 Then
                                    .标签一 = Nothing
                                ElseIf String.Compare(.标签二, 标签名称, True) = 0 Then
                                    .标签二 = Nothing
                                End If
                            End With
                        Case 同步事件_常量集合.修改讯友备注
                            SS包解读器.读取_有标签("备注", 讯友目录(I).备注)
                            Select Case 当前用户.讯友录当前显示范围
                                Case 讯友录显示范围_常量集合.最近, 讯友录显示范围_常量集合.讯友, 讯友录显示范围_常量集合.某标签, 讯友录显示范围_常量集合.黑名单
                                    刷新讯友录()
                            End Select
                        Case 同步事件_常量集合.拉黑讯友
                            讯友目录(I).拉黑 = True
                            Select Case 当前用户.讯友录当前显示范围
                                Case 讯友录显示范围_常量集合.最近, 讯友录显示范围_常量集合.讯友, 讯友录显示范围_常量集合.某标签, 讯友录显示范围_常量集合.黑名单
                                    刷新讯友录()
                            End Select
                    End Select
                    If 讯友录更新时间 > 0 Then 当前用户.讯友录更新时间 = 讯友录更新时间
                End If
            End If
        End If
    End Sub

    Private Sub 主窗体_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        Select Case e.KeyCode
            Case Keys.Return
                If e.Control = False Then
                    If 当前聊天控件 IsNot Nothing Then 当前聊天控件.按钮_说话.PerformClick()
                End If
        End Select
    End Sub

    Private Sub 主窗体_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If 当前用户.已登录() Then
            If e.CloseReason = CloseReason.UserClosing Then
                If 关闭 = False Then
                    Me.Hide()
                    任务栏小图标.Visible = True
                    If My.Settings.SmallIconClicked = False Then
                        任务栏小图标.BalloonTipTitle = Me.Text
                        任务栏小图标.BalloonTipText = 界面文字.获取(92, "我变成了小图标")
                        任务栏小图标.ShowBalloonTip(10000)
                    End If
                    e.Cancel = True
                    Return
                ElseIf 保存凭据 Then
                    GoTo 保存
                End If
            ElseIf e.CloseReason = CloseReason.WindowsShutDown Then
保存:
                My.Settings.UserID = 当前用户.编号
                My.Settings.Credential = 当前用户.凭据_中心服务器
                My.Settings.Domain = 当前用户.域名_英语
            End If
            If 备份管理器 IsNot Nothing Then
                备份管理器.停止()
                备份管理器 = Nothing
            End If
            If 副数据库 IsNot Nothing Then
                副数据库.关闭()
                副数据库 = Nothing
            End If
        End If
        If 录音类 IsNot Nothing Then
            录音类.关闭()
            录音类 = Nothing
        End If
        If 播音类 IsNot Nothing Then
            播音类.关闭()
            播音类 = Nothing
        End If
        If 下载文件的窗体 IsNot Nothing Then
            下载文件的窗体.关闭 = True
            下载文件的窗体.Close()
            下载文件的窗体 = Nothing
        End If
        My.Settings.Save()
    End Sub

    Friend Sub 显示主控机器人聊天窗体()
        浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("ClickAContact('" & 机器人id_主控 & "');")
    End Sub

    Private Sub 任务栏小图标_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles 任务栏小图标.Click
        If My.Settings.SmallIconClicked = False Then
            My.Settings.SmallIconClicked = True
            My.Settings.Save()
        End If
        任务栏小图标.Visible = False
        Me.Visible = True
        Me.BringToFront()
        Me.TopMost = True
        Me.Focus()
        Me.TopMost = False
    End Sub

    Friend Sub 刷新讯友录(Optional ByVal 范围 As 讯友录显示范围_常量集合 = 讯友录显示范围_常量集合.未指定, Optional ByVal 标签名称 As String = Nothing, Optional ByVal 自动跳转 As Boolean = False)
        If 范围 = 讯友录显示范围_常量集合.未指定 Then
            范围 = 当前用户.讯友录当前显示范围
            If 范围 = 讯友录显示范围_常量集合.某标签 Then
                标签名称 = 当前用户.讯友录当前显示标签
            End If
        End If
        当前讯友录 = Nothing
        Dim I, 当前讯友录数量 As Integer
        Dim 变长文本 As New StringBuilder(2000)
        Dim 文本写入器 As New StringWriter(变长文本)
        If 范围 = 讯友录显示范围_常量集合.最近 Then
            Dim 最近讯友(99) As 最近讯友_复合数据
            Dim 最近讯友数量 As Integer
            If 数据库_获取互动讯友排名(最近讯友, 最近讯友数量) = True Then
                If 最近讯友数量 > 0 Then
                    ReDim 当前讯友录(最近讯友数量 - 1)
                    Dim 某一最近讯友 As 最近讯友_复合数据
                    Dim J As Integer
                    For I = 0 To 最近讯友数量 - 1
                        某一最近讯友 = 最近讯友(I)
                        If 某一最近讯友.英语讯宝地址.Contains(讯宝地址标识) Then
                            If 某一最近讯友.群编号 = 0 AndAlso 当前用户.讯友目录 IsNot Nothing Then
                                Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
                                For J = 0 To 讯友目录.Length - 1
                                    If String.Compare(某一最近讯友.英语讯宝地址, 讯友目录(J).英语讯宝地址) = 0 Then
                                        讯友目录(J).新讯宝数量 = 某一最近讯友.新讯宝数量
                                        Exit For
                                    End If
                                Next
                                If J < 讯友目录.Length Then
                                    当前讯友录(当前讯友录数量) = 讯友目录(J)
                                    文本写入器.Write(生成讯友html(讯友目录(J), 当前讯友录数量))
                                    当前讯友录数量 += 1
                                End If
                            ElseIf 当前用户.加入的小聊天群 IsNot Nothing Then
                                Dim 加入的小聊天群() As 类_聊天群_小 = 当前用户.加入的小聊天群
                                For J = 0 To 加入的小聊天群.Length - 1
                                    If String.Compare(某一最近讯友.英语讯宝地址, 加入的小聊天群(J).群主.英语讯宝地址) = 0 AndAlso
                                    某一最近讯友.群编号 = 加入的小聊天群(J).编号 Then
                                        加入的小聊天群(J).新讯宝数量 = 某一最近讯友.新讯宝数量
                                        Exit For
                                    End If
                                Next
                                If J < 加入的小聊天群.Length Then
                                    当前讯友录(当前讯友录数量) = 加入的小聊天群(J)
                                    文本写入器.Write(生成小聊天群html(加入的小聊天群(J), 当前讯友录数量))
                                    当前讯友录数量 += 1
                                End If
                            End If
                        ElseIf 当前用户.加入的大聊天群 IsNot Nothing Then
                            Dim 加入的大聊天群() As 类_聊天群_大 = 当前用户.加入的大聊天群
                            For J = 0 To 加入的大聊天群.Length - 1
                                If String.Compare(某一最近讯友.英语讯宝地址, 加入的大聊天群(J).子域名) = 0 AndAlso
                                    某一最近讯友.群编号 = 加入的大聊天群(J).编号 Then
                                    加入的大聊天群(J).新讯宝数量 = 某一最近讯友.新讯宝数量
                                    Exit For
                                End If
                            Next
                            If J < 加入的大聊天群.Length Then
                                当前讯友录(当前讯友录数量) = 加入的大聊天群(J)
                                文本写入器.Write(生成大聊天群html(加入的大聊天群(J), 当前讯友录数量))
                                当前讯友录数量 += 1
                            End If
                        End If
                    Next
                End If
            End If
            If 变长文本.Length = 0 AndAlso 自动跳转 Then
                范围 = 讯友录显示范围_常量集合.讯友
                GoTo 全部
            End If
        ElseIf 范围 = 讯友录显示范围_常量集合.讯友 Then
全部:
            If 当前用户.讯友目录 IsNot Nothing Then
                Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
                ReDim 当前讯友录(讯友目录.Length - 1)
                For I = 0 To 讯友目录.Length - 1
                    If 讯友目录(I).拉黑 = False Then
                        当前讯友录(当前讯友录数量) = 讯友目录(I)
                        文本写入器.Write(生成讯友html(讯友目录(I), 当前讯友录数量))
                        当前讯友录数量 += 1
                    End If
                Next
            End If
        ElseIf 范围 = 讯友录显示范围_常量集合.聊天群 Then
            If 当前用户.加入的小聊天群 IsNot Nothing OrElse 当前用户.加入的大聊天群 IsNot Nothing Then
                If 当前用户.加入的小聊天群 IsNot Nothing AndAlso 当前用户.加入的大聊天群 IsNot Nothing Then
                    ReDim 当前讯友录(当前用户.加入的小聊天群.Length + 当前用户.加入的大聊天群.Length - 1)
                ElseIf 当前用户.加入的小聊天群 IsNot Nothing Then
                    ReDim 当前讯友录(当前用户.加入的小聊天群.Length - 1)
                ElseIf 当前用户.加入的大聊天群 IsNot Nothing Then
                    ReDim 当前讯友录(当前用户.加入的大聊天群.Length - 1)
                End If
                If 当前用户.加入的小聊天群 IsNot Nothing Then
                    Dim 加入的小聊天群() As 类_聊天群_小 = 当前用户.加入的小聊天群
                    For I = 0 To 加入的小聊天群.Length - 1
                        当前讯友录(当前讯友录数量) = 加入的小聊天群(I)
                        文本写入器.Write(生成小聊天群html(加入的小聊天群(I), 当前讯友录数量))
                        当前讯友录数量 += 1
                    Next
                End If
                If 当前用户.加入的大聊天群 IsNot Nothing Then
                    Dim 加入的大聊天群() As 类_聊天群_大 = 当前用户.加入的大聊天群
                    For I = 0 To 加入的大聊天群.Length - 1
                        当前讯友录(当前讯友录数量) = 加入的大聊天群(I)
                        文本写入器.Write(生成大聊天群html(加入的大聊天群(I), 当前讯友录数量))
                        当前讯友录数量 += 1
                    Next
                End If
            End If
        ElseIf 范围 = 讯友录显示范围_常量集合.某标签 Then
            If 当前用户.讯友目录 IsNot Nothing Then
                If String.IsNullOrEmpty(标签名称) = False Then
                    Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
                    ReDim 当前讯友录(讯友目录.Length - 1)
                    For I = 0 To 讯友目录.Length - 1
                        With 讯友目录(I)
                            If String.Compare(.标签一, 标签名称) = 0 OrElse String.Compare(.标签二, 标签名称) = 0 Then
                                当前讯友录(当前讯友录数量) = 讯友目录(I)
                                文本写入器.Write(生成讯友html(讯友目录(I), 当前讯友录数量))
                                当前讯友录数量 += 1
                                Exit For
                            End If
                        End With
                    Next
                End If
            End If
        ElseIf 范围 = 讯友录显示范围_常量集合.黑名单 Then
            If 当前用户.讯友目录 IsNot Nothing Then
                Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
                ReDim 当前讯友录(讯友目录.Length - 1)
                For I = 0 To 讯友目录.Length - 1
                    If 讯友目录(I).拉黑 = True Then
                        当前讯友录(当前讯友录数量) = 讯友目录(I)
                        文本写入器.Write(生成讯友html(讯友目录(I), 当前讯友录数量))
                        当前讯友录数量 += 1
                    End If
                Next
            End If
        ElseIf 范围 = 讯友录显示范围_常量集合.黑域 Then
            If 当前用户.黑域 IsNot Nothing Then
                Dim 黑域() As 域名_复合数据 = 当前用户.黑域
                ReDim 当前讯友录(黑域.Length - 1)
                For I = 0 To 黑域.Length - 1
                    If String.Compare(黑域(I).英语, 黑域_全部) = 0 Then
                        当前讯友录(当前讯友录数量) = 黑域(I)
                        文本写入器.Write(生成域名html(黑域(I), 当前讯友录数量))
                        当前讯友录数量 += 1
                        Exit For
                    End If
                Next
                For I = 0 To 黑域.Length - 1
                    If String.Compare(黑域(I).英语, 黑域_全部) <> 0 Then
                        当前讯友录(当前讯友录数量) = 黑域(I)
                        文本写入器.Write(生成域名html(黑域(I), 当前讯友录数量))
                        当前讯友录数量 += 1
                    End If
                Next
            End If
        ElseIf 范围 = 讯友录显示范围_常量集合.白域 Then
            If 当前用户.白域 IsNot Nothing Then
                Dim 白域() As 域名_复合数据 = 当前用户.白域
                ReDim 当前讯友录(白域.Length - 1)
                For I = 0 To 白域.Length - 1
                    当前讯友录(当前讯友录数量) = 白域(I)
                    文本写入器.Write(生成域名html(白域(I), 当前讯友录数量))
                    当前讯友录数量 += 1
                Next
            End If
        Else
            GoTo 全部
        End If
        文本写入器.Close()
        If 当前讯友录数量 > 0 Then
            If 当前讯友录数量 < 当前讯友录.Length Then ReDim Preserve 当前讯友录(当前讯友录数量 - 1)
        Else
            当前讯友录 = Nothing
        End If
        Try
            If 变长文本.Length > 0 Then
                With 当前聊天控件.聊天对象
                    If .小聊天群 IsNot Nothing Then
                        浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("LoadContactList('" & 获取范围的名称(范围, 标签名称) & "', """ & 文本写入器.ToString & """, '" & 获取id(.讯友或群主.英语讯宝地址, .小聊天群.编号) & "');")
                    ElseIf .大聊天群 IsNot Nothing Then
                        浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("LoadContactList('" & 获取范围的名称(范围, 标签名称) & "', """ & 文本写入器.ToString & """, '" & 获取id(.大聊天群.子域名, .大聊天群.编号) & "');")
                    End If
                End With
            Else
                浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("LoadContactList('" & 获取范围的名称(范围, 标签名称) & "');")
            End If
        Catch ex As Exception
        End Try
        当前用户.讯友录当前显示范围 = 范围
        If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.某标签 Then
            当前用户.讯友录当前显示标签 = 标签名称
        Else
            当前用户.讯友录当前显示标签 = Nothing
        End If
    End Sub

    Private Function 数据库_获取互动讯友排名(ByRef 最近讯友() As 最近讯友_复合数据, ByRef 最近讯友数量 As Integer) As Boolean
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"地址或域名", "群编号", "新讯宝数量"})
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "最近", Nothing, , 列添加器, , "#时间")
            读取器 = 指令.执行()
            While 读取器.读取
                If 最近讯友.Length = 最近讯友数量 Then ReDim Preserve 最近讯友(最近讯友数量 * 2 - 1)
                With 最近讯友(最近讯友数量)
                    .英语讯宝地址 = 读取器(0)
                    .群编号 = 读取器(1)
                    .新讯宝数量 = 读取器(2)
                End With
                最近讯友数量 += 1
            End While
            读取器.关闭()
            Return True
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            主控机器人.说(ex.Message)
            Return False
        End Try
    End Function

    Private Function 生成讯友html(ByVal 讯友 As 类_讯友, ByVal 索引 As Integer) As String
        Dim 备注 As String = ""
        If 显示讯友临时编号 Then
            备注 &= "(" & 讯友.临时编号 & ")&nbsp;"
        End If
        If 讯友.新讯宝数量 > 0 Then
            备注 &= "<div class='RedNotify' style='display:inline-block;'>" & 讯友.新讯宝数量 & "</div>"
        Else
            备注 &= "<div class='RedNotify' style='display:none;'></div>"
        End If
        If String.IsNullOrEmpty(讯友.备注) = False Then
            备注 &= 讯友.备注
        End If
        Dim 地址 As String
        If String.IsNullOrEmpty(讯友.本国语讯宝地址) = False Then
            地址 = 讯友.本国语讯宝地址 & "<br>" & 讯友.英语讯宝地址
        Else
            地址 = 讯友.英语讯宝地址
        End If
        Return "<div id='c" & 索引 & "' onclick='ClickAContact(\""c" & 索引 & "\"")' onmouseover='OnMouseOver(\""c" & 索引 & "\"")' onmouseout='OnMouseOut(\""c" & 索引 & "\"")'><table><tr><td class='td_SSicon' valign='top'><img class='SSicon' src='" & 获取讯友头像路径(讯友.英语讯宝地址, 讯友.主机名, 讯友.头像更新时间) & "'/></td><td valign='top' class='Contact'>" & 备注 & "<br><span class='Address'>" + 地址 + "</span></td></tr></table></div>"
    End Function

    Private Function 生成小聊天群html(ByVal 群 As 类_聊天群_小, ByVal 索引 As Integer) As String
        With 群.群主
            Dim 名称 As String = 界面文字.获取(95, "[小群] #%", New Object() {群.备注})
            Dim 备注 As String
            If 群.新讯宝数量 > 0 Then
                备注 = "<div class='RedNotify' style='display:inline-block;'>" & 群.新讯宝数量 & "</div>&nbsp;" & 名称
            Else
                备注 = "<div class='RedNotify' style='display:none;'></div>" & 名称
            End If
            Dim 地址 As String
            If String.IsNullOrEmpty(.本国语讯宝地址) = False Then
                地址 = .本国语讯宝地址 & "<br>" & .英语讯宝地址
            Else
                地址 = .英语讯宝地址
            End If
            Return "<div id='s" & 索引 & "' onclick='ClickAContact(\""s" & 索引 & "\"")' onmouseover='OnMouseOver(\""s" & 索引 & "\"")' onmouseout='OnMouseOut(\""s" & 索引 & "\"")'><table><tr><td class='td_SSicon' valign='top'><img class='SSicon' src='" & 获取讯友头像路径(.英语讯宝地址, .主机名, .头像更新时间) & "'/></td><td valign='top' class='Contact'>" & 备注 & "<br><span class='Address'>" + 地址 + "</span></td></tr></table></div>"
        End With
    End Function

    Private Function 生成大聊天群html(ByVal 群 As 类_聊天群_大, ByVal 索引 As Integer) As String
        Dim 名称 As String = 界面文字.获取(275, "[大群] #%", New Object() {群.名称})
        Dim 备注 As String
        If 群.新讯宝数量 > 0 Then
            备注 = "<div class='RedNotify' style='display:inline-block;'>" & 群.新讯宝数量 & "</div>&nbsp;" & 名称
        Else
            备注 = "<div class='RedNotify' style='display:none;'></div>" & 名称
        End If
        Dim 子域名 As String = 群.主机名 & "." & 群.英语域名
        If String.IsNullOrEmpty(群.本国语域名) = False Then 子域名 &= "/" & 群.本国语域名
        Return "<div id='l" & 索引 & "' onclick='ClickAContact(\""l" & 索引 & "\"")' onmouseover='OnMouseOver(\""l" & 索引 & "\"")' onmouseout='OnMouseOut(\""l" & 索引 & "\"")'><table><tr><td class='td_SSicon' valign='top'><img class='SSicon' src='" & 获取大聊天群图标路径(群.子域名, 群.编号, 当前用户.域名_英语, 群.图标更新时间) & "'/></td><td valign='top' class='Contact'>" & 备注 & "<br><span class='Address'>" + 子域名 + "</span></td></tr></table></div>"
    End Function

    Private Function 生成域名html(ByVal 某一域名 As 域名_复合数据, ByVal 索引 As Integer) As String
        Dim 域名 As String
        If String.Compare(某一域名.英语, 黑域_全部) = 0 Then
            域名 = 界面文字.获取(236, "所有域") & "<br>" & 黑域_全部
        Else
            If String.IsNullOrEmpty(某一域名.本国语) = False Then
                域名 = 某一域名.本国语 & "<br>" & 某一域名.英语
            Else
                域名 = 某一域名.英语
            End If
        End If
        Return "<div id='d" & 索引 & "' onclick='ClickAContact(\""d" & 索引 & "\"")' onmouseover='OnMouseOver(\""d" & 索引 & "\"")' onmouseout='OnMouseOut(\""d" & 索引 & "\"")'><table><tr><td valign='top'>" & 域名 & "</td></tr></table></div>"
    End Function

    Private Function 获取范围的名称(ByVal 范围 As 讯友录显示范围_常量集合, ByVal 标签名称 As String) As String
        Select Case 范围
            Case 讯友录显示范围_常量集合.最近 : Return 界面文字.获取(73, "最近")
            Case 讯友录显示范围_常量集合.讯友 : Return 界面文字.获取(74, "讯友")
            Case 讯友录显示范围_常量集合.聊天群 : Return 界面文字.获取(86, "聊天群")
            Case 讯友录显示范围_常量集合.某标签 : Return 界面文字.获取(264, "#% 标签", New String() {替换HTML和JS敏感字符(标签名称)})
            Case 讯友录显示范围_常量集合.黑名单 : Return 界面文字.获取(75, "黑名单")
            Case 讯友录显示范围_常量集合.黑域 : Return 界面文字.获取(49, "黑域")
            Case 讯友录显示范围_常量集合.白域 : Return 界面文字.获取(237, "白域")
            Case Else : Return 界面文字.获取(74, "讯友")
        End Select
    End Function

    Friend Sub 关闭聊天控件(ByVal id As String, ByVal 群编号 As Long)
        If String.Compare(机器人id_主控, id) = 0 Then Return
        If 当前聊天控件.聊天对象.小聊天群 IsNot Nothing Then
            If String.Compare(当前聊天控件.聊天对象.讯友或群主.英语讯宝地址, 机器人id_主控) <> 0 Then
                显示主控机器人聊天窗体()
            End If
        Else
            显示主控机器人聊天窗体()
        End If
        If 聊天控件数 > 1 Then
            Dim I As Integer
跳转点1:
            If id.Contains(讯宝地址标识) Then
                For I = 0 To 聊天控件数 - 1
                    With 聊天控件(I).聊天对象
                        If .小聊天群 IsNot Nothing Then
                            If .小聊天群.编号 = 群编号 AndAlso String.Compare(.讯友或群主.英语讯宝地址, id) = 0 Then
                                Exit For
                            End If
                        End If
                    End With
                Next
            Else
                For I = 0 To 聊天控件数 - 1
                    With 聊天控件(I).聊天对象
                        If .大聊天群 IsNot Nothing Then
                            If .大聊天群.编号 = 群编号 AndAlso String.Compare(.大聊天群.子域名, id) = 0 Then
                                Exit For
                            End If
                        End If
                    End With
                Next
            End If
            If I < 聊天控件数 Then
                聊天控件(I).Dispose()
                If I < 聊天控件数 - 1 Then
                    Dim J As Integer
                    For J = I To 聊天控件数 - 2
                        聊天控件(J) = 聊天控件(J + 1)
                    Next
                End If
                聊天控件数 -= 1
                GoTo 跳转点1
            End If
        End If
    End Sub

    Friend Sub 注销成功()
        定时器_心跳.Stop()
        If 备份管理器 IsNot Nothing Then
            备份管理器.停止()
            备份管理器 = Nothing
        End If
        当前用户 = New 类_用户
        当前用户.主控机器人 = 主控机器人
        My.Settings.UserID = 0
        My.Settings.Credential = ""
        My.Settings.Domain = ""
        My.Settings.Save()
        主控机器人.关闭与传送服务器的连接()
        主控机器人.从未自检 = True
        主控机器人.不再提示 = False
        主控机器人.心跳确认时间 = 0
        刷新讯友录(讯友录显示范围_常量集合.讯友)
        If 线程_发送讯宝 IsNot Nothing Then
            Try
                线程_发送讯宝.Abort()
            Catch ex As Exception
            End Try
            线程_发送讯宝 = Nothing
        End If
        正在发送的讯宝 = Nothing
        If 副数据库 IsNot Nothing Then
            副数据库.关闭()
            副数据库 = Nothing
        End If
        Dim I As Integer
        For I = 0 To 聊天控件数 - 1
            If TypeOf 聊天控件(I).机器人 IsNot 类_机器人_主控 Then
                聊天控件(I).Dispose()
            Else
                当前聊天控件 = 聊天控件(I)
            End If
        Next
        ReDim 聊天控件(4)
        聊天控件(0) = 当前聊天控件
        聊天控件数 = 1
        系统管理机器人 = Nothing
        当前聊天控件.注销时清除主控机器人聊天内容()
        当前聊天控件.Visible = True
        CType(当前聊天控件.机器人, 类_机器人_主控).从未自检 = True
        If 下载文件的窗体 IsNot Nothing Then
            下载文件的窗体.关闭 = True
            下载文件的窗体.Close()
            下载文件的窗体 = Nothing
        End If
        浏览器_讯友录.Reload
    End Sub

    Friend Sub 显示隐藏系统管理机器人()
        If String.IsNullOrEmpty(当前用户.职能) = False Then
            Dim html As String = "<div class='RedNotify' style='display:none;'></div>" & 界面文字.获取(133, "系统管理机器人") & "<br><span class='Address'>" & 界面文字.获取(113, "管理用户、聊天群和服务器") & "</span>"
            浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("ShowRobot0(""" & html & """);")
        Else
            浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("HideRobot0();")
            关闭聊天控件(机器人id_系统管理, 0)
        End If
    End Sub

    Private Sub 数据库_更新新讯宝数量(ByVal 地址或域名 As String, ByVal 群编号 As Long, ByVal 数量 As Short)
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("新讯宝数量", 数量)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("地址或域名", 筛选方式_常量集合.等于, 地址或域名)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_更新数据(副数据库, "最近", 列添加器_新数据, 筛选器, "#地址群编号")
            指令2.执行()
        Catch ex As Exception
            主控机器人.说(ex.Message)
        End Try
    End Sub

    Private Function 获取id(ByVal 讯宝地址或域名 As String, Optional ByVal 群编号 As Long = 0) As String
        If 当前讯友录 Is Nothing Then Return ""
        Dim I As Integer
        If 讯宝地址或域名.Contains(讯宝地址标识) Then
            If 群编号 = 0 Then
                For I = 0 To 当前讯友录.Length - 1
                    If TypeOf 当前讯友录(I) Is 类_讯友 Then
                        With CType(当前讯友录(I), 类_讯友)
                            If String.Compare(讯宝地址或域名, .英语讯宝地址) = 0 Then
                                Return "c" & I
                            End If
                        End With
                    End If
                Next
            Else
                For I = 0 To 当前讯友录.Length - 1
                    If TypeOf 当前讯友录(I) Is 类_聊天群_小 Then
                        With CType(当前讯友录(I), 类_聊天群_小)
                            If String.Compare(讯宝地址或域名, .群主.英语讯宝地址) = 0 AndAlso
                            群编号 = .编号 Then
                                Return "s" & I
                            End If
                        End With
                    End If
                Next
            End If
        Else
            For I = 0 To 当前讯友录.Length - 1
                If TypeOf 当前讯友录(I) Is 类_聊天群_大 Then
                    With CType(当前讯友录(I), 类_聊天群_大)
                        If String.Compare(讯宝地址或域名, .子域名) = 0 AndAlso
                            群编号 = .编号 Then
                            Return "l" & I
                        End If
                    End With
                End If
            Next
        End If
        Return ""
    End Function

    Friend Sub 发送者撤回讯宝(ByVal 发送者英语讯宝地址 As String, ByVal 群编号 As Byte,
                                            ByVal 群主英语讯宝地址 As String, ByVal 发送序号 As Long, ByVal 发送时间 As Long)
        Dim I As Integer
        If 群编号 = 0 Then
            For I = 0 To 聊天控件数 - 1
                With 聊天控件(I).聊天对象
                    If .小聊天群 IsNot Nothing Then
                        If .小聊天群.编号 = 0 AndAlso String.Compare(.讯友或群主.英语讯宝地址, 发送者英语讯宝地址) = 0 Then
                            Exit For
                        End If
                    End If
                End With
            Next
        Else
            For I = 0 To 聊天控件数 - 1
                With 聊天控件(I).聊天对象
                    If .小聊天群 IsNot Nothing Then
                        If .小聊天群.编号 = 群编号 AndAlso String.Compare(.讯友或群主.英语讯宝地址, 群主英语讯宝地址) = 0 Then
                            Exit For
                        End If
                    End If
                End With
            Next
        End If
        If I < 聊天控件数 Then
            聊天控件(I).发送者撤回(发送者英语讯宝地址, 发送序号, 发送时间)
        Else
            数据库_撤回讯宝(发送者英语讯宝地址, 群编号, 群主英语讯宝地址, 发送序号, 发送时间)
        End If
    End Sub

    Friend Sub 备份时出现故障(ByVal 文本 As String)
        If 系统管理机器人 IsNot Nothing Then
            If Not 系统管理机器人.聊天控件.Equals(当前聊天控件) Then
                With 系统管理机器人.聊天控件.聊天对象.讯友或群主
                    If .新讯宝数量 < 999 Then
                        .新讯宝数量 += 1
                        浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("NewSSNumber('" & 机器人id_系统管理 & "', '" & .新讯宝数量 & "');")
                    End If
                End With
            End If
            系统管理机器人.说(文本)
        Else
            浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("ClickAContact('" & 机器人id_系统管理 & "');")
            备份异常信息 = 文本
        End If
    End Sub

    Private Sub 主窗体_LocationChanged(sender As Object, e As EventArgs) Handles Me.LocationChanged
        If Me.Visible = True AndAlso Me.WindowState = FormWindowState.Normal Then
            My.Settings.WindowLeft = Me.Left
            My.Settings.WindowTop = Me.Top
        End If
    End Sub

    Private Sub 录音类_录音完毕(sender As Object, ByVal 文件路径 As String) Handles 录音类.录音完毕
        If 当前录音控件 IsNot Nothing Then
            当前录音控件.录音完毕(文件路径)
        ElseIf File.Exists(文件路径) Then
            Try
                File.Delete(文件路径)
            Catch ex As Exception
            End Try
        End If
    End Sub

    Friend Sub 播放语音(ByVal 聊天控件 As 控件_聊天, ByVal 文件路径 As String, ByVal VoiceID As String, ByVal IsNew As String)
        If 播音类 Is Nothing Then
            播音类 = New 类_音频_播放
        ElseIf 播音类.正在播放 = True Then
            If String.Compare(文件路径, 播音类.原始路径) = 0 Then
                播音类.停止播放()
                Return
            Else
                播音类.停止播放(True)
                If 聊天控件.Equals(当前聊天控件) = False Then
                    播音类_播放完毕(播音类)
                End If
            End If
        End If
        If 播音类.开始播放AMR(文件路径) = True Then
            聊天控件.播音开始(VoiceID)
            当前播音控件 = 聊天控件
            If String.Compare(IsNew, "true", True) = 0 Then
                数据库_标为已收听(聊天控件, VoiceID)
            End If
        End If
    End Sub

    Private Sub 播音类_播放完毕(sender As Object) Handles 播音类.播放完毕
        If 当前播音控件 IsNot Nothing Then 当前播音控件.播音完毕()
    End Sub

    Private Sub 数据库_标为已收听(ByVal 聊天控件 As 控件_聊天, ByVal VoiceID As String)
        Dim 段() As String = VoiceID.Split(New String() {":"}, StringSplitOptions.RemoveEmptyEntries)
        If 段.Length <> 3 Then Return
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("已收听", True)
            Dim 指令2 As 类_数据库指令_更新数据
            With 聊天控件.聊天对象
                If .小聊天群 IsNot Nothing Then
                    If 聊天控件.聊天对象.小聊天群.编号 = 0 Then
                        Dim 列添加器 As New 类_列添加器
                        列添加器.添加列_用于筛选器("讯宝地址", 筛选方式_常量集合.等于, 段(1))
                        列添加器.添加列_用于筛选器("是接收者", 筛选方式_常量集合.等于, False)
                        列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, Long.Parse(段(2)))
                        Dim 筛选器 As New 类_筛选器
                        筛选器.添加一组筛选条件(列添加器)
                        指令2 = New 类_数据库指令_更新数据(副数据库, "一对一讯宝", 列添加器_新数据, 筛选器, "#地址是接收者发送序号")
                        指令2.执行()
                    Else
                        Dim 列添加器 As New 类_列添加器
                        列添加器.添加列_用于筛选器("群主讯宝地址", 筛选方式_常量集合.等于, .讯友或群主.英语讯宝地址)
                        列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, .小聊天群.编号)
                        列添加器.添加列_用于筛选器("发送者讯宝地址", 筛选方式_常量集合.等于, 段(1))
                        列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, Long.Parse(段(2)))
                        Dim 筛选器 As New 类_筛选器
                        筛选器.添加一组筛选条件(列添加器)
                        指令2 = New 类_数据库指令_更新数据(副数据库, "小聊天群讯宝", 列添加器_新数据, 筛选器, "#群主编号发送者发送序号")
                        指令2.执行()
                    End If
                ElseIf .大聊天群 IsNot Nothing Then
                    Dim 列添加器 As New 类_列添加器
                    列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, .大聊天群.子域名)
                    列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, .大聊天群.编号)
                    列添加器.添加列_用于筛选器("发送时间", 筛选方式_常量集合.等于, Long.Parse(段(2)))
                    列添加器.添加列_用于筛选器("发送者讯宝地址", 筛选方式_常量集合.等于, 段(1))
                    Dim 筛选器 As New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    指令2 = New 类_数据库指令_更新数据(副数据库, "大聊天群讯宝", 列添加器_新数据, 筛选器, "#子域名群编号发送时间")
                    指令2.执行()
                End If
            End With
        Catch ex As Exception
        End Try
    End Sub

    Private Sub 检查大聊天群是否有新消息()
        Dim 加入的大聊天群() As 类_聊天群_大 = 当前用户.加入的大聊天群
        Dim 大聊天群服务器(加入的大聊天群.Length - 1) As 类_大聊天群服务器
        Dim I, J, 大聊天群服务器数量 As Integer
        Dim 子域名 As String
        For I = 0 To 加入的大聊天群.Length - 1
            With 加入的大聊天群(I)
                If .最新讯宝的发送时间 = 0 Then
                    Call 数据库_获取最新讯宝的发送时间(.子域名, .编号, .最新讯宝的发送时间)
                    If .最新讯宝的发送时间 = 0 Then .最新讯宝的发送时间 = 1
                End If
                If .检查时间 = 0 OrElse DateDiff(DateInterval.Minute, Date.FromBinary(.检查时间), Date.Now) >= 10 Then
                    子域名 = .子域名
                    If 大聊天群服务器数量 > 0 Then
                        For J = 0 To 大聊天群服务器数量 - 1
                            If String.Compare(子域名, 大聊天群服务器(J).子域名) = 0 Then Exit For
                        Next
                        If J = 大聊天群服务器数量 Then
                            大聊天群服务器(大聊天群服务器数量) = New 类_大聊天群服务器(子域名)
                            大聊天群服务器数量 += 1
                        End If
                    Else
                        大聊天群服务器(0) = New 类_大聊天群服务器(子域名)
                        大聊天群服务器数量 = 1
                    End If
                End If
            End With
        Next
        If 大聊天群服务器数量 > 0 Then
            For I = 0 To 大聊天群服务器数量 - 1
                子域名 = 大聊天群服务器(I).子域名
                For J = 0 To 加入的大聊天群.Length - 1
                    If String.Compare(子域名, 加入的大聊天群(J).子域名) = 0 Then
                        With 加入的大聊天群(J)
                            If String.IsNullOrEmpty(.连接凭据) = False Then
                                Dim SS包生成器 As New 类_SS包生成器()
                                Call 添加数据_检查大聊天群新讯宝数量(SS包生成器, 子域名, 加入的大聊天群)
                                启动HTTPS访问线程(New 类_访问设置(获取大聊天群服务器访问路径开头(子域名, False) & "C=CheckNewSS&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&Credential=" & 替换URI敏感字符(.连接凭据), , SS包生成器.生成SS包, 大聊天群服务器(I)))
                            Else
                                大聊天群服务器(I).无连接凭据 = True
                                Dim SS包生成器 As New 类_SS包生成器()
                                SS包生成器.添加_有标签("发送序号", 当前用户.讯宝发送序号)
                                SS包生成器.添加_有标签("子域名", 子域名)
                                SS包生成器.添加_有标签("群编号", .编号)
                                启动HTTPS访问线程(New 类_访问设置(获取传送服务器访问路径开头(当前用户.主机名, 当前用户.域名_英语, False) & "C=JoinLargeGroup&UserID=" & 当前用户.编号 & "&Position=" & 当前用户.位置号 & "&DeviceType=" & 设备类型_电脑, 20000, SS包生成器.生成SS包(当前用户.AES加密器), 大聊天群服务器(I)))
                            End If
                        End With
                        Exit For
                    End If
                Next
            Next
        End If
    End Sub

    Private Sub 数据库_获取最新讯宝的发送时间(ByVal 服务器子域名 As String, ByVal 群编号 As Long, ByRef 发送时间 As Long)
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 服务器子域名)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("发送时间")
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "大聊天群讯宝", 筛选器, 1, 列添加器, , "#子域名群编号发送时间")
            读取器 = 指令.执行()
            While 读取器.读取
                发送时间 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
        End Try
    End Sub

    Private Sub 启动HTTPS访问线程(ByVal 访问设置 As 类_访问设置)
        访问设置.大聊天群服务器.线程 = New Thread(New ParameterizedThreadStart(AddressOf HTTPS访问))
        访问设置.大聊天群服务器.线程.Start(访问设置)
    End Sub

    Private Sub HTTPS访问(ByVal 参数 As Object)
        Dim 访问设置 As 类_访问设置 = 参数
        Dim 重试次数 As Integer
        Dim 收到的字节数组() As Byte
        Dim 收到的字节数, 收到的总字节数 As Integer
重试:
        收到的总字节数 = 0
        收到的字节数组 = Nothing
        Try
            Dim HTTP网络请求 As HttpWebRequest = WebRequest.Create(访问设置.路径)
            HTTP网络请求.Method = "POST"
            HTTP网络请求.Timeout = 访问设置.收发时限_毫秒
            If 访问设置.字节数组 Is Nothing Then
                HTTP网络请求.ContentType = "text/xml"
                HTTP网络请求.ContentLength = 0
            Else
                HTTP网络请求.ContentType = "application/octet-stream"
                HTTP网络请求.ContentLength = 访问设置.字节数组.Length
                Dim 流 As Stream = HTTP网络请求.GetRequestStream
                流.Write(访问设置.字节数组, 0, 访问设置.字节数组.Length)
                流.Close()
            End If
            Using HTTP网络回应 As HttpWebResponse = HTTP网络请求.GetResponse
                If HTTP网络回应.ContentLength > 0 Then
                    ReDim 收到的字节数组(HTTP网络回应.ContentLength - 1)
                    Dim 输入流 As Stream = HTTP网络回应.GetResponseStream
继续:
                    收到的字节数 = 输入流.Read(收到的字节数组, 收到的总字节数, 收到的字节数组.Length - 收到的总字节数)
                    If 收到的字节数 > 0 Then
                        收到的总字节数 += 收到的字节数
                        If 收到的总字节数 < 收到的字节数组.Length Then
                            GoTo 继续
                        End If
                    End If
                End If
            End Using
        Catch ex As Exception
            If 重试次数 < 2 Then
                重试次数 += 1
                GoTo 重试
            Else
                Return
            End If
        End Try
        If 收到的字节数组 IsNot Nothing Then
            If 收到的总字节数 = 收到的字节数组.Length Then
                Call HTTPS请求成功(访问设置.大聊天群服务器, 收到的字节数组)
            End If
        End If
    End Sub

    Private Sub HTTPS请求成功(ByVal 大聊天群服务器 As 类_大聊天群服务器, ByVal SS包() As Byte)
        Dim SS包解读器 As 类_SS包解读器
        Try
            SS包解读器 = New 类_SS包解读器(SS包)
            If SS包解读器.查询结果 = 查询结果_常量集合.成功 Then
                If 大聊天群服务器.无连接凭据 = False Then
                    Dim SS包解读器2() As Object = SS包解读器.读取_重复标签("GP")
                    Dim 加入的大聊天群() As 类_聊天群_大
                    Dim I, J As Integer
                    If SS包解读器2 IsNot Nothing Then
                        Dim 群编号 As Long
                        Dim 新讯宝数量 As Integer
                        Dim 有新讯宝 As Boolean
                        For I = 0 To SS包解读器2.Length - 1
                            With CType(SS包解读器2(I), 类_SS包解读器)
                                .读取_有标签("GI", 群编号, 0)
                                .读取_有标签("SN", 新讯宝数量, 0)
                            End With
                            If 新讯宝数量 > 0 Then
                                数据库_更新最近互动讯友排名(大聊天群服务器.子域名, 群编号)
                                数据库_更新新讯宝数量(大聊天群服务器.子域名, 群编号, 新讯宝数量)
                                加入的大聊天群 = 当前用户.加入的大聊天群
                                For J = 0 To 加入的大聊天群.Length - 1
                                    With 加入的大聊天群(J)
                                        If String.Compare(.子域名, 大聊天群服务器.子域名) = 0 AndAlso .编号 = 群编号 Then
                                            .新讯宝数量 = 新讯宝数量
                                            Exit For
                                        End If
                                    End With
                                Next
                                刷新讯友录(讯友录显示范围_常量集合.最近)
                                浏览器_讯友录.GetMainFrame.ExecuteJavaScriptAsync("NewSSNumber('" & 获取id(大聊天群服务器.子域名, 群编号) & "', '" & 新讯宝数量 & "');")
                                If 有新讯宝 = False Then 有新讯宝 = True
                            End If
                        Next
                        If 有新讯宝 Then
                            Dim 音频播放器 As New 类_音频_播放
                            音频播放器.开始播放本地MP3("contact.mp3")
                        End If
                    End If
                    加入的大聊天群 = 当前用户.加入的大聊天群
                    Dim 检查时间 As Long = Date.Now.Ticks
                    For I = 0 To 加入的大聊天群.Length - 1
                        With 加入的大聊天群(I)
                            If String.Compare(.子域名, 大聊天群服务器.子域名) = 0 Then
                                .检查时间 = 检查时间
                            End If
                        End With
                    Next
                Else
                    Dim 子域名 As String = Nothing
                    Dim 连接凭据 As String = Nothing
                    SS包解读器.读取_有标签("子域名", 子域名)
                    SS包解读器.读取_有标签("连接凭据", 连接凭据)
                    If String.Compare(大聊天群服务器.子域名, 子域名) <> 0 Then Return
                    Dim 加入的大聊天群() As 类_聊天群_大 = 当前用户.加入的大聊天群
                    Dim I As Integer
                    For I = 0 To 加入的大聊天群.Length - 1
                        With 加入的大聊天群(I)
                            If String.Compare(.子域名, 子域名) = 0 Then
                                If String.IsNullOrEmpty(.连接凭据) = False Then
                                    .连接凭据 = 连接凭据
                                End If
                            End If
                        End With
                    Next
                    大聊天群服务器.无连接凭据 = False
                    Dim SS包生成器 As New 类_SS包生成器()
                    Call 添加数据_检查大聊天群新讯宝数量(SS包生成器, 子域名, 加入的大聊天群)
                    启动HTTPS访问线程(New 类_访问设置(获取大聊天群服务器访问路径开头(大聊天群服务器.子域名, False) & "C=CheckNewSS&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&Credential=" & 替换URI敏感字符(连接凭据), , SS包生成器.生成SS包, 大聊天群服务器))
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

    Friend Sub 访问网页(ByVal 链接 As String)
        If 浏览器窗体 Is Nothing Then
            浏览器窗体 = New 窗体_浏览器(Me, 链接)
            浏览器窗体.Show()
        Else
            If 浏览器窗体.WindowState = FormWindowState.Minimized Then
                浏览器窗体.WindowState = FormWindowState.Maximized
            End If
            浏览器窗体.BringToFront()
            浏览器窗体.打开链接(链接)
        End If
    End Sub

    Private Sub 浏览器窗体_FormClosed(sender As Object, e As FormClosedEventArgs) Handles 浏览器窗体.FormClosed
        浏览器窗体 = Nothing
    End Sub

    Friend Sub 下载文件(ByVal 下载路径 As String)
        Dim I As Integer = 下载路径.LastIndexOf("/")
        If I < 0 Then Return
        If I < 下载路径.Length - 1 Then
            文件保存对话框.FileName = 下载路径.Substring(I + 1)
        Else
            文件保存对话框.FileName = ""
        End If
        If 文件保存对话框.ShowDialog(Me) = DialogResult.OK Then
            If 下载文件的窗体 Is Nothing Then
                下载文件的窗体 = New 窗体_下载文件(下载路径, 文件保存对话框.FileName)
                下载文件的窗体.Show()
            Else
                下载文件的窗体.新下载任务(下载路径, 文件保存对话框.FileName)
                If 下载文件的窗体.WindowState = FormWindowState.Minimized Then
                    下载文件的窗体.WindowState = FormWindowState.Normal
                ElseIf 下载文件的窗体.Visible = False Then
                    下载文件的窗体.Show()
                Else
                    下载文件的窗体.BringToFront()
                End If
            End If
        End If
    End Sub

    Friend Sub 小聊天群成员有变化(ByVal 群主英语讯宝地址 As String, ByVal 群编号 As Byte)
        Dim I As Integer
        For I = 0 To 聊天控件数 - 1
            With 聊天控件(I).聊天对象
                If .小聊天群 IsNot Nothing Then
                    If .小聊天群.编号 = 群编号 AndAlso String.Compare(.讯友或群主.英语讯宝地址, 群主英语讯宝地址) = 0 Then
                        Exit For
                    End If
                End If
            End With
        Next
        If I < 聊天控件数 Then
            聊天控件(I).加载小聊天群的成员列表()
        End If
    End Sub

End Class
