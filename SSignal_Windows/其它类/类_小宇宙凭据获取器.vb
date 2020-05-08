Imports System.Net
Imports System.Threading
Imports System.IO
Imports SSignal_Protocols

Friend Class 类_小宇宙凭据获取器

    Private 线程_HTTPS访问 As Thread

    Private Delegate Sub HTTPS请求结束_跨线程(ByVal 字节数组() As Byte)

    Dim 聊天控件() As 控件_聊天
    Friend 英语子域名 As String
    Dim 是写入凭据 As Boolean
    Dim 字节数组() As Byte

    Friend Sub New(ByVal 聊天控件1 As 控件_聊天, ByVal 英语子域名1 As String, Optional ByVal 是写入凭据1 As Boolean = False)
        ReDim 聊天控件(0)
        聊天控件(0) = 聊天控件1
        英语子域名 = 英语子域名1
        是写入凭据 = 是写入凭据1
        Dim SS包生成器 As New 类_SS包生成器
        SS包生成器.添加_有标签("发送序号", 当前用户.讯宝发送序号)
        SS包生成器.添加_有标签("子域名", 英语子域名)
        字节数组 = SS包生成器.生成SS包(当前用户.AES加密器)
    End Sub

    Friend Sub 获取()
        线程_HTTPS访问 = New Thread(New ThreadStart(AddressOf HTTPS访问))
        线程_HTTPS访问.Start()
    End Sub

    Private Sub HTTPS访问()
        Dim 重试次数 As Integer
        Dim 收到的字节数组() As Byte
        Dim 收到的字节数, 收到的总字节数 As Integer
重试:
        收到的总字节数 = 0
        收到的字节数组 = Nothing
        Try
            Dim HTTP网络请求 As HttpWebRequest = WebRequest.Create(获取传送服务器访问路径开头(当前用户.主机名, 当前用户.域名_英语, False) & "C=EnterTinyUniverse&UserID=" & 当前用户.编号 & "&Position=" & 当前用户.位置号 & "&DeviceType=" & 设备类型_电脑)
            HTTP网络请求.Method = "POST"
            HTTP网络请求.Timeout = 20000
            HTTP网络请求.ContentType = "application/octet-stream"
            HTTP网络请求.ContentLength = 字节数组.Length
            Dim 流 As Stream = HTTP网络请求.GetRequestStream
            流.Write(字节数组, 0, 字节数组.Length)
            流.Close()
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
                    If 收到的总字节数 < 收到的字节数组.Length Then 收到的字节数组 = Nothing
                End If
            End Using
        Catch ex As Exception
            If 重试次数 < 2 Then
                重试次数 += 1
                GoTo 重试
            End If
        End Try
        Call HTTPS请求结束(收到的字节数组)
    End Sub

    Private Sub HTTPS请求结束(ByVal 收到的字节数组() As Byte)
        If 聊天控件(0).InvokeRequired Then
            Dim d As New HTTPS请求结束_跨线程(AddressOf HTTPS请求结束)
            聊天控件(0).Invoke(d, New Object() {收到的字节数组})
        Else
            当前用户.获取小宇宙凭据结束(Me, 收到的字节数组, 聊天控件, 是写入凭据)
        End If
    End Sub

    Friend Sub 添加聊天控件(ByVal 聊天控件1 As 控件_聊天)
        Dim I As Integer
        For I = 0 To 聊天控件.Length - 1
            If 聊天控件(I).Equals(聊天控件1) Then Return
        Next
        ReDim Preserve 聊天控件(聊天控件.Length)
        聊天控件(聊天控件.Length - 1) = 聊天控件1
    End Sub

    Friend Function 查找聊天控件(ByVal 聊天控件1 As 控件_聊天) As Boolean
        Dim I As Integer
        For I = 0 To 聊天控件.Length - 1
            If 聊天控件(I).Equals(聊天控件1) Then Return True
        Next
        Return False
    End Function

End Class
