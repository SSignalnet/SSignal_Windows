Option Strict Off
Imports System.IO
Imports System.Text
Imports SSignal_Protocols
Imports SSignal_GlobalCommonCode
Imports CefSharp

Friend Class 类_机器人_一对一
    Inherits 类_机器人

    Friend Sub New(ByVal 主窗体2 As 主窗体, ByVal 聊天控件1 As 控件_聊天)
        主窗体1 = 主窗体2
        聊天控件 = 聊天控件1
    End Sub

    Friend Overrides Sub 回答(ByVal 用户输入 As String, ByVal 时间 As Long)
        Select Case 用户输入
            Case 任务名称_小宇宙 : Call 打开小宇宙页面()
            Case 任务名称_发送语音 : Call 发送语音还是文字(True)
            Case 任务名称_发送文字 : Call 发送语音还是文字(False)
            Case 任务名称_发送图片 : Call 发送图片(用户输入)
            Case 任务名称_发送原图 : Call 发送图片(用户输入, True)
            Case 任务名称_发送文件 : Call 发送文件(用户输入)
            Case 任务名称_添加新标签 : Call 添加新标签(用户输入)
            Case 任务名称_添加现有标签 : Call 添加现有标签(用户输入)
            Case 任务名称_移除标签 : Call 移除标签(用户输入)
            Case 任务名称_备注 : Call 修改备注(用户输入)
            Case 任务名称_拉黑 : Call 拉黑(用户输入)
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
                        Case 任务名称_拉黑 : If 拉黑2(用户输入) = True Then Return
                    End Select
                End If
                任务接收用户输入(用户输入, 时间)
        End Select
    End Sub

    Private Sub 打开小宇宙页面()
        说(界面文字.获取(7, "请稍等。"))
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

    Private Sub 列出讯友当前标签()
        Dim 当前标签 As String = Nothing
        With 聊天控件.聊天对象.讯友或群主
            If String.IsNullOrEmpty(.标签一) = False Then
                当前标签 = .标签一
            End If
            If String.IsNullOrEmpty(.标签二) = False Then
                If String.IsNullOrEmpty(当前标签) Then
                    当前标签 = .标签二
                Else
                    当前标签 &= ", " & .标签二
                End If
            End If
        End With
        If String.IsNullOrEmpty(当前标签) = False Then
            说(界面文字.获取(129, "目前的标签：#%", New Object() {当前标签}))
        Else
            说(界面文字.获取(88, "目前没有标签"))
        End If
    End Sub

    Private Sub 发送图片(ByVal 用户输入 As String, Optional ByVal 原图发送 As Boolean = False)
        If 任务 IsNot Nothing Then 任务.结束()
        If 聊天控件.聊天对象.讯友或群主.拉黑 Then
            说(界面文字.获取(160, "你已将此讯友拉黑。"))
            Return
        End If
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
                    If 数据库_保存要发送的一对一讯宝(Me, 聊天控件.聊天对象.讯友或群主.英语讯宝地址, 当前UTC时刻, 讯宝指令_常量集合.发送图片, 文件路径, 宽度, 高度) = True Then
                        主窗体1.发送讯宝()
                    End If
                    当前UTC时刻 += 1
                Next
                Dim 刷新 As Boolean
                If 数据库_更新最近互动讯友排名(聊天控件.聊天对象.讯友或群主.英语讯宝地址, 0, 刷新) = True Then
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
        If 聊天控件.聊天对象.讯友或群主.拉黑 Then
            说(界面文字.获取(160, "你已将此讯友拉黑。"))
            Return
        End If
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
                If 数据库_保存要发送的一对一讯宝(Me, 聊天控件.聊天对象.讯友或群主.英语讯宝地址, 当前UTC时刻, 讯宝指令_常量集合.发送文件, .FileName) = True Then
                    主窗体1.发送讯宝()
                End If
                Dim 刷新 As Boolean
                If 数据库_更新最近互动讯友排名(聊天控件.聊天对象.讯友或群主.英语讯宝地址, 0, 刷新) = True Then
                    If 刷新 Then
                        If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.最近 Then
                            主窗体1.刷新讯友录()
                        End If
                    End If
                End If
            End If
        End With
    End Sub

    Private Sub 添加新标签(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        If 聊天控件.聊天对象.讯友或群主.拉黑 = True Then
            说(界面文字.获取(160, "你已将此讯友拉黑。"))
            Return
        End If
        列出讯友当前标签()
        With 聊天控件.聊天对象.讯友或群主
            If String.IsNullOrEmpty(.标签一) = False AndAlso String.IsNullOrEmpty(.标签二) = False Then
                说(界面文字.获取(134, "无法添加更多标签。每个讯友最多可以添加两个标签。"))
                Return
            End If
        End With
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.添加新标签, 界面文字.获取(130, "你可以用标签对讯友进行分类。请输入一个新的标签名称，如同学、市场部等。（不超过#%个字符）", New Object() {最大值_常量集合.讯友标签字符数}))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 添加现有标签(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        If 聊天控件.聊天对象.讯友或群主.拉黑 = True Then
            说(界面文字.获取(160, "你已将此讯友拉黑。"))
            Return
        End If
        列出讯友当前标签()
        With 聊天控件.聊天对象.讯友或群主
            If String.IsNullOrEmpty(.标签一) = False AndAlso String.IsNullOrEmpty(.标签二) = False Then
                说(界面文字.获取(134, "无法添加更多标签。每个讯友最多可以添加两个标签。"))
                Return
            End If
        End With
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
                With 聊天控件.聊天对象.讯友或群主
                    For I = 0 To 讯友标签数 - 1
                        If String.Compare(讯友标签(I), .标签一) <> 0 AndAlso String.Compare(讯友标签(I), .标签二) <> 0 Then
                            If I > 0 Then 文本写入器.Write(", ")
                            文本写入器.Write("<a>" & 讯友标签(I) & "</a>")
                        End If
                    Next
                End With
                文本写入器.Close()
                If 变长文本.Length > 0 Then
                    任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
                    任务.添加步骤(任务步骤_常量集合.添加现有标签, 界面文字.获取(138, "请选择现有标签：#%", New Object() {文本写入器.ToString}))
                    说(任务.获取当前步骤提示语)
                    Return
                End If
            End If
        End If
        说(界面文字.获取(137, "没有可选的标签。"))
    End Sub

    Private Sub 移除标签(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        If 聊天控件.聊天对象.讯友或群主.拉黑 = True Then
            说(界面文字.获取(160, "你已将此讯友拉黑。"))
            Return
        End If
        列出讯友当前标签()
        With 聊天控件.聊天对象.讯友或群主
            If String.IsNullOrEmpty(.标签一) = True AndAlso String.IsNullOrEmpty(.标签二) = True Then
                Return
            End If
        End With
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.移除标签, 界面文字.获取(140, "请输入要移除的标签名称。"))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 修改备注(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        If 聊天控件.聊天对象.讯友或群主.拉黑 = True Then
            说(界面文字.获取(160, "你已将此讯友拉黑。"))
            Return
        End If
        说(界面文字.获取(152, "目前的备注：#%", New Object() {聊天控件.聊天对象.讯友或群主.备注}))
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.修改讯友备注, 界面文字.获取(151, "请输入新的备注（不超过#%个字符）。", New Object() {最大值_常量集合.讯友备注字符数}))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 拉黑(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        If 聊天控件.聊天对象.讯友或群主.拉黑 = True Then
            说(界面文字.获取(160, "你已将此讯友拉黑。"))
            Return
        End If
        任务 = New 类_任务(用户输入, Me)
        说(界面文字.获取(124, "你要添加此讯友至黑名单吗？请选择<a>#%</a>或者<a>#%</a>。", New Object() {界面文字.获取(组名_任务, 0, "是"), 界面文字.获取(组名_任务, 1, "否")}))
    End Sub

    Private Function 拉黑2(ByVal 用户输入 As String) As Boolean
        Select Case 用户输入
            Case 界面文字.获取(组名_任务, 0, "是")
                Dim SS包生成器 As New 类_SS包生成器
                SS包生成器.添加_有标签("英语讯宝地址", 聊天控件.聊天对象.讯友或群主.英语讯宝地址)
                SS包生成器.添加_有标签("拉黑", True)
                If 数据库_保存要发送的一对一讯宝(Me, 当前用户.英语讯宝地址, Date.UtcNow.Ticks, 讯宝指令_常量集合.拉黑取消拉黑讯友, SS包生成器.生成纯文本) = True Then
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
                            Case 任务名称_备注
                                Dim 备注 As String = 任务.获取某步骤的输入值(任务步骤_常量集合.修改讯友备注)
                                Dim SS包生成器 As New 类_SS包生成器
                                SS包生成器.添加_有标签("英语讯宝地址", 聊天控件.聊天对象.讯友或群主.英语讯宝地址)
                                SS包生成器.添加_有标签("备注", 备注)
                                If 数据库_保存要发送的一对一讯宝(Me, 当前用户.英语讯宝地址, Date.UtcNow.Ticks, 讯宝指令_常量集合.修改讯友备注, SS包生成器.生成纯文本) = True Then
                                    主窗体1.发送讯宝()
                                End If
                                任务.结束()
                            Case 任务名称_添加新标签, 任务名称_添加现有标签
                                Dim 标签名称 As String
                                If String.Compare(任务.名称, 任务名称_添加新标签, True) = 0 Then
                                    标签名称 = 任务.获取某步骤的输入值(任务步骤_常量集合.添加新标签)
                                Else
                                    标签名称 = 任务.获取某步骤的输入值(任务步骤_常量集合.添加现有标签)
                                End If
                                Dim SS包生成器 As New 类_SS包生成器
                                SS包生成器.添加_有标签("英语讯宝地址", 聊天控件.聊天对象.讯友或群主.英语讯宝地址)
                                SS包生成器.添加_有标签("标签名称", 标签名称)
                                If 数据库_保存要发送的一对一讯宝(Me, 当前用户.英语讯宝地址, Date.UtcNow.Ticks, 讯宝指令_常量集合.给讯友添加标签, SS包生成器.生成纯文本) = True Then
                                    主窗体1.发送讯宝()
                                End If
                                任务.结束()
                            Case 任务名称_移除标签
                                Dim 标签名称 As String = 任务.获取某步骤的输入值(任务步骤_常量集合.移除标签)
                                Dim SS包生成器 As New 类_SS包生成器
                                SS包生成器.添加_有标签("英语讯宝地址", 聊天控件.聊天对象.讯友或群主.英语讯宝地址)
                                SS包生成器.添加_有标签("标签名称", 标签名称)
                                If 数据库_保存要发送的一对一讯宝(Me, 当前用户.英语讯宝地址, Date.UtcNow.Ticks, 讯宝指令_常量集合.移除讯友标签, SS包生成器.生成纯文本) = True Then
                                    主窗体1.发送讯宝()
                                End If
                                任务.结束()
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
        If 聊天控件.聊天对象.讯友或群主.拉黑 Then
            说(界面文字.获取(160, "你已将此讯友拉黑。"))
            Return
        End If
        If 数据库_保存要发送的一对一讯宝(Me, 聊天控件.聊天对象.讯友或群主.英语讯宝地址, 时间, 讯宝指令_常量集合.发送文字, 用户输入) = True Then
            Dim 刷新 As Boolean
            If 数据库_更新最近互动讯友排名(聊天控件.聊天对象.讯友或群主.英语讯宝地址, 0, 刷新) = True Then
                If 刷新 Then
                    If 当前用户.讯友录当前显示范围 = 讯友录显示范围_常量集合.最近 Then
                        主窗体1.刷新讯友录()
                    End If
                End If
                主窗体1.发送讯宝()
            End If
        End If
    End Sub

    Protected Overrides Sub HTTPS请求成功(ByVal SS包() As Byte)
        If 聊天控件.InvokeRequired Then
            Dim d As New HTTPS请求成功_跨线程(AddressOf HTTPS请求成功)
            聊天控件.Invoke(d, New Object() {SS包})
        Else
            聊天控件.下拉列表_任务.Enabled = True
            聊天控件.按钮_说话.Enabled = True
            If SS包 IsNot Nothing Then
                Dim SS包解读器 As New 类_SS包解读器(SS包)
                Select Case SS包解读器.查询结果
                    Case 查询结果_常量集合.成功
                        Select Case 任务.名称
                            Case 任务名称_加入大聊天群 : 加入大聊天群成功(SS包解读器)
                        End Select
                    Case 查询结果_常量集合.某标签讯友数满了 : 说(界面文字.获取(135, "失败，因为每个标签最多只能标记 #% 个讯友。", New Object() {最大值_常量集合.每个标签讯友数量}))
                    Case 查询结果_常量集合.不是群成员 : 说(界面文字.获取(83, "你不是当前聊天群的成员。"))
                    Case 查询结果_常量集合.稍后重试 : 说(界面文字.获取(20, "你的操作过于频繁，请#%分钟后再尝试。", New Object() {最近操作次数统计时间_分钟}))
                    Case 查询结果_常量集合.凭据无效 : 说(界面文字.获取(229, "请注销，然后重新登录。")) : Return   '不可直接调用主窗体的 注销成功 方法
                    Case 查询结果_常量集合.账号停用 : 说(界面文字.获取(15, "账号已停用。"))
                    Case 查询结果_常量集合.系统维护 : 说(界面文字.获取(14, "由于服务器正在维护中，暂停服务。"))
                    Case 查询结果_常量集合.出错 : 说(界面文字.获取(108, "出错 #%", New Object() {SS包解读器.出错提示文本}))
                    Case 查询结果_常量集合.失败 : 说(界面文字.获取(148, "由于未知原因，操作失败。"))
                    Case 查询结果_常量集合.服务器未就绪 : 说(界面文字.获取(269, "服务器还未就绪。请稍后重试。"))
                    Case 查询结果_常量集合.数据库未就绪 : 说(界面文字.获取(141, "数据库未就绪。"))
                    Case Else : 说(界面文字.获取(108, "出错 #%", New Object() {SS包解读器.查询结果}))
                End Select
            End If
            任务.结束()
            任务 = Nothing
        End If
    End Sub

    Friend Sub 添加标签成功(ByVal SS包解读器 As 类_SS包解读器)
        Dim 标签名称 As String = Nothing
        SS包解读器.读取_有标签("标签名称", 标签名称)
        Dim 讯友录更新时间 As Long
        SS包解读器.读取_有标签("时间", 讯友录更新时间)
        With 聊天控件.聊天对象.讯友或群主
            If String.IsNullOrEmpty(.标签一) Then
                .标签一 = 标签名称
            ElseIf String.IsNullOrEmpty(.标签二) Then
                .标签二 = 标签名称
            End If
        End With
        If 讯友录更新时间 > 0 Then 当前用户.讯友录更新时间 = 讯友录更新时间
        列出讯友当前标签()
    End Sub

    Friend Sub 移除标签成功(ByVal SS包解读器 As 类_SS包解读器)
        Dim 标签名称 As String = Nothing
        SS包解读器.读取_有标签("标签名称", 标签名称)
        Dim 讯友录更新时间 As Long
        SS包解读器.读取_有标签("时间", 讯友录更新时间)
        With 聊天控件.聊天对象.讯友或群主
            If String.Compare(.标签一, 标签名称, True) = 0 Then
                .标签一 = Nothing
            ElseIf String.Compare(.标签二, 标签名称, True) = 0 Then
                .标签二 = Nothing
            End If
        End With
        If 讯友录更新时间 > 0 Then 当前用户.讯友录更新时间 = 讯友录更新时间
        列出讯友当前标签()
    End Sub

    Friend Sub 修改备注成功(ByVal SS包解读器 As 类_SS包解读器)
        SS包解读器.读取_有标签("备注", 聊天控件.聊天对象.讯友或群主.备注)
        Dim 讯友录更新时间 As Long
        SS包解读器.读取_有标签("时间", 讯友录更新时间)
        If 讯友录更新时间 > 0 Then 当前用户.讯友录更新时间 = 讯友录更新时间
        Select Case 当前用户.讯友录当前显示范围
            Case 讯友录显示范围_常量集合.最近, 讯友录显示范围_常量集合.讯友, 讯友录显示范围_常量集合.某标签, 讯友录显示范围_常量集合.黑名单
                主窗体1.刷新讯友录()
        End Select
        说(界面文字.获取(156, "备注已修改为：#%。", New Object() {聊天控件.聊天对象.讯友或群主.备注}))
    End Sub

    Friend Sub 拉黑讯友成功(ByVal SS包解读器 As 类_SS包解读器)
        聊天控件.聊天对象.讯友或群主.拉黑 = True
        Dim 讯友录更新时间 As Long
        SS包解读器.读取_有标签("时间", 讯友录更新时间)
        If 讯友录更新时间 > 0 Then 当前用户.讯友录更新时间 = 讯友录更新时间
        Select Case 当前用户.讯友录当前显示范围
            Case 讯友录显示范围_常量集合.最近, 讯友录显示范围_常量集合.讯友, 讯友录显示范围_常量集合.某标签, 讯友录显示范围_常量集合.黑名单
                主窗体1.刷新讯友录()
        End Select
        说(界面文字.获取(160, "你已将此讯友拉黑。"))
    End Sub

    Private Sub 加入大聊天群成功(ByVal SS包解读器 As 类_SS包解读器)
        Dim 子域名 As String = Nothing
        Dim 群编号, 图标更新时间 As Long
        Dim 群名称 As String = Nothing
        Dim 连接凭据 As String = Nothing
        Dim 角色 As 群角色_常量集合
        Dim 本国语域名 As String = Nothing
        SS包解读器.读取_有标签("子域名", 子域名)
        SS包解读器.读取_有标签("群编号", 群编号)
        SS包解读器.读取_有标签("群名称", 群名称)
        SS包解读器.读取_有标签("图标更新时间", 图标更新时间)
        SS包解读器.读取_有标签("连接凭据", 连接凭据)
        SS包解读器.读取_有标签("角色", 角色)
        SS包解读器.读取_有标签("本国语域名", 本国语域名)
        Dim I As Integer = 子域名.IndexOf(".")
        Dim 大聊天群 As New 类_聊天群_大
        大聊天群.子域名 = 子域名
        大聊天群.编号 = 群编号
        大聊天群.名称 = 群名称
        大聊天群.图标更新时间 = 图标更新时间
        大聊天群.连接凭据 = 连接凭据
        大聊天群.我的角色 = 角色
        大聊天群.本国语域名 = 本国语域名
        大聊天群.主机名 = 子域名.Substring(0, I)
        大聊天群.英语域名 = 子域名.Substring(I + 1)
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
        End If
        加入的大聊天群 = 当前用户.加入的大聊天群
        For I = 0 To 加入的大聊天群.Length - 1
            With 加入的大聊天群(I)
                If String.Compare(.子域名, 子域名) = 0 AndAlso .编号 <> 群编号 Then
                    If String.IsNullOrEmpty(.连接凭据) = False Then .连接凭据 = 连接凭据
                End If
            End With
        Next
        Dim 聊天对象2 As New 类_聊天对象
        聊天对象2.大聊天群 = 大聊天群
        主窗体1.添加聊天控件(聊天对象2)
        数据库_更新最近互动讯友排名(子域名, 群编号)
        主窗体1.刷新讯友录(讯友录显示范围_常量集合.聊天群)
    End Sub

End Class
