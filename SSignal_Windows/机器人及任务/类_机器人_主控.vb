Option Strict Off
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
Imports CefSharp
Imports SSignal_GlobalCommonCode
Imports SSignal_Protocols
Imports SSignalDB

Friend Class 类_机器人_主控
    Inherits 类_机器人
    Implements IDisposable

    Dim 线程_传送服务器 As Thread
    Dim 正在连接传送服务器 As Boolean
    Friend 从未自检 As Boolean = True
    Friend 不再提示 As Boolean = False
    Dim 重试次数 As Byte
    Friend 心跳确认时间 As Long
    Dim 须通知讯宝数量 As Integer

    Private Delegate Sub 收到讯宝_跨线程(ByVal SS包解读器 As 类_SS包解读器, ByVal 是即时推送SS As Boolean)
    Private Delegate Sub 收到未推送的讯宝_跨线程(ByVal SS包解读器() As Object)
    Private Delegate Sub 连接传送服务器成功或失败_跨线程(ByVal 成功 As Boolean, ByVal 在线 As Boolean, ByVal 打开小宇宙 As Boolean)
    Private Delegate Sub HTTPS请求失败2_跨线程()

    Friend Sub New(ByVal 主窗体2 As 主窗体, ByVal 聊天控件1 As 控件_聊天)
        主窗体1 = 主窗体2
        聊天控件 = 聊天控件1
    End Sub

    Friend Sub 自检(Optional ByVal 询问 As Boolean = False, Optional ByVal 取消 As Boolean = False)
        If 任务 IsNot Nothing Then
            任务.结束()
            任务 = Nothing
            If 取消 Then
                With 聊天控件.输入框
                    If .PasswordChar <> vbNullChar Then
                        .PasswordChar = vbNullChar
                        .BackColor = Color.White
                    End If
                End With
                说(界面文字.获取(16, "已取消。"))
            End If
        ElseIf 取消 = True Then
            询问 = True
        End If
        If 当前用户.已登录() Then
            With 主窗体1.定时器_心跳
                .Stop()
                .Start()
            End With
            If 当前用户.获取了账户信息 = False Then
                获取账户信息()
                Return
            End If
            If 副数据库 Is Nothing Then
                Dim 目录路径 As String = My.Computer.FileSystem.SpecialDirectories.MyDocuments & "\" & 讯宝网络域名_英语
                If Directory.Exists(目录路径) = False Then Directory.CreateDirectory(目录路径)
                Dim 类 As New 类_打开或创建数据库
                副数据库 = 类.打开或创建用户数据库(目录路径)
                If 副数据库 Is Nothing Then
                    说(界面文字.获取(157, "无法打开本地数据库。请将数据库文件移出所在文件夹。路径：#%", New Object() {替换HTML和JS敏感字符(目录路径 & "\" & 当前用户.英语讯宝地址 & 数据库文件扩展名)}))
                    Return
                End If
                If 数据库_检查用户信息(目录路径) = False Then
                    副数据库.关闭()
                    副数据库 = Nothing
                    Return
                End If
            End If
            If 当前用户.获取了密钥 = False Then
                获取密钥()
                Return
            End If
            If 网络连接器 Is Nothing Then
                启动访问线程_传送服务器()
                Return
            End If
            If String.IsNullOrEmpty(当前用户.职能) = False Then
                If 当前用户.职能.Contains(职能_管理员) AndAlso String.IsNullOrEmpty(备份文件存放路径) = False Then
                    If Directory.Exists(备份文件存放路径) = True Then
                        If 备份管理器 Is Nothing Then
                            备份管理器 = New 类_备份管理器(主窗体1)
                            If 备份管理器.开始() = False Then
                                备份管理器 = Nothing
                            End If
                        End If
                    Else
                        说(界面文字.获取(208, "数据库备份文件存放路径不存在：#%", New Object() {备份文件存放路径}))
                        Return
                    End If
                End If
            End If
            If 询问 = True Then 说(界面文字.获取(93, "需要我做什么？"))
        Else
            说(界面文字.获取(280, "欢迎使用讯宝！你还未登录，请<a>#%</a>。如果没有账号，可以<a>#%</a>一个。如果<a>#%</a>密码了，可以重设密码。", New Object() {任务名称_登录, 任务名称_注册, 任务名称_忘记}))
        End If
    End Sub

    Private Sub 获取密钥()
        任务 = New 类_任务(任务名称_获取密钥, Me)
        If 不再提示 = False Then 说(界面文字.获取(70, "正在获取密钥。请稍等。"))
        启动HTTPS访问线程(New 类_访问设置(获取中心服务器访问路径开头(当前用户.域名_英语) & "C=GetKeyIV&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器), 20000))
    End Sub

    Private Function 数据库_检查用户信息(ByVal 目录路径 As String) As Boolean
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "用户", Nothing, 1)
            Dim 英语讯宝地址 As String = Nothing
            Dim 本国语讯宝地址 As String = Nothing
            读取器 = 指令.执行()
            While 读取器.读取
                英语讯宝地址 = 读取器(0)
                If String.IsNullOrEmpty(当前用户.域名_本国语) = False Then
                    本国语讯宝地址 = 读取器(1)
                End If
                备份文件存放路径 = 读取器(2)
                Exit While
            End While
            读取器.关闭()
            If String.IsNullOrEmpty(英语讯宝地址) = False Then
                If String.Compare(英语讯宝地址, 当前用户.英语讯宝地址) <> 0 Then
                    说(界面文字.获取(158, "本地数据库与当前用户不一致。请将数据库文件移出所在文件夹。路径：#%", New Object() {替换HTML和JS敏感字符(目录路径 & "\" & 当前用户.英语讯宝地址 & 数据库文件扩展名)}))
                    Return False
                ElseIf String.IsNullOrEmpty(当前用户.域名_本国语) Then
                    Return True
                End If
            End If
            If String.IsNullOrEmpty(本国语讯宝地址) = False Then
                If String.Compare(本国语讯宝地址, 当前用户.本国语讯宝地址) <> 0 Then
                    说(界面文字.获取(158, "本地数据库与当前用户不一致。请将数据库文件移出所在文件夹。路径：#%", New Object() {替换HTML和JS敏感字符(目录路径 & "\" & 当前用户.英语讯宝地址 & 数据库文件扩展名)}))
                    Return False
                End If
            End If
            If String.IsNullOrEmpty(英语讯宝地址) OrElse (String.IsNullOrEmpty(当前用户.域名_本国语) = False AndAlso String.IsNullOrEmpty(本国语讯宝地址)) Then
                Dim 列添加器 As New 类_列添加器
                列添加器.添加列_用于插入数据("英语讯宝地址", 当前用户.英语讯宝地址)
                If String.IsNullOrEmpty(当前用户.域名_本国语) = False Then
                    列添加器.添加列_用于插入数据("本国语讯宝地址", 当前用户.本国语讯宝地址)
                End If
                Dim 指令2 As New 类_数据库指令_插入新数据(副数据库, "用户", 列添加器)
                指令2.执行()
            End If
            Return True
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            说(ex.Message)
            Return False
        End Try
    End Function

#Region "回答"

    Friend Overrides Sub 回答(ByVal 用户输入 As String, ByVal 时间 As Long)
        If 聊天控件.按钮_说话.Enabled = False Then Return
        If 当前用户.已登录() = True Then
            登录后回答(用户输入)
        Else
            登录前回答(用户输入)
        End If
    End Sub

    Private Sub 登录前回答(ByVal 用户输入 As String)
        Select Case 用户输入
            Case 任务名称_登录
                任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
                任务.添加步骤(任务步骤_常量集合.讯宝地址, 界面文字.获取(4, "请输入你的讯宝地址。"))
                任务.添加步骤(任务步骤_常量集合.密码, 界面文字.获取(5, "请输入密码。"))
                说(任务.获取当前步骤提示语)
            Case 任务名称_注册
                任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
                任务.添加步骤(任务步骤_常量集合.域名, 界面文字.获取(164, "请输入域名（如果你未收到来自某域名的邀请，可以输入域名 #% 或 #% 创建一个临时账号）。", New Object() {讯宝网络域名_英语, 讯宝网络域名_本国语}))
                任务.添加步骤(任务步骤_常量集合.手机号或电子邮箱地址, 界面文字.获取(26, "请输入你的电子邮箱地址，以用于接收验证码。请务必先了解一下我们的#%使用条款#%和#%隐私政策#%。", New Object() {"<span class='TaskName' onclick='ToRobot2(\""TermsOfUse\"")'>", "</span>", "<span class='TaskName' onclick='ToRobot2(\""PrivacyPolicy\"")'>", "</span>"}))
                任务.添加步骤(任务步骤_常量集合.密码, 界面文字.获取(27, "请为你的账号设置密码。（最少#%个字符，最多#%个字符）", New Object() {最小值_常量集合.密码长度, 最大值_常量集合.密码长度}))
                任务.添加步骤(任务步骤_常量集合.重复密码, 界面文字.获取(28, "请再次输入相同的密码。"))
                任务.添加步骤(任务步骤_常量集合.验证码, 界面文字.获取(21, "请输入验证码。"))
                任务.需要获取验证码图片 = True
                说(任务.获取当前步骤提示语)
            Case 任务名称_忘记
                任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
                任务.添加步骤(任务步骤_常量集合.讯宝地址, 界面文字.获取(4, "请输入你的讯宝地址。"))
                任务.添加步骤(任务步骤_常量集合.验证码, 界面文字.获取(21, "请输入验证码。"))
                任务.需要获取验证码图片 = True
                说(任务.获取当前步骤提示语)
            Case 任务名称_取消
                Call 自检(, True)
            Case 任务名称_关闭
                主窗体1.Close()
            Case Else
                任务接收用户输入(用户输入)
        End Select
    End Sub

    Private Sub 登录后回答(ByVal 用户输入 As String)
        Select Case 用户输入
            Case 任务名称_小宇宙 : Call 打开小宇宙页面()
            Case 任务名称_添加讯友 : Call 添加讯友(用户输入)
            Case 任务名称_删除讯友 : Call 删除讯友(用户输入)
            Case 任务名称_清理黑名单 : Call 取消拉黑讯友(用户输入)
            Case 任务名称_添加黑域 : Call 添加黑域(用户输入)
            Case 任务名称_添加白域 : Call 添加白域(用户输入)
            Case 任务名称_重命名标签 : Call 重命名标签(用户输入)
            Case 任务名称_拉黑 : Call 拉黑陌生人(用户输入)
            Case 任务名称_创建小聊天群 : Call 小聊天群(用户输入)
            Case 任务名称_创建大聊天群 : Call 创建大聊天群(用户输入)
            Case 任务名称_账户 : Call 获取账户信息()
            Case 任务名称_图标 : Call 选择头像图片(用户输入)
            Case 任务名称_密码 : Call 修改密码(用户输入)
            Case 任务名称_邮箱地址 : Call 修改邮箱地址(用户输入)
            Case 任务名称_手机号 : Call 修改手机号(用户输入)
            Case 任务名称_取消 : Call 自检(, True)
            Case 任务名称_关闭 : Call 关闭(用户输入)
            Case 任务名称_注销 : Call 注销(用户输入)
            Case 任务名称_登录, 任务名称_注册, 任务名称_忘记 : 说(界面文字.获取(167, "你已登录。"))
            Case Else
                If 任务 IsNot Nothing Then
                    Select Case 任务.名称
                        Case 任务名称_移除黑域 : If 移除黑域2(用户输入) = True Then Return
                        Case 任务名称_移除白域 : If 移除白域2(用户输入) = True Then Return
                        Case 任务名称_关闭 : If 关闭2(用户输入) = True Then Return
                        Case 任务名称_注销 : If 注销2(用户输入) = True Then Return
                    End Select
                End If
                任务接收用户输入(用户输入)
        End Select
    End Sub

    Private Sub 打开小宇宙页面()
        If String.IsNullOrEmpty(当前用户.英语用户名) = True Then Return
        说(界面文字.获取(7, "请稍等。"))
        聊天控件.浏览器_小宇宙.Load(获取当前用户小宇宙的访问路径(当前用户.英语用户名, 当前用户.域名_英语))
    End Sub

    Private Sub 添加讯友(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.添加讯友, 界面文字.获取(98, "请输入一个讯宝地址。"))
        任务.添加步骤(任务步骤_常量集合.添加讯友备注, 界面文字.获取(100, "请为此讯友添加一个备注。如姓名、电话号码等。（不超过#%个字符）", New Object() {最大值_常量集合.讯友备注字符数}))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 删除讯友(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        显示讯友临时编号 = True
        主窗体1.刷新讯友录(讯友录显示范围_常量集合.讯友)
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.删除讯友, 界面文字.获取(82, "请输入讯友的讯宝地址或临时编号（讯友备注行括号内的数字）。"))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 取消拉黑讯友(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        If 当前用户.讯友目录 IsNot Nothing Then
            Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
            Dim I As Integer
            For I = 0 To 讯友目录.Length - 1
                If 讯友目录(I).拉黑 Then Exit For
            Next
            If I < 讯友目录.Length Then
                显示讯友临时编号 = True
                主窗体1.刷新讯友录(讯友录显示范围_常量集合.黑名单)
                任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
                任务.添加步骤(任务步骤_常量集合.取消拉黑讯友, 界面文字.获取(82, "请输入讯友的讯宝地址或临时编号（讯友备注行括号内的数字）。"))
                说(任务.获取当前步骤提示语)
                Return
            End If
        End If
        说(界面文字.获取(116, "目前没有黑名单。"))
    End Sub

    Private Sub 添加黑域(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        Dim I, 可选域名数 As Integer
        If 当前用户.黑域 IsNot Nothing Then
            Dim 黑域() As 域名_复合数据 = 当前用户.黑域
            For I = 0 To 黑域.Length - 1
                If String.Compare(黑域(I).英语, 黑域_全部) = 0 Then
                    说(界面文字.获取(243, "你已添加#%为黑域。", New Object() {黑域_全部}))
                    Return
                End If
            Next
        End If
        Dim 可选域名() As 域名_复合数据 = Nothing
        Call 添加黑域时统计可选域名(可选域名, 可选域名数)
        If 可选域名数 > 0 Then
            Dim 变长文本 As New StringBuilder(可选域名数 * 最大值_常量集合.域名长度)
            Dim 文本写入器 As New StringWriter(变长文本)
            For I = 0 To 可选域名数 - 1
                文本写入器.Write("<br>")
                With 可选域名(I)
                    文本写入器.Write("<a>" & .英语 & "</a>")
                    If String.Compare(.英语, 黑域_全部) <> 0 Then
                        If String.IsNullOrEmpty(.本国语) = False Then
                            文本写入器.Write(" / " & .本国语)
                        End If
                    Else
                        文本写入器.Write(" (" & 界面文字.获取(236, "所有域") & ")")
                    End If
                End With
            Next
            文本写入器.Close()
            任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
            任务.添加步骤(任务步骤_常量集合.添加黑域, 界面文字.获取(239, "陌生人给你发送文字讯宝时（陌生人只能发送文字），如来自黑域将会被屏蔽。请选择你不信任的域：#%", New Object() {文本写入器.ToString}))
            说(任务.获取当前步骤提示语)
        Else
            说(界面文字.获取(238, "没有可选的域名。"))
        End If
    End Sub

    Friend Sub 添加黑域时统计可选域名(ByRef 可选域名() As 域名_复合数据, ByRef 可选域名数 As Integer)
        Dim I, J As Integer
        If 当前用户.讯友目录 IsNot Nothing Then
            Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
            For I = 0 To 讯友目录.Length - 1
                If 讯友目录(I).拉黑 = True Then J += 1
            Next
        End If
        ReDim 可选域名(J)
        If 当前用户.黑域 IsNot Nothing Then
            Dim 黑域() As 域名_复合数据 = 当前用户.黑域
            For I = 0 To 黑域.Length - 1
                If String.Compare(黑域(I).英语, 黑域_全部) = 0 Then Exit For
            Next
            If I = 黑域.Length Then
                可选域名(可选域名数).英语 = 黑域_全部
                可选域名数 += 1
            End If
        Else
            可选域名(可选域名数).英语 = 黑域_全部
            可选域名数 += 1
        End If
        If J > 0 Then
            Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
            Dim 段(), 域名 As String
            For I = 0 To 讯友目录.Length - 1
                If 讯友目录(I).拉黑 = True Then
                    段 = 讯友目录(I).英语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                    域名 = 段(1)
                    If 当前用户.白域 IsNot Nothing Then
                        Dim 白域() As 域名_复合数据 = 当前用户.白域
                        For J = 0 To 白域.Length - 1
                            If String.Compare(白域(J).英语, 域名, True) = 0 Then Exit For
                        Next
                        If J < 白域.Length Then Continue For
                    End If
                    If 当前用户.黑域 IsNot Nothing Then
                        Dim 黑域() As 域名_复合数据 = 当前用户.黑域
                        For J = 0 To 黑域.Length - 1
                            If String.Compare(黑域(J).英语, 域名, True) = 0 Then Exit For
                        Next
                        If J < 黑域.Length Then Continue For
                    End If
                    If 可选域名数 > 0 Then
                        For J = 0 To 可选域名数 - 1
                            If String.Compare(可选域名(J).英语, 域名) = 0 Then Exit For
                        Next
                        If J < 可选域名数 Then Continue For
                    End If
                    With 可选域名(可选域名数)
                        .英语 = 域名
                        If String.IsNullOrEmpty(讯友目录(I).本国语讯宝地址) = False Then
                            段 = 讯友目录(I).本国语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                            .本国语 = 段(1)
                        End If
                    End With
                    可选域名数 += 1
                End If
            Next
        End If
    End Sub

    Private Sub 添加白域(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        Dim I, 可选域名数 As Integer
        If 当前用户.黑域 IsNot Nothing Then
            Dim 黑域() As 域名_复合数据 = 当前用户.黑域
            For I = 0 To 黑域.Length - 1
                If String.Compare(黑域(I).英语, 黑域_全部) = 0 Then Exit For
            Next
            If I = 黑域.Length Then
                GoTo 跳转点1
            End If
        Else
跳转点1:
            说(界面文字.获取(242, "未添加#%为黑域时，没有必要添加白域。", New Object() {黑域_全部}))
            Return
        End If
        Dim 可选域名() As 域名_复合数据 = Nothing
        Call 添加白域时统计可选域名(可选域名, 可选域名数)
        If 可选域名数 > 0 Then
            Dim 变长文本 As New StringBuilder(可选域名数 * 最大值_常量集合.域名长度)
            Dim 文本写入器 As New StringWriter(变长文本)
            For I = 0 To 可选域名数 - 1
                文本写入器.Write("<br>")
                With 可选域名(I)
                    文本写入器.Write("<a>" & .英语 & "</a>")
                    If String.IsNullOrEmpty(.本国语) = False Then
                        文本写入器.Write(" / " & .本国语)
                    End If
                End With
            Next
            文本写入器.Close()
            任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
            任务.添加步骤(任务步骤_常量集合.添加白域, 界面文字.获取(240, "陌生人给你发送文字讯宝时（陌生人只能发送文字），如来自白域将不会被屏蔽。请选择你信任的域：#%", New Object() {文本写入器.ToString}))
            说(任务.获取当前步骤提示语)
        Else
            说(界面文字.获取(238, "没有可选的域名。"))
        End If
    End Sub

    Friend Sub 添加白域时统计可选域名(ByRef 可选域名() As 域名_复合数据, ByRef 可选域名数 As Integer)
        If 当前用户.讯友目录 Is Nothing Then Return
        Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
        Dim I, J As Integer
        For I = 0 To 讯友目录.Length - 1
            If 讯友目录(I).拉黑 = False Then J += 1
        Next
        If J > 0 Then
            Dim 段(), 域名 As String
            ReDim 可选域名(J - 1)
            For I = 0 To 讯友目录.Length - 1
                If 讯友目录(I).拉黑 = False Then
                    段 = 讯友目录(I).英语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                    域名 = 段(1)
                    If 当前用户.白域 IsNot Nothing Then
                        Dim 白域() As 域名_复合数据 = 当前用户.白域
                        For J = 0 To 白域.Length - 1
                            If String.Compare(白域(J).英语, 域名, True) = 0 Then Exit For
                        Next
                        If J < 白域.Length Then Continue For
                    End If
                    If 当前用户.黑域 IsNot Nothing Then
                        Dim 黑域() As 域名_复合数据 = 当前用户.黑域
                        For J = 0 To 黑域.Length - 1
                            If String.Compare(黑域(J).英语, 域名, True) = 0 Then Exit For
                        Next
                        If J < 黑域.Length Then Continue For
                    End If
                    If 可选域名数 > 0 Then
                        For J = 0 To 可选域名数 - 1
                            If String.Compare(可选域名(J).英语, 域名) = 0 Then Exit For
                        Next
                        If J < 可选域名数 Then Continue For
                    End If
                    With 可选域名(可选域名数)
                        .英语 = 域名
                        If String.IsNullOrEmpty(讯友目录(I).本国语讯宝地址) = False Then
                            段 = 讯友目录(I).本国语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                            .本国语 = 段(1)
                        End If
                    End With
                    可选域名数 += 1
                End If
            Next
        End If
    End Sub

    Friend Sub 移除黑域(ByVal 域名 As 域名_复合数据)
        If 任务 IsNot Nothing Then 任务.结束()
        Dim 名称 As String = 域名.英语
        If String.IsNullOrEmpty(域名.本国语) = False Then 名称 = 域名.本国语 & " / " & 名称
        任务 = New 类_任务(任务名称_移除黑域, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.移除黑白域, "", 域名.英语)
        说(界面文字.获取(244, "你要将域[#%]从列表中移除吗？请选择<a>#%</a>或者<a>#%</a>。", New Object() {名称, 界面文字.获取(组名_任务, 0, "是"), 界面文字.获取(组名_任务, 1, "否")}))
    End Sub

    Private Function 移除黑域2(ByVal 用户输入 As String)
        Select Case 用户输入
            Case 界面文字.获取(组名_任务, 0, "是")
                If 数据库_保存要发送的一对一讯宝(Me, 当前用户.英语讯宝地址, Date.UtcNow.Ticks, 讯宝指令_常量集合.移除黑域, 任务.获取某步骤的输入值(任务步骤_常量集合.移除黑白域)) = True Then
                    主窗体1.发送讯宝()
                End If
                任务.结束()
                Return True
            Case 界面文字.获取(组名_任务, 1, "否")
                回答(任务名称_取消, 0)
                Return True
        End Select
        Return False
    End Function

    Friend Sub 移除白域(ByVal 域名 As 域名_复合数据)
        If 任务 IsNot Nothing Then 任务.结束()
        Dim 名称 As String = 域名.英语
        If String.IsNullOrEmpty(域名.本国语) = False Then 名称 = 域名.本国语 & " / " & 名称
        任务 = New 类_任务(任务名称_移除白域, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.移除黑白域, "", 域名.英语)
        说(界面文字.获取(244, "你要将域[#%]从列表中移除吗？请选择<a>#%</a>或者<a>#%</a>。", New Object() {名称, 界面文字.获取(组名_任务, 0, "是"), 界面文字.获取(组名_任务, 1, "否")}))
    End Sub

    Private Function 移除白域2(ByVal 用户输入 As String)
        Select Case 用户输入
            Case 界面文字.获取(组名_任务, 0, "是")
                If 数据库_保存要发送的一对一讯宝(Me, 当前用户.英语讯宝地址, Date.UtcNow.Ticks, 讯宝指令_常量集合.移除白域, 任务.获取某步骤的输入值(任务步骤_常量集合.移除黑白域)) = True Then
                    主窗体1.发送讯宝()
                End If
                任务.结束()
                Return True
            Case 界面文字.获取(组名_任务, 1, "否")
                回答(任务名称_取消, 0)
                Return True
        End Select
        Return False
    End Function

    Private Sub 重命名标签(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        If 当前用户.讯友目录 IsNot Nothing Then
            Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
            Dim 讯友标签(讯友目录.Length * 2 - 1) As String
            Dim 讯友标签数 As Integer
            Dim I As Integer
            For I = 0 To 讯友目录.Length - 1
                With 讯友目录(I)
                    收集标签(.标签一, 讯友标签, 讯友标签数)
                    收集标签(.标签二, 讯友标签, 讯友标签数)
                End With
            Next
            If 讯友标签数 > 0 Then
                If 讯友标签数 < 讯友标签.Length Then ReDim Preserve 讯友标签(讯友标签数 - 1)
                Array.Sort(讯友标签)
                Dim 变长文本 As New StringBuilder(讯友标签数 * 最大值_常量集合.讯友标签字符数)
                Dim 文本写入器 As New StringWriter(变长文本)
                For I = 0 To 讯友标签数 - 1
                    If I > 0 Then 文本写入器.Write(", ")
                    文本写入器.Write("<a>" & 讯友标签(I) & "</a>")
                Next
                文本写入器.Close()
                任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
                任务.添加步骤(任务步骤_常量集合.原标签名称, 界面文字.获取(143, "请选择你要重命名的标签名称：#%", New Object() {文本写入器.ToString}))
                任务.添加步骤(任务步骤_常量集合.新标签名称, 界面文字.获取(144, "请输入新名称。（不超过#%个字符）", New Object() {最大值_常量集合.讯友标签字符数}))
                说(任务.获取当前步骤提示语)
                Return
            End If
        End If
        说(界面文字.获取(88, "目前没有标签"))
    End Sub

    Private Sub 拉黑陌生人(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.添加讯友, 界面文字.获取(98, "请输入一个讯宝地址。"))
        任务.添加步骤(任务步骤_常量集合.添加讯友备注, 界面文字.获取(99, "请添加一个备注。（不超过#%个字符）", New Object() {最大值_常量集合.讯友备注字符数}))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 获取账户信息()
        If 任务 IsNot Nothing Then 任务.结束()
        任务 = New 类_任务(任务名称_账户, Me)
        说(界面文字.获取(77, "正在获取账户信息。"))
        Dim 时区偏移量 As Integer = DateDiff(DateInterval.Minute, Date.UtcNow, Date.Now)
        启动HTTPS访问线程(New 类_访问设置(获取中心服务器访问路径开头(当前用户.域名_英语) & "C=AccountInfo&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器) & "&TimezoneOffset=" & 时区偏移量))
    End Sub

    Private Sub 小聊天群(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        If 当前用户.加入的小聊天群 IsNot Nothing Then
            Dim 加入的小聊天群() As 类_聊天群_小 = 当前用户.加入的小聊天群
            Dim 用户英语讯宝地址 As String = 当前用户.英语讯宝地址
            Dim I, J As Integer
            For I = 0 To 加入的小聊天群.Length - 1
                If String.Compare(加入的小聊天群(I).群主.英语讯宝地址, 用户英语讯宝地址) = 0 Then J += 1
            Next
            If J >= 最大值_常量集合.每个用户可创建的小聊天群数量 Then
                说(界面文字.获取(126, "你创建的小聊天群数量已达上限。"))
                Return
            End If
        End If
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.小聊天群名称, 界面文字.获取(87, "你将创建一个小聊天群（最多#%个成员；新讯宝由服务器实时推送给成员）。请为其输入一个名称。（不超过#%个字符）", New Object() {最大值_常量集合.小聊天群成员数量, 最大值_常量集合.群名称字符数}))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 创建大聊天群(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.大聊天群名称, 界面文字.获取(271, "你将创建一个大聊天群（成员数没有限制，取决于服务器的容量；新讯宝不会被服务器实时推送给成员，而是由成员的客户端每隔几分钟接收一次）。请为其输入一个名称。（不超过#%个字符）", New Object() {最大值_常量集合.群名称字符数}))
        任务.添加步骤(任务步骤_常量集合.大聊天群估计成员数, 界面文字.获取(273, "你的新群预计会有多少成员？"))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 选择头像图片(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        说(界面文字.获取(59, "请选择一幅图片。"))
        With 主窗体1.文件选取器
            .Multiselect = False
            .Filter = 界面文字.获取(67, "所有图片文件") & "|*.jpg;*.jpeg;*.png"
            If .ShowDialog() = DialogResult.OK Then
                Dim 位图 As Bitmap = Nothing
                Dim 位图2 As Bitmap = Nothing
                Dim 文件路径 As String
                Try
                    位图 = New Bitmap(.FileName)
                    If 位图.Width < 长度_常量集合.图标宽高_像素 OrElse 位图.Height < 长度_常量集合.图标宽高_像素 Then
                        说(界面文字.获取(168, "图片太小。"))
                        Return
                    End If
                    位图2 = New Bitmap(长度_常量集合.图标宽高_像素, 长度_常量集合.图标宽高_像素)
                    Dim 绘图器 As Graphics = Graphics.FromImage(位图2)
                    If 位图.Height > 位图.Width Then
                        Dim 缩小比例 As Double = 位图2.Width / 位图.Width
                        绘图器.DrawImage(位图, New Rectangle(0, -(位图.Height - 位图.Width) / 2 * 缩小比例, 位图.Width * 缩小比例, 位图.Height * 缩小比例))
                    Else
                        Dim 缩小比例 As Double = 位图2.Height / 位图.Height
                        绘图器.DrawImage(位图, New Rectangle(-(位图.Width - 位图.Height) / 2 * 缩小比例, 0, 位图.Width * 缩小比例, 位图.Height * 缩小比例))
                    End If
                    位图.Dispose()
                    绘图器.Dispose()
                    文件路径 = My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData & "\" & 当前用户.英语讯宝地址
                    If Directory.Exists(文件路径) = False Then Directory.CreateDirectory(文件路径)
                    文件路径 &= "\" & 生成大写英文字母与数字的随机字符串(20) & ".jpg"
                    位图2.Save(文件路径, Imaging.ImageFormat.Jpeg)
                Catch ex As Exception
                    If 位图 IsNot Nothing Then 位图.Dispose()
                    If 位图2 IsNot Nothing Then 位图2.Dispose()
                    说(ex.Message)
                    Return
                End Try
                If 数据库_保存要发送的一对一讯宝(Me, 当前用户.英语讯宝地址, Date.UtcNow.Ticks, 讯宝指令_常量集合.修改图标, 文件路径) = True Then
                    说(界面文字.获取(7, "请稍等。"))
                    主窗体1.发送讯宝()
                End If
            End If
        End With
    End Sub

    Private Sub 修改密码(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.密码, 界面文字.获取(225, "你要修改密码吗？请输入新密码。（最少#%个字符，最多#%个字符）", New Object() {最小值_常量集合.密码长度, 最大值_常量集合.密码长度}))
        任务.添加步骤(任务步骤_常量集合.重复密码, 界面文字.获取(28, "请再次输入相同的密码。"))
        任务.添加步骤(任务步骤_常量集合.当前密码, 界面文字.获取(228, "请输入当前密码。"))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 修改邮箱地址(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.电子邮箱地址, 界面文字.获取(226, "你要修改电子邮箱地址吗？请输入新的电子邮箱地址。"))
        任务.添加步骤(任务步骤_常量集合.当前密码, 界面文字.获取(228, "请输入当前密码。"))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 修改手机号(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.手机号, 界面文字.获取(227, "你要修改手机号吗？请输入新的手机号。"))
        任务.添加步骤(任务步骤_常量集合.当前密码, 界面文字.获取(228, "请输入当前密码。"))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 关闭(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        任务 = New 类_任务(用户输入, Me)
        说(界面文字.获取(71, "是否保存登录凭据？请选择<a>#%</a>或者<a>#%</a>，或<a>#%</a>。", New Object() {界面文字.获取(组名_任务, 0, "是"), 界面文字.获取(组名_任务, 1, "否"), 任务名称_取消}))
    End Sub

    Private Function 关闭2(ByVal 用户输入 As String) As Boolean
        Select Case 用户输入
            Case 界面文字.获取(组名_任务, 0, "是")
                主窗体1.关闭 = True
                主窗体1.保存凭据 = True
                主窗体1.Close()
                Return True
            Case 界面文字.获取(组名_任务, 1, "否")
                主窗体1.关闭 = True
                主窗体1.Close()
                Return True
        End Select
        Return False
    End Function

    Private Sub 注销(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        任务 = New 类_任务(用户输入, Me)
        说(界面文字.获取(122, "你要注销吗？请选择<a>#%</a>或者<a>#%</a>。", New Object() {界面文字.获取(组名_任务, 0, "是"), 界面文字.获取(组名_任务, 1, "否")}))
    End Sub

    Private Function 注销2(ByVal 用户输入 As String) As Boolean
        Select Case 用户输入
            Case 界面文字.获取(组名_任务, 0, "是")
                说(界面文字.获取(7, "请稍等。"))
                任务 = New 类_任务(任务名称_注销, Me)
                启动HTTPS访问线程(New 类_访问设置(获取中心服务器访问路径开头(当前用户.域名_英语) & "C=Logout&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器), 20000))
                Return True
            Case 界面文字.获取(组名_任务, 1, "否")
                回答(任务名称_取消, 0)
                Return True
        End Select
        Return False
    End Function

    Private Sub 任务接收用户输入(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then
            If 任务.步骤数量 > 0 Then
                Dim 结果 As String = 任务.保存当前步骤输入值(用户输入)
                If String.IsNullOrEmpty(结果) Then
                    结果 = 任务.获取当前步骤提示语
                    If String.IsNullOrEmpty(结果) = False Then
                        说(结果)
                    Else
                        Select Case 任务.名称
                            Case 任务名称_添加讯友, 任务名称_拉黑
                                Dim SS包生成器 As New 类_SS包生成器()
                                SS包生成器.添加_有标签("发送序号", 当前用户.讯宝发送序号)
                                SS包生成器.添加_有标签("讯宝地址", 任务.获取某步骤的输入值(任务步骤_常量集合.添加讯友))
                                SS包生成器.添加_有标签("备注", 任务.获取某步骤的输入值(任务步骤_常量集合.添加讯友备注))
                                If String.Compare(任务.名称, 任务名称_拉黑, True) = 0 Then
                                    SS包生成器.添加_有标签("拉黑", True)
                                    任务.名称 = 任务名称_添加讯友
                                Else
                                    SS包生成器.添加_有标签("拉黑", False)
                                End If
                                说(界面文字.获取(7, "请稍等。"))
                                启动HTTPS访问线程(New 类_访问设置(获取传送服务器访问路径开头(当前用户.主机名, 当前用户.域名_英语, False) & "C=AddContact&UserID=" & 当前用户.编号 & "&Position=" & 当前用户.位置号 & "&DeviceType=" & 设备类型_电脑, 20000, SS包生成器.生成SS包(当前用户.AES加密器)))
                            Case 任务名称_删除讯友
                                Dim 英语讯宝地址 As String = 任务.获取某步骤的输入值(任务步骤_常量集合.删除讯友)
                                Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
                                Dim I As Integer
                                For I = 0 To 讯友目录.Length - 1
                                    If String.Compare(讯友目录(I).英语讯宝地址, 英语讯宝地址) = 0 Then
                                        Exit For
                                    End If
                                Next
                                If I < 讯友目录.Length Then
                                    If 数据库_保存要发送的一对一讯宝(Me, 当前用户.英语讯宝地址, Date.UtcNow.Ticks, 讯宝指令_常量集合.删除讯友, 英语讯宝地址) = True Then
                                        主窗体1.发送讯宝()
                                    End If
                                End If
                                任务.结束()
                            Case 任务名称_清理黑名单
                                Dim 英语讯宝地址 As String = 任务.获取某步骤的输入值(任务步骤_常量集合.取消拉黑讯友)
                                Dim 讯友 As 类_讯友 = 当前用户.查找讯友(英语讯宝地址)
                                If 讯友 IsNot Nothing Then
                                    Dim SS包生成器 As New 类_SS包生成器
                                    SS包生成器.添加_有标签("英语讯宝地址", 英语讯宝地址)
                                    SS包生成器.添加_有标签("拉黑", False)
                                    If 数据库_保存要发送的一对一讯宝(Me, 当前用户.英语讯宝地址, Date.UtcNow.Ticks, 讯宝指令_常量集合.拉黑取消拉黑讯友, SS包生成器.生成纯文本) = True Then
                                        主窗体1.发送讯宝()
                                    End If
                                End If
                                任务.结束()
                            Case 任务名称_重命名标签
                                Dim 原标签名称 As String = 任务.获取某步骤的输入值(任务步骤_常量集合.原标签名称)
                                Dim 新标签名称 As String = 任务.获取某步骤的输入值(任务步骤_常量集合.新标签名称)
                                Dim SS包生成器 As New 类_SS包生成器
                                SS包生成器.添加_有标签("原标签名称", 原标签名称)
                                SS包生成器.添加_有标签("新标签名称", 新标签名称)
                                If 数据库_保存要发送的一对一讯宝(Me, 当前用户.英语讯宝地址, Date.UtcNow.Ticks, 讯宝指令_常量集合.重命名讯友标签, SS包生成器.生成纯文本) = True Then
                                    主窗体1.发送讯宝()
                                End If
                                任务.结束()
                            Case 任务名称_添加黑域
                                If 数据库_保存要发送的一对一讯宝(Me, 当前用户.英语讯宝地址, Date.UtcNow.Ticks, 讯宝指令_常量集合.添加黑域, 任务.获取某步骤的输入值(任务步骤_常量集合.添加黑域)) = True Then
                                    主窗体1.发送讯宝()
                                End If
                                任务.结束()
                            Case 任务名称_添加白域
                                If 数据库_保存要发送的一对一讯宝(Me, 当前用户.英语讯宝地址, Date.UtcNow.Ticks, 讯宝指令_常量集合.添加白域, 任务.获取某步骤的输入值(任务步骤_常量集合.添加白域)) = True Then
                                    主窗体1.发送讯宝()
                                End If
                                任务.结束()
                            Case 任务名称_创建小聊天群
                                If 数据库_保存要发送的一对一讯宝(Me, 当前用户.英语讯宝地址, Date.UtcNow.Ticks, 讯宝指令_常量集合.创建小聊天群, 任务.获取某步骤的输入值(任务步骤_常量集合.小聊天群名称)) = True Then
                                    说(界面文字.获取(7, "请稍等。"))
                                    主窗体1.发送讯宝()
                                End If
                            Case Else
                                启动HTTPS访问线程(任务.生成访问设置)
                        End Select
                    End If
                Else
                    说(结果)
                End If
                Return
            End If
        End If
        If 当前用户.已登录() = True Then
            Call 自检(True)
        Else
            任务 = New 类_任务(任务名称_登录, 聊天控件.输入框, Me)
            任务.添加步骤(任务步骤_常量集合.讯宝地址, 界面文字.获取(4, "请输入你的讯宝地址。"))
            任务.添加步骤(任务步骤_常量集合.密码, 界面文字.获取(5, "请输入密码。"))
            Dim 结果 As String = 任务.保存当前步骤输入值(用户输入)
            If String.IsNullOrEmpty(结果) Then
                说(任务.获取当前步骤提示语)
            Else
                任务 = Nothing
                Call 自检(True)
            End If
        End If
    End Sub

#End Region

#Region "访问中心服务器"

    Protected Overrides Sub HTTPS请求成功(ByVal SS包() As Byte)
        If 聊天控件.InvokeRequired Then
            Dim d As New HTTPS请求成功_跨线程(AddressOf HTTPS请求成功)
            聊天控件.Invoke(d, New Object() {SS包})
        Else
            聊天控件.下拉列表_任务.Enabled = True
            聊天控件.按钮_说话.Enabled = True
            If 任务 Is Nothing Then Return
            If String.Compare(任务.名称, 任务名称_注销) = 0 Then
                主窗体1.注销成功()
            Else
                If SS包 IsNot Nothing Then
                    Dim SS包解读器 As 类_SS包解读器
                    Try
                        SS包解读器 = New 类_SS包解读器(SS包)
                        Select Case SS包解读器.查询结果
                            Case 查询结果_常量集合.成功
                                Select Case 任务.名称
                                    Case 任务名称_发流星语 : 流星语发布结束(True) : GoTo 跳转点1
                                    Case 任务名称_添加讯友 : 添加讯友成功(SS包解读器)
                                    Case 任务名称_创建大聊天群 : 大聊天群创建成功(SS包解读器)
                                    Case 任务名称_加入大聊天群 : 加入大聊天群成功(SS包解读器)
                                    Case 任务名称_账户 : 收到账户信息(SS包解读器) : Return
                                    Case 任务名称_发布商品 : 商品发布结束(True) : GoTo 跳转点1
                                    Case 任务名称_获取密钥 : 收到密钥(SS包解读器) : Return
                                    Case 任务名称_密码 : 密码修改成功()
                                    Case 任务名称_手机号 : 等待验证手机号(SS包解读器) : Return
                                    Case 任务名称_邮箱地址 : 等待验证电子邮箱地址(SS包解读器) : Return
                                    Case 任务名称_验证手机号 : 手机号修改成功()
                                    Case 任务名称_验证邮箱地址 : 电子邮箱地址修改成功()
                                    Case 任务名称_登录 : 登录成功(SS包解读器) : Return
                                    Case 任务名称_用户名 : 设置用户名成功(SS包解读器) : Return
                                    Case 任务名称_注册 : 等待验证(SS包解读器) : Return
                                    Case 任务名称_验证 : 验证成功(SS包解读器) : Return
                                    Case 任务名称_忘记 : 等待重设密码(SS包解读器) : Return
                                    Case 任务名称_重设 : 重设密码() : Return
                                End Select
                            Case 查询结果_常量集合.发送序号不一致 : 启动访问线程_传送服务器()
                            Case 查询结果_常量集合.拥有的大聊天群数量已达上限
                                If 大聊天群创建成功(SS包解读器) = False Then 说(界面文字.获取(272, "你创建的大聊天群数量已达上限。"))
                            Case 查询结果_常量集合.没有可用的大聊天群服务器 : 说(界面文字.获取(274, "没有可用的大聊天群服务器。"))
                            Case 查询结果_常量集合.不是群成员 : 说(界面文字.获取(83, "你不是当前聊天群的成员。"))
                            Case 查询结果_常量集合.验证码 : 收到验证码图片(SS包解读器) : Return
                            Case 查询结果_常量集合.讯宝地址不存在 : 说(界面文字.获取(109, "此讯宝地址不存在。[<a>#%</a>]", New Object() {任务名称_添加讯友}))
                            Case 查询结果_常量集合.讯友录满了 : 说(界面文字.获取(104, "失败，因为最多只能添加#%个讯友。", New Object() {最大值_常量集合.讯友数量}))
                            Case 查询结果_常量集合.某标签讯友数满了 : 说(界面文字.获取(135, "失败，因为每个标签最多只能标记 #% 个讯友。", New Object() {最大值_常量集合.每个标签讯友数量}))
                            Case 查询结果_常量集合.稍后重试 : 说(界面文字.获取(20, "你的操作过于频繁，请#%分钟后再尝试。", New Object() {最近操作次数统计时间_分钟}))
                            Case 查询结果_常量集合.凭据无效 : 说(界面文字.获取(229, "请注销，然后重新登录。")) : Return   '不可直接调用主窗体的 注销成功 方法
                            Case 查询结果_常量集合.服务器未就绪 : 说(界面文字.获取(269, "服务器还未就绪。请稍后重试。"))
                            Case 查询结果_常量集合.获取A记录失败 : 说(界面文字.获取(111, "获取域名A记录失败。"))
                            Case 查询结果_常量集合.不正确 : 用户名或密码不正确() : Return
                            Case 查询结果_常量集合.手机号已绑定 : 说(界面文字.获取(39, "手机号已绑定在其它账号上了。"))
                            Case 查询结果_常量集合.电子邮箱地址已绑定 : 说(界面文字.获取(40, "电子邮箱地址已绑定在其它账号上了。"))
                            Case 查询结果_常量集合.验证码不匹配 : 说(界面文字.获取(44, "你提交的验证码与服务器上的不匹配。"))
                            Case 查询结果_常量集合.英语用户名已注册 : 英语用户名已注册() : Return
                            Case 查询结果_常量集合.本国语用户名已注册 : 本国语用户名已注册() : Return
                            Case 查询结果_常量集合.暂停发送验证码 : 说(界面文字.获取(20, "你的操作过于频繁，请#%分钟后再尝试。", New Object() {验证码的时间间隔_分钟}))
                            Case 查询结果_常量集合.获取验证码次数过多 : 说(界面文字.获取(20, "你的操作过于频繁，请#%分钟后再尝试。", New Object() {验证码的有效时间_分钟}))
                            Case 查询结果_常量集合.手机号未验证 : 说(界面文字.获取(18, "此手机号是未验证的。"))
                            Case 查询结果_常量集合.电子邮箱地址未验证 : 说(界面文字.获取(19, "此电子邮箱地址是未验证的。"))
                            Case 查询结果_常量集合.无注册许可 : 说(界面文字.获取(300, "无法注册，因为你并非受邀用户。"))
                            Case 查询结果_常量集合.没有可用的传送服务器 : 说(界面文字.获取(118, "没有可用的传送服务器。"))
                            Case 查询结果_常量集合.传送服务器上没有空位置 : 说(界面文字.获取(125, "传送服务器上没有空位置。"))
                            Case 查询结果_常量集合.账号停用 : 说(界面文字.获取(15, "账号已停用。"))
                            Case 查询结果_常量集合.系统维护 : 说(界面文字.获取(14, "由于服务器正在维护中，暂停服务。"))
                            Case 查询结果_常量集合.出错 : 说(界面文字.获取(108, "出错 #%", New Object() {SS包解读器.出错提示文本}))
                            Case 查询结果_常量集合.失败
                                Dim 出错提示文本 As String = SS包解读器.出错提示文本
                                If String.IsNullOrEmpty(出错提示文本) Then
                                    说(界面文字.获取(148, "由于未知原因，操作失败。"))
                                Else
                                    说(界面文字.获取(199, "操作失败（#%）。", New Object() {出错提示文本}))
                                End If
                                Select Case 任务.名称
                                    Case 任务名称_获取密钥
                                        If 重试次数 < 3 Then
                                            重试次数 += 1
                                            任务.结束()
                                            Call 获取密钥()
                                            Return
                                        Else
                                            重试次数 = 0
                                        End If
                                End Select
                            Case 查询结果_常量集合.数据库未就绪 : 说(界面文字.获取(141, "数据库未就绪。"))
                            Case Else : 说(界面文字.获取(108, "出错 #%", New Object() {SS包解读器.查询结果}))
                        End Select
                        Select Case 任务.名称
                            Case 任务名称_发流星语 : 流星语发布结束(False)
                            Case 任务名称_发布商品 : 商品发布结束(False)
                        End Select
                    Catch ex As Exception
                        说(ex.Message)
                    End Try
                End If
跳转点1:
                任务.结束()
                任务 = Nothing
            End If
        End If
    End Sub

    Private Sub 添加讯友成功(ByVal SS包解读器 As 类_SS包解读器)
        Dim 新讯友 As New 类_讯友
        SS包解读器.读取_有标签("英语讯宝地址", 新讯友.英语讯宝地址)
        If String.IsNullOrEmpty(新讯友.英语讯宝地址) = False Then
            SS包解读器.读取_有标签("备注", 新讯友.备注)
            SS包解读器.读取_有标签("主机名", 新讯友.主机名)
            SS包解读器.读取_有标签("位置号", 新讯友.位置号)
            SS包解读器.读取_有标签("本国语讯宝地址", 新讯友.本国语讯宝地址)
            SS包解读器.读取_有标签("拉黑", 新讯友.拉黑)
            Dim 讯友录更新时间 As Long
            SS包解读器.读取_有标签("时间", 讯友录更新时间)
            If 当前用户.讯友目录 Is Nothing Then
                ReDim 当前用户.讯友目录(0)
                新讯友.临时编号 = 1
                当前用户.讯友目录(0) = 新讯友
                If 新讯友.拉黑 = False Then
                    If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.讯友 Then
                        主窗体1.刷新讯友录()
                    End If
                Else
                    Select Case 当前用户.讯友录当前显示范围
                        Case 讯友录显示范围_常量集合.最近, 讯友录显示范围_常量集合.讯友, 讯友录显示范围_常量集合.某标签, 讯友录显示范围_常量集合.黑名单
                            主窗体1.刷新讯友录()
                    End Select
                End If
            Else
                Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
                Dim I As Integer
                For I = 0 To 讯友目录.Length - 1
                    If String.Compare(讯友目录(I).英语讯宝地址, 新讯友.英语讯宝地址) = 0 Then
                        Exit For
                    End If
                Next
                If I = 讯友目录.Length Then
                    For I = 0 To 讯友目录.Length - 1
                        If String.Compare(讯友目录(I).英语讯宝地址, 新讯友.英语讯宝地址) > 0 Then
                            Exit For
                        End If
                    Next
                    If I < 讯友目录.Length Then
                        Dim 讯友2(讯友目录.Length) As 类_讯友
                        If I > 0 Then
                            Array.Copy(讯友目录, 0, 讯友2, 0, I)
                        End If
                        讯友2(I) = 新讯友
                        Array.Copy(讯友目录, I, 讯友2, I + 1, 讯友目录.Length - I)
                        讯友目录 = 讯友2
                    Else
                        ReDim Preserve 讯友目录(讯友目录.Length)
                        讯友目录(讯友目录.Length - 1) = 新讯友
                    End If
                    当前用户.讯友目录 = 讯友目录
                    For I = 0 To 讯友目录.Length - 1
                        讯友目录(I).临时编号 = I + 1
                    Next
                    If 新讯友.拉黑 = False Then
                        If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.讯友 Then
                            主窗体1.刷新讯友录()
                        End If
                    Else
                        Select Case 当前用户.讯友录当前显示范围
                            Case 讯友录显示范围_常量集合.最近, 讯友录显示范围_常量集合.讯友, 讯友录显示范围_常量集合.某标签, 讯友录显示范围_常量集合.黑名单
                                主窗体1.刷新讯友录()
                        End Select
                    End If
                End If
            End If
            If 讯友录更新时间 > 0 Then 当前用户.讯友录更新时间 = 讯友录更新时间
            If 新讯友.拉黑 = False Then
                数据库_分配地址或域编号(新讯友.英语讯宝地址)
                说(界面文字.获取(107, "讯友 #% 添加成功。[<a>#%</a>]", New Object() {新讯友.英语讯宝地址, 任务名称_添加讯友}))
            Else
                说(界面文字.获取(97, "你将不会收到 #% 的任何消息。", New Object() {新讯友.英语讯宝地址}))
            End If
        End If
    End Sub

    Private Sub 数据库_清除讯友数据(ByVal 英语讯宝地址 As String)
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 地址或域编号 As Long
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("地址或域", 筛选方式_常量集合.等于, 英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("编号")
            Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, "地址或域编号", 筛选器, 1, 列添加器, , 主键索引名)
            读取器 = 指令2.执行()
            While 读取器.读取
                地址或域编号 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("讯宝地址", 筛选方式_常量集合.等于, 英语讯宝地址)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "一对一讯宝", 筛选器, "#地址存储时间")
            指令.执行()
            If 地址或域编号 > 0 Then
                Dim I, 最大字数, 最大字数2 As Integer
                For I = 1 To 最大值_常量集合.讯宝文本长度
                    最大字数 = 获取文本库号(I)
                    If 最大字数 <> 最大字数2 Then
                        最大字数2 = 最大字数
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于筛选器("地址或域编号", 筛选方式_常量集合.等于, 地址或域编号)
                        列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 0)
                        筛选器 = New 类_筛选器
                        筛选器.添加一组筛选条件(列添加器)
                        指令 = New 类_数据库指令_删除数据(副数据库, 最大字数 & "库", 筛选器, "#地址域群编号")
                        指令.执行()
                    End If
                Next
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("地址或域名", 筛选方式_常量集合.等于, 英语讯宝地址)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 0)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            指令 = New 类_数据库指令_删除数据(副数据库, "最近", 筛选器, "#地址群编号")
            指令.执行()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("地址或域", 筛选方式_常量集合.等于, 英语讯宝地址)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            指令 = New 类_数据库指令_删除数据(副数据库, "地址或域编号", 筛选器, 主键索引名)
            指令.执行()
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            说(ex.Message)
        End Try
    End Sub

    Private Function 大聊天群创建成功(ByVal SS包解读器 As 类_SS包解读器) As Boolean
        Dim SS包解读器2() As Object = SS包解读器.读取_重复标签("群")
        If SS包解读器2 IsNot Nothing Then
            Dim 子域名 As String
            Dim 主机名 As String = Nothing
            Dim 群名称 As String = Nothing
            Dim I, J As Integer
            Dim 群编号 As Long
            For I = 0 To SS包解读器2.Length - 1
                With CType(SS包解读器2(I), 类_SS包解读器)
                    .读取_有标签("主机名", 主机名, Nothing)
                    .读取_有标签("群编号", 群编号, 0)
                    .读取_有标签("群名称", 群名称, Nothing)
                End With
                子域名 = 主机名 & "." & 当前用户.域名_英语
                If 当前用户.加入的大聊天群 IsNot Nothing Then
                    Dim 加入的大聊天群() As 类_聊天群_大 = 当前用户.加入的大聊天群
                    For J = 0 To 加入的大聊天群.Length - 1
                        With 加入的大聊天群(J)
                            If String.Compare(.子域名, 子域名) = 0 AndAlso .编号 = 群编号 Then
                                Return False
                            End If
                        End With
                    Next
                End If
                说(界面文字.获取(276, "大聊天群 [#%] 创建成功。地址是 #% 。", New Object() {群名称, 子域名 & "/" & 群编号}) & "&nbsp;<span class='TaskName' onclick='ToRobot2(\""JoinLargeGroup\"", \""" & 子域名 & "\"", \""" & 群编号 & "\"")'>" & 界面文字.获取(173, "加入") & "</span>")
            Next
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub 加入大聊天群成功(ByVal SS包解读器 As 类_SS包解读器)
        Dim 子域名 As String = Nothing
        Dim 群编号, 图标更新时间 As Long
        Dim 群名称 As String = Nothing
        Dim 连接凭据 As String = Nothing
        Dim 本国语域名 As String = Nothing
        Dim 角色 As 群角色_常量集合
        SS包解读器.读取_有标签("子域名", 子域名)
        SS包解读器.读取_有标签("群编号", 群编号)
        SS包解读器.读取_有标签("群名称", 群名称)
        SS包解读器.读取_有标签("图标更新时间", 图标更新时间)
        SS包解读器.读取_有标签("连接凭据", 连接凭据)
        SS包解读器.读取_有标签("角色", 角色)
        SS包解读器.读取_有标签("本国语域名", 本国语域名)
        Dim I As Integer = 子域名.IndexOf(".")
        Dim 大聊天群 As New 类_聊天群_大 With {
            .编号 = 群编号,
            .名称 = 群名称,
            .图标更新时间 = 图标更新时间,
            .主机名 = 子域名.Substring(0, I),
            .英语域名 = 子域名.Substring(I + 1),
            .本国语域名 = 本国语域名,
            .子域名 = 子域名,
            .连接凭据 = 连接凭据,
            .我的角色 = 角色
        }
        Dim 加入的大聊天群() As 类_聊天群_大
        If 当前用户.加入的大聊天群 IsNot Nothing Then
            加入的大聊天群 = 当前用户.加入的大聊天群
            Dim J As Integer
            For J = 0 To 加入的大聊天群.Length - 1
                If String.Compare(加入的大聊天群(J).子域名, 子域名) = 0 AndAlso
                    加入的大聊天群(J).编号 = 群编号 Then
                    Return
                End If
            Next
            ReDim Preserve 加入的大聊天群(加入的大聊天群.Length)
            加入的大聊天群(加入的大聊天群.Length - 1) = 大聊天群
            当前用户.加入的大聊天群 = 加入的大聊天群
        Else
            ReDim 当前用户.加入的大聊天群(0)
            当前用户.加入的大聊天群(0) = 大聊天群
            加入的大聊天群 = 当前用户.加入的大聊天群
        End If
        For I = 0 To 加入的大聊天群.Length - 1
            With 加入的大聊天群(I)
                If String.Compare(.子域名, 子域名) = 0 AndAlso .编号 <> 群编号 Then
                    If String.IsNullOrEmpty(.连接凭据) = False Then .连接凭据 = 连接凭据
                End If
            End With
        Next

        Dim 聊天对象2 As New 类_聊天对象 With {
            .大聊天群 = 大聊天群
        }
        主窗体1.添加聊天控件(聊天对象2)
        数据库_更新最近互动讯友排名(子域名, 群编号)
        主窗体1.刷新讯友录(讯友录显示范围_常量集合.聊天群)
    End Sub

    Private Sub 收到账户信息(ByVal SS包解读器 As 类_SS包解读器)
        Dim SS包() As Byte = Nothing
        SS包解读器.读取_有标签("用户信息", SS包)
        解读账户和登录信息(SS包)
        Call 自检()
    End Sub

    Private Sub 收到密钥(ByVal SS包解读器 As 类_SS包解读器)
        重试次数 = 0
        SS包解读器.读取_有标签("主机名", 当前用户.主机名)
        SS包解读器.读取_有标签("位置号", 当前用户.位置号)
        当前用户.AES加解密模块 = New RijndaelManaged
        SS包解读器.读取_有标签("对称密钥", 当前用户.AES加解密模块.Key)
        SS包解读器.读取_有标签("初始向量", 当前用户.AES加解密模块.IV)
        当前用户.AES加密器 = 当前用户.AES加解密模块.CreateEncryptor
        当前用户.AES解密器 = 当前用户.AES加解密模块.CreateDecryptor
        SS包解读器.读取_有标签("时间", 当前用户.密钥创建时间)
        当前用户.获取了密钥 = True
        If 不再提示 = False Then 说(界面文字.获取(246, "密钥收到。"))
        Call 自检()
    End Sub

    Private Sub 密码修改成功()
        说(界面文字.获取(230, "密码修改成功。"))
    End Sub

    Private Sub 等待验证手机号(ByVal SS包解读器 As 类_SS包解读器)
        Dim 之前任务 As 类_任务 = 任务
        任务 = New 类_任务(任务名称_验证手机号, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.手机号, "", 之前任务.获取某步骤的输入值(任务步骤_常量集合.手机号))
        之前任务.结束()
        SS包解读器.读取_有标签("验证码添加时间", 任务.验证码添加时间)
        任务.添加步骤(任务步骤_常量集合.验证码, 界面文字.获取(232, "验证码已发送至你的新手机号。请在#%分钟内输入。", New Object() {验证码的有效时间_分钟}))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 等待验证电子邮箱地址(ByVal SS包解读器 As 类_SS包解读器)
        Dim 之前任务 As 类_任务 = 任务
        任务 = New 类_任务(任务名称_验证邮箱地址, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.电子邮箱地址, "", 之前任务.获取某步骤的输入值(任务步骤_常量集合.电子邮箱地址))
        之前任务.结束()
        SS包解读器.读取_有标签("验证码添加时间", 任务.验证码添加时间)
        任务.添加步骤(任务步骤_常量集合.验证码, 界面文字.获取(233, "验证码已发送至你的新电子邮箱地址。请在#%分钟内输入。", New Object() {验证码的有效时间_分钟}))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 手机号修改成功()
        说(界面文字.获取(234, "手机号修改成功。"))
    End Sub

    Private Sub 电子邮箱地址修改成功()
        说(界面文字.获取(235, "电子邮箱地址修改成功。"))
    End Sub

    Private Sub 登录成功(ByVal SS包解读器 As 类_SS包解读器)
        SS包解读器.读取_有标签("用户编号", 当前用户.编号)
        SS包解读器.读取_有标签("连接凭据", 当前用户.凭据_中心服务器)
        Dim SS包() As Byte = Nothing
        SS包解读器.读取_有标签("用户信息", SS包)
        解读账户和登录信息(SS包)
        If 测试 = True Then
            My.Settings.UserID = 当前用户.编号
            My.Settings.Credential = 当前用户.凭据_中心服务器
            My.Settings.Domain = 当前用户.域名_英语
            My.Settings.Save()
        End If
        聊天控件.载入任务名称()
        Call 自检()
    End Sub

    Private Sub 解读账户和登录信息(ByVal SS包() As Byte)
        Dim SS包解读器 As New 类_SS包解读器(SS包)
        SS包解读器.读取_有标签("英语域名", 当前用户.域名_英语)
        SS包解读器.读取_有标签("本国语域名", 当前用户.域名_本国语, Nothing)
        SS包解读器.读取_有标签("英语用户名", 当前用户.英语用户名, Nothing)
        SS包解读器.读取_有标签("本国语用户名", 当前用户.本国语用户名, Nothing)
        Dim 手机号 As Long
        SS包解读器.读取_有标签("手机号", 手机号)
        SS包解读器.读取_有标签("电子邮箱地址", 当前用户.电子邮箱地址)
        SS包解读器.读取_有标签("职能", 当前用户.职能, Nothing)
        Dim 登录时间_电脑 As String = Nothing
        SS包解读器.读取_有标签("登录时间_电脑", 登录时间_电脑)
        Dim 登录时间_手机 As String = Nothing
        SS包解读器.读取_有标签("登录时间_手机", 登录时间_手机)
        Dim 网络地址_电脑 As String = Nothing
        SS包解读器.读取_有标签("网络地址_电脑", 网络地址_电脑)
        Dim 网络地址_手机 As String = Nothing
        SS包解读器.读取_有标签("网络地址_手机", 网络地址_手机)
        Dim 变长文本 As New StringBuilder(1000)
        Dim 文本写入器 As New StringWriter(变长文本)
        If String.IsNullOrEmpty(当前用户.英语用户名) = False Then
            文本写入器.Write(界面文字.获取(68, "你的英语讯宝地址为 #% 。", New Object() {当前用户.英语讯宝地址}))
        End If
        If String.IsNullOrEmpty(当前用户.本国语用户名) = False Then
            If 变长文本.Length > 0 Then 文本写入器.Write("<br>")
            文本写入器.Write(界面文字.获取(69, "你的中文讯宝地址为 #% 。", New Object() {当前用户.本国语讯宝地址}))
        End If
        If 手机号 > 0 Then
            If 变长文本.Length > 0 Then 文本写入器.Write("<br>")
            文本写入器.Write(界面文字.获取(78, "你的手机号为 #% 。", New Object() {手机号}))
        End If
        If String.IsNullOrEmpty(当前用户.电子邮箱地址) = False Then
            If 变长文本.Length > 0 Then 文本写入器.Write("<br>")
            文本写入器.Write(界面文字.获取(79, "你的电子邮箱地址为 #% 。", New Object() {当前用户.电子邮箱地址}))
        End If
        If String.IsNullOrEmpty(登录时间_电脑) = False Then
            If 变长文本.Length > 0 Then 文本写入器.Write("<br>")
            文本写入器.Write(界面文字.获取(80, "电脑登录时间 #% ，登录IP #% 。", New Object() {登录时间_电脑, 网络地址_电脑}))
        End If
        If String.IsNullOrEmpty(登录时间_手机) = False Then
            If 变长文本.Length > 0 Then 文本写入器.Write("<br>")
            文本写入器.Write(界面文字.获取(81, "手机登录时间 #% ，登录IP #% 。", New Object() {登录时间_手机, 网络地址_手机}))
        End If
        文本写入器.Close()
        说(文本写入器.ToString)
        当前用户.获取了账户信息 = True
        主窗体1.显示隐藏系统管理机器人()
    End Sub

    Private Sub 用户名或密码不正确()
        Select Case 任务.名称
            Case 任务名称_密码, 任务名称_手机号, 任务名称_邮箱地址
                说(界面文字.获取(231, "当前密码不正确。"))
            Case Else
                说(界面文字.获取(17, "用户名或密码不正确。"))
        End Select
        任务.清除所有步骤的输入值()
        任务.需要获取验证码图片 = True
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 英语用户名已注册()
        说(界面文字.获取(37, "英语用户名已被其他人注册了。"))
        任务.清除某步骤的输入值(任务步骤_常量集合.英语用户名)
        任务.清除某步骤的输入值(任务步骤_常量集合.重复英语用户名)
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 本国语用户名已注册()
        说(界面文字.获取(38, "中文用户名已被其他人注册了。"))
        任务.清除某步骤的输入值(任务步骤_常量集合.本国语用户名)
        任务.清除某步骤的输入值(任务步骤_常量集合.重复本国语用户名)
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 等待验证(ByVal SS包解读器 As 类_SS包解读器)
        Dim 之前任务 As 类_任务 = 任务
        任务 = New 类_任务(任务名称_验证, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.域名, "", 之前任务.获取某步骤的输入值(任务步骤_常量集合.域名))
        任务.添加步骤(任务步骤_常量集合.手机号或电子邮箱地址, "", 之前任务.获取某步骤的输入值(任务步骤_常量集合.手机号或电子邮箱地址))
        任务.添加步骤(任务步骤_常量集合.密码, "", 之前任务.获取某步骤的输入值(任务步骤_常量集合.密码))
        任务.身份码类型 = 之前任务.身份码类型
        之前任务.结束()
        SS包解读器.读取_有标签("验证码添加时间", 任务.验证码添加时间)
        If String.Compare(任务.身份码类型, 身份码类型_手机号) = 0 Then
            任务.添加步骤(任务步骤_常量集合.验证码, 界面文字.获取(41, "验证码已发送至你的手机号。请在#%分钟内输入。", New Object() {验证码的有效时间_分钟}))
        Else
            任务.添加步骤(任务步骤_常量集合.验证码, 界面文字.获取(42, "验证码已发送至你的电子邮箱地址。请在#%分钟内输入。", New Object() {验证码的有效时间_分钟}))
        End If
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 验证成功(ByVal SS包解读器 As 类_SS包解读器)
        Dim 之前任务 As 类_任务 = 任务
        SS包解读器.读取_有标签("英语域名", 当前用户.域名_英语)
        SS包解读器.读取_有标签("本国语域名", 当前用户.域名_本国语, Nothing)
        任务 = New 类_任务(任务名称_用户名, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.手机号或电子邮箱地址, "", 之前任务.获取某步骤的输入值(任务步骤_常量集合.手机号或电子邮箱地址))
        任务.添加步骤(任务步骤_常量集合.密码, "", 之前任务.获取某步骤的输入值(任务步骤_常量集合.密码))
        任务.添加步骤(任务步骤_常量集合.验证码, "", 之前任务.获取某步骤的输入值(任务步骤_常量集合.验证码))
        任务.身份码类型 = 之前任务.身份码类型
        任务.验证码添加时间 = 之前任务.验证码添加时间
        之前任务.结束()
        任务.添加步骤(任务步骤_常量集合.英语用户名, 界面文字.获取(24, "请输入你想要拥有的英语用户名（只能有英语字母 a-z、数字 0-9 和下划线 _ ，最多#%个字符）。", New Object() {最大值_常量集合.英语用户名长度}))
        If String.IsNullOrEmpty(当前用户.域名_本国语) Then
            任务.添加步骤(任务步骤_常量集合.重复英语用户名, 界面文字.获取(61, "再次输入相同的英语用户名将提交至服务器。你将永久无法修改。"))
        Else
            任务.添加步骤(任务步骤_常量集合.重复英语用户名, 界面文字.获取(62, "请再次输入相同的英语用户名。"))
            任务.添加步骤(任务步骤_常量集合.本国语用户名, 界面文字.获取(25, "请输入你想要拥有的汉语用户名（只能有汉字、数字 0-9 和下划线 _ ，最多#%个字符）。", New Object() {最大值_常量集合.本国语用户名长度}))
            任务.添加步骤(任务步骤_常量集合.重复本国语用户名, 界面文字.获取(63, "再次输入相同的汉语用户名将提交至服务器。你将永久无法修改。"))
        End If
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 设置用户名成功(ByVal SS包解读器 As 类_SS包解读器)
        Dim 英语用户名 As String = Nothing
        Dim 本国语用户名 As String = Nothing
        SS包解读器.读取_有标签("英语用户名", 英语用户名)
        SS包解读器.读取_有标签("本国语用户名", 本国语用户名)
        Dim 英语讯宝地址 As String = 英语用户名 & 讯宝地址标识 & 当前用户.域名_英语
        说(界面文字.获取(68, "你的英语讯宝地址为 #% 。", New Object() {英语讯宝地址}))
        If String.IsNullOrEmpty(本国语用户名) = False Then
            说(界面文字.获取(69, "你的中文讯宝地址为 #% 。", New Object() {本国语用户名 & 讯宝地址标识 & 当前用户.域名_本国语}))
        End If
        说(界面文字.获取(45, "你的账号创建成功！现在你可以<a>#%</a>了。", New Object() {任务名称_登录}))
        Dim 之前任务 As 类_任务 = 任务
        任务 = New 类_任务(任务名称_登录, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.讯宝地址, "", 英语讯宝地址)
        任务.添加步骤(任务步骤_常量集合.密码, "", 之前任务.获取某步骤的输入值(任务步骤_常量集合.密码))
        之前任务.结束()
        启动HTTPS访问线程(任务.生成访问设置)
    End Sub

    Private Sub 等待重设密码(ByVal SS包解读器 As 类_SS包解读器)
        Dim 之前任务 As 类_任务 = 任务
        任务 = New 类_任务(任务名称_重设, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.讯宝地址, "", 之前任务.获取某步骤的输入值(任务步骤_常量集合.讯宝地址))
        之前任务.结束()
        SS包解读器.读取_有标签("验证码添加时间", 任务.验证码添加时间)
        Dim 发送至手机 As Boolean
        SS包解读器.读取_有标签("发送至手机", 发送至手机)
        If 发送至手机 = True Then
            任务.添加步骤(任务步骤_常量集合.验证码, 界面文字.获取(41, "验证码已发送至你的手机号。请在#%分钟内输入。", New Object() {验证码的有效时间_分钟}))
        Else
            任务.添加步骤(任务步骤_常量集合.验证码, 界面文字.获取(42, "验证码已发送至你的电子邮箱地址。请在#%分钟内输入。", New Object() {验证码的有效时间_分钟}))
        End If
        任务.添加步骤(任务步骤_常量集合.密码, 界面文字.获取(27, "请为你的账号设置密码（最少#%个字符，最多#%个字符）。", New Object() {最小值_常量集合.密码长度, 最大值_常量集合.密码长度}))
        任务.添加步骤(任务步骤_常量集合.重复密码, 界面文字.获取(28, "请再次输入相同的密码。"))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 重设密码()
        说(界面文字.获取(55, "你的账号密码已更换。现在你可以<a>#%</a>了。", New Object() {任务名称_登录}))
        Dim 之前任务 As 类_任务 = 任务
        任务 = New 类_任务(任务名称_登录, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.讯宝地址, "", 之前任务.获取某步骤的输入值(任务步骤_常量集合.讯宝地址))
        任务.添加步骤(任务步骤_常量集合.密码, "", 之前任务.获取某步骤的输入值(任务步骤_常量集合.密码))
        之前任务.结束()
        启动HTTPS访问线程(任务.生成访问设置)
    End Sub

    Private Sub 收到验证码图片(ByVal SS包解读器 As 类_SS包解读器)
        SS包解读器.读取_有标签("验证码添加时间", 任务.验证码添加时间)
        Dim 图片字节数组() As Byte = Nothing
        SS包解读器.读取_有标签("图片", 图片字节数组)
        If 图片字节数组 IsNot Nothing Then
            Dim 字符串 As String = "data:image/jpg;base64," & Convert.ToBase64String(图片字节数组)
            聊天控件.浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSin_Base64Img('" + 字符串 + "', '" + 机器人id_主控 & ".jpg');")
            聊天控件.输入框.Focus()
            任务.添加步骤(任务步骤_常量集合.验证码, 界面文字.获取(21, "请输入验证码。"))
            任务.需要获取验证码图片 = False
            说(任务.获取当前步骤提示语)
        End If
    End Sub

#End Region

#Region "访问传送服务器"

    Friend Sub 启动访问线程_传送服务器()
        If 正在连接传送服务器 Then Return
        正在连接传送服务器 = True
        聊天控件.下拉列表_任务.Enabled = False
        聊天控件.按钮_说话.Enabled = False
        If 不再提示 = False Then 说(界面文字.获取(46, "正在连接传送服务器。"))
        线程_传送服务器 = New Thread(New ThreadStart(AddressOf 连接传送服务器))
        线程_传送服务器.Start()
    End Sub

    Private Sub 连接传送服务器()
        Dim 终止原因 As String = Nothing
        Try
            Dim 传送服务器网络地址 As IPAddress
            If 测试 = False Then
                传送服务器网络地址 = Dns.GetHostAddresses(当前用户.主机名 & "." & 当前用户.域名_英语)(0)
            Else
                传送服务器网络地址 = IPAddress.Parse("127.0.0.1")    '使用IPv6地址 ::1 无法连接
            End If

            网络连接器 = New Socket(传送服务器网络地址.AddressFamily, SocketType.Stream, ProtocolType.Tcp) With {
                .SendTimeout = 收发时限,
                .ReceiveTimeout = 收发时限
            }
            Dim 服务器地址和端口 As New IPEndPoint(传送服务器网络地址, 获取传送服务器侦听端口(当前用户.域名_英语))
            网络连接器.Connect(服务器地址和端口)
            Dim SS包生成器1 As New 类_SS包生成器(True)
            With SS包生成器1
                .添加_无标签(当前用户.位置号)
                .添加_无标签(设备类型_常量集合.电脑)
                .添加_无标签(当前用户.密钥创建时间)
                Dim SS包生成器2 As New 类_SS包生成器()
                With SS包生成器2
                    .添加_有标签("用户编号", 当前用户.编号)
                    当前用户.传送服务器验证码 = 生成大小写英文字母与数字的随机字符串(长度_常量集合.验证码)
                    .添加_有标签("验证码", 当前用户.传送服务器验证码)
                    .添加_有标签("讯友录更新时间", 当前用户.讯友录更新时间)
                End With
                .添加_无标签(SS包生成器2.生成SS包(当前用户.AES加密器), 长度信息字节数_常量集合.两字节)
            End With
            If SS包生成器1.发送SS包(网络连接器) = False Then
                GoTo 结束
            End If
            Dim 字节数组() As Byte = 接收指定长度的数据(网络连接器, 1)
            Select Case Encoding.ASCII.GetString(字节数组)
                Case "Y"
                    Dim SS包解读器 As New 类_SS包解读器(网络连接器, 当前用户.AES解密器)
                    Dim 验证码 As String = Nothing
                    SS包解读器.读取_有标签("验证码", 验证码)
                    If String.Compare(当前用户.传送服务器验证码, 验证码, False) <> 0 Then
                        终止原因 = 界面文字.获取(53, "验证码不匹配。")
                        GoTo 结束
                    End If
                    Dim 讯友录更新时间 As Long = 当前用户.讯友录更新时间
                    SS包解读器.读取_有标签("讯友录更新时间", 当前用户.讯友录更新时间)
                    Dim 讯友数量, 加入的小聊天群数量, 加入的大聊天群数量, 讯宝数量 As Integer
                    SS包解读器.读取_有标签("头像更新时间", 当前用户.头像更新时间)
                    Dim SS包解读器2 As 类_SS包解读器 = Nothing
                    Dim SS包解读器3() As Object = Nothing
                    If 讯友录更新时间 <> 当前用户.讯友录更新时间 Then
                        SS包解读器.读取_有标签("讯友录", SS包解读器2)
                        If SS包解读器2 IsNot Nothing Then
                            SS包解读器3 = SS包解读器2.读取_重复标签("讯友")
                            If SS包解读器3 IsNot Nothing Then
                                Dim 讯友2(SS包解读器3.Length - 1), 某一讯友 As 类_讯友
                                Dim I As Integer
                                For I = 0 To SS包解读器3.Length - 1
                                    某一讯友 = New 类_讯友
                                    With CType(SS包解读器3(I), 类_SS包解读器)
                                        .读取_有标签("英语讯宝地址", 某一讯友.英语讯宝地址)
                                        .读取_有标签("本国语讯宝地址", 某一讯友.本国语讯宝地址)
                                        .读取_有标签("备注", 某一讯友.备注)
                                        .读取_有标签("标签一", 某一讯友.标签一)
                                        .读取_有标签("标签二", 某一讯友.标签二)
                                        .读取_有标签("主机名", 某一讯友.主机名)
                                        .读取_有标签("拉黑", 某一讯友.拉黑)
                                        .读取_有标签("位置号", 某一讯友.位置号)
                                    End With
                                    某一讯友.临时编号 = I + 1
                                    讯友2(I) = 某一讯友
                                Next
                                当前用户.讯友目录 = 讯友2
                                讯友数量 = 当前用户.讯友目录.Length
                            End If
                        End If
                        SS包解读器.读取_有标签("白域", SS包解读器2, Nothing)
                        If SS包解读器2 IsNot Nothing Then
                            SS包解读器3 = SS包解读器2.读取_重复标签("域名")
                            If SS包解读器3 IsNot Nothing Then
                                Dim 白域(SS包解读器3.Length - 1) As 域名_复合数据
                                Dim I As Integer
                                For I = 0 To SS包解读器3.Length - 1
                                    With CType(SS包解读器3(I), 类_SS包解读器)
                                        .读取_有标签("英语", 白域(I).英语)
                                        .读取_有标签("本国语", 白域(I).本国语)
                                    End With
                                Next
                                当前用户.白域 = 白域
                            End If
                        End If
                        SS包解读器.读取_有标签("黑域", SS包解读器2, Nothing)
                        If SS包解读器2 IsNot Nothing Then
                            SS包解读器3 = SS包解读器2.读取_重复标签("域名")
                            If SS包解读器3 IsNot Nothing Then
                                Dim 黑域(SS包解读器3.Length - 1) As 域名_复合数据
                                Dim I As Integer
                                For I = 0 To SS包解读器3.Length - 1
                                    With CType(SS包解读器3(I), 类_SS包解读器)
                                        .读取_有标签("英语", 黑域(I).英语)
                                        .读取_有标签("本国语", 黑域(I).本国语)
                                    End With
                                Next
                                当前用户.黑域 = 黑域
                            End If
                        End If
                    End If
                    SS包解读器.读取_有标签("小聊天群", SS包解读器2, Nothing)
                    If SS包解读器2 IsNot Nothing Then
                        SS包解读器3 = SS包解读器2.读取_重复标签("群")
                        If SS包解读器3 IsNot Nothing Then
                            Dim 加入的群2(SS包解读器3.Length - 1), 某一群 As 类_聊天群_小
                            Dim 群主英语讯宝地址 As String = Nothing
                            Dim 群编号 As Byte
                            Dim 群备注 As String = Nothing
                            Dim 聊天控件() As 控件_聊天 = 主窗体1.聊天控件
                            Dim I, J As Integer
                            For I = 0 To SS包解读器3.Length - 1
                                With CType(SS包解读器3(I), 类_SS包解读器)
                                    .读取_有标签("群主", 群主英语讯宝地址, Nothing)
                                    .读取_有标签("群编号", 群编号, 0)
                                    .读取_有标签("群备注", 群备注, Nothing)
                                End With
                                For J = 0 To 主窗体1.聊天控件数 - 1
                                    With 聊天控件(J).聊天对象
                                        If .小聊天群 IsNot Nothing Then
                                            If String.Compare(群主英语讯宝地址, .讯友或群主.英语讯宝地址) = 0 AndAlso 群编号 = .小聊天群.编号 Then
                                                Exit For
                                            End If
                                        End If
                                    End With
                                Next
                                If J < 主窗体1.聊天控件数 Then
                                    某一群 = 聊天控件(J).聊天对象.小聊天群
                                    某一群.备注 = 群备注
                                Else
                                    某一群 = New 类_聊天群_小 With {
                                    .编号 = 群编号,
                                    .备注 = 群备注
                                }
                                    If String.Compare(群主英语讯宝地址, 当前用户.英语讯宝地址) = 0 Then
                                        Dim 我 As New 类_讯友
                                        我.英语讯宝地址 = 当前用户.英语讯宝地址
                                        If String.IsNullOrEmpty(当前用户.域名_本国语) = False Then
                                            我.本国语讯宝地址 = 当前用户.本国语讯宝地址
                                        End If
                                        我.主机名 = 当前用户.主机名
                                        我.头像更新时间 = 当前用户.头像更新时间
                                        某一群.群主 = 我
                                    Else
                                        Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
                                        For J = 0 To 讯友目录.Length - 1
                                            If String.Compare(群主英语讯宝地址, 讯友目录(J).英语讯宝地址) = 0 Then Exit For
                                        Next
                                        If J < 讯友目录.Length Then
                                            某一群.群主 = 讯友目录(J)
                                        Else
                                            GoTo 结束
                                        End If
                                    End If
                                End If
                                加入的群2(I) = 某一群
                            Next
                            当前用户.加入的小聊天群 = 加入的群2
                            加入的小聊天群数量 = 当前用户.加入的小聊天群.Length
                        End If
                    End If
                    SS包解读器.读取_有标签("大聊天群", SS包解读器2, Nothing)
                    If SS包解读器2 IsNot Nothing Then
                        SS包解读器3 = SS包解读器2.读取_重复标签("群")
                        If SS包解读器3 IsNot Nothing Then
                            Dim 加入的群2(SS包解读器3.Length - 1), 某一群 As 类_聊天群_大
                            Dim 主机名 As String = Nothing
                            Dim 英语域名 As String = Nothing
                            Dim 本国语域名 As String = Nothing
                            Dim 子域名 As String = Nothing
                            Dim 群编号 As Long
                            Dim 群名称 As String = Nothing
                            Dim 聊天控件() As 控件_聊天 = 主窗体1.聊天控件
                            Dim I, J As Integer
                            For I = 0 To SS包解读器3.Length - 1
                                With CType(SS包解读器3(I), 类_SS包解读器)
                                    .读取_有标签("主机名", 主机名, Nothing)
                                    .读取_有标签("英语域名", 英语域名, Nothing)
                                    .读取_有标签("本国语域名", 本国语域名, Nothing)
                                    .读取_有标签("群编号", 群编号, 0)
                                    .读取_有标签("群名称", 群名称, Nothing)
                                End With
                                子域名 = 主机名 & "." & 英语域名
                                For J = 0 To 主窗体1.聊天控件数 - 1
                                    With 聊天控件(J).聊天对象
                                        If .大聊天群 IsNot Nothing Then
                                            If String.Compare(子域名, .大聊天群.子域名) = 0 AndAlso 群编号 = .大聊天群.编号 Then
                                                Exit For
                                            End If
                                        End If
                                    End With
                                Next
                                If J < 主窗体1.聊天控件数 Then
                                    某一群 = 聊天控件(J).聊天对象.大聊天群
                                    某一群.名称 = 群名称
                                Else
                                    某一群 = New 类_聊天群_大 With {
                                    .主机名 = 主机名,
                                    .英语域名 = 英语域名,
                                    .本国语域名 = 本国语域名,
                                    .子域名 = 子域名,
                                    .编号 = 群编号,
                                    .名称 = 群名称
                                }
                                    Call 数据库_获取大聊天群最新讯宝的发送时间(子域名, 群编号, 某一群.最新讯宝的发送时间)
                                End If
                                加入的群2(I) = 某一群
                            Next
                            当前用户.加入的大聊天群 = 加入的群2
                            加入的大聊天群数量 = 当前用户.加入的大聊天群.Length
                        End If
                    End If
                    SS包解读器.读取_有标签("新讯宝", SS包解读器2, Nothing)
                    Dim SS包解读器_新讯宝() As Object = Nothing
                    If SS包解读器2 IsNot Nothing Then
                        SS包解读器_新讯宝 = SS包解读器2.读取_重复标签("讯宝")
                        If SS包解读器_新讯宝 IsNot Nothing Then
                            讯宝数量 = SS包解读器_新讯宝.Length
                            收到未推送的讯宝(SS包解读器_新讯宝)
                        End If
                    End If
                    SS包解读器.读取_有标签("发送序号", 当前用户.讯宝发送序号)
                    Dim 在线 As Boolean
                    SS包解读器.读取_有标签("在线", 在线)
                    Dim SS包生成器 As New 类_SS包生成器()
                    If 讯友录更新时间 <> 当前用户.讯友录更新时间 Then
                        SS包生成器.添加_有标签("讯友数量", 讯友数量)
                        SS包生成器.添加_有标签("小聊天群数量", 加入的小聊天群数量)
                        SS包生成器.添加_有标签("大聊天群数量", 加入的大聊天群数量)
                    End If
                    SS包生成器.添加_有标签("讯宝数量", 讯宝数量)
                    If SS包生成器.发送SS包(网络连接器, 当前用户.AES加密器) = True Then
                        Call 连接传送服务器成功或失败(True, 在线, IIf(讯友录更新时间 < 0, True, False))
                    Else
                        Call 连接传送服务器成功或失败(False, 在线, False)
                        GoTo 结束
                    End If
                    网络连接器.ReceiveTimeout = 0
                    重试次数 = 0
                    心跳确认时间 = Date.Now.Ticks
                    Do
                        SS包解读器 = New 类_SS包解读器(网络连接器, 当前用户.AES解密器)
                        收到讯宝(SS包解读器, True)
                    Loop
                Case "N"
                    Call 关闭网络连接器()
                    当前用户.获取了密钥 = False
                    Call 访问传送服务器失败2()
                    Return
                Case Else
                    GoTo 结束
            End Select
        Catch ex As SocketException
            Call 关闭网络连接器()
            If 重试次数 < 3 Then
                重试次数 += 1
                Call 用http访问传送服务器()
                Call 访问传送服务器失败(ex.Message, False)
            Else
                重试次数 = 0
                Call 访问传送服务器失败(ex.Message, True)
            End If
        Catch ex As 异常_接收SS包失败
            Call 关闭网络连接器()
            If 重试次数 < 3 Then
                重试次数 += 1
                Call 用http访问传送服务器()
                Call 访问传送服务器失败(ex.Message, False)
            Else
                重试次数 = 0
                Call 访问传送服务器失败(ex.Message, True)
            End If
        Catch ex As Exception
            Call 关闭网络连接器()
            Call 访问传送服务器失败(ex.Message, True)
        End Try
        Return
结束:
        Call 关闭网络连接器()
        If String.IsNullOrEmpty(终止原因) Then
            终止原因 = 界面文字.获取(112, "与传送服务器的连接中断了。")
        End If
        Call 访问传送服务器失败(终止原因, True)
    End Sub

    Private Sub 收到未推送的讯宝(ByVal SS包解读器() As Object)
        If 聊天控件.InvokeRequired Then
            Dim d As New 收到未推送的讯宝_跨线程(AddressOf 收到未推送的讯宝)
            聊天控件.Invoke(d, New Object() {SS包解读器})
        Else
            须通知讯宝数量 = 0
            Dim I As Integer
            For I = 0 To SS包解读器.Length - 1
                收到讯宝(SS包解读器(I), False)
            Next
            If 须通知讯宝数量 > 0 Then
                Dim 音频播放器 As New 类_音频_播放
                音频播放器.开始播放本地MP3("contact.mp3")
            End If
        End If
    End Sub

    Private Sub 连接传送服务器成功或失败(ByVal 成功 As Boolean, ByVal 在线 As Boolean, ByVal 打开小宇宙 As Boolean)
        If 聊天控件.InvokeRequired Then
            Dim d As New 连接传送服务器成功或失败_跨线程(AddressOf 连接传送服务器成功或失败)
            聊天控件.Invoke(d, New Object() {成功, 在线, 打开小宇宙})
        Else
            正在连接传送服务器 = False
            主窗体1.刷新讯友录(讯友录显示范围_常量集合.最近, , True)
            If 不再提示 = False Then
                If 在线 Then
                    说(界面文字.获取(90, "你的手机也在线。"))
                Else
                    说(界面文字.获取(91, "你的手机不在线。"))
                End If
            End If
            聊天控件.下拉列表_任务.Enabled = True
            聊天控件.按钮_说话.Enabled = True
            If 成功 = True Then
                If 不再提示 = False Then
                    说(界面文字.获取(56, "你可以收发讯宝了。（每小时最多可发送#%条讯宝，每天最多可发送#%条讯宝）", New Object() {最大值_常量集合.每人每小时可发送讯宝数量, 最大值_常量集合.每人每天可发送讯宝数量}))
                    If 当前用户.黑域 IsNot Nothing Then
                        Dim 黑域() As 域名_复合数据 = 当前用户.黑域
                        Dim I As Integer
                        For I = 0 To 黑域.Length - 1
                            If String.Compare(黑域(I).英语, 黑域_全部) = 0 Then
                                说(界面文字.获取(72, "目前拒绝除白域以外任何陌生人发来的讯宝。"))
                                Exit For
                            End If
                        Next
                    End If
                    If String.Compare(当前用户.域名_英语, 讯宝网络域名_英语) = 0 Then
                        说("温馨提示：你注册的账号只是用于体验的临时账号，会在注册成功1个小时后失效。")
                    End If
                End If
                If 不再提示 = False Then 不再提示 = True
            End If
            提示新任务()
            If 成功 = True Then
                If 聊天控件.载入了陌生人讯宝 = False Then
                    聊天控件.载入了陌生人讯宝 = True
                    聊天控件.载入最近聊天记录()
                End If
                If 打开小宇宙 = True Then
                    聊天控件.浏览器_小宇宙.Load(获取当前用户小宇宙的访问路径(当前用户.英语用户名, 当前用户.域名_英语))
                End If
                主窗体1.发送讯宝()
                自检()
            Else
                启动访问线程_传送服务器()
            End If
        End If
    End Sub

    Private Sub 访问传送服务器失败(ByVal 原因 As String, ByVal 结束 As Boolean)
        If 聊天控件 Is Nothing Then Return
        If 聊天控件.InvokeRequired Then
            Dim d As New HTTPS请求失败_跨线程(AddressOf 访问传送服务器失败)
            聊天控件.Invoke(d, New Object() {原因, 结束})
        Else
            正在连接传送服务器 = False
            If 当前用户.已登录() Then
                If String.IsNullOrEmpty(原因) = False AndAlso 不再提示 = False Then 说(原因)
                If 结束 = False Then
                    If 任务 IsNot Nothing Then
                        If String.Compare(任务.名称, 任务名称_注销) = 0 Then
                            Return
                        End If
                    End If
                    启动访问线程_传送服务器()
                Else
                    聊天控件.下拉列表_任务.Enabled = True
                    聊天控件.按钮_说话.Enabled = True
                    提示新任务()
                End If
            End If
        End If
    End Sub

    Private Sub 访问传送服务器失败2()
        If 聊天控件 Is Nothing Then Return
        If 聊天控件.InvokeRequired Then
            Dim d As New HTTPS请求失败2_跨线程(AddressOf 访问传送服务器失败2)
            聊天控件.Invoke(d, New Object() {})
        Else
            正在连接传送服务器 = False
            自检()
        End If
    End Sub

    Private Sub 收到讯宝(ByVal SS包解读器 As 类_SS包解读器, ByVal 是即时推送讯宝 As Boolean)
        If 聊天控件.InvokeRequired Then
            Dim d As New 收到讯宝_跨线程(AddressOf 收到讯宝)
            聊天控件.Invoke(d, New Object() {SS包解读器, 是即时推送讯宝})
        Else
            Dim 发送时间, 发送序号 As Long
            Dim 讯宝指令 As 讯宝指令_常量集合
            Dim 群主英语讯宝地址 As String = Nothing
            Dim 发送者英语讯宝地址 As String = Nothing
            Dim 讯宝文本 As String = Nothing
            Dim 宽度, 高度 As Short
            Dim 秒数, 群编号 As Byte
            SS包解读器.读取_有标签("指令", 讯宝指令, 讯宝指令_常量集合.无)
            If 讯宝指令 = 讯宝指令_常量集合.从客户端发送至服务器成功 Then
                If 是即时推送讯宝 = True Then
                    SS包解读器.读取_有标签("发送序号", 发送序号)
                    If 主窗体1.正在发送的讯宝 IsNot Nothing Then
                        If 主窗体1.正在发送的讯宝.发送序号 = 发送序号 Then
                            If 主窗体1.定时器_等待确认.Enabled = True Then
                                主窗体1.定时器_等待确认.Stop()
                            End If
                            心跳确认时间 = Date.Now.Ticks
                            主窗体1.发送完毕(True)
                        End If
                    End If
                End If
                Return
            End If
            SS包解读器.读取_有标签("发送者", 发送者英语讯宝地址)
            If String.IsNullOrEmpty(发送者英语讯宝地址) Then Return
            SS包解读器.读取_有标签("发送时间", 发送时间)
            If 发送时间 = 0 Then Return
            If 讯宝指令 = 讯宝指令_常量集合.用http访问我 Then
                If 是即时推送讯宝 = True Then
                    Dim 线程 As New Thread(New ThreadStart(AddressOf 用http访问传送服务器))
                    线程.Start()
                End If
                Return
            End If
            If 讯宝指令 <= 讯宝指令_常量集合.手机和电脑同步 AndAlso 是即时推送讯宝 = True Then
                SS包解读器.读取_有标签("发送序号", 发送序号)
                Dim SS包生成器 As New 类_SS包生成器
                SS包生成器.添加_有标签("发送者", 发送者英语讯宝地址)
                SS包生成器.添加_有标签("发送序号", 发送序号)
                If 数据库_保存要发送的一对一讯宝(Me, 当前用户.英语讯宝地址, Date.UtcNow.Ticks, 讯宝指令_常量集合.确认收到, SS包生成器.生成纯文本) = True Then
                    主窗体1.发送讯宝()
                End If
            End If
            SS包解读器.读取_有标签("群编号", 群编号)
            SS包解读器.读取_有标签("群主", 群主英语讯宝地址, Nothing)
            If 群编号 > 0 AndAlso String.IsNullOrEmpty(群主英语讯宝地址) Then Return
            If 讯宝指令 < 讯宝指令_常量集合.手机和电脑同步 Then
                If 发送序号 = 0 Then
                    SS包解读器.读取_有标签("发送序号", 发送序号)
                    If 发送序号 = 0 Then Return
                End If
                SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                Select Case 讯宝指令
                    Case 讯宝指令_常量集合.发送图片, 讯宝指令_常量集合.发送短视频
                        SS包解读器.读取_有标签("宽度", 宽度, 0)
                        If 宽度 < 1 Then Return
                        SS包解读器.读取_有标签("高度", 高度, 0)
                        If 高度 < 1 Then Return
                End Select
                If 讯宝指令 = 讯宝指令_常量集合.发送语音 Then
                    SS包解读器.读取_有标签("秒数", 秒数, 0)
                    If 秒数 = 0 Then Return
                End If
                Select Case 讯宝指令
                    Case 讯宝指令_常量集合.撤回
                        Dim 发送序号_撤回的讯宝 As Long
                        If Long.TryParse(讯宝文本, 发送序号_撤回的讯宝) = True Then
                            主窗体1.发送者撤回讯宝(发送者英语讯宝地址, 群编号, 群主英语讯宝地址, 发送序号_撤回的讯宝, 发送时间)
                        End If
                    Case 讯宝指令_常量集合.获取小聊天群成员列表
                        If 是即时推送讯宝 Then
                            Try
                                Dim SS包解读器2 As New 类_SS包解读器()
                                SS包解读器2.解读纯文本(讯宝文本)
                                Dim SS包解读器3() As Object = SS包解读器2.读取_重复标签("M")
                                If SS包解读器3 IsNot Nothing Then
                                    Dim 群成员(SS包解读器3.Length - 1), 某一成员 As 类_群成员
                                    Dim I As Integer
                                    For I = 0 To SS包解读器3.Length - 1
                                        With CType(SS包解读器3(I), 类_SS包解读器)
                                            某一成员 = New 类_群成员
                                            .读取_有标签("E", 某一成员.英语讯宝地址)
                                            .读取_有标签("H", 某一成员.主机名)
                                            .读取_有标签("P", 某一成员.位置号)
                                            .读取_有标签("R", 某一成员.角色)
                                            .读取_有标签("N", 某一成员.本国语讯宝地址)
                                            群成员(I) = 某一成员
                                        End With
                                    Next
                                    If 当前用户.加入的小聊天群 IsNot Nothing Then
                                        Dim 加入的小聊天群() As 类_聊天群_小 = 当前用户.加入的小聊天群
                                        Dim 某一群 As 类_聊天群_小
                                        For I = 0 To 加入的小聊天群.Length - 1
                                            某一群 = 加入的小聊天群(I)
                                            If String.Compare(某一群.群主.英语讯宝地址, 群主英语讯宝地址) = 0 AndAlso 某一群.编号 = 群编号 Then
                                                Dim J As Integer
                                                Dim K As Integer = 1
                                                For J = 0 To 群成员.Length - 1
                                                    With 群成员(J)
                                                        .所属的群 = 某一群
                                                        If .角色 <> 群角色_常量集合.群主 Then
                                                            .临时编号 = K
                                                            K += 1
                                                        End If
                                                    End With
                                                Next
                                                某一群.群成员 = 群成员
                                                某一群.待加入确认 = False
                                                Exit For
                                            End If
                                        Next
                                    End If
                                End If
                            Catch ex As Exception
                                说(ex.Message)
                                Return
                            End Try
                        End If
                    Case 讯宝指令_常量集合.修改图标
                        If 是即时推送讯宝 Then
                            Long.TryParse(讯宝文本, 当前用户.头像更新时间)
                            If 当前用户.加入的小聊天群 IsNot Nothing Then
                                Dim 加入的小聊天群() As 类_聊天群_小 = 当前用户.加入的小聊天群
                                Dim 用户英语讯宝地址 As String = 当前用户.英语讯宝地址
                                Dim I As Integer
                                For I = 0 To 加入的小聊天群.Length - 1
                                    If String.Compare(加入的小聊天群(I).群主.英语讯宝地址, 用户英语讯宝地址) = 0 Then
                                        加入的小聊天群(I).群主.头像更新时间 = 当前用户.头像更新时间
                                    End If
                                Next
                            End If
                            说(界面文字.获取(169, "你的账号图标已更新。"))
                        End If
                    Case 讯宝指令_常量集合.创建小聊天群
                        If 是即时推送讯宝 Then
                            Dim 我 As New 类_讯友
                            我.英语讯宝地址 = 当前用户.英语讯宝地址
                            If String.IsNullOrEmpty(当前用户.域名_本国语) = False Then
                                我.本国语讯宝地址 = 当前用户.本国语讯宝地址
                            End If
                            我.主机名 = 当前用户.主机名
                            我.头像更新时间 = 当前用户.头像更新时间
                            Dim 新群 As New 类_聊天群_小
                            新群.群主 = 我
                            新群.备注 = 讯宝文本
                            新群.编号 = 群编号
                            Dim 某一成员 As New 类_群成员
                            某一成员.英语讯宝地址 = 我.英语讯宝地址
                            某一成员.本国语讯宝地址 = 我.本国语讯宝地址
                            某一成员.主机名 = 当前用户.主机名
                            某一成员.位置号 = 当前用户.位置号
                            某一成员.角色 = 群角色_常量集合.群主
                            某一成员.所属的群 = 新群
                            ReDim 新群.群成员(0)
                            新群.群成员(0) = 某一成员
                            If 当前用户.加入的小聊天群 Is Nothing Then
                                ReDim 当前用户.加入的小聊天群(0)
                                当前用户.加入的小聊天群(0) = 新群
                            Else
                                ReDim Preserve 当前用户.加入的小聊天群(当前用户.加入的小聊天群.Length)
                                当前用户.加入的小聊天群(当前用户.加入的小聊天群.Length - 1) = 新群
                            End If
                            Dim 聊天对象 As New 类_聊天对象
                            聊天对象.讯友或群主 = 我
                            聊天对象.小聊天群 = 新群
                            If 任务 IsNot Nothing Then 任务.结束()
                            主窗体1.添加聊天控件(聊天对象)
                            If 数据库_更新最近互动讯友排名(我.英语讯宝地址, 群编号) = True Then
                                主窗体1.刷新讯友录(讯友录显示范围_常量集合.聊天群)
                            End If
                        End If
                    Case 讯宝指令_常量集合.解散小聊天群
                        If 是即时推送讯宝 Then
                            If String.Compare(群主英语讯宝地址, 当前用户.英语讯宝地址) = 0 Then
                                退出小聊天群(群主英语讯宝地址, 群编号)
                            End If
                        End If
                    Case Else
                        Select Case 讯宝指令
                            Case 讯宝指令_常量集合.某人加入聊天群
                                Dim 英语讯宝地址 As String = Nothing
                                Dim 本国语讯宝地址 As String = Nothing
                                Dim SS包解读器2 As New 类_SS包解读器()
                                Try
                                    SS包解读器2.解读纯文本(讯宝文本)
                                    SS包解读器2.读取_有标签("E", 英语讯宝地址)
                                    SS包解读器2.读取_有标签("N", 本国语讯宝地址)
                                Catch ex As Exception
                                    说(ex.Message)
                                    Return
                                End Try
                                讯宝文本 = IIf(String.IsNullOrEmpty(本国语讯宝地址), "", 本国语讯宝地址 & " / ") & 英语讯宝地址
                                If 是即时推送讯宝 AndAlso String.Compare(英语讯宝地址, 当前用户.英语讯宝地址) <> 0 Then
                                    聊天群成员增加(群主英语讯宝地址, 群编号, SS包解读器2, 英语讯宝地址, 本国语讯宝地址)
                                End If
                            Case 讯宝指令_常量集合.退出小聊天群
                                Dim 英语讯宝地址 As String = Nothing
                                Dim 本国语讯宝地址 As String = Nothing
                                Try
                                    Dim SS包解读器2 As New 类_SS包解读器()
                                    SS包解读器2.解读纯文本(讯宝文本)
                                    SS包解读器2.读取_有标签("E", 英语讯宝地址)
                                    SS包解读器2.读取_有标签("N", 本国语讯宝地址)
                                Catch ex As Exception
                                    说(ex.Message)
                                    Return
                                End Try
                                If 是即时推送讯宝 Then
                                    If String.Compare(英语讯宝地址, 当前用户.英语讯宝地址) = 0 Then
                                        退出小聊天群(群主英语讯宝地址, 群编号)
                                        Return
                                    Else
                                        聊天群成员减少(群主英语讯宝地址, 群编号, 英语讯宝地址)
                                    End If
                                End If
                                讯宝文本 = IIf(String.IsNullOrEmpty(本国语讯宝地址), "", 本国语讯宝地址 & " / ") & 英语讯宝地址
                            Case 讯宝指令_常量集合.删减聊天群成员
                                Dim 英语讯宝地址 As String = Nothing
                                Dim 本国语讯宝地址 As String = Nothing
                                Try
                                    Dim SS包解读器2 As New 类_SS包解读器()
                                    SS包解读器2.解读纯文本(讯宝文本)
                                    SS包解读器2.读取_有标签("E", 英语讯宝地址)
                                    SS包解读器2.读取_有标签("N", 本国语讯宝地址)
                                Catch ex As Exception
                                    说(ex.Message)
                                    Return
                                End Try
                                If 是即时推送讯宝 Then
                                    聊天群成员减少(群主英语讯宝地址, 群编号, 英语讯宝地址)
                                End If
                                讯宝文本 = IIf(String.IsNullOrEmpty(本国语讯宝地址), "", 本国语讯宝地址 & " / ") & 英语讯宝地址
                            Case 讯宝指令_常量集合.修改聊天群名称
                                If 是即时推送讯宝 Then
                                    If 当前用户.加入的小聊天群 IsNot Nothing Then
                                        Dim 加入的小聊天群() As 类_聊天群_小 = 当前用户.加入的小聊天群
                                        Dim I As Integer
                                        For I = 0 To 加入的小聊天群.Length - 1
                                            If String.Compare(加入的小聊天群(I).群主.英语讯宝地址, 群主英语讯宝地址) = 0 AndAlso 加入的小聊天群(I).编号 = 群编号 Then
                                                加入的小聊天群(I).备注 = 讯宝文本
                                                If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.聊天群 OrElse 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.最近 Then
                                                    主窗体1.刷新讯友录()
                                                End If
                                                Exit For
                                            End If
                                        Next
                                    End If
                                End If
                        End Select
                        If 群编号 = 0 Then
                            If String.Compare(发送者英语讯宝地址, 当前用户.英语讯宝地址) <> 0 Then
                                If 当前用户.查找讯友(发送者英语讯宝地址) IsNot Nothing Then
                                    If 数据库_保存收到的一对一讯宝(发送者英语讯宝地址, 发送序号, 发送时间, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数) = True Then
                                        Dim 刷新 As Boolean
                                        If 数据库_更新最近互动讯友排名(发送者英语讯宝地址, 0, 刷新) = True Then
                                            主窗体1.显示收到的讯友讯宝(发送者英语讯宝地址, 群编号, 群主英语讯宝地址, 发送时间, 发送序号, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数, 刷新)
                                            If 是即时推送讯宝 Then
                                                Dim 音频播放器 As New 类_音频_播放
                                                音频播放器.开始播放本地MP3("contact.mp3")
                                            End If
                                        End If
                                        If 是即时推送讯宝 = False Then 须通知讯宝数量 += 1
                                    End If
                                Else
                                    If 数据库_保存收到的陌生人讯宝(发送者英语讯宝地址, 发送序号, 发送时间, 讯宝指令, 讯宝文本) = True Then
                                        主窗体1.显示收到的陌生人讯宝(发送者英语讯宝地址, 发送时间, 发送序号, 讯宝指令, 讯宝文本)
                                        If 是即时推送讯宝 Then
                                            Dim 音频播放器 As New 类_音频_播放
                                            音频播放器.开始播放本地MP3("stranger.mp3")
                                        Else
                                            If 是即时推送讯宝 = False Then 须通知讯宝数量 += 1
                                        End If
                                    End If
                                End If
                            Else
                                If 数据库_保存收到的一对一讯宝(群主英语讯宝地址, 发送序号, 发送时间, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数, True) = True Then
                                    Dim 刷新 As Boolean
                                    If 数据库_更新最近互动讯友排名(群主英语讯宝地址, 0, 刷新) = True Then
                                        主窗体1.显示同步的讯宝(群编号, 群主英语讯宝地址, 发送时间, 发送序号, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数, 刷新)
                                    End If
                                End If
                            End If
                        Else
                            If String.Compare(发送者英语讯宝地址, 当前用户.英语讯宝地址) <> 0 Then
                                If 当前用户.查找讯友(群主英语讯宝地址) IsNot Nothing OrElse String.Compare(群主英语讯宝地址, 当前用户.英语讯宝地址) = 0 Then
                                    If 数据库_保存收到的小聊天群讯宝(群主英语讯宝地址, 群编号, 发送者英语讯宝地址, 发送序号, 发送时间, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数) = True Then
                                        Dim 刷新 As Boolean
                                        If 数据库_更新最近互动讯友排名(群主英语讯宝地址, 群编号, 刷新) = True Then
                                            主窗体1.显示收到的讯友讯宝(发送者英语讯宝地址, 群编号, 群主英语讯宝地址, 发送时间, 发送序号, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数, 刷新)
                                            If 是即时推送讯宝 Then
                                                Dim 音频播放器 As New 类_音频_播放
                                                音频播放器.开始播放本地MP3("contact.mp3")
                                            End If
                                        End If
                                        If 是即时推送讯宝 = False Then 须通知讯宝数量 += 1
                                    End If
                                End If
                            Else
                                If 数据库_保存收到的小聊天群讯宝(群主英语讯宝地址, 群编号, 发送者英语讯宝地址, 发送序号, 发送时间, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数) = True Then
                                    Dim 刷新 As Boolean
                                    If 数据库_更新最近互动讯友排名(群主英语讯宝地址, 群编号, 刷新) = True Then
                                        主窗体1.显示同步的讯宝(群编号, 群主英语讯宝地址, 发送时间, 发送序号, 讯宝指令, 讯宝文本, 宽度, 高度, 秒数, 刷新)
                                    End If
                                End If
                            End If
                        End If
                End Select
            ElseIf 是即时推送讯宝 Then
                If 讯宝指令 = 讯宝指令_常量集合.手机和电脑同步 Then
                    SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                    Try
                        Dim SS包解读器2 As New 类_SS包解读器()
                        SS包解读器2.解读纯文本(讯宝文本)
                        Dim 同步事件 As 同步事件_常量集合
                        SS包解读器2.读取_有标签("事件", 同步事件)
                        Select Case 同步事件
                            Case 同步事件_常量集合.手机上线 : 说(界面文字.获取(247, "你的手机上线了。"))
                            Case 同步事件_常量集合.讯友添加标签, 同步事件_常量集合.讯友移除标签,
                                    同步事件_常量集合.修改讯友备注, 同步事件_常量集合.拉黑讯友
                                主窗体1.事件同步(同步事件, SS包解读器2)
                            Case 同步事件_常量集合.添加讯友 : 添加讯友成功(SS包解读器2)
                            Case 同步事件_常量集合.删除讯友 : 删除讯友成功(SS包解读器2)
                            Case 同步事件_常量集合.重命名标签 : 重命名标签成功(SS包解读器2)
                            Case 同步事件_常量集合.取消拉黑讯友 : 取消拉黑讯友成功(SS包解读器2)
                            Case 同步事件_常量集合.修改群名称 : 群名称修改了(SS包解读器2)
                            Case 同步事件_常量集合.添加黑域 : 增加黑白域(True, SS包解读器2)
                            Case 同步事件_常量集合.添加白域 : 增加黑白域(False, SS包解读器2)
                            Case 同步事件_常量集合.移除黑域 : 移除黑白域(True, SS包解读器2)
                            Case 同步事件_常量集合.移除白域 : 移除黑白域(False, SS包解读器2)
                            Case 同步事件_常量集合.加入小聊天群 : 加入小聊天群(SS包解读器2)
                            Case 同步事件_常量集合.加入大聊天群 : 加入大聊天群(SS包解读器2)
                            Case 同步事件_常量集合.退出小聊天群
                            Case 同步事件_常量集合.退出大聊天群 : 退出大聊天群(SS包解读器2)
                        End Select
                    Catch ex As Exception
                        说(ex.Message)
                        Return
                    End Try
                ElseIf 讯宝指令 = 讯宝指令_常量集合.对讯友录的编辑过于频繁 Then
                    说(界面文字.获取(297, "请不要过于频繁地编辑讯友录。"))
                Else
                    If 发送序号 = 0 Then
                        SS包解读器.读取_有标签("发送序号", 发送序号)
                        If 发送序号 = 0 Then Return
                    End If
                    If 讯宝指令 = 讯宝指令_常量集合.被邀请加入大聊天群者未添加我为讯友 Then
                        SS包解读器.读取_有标签("文本", 讯宝文本, Nothing)
                    End If
                    主窗体1.提示讯宝发送失败(发送者英语讯宝地址, 群编号, 群主英语讯宝地址, 讯宝指令, 发送序号, 讯宝文本)
                End If
            End If
        End If
    End Sub

    Private Function 数据库_保存收到的一对一讯宝(ByVal 讯宝地址 As String, ByVal 发送序号 As Long,
                                     ByVal 发送时间 As Long, ByVal 讯宝指令 As 讯宝指令_常量集合,
                                     ByVal 文本 As String, ByVal 宽度 As Short, ByVal 高度 As Short,
                                     ByVal 秒数 As Byte, Optional ByVal 是同步 As Boolean = False) As Boolean
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 文本库号 As Short
            Dim 文本编号 As Long
            Dim 列添加器 As 类_列添加器
            If String.IsNullOrEmpty(文本) = False Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("地址或域", 筛选方式_常量集合.等于, 讯宝地址)
                Dim 筛选器 As New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, "地址或域编号", 筛选器, 1, 列添加器, , 主键索引名)
                Dim 地址或域编号 As Long
                读取器 = 指令2.执行()
                While 读取器.读取
                    地址或域编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                If 地址或域编号 = 0 Then
                    Call 数据库_分配地址或域编号(讯宝地址, 地址或域编号)
                    If 地址或域编号 = 0 Then Return False
                End If
                文本库号 = 获取文本库号(文本.Length)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                指令2 = New 类_数据库指令_请求获取数据(副数据库, 文本库号 & "库", Nothing, 1, 列添加器, , 主键索引名, True)
                读取器 = 指令2.执行()
                While 读取器.读取
                    文本编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                文本编号 += 1
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("编号", 文本编号)
                列添加器.添加列_用于插入数据("文本", 文本)
                列添加器.添加列_用于插入数据("地址或域编号", 地址或域编号)
                列添加器.添加列_用于插入数据("群编号", 0)
                Dim 指令3 As New 类_数据库指令_插入新数据(副数据库, 文本库号 & "库", 列添加器, True)
                指令3.执行()
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("讯宝地址", 讯宝地址)
            列添加器.添加列_用于插入数据("是接收者", 是同步)
            列添加器.添加列_用于插入数据("指令", 讯宝指令)
            列添加器.添加列_用于插入数据("文本库号", 文本库号)
            列添加器.添加列_用于插入数据("文本编号", 文本编号)
            列添加器.添加列_用于插入数据("宽度", 宽度)
            列添加器.添加列_用于插入数据("高度", 高度)
            列添加器.添加列_用于插入数据("秒数", 秒数)
            列添加器.添加列_用于插入数据("已收听", False)
            列添加器.添加列_用于插入数据("发送序号", 发送序号)
            列添加器.添加列_用于插入数据("发送时间", 发送时间)
            列添加器.添加列_用于插入数据("存储时间", Date.Now.Ticks)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "一对一讯宝", 列添加器)
            指令.执行()
            Return True
        Catch ex As 类_值已存在
            Return False
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            说(ex.Message)
            Return False
        End Try
    End Function

    Private Function 数据库_保存收到的小聊天群讯宝(ByVal 群主讯宝地址 As String, ByVal 群编号 As Byte, ByVal 发送者讯宝地址 As String,
                               ByVal 发送序号 As Long, ByVal 发送时间 As Long, ByVal 讯宝指令 As 讯宝指令_常量集合,
                               ByVal 文本 As String, ByVal 宽度 As Short, ByVal 高度 As Short, ByVal 秒数 As Byte) As Boolean
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 文本库号 As Short
            Dim 文本编号 As Long
            Dim 列添加器 As 类_列添加器
            If String.IsNullOrEmpty(文本) = False Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("地址或域", 筛选方式_常量集合.等于, 群主讯宝地址)
                Dim 筛选器 As New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, "地址或域编号", 筛选器, 1, 列添加器, , 主键索引名)
                Dim 地址或域编号 As Long
                读取器 = 指令2.执行()
                While 读取器.读取
                    地址或域编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                If 地址或域编号 = 0 Then
                    Call 数据库_分配地址或域编号(群主讯宝地址, 地址或域编号)
                    If 地址或域编号 = 0 Then Return False
                End If
                文本库号 = 获取文本库号(文本.Length)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                指令2 = New 类_数据库指令_请求获取数据(副数据库, 文本库号 & "库", Nothing, 1, 列添加器, , 主键索引名, True)
                读取器 = 指令2.执行()
                While 读取器.读取
                    文本编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                文本编号 += 1
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("编号", 文本编号)
                列添加器.添加列_用于插入数据("文本", 文本)
                列添加器.添加列_用于插入数据("地址或域编号", 地址或域编号)
                列添加器.添加列_用于插入数据("群编号", 群编号)
                Dim 指令3 As New 类_数据库指令_插入新数据(副数据库, 文本库号 & "库", 列添加器, True)
                指令3.执行()
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("群主讯宝地址", 群主讯宝地址)
            列添加器.添加列_用于插入数据("群编号", 群编号)
            列添加器.添加列_用于插入数据("发送者讯宝地址", 发送者讯宝地址)
            列添加器.添加列_用于插入数据("指令", 讯宝指令)
            列添加器.添加列_用于插入数据("文本库号", 文本库号)
            列添加器.添加列_用于插入数据("文本编号", 文本编号)
            列添加器.添加列_用于插入数据("宽度", 宽度)
            列添加器.添加列_用于插入数据("高度", 高度)
            列添加器.添加列_用于插入数据("秒数", 秒数)
            列添加器.添加列_用于插入数据("已收听", False)
            列添加器.添加列_用于插入数据("发送序号", 发送序号)
            列添加器.添加列_用于插入数据("发送时间", 发送时间)
            列添加器.添加列_用于插入数据("存储时间", Date.UtcNow.Ticks)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "小聊天群讯宝", 列添加器)
            指令.执行()
            Return True
        Catch ex As 类_值已存在
            Return False
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            说(ex.Message)
            Return False
        End Try
    End Function

    Private Function 数据库_保存收到的陌生人讯宝(ByVal 讯宝地址 As String, ByVal 发送序号 As Long,
                                     ByVal 发送时间 As Long, ByVal 讯宝指令 As 讯宝指令_常量集合, ByVal 文本 As String) As Boolean
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 文本库号 As Short
            Dim 文本编号 As Long
            Dim 列添加器 As 类_列添加器
            If String.IsNullOrEmpty(文本) = False Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("地址或域", 筛选方式_常量集合.等于, 讯宝地址)
                Dim 筛选器 As New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, "地址或域编号", 筛选器, 1, 列添加器, , 主键索引名)
                Dim 地址或域编号 As Long
                读取器 = 指令2.执行()
                While 读取器.读取
                    地址或域编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                If 地址或域编号 = 0 Then
                    Call 数据库_分配地址或域编号(讯宝地址, 地址或域编号)
                    If 地址或域编号 = 0 Then Return False
                End If
                文本库号 = 获取文本库号(文本.Length)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                指令2 = New 类_数据库指令_请求获取数据(副数据库, 文本库号 & "库", Nothing, 1, 列添加器, , 主键索引名, True)
                读取器 = 指令2.执行()
                While 读取器.读取
                    文本编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                文本编号 += 1
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("编号", 文本编号)
                列添加器.添加列_用于插入数据("文本", 文本)
                列添加器.添加列_用于插入数据("地址或域编号", 地址或域编号)
                列添加器.添加列_用于插入数据("群编号", 0)
                Dim 指令3 As New 类_数据库指令_插入新数据(副数据库, 文本库号 & "库", 列添加器, True)
                指令3.执行()
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("讯宝地址", 讯宝地址)
            列添加器.添加列_用于插入数据("指令", 讯宝指令)
            列添加器.添加列_用于插入数据("文本库号", 文本库号)
            列添加器.添加列_用于插入数据("文本编号", 文本编号)
            列添加器.添加列_用于插入数据("发送序号", 发送序号)
            列添加器.添加列_用于插入数据("发送时间", 发送时间)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "陌生人讯宝", 列添加器)
            指令.执行()
            Return True
        Catch ex As 类_值已存在
            Return False
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            说(ex.Message)
            Return False
        End Try
    End Function

    Private Sub 数据库_清除小聊天群数据(ByVal 群主英语讯宝地址 As String, ByVal 群编号 As Byte)
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 地址或域编号 As Long
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("地址或域", 筛选方式_常量集合.等于, 群主英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("编号")
            Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, "地址或域编号", 筛选器, 1, 列添加器, , 主键索引名)
            读取器 = 指令2.执行()
            While 读取器.读取
                地址或域编号 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("群主讯宝地址", 筛选方式_常量集合.等于, 群主英语讯宝地址)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "小聊天群讯宝", 筛选器, "#群主编号存储时间")
            指令.执行()
            If 地址或域编号 > 0 Then
                Dim I, 最大字数, 最大字数2 As Integer
                For I = 1 To 最大值_常量集合.讯宝文本长度
                    最大字数 = 获取文本库号(I)
                    If 最大字数 <> 最大字数2 Then
                        最大字数2 = 最大字数
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于筛选器("地址或域编号", 筛选方式_常量集合.等于, 地址或域编号)
                        列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
                        筛选器 = New 类_筛选器
                        筛选器.添加一组筛选条件(列添加器)
                        指令 = New 类_数据库指令_删除数据(副数据库, 最大字数 & "库", 筛选器, "#地址域群编号")
                        指令.执行()
                    End If
                Next
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("地址或域名", 筛选方式_常量集合.等于, 群主英语讯宝地址)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            指令 = New 类_数据库指令_删除数据(副数据库, "最近", 筛选器, "#地址群编号")
            指令.执行()
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            说(ex.Message)
        End Try
    End Sub

    Private Sub 退出小聊天群(ByVal 群主英语讯宝地址 As String, ByVal 群编号 As Byte)
        If 当前用户.加入的小聊天群 IsNot Nothing Then
            Dim 加入的小聊天群() As 类_聊天群_小 = 当前用户.加入的小聊天群
            Dim I As Integer
            For I = 0 To 加入的小聊天群.Length - 1
                If String.Compare(加入的小聊天群(I).群主.英语讯宝地址, 群主英语讯宝地址) = 0 AndAlso 加入的小聊天群(I).编号 = 群编号 Then
                    Exit For
                End If
            Next
            If I < 加入的小聊天群.Length Then
                If 加入的小聊天群.Length > 1 Then
                    Dim 加入的群2(加入的小聊天群.Length - 2) As 类_聊天群_小
                    If I > 0 Then
                        Array.Copy(加入的小聊天群, 0, 加入的群2, 0, I)
                    End If
                    If I < 加入的小聊天群.Length - 1 Then
                        Array.Copy(加入的小聊天群, I + 1, 加入的群2, I, 加入的小聊天群.Length - I - 1)
                    End If
                    加入的小聊天群 = 加入的群2
                Else
                    加入的小聊天群 = Nothing
                End If
                当前用户.加入的小聊天群 = 加入的小聊天群
                主窗体1.关闭聊天控件(群主英语讯宝地址, 群编号)
                数据库_清除小聊天群数据(群主英语讯宝地址, 群编号)
                主窗体1.刷新讯友录(讯友录显示范围_常量集合.聊天群)
            End If
        End If
    End Sub

    Private Sub 聊天群成员增加(ByVal 群主英语讯宝地址 As String, ByVal 群编号 As Byte, ByVal SS包解读器 As 类_SS包解读器,
                        ByVal 英语讯宝地址 As String, ByVal 本国语讯宝地址 As String)
        If 当前用户.加入的小聊天群 IsNot Nothing Then
            Dim 加入的小聊天群() As 类_聊天群_小 = 当前用户.加入的小聊天群
            Dim I As Integer
            For I = 0 To 加入的小聊天群.Length - 1
                If String.Compare(加入的小聊天群(I).群主.英语讯宝地址, 群主英语讯宝地址) = 0 AndAlso 加入的小聊天群(I).编号 = 群编号 Then
                    Exit For
                End If
            Next
            If I < 加入的小聊天群.Length Then
                Dim 群成员() As 类_群成员 = 加入的小聊天群(I).群成员
                If 群成员 Is Nothing Then Return
                Dim 主机名 As String = Nothing
                SS包解读器.读取_有标签("H", 主机名)
                Dim 位置号 As Short
                SS包解读器.读取_有标签("P", 位置号)
                Dim J As Integer
                For J = 0 To 群成员.Length - 1
                    If String.Compare(群成员(J).英语讯宝地址, 英语讯宝地址) = 0 Then
                        Exit For
                    End If
                Next
                If J = 群成员.Length Then
                    Dim 新成员 As New 类_群成员
                    新成员.英语讯宝地址 = 英语讯宝地址
                    新成员.本国语讯宝地址 = 本国语讯宝地址
                    新成员.主机名 = 主机名
                    新成员.位置号 = 位置号
                    新成员.角色 = 群角色_常量集合.成员_可以发言
                    新成员.所属的群 = 加入的小聊天群(I)
                    ReDim Preserve 群成员(群成员.Length)
                    群成员(群成员.Length - 1) = 新成员
                    Dim K As Integer = 1
                    For J = 0 To 群成员.Length - 1
                        With 群成员(J)
                            If .角色 <> 群角色_常量集合.群主 Then
                                群成员(J).临时编号 = K
                                K += 1
                            End If
                        End With
                    Next
                    加入的小聊天群(I).群成员 = 群成员
                    主窗体1.小聊天群成员有变化(群主英语讯宝地址, 群编号)
                Else
                    If 群成员(J).角色 = 群角色_常量集合.邀请加入_可以发言 Then
                        群成员(J).角色 = 群角色_常量集合.成员_可以发言
                        主窗体1.小聊天群成员有变化(群主英语讯宝地址, 群编号)
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub 聊天群成员减少(ByVal 群主英语讯宝地址 As String, ByVal 群编号 As Byte, ByVal 英语讯宝地址 As String)
        If 当前用户.加入的小聊天群 IsNot Nothing Then
            Dim 加入的小聊天群() As 类_聊天群_小 = 当前用户.加入的小聊天群
            Dim I As Integer
            For I = 0 To 加入的小聊天群.Length - 1
                If String.Compare(加入的小聊天群(I).群主.英语讯宝地址, 群主英语讯宝地址) = 0 AndAlso 加入的小聊天群(I).编号 = 群编号 Then
                    Exit For
                End If
            Next
            If I < 加入的小聊天群.Length Then
                Dim 群成员() As 类_群成员 = 加入的小聊天群(I).群成员
                If 群成员 Is Nothing Then Return
                Dim J As Integer
                For J = 0 To 群成员.Length - 1
                    If String.Compare(群成员(J).英语讯宝地址, 英语讯宝地址) = 0 Then
                        Exit For
                    End If
                Next
                If J < 群成员.Length Then
                    群成员(J) = Nothing
                    Dim 群成员2(群成员.Length - 2) As 类_群成员
                    Dim M As Integer
                    Dim K As Integer = 1
                    For J = 0 To 群成员.Length - 1
                        If 群成员(J) IsNot Nothing Then
                            群成员2(M) = 群成员(J)
                            If 群成员2(M).角色 <> 群角色_常量集合.群主 Then
                                群成员2(M).临时编号 = K
                                K += 1
                            End If
                            M += 1
                        End If
                    Next
                    加入的小聊天群(I).群成员 = 群成员2
                    主窗体1.小聊天群成员有变化(群主英语讯宝地址, 群编号)
                End If
            End If
        End If
    End Sub

    Private Sub 用http访问传送服务器()
        Try
            Dim HTTP网络请求 As HttpWebRequest = WebRequest.Create("https://" & 获取服务器域名(当前用户.主机名 & "." & 当前用户.域名_英语) & "/")
            HTTP网络请求.Method = "POST"
            HTTP网络请求.ContentType = "text/xml"
            HTTP网络请求.ContentLength = 0
            Using HTTP网络回应 As HttpWebResponse = HTTP网络请求.GetResponse
                If HTTP网络回应.ContentLength > 0 Then
                End If
            End Using
        Catch ex As Exception
        End Try
    End Sub

    Private Sub 群名称修改了(ByVal SS包解读器 As 类_SS包解读器)
        If 当前用户.加入的小聊天群 Is Nothing Then Return
        Dim 群编号 As Byte
        SS包解读器.读取_有标签("群编号", 群编号)
        Dim 群备注 As String = Nothing
        SS包解读器.读取_有标签("群备注", 群备注)
        Dim 用户英语讯宝地址 As String = 当前用户.英语讯宝地址
        Dim 加入的小聊天群() As 类_聊天群_小 = 当前用户.加入的小聊天群
        Dim I As Integer
        For I = 0 To 加入的小聊天群.Length - 1
            If 加入的小聊天群(I).编号 = 群编号 AndAlso String.Compare(加入的小聊天群(I).群主.英语讯宝地址, 用户英语讯宝地址) = 0 Then
                加入的小聊天群(I).备注 = 群备注
                If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.聊天群 Then
                    主窗体1.刷新讯友录()
                End If
                Exit For
            End If
        Next
    End Sub

    Private Sub 增加黑白域(ByVal 是黑域 As Boolean, ByVal SS包解读器 As 类_SS包解读器)
        Dim 英语域名 As String = Nothing
        SS包解读器.读取_有标签("英语域名", 英语域名)
        Dim 本国语域名 As String = Nothing
        SS包解读器.读取_有标签("本国语域名", 本国语域名)
        Dim 讯友录更新时间 As Long
        SS包解读器.读取_有标签("时间", 讯友录更新时间)
        If 是黑域 Then
            If String.Compare(英语域名, 黑域_全部) = 0 Then GoTo 跳转点
        End If
        If String.IsNullOrEmpty(本国语域名) Then
            If 当前用户.讯友目录 Is Nothing Then Return
            Dim 字符串 As String = 讯宝地址标识 & 英语域名
            Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
            Dim I As Integer
            For I = 0 To 讯友目录.Length - 1
                If 讯友目录(I).英语讯宝地址.EndsWith(字符串) Then Exit For
            Next
            If I = 讯友目录.Length Then Return
            If String.IsNullOrEmpty(讯友目录(I).本国语讯宝地址) = False Then
                Dim 段() As String = 讯友目录(I).本国语讯宝地址.Split(New String() {讯宝地址标识}, StringSplitOptions.RemoveEmptyEntries)
                本国语域名 = 段(1)
            Else
                本国语域名 = Nothing
            End If
        End If
跳转点:
        If 是黑域 Then
            If 当前用户.黑域 IsNot Nothing Then
                ReDim Preserve 当前用户.黑域(当前用户.黑域.Length)
            Else
                ReDim 当前用户.黑域(0)
            End If
            With 当前用户.黑域(当前用户.黑域.Length - 1)
                .英语 = 英语域名
                .本国语 = 本国语域名
            End With
            主窗体1.刷新讯友录(讯友录显示范围_常量集合.黑域)
        Else
            If 当前用户.白域 IsNot Nothing Then
                ReDim Preserve 当前用户.白域(当前用户.白域.Length)
            Else
                ReDim 当前用户.白域(0)
            End If
            With 当前用户.白域(当前用户.白域.Length - 1)
                .英语 = 英语域名
                .本国语 = 本国语域名
            End With
            主窗体1.刷新讯友录(讯友录显示范围_常量集合.白域)
        End If
        If 讯友录更新时间 > 0 Then 当前用户.讯友录更新时间 = 讯友录更新时间
        Dim 域名 As String
        If String.IsNullOrEmpty(本国语域名) Then
            域名 = 英语域名
        Else
            域名 = 本国语域名 & " / " & 英语域名
        End If
        If 是黑域 Then
            说(界面文字.获取(249, "域名[#%]被添加至黑域列表。", New Object() {域名}))
        Else
            说(界面文字.获取(250, "域名[#%]被添加至白域列表。", New Object() {域名}))
        End If
    End Sub

    Private Sub 移除黑白域(ByVal 是黑域 As Boolean, ByVal SS包解读器 As 类_SS包解读器)
        Dim 英语域名 As String = Nothing
        SS包解读器.读取_有标签("英语域名", 英语域名)
        Dim 讯友录更新时间 As Long
        SS包解读器.读取_有标签("时间", 讯友录更新时间)
        If 是黑域 Then
            If 当前用户.黑域 Is Nothing Then Return
            Dim 黑域() As 域名_复合数据 = 当前用户.黑域
            If 黑域.Length > 1 Then
                Dim 黑域2(黑域.Length - 2) As 域名_复合数据
                Dim I, J As Integer
                For I = 0 To 黑域.Length - 1
                    If String.Compare(黑域(I).英语, 英语域名) <> 0 Then
                        黑域2(J) = 黑域(I)
                        J += 1
                    End If
                Next
                黑域 = 黑域2
            Else
                黑域 = Nothing
            End If
            当前用户.黑域 = 黑域
            If 讯友录更新时间 > 0 Then 当前用户.讯友录更新时间 = 讯友录更新时间
            主窗体1.刷新讯友录(讯友录显示范围_常量集合.黑域)
            说(界面文字.获取(251, "域名[#%]从黑域列表移除了。", New Object() {英语域名}))
        Else
            If 当前用户.白域 Is Nothing Then Return
            Dim 白域() As 域名_复合数据 = 当前用户.白域
            If 白域.Length > 1 Then
                Dim 白域2(白域.Length - 2) As 域名_复合数据
                Dim I, J As Integer
                For I = 0 To 白域.Length - 1
                    If String.Compare(白域(I).英语, 英语域名) <> 0 Then
                        白域2(J) = 白域(I)
                        J += 1
                    End If
                Next
                白域 = 白域2
            Else
                白域 = Nothing
            End If
            当前用户.白域 = 白域
            If 讯友录更新时间 > 0 Then 当前用户.讯友录更新时间 = 讯友录更新时间
            主窗体1.刷新讯友录(讯友录显示范围_常量集合.白域)
            说(界面文字.获取(252, "域名[#%]从白域列表移除了。", New Object() {英语域名}))
        End If
    End Sub

    Private Sub 加入小聊天群(ByVal SS包解读器 As 类_SS包解读器)
        If 当前用户.讯友目录 Is Nothing Then Return
        Dim 群主讯宝地址 As String = Nothing
        SS包解读器.读取_有标签("群主讯宝地址", 群主讯宝地址)
        Dim 群编号 As Byte
        SS包解读器.读取_有标签("群编号", 群编号)
        Dim 群备注 As String = Nothing
        SS包解读器.读取_有标签("群备注", 群备注)
        Dim I As Integer
        If 当前用户.加入的小聊天群 IsNot Nothing Then
            Dim 加入的小聊天群() As 类_聊天群_小 = 当前用户.加入的小聊天群
            For I = 0 To 加入的小聊天群.Length - 1
                If 加入的小聊天群(I).编号 = 群编号 AndAlso String.Compare(加入的小聊天群(I).群主.英语讯宝地址, 群主讯宝地址) = 0 Then
                    Return
                End If
            Next
        End If
        Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
        For I = 0 To 讯友目录.Length - 1
            If String.Compare(讯友目录(I).英语讯宝地址, 群主讯宝地址) = 0 Then Exit For
        Next
        If I = 讯友目录.Length Then Return
        Dim 新群 As New 类_聊天群_小
        新群.群主 = 讯友目录(I)
        新群.备注 = 群备注
        新群.编号 = 群编号
        If 当前用户.加入的小聊天群 IsNot Nothing Then
            ReDim Preserve 当前用户.加入的小聊天群(当前用户.加入的小聊天群.Length)
            当前用户.加入的小聊天群(当前用户.加入的小聊天群.Length - 1) = 新群
        Else
            ReDim 当前用户.加入的小聊天群(0)
            当前用户.加入的小聊天群(0) = 新群
        End If
        主窗体1.刷新讯友录(讯友录显示范围_常量集合.聊天群)
        数据库_更新最近互动讯友排名(群主讯宝地址, 群编号)
    End Sub

    Private Sub 加入大聊天群(ByVal SS包解读器 As 类_SS包解读器)
        Dim 主机名 As String = Nothing
        SS包解读器.读取_有标签("主机名", 主机名)
        Dim 英语域名 As String = Nothing
        SS包解读器.读取_有标签("英语域名", 英语域名)
        Dim 本国语域名 As String = Nothing
        SS包解读器.读取_有标签("本国语域名", 本国语域名)
        Dim 群编号 As Long
        SS包解读器.读取_有标签("群编号", 群编号)
        Dim 群名称 As String = Nothing
        SS包解读器.读取_有标签("群名称", 群名称)
        Dim 子域名 As String = 主机名 & "." & 英语域名
        If 当前用户.加入的大聊天群 IsNot Nothing Then
            Dim 加入的大聊天群() As 类_聊天群_大 = 当前用户.加入的大聊天群
            Dim I As Integer
            For I = 0 To 加入的大聊天群.Length - 1
                If 加入的大聊天群(I).编号 = 群编号 AndAlso String.Compare(加入的大聊天群(I).子域名, 子域名) = 0 Then
                    Return
                End If
            Next
        End If
        Dim 新群 As New 类_聊天群_大
        新群.主机名 = 主机名
        新群.英语域名 = 英语域名
        新群.本国语域名 = 本国语域名
        新群.子域名 = 子域名
        新群.编号 = 群编号
        新群.名称 = 群名称
        If 当前用户.加入的大聊天群 IsNot Nothing Then
            ReDim Preserve 当前用户.加入的大聊天群(当前用户.加入的大聊天群.Length)
            当前用户.加入的大聊天群(当前用户.加入的大聊天群.Length - 1) = 新群
        Else
            ReDim 当前用户.加入的大聊天群(0)
            当前用户.加入的大聊天群(0) = 新群
        End If
        主窗体1.刷新讯友录(讯友录显示范围_常量集合.聊天群)
        数据库_更新最近互动讯友排名(子域名, 群编号)
    End Sub

    Private Sub 退出大聊天群(ByVal SS包解读器 As 类_SS包解读器)
        If 当前用户.加入的大聊天群 Is Nothing Then Return
        Dim 英语域名 As String = Nothing
        SS包解读器.读取_有标签("英语域名", 英语域名)
        Dim 主机名 As String = Nothing
        SS包解读器.读取_有标签("主机名", 主机名)
        Dim 群编号 As Long
        SS包解读器.读取_有标签("群编号", 群编号)
        Dim 子域名 As String = 主机名 & "." & 英语域名
        Dim 加入的大聊天群() As 类_聊天群_大 = 当前用户.加入的大聊天群
        Dim I As Integer
        For I = 0 To 加入的大聊天群.Length - 1
            If String.Compare(加入的大聊天群(I).子域名, 子域名) = 0 AndAlso 加入的大聊天群(I).编号 = 群编号 Then
                Exit For
            End If
        Next
        If I < 加入的大聊天群.Length Then
            If 加入的大聊天群.Length > 1 Then
                Dim 加入的群2(加入的大聊天群.Length - 2) As 类_聊天群_大
                If I > 0 Then
                    Array.Copy(加入的大聊天群, 0, 加入的群2, 0, I)
                End If
                If I < 加入的大聊天群.Length - 1 Then
                    Array.Copy(加入的大聊天群, I + 1, 加入的群2, I, 加入的大聊天群.Length - I - 1)
                End If
                加入的大聊天群 = 加入的群2
            Else
                加入的大聊天群 = Nothing
            End If
            当前用户.加入的大聊天群 = 加入的大聊天群
            主窗体1.关闭聊天控件(子域名, 群编号)
            数据库_清除大聊天群数据(子域名, 群编号)
            主窗体1.刷新讯友录(讯友录显示范围_常量集合.聊天群)
        End If
    End Sub

    Private Sub 数据库_获取大聊天群最新讯宝的发送时间(ByVal 子域名 As String, ByVal 群编号 As Long, ByRef 发送时间 As Long)
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 子域名)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("发送时间")
            Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, "大聊天群讯宝", 筛选器, 1, , , "#子域名群编号发送时间")
            读取器 = 指令2.执行()
            While 读取器.读取
                发送时间 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
        End Try
    End Sub

    Private Sub 重命名标签成功(ByVal SS包解读器 As 类_SS包解读器)
        Dim 原标签名称 As String = Nothing
        SS包解读器.读取_有标签("旧名称", 原标签名称)
        Dim 新标签名称 As String = Nothing
        SS包解读器.读取_有标签("新名称", 新标签名称)
        Dim 讯友录更新时间 As Long
        SS包解读器.读取_有标签("时间", 讯友录更新时间)
        Dim 是合并 As Boolean
        Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
        Dim I As Integer
        For I = 0 To 讯友目录.Length - 1
            With 讯友目录(I)
                If String.Compare(.标签一, 新标签名称, True) = 0 Then
                    是合并 = True
                    Exit For
                ElseIf String.Compare(.标签二, 新标签名称, True) = 0 Then
                    是合并 = True
                    Exit For
                End If
            End With
        Next
        For I = 0 To 讯友目录.Length - 1
            With 讯友目录(I)
                If String.Compare(.标签一, 原标签名称, True) = 0 Then
                    .标签一 = 新标签名称
                ElseIf String.Compare(.标签二, 原标签名称, True) = 0 Then
                    .标签二 = 新标签名称
                End If
                If String.IsNullOrEmpty(.标签一) = False AndAlso String.Compare(.标签一, .标签二, True) = 0 Then
                    .标签二 = Nothing
                End If
            End With
        Next
        If 讯友录更新时间 > 0 Then 当前用户.讯友录更新时间 = 讯友录更新时间
        If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.某标签 Then
            主窗体1.刷新讯友录()
        End If
        If 是合并 = False Then
            说(界面文字.获取(149, "标签 #% 已重命名为 #%。", New Object() {原标签名称, 新标签名称}))
        Else
            说(界面文字.获取(150, "标签 #% 里的讯友已并入标签 #%。", New Object() {原标签名称, 新标签名称}))
        End If
    End Sub

    Private Sub 删除讯友成功(ByVal SS包解读器 As 类_SS包解读器)
        If 当前用户.讯友目录 Is Nothing Then Return
        Dim 英语讯宝地址 As String = Nothing
        SS包解读器.读取_有标签("英语讯宝地址", 英语讯宝地址)
        Dim 讯友录更新时间 As Long
        SS包解读器.读取_有标签("时间", 讯友录更新时间)
        Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
        Dim I As Integer
        For I = 0 To 讯友目录.Length - 1
            If String.Compare(讯友目录(I).英语讯宝地址, 英语讯宝地址) = 0 Then
                Exit For
            End If
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
            If 讯友目录.Length > 1 Then
                Dim 讯友2(讯友目录.Length - 2) As 类_讯友
                If I > 0 Then
                    Array.Copy(讯友目录, 0, 讯友2, 0, I)
                End If
                If I < 讯友目录.Length - 1 Then
                    Array.Copy(讯友目录, I + 1, 讯友2, I, 讯友目录.Length - I - 1)
                End If
                讯友目录 = 讯友2
                For I = 0 To 讯友目录.Length - 1
                    讯友目录(I).临时编号 = I + 1
                Next
            Else
                讯友目录 = Nothing
            End If
            当前用户.讯友目录 = 讯友目录
            If 讯友录更新时间 > 0 Then 当前用户.讯友录更新时间 = 讯友录更新时间
            主窗体1.关闭聊天控件(英语讯宝地址, 0)
            数据库_清除讯友数据(英语讯宝地址)
            主窗体1.刷新讯友录()
            说(界面文字.获取(115, "讯友 #% 删除成功。[<a>#%</a>]", New Object() {地址, 任务名称_删除讯友}))
        End If
    End Sub

    Private Sub 取消拉黑讯友成功(ByVal SS包解读器 As 类_SS包解读器)
        Dim 英语讯宝地址 As String = Nothing
        SS包解读器.读取_有标签("英语讯宝地址", 英语讯宝地址)
        Dim 讯友录更新时间 As Long
        SS包解读器.读取_有标签("时间", 讯友录更新时间)
        Dim 讯友 As 类_讯友 = 当前用户.查找讯友(英语讯宝地址)
        If 讯友 IsNot Nothing Then
            讯友.拉黑 = False
            If 讯友录更新时间 > 0 Then 当前用户.讯友录更新时间 = 讯友录更新时间
            Select Case 当前用户.讯友录当前显示范围
                Case 讯友录显示范围_常量集合.最近, 讯友录显示范围_常量集合.讯友, 讯友录显示范围_常量集合.某标签, 讯友录显示范围_常量集合.黑名单
                    主窗体1.刷新讯友录()
            End Select
            Dim 地址 As String
            If String.IsNullOrEmpty(讯友.本国语讯宝地址) = False Then
                地址 = 讯友.本国语讯宝地址 & " / " & 讯友.英语讯宝地址
            Else
                地址 = 讯友.英语讯宝地址
            End If
            说(界面文字.获取(127, "已将 #% 移出黑名单。", New Object() {地址}))
        End If
    End Sub

#End Region

#Region "访问小宇宙服务器"

    Friend Sub 发布流星语(ByVal SS包() As Byte)
        任务 = New 类_任务(任务名称_发流星语, Me)
        说(界面文字.获取(159, "正在发流星语。请稍等。"))
        启动HTTPS访问线程(New 类_访问设置("https://" & 当前用户.子域名_小宇宙写入 & "/?C=PostMeteorRain&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&Credential=" & 替换URI敏感字符(当前用户.小宇宙写入凭据), , SS包))
    End Sub

    Friend Sub 流星语发布结束(ByVal 成功 As Boolean)
        If 成功 = True Then
            说(界面文字.获取(268, "流星语发布成功。"))
            聊天控件.浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("PublishSuccessful();")
        Else
            聊天控件.浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("PublishFailed();")
        End If
    End Sub

    Friend Sub 发布商品(ByVal SS包() As Byte)
        任务 = New 类_任务(任务名称_发布商品, Me)
        说(界面文字.获取(101, "正在发布商品。请稍等。"))
        启动HTTPS访问线程(New 类_访问设置("https://" & 当前用户.子域名_小宇宙写入 & "/?C=PostGoods&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&Credential=" & 替换URI敏感字符(当前用户.小宇宙写入凭据), , SS包))
    End Sub

    Friend Sub 商品发布结束(ByVal 成功 As Boolean)
        If 成功 = True Then
            说(界面文字.获取(57, "商品发布成功。"))
            聊天控件.浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("PublishSuccessful();")
        Else
            聊天控件.浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("PublishFailed();")
        End If
    End Sub

#End Region

#Region "IDisposable Support"
    Private disposedValue As Boolean

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                If 线程_HTTPS访问 IsNot Nothing Then
                    Try
                        线程_HTTPS访问.Abort()
                    Catch ex As Exception
                    End Try
                    线程_HTTPS访问 = Nothing
                End If
                聊天控件 = Nothing
                Call 关闭与传送服务器的连接()
            End If
        End If
        disposedValue = True
    End Sub

    Friend Sub 关闭与传送服务器的连接()
        Call 关闭网络连接器()
        If 线程_传送服务器 IsNot Nothing Then
            Try
                线程_传送服务器.Abort()
            Catch ex As Exception
            End Try
            线程_传送服务器 = Nothing
        End If
        If 当前用户.AES加密器 IsNot Nothing Then
            当前用户.AES加密器.Dispose()
            当前用户.AES加密器 = Nothing
        End If
        If 当前用户.AES解密器 IsNot Nothing Then
            当前用户.AES解密器.Dispose()
            当前用户.AES解密器 = Nothing
        End If
        If 当前用户.AES加解密模块 IsNot Nothing Then
            当前用户.AES加解密模块.Dispose()
            当前用户.AES加解密模块 = Nothing
        End If
    End Sub

    Friend Sub 关闭网络连接器()
        If 主窗体1.定时器_等待确认.Enabled = True Then
            主窗体1.定时器_等待确认.Stop()
        End If
        If 网络连接器 IsNot Nothing Then
            Try
                网络连接器.Close()
            Catch ex As Exception
            End Try
            网络连接器 = Nothing
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
    End Sub

#End Region

End Class
