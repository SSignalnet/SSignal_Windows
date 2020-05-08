Imports SSignal_Protocols
Imports SSignal_GlobalCommonCode

Friend Class 类_访问设置

    Friend 路径 As String
    Friend 字节数组() As Byte
    Friend 收发时限_毫秒 As Integer
    Friend 大聊天群服务器 As 类_大聊天群服务器

    Friend Sub New(ByVal 路径1 As String, Optional ByVal 收发时限_毫秒1 As Integer = 收发时限, Optional ByVal 字节数组1() As Byte = Nothing,
                   Optional ByVal 大聊天群服务器1 As 类_大聊天群服务器 = Nothing)
        路径 = 路径1
        If 收发时限_毫秒1 < 收发时限 Then
            收发时限_毫秒 = 收发时限
        Else
            收发时限_毫秒 = 收发时限_毫秒1
        End If
        字节数组 = 字节数组1
        大聊天群服务器 = 大聊天群服务器1
    End Sub

End Class
