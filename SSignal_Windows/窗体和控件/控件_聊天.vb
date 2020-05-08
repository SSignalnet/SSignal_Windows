Imports System.IO
Imports System.Text
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports SSignal_Protocols
Imports CefSharp.WinForms
Imports CefSharp

Public Class 控件_聊天

    Private Structure 话语_复合数据
        Dim 文本 As String
        Dim 时间 As Long
    End Structure

    Private Structure 讯宝_复合数据
        Dim 收发者讯宝地址, 文本 As String
        Dim 是接收者, 已收听 As Boolean
        Dim 讯宝指令 As 讯宝指令_常量集合
        Dim 文本编号, 发送序号, 发送时间, 存储时间 As Long
        Dim 文本库号, 宽度, 高度 As Short
        Dim 秒数 As Byte
    End Structure

    Friend WithEvents 浏览器_聊天内容 As ChromiumWebBrowser
    Friend WithEvents 浏览器_小宇宙 As ChromiumWebBrowser
    Private WithEvents 进度条 As 控件_进度条

    Friend 主窗体1 As 主窗体
    Friend 聊天对象 As 类_聊天对象
    Friend 机器人 As 类_机器人
    Friend 不活动秒数 As Short
    Dim 起始时刻 As Long
    Friend 发送语音, 取消录音, 载入了陌生人讯宝 As Boolean
    Dim 录音剩余秒数 As Single

    Private Delegate Sub 页面加载完毕_跨线程()
    Private Delegate Sub 收到子域名_跨线程(ByVal 子域名 As String)

    Friend Sub New()
        InitializeComponent()
    End Sub

    Friend Sub New(ByVal 主窗体2 As 主窗体, ByVal 聊天对象1 As 类_聊天对象)
        InitializeComponent()
        进度条 = New 控件_进度条
        进度条.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        进度条.BorderStyle = BorderStyle.FixedSingle
        进度条.Location = 输入框.Location
        进度条.Size = 输入框.Size
        进度条.TabIndex = 6
        进度条.Visible = False
        进度条.当前值 = 0
        进度条.无边框 = False
        进度条.最大值 = 60
        Me.Controls.Add(进度条)

        主窗体1 = 主窗体2
        聊天对象 = 聊天对象1
        Dim 本程序路径 As String = My.Application.Info.DirectoryPath
        If 本程序路径.EndsWith("\") = False Then 本程序路径 &= "\"
        浏览器_聊天内容 = New ChromiumWebBrowser("file://" & 本程序路径.Replace("\", "/") & "Chat.html") With {
            .Dock = DockStyle.Fill
        }
        浏览器_聊天内容.MenuHandler = New MenuHandler
        浏览器_聊天内容.LifeSpanHandler = New LifeSpanHandler(主窗体1)
        浏览器_聊天内容.DownloadHandler = New DownloadHandler(主窗体1)
        聊天内容的容器.Controls.Add(浏览器_聊天内容)
        Dim 绑定设置 As BindingOptions = BindingOptions.DefaultBinder
        绑定设置.CamelCaseJavascriptNames = False
        浏览器_聊天内容.RegisterJsObject("external", New JS接口_聊天(Me), 绑定设置)
        Dim 初始页面的路径 As String
        If 聊天对象.小聊天群 IsNot Nothing Then
            Select Case 聊天对象.讯友或群主.英语讯宝地址
                Case 机器人id_主控
                    If 当前用户.已登录() Then
                        If String.IsNullOrEmpty(当前用户.英语用户名) = False Then
                            初始页面的路径 = 获取当前用户小宇宙的访问路径(当前用户.英语用户名, 当前用户.域名_英语)
                        Else
                            初始页面的路径 = 获取主站首页的访问路径()
                        End If
                    Else
                        初始页面的路径 = 获取主站首页的访问路径()
                    End If
                Case 机器人id_系统管理
                    初始页面的路径 = 获取系统管理页面的访问路径(当前用户.域名_英语)
                Case Else
                    初始页面的路径 = 获取讯友小宇宙的访问路径(聊天对象.讯友或群主.英语讯宝地址, 当前用户.英语讯宝地址)
            End Select
        Else
            初始页面的路径 = "about:blank"
        End If
        浏览器_小宇宙 = New ChromiumWebBrowser(初始页面的路径) With {
            .Dock = DockStyle.Fill
        }
        浏览器_小宇宙.MenuHandler = New MenuHandler
        浏览器_小宇宙.LifeSpanHandler = New LifeSpanHandler(主窗体1)
        浏览器_小宇宙.DownloadHandler = New DownloadHandler(主窗体1)
        小宇宙的容器.Controls.Add(浏览器_小宇宙)
        Dim 绑定设置2 As BindingOptions = BindingOptions.DefaultBinder
        绑定设置2.CamelCaseJavascriptNames = False
        浏览器_小宇宙.RegisterJsObject("external", New JS接口_小宇宙(Me), 绑定设置2)
    End Sub

    Private Sub 控件_聊天_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        下拉列表_任务.Items.Add(界面文字.获取(13, "对机器人说"))
        If 聊天对象.小聊天群 IsNot Nothing Then
            按钮_说话.Text = 界面文字.获取(1, 按钮_说话.Text)
        ElseIf 聊天对象.大聊天群 IsNot Nothing Then
            按钮_说话.Text = 界面文字.获取(2, "刷新")
        End If
        输入框.MaxLength = 最大值_常量集合.讯宝文本长度
    End Sub

    Private Sub 浏览器_聊天内容_FrameLoadEnd(sender As Object, e As FrameLoadEndEventArgs) Handles 浏览器_聊天内容.FrameLoadEnd
        If e.Frame.IsMain Then 页面加载完毕()
    End Sub

    Private Sub 浏览器_小宇宙_FrameLoadEnd(sender As Object, e As FrameLoadEndEventArgs) Handles 浏览器_小宇宙.FrameLoadEnd
        If e.Frame.IsMain Then
            If TypeOf 机器人 Is 类_机器人_小聊天群 Then
                Call 加载小聊天群的成员列表()
            ElseIf TypeOf 机器人 Is 类_机器人_大聊天群 Then
                With 聊天对象.大聊天群
                    If String.IsNullOrEmpty(.连接凭据) = False Then
                        浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("CredentialReady('" & 替换HTML和JS敏感字符(当前用户.英语讯宝地址) & "', '" & 替换HTML和JS敏感字符(.连接凭据) & "', '" & .编号 & "');")
                    End If
                End With
            ElseIf TypeOf 机器人 Is 类_机器人_系统管理 Then
                浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("UserIDCredential('" & 当前用户.编号 & "', '" & 当前用户.凭据_中心服务器 & "');")
            End If
        End If
    End Sub

    Private Sub 页面加载完毕()
        If InvokeRequired Then
            Dim d As New 页面加载完毕_跨线程(AddressOf 页面加载完毕)
            Invoke(d, New Object() {})
        Else
            浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("MenuText('" & 界面文字.获取(193, "撤回") & "', '" & 界面文字.获取(194, "删除") & "');")
            If TypeOf 机器人 Is 类_机器人_小聊天群 Then
                载入最近聊天记录()
                If 聊天对象.小聊天群.群成员 Is Nothing Then 获取成员列表()
                浏览器_小宇宙.Load(获取讯友小宇宙的访问路径(聊天对象.讯友或群主.英语讯宝地址, 当前用户.英语讯宝地址))
            ElseIf TypeOf 机器人 Is 类_机器人_大聊天群 Then
                If String.IsNullOrEmpty(聊天对象.大聊天群.连接凭据) = True Then
                    获取连接凭据()
                    Return
                Else
                    载入最近聊天记录()
                    浏览器_小宇宙.Load(获取大聊天群小宇宙的访问路径(聊天对象.大聊天群.子域名))
                End If
            ElseIf TypeOf 机器人 Is 类_机器人_主控 Then
                按钮_说话.Enabled = True
                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("LoadEnd(false);")
            Else
                载入最近聊天记录()
            End If
        End If
    End Sub

    Friend Sub 获取连接凭据()
        Dim SS包生成器 As New 类_SS包生成器()
        SS包生成器.添加_有标签("发送序号", 当前用户.讯宝发送序号)
        SS包生成器.添加_有标签("子域名", 聊天对象.大聊天群.子域名)
        SS包生成器.添加_有标签("群编号", 聊天对象.大聊天群.编号)
        If 机器人.任务 IsNot Nothing Then 机器人.任务.结束()
        机器人.任务 = New 类_任务(任务名称_加入大聊天群, 机器人)
        机器人.说(界面文字.获取(281, "正在获取连接凭据。请稍等。"))
        机器人.启动HTTPS访问线程(New 类_访问设置(获取传送服务器访问路径开头(当前用户.主机名, 当前用户.域名_英语, False) & "C=JoinLargeGroup&UserID=" & 当前用户.编号 & "&Position=" & 当前用户.位置号 & "&DeviceType=" & 设备类型_电脑, 20000, SS包生成器.生成SS包(当前用户.AES加密器)))
    End Sub

    Friend Sub 获取成员列表()
        With 聊天对象.小聊天群
            If 数据库_保存要发送的小聊天群讯宝(机器人, 聊天对象.讯友或群主.英语讯宝地址, 聊天对象.小聊天群.编号, Date.UtcNow.Ticks, 讯宝指令_常量集合.获取小聊天群成员列表, IIf(.待加入确认, .备注, Nothing)) = True Then
                主窗体1.发送讯宝()
            End If
        End With
    End Sub

    Friend Sub 载入最近聊天记录()
        按钮_说话.Enabled = True
        Try
            If TypeOf 机器人 Is 类_机器人_一对一 OrElse TypeOf 机器人 Is 类_机器人_小聊天群 Then
                Dim 讯宝() As 讯宝_复合数据 = Nothing
                Dim 讯宝数量 As Integer
                Call 数据库_读取讯宝(讯宝, 讯宝数量)
                If 讯宝数量 > 0 Then
                    If 讯宝数量 < 讯宝.Length Then
                        起始时刻 = 0
                    Else
                        起始时刻 = 讯宝(讯宝数量 - 1).存储时间
                    End If
                    Dim I As Integer
                    If 聊天对象.小聊天群.编号 = 0 Then
                        For I = 讯宝数量 - 1 To 0 Step -1
                            With 讯宝(I)
                                If .是接收者 = False Then
                                    讯友说(.收发者讯宝地址, .发送时间, .发送序号, .讯宝指令, .文本, .宽度, .高度, .秒数, .已收听)
                                Else
                                    Select Case .讯宝指令
                                        Case 讯宝指令_常量集合.发送文字
                                            浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Text('" & .存储时间 & "', '" & 替换HTML和JS敏感字符(.文本) & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                        Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送短视频
                                            Dim 路径 As String
                                            If Path.GetFileName(.文本).Contains(特征字符_下划线) = False Then
                                                路径 = 处理文件路径以用作JS函数参数(.文本)
                                            Else
                                                路径 = 获取传送服务器访问路径开头(当前用户.主机名, 当前用户.域名_英语, True) & "Position=" & 当前用户.位置号 & "&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&FileName=" & 替换URI敏感字符(.文本)
                                            End If
                                            Select Case .讯宝指令
                                                Case 讯宝指令_常量集合.发送图片
                                                    浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Img('" & .存储时间 & "', '" & 路径 & "', '" & .宽度 & "', '" & .高度 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                                Case 讯宝指令_常量集合.发送语音
                                                    Dim 文本 As String = 界面文字.获取(258, "语音：#% 秒", New Object() { .秒数})
                                                    浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Voice('" & .存储时间 & "', '" & 替换HTML和JS敏感字符(文本) & "', '" & 路径 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                                Case 讯宝指令_常量集合.发送短视频
                                                    浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Video('" & .存储时间 & "', '" & 路径 & "', '" & .宽度 & "', '" & .高度 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                            End Select
                                        Case 讯宝指令_常量集合.发送文件
                                            Dim 原始文件名 As String = ""
                                            Dim 路径 As String
                                            If .文本.StartsWith(SS包标识_纯文本) = False Then
                                                原始文件名 = Path.GetFileName(.文本)
                                                路径 = 处理文件路径以用作JS函数参数(.文本)
                                            Else
                                                Try
                                                    Dim SS包解读器2 As New 类_SS包解读器
                                                    SS包解读器2.解读纯文本(.文本)
                                                    SS包解读器2.读取_有标签("O", 原始文件名)
                                                    Dim 存储文件名 As String = ""
                                                    SS包解读器2.读取_有标签("S", 存储文件名)
                                                    路径 = 获取传送服务器访问路径开头(当前用户.主机名, 当前用户.域名_英语, True) & "Position=" & 当前用户.位置号 & "&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&FileName=" & 替换URI敏感字符(存储文件名)
                                                Catch ex As Exception
                                                    Continue For
                                                End Try
                                            End If
                                            浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_File('" & .存储时间 & "', '" & 处理文件路径以用作JS函数参数(原始文件名) & "', '" & 路径 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                    End Select
                                End If
                            End With
                        Next
                    Else
                        Dim 当前用户英语讯宝地址 As String = 当前用户.英语讯宝地址
                        For I = 讯宝数量 - 1 To 0 Step -1
                            With 讯宝(I)
                                Select Case .讯宝指令
                                    Case 讯宝指令_常量集合.某人加入聊天群
                                        机器人.说(替换HTML和JS敏感字符(界面文字.获取(175, "#% 加入了本群。", (New String() { .文本}))), .发送时间)
                                    Case 讯宝指令_常量集合.退出小聊天群
                                        机器人.说(替换HTML和JS敏感字符(界面文字.获取(178, "#% 离开了本群。", (New String() { .文本}))), .发送时间)
                                    Case 讯宝指令_常量集合.删减聊天群成员
                                        机器人.说(替换HTML和JS敏感字符(界面文字.获取(190, "群主让 #% 离开了本群。", (New String() { .文本}))), .发送时间)
                                    Case 讯宝指令_常量集合.修改聊天群名称
                                        机器人.说(替换HTML和JS敏感字符(界面文字.获取(185, "本群名称更改为 #%。", (New String() { .文本}))), .发送时间)
                                    Case Else
                                        If String.Compare(.收发者讯宝地址, 当前用户英语讯宝地址) <> 0 Then
                                            讯友说(.收发者讯宝地址, .发送时间, .发送序号, .讯宝指令, .文本, .宽度, .高度, .秒数, .已收听)
                                        Else
                                            Select Case .讯宝指令
                                                Case 讯宝指令_常量集合.发送文字
                                                    浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Text('" & .存储时间 & "', '" & 替换HTML和JS敏感字符(.文本) & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                                Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送短视频
                                                    Dim 路径 As String
                                                    If Path.GetFileName(.文本.Contains(特征字符_下划线)) = False Then
                                                        路径 = 处理文件路径以用作JS函数参数(.文本)
                                                    Else
                                                        路径 = 获取传送服务器访问路径开头(当前用户.主机名, 当前用户.域名_英语, True) & "Position=" & 当前用户.位置号 & "&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&FileName=" & 替换URI敏感字符(.文本)
                                                    End If
                                                    Select Case .讯宝指令
                                                        Case 讯宝指令_常量集合.发送图片
                                                            浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Img('" & .存储时间 & "', '" & 路径 & "', '" & .宽度 & "', '" & .高度 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                                        Case 讯宝指令_常量集合.发送语音
                                                            Dim 文本 As String = 界面文字.获取(258, "语音：#% 秒", New Object() { .秒数})
                                                            浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Voice('" & .存储时间 & "', '" & 替换HTML和JS敏感字符(文本) & "', '" & 路径 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                                        Case 讯宝指令_常量集合.发送短视频
                                                            浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Video('" & .存储时间 & "', '" & 路径 & "', '" & .宽度 & "', '" & .高度 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                                    End Select
                                                Case 讯宝指令_常量集合.发送文件
                                                    Dim 原始文件名 As String = ""
                                                    Dim 路径 As String
                                                    If .文本.StartsWith(SS包标识_纯文本) = False Then
                                                        原始文件名 = Path.GetFileName(.文本)
                                                        路径 = 处理文件路径以用作JS函数参数(.文本)
                                                    Else
                                                        Dim SS包解读器2 As New 类_SS包解读器
                                                        SS包解读器2.解读纯文本(.文本)
                                                        SS包解读器2.读取_有标签("O", 原始文件名)
                                                        Dim 存储文件名 As String = ""
                                                        SS包解读器2.读取_有标签("S", 存储文件名)
                                                        路径 = 获取传送服务器访问路径开头(当前用户.主机名, 当前用户.域名_英语, True) & "Position=" & 当前用户.位置号 & "&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&FileName=" & 替换URI敏感字符(存储文件名)
                                                    End If
                                                    浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_File('" & .存储时间 & "', '" & 处理文件路径以用作JS函数参数(原始文件名) & "', '" & 路径 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                            End Select
                                        End If
                                End Select
                            End With
                        Next
                    End If
                Else
                    起始时刻 = 0
                End If
                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("LoadEnd(false);")
            ElseIf TypeOf 机器人 Is 类_机器人_大聊天群 Then
                Dim 讯宝() As 讯宝_复合数据 = Nothing
                Dim 讯宝数量 As Integer
                Call 数据库_读取讯宝(讯宝, 讯宝数量)
                If 讯宝数量 > 0 Then
                    If 讯宝数量 < 讯宝.Length Then
                        起始时刻 = 0
                    Else
                        起始时刻 = 讯宝(讯宝数量 - 1).发送时间
                    End If
                    聊天对象.大聊天群.最新讯宝的发送时间 = 讯宝(0).发送时间
                    Dim 当前用户英语讯宝地址 As String = 当前用户.英语讯宝地址
                    For I = 讯宝数量 - 1 To 0 Step -1
                        With 讯宝(I)
                            If String.Compare(.收发者讯宝地址, 当前用户英语讯宝地址) <> 0 Then
                                讯友说(.收发者讯宝地址, .发送时间, .发送序号, .讯宝指令, .文本, .宽度, .高度, .秒数, .已收听)
                            Else
                                Select Case .讯宝指令
                                    Case 讯宝指令_常量集合.发送文字
                                        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Text('" & .发送时间 & "', '" & 替换HTML和JS敏感字符(.文本) & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                    Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送短视频
                                        Dim 路径 As String
                                        If Path.GetFileName(.文本.Contains(特征字符_下划线)) = False Then
                                            路径 = 处理文件路径以用作JS函数参数(.文本)
                                        Else
                                            路径 = 获取大聊天群服务器访问路径开头(聊天对象.大聊天群.子域名, True) & "GroupID=" & 聊天对象.大聊天群.编号 & "&FileName=" & 替换URI敏感字符(.文本)
                                        End If
                                        Select Case .讯宝指令
                                            Case 讯宝指令_常量集合.发送图片
                                                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Img('" & .发送时间 & "', '" & 路径 & "', '" & .宽度 & "', '" & .高度 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "', 'true');")
                                            Case 讯宝指令_常量集合.发送语音
                                                Dim 文本 As String = 界面文字.获取(258, "语音：#% 秒", New Object() { .秒数})
                                                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Voice('" & .发送时间 & "', '" & 替换HTML和JS敏感字符(文本) & "', '" & 路径 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                            Case 讯宝指令_常量集合.发送短视频
                                                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Video('" & .发送时间 & "', '" & 路径 & "', '" & .宽度 & "', '" & .高度 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                        End Select
                                    Case 讯宝指令_常量集合.发送文件
                                        Dim 原始文件名 As String = ""
                                        Dim 路径 As String
                                        If .文本.StartsWith(SS包标识_纯文本) = False Then
                                            原始文件名 = Path.GetFileName(.文本)
                                            路径 = 处理文件路径以用作JS函数参数(.文本)
                                        Else
                                            Try
                                                Dim SS包解读器2 As New 类_SS包解读器
                                                SS包解读器2.解读纯文本(.文本)
                                                SS包解读器2.读取_有标签("O", 原始文件名)
                                                Dim 存储文件名 As String = ""
                                                SS包解读器2.读取_有标签("S", 存储文件名)
                                                路径 = 获取大聊天群服务器访问路径开头(聊天对象.大聊天群.子域名, True) & "GroupID=" & 聊天对象.大聊天群.编号 & "&FileName=" & 替换URI敏感字符(存储文件名)
                                            Catch ex As Exception
                                                Continue For
                                            End Try
                                        End If
                                        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_File('" & .发送时间 & "', '" & 处理文件路径以用作JS函数参数(原始文件名) & "', '" & 路径 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                End Select
                            End If
                        End With
                    Next
                Else
                    起始时刻 = 0
                End If
                按钮_说话.PerformClick()
                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("LoadEnd(true);")
            ElseIf TypeOf 机器人 Is 类_机器人_主控 Then
                Dim 讯宝() As 讯宝_复合数据 = Nothing
                Dim 讯宝数量 As Integer
                Call 数据库_读取讯宝(讯宝, 讯宝数量)
                If 讯宝数量 > 0 Then
                    If 讯宝数量 < 讯宝.Length Then
                        起始时刻 = 0
                    Else
                        起始时刻 = 讯宝(讯宝数量 - 1).发送时间
                    End If
                    Dim I As Integer
                    For I = 讯宝数量 - 1 To 0 Step -1
                        With 讯宝(I)
                            陌生人说(.收发者讯宝地址, .发送时间, .发送序号, .讯宝指令, .文本)
                        End With
                    Next
                Else
                    起始时刻 = 0
                End If
                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("LoadEnd(false);")
            ElseIf TypeOf 机器人 Is 类_机器人_系统管理 Then
                Dim 系统管理机器人 As 类_机器人_系统管理 = 机器人
                If String.IsNullOrEmpty(备份异常信息) = False Then
                    浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSin_Text('" & 机器人id_系统管理 & "', '0', '" & 备份异常信息 & "', '" & 机器人id_系统管理 & ".jpg" & "', '" & 时间格式(Date.FromBinary(Date.UtcNow.Ticks)) & "');")
                    备份异常信息 = Nothing
                End If
                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("LoadEnd(false);")
            Else
                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("LoadEnd(false);")
            End If
        Catch ex As Exception
            机器人.说(ex.Message)
        End Try
    End Sub

    Private Sub 数据库_读取讯宝(ByRef 讯宝() As 讯宝_复合数据, ByRef 讯宝数量 As Integer, Optional ByVal 最新的 As Boolean = False)
        If 副数据库 Is Nothing Then Return
        Const 最大值 As Integer = 20
        ReDim 讯宝(最大值 - 1)
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            If TypeOf 机器人 Is 类_机器人_一对一 Then
                Dim 列添加器 As New 类_列添加器
                列添加器.添加列_用于筛选器("讯宝地址", 筛选方式_常量集合.等于, 聊天对象.讯友或群主.英语讯宝地址)
                If 起始时刻 > 0 Then
                    列添加器.添加列_用于筛选器("存储时间", 筛选方式_常量集合.小于, 起始时刻)
                End If
                Dim 筛选器 As New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据(New String() {"讯宝地址", "是接收者", "指令", "文本库号", "文本编号", "宽度", "高度", "秒数", "已收听", "发送序号", "发送时间", "存储时间"})
                Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "一对一讯宝", 筛选器, 最大值, 列添加器, 最大值, "#地址存储时间")
                读取器 = 指令.执行()
                While 读取器.读取
                    With 讯宝(讯宝数量)
                        .收发者讯宝地址 = 读取器(0)
                        .是接收者 = 读取器(1)
                        .讯宝指令 = 读取器(2)
                        .文本库号 = 读取器(3)
                        .文本编号 = 读取器(4)
                        .宽度 = 读取器(5)
                        .高度 = 读取器(6)
                        .秒数 = 读取器(7)
                        .已收听 = 读取器(8)
                        .发送序号 = 读取器(9)
                        .发送时间 = 读取器(10)
                        .存储时间 = 读取器(11)
                    End With
                    讯宝数量 += 1
                End While
                读取器.关闭()
            ElseIf TypeOf 机器人 Is 类_机器人_小聊天群 Then
                Dim 列添加器 As New 类_列添加器
                列添加器.添加列_用于筛选器("群主讯宝地址", 筛选方式_常量集合.等于, 聊天对象.讯友或群主.英语讯宝地址)
                列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 聊天对象.小聊天群.编号)
                If 起始时刻 > 0 Then
                    列添加器.添加列_用于筛选器("存储时间", 筛选方式_常量集合.小于, 起始时刻)
                End If
                Dim 筛选器 As New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据(New String() {"发送者讯宝地址", "指令", "文本库号", "文本编号", "宽度", "高度", "秒数", "已收听", "发送序号", "发送时间", "存储时间"})
                Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "小聊天群讯宝", 筛选器, 最大值, 列添加器, 最大值, "#群主编号存储时间")
                读取器 = 指令.执行()
                While 读取器.读取
                    With 讯宝(讯宝数量)
                        .收发者讯宝地址 = 读取器(0)
                        .讯宝指令 = 读取器(1)
                        .文本库号 = 读取器(2)
                        .文本编号 = 读取器(3)
                        .宽度 = 读取器(4)
                        .高度 = 读取器(5)
                        .秒数 = 读取器(6)
                        .已收听 = 读取器(7)
                        .发送序号 = 读取器(8)
                        .发送时间 = 读取器(9)
                        .存储时间 = 读取器(10)
                    End With
                    讯宝数量 += 1
                End While
                读取器.关闭()
            ElseIf TypeOf 机器人 Is 类_机器人_大聊天群 Then
                Dim 列添加器 As New 类_列添加器
                列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 聊天对象.大聊天群.子域名)
                列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 聊天对象.大聊天群.编号)
                If 最新的 = False Then
                    If 起始时刻 > 0 Then
                        列添加器.添加列_用于筛选器("发送时间", 筛选方式_常量集合.小于, 起始时刻)
                    End If
                Else
                    列添加器.添加列_用于筛选器("发送时间", 筛选方式_常量集合.大于, 聊天对象.大聊天群.最新讯宝的发送时间)
                End If
                Dim 筛选器 As New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据(New String() {"发送者讯宝地址", "指令", "文本库号", "文本编号", "宽度", "高度", "秒数", "已收听", "发送时间"})
                Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "大聊天群讯宝", 筛选器, 最大值, 列添加器, 最大值, "#子域名群编号发送时间")
                读取器 = 指令.执行()
                While 读取器.读取
                    With 讯宝(讯宝数量)
                        .收发者讯宝地址 = 读取器(0)
                        .讯宝指令 = 读取器(1)
                        .文本库号 = 读取器(2)
                        .文本编号 = 读取器(3)
                        .宽度 = 读取器(4)
                        .高度 = 读取器(5)
                        .秒数 = 读取器(6)
                        .已收听 = 读取器(7)
                        .发送时间 = 读取器(8)
                    End With
                    讯宝数量 += 1
                End While
                读取器.关闭()
            ElseIf TypeOf 机器人 Is 类_机器人_主控 Then
                Dim 筛选器 As 类_筛选器 = Nothing
                Dim 列添加器 As 类_列添加器
                If 起始时刻 > 0 Then
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于筛选器("发送时间", 筛选方式_常量集合.小于, 起始时刻)
                    筛选器 = New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                End If
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据(New String() {"讯宝地址", "指令", "文本库号", "文本编号", "发送序号", "发送时间"})
                Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "陌生人讯宝", 筛选器, 最大值, 列添加器, 最大值, "#发送时间")
                读取器 = 指令.执行()
                While 读取器.读取
                    With 讯宝(讯宝数量)
                        .收发者讯宝地址 = 读取器(0)
                        .讯宝指令 = 读取器(1)
                        .文本库号 = 读取器(2)
                        .文本编号 = 读取器(3)
                        .发送序号 = 读取器(4)
                        .发送时间 = 读取器(5)
                    End With
                    讯宝数量 += 1
                End While
                读取器.关闭()
            End If
            Dim I As Integer
            For I = 0 To 讯宝数量 - 1
                With 讯宝(I)
                    If .文本库号 > 0 Then
                        Dim 列添加器 As New 类_列添加器
                        列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, .文本编号)
                        Dim 筛选器 As New 类_筛选器
                        筛选器.添加一组筛选条件(列添加器)
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于获取数据("文本")
                        Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, .文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
                        读取器 = 指令.执行()
                        While 读取器.读取
                            .文本 = 读取器(0)
                            Exit While
                        End While
                        读取器.关闭()
                    End If
                End With
            Next
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            机器人.说(ex.Message)
        End Try
    End Sub

    Friend Sub 讯友说(ByVal 讯宝地址 As String, ByVal 发送时间 As Long, ByVal 发送序号 As Long,
                   ByVal 讯宝指令 As 讯宝指令_常量集合, ByVal 文本 As String, ByVal 宽度 As Short, ByVal 高度 As Short,
                   ByVal 秒数 As Byte, ByVal 已收听 As Boolean, Optional ByVal 时间提示文本 As String = Nothing)
        If String.IsNullOrEmpty(文本) Then Return
        Dim 头像路径 As String
        Dim 主机名2 As String = Nothing
        Dim 位置号2 As Short
        Dim I As Integer
        If 聊天对象.小聊天群 IsNot Nothing Then
            If 聊天对象.小聊天群.编号 = 0 Then
跳转点1:
                If 当前用户.讯友目录 IsNot Nothing Then
                    Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
                    For I = 0 To 讯友目录.Length - 1
                        If String.Compare(讯友目录(I).英语讯宝地址, 讯宝地址) = 0 Then Exit For
                    Next
                    If I < 讯友目录.Length Then
                        With 讯友目录(I)
                            头像路径 = 获取讯友头像路径(.英语讯宝地址, .主机名, .头像更新时间)
                        End With
                    Else
                        头像路径 = 获取陌生人头像路径()
                    End If
                Else
                    头像路径 = 获取陌生人头像路径()
                End If
            Else
                Dim 群成员() As 类_群成员 = 聊天对象.小聊天群.群成员
                If 群成员 IsNot Nothing Then
                    For I = 0 To 群成员.Length - 1
                        If String.Compare(群成员(I).英语讯宝地址, 讯宝地址) = 0 Then Exit For
                    Next
                    If I < 群成员.Length Then
                        With 群成员(I)
                            头像路径 = 获取讯友头像路径(讯宝地址, .主机名)
                            主机名2 = .主机名
                            位置号2 = .位置号
                        End With
                    Else
                        GoTo 跳转点1
                    End If
                Else
                    GoTo 跳转点1
                End If
            End If
            If 时间提示文本 Is Nothing Then  '不用 String.IsNullOrEmpty判断
                时间提示文本 = 时间格式(Date.FromBinary(发送时间))
            End If
            Dim 段() As String = 讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
            Try
                Select Case 讯宝指令
                    Case 讯宝指令_常量集合.发送文字
                        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSin_Text('" & 讯宝地址 & "', '" & 发送序号 & "', '" & 替换HTML和JS敏感字符(文本) & "', '" & 头像路径 & "', '" & 时间提示文本 & "');")
                    Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送短视频
                        Dim 路径 As String
                        If 聊天对象.小聊天群.编号 = 0 Then
                            路径 = 获取传送服务器访问路径开头(聊天对象.讯友或群主.主机名, 段(1), True) & "Position=" & 聊天对象.讯友或群主.位置号 & "&EnglishSSAddress=" & 替换URI敏感字符(聊天对象.讯友或群主.英语讯宝地址) & "&FileName=" & 替换URI敏感字符(文本)
                        Else
                            路径 = 获取传送服务器访问路径开头(主机名2, 段(1), True) & "Position=" & 位置号2 & "&EnglishSSAddress=" & 替换URI敏感字符(讯宝地址) & "&FileName=" & 替换URI敏感字符(文本)
                        End If
                        Select Case 讯宝指令
                            Case 讯宝指令_常量集合.发送图片
                                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSin_Img('" & 讯宝地址 & "', '" & 发送序号 & "', '" & 处理文件路径以用作JS函数参数(路径) & "', '" & 宽度 & "', '" & 高度 & "', '" & 头像路径 & "', '" & 时间提示文本 & "');")
                            Case 讯宝指令_常量集合.发送语音
                                Dim 文本2 As String = 界面文字.获取(258, "语音：#% 秒", New Object() {秒数})
                                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSin_Voice('" & 讯宝地址 & "', '" & 发送序号 & "', '" & 替换HTML和JS敏感字符(文本2) & "', '" & 处理文件路径以用作JS函数参数(路径) & "', '" & 头像路径 & "', '" & 时间提示文本 & "'" & IIf(已收听, ", 'true'", "") & ");")
                            Case 讯宝指令_常量集合.发送短视频
                                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSin_Video('" & 讯宝地址 & "', '" & 发送序号 & "', '" & 处理文件路径以用作JS函数参数(路径) & "', '" & 宽度 & "', '" & 高度 & "', '" & 头像路径 & "', '" & 时间提示文本 & "');")
                        End Select
                    Case 讯宝指令_常量集合.发送文件
                        Dim SS包解读器2 As New 类_SS包解读器
                        SS包解读器2.解读纯文本(文本)
                        Dim 原始文件名 As String = ""
                        SS包解读器2.读取_有标签("O", 原始文件名)
                        Dim 存储文件名 As String = ""
                        SS包解读器2.读取_有标签("S", 存储文件名)
                        Dim 路径 As String
                        If 聊天对象.小聊天群.编号 = 0 Then
                            路径 = 获取传送服务器访问路径开头(聊天对象.讯友或群主.主机名, 段(1), True) & "Position=" & 聊天对象.讯友或群主.位置号 & "&EnglishSSAddress=" & 替换URI敏感字符(聊天对象.讯友或群主.英语讯宝地址) & "&FileName=" & 替换URI敏感字符(存储文件名)
                        Else
                            路径 = 获取传送服务器访问路径开头(主机名2, 段(1), True) & "Position=" & 位置号2 & "&EnglishSSAddress=" & 替换URI敏感字符(讯宝地址) & "&FileName=" & 替换URI敏感字符(存储文件名)
                        End If
                        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSin_File('" & 讯宝地址 & "', '" & 发送序号 & "', '" & 处理文件路径以用作JS函数参数(原始文件名) & "', '" & 处理文件路径以用作JS函数参数(路径) & "', '" & 头像路径 & "', '" & 时间提示文本 & "');")
                    Case 讯宝指令_常量集合.邀请加入小聊天群
                        Dim SS包解读器2 As New 类_SS包解读器()
                        SS包解读器2.解读纯文本(文本)
                        Dim 群编号 As Byte
                        SS包解读器2.读取_有标签("I", 群编号)
                        Dim 群名称 As String = Nothing
                        SS包解读器2.读取_有标签("N", 群名称)
                        文本 = 界面文字.获取(172, "我创建了一个小聊天群[#%]，希望你加入。", New Object() {替换HTML和JS敏感字符(群名称)}) & "&nbsp;<span class='TaskName' onclick='ToRobot2(\""JoinSmallGroup\"", \""" & 群名称 & "\"", \""" & 群编号 & "\"")'>" & 界面文字.获取(173, "加入") & "</span>"
                        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSin_Text('" & 讯宝地址 & "', '" & 发送序号 & "', """ & 文本 & """, '" & 头像路径 & "', '" & 时间提示文本 & "');")
                    Case 讯宝指令_常量集合.邀请加入大聊天群
                        Dim SS包解读器2 As New 类_SS包解读器()
                        SS包解读器2.解读纯文本(文本)
                        Dim 子域名 As String = Nothing
                        SS包解读器2.读取_有标签("D", 子域名)
                        Dim 群编号 As Long
                        SS包解读器2.读取_有标签("I", 群编号)
                        Dim 群名称 As String = Nothing
                        SS包解读器2.读取_有标签("N", 群名称)
                        文本 = 界面文字.获取(289, "我邀请你加入大聊天群[#%]。", New Object() {替换HTML和JS敏感字符(群名称)}) & "&nbsp;<span class='TaskName' onclick='ToRobot2(\""JoinLargeGroup\"", \""" & 子域名 & "\"", \""" & 群编号 & "\"")'>" & 界面文字.获取(173, "加入") & "</span>"
                        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSin_Text('" & 讯宝地址 & "', '" & 发送序号 & "', """ & 文本 & """, '" & 头像路径 & "', '" & 时间提示文本 & "');")
                End Select
            Catch ex As Exception
            End Try
        ElseIf 聊天对象.大聊天群 IsNot Nothing Then
            Dim 讯友 As 类_讯友 = 当前用户.查找讯友(讯宝地址)
            If 讯友 IsNot Nothing Then
                头像路径 = 获取讯友头像路径(讯宝地址, 讯友.主机名)
            Else
                主机名2 = 数据库_获取主机名(讯宝地址)
                If String.IsNullOrEmpty(主机名2) = False Then
                    头像路径 = 获取讯友头像路径(讯宝地址, 主机名2)
                Else
                    头像路径 = 获取陌生人头像路径()
                End If
            End If
            If 时间提示文本 Is Nothing Then  '不用 String.IsNullOrEmpty判断
                时间提示文本 = 时间格式(Date.FromBinary(发送时间))
            End If
            Try
                Select Case 讯宝指令
                    Case 讯宝指令_常量集合.发送文字
                        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSin_Text('" & 讯宝地址 & "', '" & 发送时间 & "', '" & 替换HTML和JS敏感字符(文本) & "', '" & 头像路径 & "', '" & 时间提示文本 & "');")
                    Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送短视频
                        Dim 路径 As String = 获取大聊天群服务器访问路径开头(聊天对象.大聊天群.子域名, True) & "GroupID=" & 聊天对象.大聊天群.编号 & "&FileName=" & 替换URI敏感字符(文本)
                        Select Case 讯宝指令
                            Case 讯宝指令_常量集合.发送图片
                                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSin_Img('" & 讯宝地址 & "', '" & 发送时间 & "', '" & 处理文件路径以用作JS函数参数(路径) & "', '" & 宽度 & "', '" & 高度 & "', '" & 头像路径 & "', '" & 时间提示文本 & "');")
                            Case 讯宝指令_常量集合.发送语音
                                Dim 文本2 As String = 界面文字.获取(258, "语音：#% 秒", New Object() {秒数})
                                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSin_Voice('" & 讯宝地址 & "', '" & 发送时间 & "', '" & 替换HTML和JS敏感字符(文本2) & "', '" & 处理文件路径以用作JS函数参数(路径) & "', '" & 头像路径 & "', '" & 时间提示文本 & "'" & IIf(已收听, ", 'true'", "") & ");")
                            Case 讯宝指令_常量集合.发送短视频
                                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSin_Video('" & 讯宝地址 & "', '" & 发送时间 & "', '" & 处理文件路径以用作JS函数参数(路径) & "', '" & 宽度 & "', '" & 高度 & "', '" & 头像路径 & "', '" & 时间提示文本 & "');")
                        End Select
                    Case 讯宝指令_常量集合.发送文件
                        Dim SS包解读器2 As New 类_SS包解读器
                        SS包解读器2.解读纯文本(文本)
                        Dim 原始文件名 As String = ""
                        SS包解读器2.读取_有标签("O", 原始文件名)
                        Dim 存储文件名 As String = ""
                        SS包解读器2.读取_有标签("S", 存储文件名)
                        Dim 路径 As String = 获取大聊天群服务器访问路径开头(聊天对象.大聊天群.子域名, True) & "GroupID=" & 聊天对象.大聊天群.编号 & "&FileName=" & 替换URI敏感字符(存储文件名)
                        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSin_File('" & 讯宝地址 & "', '" & 发送序号 & "', '" & 处理文件路径以用作JS函数参数(原始文件名) & "', '" & 处理文件路径以用作JS函数参数(路径) & "', '" & 头像路径 & "', '" & 时间提示文本 & "');")
                End Select
            Catch ex As Exception
            End Try
        End If
    End Sub

    Private Function 数据库_获取主机名(ByVal 英语讯宝地址 As String) As String
        Dim 主机名 As String = Nothing
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("主机名")
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "群成员主机名", 筛选器, 1, 列添加器, , 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                主机名 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
        End Try
        Return 主机名
    End Function

    Friend Sub 陌生人说(ByVal 讯宝地址 As String, ByVal 发送时间 As Long, ByVal 发送序号 As Long, ByVal 讯宝指令 As 讯宝指令_常量集合,
                                    ByVal 文本 As String, Optional ByVal 时间提示文本 As String = Nothing)
        If String.IsNullOrEmpty(文本) Then Return
        If 时间提示文本 Is Nothing Then  '不用 String.IsNullOrEmpty判断
            时间提示文本 = 时间格式(Date.FromBinary(发送时间))
        End If
        If 讯宝指令 = 讯宝指令_常量集合.邀请加入大聊天群 Then
            Try
                Dim SS包解读器2 As New 类_SS包解读器
                SS包解读器2.解读纯文本(文本)
                Dim 子域名 As String = Nothing
                SS包解读器2.读取_有标签("D", 子域名)
                Dim 大聊天群编号 As Long
                SS包解读器2.读取_有标签("I", 大聊天群编号)
                Dim 群名称 As String = Nothing
                SS包解读器2.读取_有标签("N", 群名称)
                文本 = 界面文字.获取(289, "我邀请你加入大聊天群[#%]。", New Object() {替换HTML和JS敏感字符(群名称)}) & "&nbsp;<span class='TaskName' onclick='ToRobot2(\""JoinLargeGroup\"", \""" & 子域名 & "\"", \""" & 大聊天群编号 & "\"")'>" & 界面文字.获取(173, "加入") & "</span>"
            Catch ex As Exception
                Return
            End Try
        Else
            文本 = 替换HTML和JS敏感字符(文本)
        End If
        Dim 陌生人 As String
        Dim 讯友 As 类_讯友 = 当前用户.查找讯友(讯宝地址)
        If 讯友 Is Nothing Then
            陌生人 = 界面文字.获取(76, "陌生人") & "&nbsp;" & 讯宝地址 & "<br><span class='TaskName' onclick='ToRobot2(\""AddContact\"", \""" & 讯宝地址 & "\"")'>" & 界面文字.获取(35, "添加为讯友") & "</span>&nbsp;<span class='TaskName' onclick='ToRobot2(\""Block\"", \""" & 讯宝地址 & "\"")'>" & 界面文字.获取(60, "添加至黑名单") & "</span>"
        ElseIf 讯友.拉黑 = False Then
            陌生人 = "[" & 界面文字.获取(58, "已成为讯友") & "]"
        Else
            陌生人 = "[" & 界面文字.获取(43, "已拉黑") & "]"
        End If
        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSin_Text('" & 讯宝地址 & "', '" & 发送序号 & "', """ & 文本 & """, '" & 获取陌生人头像路径() & "', '" & 时间提示文本 & "', """ & 陌生人 & """);")
    End Sub

    Friend Sub 显示同步的讯宝(ByVal 发送时间 As Long, ByVal 讯宝指令 As 讯宝指令_常量集合,
                       ByVal 文本 As String, ByVal 宽度 As Short, ByVal 高度 As Short, ByVal 秒数 As Byte)
        Try
            Select Case 讯宝指令
                Case 讯宝指令_常量集合.发送文字
                    浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Text('" & 发送时间 & "', '" & 替换HTML和JS敏感字符(文本) & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(发送时间)) & "');")
                Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送短视频
                    Dim 路径 As String = 获取传送服务器访问路径开头(当前用户.主机名, 当前用户.域名_英语, True) & "Position=" & 当前用户.位置号 & "&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&FileName=" & 替换URI敏感字符(文本)
                    Select Case 讯宝指令
                        Case 讯宝指令_常量集合.发送图片
                            浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Img('" & 发送时间 & "', '" & 处理文件路径以用作JS函数参数(路径) & "', '" & 宽度 & "', '" & 高度 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(发送时间)) & "');")
                        Case 讯宝指令_常量集合.发送语音
                            Dim 文本2 As String = 界面文字.获取(258, "语音：#% 秒", New Object() {秒数})
                            浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Voice('" & 发送时间 & "', '" & 替换HTML和JS敏感字符(文本2) & "', '" & 处理文件路径以用作JS函数参数(路径) & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(发送时间)) & "');")
                        Case 讯宝指令_常量集合.发送短视频
                            浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Video('" & 发送时间 & "', '" & 处理文件路径以用作JS函数参数(路径) & "', '" & 宽度 & "', '" & 高度 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(发送时间)) & "');")
                    End Select
                Case 讯宝指令_常量集合.发送文件
                    Dim SS包解读器2 As New 类_SS包解读器
                    SS包解读器2.解读纯文本(文本)
                    Dim 原始文件名 As String = ""
                    SS包解读器2.读取_有标签("O", 原始文件名)
                    Dim 存储文件名 As String = ""
                    SS包解读器2.读取_有标签("S", 存储文件名)
                    Dim 路径 As String = 获取传送服务器访问路径开头(当前用户.主机名, 当前用户.域名_英语, True) & "Position=" & 当前用户.位置号 & "&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&FileName=" & 替换URI敏感字符(存储文件名)
                    浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_File('" & 发送时间 & "', '" & 处理文件路径以用作JS函数参数(原始文件名) & "', '" & 路径 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(发送时间)) & "');")
            End Select
        Catch ex As Exception
        End Try
    End Sub

    Friend Sub 加载大聊天群新讯宝(ByVal 最新讯宝的时刻 As Long)
        Dim 讯宝() As 讯宝_复合数据 = Nothing
        Dim 讯宝数量 As Integer
        Call 数据库_读取讯宝(讯宝, 讯宝数量, True)
        If 讯宝数量 > 0 Then
            If 聊天对象.大聊天群.最新讯宝的发送时间 < 讯宝(0).发送时间 Then 聊天对象.大聊天群.最新讯宝的发送时间 = 讯宝(0).发送时间
            浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("LoadLaterStart();")
            Dim 当前用户英语讯宝地址 As String = 当前用户.英语讯宝地址
            Dim I As Integer
            For I = 讯宝数量 - 1 To 0 Step -1
                With 讯宝(I)
                    If String.Compare(.收发者讯宝地址, 当前用户英语讯宝地址) <> 0 Then
                        讯友说(.收发者讯宝地址, .发送时间, .发送序号, .讯宝指令, .文本, .宽度, .高度, .秒数, .已收听)
                    Else
                        Select Case .讯宝指令
                            Case 讯宝指令_常量集合.发送文字
                                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Text('" & .发送时间 & "', '" & 替换HTML和JS敏感字符(.文本) & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                            Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送短视频
                                Dim 路径 As String
                                If Path.GetFileName(.文本.Contains(特征字符_下划线)) = False Then
                                    路径 = 处理文件路径以用作JS函数参数(.文本)
                                Else
                                    路径 = 获取大聊天群服务器访问路径开头(聊天对象.大聊天群.子域名, True) & "GroupID=" & 聊天对象.大聊天群.编号 & "&FileName=" & 替换URI敏感字符(.文本)
                                End If
                                Select Case .讯宝指令
                                    Case 讯宝指令_常量集合.发送图片
                                        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Img('" & .发送时间 & "', '" & 路径 & "', '" & .宽度 & "', '" & .高度 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "', 'true');")
                                    Case 讯宝指令_常量集合.发送语音
                                        Dim 文本 As String = 界面文字.获取(258, "语音：#% 秒", New Object() { .秒数})
                                        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Voice('" & .发送时间 & "', '" & 替换HTML和JS敏感字符(文本) & "', '" & 路径 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                    Case 讯宝指令_常量集合.发送短视频
                                        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Video('" & .发送时间 & "', '" & 路径 & "', '" & .宽度 & "', '" & .高度 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                End Select
                            Case 讯宝指令_常量集合.发送文件
                                Dim 原始文件名 As String = ""
                                Dim 路径 As String
                                If .文本.StartsWith(SS包标识_纯文本) = False Then
                                    原始文件名 = Path.GetFileName(.文本)
                                    路径 = 处理文件路径以用作JS函数参数(.文本)
                                Else
                                    Dim SS包解读器2 As New 类_SS包解读器
                                    SS包解读器2.解读纯文本(.文本)
                                    SS包解读器2.读取_有标签("O", 原始文件名)
                                    Dim 存储文件名 As String = ""
                                    SS包解读器2.读取_有标签("S", 存储文件名)
                                    路径 = 获取大聊天群服务器访问路径开头(聊天对象.大聊天群.子域名, True) & "GroupID=" & 聊天对象.大聊天群.编号 & "&FileName=" & 替换URI敏感字符(存储文件名)
                                End If
                                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_File('" & .发送时间 & "', '" & 处理文件路径以用作JS函数参数(原始文件名) & "', '" & 路径 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                        End Select
                    End If
                End With
            Next
            浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("LoadEnd(true);")
        End If
        If 聊天对象.大聊天群.最新讯宝的发送时间 < 最新讯宝的时刻 Then 聊天对象.大聊天群.最新讯宝的发送时间 = 最新讯宝的时刻
    End Sub

    Friend Sub 点击群成员(ByVal who As String)
        If String.IsNullOrEmpty(who) = True Then Return
        If 聊天对象.小聊天群 IsNot Nothing Then
            If 聊天对象.小聊天群.编号 > 0 Then GoTo 跳转点1
        ElseIf 聊天对象.大聊天群 IsNot Nothing Then
跳转点1:
            Dim 段() As String = who.Split("/")
            Dim 用户英语讯宝地址 As String = 当前用户.英语讯宝地址
            Dim I As Integer
            For I = 0 To 段.Length - 1
                If String.Compare(段(I).Trim, 用户英语讯宝地址) = 0 Then Return
            Next
            If 说话对象的容器.Controls.Count >= 10 Then Return
            For I = 0 To 说话对象的容器.Controls.Count - 1
                If String.Compare(CType(说话对象的容器.Controls.Item(I), Label).Text, who) = 0 Then Return
            Next
            Dim 文字控件 As New Label
            文字控件.Text = who
            文字控件.AutoSize = True
            文字控件.ForeColor = Color.Red
            说话对象的容器.Controls.Add(文字控件)
            AddHandler 文字控件.Click, AddressOf 文字_说话对象_Click
            聊天内容的容器.Height = 说话对象的容器.Top - 浏览器_聊天内容.Top
            Dim 字数 As Integer
            For I = 0 To 说话对象的容器.Controls.Count - 1
                字数 += CType(说话对象的容器.Controls.Item(I), Label).Text.Length + 2
            Next
            If 字数 > 0 Then
                输入框.MaxLength = 最大值_常量集合.讯宝文本长度 - 字数 - 20
            Else
                输入框.MaxLength = 最大值_常量集合.讯宝文本长度
            End If
            输入框.Focus()
        End If
    End Sub

    Private Sub 文字_说话对象_Click(sender As Object, e As EventArgs)
        RemoveHandler CType(sender, Label).Click, AddressOf 文字_说话对象_Click
        说话对象的容器.Controls.Remove(sender)
        聊天内容的容器.Height = 说话对象的容器.Top - 浏览器_聊天内容.Top
        Dim I, 字数 As Integer
        For I = 0 To 说话对象的容器.Controls.Count - 1
            字数 += CType(说话对象的容器.Controls.Item(I), Label).Text.Length + 2
        Next
        If 字数 > 0 Then
            输入框.MaxLength = 最大值_常量集合.讯宝文本长度 - 字数 - 20
        Else
            输入框.MaxLength = 最大值_常量集合.讯宝文本长度
        End If
        输入框.Focus()
    End Sub

    Private Sub 按钮_说话_Click(sender As Object, e As EventArgs) Handles 按钮_说话.Click
        If 发送语音 = False Then
            Dim 文本 As String = 输入框.Text.Trim
            输入框.Clear()
            If TypeOf 机器人 IsNot 类_机器人_大聊天群 Then
                If String.IsNullOrEmpty(文本) Then Return
                GoTo 跳转点1
            Else
                If String.IsNullOrEmpty(文本) Then
                    CType(机器人, 类_机器人_大聊天群).刷新()
                Else
跳转点1:
                    If 说话对象的容器.Controls.Count > 0 Then
                        Dim 变长文本 As New StringBuilder(100 * 说话对象的容器.Controls.Count)
                        Dim 文本写入器 As New StringWriter(变长文本)
                        Dim I As Integer
                        For I = 0 To 说话对象的容器.Controls.Count - 1
                            文本写入器.Write(CType(说话对象的容器.Controls.Item(I), Label).Text & vbCrLf)
                        Next
                        文本写入器.Close()
                        文本 = "★★★" & vbCrLf & 文本写入器.ToString & vbCrLf & 文本
                    End If
                    对机器人说(文本, True)
                End If
            End If
        Else
            If 主窗体1.录音类 Is Nothing Then 主窗体1.录音类 = New 类_音频_录制
            If 主窗体1.录音类.正在录音 = False Then
                录音剩余秒数 = 最大值_常量集合.语音时长_秒
                进度条.当前值 = 0
                进度条.ForeColor = Color.Orange
                进度条.最大值 = 录音剩余秒数 * 10
                进度条.当前值 = 进度条.最大值
                Dim 目录路径 As String = My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData & "\" & 当前用户.英语讯宝地址
                If Directory.Exists(目录路径) = False Then Directory.CreateDirectory(目录路径)
                If 主窗体1.录音类.开始录音(目录路径, 生成大写英文字母与数字的随机字符串(20), 类_音频_录制.音频格式_常量集合.amr) = True Then
                    定时器_录音.Start()
                    按钮_说话.Text = 界面文字.获取(256, "停止并发送")
                    进度条.提示文字 = 界面文字.获取(257, "点击进度条可取消录音。")
                    主窗体1.当前录音控件 = Me
                    取消录音 = False
                End If
            Else
                If Equals(Me, 主窗体1.当前录音控件) = False Then 主窗体1.当前录音控件.取消录音 = True
                主窗体1.录音类.停止录音()
            End If
        End If
    End Sub

    Friend Sub 录音完毕(ByVal 文件路径 As String)
        定时器_录音.Stop()
        If 取消录音 = False Then
            Dim 录音时间长度 As Single = 最大值_常量集合.语音时长_秒 - 录音剩余秒数
            录音时间长度 = Math.Round(录音时间长度, 1)
            If 录音时间长度 >= 1.0F Then
                Dim 文本 As String = 界面文字.获取(258, "语音：#% 秒", New Object() {CByte(录音时间长度)})
                Dim 当前UTC时刻 As Long = Date.UtcNow.Ticks
                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Voice('" & 当前UTC时刻 & "', '" & 替换HTML和JS敏感字符(文本) & "', '" & 处理文件路径以用作JS函数参数(文件路径) & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(当前UTC时刻)) & "');")
                If TypeOf 机器人 Is 类_机器人_一对一 Then
                    If 数据库_保存要发送的一对一讯宝(机器人, 聊天对象.讯友或群主.英语讯宝地址, 当前UTC时刻, 讯宝指令_常量集合.发送语音, 文件路径, , , CByte(录音时间长度)) = True Then
                        Dim 刷新 As Boolean
                        If 数据库_更新最近互动讯友排名(聊天对象.讯友或群主.英语讯宝地址, 0, 刷新) = True Then
                            If 刷新 Then
                                If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.最近 Then
                                    主窗体1.刷新讯友录()
                                End If
                            End If
                            主窗体1.发送讯宝()
                        End If
                    End If
                ElseIf TypeOf 机器人 Is 类_机器人_小聊天群 Then
                    If 数据库_保存要发送的小聊天群讯宝(机器人, 聊天对象.讯友或群主.英语讯宝地址, 聊天对象.小聊天群.编号, 当前UTC时刻, 讯宝指令_常量集合.发送语音, 文件路径, , , CByte(录音时间长度)) = True Then
                        Dim 刷新 As Boolean
                        If 数据库_更新最近互动讯友排名(聊天对象.讯友或群主.英语讯宝地址, 聊天对象.小聊天群.编号, 刷新) = True Then
                            If 刷新 Then
                                If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.最近 Then
                                    主窗体1.刷新讯友录()
                                End If
                            End If
                            主窗体1.发送讯宝()
                        End If
                    End If
                ElseIf TypeOf 机器人 Is 类_机器人_大聊天群 Then
                    CType(机器人, 类_机器人_大聊天群).发送或接收(讯宝指令_常量集合.发送语音, Path.GetExtension(文件路径).Replace(".", ""), , , CByte(录音时间长度), File.ReadAllBytes(文件路径), 当前UTC时刻, 文件路径)
                    Dim 刷新 As Boolean
                    If 数据库_更新最近互动讯友排名(聊天对象.大聊天群.子域名, 聊天对象.大聊天群.编号, 刷新) = True Then
                        If 刷新 Then
                            If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.最近 Then
                                主窗体1.刷新讯友录()
                            End If
                        End If
                    End If
                End If
            End If
        ElseIf File.Exists(文件路径) Then
            Try
                File.Delete(文件路径)
            Catch ex As Exception
            End Try
        End If
        按钮_说话.Text = 界面文字.获取(255, "录制语音")
        进度条.提示文字 = ""
        进度条.当前值 = 0
    End Sub

    Private Sub 定时器_录音_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles 定时器_录音.Tick
        录音剩余秒数 -= 0.1
        进度条.当前值 = 录音剩余秒数 * 10
        If 录音剩余秒数 <= 0 Then Call 按钮_说话_Click(Nothing, Nothing)
    End Sub

    Friend Sub 播音开始(ByVal VoiceID As String)
        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("VoiceStarted('" & VoiceID & "');")
    End Sub

    Friend Sub 播音完毕()
        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("VoiceEnded();")
    End Sub

    Private Sub 输入框_KeyPress(sender As Object, e As KeyPressEventArgs) Handles 输入框.KeyPress
        Select Case e.KeyChar
            Case vbCr : e.Handled = True
            Case ChrW(27)
                输入框.Clear()
                e.Handled = True
        End Select
    End Sub

    Private Sub 输入框_TextChanged(sender As Object, e As EventArgs) Handles 输入框.TextChanged
        文字_字数.Text = 输入框.Text.Length
        If 输入框.PasswordChar <> vbNullChar Then
            If 输入框.Text.Length = 最小值_常量集合.密码长度 OrElse 输入框.Text.Length = 最大值_常量集合.密码长度 Then
                输入框.BackColor = Color.SpringGreen
            ElseIf 输入框.Text.Length = 最小值_常量集合.密码长度 - 1 OrElse 输入框.Text.Length = 最大值_常量集合.密码长度 + 1 OrElse 输入框.Text.Length = 0 Then
                输入框.BackColor = Color.Pink
            End If
        ElseIf TypeOf 机器人 Is 类_机器人_大聊天群 Then
            If String.IsNullOrEmpty(输入框.Text.Trim) Then
                按钮_说话.Text = 界面文字.获取(2, "刷新")
            Else
                按钮_说话.Text = 界面文字.获取(1, "说话")
            End If
        End If
    End Sub

    Private Sub 下拉列表_机器人_SelectedIndexChanged(sender As Object, e As EventArgs) Handles 下拉列表_任务.SelectedIndexChanged
        If 下拉列表_任务.SelectedIndex > 0 Then
            对机器人说(下拉列表_任务.Text)
            下拉列表_任务.SelectedIndex = 0
        End If
    End Sub

    Friend Sub 对机器人说(ByVal 文本 As String, Optional ByVal 来自输入框 As Boolean = False, Optional ByVal 不使用定时器 As Boolean = False)
        If String.IsNullOrEmpty(文本) = True Then Return
        If 定时器_机器人回答.Enabled = True Then
            定时器_机器人回答.Stop()
            With CType(定时器_机器人回答.Tag, 话语_复合数据)
                机器人.回答(.文本, .时间)
            End With
        End If
        Dim 当前UTC时刻 As Long = Date.UtcNow.Ticks
        If 输入框.PasswordChar = vbNullChar Then
            浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Text('" & 当前UTC时刻 & "', '" & 替换HTML和JS敏感字符(文本) & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(当前UTC时刻)) & "');")
        Else
            浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Text('" & 当前UTC时刻 & "', '" & 替换HTML和JS敏感字符(界面文字.获取(128, "[密码]")) & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(当前UTC时刻)) & "');")
        End If
        If 不使用定时器 = False Then
            Dim 话语 As New 话语_复合数据
            话语.文本 = 文本
            话语.时间 = 当前UTC时刻
            定时器_机器人回答.Tag = 话语
            定时器_机器人回答.Start()
        Else
            机器人.回答(文本, 当前UTC时刻)
        End If
    End Sub

    Private Sub 定时器_机器人回答_Tick(sender As Object, e As EventArgs) Handles 定时器_机器人回答.Tick
        定时器_机器人回答.Stop()
        With CType(定时器_机器人回答.Tag, 话语_复合数据)
            机器人.回答(.文本, .时间)
        End With
    End Sub

    Friend Sub 载入任务名称()
        下拉列表_任务.Items.Clear()
        下拉列表_任务.Items.Add(界面文字.获取(13, "对机器人说"))
        下拉列表_任务.Items.Add(任务名称_取消)
        If TypeOf 机器人 Is 类_机器人_一对一 Then
            下拉列表_任务.Items.Add(任务名称_小宇宙)
            If 发送语音 = False Then
                下拉列表_任务.Items.Add(任务名称_发送语音)
            Else
                下拉列表_任务.Items.Add(任务名称_发送文字)
            End If
            下拉列表_任务.Items.Add(任务名称_发送图片)
            下拉列表_任务.Items.Add(任务名称_发送原图)
            下拉列表_任务.Items.Add(任务名称_发送文件)
            下拉列表_任务.Items.Add(任务名称_添加新标签)
            下拉列表_任务.Items.Add(任务名称_添加现有标签)
            下拉列表_任务.Items.Add(任务名称_移除标签)
            下拉列表_任务.Items.Add(任务名称_备注)
            下拉列表_任务.Items.Add(任务名称_拉黑)
        ElseIf TypeOf 机器人 Is 类_机器人_小聊天群 Then
            下拉列表_任务.Items.Add(任务名称_小宇宙)
            If 发送语音 = False Then
                下拉列表_任务.Items.Add(任务名称_发送语音)
            Else
                下拉列表_任务.Items.Add(任务名称_发送文字)
            End If
            下拉列表_任务.Items.Add(任务名称_发送图片)
            下拉列表_任务.Items.Add(任务名称_发送原图)
            下拉列表_任务.Items.Add(任务名称_发送文件)
            If String.Compare(聊天对象.讯友或群主.英语讯宝地址, 当前用户.英语讯宝地址) = 0 Then
                下拉列表_任务.Items.Add(任务名称_邀请)
                下拉列表_任务.Items.Add(任务名称_删减成员)
                下拉列表_任务.Items.Add(任务名称_群名称)
                下拉列表_任务.Items.Add(任务名称_解散聊天群)
            Else
                下拉列表_任务.Items.Add(任务名称_退出聊天群)
            End If
        ElseIf TypeOf 机器人 Is 类_机器人_大聊天群 Then
            If 聊天对象.大聊天群.我的角色 > 群角色_常量集合.成员_不可发言 Then
                下拉列表_任务.Items.Add(任务名称_小宇宙)
                If 发送语音 = False Then
                    下拉列表_任务.Items.Add(任务名称_发送语音)
                Else
                    下拉列表_任务.Items.Add(任务名称_发送文字)
                End If
                下拉列表_任务.Items.Add(任务名称_发送图片)
                下拉列表_任务.Items.Add(任务名称_发送原图)
                下拉列表_任务.Items.Add(任务名称_发送文件)
                下拉列表_任务.Items.Add(任务名称_昵称)
                Select Case 聊天对象.大聊天群.我的角色
                    Case 群角色_常量集合.群主
                        下拉列表_任务.Items.Add(任务名称_邀请)
                        下拉列表_任务.Items.Add(任务名称_修改角色)
                        下拉列表_任务.Items.Add(任务名称_删减成员)
                        下拉列表_任务.Items.Add(任务名称_群名称)
                        下拉列表_任务.Items.Add(任务名称_图标)
                        下拉列表_任务.Items.Add(任务名称_解散聊天群)
                    Case 群角色_常量集合.管理员
                        下拉列表_任务.Items.Add(任务名称_邀请)
                        下拉列表_任务.Items.Add(任务名称_修改角色)
                        下拉列表_任务.Items.Add(任务名称_删减成员)
                        下拉列表_任务.Items.Add(任务名称_退出聊天群)
                    Case Else
                        下拉列表_任务.Items.Add(任务名称_退出聊天群)
                End Select
            ElseIf 聊天对象.大聊天群.我的角色 = 群角色_常量集合.成员_不可发言 Then
                下拉列表_任务.Items.Add(任务名称_小宇宙)
                下拉列表_任务.Items.Add(任务名称_昵称)
                下拉列表_任务.Items.Add(任务名称_退出聊天群)
            Else
                下拉列表_任务.Items.Add(任务名称_退出聊天群)
            End If
        ElseIf TypeOf 机器人 Is 类_机器人_主控 Then
            If 当前用户.已登录() Then
                下拉列表_任务.Items.Add(任务名称_小宇宙)
                下拉列表_任务.Items.Add(任务名称_添加讯友)
                下拉列表_任务.Items.Add(任务名称_删除讯友)
                下拉列表_任务.Items.Add(任务名称_清理黑名单)
                下拉列表_任务.Items.Add(任务名称_添加黑域)
                下拉列表_任务.Items.Add(任务名称_添加白域)
                下拉列表_任务.Items.Add(任务名称_重命名标签)
                下拉列表_任务.Items.Add(任务名称_创建小聊天群)
                下拉列表_任务.Items.Add(任务名称_创建大聊天群)
                下拉列表_任务.Items.Add(任务名称_账户)
                下拉列表_任务.Items.Add(任务名称_图标)
                下拉列表_任务.Items.Add(任务名称_密码)
                下拉列表_任务.Items.Add(任务名称_手机号)
                下拉列表_任务.Items.Add(任务名称_邮箱地址)
                下拉列表_任务.Items.Add(任务名称_注销)
            Else
                下拉列表_任务.Items.Add(任务名称_登录)
                下拉列表_任务.Items.Add(任务名称_注册)
                下拉列表_任务.Items.Add(任务名称_忘记)
            End If
            下拉列表_任务.Items.Add(任务名称_关闭)
        ElseIf TypeOf 机器人 Is 类_机器人_系统管理 Then
            下拉列表_任务.Items.Add(任务名称_报表)
            If 当前用户.职能.Contains(职能_管理员) Then
                下拉列表_任务.Items.Add(任务名称_备份数据库)
                下拉列表_任务.Items.Add(任务名称_新传送服务器)
                下拉列表_任务.Items.Add(任务名称_新大聊天群服务器)
                下拉列表_任务.Items.Add(任务名称_小宇宙中心服务器)
                下拉列表_任务.Items.Add(任务名称_添加可注册者)
                下拉列表_任务.Items.Add(任务名称_移除可注册者)
                下拉列表_任务.Items.Add(任务名称_商品编辑者)
            End If
        End If
        下拉列表_任务.SelectedIndex = 0
    End Sub

    Private Sub 控件_聊天_Disposed(sender As Object, e As EventArgs) Handles Me.Disposed
        If 机器人 IsNot Nothing Then
            If TypeOf 机器人 Is 类_机器人_主控 Then
                CType(机器人, 类_机器人_主控).Dispose()
            ElseIf TypeOf 机器人 Is 类_机器人_系统管理 Then
                CType(机器人, 类_机器人_系统管理).Dispose()
            End If
            机器人 = Nothing
        End If
    End Sub

    Friend Function 时间格式(ByVal UTC时间 As Date) As String
        Dim 当前UTC时间 As Date = Date.UtcNow
        Dim 时区偏移分钟数 As Integer = DateDiff(DateInterval.Minute, 当前UTC时间, Date.Now)
        If UTC时间.Year = 当前UTC时间.Year Then
            If UTC时间.Month = 当前UTC时间.Month Then
                If UTC时间.Day = 当前UTC时间.Day Then
                    Return UTC时间.AddMinutes(时区偏移分钟数).ToString("HH:mm")
                Else
                    Return UTC时间.AddMinutes(时区偏移分钟数).ToString("dd HH:mm")
                End If
            Else
                Return UTC时间.AddMinutes(时区偏移分钟数).ToString("MM-dd HH:mm")
            End If
        Else
            Return UTC时间.AddMinutes(时区偏移分钟数).ToString("yyyy-MM-dd HH:mm")
        End If
    End Function

    Friend Sub 对机器人说2(ByVal 指令名称 As String, ByVal 参数1 As String, ByVal 参数2 As String)
        If TypeOf 机器人 Is 类_机器人_主控 Then
            Select Case 指令名称
                Case "AddContact"
                    Dim 讯友 As 类_讯友 = 当前用户.查找讯友(参数1)
                    If 讯友 Is Nothing Then
                        对机器人说(任务名称_添加讯友, , True)
                        对机器人说(参数1)
                    Else
                        机器人.说(界面文字.获取(119, "已在你的讯友录中。"))
                    End If
                Case "Block"
                    Dim 讯友 As 类_讯友 = 当前用户.查找讯友(参数1)
                    If 讯友 Is Nothing Then
跳转点1:
                        对机器人说(任务名称_拉黑, , True)
                        对机器人说(参数1)
                    ElseIf 讯友.拉黑 = False Then
                        GoTo 跳转点1
                    Else
                        机器人.说(界面文字.获取(110, "已在黑名单中。"))
                    End If
                Case "DeleteSS"
                    Call 删除陌生人讯宝(参数2)
                Case "JoinLargeGroup"
                    GoTo 跳转点2
                Case "TermsOfUse"
                    主窗体1.访问网页(获取主站首页的访问路径() & "TermsOfUse.html")
                Case "PrivacyPolicy"
                    主窗体1.访问网页(获取主站首页的访问路径() & "PrivacyPolicy.html")
            End Select
        Else
            Select Case 指令名称
                Case "ClickImage"
                    Dim 窗体 As New 窗体_查看图片
                    窗体.Show(主窗体)
                    窗体.显示图片(参数1)
                Case "ClickVideo"
                    Dim 窗体 As New 窗体_播放视频(参数1)
                    窗体.Show(主窗体)
                Case "DownloadFile"
                    If 参数2.StartsWith("https://") OrElse 参数2.StartsWith("http://") Then
                        Dim 保存路径 As String = Path.GetDirectoryName(My.Computer.FileSystem.SpecialDirectories.MyDocuments) & "\Downloads"
                        If Directory.Exists(保存路径) = False Then
                            保存路径 = My.Computer.FileSystem.SpecialDirectories.MyDocuments
                        End If
                        Dim 段() As String = 参数2.Split(New String() {"&"}, StringSplitOptions.RemoveEmptyEntries)
                        Const 字符串 As String = "FileName="
                        Dim I As Integer
                        For I = 段.Length - 1 To 0 Step -1
                            If 段(I).StartsWith(字符串) Then Exit For
                        Next
                        If I < 0 Then
                            保存路径 &= "\" & 参数1
                        Else
                            Dim J As Integer = 段(I).LastIndexOf(特征字符_下划线)
                            If J > 0 AndAlso J < 段(I).Length - 1 Then
                                保存路径 &= "\" & Path.GetFileNameWithoutExtension(参数1) & 段(I).Substring(J)
                            Else
                                保存路径 &= "\" & 参数1
                            End If
                        End If
                        If File.Exists(保存路径) = False Then
                            If 主窗体1.下载文件的窗体 Is Nothing Then
                                主窗体1.下载文件的窗体 = New 窗体_下载文件(参数2, 保存路径)
                                主窗体1.下载文件的窗体.Show()
                            Else
                                主窗体1.下载文件的窗体.新下载任务(参数2, 保存路径)
                                If 主窗体1.下载文件的窗体.WindowState = FormWindowState.Minimized Then
                                    主窗体1.下载文件的窗体.WindowState = FormWindowState.Normal
                                ElseIf 主窗体1.下载文件的窗体.Visible = False Then
                                    主窗体1.下载文件的窗体.Show()
                                Else
                                    主窗体1.下载文件的窗体.BringToFront()
                                End If
                            End If
                        Else
                            打开资源管理器并选中文件(保存路径)
                        End If
                    Else
                        打开资源管理器并选中文件(参数2)
                    End If
                Case "DeleteSS"
                    Call 删除讯宝(Long.Parse(参数1), 参数2)
                Case "CancelSS"
                    Call 撤回讯宝(Long.Parse(参数1))
                Case "CopyText"
                    Clipboard.SetText(参数1.Trim)
                Case "ClickIcon"
                    If 参数1.Contains(讯宝地址标识) = False Then Return
                    If String.Compare(参数1, 当前用户.英语讯宝地址) = 0 Then Return
                    Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
                    If 讯友目录 IsNot Nothing Then
                        Dim I As Integer
                        For I = 0 To 讯友目录.Length - 1
                            If String.Compare(讯友目录(I).英语讯宝地址, 参数1) = 0 Then Exit For
                        Next
                        If I < 讯友目录.Length Then
                            Dim 地址 As String
                            With 讯友目录(I)
                                If String.IsNullOrEmpty(.本国语讯宝地址) = False Then
                                    地址 = .本国语讯宝地址 & " / " & .英语讯宝地址
                                Else
                                    地址 = .英语讯宝地址
                                End If
                            End With
                            点击群成员(地址)
                        Else
                            点击群成员(参数1)
                        End If
                    Else
                        点击群成员(参数1)
                    End If
                Case "JoinSmallGroup"
                    Dim 群编号 As Byte
                    If Byte.TryParse(参数2, 群编号) = False Then Return
                    If 群编号 = 0 Then Return
                    Dim 群主英语讯宝地址 As String = 聊天对象.讯友或群主.英语讯宝地址
                    If 当前用户.加入的小聊天群 IsNot Nothing Then
                        Dim 加入的小聊天群() As 类_聊天群_小 = 当前用户.加入的小聊天群
                        Dim I As Integer
                        For I = 0 To 加入的小聊天群.Length - 1
                            If String.Compare(加入的小聊天群(I).群主.英语讯宝地址, 群主英语讯宝地址) = 0 AndAlso
                                加入的小聊天群(I).编号 = 群编号 Then
                                机器人.说(界面文字.获取(102, "你已加入该聊天群。"))
                                Return
                            End If
                        Next
                        If 加入的小聊天群.Length >= 最大值_常量集合.每个用户可加入的小聊天群数量 Then
                            机器人.说(界面文字.获取(174, "你加入的小聊天群数量已达上限。"))
                            Return
                        End If
                    End If
                    Dim 新群 As New 类_聊天群_小
                    新群.群主 = 聊天对象.讯友或群主
                    新群.备注 = 参数1
                    新群.编号 = 群编号
                    新群.待加入确认 = True
                    If 当前用户.加入的小聊天群 IsNot Nothing Then
                        ReDim Preserve 当前用户.加入的小聊天群(当前用户.加入的小聊天群.Length)
                        当前用户.加入的小聊天群(当前用户.加入的小聊天群.Length - 1) = 新群
                    Else
                        ReDim 当前用户.加入的小聊天群(0)
                        当前用户.加入的小聊天群(0) = 新群
                    End If
                    Dim 聊天对象2 As New 类_聊天对象
                    聊天对象2.讯友或群主 = 新群.群主
                    聊天对象2.小聊天群 = 新群
                    主窗体1.添加聊天控件(聊天对象2)
                    数据库_更新最近互动讯友排名(群主英语讯宝地址, 群编号)
                Case "JoinLargeGroup"
跳转点2:
                    Dim 群编号 As Long
                    If Long.TryParse(参数2, 群编号) = False Then Return
                    If 群编号 = 0 Then Return
                    If 当前用户.加入的大聊天群 IsNot Nothing Then
                        Dim 加入的大聊天群() As 类_聊天群_大 = 当前用户.加入的大聊天群
                        Dim J As Integer
                        For J = 0 To 加入的大聊天群.Length - 1
                            If String.Compare(加入的大聊天群(J).子域名, 参数1) = 0 AndAlso
                                加入的大聊天群(J).编号 = 群编号 Then
                                机器人.说(界面文字.获取(102, "你已加入该聊天群。"))
                                Return
                            End If
                        Next
                        If 加入的大聊天群.Length >= 最大值_常量集合.每个用户可加入的大聊天群数量 Then
                            机器人.说(界面文字.获取(279, "你加入的大聊天群数量已达上限。"))
                            Return
                        End If
                    End If
                    Dim SS包生成器 As New 类_SS包生成器()
                    SS包生成器.添加_有标签("发送序号", 当前用户.讯宝发送序号)
                    SS包生成器.添加_有标签("子域名", 参数1)
                    SS包生成器.添加_有标签("群编号", 群编号)
                    If 机器人.任务 IsNot Nothing Then 机器人.任务.结束()
                    机器人.任务 = New 类_任务(任务名称_加入大聊天群, 机器人)
                    机器人.说(界面文字.获取(7, "请稍等。"))
                    机器人.启动HTTPS访问线程(New 类_访问设置(获取传送服务器访问路径开头(当前用户.主机名, 当前用户.域名_英语, False) & "C=JoinLargeGroup&UserID=" & 当前用户.编号 & "&Position=" & 当前用户.位置号 & "&DeviceType=" & 设备类型_电脑, 20000, SS包生成器.生成SS包(当前用户.AES加密器)))
            End Select
        End If
    End Sub

    Friend Sub 滚动至顶部()
        If 起始时刻 > 0 Then
            Dim 讯宝() As 讯宝_复合数据 = Nothing
            Dim 讯宝数量 As Integer
            Call 数据库_读取讯宝(讯宝, 讯宝数量)
            If 讯宝数量 > 0 Then
                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("LoadEarlierStart();")
                Dim I, 上限 As Integer
                上限 = 讯宝数量 - 1
                Dim 时间提示文本 As String
                If TypeOf 机器人 IsNot 类_机器人_主控 Then
                    If 聊天对象.小聊天群 IsNot Nothing Then
                        If 讯宝数量 < 讯宝.Length Then
                            起始时刻 = 0
                        Else
                            起始时刻 = 讯宝(讯宝数量 - 1).存储时间
                        End If
                        If 聊天对象.小聊天群.编号 = 0 Then
                            For I = 0 To 上限
                                With 讯宝(I)
                                    If I < 上限 Then
                                        If Math.Abs(DateDiff(DateInterval.Second, Date.FromBinary(讯宝(I + 1).发送时间), Date.FromBinary(.发送时间))) > 60 Then
                                            GoTo 跳转点1
                                        Else
                                            时间提示文本 = ""   '不能赋值为 Nothing
                                        End If
                                    Else
跳转点1:
                                        时间提示文本 = 时间格式(Date.FromBinary(.发送时间))
                                    End If
                                    If .是接收者 = False Then
                                        讯友说(.收发者讯宝地址, .发送时间, .发送序号, .讯宝指令, .文本, .宽度, .高度, .秒数, .已收听, 时间提示文本)
                                    Else
                                        Select Case .讯宝指令
                                            Case 讯宝指令_常量集合.发送文字
                                                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Text('" & .存储时间 & "', '" & 替换HTML和JS敏感字符(.文本) & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间提示文本 & "');")
                                            Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送短视频
                                                Dim 路径 As String
                                                If Path.GetFileName(.文本.Contains(特征字符_下划线)) = False Then
                                                    路径 = 处理文件路径以用作JS函数参数(.文本)
                                                Else
                                                    路径 = 获取传送服务器访问路径开头(当前用户.主机名, 当前用户.域名_英语, True) & "Position=" & 当前用户.位置号 & "&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&FileName=" & 替换URI敏感字符(.文本)
                                                End If
                                                Select Case .讯宝指令
                                                    Case 讯宝指令_常量集合.发送图片
                                                        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Img('" & .存储时间 & "', '" & 路径 & "', '" & .宽度 & "', '" & .高度 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间提示文本 & "');")
                                                    Case 讯宝指令_常量集合.发送语音
                                                        Dim 文本 As String = 界面文字.获取(258, "语音：#% 秒", New Object() { .秒数})
                                                        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Voice('" & .存储时间 & "', '" & 替换HTML和JS敏感字符(文本) & "', '" & 路径 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间提示文本 & "');")
                                                    Case 讯宝指令_常量集合.发送短视频
                                                        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Video('" & .存储时间 & "', '" & 路径 & "', '" & .宽度 & "', '" & .高度 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间提示文本 & "');")
                                                End Select
                                            Case 讯宝指令_常量集合.发送文件
                                                Dim 原始文件名 As String = ""
                                                Dim 路径 As String
                                                If .文本.StartsWith(SS包标识_纯文本) = False Then
                                                    原始文件名 = Path.GetFileName(.文本)
                                                    路径 = 处理文件路径以用作JS函数参数(.文本)
                                                Else
                                                    Try
                                                        Dim SS包解读器2 As New 类_SS包解读器
                                                        SS包解读器2.解读纯文本(.文本)
                                                        SS包解读器2.读取_有标签("O", 原始文件名)
                                                        Dim 存储文件名 As String = ""
                                                        SS包解读器2.读取_有标签("S", 存储文件名)
                                                        路径 = 获取传送服务器访问路径开头(当前用户.主机名, 当前用户.域名_英语, True) & "Position=" & 当前用户.位置号 & "&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&FileName=" & 替换URI敏感字符(存储文件名)
                                                    Catch ex As Exception
                                                        Continue For
                                                    End Try
                                                End If
                                                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_File('" & .存储时间 & "', '" & 处理文件路径以用作JS函数参数(原始文件名) & "', '" & 路径 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                        End Select
                                    End If
                                End With
                            Next
                        Else
                            For I = 0 To 上限
                                With 讯宝(I)
                                    Select Case .讯宝指令
                                        Case 讯宝指令_常量集合.某人加入聊天群
                                            机器人.说(界面文字.获取(175, "#% 加入了本群。", (New String() {替换HTML和JS敏感字符(.文本)})), .发送时间)
                                        Case 讯宝指令_常量集合.退出小聊天群
                                            机器人.说(界面文字.获取(178, "#% 离开了本群。", (New String() {替换HTML和JS敏感字符(.文本)})), .发送时间)
                                        Case 讯宝指令_常量集合.删减聊天群成员
                                            机器人.说(界面文字.获取(190, "群主让 #% 离开了本群。", (New String() {替换HTML和JS敏感字符(.文本)})), .发送时间)
                                        Case 讯宝指令_常量集合.修改聊天群名称
                                            机器人.说(界面文字.获取(185, "本群名称更改为 #%。", (New String() {替换HTML和JS敏感字符(.文本)})), .发送时间)
                                        Case Else
                                            If I < 上限 Then
                                                If Math.Abs(DateDiff(DateInterval.Second, Date.FromBinary(讯宝(I + 1).发送时间), Date.FromBinary(.发送时间))) > 60 Then
                                                    GoTo 跳转点2
                                                Else
                                                    时间提示文本 = ""   '不能赋值为 Nothing
                                                End If
                                            Else
跳转点2:
                                                时间提示文本 = 时间格式(Date.FromBinary(.发送时间))
                                            End If
                                            If String.Compare(.收发者讯宝地址, 当前用户.英语讯宝地址) <> 0 Then
                                                讯友说(.收发者讯宝地址, .发送时间, .发送序号, .讯宝指令, .文本, .宽度, .高度, .秒数, .已收听, 时间提示文本)
                                            Else
                                                Select Case .讯宝指令
                                                    Case 讯宝指令_常量集合.发送文字
                                                        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Text('" & .存储时间 & "', '" & 替换HTML和JS敏感字符(.文本) & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间提示文本 & "');")
                                                    Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送短视频
                                                        Dim 路径 As String
                                                        If Path.GetFileName(.文本).Contains(特征字符_下划线) = False Then
                                                            路径 = 处理文件路径以用作JS函数参数(.文本)
                                                        Else
                                                            路径 = 获取传送服务器访问路径开头(当前用户.主机名, 当前用户.域名_英语, True) & "Position=" & 当前用户.位置号 & "&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&FileName=" & 替换URI敏感字符(.文本)
                                                        End If
                                                        Select Case .讯宝指令
                                                            Case 讯宝指令_常量集合.发送图片
                                                                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Img('" & .存储时间 & "', '" & 路径 & "', '" & .宽度 & "', '" & .高度 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间提示文本 & "');")
                                                            Case 讯宝指令_常量集合.发送语音
                                                                Dim 文本 As String = 界面文字.获取(258, "语音：#% 秒", New Object() { .秒数})
                                                                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Voice('" & .存储时间 & "', '" & 替换HTML和JS敏感字符(文本) & "', '" & 路径 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间提示文本 & "');")
                                                            Case 讯宝指令_常量集合.发送短视频
                                                                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Video('" & .存储时间 & "', '" & 路径 & "', '" & .宽度 & "', '" & .高度 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间提示文本 & "');")
                                                        End Select
                                                    Case 讯宝指令_常量集合.发送文件
                                                        Dim 原始文件名 As String = ""
                                                        Dim 路径 As String
                                                        If .文本.StartsWith(SS包标识_纯文本) = False Then
                                                            原始文件名 = Path.GetFileName(.文本)
                                                            路径 = 处理文件路径以用作JS函数参数(.文本)
                                                        Else
                                                            Try
                                                                Dim SS包解读器2 As New 类_SS包解读器
                                                                SS包解读器2.解读纯文本(.文本)
                                                                SS包解读器2.读取_有标签("O", 原始文件名)
                                                                Dim 存储文件名 As String = ""
                                                                SS包解读器2.读取_有标签("S", 存储文件名)
                                                                路径 = 获取传送服务器访问路径开头(当前用户.主机名, 当前用户.域名_英语, True) & "Position=" & 当前用户.位置号 & "&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&FileName=" & 替换URI敏感字符(存储文件名)
                                                            Catch ex As Exception
                                                                Continue For
                                                            End Try
                                                        End If
                                                        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_File('" & .存储时间 & "', '" & 处理文件路径以用作JS函数参数(原始文件名) & "', '" & 路径 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                                End Select
                                            End If
                                    End Select
                                End With
                            Next
                        End If
                        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("LoadEnd(false);")
                    Else
                        If 讯宝数量 < 讯宝.Length Then
                            起始时刻 = 0
                        Else
                            起始时刻 = 讯宝(讯宝数量 - 1).发送时间
                        End If
                        For I = 0 To 上限
                            With 讯宝(I)
                                If I < 上限 Then
                                    If Math.Abs(DateDiff(DateInterval.Second, Date.FromBinary(讯宝(I + 1).发送时间), Date.FromBinary(.发送时间))) > 60 Then
                                        GoTo 跳转点3
                                    Else
                                        时间提示文本 = ""   '不能赋值为 Nothing
                                    End If
                                Else
跳转点3:
                                    时间提示文本 = 时间格式(Date.FromBinary(.发送时间))
                                End If
                                If String.Compare(.收发者讯宝地址, 当前用户.英语讯宝地址) <> 0 Then
                                    讯友说(.收发者讯宝地址, .发送时间, .发送序号, .讯宝指令, .文本, .宽度, .高度, .秒数, .已收听, 时间提示文本)
                                Else
                                    Select Case .讯宝指令
                                        Case 讯宝指令_常量集合.发送文字
                                            浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Text('" & .发送时间 & "', '" & 替换HTML和JS敏感字符(.文本) & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间提示文本 & "');")
                                        Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送短视频
                                            Dim 路径 As String
                                            If Path.GetFileName(.文本).Contains(特征字符_下划线) = False Then
                                                路径 = 处理文件路径以用作JS函数参数(.文本)
                                            Else
                                                路径 = 获取大聊天群服务器访问路径开头(聊天对象.大聊天群.子域名, True) & "GroupID=" & 聊天对象.大聊天群.编号 & "&FileName=" & 替换URI敏感字符(.文本)
                                            End If
                                            Select Case .讯宝指令
                                                Case 讯宝指令_常量集合.发送图片
                                                    浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Img('" & .发送时间 & "', '" & 路径 & "', '" & .宽度 & "', '" & .高度 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间提示文本 & "', 'true');")
                                                Case 讯宝指令_常量集合.发送语音
                                                    Dim 文本 As String = 界面文字.获取(258, "语音：#% 秒", New Object() { .秒数})
                                                    浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Voice('" & .发送时间 & "', '" & 替换HTML和JS敏感字符(文本) & "', '" & 路径 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间提示文本 & "');")
                                                Case 讯宝指令_常量集合.发送短视频
                                                    浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Video('" & .发送时间 & "', '" & 路径 & "', '" & .宽度 & "', '" & .高度 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间提示文本 & "');")
                                            End Select
                                        Case 讯宝指令_常量集合.发送文件
                                            Dim 原始文件名 As String = ""
                                            Dim 路径 As String
                                            If .文本.StartsWith(SS包标识_纯文本) = False Then
                                                原始文件名 = Path.GetFileName(.文本)
                                                路径 = 处理文件路径以用作JS函数参数(.文本)
                                            Else
                                                Try
                                                    Dim SS包解读器2 As New 类_SS包解读器
                                                    SS包解读器2.解读纯文本(.文本)
                                                    SS包解读器2.读取_有标签("O", 原始文件名)
                                                    Dim 存储文件名 As String = ""
                                                    SS包解读器2.读取_有标签("S", 存储文件名)
                                                    路径 = 获取大聊天群服务器访问路径开头(聊天对象.大聊天群.子域名, True) & "GroupID=" & 聊天对象.大聊天群.编号 & "&FileName=" & 替换URI敏感字符(存储文件名)
                                                Catch ex As Exception
                                                    Continue For
                                                End Try
                                            End If
                                            浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_File('" & .存储时间 & "', '" & 处理文件路径以用作JS函数参数(原始文件名) & "', '" & 路径 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 时间格式(Date.FromBinary(.发送时间)) & "');")
                                    End Select
                                End If
                            End With
                        Next
                        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("LoadEnd(true);")
                    End If
                Else
                    If 讯宝数量 < 讯宝.Length Then
                        起始时刻 = 0
                    Else
                        起始时刻 = 讯宝(讯宝数量 - 1).发送时间
                    End If
                    For I = 0 To 上限
                        With 讯宝(I)
                            If I < 上限 Then
                                If Math.Abs(DateDiff(DateInterval.Second, Date.FromBinary(讯宝(I + 1).发送时间), Date.FromBinary(.发送时间))) > 60 Then
                                    GoTo 跳转点4
                                Else
                                    时间提示文本 = ""   '不能赋值为 Nothing
                                End If
                            Else
跳转点4:
                                时间提示文本 = 时间格式(Date.FromBinary(.发送时间))
                            End If
                            陌生人说(.收发者讯宝地址, .发送时间, .发送序号, .讯宝指令, .文本, 时间提示文本)
                        End With
                    Next
                    浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("LoadEnd(false);")
                End If
            Else
                起始时刻 = 0
            End If
        End If
    End Sub

    Private Sub 撤回讯宝(ByVal 发送序号或存储时间 As Long)
        Dim 读取器 As 类_读取器_外部 = Nothing
        Dim 发送序号 As Long
        Try
            Dim 表名称 As String = Nothing
            Dim 索引名称 As String = Nothing
            Dim 列添加器 As New 类_列添加器
            If 聊天对象.小聊天群 IsNot Nothing Then
                If 聊天对象.小聊天群.编号 = 0 Then
                    表名称 = "一对一讯宝"
                    列添加器.添加列_用于筛选器("讯宝地址", 筛选方式_常量集合.等于, 聊天对象.讯友或群主.英语讯宝地址)
                    列添加器.添加列_用于筛选器("是接收者", 筛选方式_常量集合.等于, True)
                    列添加器.添加列_用于筛选器("存储时间", 筛选方式_常量集合.等于, 发送序号或存储时间)
                    索引名称 = "#地址是接收者存储时间"
                Else
                    表名称 = "小聊天群讯宝"
                    列添加器.添加列_用于筛选器("群主讯宝地址", 筛选方式_常量集合.等于, 聊天对象.讯友或群主.英语讯宝地址)
                    列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 聊天对象.小聊天群.编号)
                    列添加器.添加列_用于筛选器("发送者讯宝地址", 筛选方式_常量集合.等于, 当前用户.英语讯宝地址)
                    列添加器.添加列_用于筛选器("存储时间", 筛选方式_常量集合.等于, 发送序号或存储时间)
                    索引名称 = "#群主编号发送者存储时间"
                End If
            ElseIf 聊天对象.大聊天群 IsNot Nothing Then
                表名称 = "大聊天群讯宝"
                列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 聊天对象.大聊天群.子域名)
                列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 聊天对象.大聊天群.编号)
                列添加器.添加列_用于筛选器("发送时间", 筛选方式_常量集合.等于, 发送序号或存储时间)
                索引名称 = "#子域名群编号发送时间"
            End If
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            If 聊天对象.小聊天群 IsNot Nothing Then
                列添加器.添加列_用于获取数据(New String() {"指令", "文本库号", "文本编号", "发送序号"})
            ElseIf 聊天对象.大聊天群 IsNot Nothing Then
                列添加器.添加列_用于获取数据(New String() {"指令", "文本库号", "文本编号", "发送时间"})
            End If
            Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, 表名称, 筛选器, 1, 列添加器, , 索引名称)
            Dim 讯宝指令 As 讯宝指令_常量集合
            Dim 文本库号 As Short
            Dim 文本编号 As Long
            读取器 = 指令2.执行()
            While 读取器.读取
                讯宝指令 = 读取器(0)
                文本库号 = 读取器(1)
                文本编号 = 读取器(2)
                发送序号 = 读取器(3)
                Exit While
            End While
            读取器.关闭()
            列添加器 = New 类_列添加器
            If 聊天对象.小聊天群 IsNot Nothing Then
                If 聊天对象.小聊天群.编号 = 0 Then
                    列添加器.添加列_用于筛选器("讯宝地址", 筛选方式_常量集合.等于, 聊天对象.讯友或群主.英语讯宝地址)
                    列添加器.添加列_用于筛选器("是接收者", 筛选方式_常量集合.等于, True)
                    列添加器.添加列_用于筛选器("存储时间", 筛选方式_常量集合.等于, 发送序号或存储时间)
                Else
                    列添加器.添加列_用于筛选器("群主讯宝地址", 筛选方式_常量集合.等于, 聊天对象.讯友或群主.英语讯宝地址)
                    列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 聊天对象.小聊天群.编号)
                    列添加器.添加列_用于筛选器("发送者讯宝地址", 筛选方式_常量集合.等于, 当前用户.英语讯宝地址)
                    列添加器.添加列_用于筛选器("存储时间", 筛选方式_常量集合.等于, 发送序号或存储时间)
                End If
            ElseIf 聊天对象.大聊天群 IsNot Nothing Then
                列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 聊天对象.大聊天群.子域名)
                列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 聊天对象.大聊天群.编号)
                列添加器.添加列_用于筛选器("发送时间", 筛选方式_常量集合.等于, 发送序号或存储时间)
            End If
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, 表名称, 筛选器, 索引名称)
            If 指令.执行() > 0 Then
                If 文本库号 > 0 Then
                    Select Case 讯宝指令
                        Case 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 文本编号)
                            筛选器 = New 类_筛选器
                            筛选器.添加一组筛选条件(列添加器)
                            列添加器 = New 类_列添加器
                            列添加器.添加列_用于获取数据("文本")
                            指令2 = New 类_数据库指令_请求获取数据(副数据库, 文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
                            Dim 文本 As String = Nothing
                            读取器 = 指令2.执行()
                            While 读取器.读取
                                文本 = 读取器(0)
                                Exit While
                            End While
                            读取器.关闭()
                            If String.IsNullOrEmpty(文本) = False Then
                                If File.Exists(文本) = True Then
                                    If 文本.StartsWith(My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData) Then
                                        Try
                                            File.Delete(文本)
                                        Catch ex As Exception
                                        End Try
                                    End If
                                Else
                                    文本 = My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData & "\" & 当前用户.英语讯宝地址 & "\" & 文本
                                    If File.Exists(文本) Then
                                        Try
                                            File.Delete(文本)
                                        Catch ex As Exception
                                        End Try
                                    End If
                                End If
                                If 讯宝指令 = 讯宝指令_常量集合.发送短视频 Then
                                    文本 &= ".jpg"
                                    If File.Exists(文本) = True Then
                                        Try
                                            File.Delete(文本)
                                        Catch ex As Exception
                                        End Try
                                    End If
                                End If
                            End If
                    End Select
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 文本编号)
                    筛选器 = New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    指令 = New 类_数据库指令_删除数据(副数据库, 文本库号 & "库", 筛选器, 主键索引名)
                    指令.执行()
                End If
                浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("RemoveSS('" & 发送序号或存储时间 & "');")
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            机器人.说(ex.Message)
            Return
        End Try
        If 发送序号 > 0 Then
            If 聊天对象.小聊天群 IsNot Nothing Then
                Dim 成功 As Boolean
                If 聊天对象.小聊天群.编号 = 0 Then
                    成功 = 数据库_保存要发送的一对一讯宝(机器人, 聊天对象.讯友或群主.英语讯宝地址, Date.UtcNow.Ticks, 讯宝指令_常量集合.撤回, 发送序号)
                Else
                    成功 = 数据库_保存要发送的小聊天群讯宝(机器人, 聊天对象.讯友或群主.英语讯宝地址, 聊天对象.小聊天群.编号, Date.UtcNow.Ticks, 讯宝指令_常量集合.撤回, 发送序号)
                End If
                If 成功 Then
                    主窗体1.发送讯宝()
                End If
            ElseIf 聊天对象.大聊天群 IsNot Nothing Then
                CType(机器人, 类_机器人_大聊天群).发送或接收(讯宝指令_常量集合.撤回, 发送序号)
            End If
        End If
    End Sub

    Private Sub 删除讯宝(ByVal 发送序号或存储时间 As Long, ByVal 发送者 As String)
        Dim 决定 As DialogResult = MsgBox(界面文字.获取(195, "删除吗？"), MsgBoxStyle.YesNo Or MsgBoxStyle.Information Or MsgBoxStyle.DefaultButton2)
        If 决定 = DialogResult.Yes Then
            Dim 读取器 As 类_读取器_外部 = Nothing
            Try
                Dim 表名称 As String = Nothing
                Dim 索引名称 As String = Nothing
                Dim 列添加器 As New 类_列添加器
                If 聊天对象.小聊天群 IsNot Nothing Then
                    If 聊天对象.小聊天群.编号 = 0 Then
                        表名称 = "一对一讯宝"
                        列添加器.添加列_用于筛选器("讯宝地址", 筛选方式_常量集合.等于, 聊天对象.讯友或群主.英语讯宝地址)
                        If String.IsNullOrEmpty(发送者) Then
                            列添加器.添加列_用于筛选器("是接收者", 筛选方式_常量集合.等于, True)
                            列添加器.添加列_用于筛选器("存储时间", 筛选方式_常量集合.等于, 发送序号或存储时间)
                            索引名称 = "#地址是接收者存储时间"
                        Else
                            列添加器.添加列_用于筛选器("是接收者", 筛选方式_常量集合.等于, False)
                            列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, 发送序号或存储时间)
                            索引名称 = "#地址是接收者发送序号"
                        End If
                    Else
                        表名称 = "小聊天群讯宝"
                        列添加器.添加列_用于筛选器("群主讯宝地址", 筛选方式_常量集合.等于, 聊天对象.讯友或群主.英语讯宝地址)
                        列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 聊天对象.小聊天群.编号)
                        If String.IsNullOrEmpty(发送者) Then
                            列添加器.添加列_用于筛选器("发送者讯宝地址", 筛选方式_常量集合.等于, 当前用户.英语讯宝地址)
                            列添加器.添加列_用于筛选器("存储时间", 筛选方式_常量集合.等于, 发送序号或存储时间)
                            索引名称 = "#群主编号发送者存储时间"
                        Else
                            列添加器.添加列_用于筛选器("发送者讯宝地址", 筛选方式_常量集合.等于, 发送者)
                            列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, 发送序号或存储时间)
                            索引名称 = "#群主编号发送者发送序号"
                        End If
                    End If
                ElseIf 聊天对象.大聊天群 IsNot Nothing Then
                    表名称 = "大聊天群讯宝"
                    列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 聊天对象.大聊天群.子域名)
                    列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 聊天对象.大聊天群.编号)
                    列添加器.添加列_用于筛选器("发送时间", 筛选方式_常量集合.等于, 发送序号或存储时间)
                    If String.IsNullOrEmpty(发送者) Then
                        列添加器.添加列_用于筛选器("发送者讯宝地址", 筛选方式_常量集合.等于, 当前用户.英语讯宝地址)
                    Else
                        列添加器.添加列_用于筛选器("发送者讯宝地址", 筛选方式_常量集合.等于, 发送者)
                    End If
                    索引名称 = "#子域名群编号发送时间"
                End If
                Dim 筛选器 As New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据(New String() {"指令", "文本库号", "文本编号"})
                Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, 表名称, 筛选器, 1, 列添加器, , 索引名称)
                Dim 讯宝指令 As 讯宝指令_常量集合
                Dim 文本库号 As Short
                Dim 文本编号 As Long
                读取器 = 指令2.执行()
                While 读取器.读取
                    讯宝指令 = 读取器(0)
                    文本库号 = 读取器(1)
                    文本编号 = 读取器(2)
                    Exit While
                End While
                读取器.关闭()
                列添加器 = New 类_列添加器
                If 聊天对象.小聊天群 IsNot Nothing Then
                    If 聊天对象.小聊天群.编号 = 0 Then
                        列添加器.添加列_用于筛选器("讯宝地址", 筛选方式_常量集合.等于, 聊天对象.讯友或群主.英语讯宝地址)
                        If String.IsNullOrEmpty(发送者) Then
                            列添加器.添加列_用于筛选器("是接收者", 筛选方式_常量集合.等于, True)
                            列添加器.添加列_用于筛选器("存储时间", 筛选方式_常量集合.等于, 发送序号或存储时间)
                        Else
                            列添加器.添加列_用于筛选器("是接收者", 筛选方式_常量集合.等于, False)
                            列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, 发送序号或存储时间)
                        End If
                    Else
                        列添加器.添加列_用于筛选器("群主讯宝地址", 筛选方式_常量集合.等于, 聊天对象.讯友或群主.英语讯宝地址)
                        列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 聊天对象.小聊天群.编号)
                        If String.IsNullOrEmpty(发送者) Then
                            列添加器.添加列_用于筛选器("发送者讯宝地址", 筛选方式_常量集合.等于, 当前用户.英语讯宝地址)
                            列添加器.添加列_用于筛选器("存储时间", 筛选方式_常量集合.等于, 发送序号或存储时间)
                        Else
                            列添加器.添加列_用于筛选器("发送者讯宝地址", 筛选方式_常量集合.等于, 发送者)
                            列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, 发送序号或存储时间)
                        End If
                    End If
                ElseIf 聊天对象.大聊天群 IsNot Nothing Then
                    列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 聊天对象.大聊天群.子域名)
                    列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 聊天对象.大聊天群.编号)
                    列添加器.添加列_用于筛选器("发送时间", 筛选方式_常量集合.等于, 发送序号或存储时间)
                    If String.IsNullOrEmpty(发送者) Then
                        列添加器.添加列_用于筛选器("发送者讯宝地址", 筛选方式_常量集合.等于, 当前用户.英语讯宝地址)
                    Else
                        列添加器.添加列_用于筛选器("发送者讯宝地址", 筛选方式_常量集合.等于, 发送者)
                    End If
                End If
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令 As New 类_数据库指令_删除数据(副数据库, 表名称, 筛选器, 索引名称)
                If 指令.执行() > 0 Then
                    If 文本库号 > 0 Then
                        Select Case 讯宝指令
                            Case 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 文本编号)
                                筛选器 = New 类_筛选器
                                筛选器.添加一组筛选条件(列添加器)
                                列添加器 = New 类_列添加器
                                列添加器.添加列_用于获取数据("文本")
                                指令2 = New 类_数据库指令_请求获取数据(副数据库, 文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
                                Dim 文本 As String = Nothing
                                读取器 = 指令2.执行()
                                While 读取器.读取
                                    文本 = 读取器(0)
                                    Exit While
                                End While
                                读取器.关闭()
                                If String.IsNullOrEmpty(文本) = False Then
                                    If File.Exists(文本) = True Then
                                        If 文本.StartsWith(My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData) Then
                                            Try
                                                File.Delete(文本)
                                            Catch ex As Exception
                                            End Try
                                        End If
                                    Else
                                        文本 = My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData & "\" & 当前用户.英语讯宝地址 & "\" & 文本
                                        If File.Exists(文本) Then
                                            Try
                                                File.Delete(文本)
                                            Catch ex As Exception
                                            End Try
                                        End If
                                    End If
                                    If 讯宝指令 = 讯宝指令_常量集合.发送短视频 Then
                                        文本 &= ".jpg"
                                        If File.Exists(文本) = True Then
                                            Try
                                                File.Delete(文本)
                                            Catch ex As Exception
                                            End Try
                                        End If
                                    End If
                                End If
                        End Select
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 文本编号)
                        筛选器 = New 类_筛选器
                        筛选器.添加一组筛选条件(列添加器)
                        指令 = New 类_数据库指令_删除数据(副数据库, 文本库号 & "库", 筛选器, 主键索引名)
                        指令.执行()
                    End If
                    Dim id As String
                    If String.IsNullOrEmpty(发送者) Then
                        id = 发送序号或存储时间
                    Else
                        id = 发送者 & ":" & 发送序号或存储时间
                    End If
                    浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("RemoveSS('" & id & "');")
                End If
            Catch ex As Exception
                If 读取器 IsNot Nothing Then 读取器.关闭()
                机器人.说(ex.Message)
            End Try
        End If
    End Sub

    Private Sub 删除陌生人讯宝(ByVal 发送者 As String)
        Dim 决定 As DialogResult = MsgBox(界面文字.获取(195, "删除吗？"), MsgBoxStyle.YesNo Or MsgBoxStyle.Information Or MsgBoxStyle.DefaultButton2)
        If 决定 = DialogResult.Yes Then
            Dim 讯宝(99) As 讯宝_复合数据
            Dim 讯宝数量 As Integer
            Dim 读取器 As 类_读取器_外部 = Nothing
            Try
                Dim 列添加器 As New 类_列添加器
                列添加器.添加列_用于筛选器("讯宝地址", 筛选方式_常量集合.等于, 发送者)
                Dim 筛选器 As New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据(New String() {"指令", "文本库号", "文本编号"})
                Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, "陌生人讯宝", 筛选器, 1, 列添加器, , "#地址发送序号")
                读取器 = 指令2.执行()
                While 读取器.读取
                    If 讯宝数量 = 讯宝.Length Then ReDim Preserve 讯宝(讯宝数量 * 2 - 1)
                    With 讯宝(讯宝数量)
                        .讯宝指令 = 读取器(0)
                        .文本库号 = 读取器(1)
                        .文本编号 = 读取器(2)
                    End With
                    讯宝数量 += 1
                End While
                读取器.关闭()
                If 讯宝数量 = 0 Then Return
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("讯宝地址", 筛选方式_常量集合.等于, 发送者)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令 As New 类_数据库指令_删除数据(副数据库, "陌生人讯宝", 筛选器, "#地址发送序号")
                If 指令.执行() > 0 Then
                    Dim I As Integer
                    For I = 0 To 讯宝数量 - 1
                        With 讯宝(I)
                            If .文本库号 > 0 Then
                                列添加器 = New 类_列添加器
                                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, .文本编号)
                                筛选器 = New 类_筛选器
                                筛选器.添加一组筛选条件(列添加器)
                                指令 = New 类_数据库指令_删除数据(副数据库, .文本库号 & "库", 筛选器, 主键索引名)
                                指令.执行()
                            End If
                        End With
                    Next
                    浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("RemoveStrangerSS('" & 发送者 & "');")
                End If
            Catch ex As Exception
                If 读取器 IsNot Nothing Then 读取器.关闭()
                机器人.说(ex.Message)
            End Try
        End If
    End Sub

    Friend Sub 发送者撤回(ByVal 发送者英语讯宝地址 As String, ByVal 发送序号 As Long, ByVal 发送时间 As Long)
        If 数据库_撤回讯宝(发送者英语讯宝地址, 聊天对象.小聊天群.编号, 聊天对象.讯友或群主.英语讯宝地址, 发送序号, 发送时间) = True Then
            浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("RemoveSS('" & 发送者英语讯宝地址 & ":" & 发送序号 & "');")
            If 聊天对象.小聊天群.编号 = 0 Then
                机器人.说(界面文字.获取(196, "讯友撤回了一条讯宝。"))
            Else
                Dim I As Integer
                Dim 群成员() As 类_群成员 = 聊天对象.小聊天群.群成员
                If 群成员 IsNot Nothing Then
                    For I = 0 To 群成员.Length - 1
                        If String.Compare(群成员(I).英语讯宝地址, 发送者英语讯宝地址) = 0 Then Exit For
                    Next
                    If I < 群成员.Length Then
                        Dim 谁 As String = IIf(String.IsNullOrEmpty(群成员(I).本国语讯宝地址), "", 群成员(I).本国语讯宝地址 & " / ") & 发送者英语讯宝地址
                        机器人.说(界面文字.获取(197, "#% 撤回了一条讯宝。", New Object() {谁}))
                    Else
跳转点1:
                        机器人.说(界面文字.获取(197, "#% 撤回了一条讯宝。", New Object() {发送者英语讯宝地址}))
                    End If
                ElseIf 当前用户.讯友目录 IsNot Nothing Then
                    Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
                    For I = 0 To 讯友目录.Length - 1
                        If String.Compare(讯友目录(I).英语讯宝地址, 发送者英语讯宝地址) = 0 Then Exit For
                    Next
                    If I < 讯友目录.Length Then
                        With 讯友目录(I)
                            Dim 谁 As String = IIf(String.IsNullOrEmpty(.本国语讯宝地址), "", .本国语讯宝地址 & " / ") & 发送者英语讯宝地址
                            机器人.说(界面文字.获取(197, "#% 撤回了一条讯宝。", New Object() {谁}))
                        End With
                    Else
                        GoTo 跳转点1
                    End If
                Else
                    GoTo 跳转点1
                End If
            End If
        End If
    End Sub

    Friend Sub 发送语音还是文字(ByVal 语音 As Boolean)
        发送语音 = 语音
        载入任务名称()
        If 语音 Then
            进度条.Visible = True
            输入框.Visible = False
            按钮_说话.Text = 界面文字.获取(255, "录制语音")
            文字_字数.Text = 界面文字.获取(2, "刷新")
        Else
            输入框.Visible = True
            进度条.Visible = False
            If TypeOf 机器人 IsNot 类_机器人_大聊天群 Then
                按钮_说话.Text = 界面文字.获取(1, "说话")
            Else
                If String.IsNullOrEmpty(输入框.Text.Trim) Then
                    按钮_说话.Text = 界面文字.获取(2, "刷新")
                Else
                    按钮_说话.Text = 界面文字.获取(1, "说话")
                End If
            End If
            文字_字数.Text = 输入框.Text.Length
        End If
    End Sub

    Private Sub 进度条_Click(sender As Object, e As EventArgs) Handles 进度条.Click
        If 主窗体1.录音类 Is Nothing Then Return
        If 主窗体1.录音类.正在录音 = False Then Return
        If Equals(Me, 主窗体1.当前录音控件) = True Then
            主窗体1.当前录音控件.取消录音 = True
            主窗体1.录音类.停止录音()
        End If
    End Sub

    Private Sub 控件_聊天_VisibleChanged(sender As Object, e As EventArgs) Handles Me.VisibleChanged
        Call 进度条_Click(Nothing, Nothing)
    End Sub

    Friend Sub 注销时清除主控机器人聊天内容()
        载入了陌生人讯宝 = False
        载入任务名称()
        浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("ClearSS();")
        浏览器_小宇宙.Load(获取主站首页的访问路径())
    End Sub

    Private Sub 文字_字数_Click(sender As Object, e As EventArgs) Handles 文字_字数.Click
        If TypeOf 机器人 IsNot 类_机器人_大聊天群 Then Return
        If 发送语音 = False Then Return
        If 主窗体1.录音类 IsNot Nothing Then If 主窗体1.录音类.正在录音 = True Then Return
        CType(机器人, 类_机器人_大聊天群).发送或接收(讯宝指令_常量集合.无)
    End Sub

    Friend Sub 收到子域名(ByVal 子域名 As String)
        If InvokeRequired Then
            Dim d As New 收到子域名_跨线程(AddressOf 收到子域名)
            Invoke(d, New Object() {子域名})
        Else
            当前用户.获取小宇宙凭据(Me, 子域名)
        End If
    End Sub

    Friend Sub 收到小宇宙的连接凭据(ByVal 子域名 As String, ByVal 连接凭据 As String, ByVal 是商品编辑 As Boolean, Optional ByVal 是写入凭据 As Boolean = False)
        If 是写入凭据 = False Then
            If TypeOf 机器人 Is 类_机器人_主控 Then
                浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("ReadCredentialReady('" & 子域名 & "', '" & 替换HTML和JS敏感字符(连接凭据) & "', '" & 当前用户.英语用户名 & "', '" & 替换HTML和JS敏感字符(当前用户.英语讯宝地址) & "', '" & IIf(是商品编辑, "true", "false") & "');")
            ElseIf TypeOf 机器人 Is 类_机器人_大聊天群 Then
                浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("ReadCredentialReady('" & 子域名 & "', '" & 替换HTML和JS敏感字符(连接凭据) & "');")
            Else
                Dim 段() As String = 聊天对象.讯友或群主.英语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("ReadCredentialReady('" & 子域名 & "', '" & 替换HTML和JS敏感字符(连接凭据) & "', '" & 段(0) & "', '" & 替换HTML和JS敏感字符(当前用户.英语讯宝地址) & "');")
            End If
        Else
            浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("WriteCredentialReady('" & 子域名 & "', '" & 替换HTML和JS敏感字符(连接凭据) & "');")
        End If
    End Sub

    Friend Sub 加载小聊天群的成员列表()
        Dim 群成员() As 类_群成员 = 聊天对象.小聊天群.群成员
        If 群成员 IsNot Nothing Then
            Dim 变长文本 As New StringBuilder(200 * 群成员.Length)
            Dim 文本写入器 As New StringWriter(变长文本)
            文本写入器.Write("<xml><MEMBERS>")
            Dim I As Short
            For I = 0 To 群成员.Length - 1
                文本写入器.Write("<MEMBER>")
                With 群成员(I)
                    文本写入器.Write("<ENGLISH>" & .英语讯宝地址 & "</ENGLISH>")
                    If String.IsNullOrEmpty(.本国语讯宝地址) = False Then
                        文本写入器.Write("<NATIVE>" & .本国语讯宝地址 & "</NATIVE>")
                    End If
                    文本写入器.Write("<ROLE>" & .角色 & "</ROLE>")
                    文本写入器.Write("<ICON>" & 获取讯友头像路径(.英语讯宝地址, .主机名) & "</ICON>")
                End With
                文本写入器.Write("</MEMBER>")
            Next
            文本写入器.Write("</MEMBERS></xml>")
            文本写入器.Close()
            浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("ListMembers('" & 文本写入器.ToString & "');")
        Else
            浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("ListMembers('');")
        End If
    End Sub

End Class
