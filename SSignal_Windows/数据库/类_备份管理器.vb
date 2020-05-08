Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Net
Imports SSignal_Protocols
Imports SSignalDB

Friend Class 类_备份管理器

    Dim 主窗体1 As New 主窗体
    Friend 数据库_备份凭据 As 类_数据库
    WithEvents 备份器_中心数据库 As 类_备份器
    Dim 备份器_其它服务器() As 类_备份器
    Dim 线程_HTTPS访问 As Thread

    Private Delegate Sub 显示异常信息_跨线程(ByVal 来自 As Object, ByVal 文本 As String)

    Friend Sub New(ByVal 主窗体2 As 主窗体)
        主窗体1 = 主窗体2
    End Sub

    Friend Function 开始() As Boolean
        Dim 类 As New 类_打开或创建数据库
        数据库_备份凭据 = 类.打开或创建备份凭据数据库
        If 数据库_备份凭据 Is Nothing Then
            显示异常信息(Me, 界面文字.获取(210, "无法打开存放备份凭据的数据库"))
            Return False
        End If
        If 备份器_中心数据库 Is Nothing Then 备份器_中心数据库 = New 类_备份器(Me, 讯宝中心服务器主机名 & "." & 当前用户.域名_英语, True)
        If 备份器_中心数据库.开始() = True Then
            If 备份器_其它服务器 Is Nothing Then
                线程_HTTPS访问 = New Thread(New ThreadStart(AddressOf HTTPS访问))
                线程_HTTPS访问.Start()
            Else
                Dim I As Integer
                For I = 0 To 备份器_其它服务器.Length - 1
                    备份器_其它服务器(I).开始()
                Next
            End If
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub HTTPS访问()
        Dim 重试次数 As Integer
        Dim 收到的字节数组() As Byte
        Dim 收到的字节数, 收到的总字节数 As Integer
重试:
        收到的总字节数 = 0
        收到的字节数组 = Nothing
        Try
            Dim HTTP网络请求 As HttpWebRequest = WebRequest.Create(获取中心服务器访问路径开头(当前用户.域名_英语) & "C=GetServerList&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器))
            HTTP网络请求.Method = "POST"
            HTTP网络请求.Timeout = 20000
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
                Call HTTPS请求失败(ex.Message)
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

    Private Sub HTTPS请求成功(ByVal SS包() As Byte)
        线程_HTTPS访问 = Nothing
        If SS包 IsNot Nothing Then
            Try
                Dim SS包解读器 As New 类_SS包解读器(SS包)
                Select Case SS包解读器.查询结果
                    Case 查询结果_常量集合.成功
                        Dim SS包解读器2 As 类_SS包解读器 = Nothing
                        SS包解读器.读取_有标签("服务器", SS包解读器2)
                        If SS包解读器2 IsNot Nothing Then
                            Dim 主机名() As Object = SS包解读器2.读取_重复标签("主机名")
                            If 主机名 IsNot Nothing Then
                                ReDim 备份器_其它服务器(主机名.Length - 1)
                                Dim I As Integer
                                For I = 0 To 主机名.Length - 1
                                    Dim 备份器 As New 类_备份器(Me, 主机名(I) & "." & 当前用户.域名_英语)
                                    备份器_其它服务器(I) = 备份器
                                    AddHandler 备份器.显示异常信息, AddressOf 显示异常信息
                                    备份器.开始()
                                Next
                            End If
                        End If
                    Case 查询结果_常量集合.失败
                        显示异常信息(Me, 界面文字.获取(148, "由于未知原因，操作失败。"))
                    Case Else
                        显示异常信息(Me, 界面文字.获取(108, "出错 #%", New Object() {SS包解读器.查询结果}))
                End Select
            Catch ex As Exception
                显示异常信息(Me, ex.Message)
            End Try
        End If
    End Sub

    Private Sub HTTPS请求失败(ByVal 原因 As String)
        Thread.Sleep(300000)
        HTTPS访问()
    End Sub

    Friend Function 获取当前备份状态() As String
        Dim 变长文本 As StringBuilder
        If 备份器_其它服务器 IsNot Nothing Then
            变长文本 = New StringBuilder(200 + 50 * 备份器_其它服务器.Length)
        Else
            变长文本 = New StringBuilder(200)
        End If
        Dim 文本写入器 As New StringWriter(变长文本)
        文本写入器.Write(界面文字.获取(207, "存放路径为：#%", New Object() {备份文件存放路径.Replace("\", "\\")}) & "<br>")
        If 备份器_中心数据库 IsNot Nothing Then
            文本写入器.Write("<br>")
            文本写入器.Write(备份器_中心数据库.状态信息.Replace("\", "\\"))
        End If
        If 备份器_其它服务器 IsNot Nothing Then
            Dim I As Integer
            For I = 0 To 备份器_其它服务器.Length - 1
                文本写入器.Write("<br>")
                文本写入器.Write(备份器_其它服务器(I).状态信息.Replace("\", "\\"))
            Next
        End If
        文本写入器.Write("<br><br>" & 界面文字.获取(209, "（提示：备份的数据库与其源数据库的字节在第8192字节之前和第40959字节之后是完全一致的。）"))
        文本写入器.Close()
        Return 文本写入器.ToString
    End Function

    Friend Sub 停止()
        If 线程_HTTPS访问 IsNot Nothing Then
            Try
                线程_HTTPS访问.Abort()
                线程_HTTPS访问 = Nothing
            Catch ex As Exception
            End Try
        End If
        If 备份器_中心数据库 IsNot Nothing Then
            备份器_中心数据库.停止()
        End If
        If 备份器_其它服务器 IsNot Nothing Then
            Dim I As Integer
            For I = 0 To 备份器_其它服务器.Length - 1
                备份器_其它服务器(I).停止()
            Next
        End If
        If 数据库_备份凭据 IsNot Nothing Then
            数据库_备份凭据.关闭()
            数据库_备份凭据 = Nothing
        End If
    End Sub

    Private Sub 显示异常信息(来自 As Object, 文本 As String) Handles 备份器_中心数据库.显示异常信息
        If 主窗体1.InvokeRequired Then
            Dim d As New 显示异常信息_跨线程(AddressOf 显示异常信息)
            主窗体1.Invoke(d, New Object() {来自, 文本})
        Else
            主窗体1.备份时出现故障(文本.Replace("\", "\\"))
        End If
    End Sub

End Class
