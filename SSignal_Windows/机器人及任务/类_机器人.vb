Imports System.Net
Imports System.Threading
Imports System.IO
Imports System.Text
Imports CefSharp

Friend MustInherit Class 类_机器人

    Friend 主窗体1 As 主窗体
    Friend 聊天控件 As 控件_聊天
    Friend 任务 As 类_任务
    Protected 线程_HTTPS访问 As Thread

    Protected Delegate Sub HTTPS请求成功_跨线程(ByVal SS包() As Byte)
    Protected Delegate Sub HTTPS请求失败_跨线程(ByVal 原因 As String, ByVal 结束 As Boolean)

    Friend Sub 说(ByVal 文本 As String, Optional ByVal 时刻 As Long = 0)
        If String.IsNullOrEmpty(文本) OrElse 聊天控件 Is Nothing Then Return
        If 文本.Contains("<a>") Then
            Dim 段() As String = 文本.Split(New String() {"<a>"}, StringSplitOptions.RemoveEmptyEntries)
            Dim 变长文本 As New StringBuilder(文本.Length * 2)
            Dim 文本写入器 As New StringWriter(变长文本)
            Dim I As Integer
            For I = 0 To 段.Length - 1
                If 段(I).Contains("</a>") AndAlso 段(I).Contains("<a ") = False Then
                    Dim 节() As String = 段(I).Split(New String() {"</a>"}, StringSplitOptions.RemoveEmptyEntries)
                    If 节.Length = 1 Then
                        文本写入器.Write("<span class='TaskName' onclick='ToRobot(\""" & 节(0) & "\"")'>" & 节(0) & "</span>")
                    ElseIf 节.Length = 2 Then
                        文本写入器.Write("<span class='TaskName' onclick='ToRobot(\""" & 节(0) & "\"")'>" & 节(0) & "</span>")
                        文本写入器.Write(节(1))
                    Else
                        文本写入器.Write(段(I))
                    End If
                Else
                    文本写入器.Write(段(I))
                End If
            Next
            文本 = 文本写入器.ToString
            文本写入器.Close()
        End If
        Dim 谁 As String
        If 聊天控件.聊天对象.小聊天群 IsNot Nothing Then
            Select Case 聊天控件.聊天对象.讯友或群主.英语讯宝地址
                Case 机器人id_主控, 机器人id_系统管理 : 谁 = 聊天控件.聊天对象.讯友或群主.英语讯宝地址
                Case Else : 谁 = 机器人id_主控
            End Select
        Else
            谁 = 机器人id_主控
        End If
        Try
            聊天控件.浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("SSin_Text('" + 谁 + "', '0', """ + 文本 + """, '" + 谁 & ".jpg" + "', '" + 聊天控件.时间格式(Date.FromBinary(IIf(时刻 = 0, Date.UtcNow.Ticks, 时刻))) + "');")
        Catch ex As Exception
        End Try
        聊天控件.输入框.Focus()
    End Sub

    Friend MustOverride Sub 回答(ByVal 用户输入 As String, ByVal 时间 As Long)

    Friend Sub 启动HTTPS访问线程(ByVal 访问设置 As 类_访问设置)
        聊天控件.下拉列表_任务.Enabled = False
        聊天控件.按钮_说话.Enabled = False
        线程_HTTPS访问 = New Thread(New ParameterizedThreadStart(AddressOf HTTPS访问))
        线程_HTTPS访问.Start(访问设置)
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
                Call HTTPS请求失败(ex.Message, False)
                重试次数 += 1
                GoTo 重试
            Else
                Call HTTPS请求失败(ex.Message, True)
                Return
            End If
        End Try
        If 收到的字节数组 IsNot Nothing Then
            If 收到的总字节数 = 收到的字节数组.Length Then
                Call HTTPS请求成功(收到的字节数组)
                Return
            End If
        End If
        Call HTTPS请求成功(Nothing)
    End Sub

    Protected Overridable Sub HTTPS请求成功(ByVal SS包() As Byte)

    End Sub

    Friend Sub 提示新任务()
        Try
            聊天控件.浏览器_聊天内容.GetMainFrame.ExecuteJavaScriptAsync("NewTask();")
        Catch ex As Exception
        End Try
    End Sub

    Protected Sub HTTPS请求失败(ByVal 原因 As String, ByVal 结束 As Boolean)
        If 聊天控件.InvokeRequired Then
            Dim d As New HTTPS请求失败_跨线程(AddressOf HTTPS请求失败)
            聊天控件.Invoke(d, New Object() {原因, 结束})
        Else
            If 结束 = False Then
                说(界面文字.获取(12, "#% 正在重试", New Object() {原因}))
            Else
                聊天控件.下拉列表_任务.Enabled = True
                聊天控件.按钮_说话.Enabled = True
                If 任务 IsNot Nothing Then
                    Select Case 任务.名称
                        Case 任务名称_发流星语
                            If TypeOf Me Is 类_机器人_主控 Then
                                CType(Me, 类_机器人_主控).流星语发布结束(False)
                            ElseIf TypeOf Me Is 类_机器人_大聊天群 Then
                                CType(Me, 类_机器人_大聊天群).流星语发布结束(False)
                            End If
                        Case 任务名称_发布商品
                            CType(Me, 类_机器人_主控).商品发布结束(False)
                        Case 任务名称_注销
                            主窗体1.注销成功()
                    End Select
                    任务.结束()
                    任务 = Nothing
                End If
                说(原因)
            End If
        End If
    End Sub

End Class
