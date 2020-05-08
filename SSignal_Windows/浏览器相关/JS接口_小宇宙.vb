Imports System.IO
Imports System.Xml
Imports System.Text
Imports SSignal_Protocols
Imports SSignal_GlobalCommonCode
Imports CefSharp

Public Class JS接口_小宇宙

    Dim 聊天控件 As 控件_聊天

    Private Delegate Sub 跨线程0()
    Private Delegate Sub 跨线程1(ByVal Parameter1 As String)

    Public Sub New(ByVal 聊天控件2 As 控件_聊天)
        聊天控件 = 聊天控件2
    End Sub

    Public Sub RequestReadCredential(ByVal 子域名 As String)
        If 聊天控件.InvokeRequired Then
            Dim d As New 跨线程1(AddressOf RequestReadCredential)
            聊天控件.Invoke(d, New Object() {子域名})
        Else
            当前用户.获取小宇宙凭据(聊天控件, 子域名)
        End If
    End Sub

    Public Sub RequestWriteCredential(ByVal 子域名 As String)
        If 聊天控件.InvokeRequired Then
            Dim d As New 跨线程1(AddressOf RequestWriteCredential)
            聊天控件.Invoke(d, New Object() {子域名})
        Else
            当前用户.获取小宇宙凭据(聊天控件, 子域名, True)
        End If
    End Sub

    Public Sub RequestReadCredential2()
        If 聊天控件.InvokeRequired Then
            Dim d As New 跨线程0(AddressOf RequestReadCredential2)
            聊天控件.Invoke(d, New Object() {})
        Else
            CType(聊天控件.机器人, 类_机器人_大聊天群).请求讯宝中心小宇宙分配读取服务器()
        End If
    End Sub

    Public Sub PlayVideo(ByVal url As String)
        If 聊天控件.InvokeRequired Then
            Dim d As New 跨线程1(AddressOf PlayVideo)
            聊天控件.Invoke(d, New Object() {url})
        Else
            Dim 窗体 As New 窗体_播放视频(url)
            窗体.Show(主窗体)
        End If
    End Sub

    Public Sub ClickAMember(ByVal who As String)
        If 聊天控件.InvokeRequired Then
            Dim d As New 跨线程1(AddressOf ClickAMember)
            聊天控件.Invoke(d, New Object() {who})
        Else
            聊天控件.点击群成员(who)
        End If
    End Sub

    Public Sub RequestMemberList()
        If 聊天控件.InvokeRequired Then
            Dim d As New 跨线程0(AddressOf RequestMemberList)
            聊天控件.Invoke(d, New Object() {})
        Else
            聊天控件.加载小聊天群的成员列表()
        End If
    End Sub

    Public Sub SelectImage(ByVal id As String)
        If 聊天控件.InvokeRequired Then
            Dim d As New 跨线程1(AddressOf SelectImage)
            聊天控件.Invoke(d, New Object() {id})
        Else
            With 聊天控件.主窗体1.文件选取器
                .Multiselect = False
                .Filter = 界面文字.获取(67, "所有图片文件") & "|*.jpg;*.jpeg;*.png;*.gif"
                If .ShowDialog() = DialogResult.OK Then
                    Dim DataURL As String
                    Dim 内存流 As MemoryStream = Nothing
                    Try
                        Dim 原图 As New Bitmap(.FileName)
                        Dim 压缩后图片 As Bitmap = Nothing
                        If 原图.Width > 最大值_常量集合.讯宝预览图片宽高_像素 OrElse 原图.Height > 最大值_常量集合.讯宝预览图片宽高_像素 Then
                            Dim 缩小比例 As Double
                            If 原图.Height > 原图.Width Then
                                缩小比例 = 最大值_常量集合.讯宝预览图片宽高_像素 / 原图.Height
                            Else
                                缩小比例 = 最大值_常量集合.讯宝预览图片宽高_像素 / 原图.Width
                            End If
                            压缩后图片 = New Bitmap(CInt(原图.Width * 缩小比例), CInt(原图.Height * 缩小比例))
                        Else
                            压缩后图片 = New Bitmap(原图.Width, 原图.Height)
                        End If
                        Dim 绘图器 As Graphics = Graphics.FromImage(压缩后图片)
                        绘图器.DrawImage(原图, 0, 0, 压缩后图片.Width, 压缩后图片.Height)
                        绘图器.Dispose()
                        内存流 = New MemoryStream
                        压缩后图片.Save(内存流, Imaging.ImageFormat.Jpeg)
                        压缩后图片.Dispose()
                        原图.Dispose()
                        DataURL = "data:image/jpg;base64," & Convert.ToBase64String(内存流.ToArray)
                        内存流.Close()
                    Catch ex As Exception
                        If 内存流 IsNot Nothing Then 内存流.Close()
                        聊天控件.机器人.说(ex.Message)
                        Return
                    End Try
                    If String.IsNullOrEmpty(id) = True Then
                        聊天控件.浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("InsertImage('" & 处理文件路径以用作JS函数参数(.FileName) & "', '" + DataURL + "');")
                    Else
                        聊天控件.浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("ReviseImage('" + id + "', '" & 处理文件路径以用作JS函数参数(.FileName) & "', '" + DataURL + "');")
                    End If
                End If
            End With
        End If
    End Sub

    Public Sub SelectVideo()
        If 聊天控件.InvokeRequired Then
            Dim d As New 跨线程0(AddressOf SelectVideo)
            聊天控件.Invoke(d, New Object() {})
        Else
            With 聊天控件.主窗体1.文件选取器
                .Multiselect = False
                .Filter = "*.mp4|*.mp4"
                If .ShowDialog() = DialogResult.OK Then
                    聊天控件.浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("InsertVideo('" & 处理文件路径以用作JS函数参数(.FileName) & "');")
                End If
            End With
        End If
    End Sub

    Public Sub SelectVideoPreview()
        If 聊天控件.InvokeRequired Then
            Dim d As New 跨线程0(AddressOf SelectVideoPreview)
            聊天控件.Invoke(d, New Object() {})
        Else
            With 聊天控件.主窗体1.文件选取器
                .Multiselect = False
                .Filter = 界面文字.获取(67, "所有图片文件") & "|*.jpg;*.jpeg;*.png;*.gif"
                If .ShowDialog() = DialogResult.OK Then
                    Dim DataURL As String
                    Dim 内存流 As MemoryStream = Nothing
                    Try
                        Dim 原图 As New Bitmap(.FileName)
                        Dim 压缩后图片 As Bitmap = Nothing
                        If 原图.Width > 最大值_常量集合.讯宝预览图片宽高_像素 OrElse 原图.Height > 最大值_常量集合.讯宝预览图片宽高_像素 Then
                            Dim 缩小比例 As Double
                            If 原图.Height > 原图.Width Then
                                缩小比例 = 最大值_常量集合.讯宝预览图片宽高_像素 / 原图.Height
                            Else
                                缩小比例 = 最大值_常量集合.讯宝预览图片宽高_像素 / 原图.Width
                            End If
                            压缩后图片 = New Bitmap(CInt(原图.Width * 缩小比例), CInt(原图.Height * 缩小比例))
                        Else
                            压缩后图片 = New Bitmap(原图.Width, 原图.Height)
                        End If
                        Dim 绘图器 As Graphics = Graphics.FromImage(压缩后图片)
                        绘图器.DrawImage(原图, 0, 0, 压缩后图片.Width, 压缩后图片.Height)
                        绘图器.Dispose()
                        内存流 = New MemoryStream
                        压缩后图片.Save(内存流, Imaging.ImageFormat.Jpeg)
                        压缩后图片.Dispose()
                        原图.Dispose()
                        DataURL = "data:image/jpg;base64," & Convert.ToBase64String(内存流.ToArray)
                        内存流.Close()
                    Catch ex As Exception
                        If 内存流 IsNot Nothing Then 内存流.Close()
                        聊天控件.机器人.说(ex.Message)
                        Return
                    End Try
                    聊天控件.浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("InsertVideoPreview('" & 处理文件路径以用作JS函数参数(.FileName) & "', '" + DataURL + "');")
                End If
            End With
        End If
    End Sub

    Public Sub GetTags(ByVal id As String)
        If 聊天控件.InvokeRequired Then
            Dim d As New 跨线程1(AddressOf GetTags)
            聊天控件.Invoke(d, New Object() {id})
        Else
            If 当前用户.讯友目录 Is Nothing Then Return
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
                Dim 变长文本 As New StringBuilder(讯友标签数 * (最大值_常量集合.讯友标签字符数 + 17))
                Dim 文本写入器 As New StringWriter(变长文本)
                For I = 0 To 讯友标签数 - 1
                    文本写入器.Write("<option>" & 替换HTML和JS敏感字符(讯友标签(I)) & "</option>")
                Next
                文本写入器.Close()
                聊天控件.浏览器_小宇宙.GetMainFrame.ExecuteJavaScriptAsync("SSPalTags('" & id & "', '" + 文本写入器.ToString + "');")
            End If
        End If
    End Sub

    Public Sub PublishMeteorRain(ByVal XML As String)
        If 聊天控件.InvokeRequired Then
            Dim d As New 跨线程1(AddressOf PublishMeteorRain)
            聊天控件.Invoke(d, New Object() {XML})
        Else
            Dim SS包生成器 As New 类_SS包生成器
            Try
                Dim XML文档 As New XmlDocument
                XML文档.LoadXml(XML)
                Dim 节点 As XmlNode = XML文档.SelectSingleNode("/MeteorRain")
                Dim 类型 As 流星语类型_常量集合 = Byte.Parse(节点.SelectSingleNode("Type").InnerText)
                SS包生成器.添加_有标签("类型", 类型)
                Dim 标题 As String = 节点.SelectSingleNode("Title").InnerText
                SS包生成器.添加_有标签("标题", 标题)
                Dim 子节点 As XmlNode = 节点.SelectSingleNode("Permission")
                If 子节点 IsNot Nothing Then
                    Dim 访问权限 As 流星语访问权限_常量集合 = Byte.Parse(子节点.InnerText)
                    SS包生成器.添加_有标签("访问权限", 访问权限)
                    If 访问权限 = 流星语访问权限_常量集合.某标签讯友 Then
                        Dim 讯友标签 As String = 节点.SelectSingleNode("Tag").InnerText
                        SS包生成器.添加_有标签("讯友标签", 讯友标签)
                    End If
                End If
                子节点 = 节点.SelectSingleNode("Domain_Read")
                If 子节点 IsNot Nothing Then
                    Dim 子域名_读取 As String = 子节点.InnerText
                    SS包生成器.添加_有标签("域名_读取", 子域名_读取)
                End If
                Dim 样式 As 流星语列表项样式_常量集合 = Byte.Parse(节点.SelectSingleNode("Style").InnerText)
                SS包生成器.添加_有标签("样式", 样式)
                Select Case 类型
                    Case 流星语类型_常量集合.图文
                        节点 = 节点.SelectSingleNode("Body")
                        Dim 节点列表 As XmlNodeList = 节点.ChildNodes
                        Dim I As Integer
                        For I = 0 To 节点列表.Count - 1
                            节点 = 节点列表(I)
                            Select Case 节点.Name
                                Case "Text"
                                    Dim SS包生成器2 As New 类_SS包生成器
                                    SS包生成器2.添加_有标签("是图片", False)
                                    SS包生成器2.添加_有标签("文本", 节点.InnerText)
                                    SS包生成器.添加_有标签("段落", SS包生成器2)
                                Case "Image"
                                    Dim SS包生成器2 As New 类_SS包生成器
                                    SS包生成器2.添加_有标签("是图片", True)
                                    Dim 路径 As String = 节点.InnerText
                                    SS包生成器2.添加_有标签("扩展名", Path.GetExtension(路径).Replace(".", ""))
                                    SS包生成器2.添加_有标签("图片数据", File.ReadAllBytes(路径))
                                    SS包生成器.添加_有标签("段落", SS包生成器2)
                            End Select
                        Next
                    Case 流星语类型_常量集合.视频
                        子节点 = 节点.SelectSingleNode("Video")
                        SS包生成器.添加_有标签("视频数据", File.ReadAllBytes(子节点.InnerText))
                        子节点 = 节点.SelectSingleNode("Image")
                        SS包生成器.添加_有标签("预览图片", File.ReadAllBytes(子节点.InnerText))
                    Case Else
                        If TypeOf 聊天控件.机器人 Is 类_机器人_主控 Then
                            CType(聊天控件.机器人, 类_机器人_主控).流星语发布结束(False)
                        ElseIf TypeOf 聊天控件.机器人 Is 类_机器人_大聊天群 Then
                            CType(聊天控件.机器人, 类_机器人_大聊天群).流星语发布结束(False)
                        End If
                        Return
                End Select
            Catch ex As Exception
                If TypeOf 聊天控件.机器人 Is 类_机器人_主控 Then
                    With CType(聊天控件.机器人, 类_机器人_主控)
                        .说(ex.Message)
                        .流星语发布结束(False)
                    End With
                ElseIf TypeOf 聊天控件.机器人 Is 类_机器人_大聊天群 Then
                    With CType(聊天控件.机器人, 类_机器人_大聊天群)
                        .说(ex.Message)
                        .流星语发布结束(False)
                    End With
                End If
                Return
            End Try
            Dim SS包() As Byte = SS包生成器.生成SS包
            Const 最大兆数 As Integer = 最大值_常量集合.小宇宙文件数据长度_兆
            If SS包.Length > 最大兆数 * 1024 * 1024 Then
                If TypeOf 聊天控件.机器人 Is 类_机器人_主控 Then
                    With CType(聊天控件.机器人, 类_机器人_主控)
                        .说(界面文字.获取(96, "总数据量不可以超过#%MB。", New Object() {最大兆数}))
                        .流星语发布结束(False)
                    End With
                ElseIf TypeOf 聊天控件.机器人 Is 类_机器人_大聊天群 Then
                    With CType(聊天控件.机器人, 类_机器人_大聊天群)
                        .说(界面文字.获取(96, "总数据量不可以超过#%MB。", New Object() {最大兆数}))
                        .流星语发布结束(False)
                    End With
                End If
                Return
            End If
            If TypeOf 聊天控件.机器人 Is 类_机器人_主控 Then
                CType(聊天控件.机器人, 类_机器人_主控).发布流星语(SS包)
            ElseIf TypeOf 聊天控件.机器人 Is 类_机器人_大聊天群 Then
                CType(聊天控件.机器人, 类_机器人_大聊天群).发布流星语(SS包)
            End If
        End If
    End Sub

    Public Sub PublishGoods(ByVal XML As String)
        If 聊天控件.InvokeRequired Then
            Dim d As New 跨线程1(AddressOf PublishGoods)
            聊天控件.Invoke(d, New Object() {XML})
        Else
            Dim SS包生成器 As New 类_SS包生成器
            Try
                Dim XML文档 As New XmlDocument
                XML文档.LoadXml(XML)
                Dim 节点 As XmlNode = XML文档.SelectSingleNode("/Goods")
                Dim 标题 As String = 节点.SelectSingleNode("Title").InnerText
                SS包生成器.添加_有标签("标题", 标题)
                Dim 子域名_读取 As String = 节点.SelectSingleNode("Domain_Read").InnerText
                SS包生成器.添加_有标签("域名_读取", 子域名_读取)
                Dim 样式 As 流星语列表项样式_常量集合 = Byte.Parse(节点.SelectSingleNode("Style").InnerText)
                SS包生成器.添加_有标签("样式", 样式)
                Dim 价格 As Double
                Double.TryParse(节点.SelectSingleNode("Price").InnerText, 价格)
                SS包生成器.添加_有标签("价格", 价格)
                Dim 币种 As String = 节点.SelectSingleNode("Currency").InnerText
                SS包生成器.添加_有标签("币种", 币种)
                Dim 购买链接 As String = 节点.SelectSingleNode("Buy").InnerText
                SS包生成器.添加_有标签("购买链接", 购买链接)
                节点 = 节点.SelectSingleNode("Body")
                Dim 节点列表 As XmlNodeList = 节点.ChildNodes
                Dim I As Integer
                For I = 0 To 节点列表.Count - 1
                    节点 = 节点列表(I)
                    Select Case 节点.Name
                        Case "Text"
                            Dim SS包生成器2 As New 类_SS包生成器
                            SS包生成器2.添加_有标签("是图片", False)
                            SS包生成器2.添加_有标签("文本", 节点.InnerText)
                            SS包生成器.添加_有标签("段落", SS包生成器2)
                        Case "Image"
                            Dim SS包生成器2 As New 类_SS包生成器
                            SS包生成器2.添加_有标签("是图片", True)
                            Dim 路径 As String = 节点.InnerText
                            SS包生成器2.添加_有标签("扩展名", Path.GetExtension(路径).Replace(".", ""))
                            SS包生成器2.添加_有标签("图片数据", File.ReadAllBytes(路径))
                            SS包生成器.添加_有标签("段落", SS包生成器2)
                    End Select
                Next
            Catch ex As Exception
                With CType(聊天控件.机器人, 类_机器人_主控)
                    .说(ex.Message)
                    .商品发布结束(False)
                End With
                Return
            End Try
            Dim SS包() As Byte = SS包生成器.生成SS包
            Const 最大兆数 As Integer = 最大值_常量集合.小宇宙文件数据长度_兆
            If SS包.Length > 最大兆数 * 1024 * 1024 Then
                With CType(聊天控件.机器人, 类_机器人_主控)
                    .说(界面文字.获取(96, "总数据量不可以超过#%MB。", New Object() {最大兆数}))
                    .商品发布结束(False)
                End With
                Return
            End If
            CType(聊天控件.机器人, 类_机器人_主控).发布商品(SS包)
        End If
    End Sub

    Public Sub ServerInfo(ByVal info As String)
        If 聊天控件.InvokeRequired Then
            Dim d As New 跨线程1(AddressOf ServerInfo)
            聊天控件.Invoke(d, New Object() {info})
        Else
            聊天控件.机器人.说(info)
        End If
    End Sub

    Public Sub AdminLogin(ByVal Passcode As String)
        If 聊天控件.InvokeRequired Then
            Dim d As New 跨线程1(AddressOf AdminLogin)
            聊天控件.Invoke(d, New Object() {Passcode})
        Else
            当前用户.凭据_管理员 = Passcode
        End If
    End Sub

End Class
