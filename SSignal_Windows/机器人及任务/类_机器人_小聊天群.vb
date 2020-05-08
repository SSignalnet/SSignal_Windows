Option Strict Off
Imports System.IO
Imports SSignal_Protocols
Imports CefSharp

Friend Class 类_机器人_小聊天群
    Inherits 类_机器人

    Friend Sub New(ByVal 主窗体2 As 主窗体, ByVal 聊天控件1 As 控件_聊天)
        主窗体1 = 主窗体2
        聊天控件 = 聊天控件1
    End Sub

    Friend Overrides Sub 回答(ByVal 用户输入 As String, ByVal 时间 As Long)
        If 聊天控件.聊天对象.小聊天群.群成员 Is Nothing Then
            说(界面文字.获取(187, "正在获取成员列表。请稍等。"))
            聊天控件.获取成员列表()
            Return
        End If
        Select Case 用户输入
            Case 任务名称_小宇宙 : Call 打开小宇宙页面()
            Case 任务名称_发送语音 : Call 发送语音还是文字(True)
            Case 任务名称_发送文字 : Call 发送语音还是文字(False)
            Case 任务名称_发送图片 : Call 发送图片(用户输入)
            Case 任务名称_发送原图 : Call 发送图片(用户输入, True)
            Case 任务名称_发送文件 : Call 发送文件(用户输入)
            Case 任务名称_邀请 : Call 邀请(用户输入)
            Case 任务名称_退出聊天群 : Call 退出聊天群(用户输入)
            Case 任务名称_删减成员 : Call 删减成员(用户输入)
            Case 任务名称_群名称 : Call 群名称(用户输入)
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

    Friend Sub 打开小宇宙页面(Optional ByVal 不显示提示 As Boolean = False)
        If 不显示提示 = False Then 说(界面文字.获取(7, "请稍等。"))
        聊天控件.浏览器_小宇宙.Load(获取讯友小宇宙的访问路径(聊天控件.聊天对象.讯友或群主.英语讯宝地址, 当前用户.英语讯宝地址))
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
                    If 数据库_保存要发送的小聊天群讯宝(Me, 聊天控件.聊天对象.讯友或群主.英语讯宝地址, 聊天控件.聊天对象.小聊天群.编号, 当前UTC时刻, 讯宝指令_常量集合.发送图片, 文件路径, 宽度, 高度) = True Then
                        主窗体1.发送讯宝()
                    End If
                    当前UTC时刻 += 1
                Next
                Dim 刷新 As Boolean
                If 数据库_更新最近互动讯友排名(聊天控件.聊天对象.讯友或群主.英语讯宝地址, 聊天控件.聊天对象.小聊天群.编号, 刷新) = True Then
                    If 刷新 Then
                        If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.最近 Then
                            主窗体1.刷新讯友录()
                        End If
                    End If
                End If
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
                If 数据库_保存要发送的小聊天群讯宝(Me, 聊天控件.聊天对象.讯友或群主.英语讯宝地址, 聊天控件.聊天对象.小聊天群.编号, 当前UTC时刻, 讯宝指令_常量集合.发送文件, .FileName) = True Then
                    主窗体1.发送讯宝()
                End If
                Dim 刷新 As Boolean
                If 数据库_更新最近互动讯友排名(聊天控件.聊天对象.讯友或群主.英语讯宝地址, 聊天控件.聊天对象.小聊天群.编号, 刷新) = True Then
                    If 刷新 Then
                        If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.最近 Then
                            主窗体1.刷新讯友录()
                        End If
                    End If
                End If
            End If
        End With
    End Sub

    Private Sub 邀请(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        If 聊天控件.聊天对象.小聊天群.群成员.Length >= 最大值_常量集合.小聊天群成员数量 Then
            说(界面文字.获取(171, "群成员数量已达上限。"))
            Return
        End If
        显示讯友临时编号 = True
        主窗体1.刷新讯友录(讯友录显示范围_常量集合.讯友)
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.小聊天群邀请, 界面文字.获取(82, "请输入讯友的讯宝地址或临时编号（讯友备注行括号内的数字）。"))
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
                If 数据库_保存要发送的小聊天群讯宝(Me, 聊天控件.聊天对象.讯友或群主.英语讯宝地址, 聊天控件.聊天对象.小聊天群.编号, Date.UtcNow.Ticks, 讯宝指令_常量集合.退出小聊天群) = True Then
                    说(界面文字.获取(7, "请稍等。"))
                    主窗体1.发送讯宝()
                End If
                Return True
            Case 界面文字.获取(组名_任务, 1, "否")
                回答(任务名称_取消, 0)
                Return True
        End Select
        Return False
    End Function

    Private Sub 删减成员(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        If 聊天控件.聊天对象.小聊天群.群成员.Length = 1 Then
            说(界面文字.获取(170, "除你之外，聊天群里没有其他人。"))
            Return
        End If
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.小聊天群删减成员, 界面文字.获取(180, "请输入成员的讯宝地址。"))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 群名称(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.小聊天群名称, 界面文字.获取(183, "请为输入群的新名称。（不超过#%个字符）", New Object() {最大值_常量集合.群名称字符数}))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 解散聊天群(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        If 聊天控件.聊天对象.小聊天群.群成员.Length > 1 Then
            说(界面文字.获取(182, "无法解散还有成员的群。"))
            Return
        End If
        任务 = New 类_任务(用户输入, Me)
        说(界面文字.获取(181, "你要解散此聊天群吗？请选择<a>#%</a>或者<a>#%</a>。", New Object() {界面文字.获取(组名_任务, 0, "是"), 界面文字.获取(组名_任务, 1, "否")}))
    End Sub

    Private Function 解散聊天群2(ByVal 用户输入 As String) As Boolean
        Select Case 用户输入
            Case 界面文字.获取(组名_任务, 0, "是")
                If 数据库_保存要发送的小聊天群讯宝(Me, 聊天控件.聊天对象.讯友或群主.英语讯宝地址, 聊天控件.聊天对象.小聊天群.编号, Date.UtcNow.Ticks, 讯宝指令_常量集合.解散小聊天群) = True Then
                    说(界面文字.获取(7, "请稍等。"))
                    主窗体1.发送讯宝()
                End If
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
                            Case 任务名称_邀请
                                Dim 某一讯友 As 类_讯友 = 当前用户.查找讯友(任务.获取某步骤的输入值(任务步骤_常量集合.小聊天群邀请))
                                Dim 小聊天群 As 类_聊天群_小 = 聊天控件.聊天对象.小聊天群
                                If 数据库_保存要发送的一对一讯宝(Me, 某一讯友.英语讯宝地址, 时间, 讯宝指令_常量集合.邀请加入小聊天群, 生成文本_邀请加入小聊天群(小聊天群.编号, 小聊天群.备注)) = True Then
                                    任务.结束()
                                    说(界面文字.获取(85, "已给 #% 发送了邀请。[<a>#%</a>]", New Object() {IIf(String.IsNullOrEmpty(某一讯友.本国语讯宝地址), "", 某一讯友.本国语讯宝地址 & " / ") & 某一讯友.英语讯宝地址, 任务名称_邀请}))
                                    Dim 新成员 As New 类_群成员 With {
                                        .英语讯宝地址 = 某一讯友.英语讯宝地址,
                                        .本国语讯宝地址 = 某一讯友.本国语讯宝地址,
                                        .主机名 = 某一讯友.主机名,
                                        .位置号 = 某一讯友.位置号,
                                        .角色 = 群角色_常量集合.邀请加入_可以发言,
                                        .所属的群 = 小聊天群
                                    }
                                    Dim 群成员() As 类_群成员 = CType(新成员.所属的群, 类_聊天群_小).群成员
                                    ReDim Preserve 群成员(群成员.Length)
                                    群成员(群成员.Length - 1) = 新成员
                                    Dim K As Integer = 1
                                    Dim J As Integer
                                    For J = 0 To 群成员.Length - 1
                                        With 群成员(J)
                                            If .角色 <> 群角色_常量集合.群主 Then
                                                群成员(J).临时编号 = K
                                                K += 1
                                            End If
                                        End With
                                    Next
                                    CType(新成员.所属的群, 类_聊天群_小).群成员 = 群成员
                                    If 数据库_更新最近互动讯友排名(聊天控件.聊天对象.讯友或群主.英语讯宝地址, 小聊天群.编号) = True Then
                                        主窗体1.发送讯宝()
                                    End If
                                End If
                            Case 任务名称_删减成员
                                If 数据库_保存要发送的小聊天群讯宝(Me, 聊天控件.聊天对象.讯友或群主.英语讯宝地址, 聊天控件.聊天对象.小聊天群.编号, Date.UtcNow.Ticks, 讯宝指令_常量集合.删减聊天群成员, 任务.获取某步骤的输入值(任务步骤_常量集合.小聊天群删减成员)) = True Then
                                    任务.结束()
                                    说(界面文字.获取(7, "请稍等。"))
                                    GoTo 跳转点1
                                End If
                            Case 任务名称_群名称
                                Dim 群的新名称 As String = 任务.获取某步骤的输入值(任务步骤_常量集合.小聊天群名称)
                                If 数据库_保存要发送的小聊天群讯宝(Me, 聊天控件.聊天对象.讯友或群主.英语讯宝地址, 聊天控件.聊天对象.小聊天群.编号, Date.UtcNow.Ticks, 讯宝指令_常量集合.修改聊天群名称, 群的新名称) = True Then
                                    任务.结束()
                                    If 当前用户.加入的小聊天群 IsNot Nothing Then
                                        Dim 群主英语讯宝地址 As String = 聊天控件.聊天对象.讯友或群主.英语讯宝地址
                                        Dim 群编号 As Byte = 聊天控件.聊天对象.小聊天群.编号
                                        Dim 加入的小聊天群() As 类_聊天群_小 = 当前用户.加入的小聊天群
                                        Dim I As Integer
                                        For I = 0 To 加入的小聊天群.Length - 1
                                            If String.Compare(加入的小聊天群(I).群主.英语讯宝地址, 群主英语讯宝地址) = 0 AndAlso 加入的小聊天群(I).编号 = 群编号 Then
                                                加入的小聊天群(I).备注 = 群的新名称
                                                主窗体1.刷新讯友录()
                                                Exit For
                                            End If
                                        Next
                                    End If
                                    GoTo 跳转点1
                                End If
                        End Select
                    End If
                Else
                    说(结果)
                End If
                Return
            End If
        End If
        If 数据库_保存要发送的小聊天群讯宝(Me, 聊天控件.聊天对象.讯友或群主.英语讯宝地址, 聊天控件.聊天对象.小聊天群.编号, 时间, 讯宝指令_常量集合.发送文字, 用户输入) = True Then
跳转点1:
            Dim 刷新 As Boolean
            If 数据库_更新最近互动讯友排名(聊天控件.聊天对象.讯友或群主.英语讯宝地址, 聊天控件.聊天对象.小聊天群.编号, 刷新) = True Then
                If 刷新 Then
                    If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.最近 Then
                        主窗体1.刷新讯友录()
                    End If
                End If
                主窗体1.发送讯宝()
            End If
        End If
    End Sub

End Class
