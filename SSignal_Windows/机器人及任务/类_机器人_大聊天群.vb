Option Strict Off
Imports System.IO
Imports System.Threading
Imports System.Net
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode
Imports CefSharp

Friend Class 类_机器人_大聊天群
    Inherits 类_机器人

    Private Structure 需删除的讯宝_复合数据
        Dim 时间 As Long
        Dim 文件路径 As String
        Dim 是视频 As Boolean
    End Structure

    Dim 需删除 As 需删除的讯宝_复合数据
    Dim 正在退出聊天群, 已载入最近聊天记录 As Boolean

    Friend Sub New(ByVal 主窗体2 As 主窗体, ByVal 聊天控件1 As 控件_聊天)
        主窗体1 = 主窗体2
        聊天控件 = 聊天控件1
    End Sub

    Friend Overrides Sub 回答(ByVal 用户输入 As String, ByVal 时间 As Long)
        If String.Compare(用户输入, 任务名称_退出聊天群) = 0 Then
            正在退出聊天群 = True
        ElseIf 正在退出聊天群 = True Then
            正在退出聊天群 = False
        End If
        If String.IsNullOrEmpty(聊天控件.聊天对象.大聊天群.连接凭据) Then
            聊天控件.获取连接凭据()
            Return
        End If
        Select Case 用户输入
            Case 任务名称_小宇宙 : Call 打开群小宇宙页面()
            Case 任务名称_发送语音 : Call 发送语音还是文字(True)
            Case 任务名称_发送文字 : Call 发送语音还是文字(False)
            Case 任务名称_发送图片 : Call 发送图片(用户输入)
            Case 任务名称_发送原图 : Call 发送图片(用户输入, True)
            Case 任务名称_发送文件 : Call 发送文件(用户输入)
            Case 任务名称_邀请 : Call 邀请(用户输入)
            Case 任务名称_退出聊天群 : Call 退出聊天群(用户输入)
            Case 任务名称_昵称 : Call 昵称(用户输入)
            Case 任务名称_修改角色 : Call 修改角色(用户输入)
            Case 任务名称_删减成员 : Call 删减成员(用户输入)
            Case 任务名称_群名称 : Call 群名称(用户输入)
            Case 任务名称_图标 : Call 选择图标图片(用户输入)
            Case 任务名称_解散聊天群 : Call 解散聊天群(用户输入)
            Case 任务名称_取消
                If 任务 IsNot Nothing Then
                    任务.结束()
                    任务 = Nothing
                    说(界面文字.获取(16, "已取消。"))
                Else
                    说(界面文字.获取(93, "需要我做什么？"))
                End If
            Case Else
                If 任务 IsNot Nothing Then
                    Select Case 任务.名称
                        Case 任务名称_退出聊天群 : If 退出聊天群2(用户输入) = True Then Return
                        Case 任务名称_解散聊天群 : If 解散聊天群2(用户输入) = True Then Return
                    End Select
                End If
                任务接收用户输入(用户输入, 时间)
        End Select
    End Sub

    Private Sub 打开群小宇宙页面()
        说(界面文字.获取(7, "请稍等。"))
        聊天控件.浏览器_小宇宙.Load(获取大聊天群小宇宙的访问路径(聊天控件.聊天对象.大聊天群.子域名))
    End Sub

    Private Sub 发送语音还是文字(ByVal 语音 As Boolean)
        聊天控件.发送语音还是文字(语音)
        If 语音 Then
            说(界面文字.获取(253, "已切换至语音模式。"))
        Else
            说(界面文字.获取(254, "已切换至文字模式。"))
        End If
    End Sub

    Private Sub 发送图片(ByVal 用户输入 As String, Optional ByVal 原图发送 As Boolean = False)
        If 任务 IsNot Nothing Then 任务.结束()
        If 原图发送 = False Then
            说(界面文字.获取(23, "请选择图片（最多#%幅）。", New Object() {最大值_常量集合.选择的图片数量}))
        Else
            说(界面文字.获取(47, "请选择图片（最多#%幅）。图片不会被转换成jpg格式。", New Object() {最大值_常量集合.选择的图片数量}))
        End If
        With 主窗体1.文件选取器
            .Multiselect = True
            .Filter = 界面文字.获取(67, "所有图片文件") & "|*.jpg;*.jpeg;*.png;*.gif;*.tif;*.bmp"
            If .ShowDialog() = DialogResult.OK Then
                If .FileNames.Length > 最大值_常量集合.选择的图片数量 Then
                    说(界面文字.获取(54, "选中的图片数量不要超过#%幅。", New Object() {最大值_常量集合.选择的图片数量}))
                    Return
                End If
                Dim I As Integer
                If 原图发送 Then
                    Dim 最大值 As Long = 1024 * 1024 * 最大值_常量集合.讯宝文件数据长度_兆
                    For I = 0 To .FileNames.Length - 1
                        Dim 文件信息 As New FileInfo(.FileNames(I))
                        If 文件信息.Length > 最大值 Then
                            说(界面文字.获取(145, "文件的大小超过#%兆了。", New Object() {最大值_常量集合.讯宝文件数据长度_兆}))
                            Return
                        End If
                    Next
                End If
                Dim 当前UTC时刻 As Long = Date.UtcNow.Ticks
                Dim 原图 As Bitmap = Nothing
                Dim 压缩后图片 As Bitmap = Nothing
                Dim 文件路径 As String
                Dim 宽度, 高度 As Short
                For I = 0 To .FileNames.Length - 1
                    Try
                        文件路径 = .FileNames(I)
                        原图 = New Bitmap(文件路径)
                        If 原图发送 = False Then
                            If 原图.Width > 最大值_常量集合.讯宝图片宽高_像素 OrElse 原图.Height > 最大值_常量集合.讯宝图片宽高_像素 Then
                                Dim 缩小比例 As Double
                                If 原图.Height > 原图.Width Then
                                    缩小比例 = 最大值_常量集合.讯宝图片宽高_像素 / 原图.Height
                                Else
                                    缩小比例 = 最大值_常量集合.讯宝图片宽高_像素 / 原图.Width
                                End If
                                压缩后图片 = New Bitmap(CInt(原图.Width * 缩小比例), CInt(原图.Height * 缩小比例))
                            Else
                                压缩后图片 = New Bitmap(原图.Width, 原图.Height)
                            End If
                            Dim 绘图器 As Graphics = Graphics.FromImage(压缩后图片)
                            绘图器.DrawImage(原图, 0, 0, 压缩后图片.Width, 压缩后图片.Height)
                            绘图器.Dispose()
                            文件路径 = My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData & "\" & 当前用户.英语讯宝地址
                            If Directory.Exists(文件路径) = False Then Directory.CreateDirectory(文件路径)
                            文件路径 &= "\" & 生成大写英文字母与数字的随机字符串(20) & ".jpg"
                            压缩后图片.Save(文件路径, Imaging.ImageFormat.Jpeg)
                            压缩后图片.Dispose()
                        End If
                        If 原图.Width > 最大值_常量集合.讯宝预览图片宽高_像素 OrElse 原图.Height > 最大值_常量集合.讯宝预览图片宽高_像素 Then
                            Dim 缩小比例 As Double
                            If 原图.Height > 原图.Width Then
                                缩小比例 = 最大值_常量集合.讯宝预览图片宽高_像素 / 原图.Height
                            Else
                                缩小比例 = 最大值_常量集合.讯宝预览图片宽高_像素 / 原图.Width
                            End If
                            宽度 = 原图.Width * 缩小比例
                            高度 = 原图.Height * 缩小比例
                        Else
                            宽度 = 原图.Width
                            高度 = 原图.Height
                        End If
                        原图.Dispose()
                        聊天控件.浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_Img('" & 当前UTC时刻 & "', '" & 处理文件路径以用作JS函数参数(文件路径) & "', '" & 宽度 & "', '" & 高度 & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 聊天控件.时间格式(Date.FromBinary(当前UTC时刻)) & "');")
                    Catch ex As Exception
                        If 原图 IsNot Nothing Then 原图.Dispose()
                        If 压缩后图片 IsNot Nothing Then 压缩后图片.Dispose()
                        说(ex.Message)
                        Return
                    End Try
                    If 原图发送 = False Then
                        发送或接收(讯宝指令_常量集合.发送图片, Path.GetExtension(文件路径).Replace(".", ""), 宽度, 高度, , File.ReadAllBytes(文件路径), 当前UTC时刻, 文件路径)
                    Else
                        发送或接收(讯宝指令_常量集合.发送图片, Path.GetExtension(文件路径).Replace(".", ""), 宽度, 高度, , File.ReadAllBytes(文件路径), 当前UTC时刻)
                    End If
                    当前UTC时刻 += 1
                Next
            End If
        End With
    End Sub

    Private Sub 发送文件(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        说(界面文字.获取(305, "请选择一个文件。"))
        With 主窗体1.文件选取器
            .Multiselect = False
            .Filter = 界面文字.获取(306, "所有文件") & "|*.*"
            If .ShowDialog() = DialogResult.OK Then
                Dim 文件信息 As New FileInfo(.FileName)
                If 文件信息.Length > 1024 * 1024 * 最大值_常量集合.讯宝文件数据长度_兆 Then
                    说(界面文字.获取(145, "文件的大小超过#%兆了。", New Object() {最大值_常量集合.讯宝文件数据长度_兆}))
                    Return
                End If
                Dim 当前UTC时刻 As Long = Date.UtcNow.Ticks
                Try
                    聊天控件.浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSout_File('" & 当前UTC时刻 & "', '" & 处理文件路径以用作JS函数参数(Path.GetFileName(.FileName)) & "', '" & 处理文件路径以用作JS函数参数(.FileName) & "', '" & 获取我的头像路径(当前用户.英语用户名, 当前用户.主机名, 当前用户.头像更新时间, 当前用户.域名_英语) & "', '" & 聊天控件.时间格式(Date.FromBinary(当前UTC时刻)) & "');")
                Catch ex As Exception
                    说(ex.Message)
                    Return
                End Try
                发送或接收(讯宝指令_常量集合.发送文件, Path.GetFileName(.FileName), , , , File.ReadAllBytes(.FileName), 当前UTC时刻)
            End If
        End With
    End Sub

    Private Sub 邀请(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        显示讯友临时编号 = True
        主窗体1.刷新讯友录(讯友录显示范围_常量集合.讯友)
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.大聊天群邀请, 界面文字.获取(82, "请输入讯友的讯宝地址或临时编号（讯友备注行括号内的数字）。"))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 退出聊天群(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        任务 = New 类_任务(用户输入, Me)
        说(界面文字.获取(176, "你要退出此聊天群吗？请选择<a>#%</a>或者<a>#%</a>。", New Object() {界面文字.获取(组名_任务, 0, "是"), 界面文字.获取(组名_任务, 1, "否")}))
    End Sub

    Private Function 退出聊天群2(ByVal 用户输入 As String) As Boolean
        Select Case 用户输入
            Case 界面文字.获取(组名_任务, 0, "是")
                说(界面文字.获取(7, "请稍等。"))
                With 聊天控件.聊天对象.大聊天群
                    启动HTTPS访问线程(New 类_访问设置(获取大聊天群服务器访问路径开头(.子域名, False) & "C=LeaveGroup&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&Credential=" & 替换URI敏感字符(.连接凭据) & "&GroupID=" & .编号))
                End With
                Return True
            Case 界面文字.获取(组名_任务, 1, "否")
                回答(任务名称_取消, 0)
                Return True
        End Select
        Return False
    End Function

    Private Sub 昵称(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.大聊天群昵称, 界面文字.获取(296, "请输入你在本聊天群的新昵称（不超过#%个字符）。", New Object() {最大值_常量集合.讯友备注字符数}))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 修改角色(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.大聊天群修改角色, 界面文字.获取(180, "请输入成员的讯宝地址。"))
        任务.添加步骤(任务步骤_常量集合.大聊天群某成员的新角色, 界面文字.获取(291, "请输入括号内的数字：不可发言的普通成员（#%）、可发言的普通成员（#%）或管理员（#%）。", New Object() {群角色_常量集合.成员_不可发言, 群角色_常量集合.成员_可以发言, 群角色_常量集合.管理员}))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 删减成员(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.大聊天群删减成员, 界面文字.获取(180, "请输入成员的讯宝地址。"))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 群名称(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.大聊天群名称, 界面文字.获取(183, "请为输入群的新名称。（不超过#%个字符）", New Object() {最大值_常量集合.群名称字符数}))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 选择图标图片(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        说(界面文字.获取(59, "请选择一幅图片。"))
        With 主窗体1.文件选取器
            .Multiselect = False
            .Filter = 界面文字.获取(67, "所有图片文件") & "|*.jpg;*.jpeg;*.png"
            If .ShowDialog() = DialogResult.OK Then
                Dim 位图 As Bitmap = Nothing
                Dim 位图2 As Bitmap = Nothing
                Dim 内存流 As MemoryStream = Nothing
                Dim 字节数组() As Byte = Nothing
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
                    内存流 = New MemoryStream
                    位图2.Save(内存流, Imaging.ImageFormat.Jpeg)
                    字节数组 = 内存流.ToArray
                    内存流.Close()
                Catch ex As Exception
                    If 位图 IsNot Nothing Then 位图.Dispose()
                    If 位图2 IsNot Nothing Then 位图2.Dispose()
                    If 内存流 IsNot Nothing Then 内存流.Close()
                    说(ex.Message)
                    Return
                End Try
                任务 = New 类_任务(用户输入, Me)
                说(界面文字.获取(7, "请稍等。"))
                With 聊天控件.聊天对象.大聊天群
                    启动HTTPS访问线程(New 类_访问设置(获取大聊天群服务器访问路径开头(.子域名, False) & "C=ChangeIcon&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&Credential=" & 替换URI敏感字符(.连接凭据) & "&GroupID=" & .编号, , 字节数组))
                End With
            End If
        End With
    End Sub

    Private Sub 解散聊天群(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        任务 = New 类_任务(用户输入, Me)
        说(界面文字.获取(181, "你要解散此聊天群吗？请选择<a>#%</a>或者<a>#%</a>。", New Object() {界面文字.获取(组名_任务, 0, "是"), 界面文字.获取(组名_任务, 1, "否")}))
    End Sub

    Private Function 解散聊天群2(ByVal 用户输入 As String) As Boolean
        Select Case 用户输入
            Case 界面文字.获取(组名_任务, 0, "是")
                说(界面文字.获取(7, "请稍等。"))
                With 聊天控件.聊天对象.大聊天群
                    启动HTTPS访问线程(New 类_访问设置(获取中心服务器访问路径开头(当前用户.域名_英语) & "C=DeleteGroup&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器) & "&HostName=" & .主机名 & "&GroupID=" & .编号, 20000))
                End With
                Return True
            Case 界面文字.获取(组名_任务, 1, "否")
                回答(任务名称_取消, 0)
                Return True
        End Select
        Return False
    End Function

    Private Sub 任务接收用户输入(ByVal 用户输入 As String, ByVal 时间 As Long)
        If 任务 IsNot Nothing Then
            If 任务.步骤数量 > 0 Then
                Dim 结果 As String = 任务.保存当前步骤输入值(用户输入)
                If String.IsNullOrEmpty(结果) Then
                    结果 = 任务.获取当前步骤提示语
                    If String.IsNullOrEmpty(结果) = False Then
                        说(结果)
                    Else
                        Select Case 任务.名称
                            Case 任务名称_修改角色
                                说(界面文字.获取(7, "请稍等。"))
                                With 聊天控件.聊天对象.大聊天群
                                    启动HTTPS访问线程(New 类_访问设置(获取大聊天群服务器访问路径开头(.子域名, False) & "C=ChangeRole&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&Credential=" & 替换URI敏感字符(.连接凭据) & "&GroupID=" & .编号 & "&MEnglishSSAddress=" & 替换URI敏感字符(任务.获取某步骤的输入值(任务步骤_常量集合.大聊天群修改角色)) & "&NewRole=" & 任务.获取某步骤的输入值(任务步骤_常量集合.大聊天群某成员的新角色)))
                                End With
                            Case 任务名称_邀请
                                Dim 某一讯友 As 类_讯友 = 当前用户.查找讯友(任务.获取某步骤的输入值(任务步骤_常量集合.大聊天群邀请))
                                说(界面文字.获取(7, "请稍等。"))
                                With 聊天控件.聊天对象.大聊天群
                                    启动HTTPS访问线程(New 类_访问设置(获取大聊天群服务器访问路径开头(.子域名, False) & "C=AddMember&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&Credential=" & 替换URI敏感字符(.连接凭据) & "&GroupID=" & .编号 & "&MEnglishSSAddress=" & 替换URI敏感字符(某一讯友.英语讯宝地址) & "&MNativeSSAddress=" & 替换URI敏感字符(某一讯友.本国语讯宝地址) & "&CanSpeak=true&HostName=" & 某一讯友.主机名 & "&Position=" & 某一讯友.位置号))
                                End With
                            Case 任务名称_昵称
                                说(界面文字.获取(7, "请稍等。"))
                                With 聊天控件.聊天对象.大聊天群
                                    启动HTTPS访问线程(New 类_访问设置(获取大聊天群服务器访问路径开头(.子域名, False) & "C=ChangeNickname&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&Credential=" & 替换URI敏感字符(.连接凭据) & "&GroupID=" & .编号 & "&Nickname=" & 替换URI敏感字符(任务.获取某步骤的输入值(任务步骤_常量集合.大聊天群昵称))))
                                End With
                            Case 任务名称_删减成员
                                说(界面文字.获取(7, "请稍等。"))
                                With 聊天控件.聊天对象.大聊天群
                                    启动HTTPS访问线程(New 类_访问设置(获取大聊天群服务器访问路径开头(.子域名, False) & "C=RemoveMember&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&Credential=" & 替换URI敏感字符(.连接凭据) & "&GroupID=" & .编号 & "&MEnglishSSAddress=" & 替换URI敏感字符(任务.获取某步骤的输入值(任务步骤_常量集合.大聊天群删减成员))))
                                End With
                            Case 任务名称_群名称
                                说(界面文字.获取(7, "请稍等。"))
                                With 聊天控件.聊天对象.大聊天群
                                    启动HTTPS访问线程(New 类_访问设置(获取大聊天群服务器访问路径开头(.子域名, False) & "C=ChangeGroupName&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&Credential=" & 替换URI敏感字符(.连接凭据) & "&GroupID=" & .编号 & "&NewName=" & 替换URI敏感字符(任务.获取某步骤的输入值(任务步骤_常量集合.大聊天群名称))))
                                End With
                        End Select
                    End If
                Else
                    说(结果)
                End If
                Return
            End If
        End If
        发送或接收(讯宝指令_常量集合.发送文字, 用户输入, , , , , 时间)
    End Sub

    Friend Sub 刷新()
        If String.IsNullOrEmpty(聊天控件.聊天对象.大聊天群.连接凭据) Then
            聊天控件.获取连接凭据()
            Return
        End If
        If 任务 IsNot Nothing Then
            任务.结束()
            任务 = Nothing
        End If
        发送或接收(讯宝指令_常量集合.无)
    End Sub

    Friend Sub 发送或接收(ByVal 讯宝指令 As 讯宝指令_常量集合, Optional ByVal 文字 As String = Nothing,
                     Optional ByVal 宽度 As Short = 0, Optional ByVal 高度 As Short = 0, Optional ByVal 秒数 As Byte = 0,
                     Optional ByVal 文件数据() As Byte = Nothing, Optional ByVal 时间 As Long = 0,
                     Optional ByVal 文件路径 As String = Nothing, Optional ByVal 视频预览图片数据() As Byte = Nothing)
        需删除 = New 需删除的讯宝_复合数据
        需删除.时间 = 时间
        需删除.文件路径 = 文件路径
        If 视频预览图片数据 IsNot Nothing Then 需删除.是视频 = True
        With 聊天控件.聊天对象.大聊天群
            Dim SS包生成器 As New 类_SS包生成器()
            Call 添加数据_大聊天群发送或接收讯宝(SS包生成器, 讯宝指令, 文字, 宽度, 高度, 秒数, 文件数据, .子域名, 当前用户.加入的大聊天群, 视频预览图片数据)
            启动HTTPS访问线程(New 类_访问设置(获取大聊天群服务器访问路径开头(.子域名, False) & "C=PostOrGet&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&Credential=" & 替换URI敏感字符(.连接凭据) & "&GroupID=" & .编号, , SS包生成器.生成SS包))
            Dim 刷新 As Boolean
            If 数据库_更新最近互动讯友排名(.子域名, .编号, 刷新) = True Then
                If 刷新 Then
                    If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.最近 Then
                        主窗体1.刷新讯友录()
                    End If
                End If
            End If
        End With
    End Sub


    Protected Overrides Sub HTTPS请求成功(ByVal SS包() As Byte)
        If 聊天控件.InvokeRequired Then
            Dim d As New HTTPS请求成功_跨线程(AddressOf HTTPS请求成功)
            聊天控件.Invoke(d, New Object() {SS包})
        Else
            聊天控件.下拉列表_任务.Enabled = True
            聊天控件.按钮_说话.Enabled = True
            If SS包 IsNot Nothing Then
                Dim SS包解读器 As 类_SS包解读器
                Try
                    SS包解读器 = New 类_SS包解读器(SS包)
                    Select Case SS包解读器.查询结果
                        Case 查询结果_常量集合.成功
                            If 任务 Is Nothing Then
                                发送或接收成功(SS包解读器)
                            Else
                                Select Case 任务.名称
                                    Case 任务名称_发流星语 : 流星语发布结束(True) : GoTo 跳转点1
                                    Case 任务名称_加入大聊天群 : 获取连接凭据成功(SS包解读器)
                                    Case 任务名称_退出聊天群 : 退出聊天群成功()
                                    Case 任务名称_邀请 : 添加成员成功()
                                    Case 任务名称_群名称 : 群名称修改成功()
                                    Case 任务名称_图标 : 修改图标成功(SS包解读器)
                                    Case 任务名称_解散聊天群 : 退出聊天群成功()
                                    Case Else : 说(界面文字.获取(245, "完成。"))
                                End Select
                            End If
                        Case 查询结果_常量集合.不可发言 : 说(界面文字.获取(299, "抱歉，你不可以发言。"))
                        Case 查询结果_常量集合.发送序号不一致 : 当前用户.主控机器人.启动访问线程_传送服务器()
                        Case 讯宝指令_常量集合.本小时发送的讯宝数量已达上限 : 说(界面文字.获取(266, "本小时发送的讯宝数量已达上限。"))
                        Case 讯宝指令_常量集合.今日发送的讯宝数量已达上限 : 说(界面文字.获取(267, "今日发送的讯宝数量已达上限。"))
                        Case 查询结果_常量集合.凭据无效
                            说(界面文字.获取(282, "连接凭据已过期。"))
                            聊天控件.聊天对象.大聊天群.连接凭据 = Nothing
                            聊天控件.获取连接凭据()
                            Return
                        Case 查询结果_常量集合.稍后重试 : 说(界面文字.获取(20, "你的操作过于频繁，请#%分钟后再尝试。", New Object() {最近操作次数统计时间_分钟}))
                        Case 查询结果_常量集合.大聊天群服务器用户数已满 : 说(界面文字.获取(287, "大聊天群服务器的用户数已满。"))
                        Case 查询结果_常量集合.大聊天群名称已存在 : 说(界面文字.获取(286, "其它群已使用此名称。"))
                        Case 查询结果_常量集合.不是群成员
                            If 正在退出聊天群 = False Then
                                说(界面文字.获取(83, "你不是当前聊天群的成员。"))
                            Else
                                Call 退出聊天群成功()
                            End If
                        Case 查询结果_常量集合.不是正式群成员 : 说(界面文字.获取(285, "请在对方接受你的邀请后再进行此操作。"))
                        Case 查询结果_常量集合.无权操作 : 说(界面文字.获取(154, "你无权进行此项操作。"))
                        Case 查询结果_常量集合.账号停用 : 说(界面文字.获取(15, "账号已停用。"))
                        Case 查询结果_常量集合.系统维护 : 说(界面文字.获取(14, "由于服务器正在维护中，暂停服务。"))
                        Case 查询结果_常量集合.出错 : 说(界面文字.获取(108, "出错 #%", New Object() {SS包解读器.出错提示文本}))
                        Case 查询结果_常量集合.失败 : 说(界面文字.获取(148, "由于未知原因，操作失败。"))
                        Case 查询结果_常量集合.服务器未就绪 : 说(界面文字.获取(269, "服务器还未就绪。请稍后重试。"))
                        Case 查询结果_常量集合.数据库未就绪 : 说(界面文字.获取(141, "数据库未就绪。"))
                        Case Else : 说(界面文字.获取(108, "出错 #%", New Object() {SS包解读器.查询结果}))
                    End Select
                    If 任务 IsNot Nothing Then
                        Select Case 任务.名称
                            Case 任务名称_发流星语 : 流星语发布结束(False)
                        End Select
                    End If
                Catch ex As Exception
                    说(ex.Message)
                End Try
            End If
            If 任务 IsNot Nothing Then
跳转点1:
                任务.结束()
                任务 = Nothing
            End If
        End If
    End Sub

    Private Sub 发送或接收成功(ByVal SS包解读器 As 类_SS包解读器)
        If 当前用户.加入的大聊天群 Is Nothing Then Return
        Dim SS包解读器2() As Object = SS包解读器.读取_重复标签("GP")
        If SS包解读器2 IsNot Nothing Then
            Dim 子域名 As String = 聊天控件.聊天对象.大聊天群.子域名
            Dim 群编号, 时间 As Long
            Dim 发送者英语地址 As String = Nothing
            Dim 主机名 As String = Nothing
            Dim 讯宝指令 As 讯宝指令_常量集合
            Dim 文本 As String = Nothing
            Dim 宽度, 高度 As Short
            Dim 秒数 As Byte
            Dim 群(SS包解读器2.Length - 1) As 有新讯宝的群_复合数据
            Dim SS包解读器3() As Object
            Dim 大聊天群 As 类_聊天群_大
            Dim SS包解读器4 As 类_SS包解读器
            Dim 英语讯宝地址 As String = Nothing
            Dim 角色 As 群角色_常量集合
            Dim 图标或名称有变动 As Boolean
            Dim 加入的大聊天群() As 类_聊天群_大 = 当前用户.加入的大聊天群
            Dim 有新讯宝 As Boolean
            Dim I, J As Integer
            For I = 0 To SS包解读器2.Length - 1
                With CType(SS包解读器2(I), 类_SS包解读器)
                    .读取_有标签("GI", 群编号, 0)
                    For J = 0 To 加入的大聊天群.Length - 1
                        If 加入的大聊天群(J).编号 = 群编号 AndAlso String.Compare(加入的大聊天群(J).子域名, 子域名) = 0 Then Exit For
                    Next
                    If J < 加入的大聊天群.Length Then
                        大聊天群 = 加入的大聊天群(J)
                    Else
                        Continue For
                    End If
                    SS包解读器3 = .读取_重复标签("SS")
                End With
                If SS包解读器3 IsNot Nothing Then
                    Dim 新讯宝数量 As Integer = 0
                    Dim 撤回的讯宝() As Long = Nothing
                    Dim 撤回的讯宝数量 As Integer = 0
                    For J = 0 To SS包解读器3.Length - 1
                        With CType(SS包解读器3(J), 类_SS包解读器)
                            .读取_有标签("DT", 时间, 0)
                            .读取_有标签("FR", 发送者英语地址, Nothing)
                            .读取_有标签("HN", 主机名, Nothing)
                            .读取_有标签("CM", 讯宝指令, 讯宝指令_常量集合.无)
                            .读取_有标签("TX", 文本, Nothing)
                            .读取_有标签("WD", 宽度, 0)
                            .读取_有标签("HT", 高度, 0)
                            .读取_有标签("SC", 秒数, 0)
                        End With
                        Select Case 讯宝指令
                            Case 讯宝指令_常量集合.撤回
                                Dim 发送时间 As Long
                                If Long.TryParse(文本, 发送时间) = False Then Continue For
                                If 数据库_撤回讯宝(群编号, 发送时间) = True Then
                                    If 撤回的讯宝数量 > 0 Then
                                        撤回的讯宝(撤回的讯宝数量) = 发送时间
                                        撤回的讯宝数量 += 1
                                    Else
                                        ReDim 撤回的讯宝(SS包解读器3.Length - 1)
                                        撤回的讯宝(0) = 发送时间
                                        撤回的讯宝数量 = 1
                                    End If
                                End If
                                Continue For
                            Case 讯宝指令_常量集合.某人在聊天群的角色改变
                                Try
                                    SS包解读器4 = New 类_SS包解读器()
                                    SS包解读器4.解读纯文本(文本)
                                    SS包解读器4.读取_有标签("E", 英语讯宝地址, Nothing)
                                    If String.IsNullOrEmpty(英语讯宝地址) Then Continue For
                                    SS包解读器4.读取_有标签("R", 角色, 群角色_常量集合.无)
                                Catch ex As Exception
                                    Continue For
                                End Try
                                If String.Compare(当前用户.英语讯宝地址, 英语讯宝地址) = 0 Then
                                    大聊天群.我的角色 = 角色
                                    聊天控件.载入任务名称()
                                End If
                                Continue For
                            Case 讯宝指令_常量集合.修改聊天群名称
                                大聊天群.名称 = 文本
                                图标或名称有变动 = True
                                Continue For
                            Case 讯宝指令_常量集合.聊天群图标改变
                                Long.TryParse(文本, 大聊天群.图标更新时间)
                                图标或名称有变动 = True
                                Continue For
                        End Select
                        If String.IsNullOrEmpty(主机名) = False AndAlso String.Compare(发送者英语地址, 当前用户.英语讯宝地址) <> 0 Then
                            数据库_保存主机名(发送者英语地址, 主机名)
                        End If
                        If 数据库_保存收到的大聊天群讯宝(群编号, 发送者英语地址, 时间, 讯宝指令, 文本, 宽度, 高度, 秒数) = False Then Return
                        新讯宝数量 += 1
                        If 有新讯宝 = False Then
                            If String.Compare(发送者英语地址, 当前用户.英语讯宝地址) <> 0 Then
                                有新讯宝 = True
                            End If
                        End If
                    Next
                    With 群(I)
                        .编号 = 群编号
                        .时间 = 时间
                        .新讯宝数量 = 新讯宝数量
                        .撤回的讯宝 = 撤回的讯宝
                        .撤回的讯宝数量 = 撤回的讯宝数量
                    End With
                End If
            Next
            If 群.Length > 1 Then
                For I = 0 To 群.Length - 2
                    For J = I + 1 To 群.Length - 1
                        If 群(I).时间 > 群(J).时间 Then
                            时间 = 群(I).时间
                            群(I).时间 = 群(J).时间
                            群(J).时间 = 时间
                        End If
                    Next
                Next
            End If
            Dim 刷新 As Boolean
            For I = 0 To 群.Length - 1
                If 群(I).新讯宝数量 > 0 Then
                    数据库_更新最近互动讯友排名(子域名, 群(I).编号, 刷新)
                End If
            Next
            If 需删除.时间 > 0 Then
                If String.IsNullOrEmpty(需删除.文件路径) = False Then
                    If 需删除.文件路径.StartsWith(My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData) Then
                        If File.Exists(需删除.文件路径) Then
                            Try
                                File.Delete(需删除.文件路径)
                            Catch ex As Exception
                            End Try
                        End If
                    End If
                    If 需删除.是视频 = True Then
                        If File.Exists(需删除.文件路径 & ".jpg") Then
                            Try
                                File.Delete(需删除.文件路径 & ".jpg")
                            Catch ex As Exception
                            End Try
                        End If
                    End If
                End If
                聊天控件.浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("RemoveSS('" & 需删除.时间 & "');")
            End If
            主窗体1.显示收到的大聊天群讯宝(子域名, 群, 刷新)
            If 图标或名称有变动 Then
                If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.聊天群 Then
                    主窗体1.刷新讯友录()
                End If
            End If
            Dim 检查时间 As Long = Date.Now.Ticks
            For I = 0 To 加入的大聊天群.Length - 1
                With 加入的大聊天群(I)
                    If String.Compare(.子域名, 子域名) = 0 Then
                        .检查时间 = 检查时间
                    End If
                End With
            Next
            If 有新讯宝 Then
                Dim 音频播放器 As New 类_音频_播放
                音频播放器.开始播放本地MP3("contact.mp3")
            End If
        End If
        聊天控件.输入框.Focus()
    End Sub

    Private Sub 数据库_保存主机名(ByVal 发送者讯宝地址 As String, ByVal 主机名 As String)
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("英语讯宝地址", 发送者讯宝地址)
            列添加器.添加列_用于插入数据("主机名", 主机名)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "群成员主机名", 列添加器)
            指令.执行()
        Catch ex As 类_值已存在
        Catch ex As Exception
            说(ex.Message)
        End Try
    End Sub

    Private Function 数据库_保存收到的大聊天群讯宝(ByVal 群编号 As Long, ByVal 发送者讯宝地址 As String, ByVal 发送时间 As Long,
                                     ByVal 讯宝指令 As 讯宝指令_常量集合, ByVal 文本 As String, ByVal 宽度 As Short, ByVal 高度 As Short, ByVal 秒数 As Byte) As Boolean
        Dim 子域名 As String = 聊天控件.聊天对象.大聊天群.子域名
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 子域名)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("发送时间", 筛选方式_常量集合.等于, 发送时间)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, "大聊天群讯宝", 筛选器, 1, , , "#子域名群编号发送时间")
            Dim 已存在 As Boolean
            读取器 = 指令2.执行()
            While 读取器.读取
                已存在 = True
                Exit While
            End While
            读取器.关闭()
            If 已存在 = True Then Return True
            Dim 文本库号 As Short
            Dim 文本编号 As Long
            If String.IsNullOrEmpty(文本) = False Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("地址或域", 筛选方式_常量集合.等于, 子域名)
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                指令2 = New 类_数据库指令_请求获取数据(副数据库, "地址或域编号", 筛选器, 1, 列添加器, , 主键索引名)
                Dim 地址或域编号 As Long
                读取器 = 指令2.执行()
                While 读取器.读取
                    地址或域编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                If 地址或域编号 = 0 Then
                    Call 数据库_分配地址或域编号(子域名, 地址或域编号)
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
            列添加器.添加列_用于插入数据("子域名", 子域名)
            列添加器.添加列_用于插入数据("群编号", 群编号)
            列添加器.添加列_用于插入数据("发送者讯宝地址", 发送者讯宝地址)
            列添加器.添加列_用于插入数据("指令", 讯宝指令)
            列添加器.添加列_用于插入数据("文本库号", 文本库号)
            列添加器.添加列_用于插入数据("文本编号", 文本编号)
            列添加器.添加列_用于插入数据("宽度", 宽度)
            列添加器.添加列_用于插入数据("高度", 高度)
            列添加器.添加列_用于插入数据("秒数", 秒数)
            列添加器.添加列_用于插入数据("已收听", False)
            列添加器.添加列_用于插入数据("发送时间", 发送时间)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "大聊天群讯宝", 列添加器)
            指令.执行()
            Return True
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            说(ex.Message)
            Return False
        End Try
    End Function

    Friend Function 数据库_撤回讯宝(ByVal 群编号 As Long, ByVal 发送时间 As Long) As Boolean
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 聊天控件.聊天对象.大聊天群.子域名)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("发送时间", 筛选方式_常量集合.等于, 发送时间)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"指令", "文本库号", "文本编号"})
            Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, "大聊天群讯宝", 筛选器, 1, 列添加器, , "#子域名群编号发送时间")
            Dim 讯宝指令 As 讯宝指令_常量集合 = 讯宝指令_常量集合.无
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
            If 讯宝指令 = 讯宝指令_常量集合.无 Then Return False
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 聊天控件.聊天对象.大聊天群.子域名)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            列添加器.添加列_用于筛选器("发送时间", 筛选方式_常量集合.等于, 发送时间)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "大聊天群讯宝", 筛选器, "#子域名群编号发送时间")
            If 指令.执行() > 0 Then
                If 文本库号 > 0 Then
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 文本编号)
                    筛选器 = New 类_筛选器
                    筛选器.添加一组筛选条件(列添加器)
                    指令 = New 类_数据库指令_删除数据(副数据库, 文本库号 & "库", 筛选器, 主键索引名)
                    指令.执行()
                End If
                Return True
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
        End Try
        Return False
    End Function

    Private Sub 获取连接凭据成功(ByVal SS包解读器 As 类_SS包解读器)
        Dim 大聊天群 As 类_聊天群_大 = 聊天控件.聊天对象.大聊天群
        Dim 子域名 As String = Nothing
        SS包解读器.读取_有标签("子域名", 子域名)
        If String.Compare(大聊天群.子域名, 子域名) <> 0 Then Return
        Dim 群编号 As Long
        SS包解读器.读取_有标签("群编号", 群编号)
        If 大聊天群.编号 <> 群编号 Then Return
        SS包解读器.读取_有标签("群名称", 大聊天群.名称)
        SS包解读器.读取_有标签("图标更新时间", 大聊天群.图标更新时间)
        SS包解读器.读取_有标签("连接凭据", 大聊天群.连接凭据)
        SS包解读器.读取_有标签("角色", 大聊天群.我的角色)
        SS包解读器.读取_有标签("本国语域名", 大聊天群.本国语域名)
        Dim 加入的大聊天群() As 类_聊天群_大 = 当前用户.加入的大聊天群
        Dim I As Integer
        For I = 0 To 加入的大聊天群.Length - 1
            With 加入的大聊天群(I)
                If .编号 <> 群编号 AndAlso String.Compare(.子域名, 子域名) = 0 Then
                    If String.IsNullOrEmpty(.连接凭据) = False Then
                        .连接凭据 = 大聊天群.连接凭据
                    End If
                End If
            End With
        Next
        说(界面文字.获取(283, "收到新的连接凭据。"))
        聊天控件.载入任务名称()
        If 已载入最近聊天记录 = False Then
            聊天控件.载入最近聊天记录()
            已载入最近聊天记录 = True
        End If
        聊天控件.浏览器_小宇宙.Load(获取大聊天群小宇宙的访问路径(大聊天群.子域名))
    End Sub

    Private Sub 添加成员成功()
        Dim 某一讯友 As 类_讯友 = 当前用户.查找讯友(任务.获取某步骤的输入值(任务步骤_常量集合.大聊天群邀请))
        If 某一讯友 Is Nothing Then Return
        Dim 大聊天群 As 类_聊天群_大 = 聊天控件.聊天对象.大聊天群
        If 数据库_保存要发送的一对一讯宝(Me, 某一讯友.英语讯宝地址, Date.UtcNow.Ticks, 讯宝指令_常量集合.邀请加入大聊天群, 生成文本_邀请加入大聊天群(大聊天群.子域名, 大聊天群.编号, 大聊天群.名称)) = True Then
            主窗体1.发送讯宝()
            说(界面文字.获取(85, "已给 #% 发送了邀请。[<a>#%</a>]", New Object() {IIf(String.IsNullOrEmpty(某一讯友.本国语讯宝地址), "", 某一讯友.本国语讯宝地址 & " / ") & 某一讯友.英语讯宝地址, 任务名称_邀请}))
            数据库_更新最近互动讯友排名(大聊天群.子域名, 大聊天群.编号)
        End If
    End Sub

    Private Sub 群名称修改成功()
        聊天控件.聊天对象.大聊天群.名称 = 任务.获取某步骤的输入值(任务步骤_常量集合.大聊天群名称)
        If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.聊天群 Then
            主窗体1.刷新讯友录()
        End If
        说(界面文字.获取(245, "完成。"))
    End Sub

    Private Sub 修改图标成功(ByVal SS包解读器 As 类_SS包解读器)
        SS包解读器.读取_有标签("图标更新时间", 聊天控件.聊天对象.大聊天群.图标更新时间)
        说(界面文字.获取(284, "群图标已更新。"))
        If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.聊天群 Then
            主窗体1.刷新讯友录()
        End If
    End Sub

    Private Sub 退出聊天群成功()
        Dim SS包生成器 As New 类_SS包生成器()
        With 聊天控件.聊天对象.大聊天群
            SS包生成器.添加_有标签("英语域名", .英语域名)
            SS包生成器.添加_有标签("主机名", .主机名)
            SS包生成器.添加_有标签("群编号", .编号)
        End With
        If 数据库_保存要发送的一对一讯宝(Me, 当前用户.英语讯宝地址, Date.UtcNow.Ticks, 讯宝指令_常量集合.退出大聊天群, SS包生成器.生成纯文本) = True Then
            主窗体1.发送讯宝()
            If 当前用户.加入的大聊天群 Is Nothing Then Return
            Dim 子域名 As String
            Dim 群编号 As Long
            With 聊天控件.聊天对象.大聊天群
                子域名 = .子域名
                群编号 = .编号
            End With
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
            End If
            主窗体1.关闭聊天控件(子域名, 群编号)
            数据库_清除大聊天群数据(子域名, 群编号)
            主窗体1.刷新讯友录(讯友录显示范围_常量集合.聊天群)
        End If
    End Sub


    Friend Sub 发布流星语(ByVal SS包() As Byte)
        任务 = New 类_任务(任务名称_发流星语, Me)
        说(界面文字.获取(159, "正在发流星语。请稍等。"))
        With 聊天控件.聊天对象.大聊天群
            启动HTTPS访问线程(New 类_访问设置(获取大聊天群服务器访问路径开头(.子域名, False) & "C=PostMeteorRain&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&Credential=" & 替换URI敏感字符(.连接凭据) & "&GroupID=" & .编号, , SS包))
        End With
    End Sub

    Friend Sub 流星语发布结束(ByVal 成功 As Boolean)
        If 成功 = True Then
            说(界面文字.获取(268, "流星语发布成功。"))
            聊天控件.浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("PublishSuccessful();")
        Else
            聊天控件.浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("PublishFailed();")
        End If
    End Sub

    Friend Sub 请求讯宝中心小宇宙分配读取服务器()
        Dim 访问设置 As 类_访问设置
        With 聊天控件.聊天对象.大聊天群
            访问设置 = New 类_访问设置(获取大聊天群服务器访问路径开头(.子域名, False) & "C=GetAServerForRead&EnglishSSAddress=" & 替换URI敏感字符(当前用户.英语讯宝地址) & "&Credential=" & 替换URI敏感字符(.连接凭据) & "&GroupID=" & .编号)
        End With
        Dim 线程 As New Thread(New ParameterizedThreadStart(AddressOf HTTPS静默访问))
        线程.Start(访问设置)
    End Sub

    Private Sub HTTPS静默访问(ByVal 参数 As Object)
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
            HTTP网络请求.ContentType = "text/xml"
            HTTP网络请求.ContentLength = 0
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
                Dim 子域名 As String = Nothing
                Try
                    Dim SS包解读器 As New 类_SS包解读器(收到的字节数组)
                    If SS包解读器.查询结果 = 查询结果_常量集合.成功 Then
                        SS包解读器.读取_有标签("子域名", 子域名)
                    End If
                Catch ex As Exception
                End Try
                If String.IsNullOrEmpty(子域名) = False Then
                    聊天控件.收到子域名(子域名)
                End If
            End If
        End If
    End Sub

End Class
