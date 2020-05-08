Imports System.Security.Cryptography
Imports SSignal_GlobalCommonCode
Imports SSignal_Protocols

Friend Class 类_用户

    Private Structure 小宇宙凭据_复合数据
        Dim 英语子域名, 连接凭据 As String
        Dim 是商品编辑 As Boolean
    End Structure

    Friend 域名_英语, 域名_本国语 As String

    Friend 英语用户名, 本国语用户名, 职能, 电子邮箱地址 As String
    Friend 凭据_中心服务器, 凭据_管理员 As String
    Friend 编号, 头像更新时间, 密钥创建时间 As Long
    Friend 讯友录更新时间 As Long = -1
    Friend 主机名, 传送服务器验证码 As String
    Friend 位置号 As Short = -1
    Friend AES加解密模块 As RijndaelManaged
    Friend AES加密器, AES解密器 As ICryptoTransform

    Friend 获取了密钥, 获取了账户信息 As Boolean
    Friend 讯友目录() As 类_讯友
    Friend 讯友录当前显示范围 As 讯友录显示范围_常量集合
    Friend 讯友录当前显示标签 As String
    Friend 加入的小聊天群() As 类_聊天群_小
    Friend 加入的大聊天群() As 类_聊天群_大

    Friend 讯宝发送序号 As Long

    Friend 白域(), 黑域() As 域名_复合数据

    Dim 小宇宙读取凭据() As 小宇宙凭据_复合数据
    Dim 小宇宙凭据获取器() As 类_小宇宙凭据获取器
    Friend 子域名_小宇宙写入, 小宇宙写入凭据 As String

    Friend 主控机器人 As 类_机器人_主控

    Friend ReadOnly Property 英语讯宝地址 As String
        Get
            Return 英语用户名 & 讯宝地址标识 & 域名_英语
        End Get
    End Property

    Friend ReadOnly Property 本国语讯宝地址 As String
        Get
            Return 本国语用户名 & 讯宝地址标识 & 域名_本国语
        End Get
    End Property

    Friend Function 查找讯友(ByVal 讯宝地址 As String) As 类_讯友
        If 讯友目录 IsNot Nothing Then
            Dim I As Integer
            For I = 0 To 讯友目录.Length - 1
                If String.Compare(讯友目录(I).英语讯宝地址, 讯宝地址) = 0 OrElse String.Compare(讯友目录(I).本国语讯宝地址, 讯宝地址) = 0 Then
                    Return 讯友目录(I)
                End If
            Next
        End If
        Return Nothing
    End Function

    Friend Function 查找小聊天群(ByVal 讯宝地址 As String, ByVal 群编号 As Byte) As 类_聊天群_小
        If 加入的小聊天群 IsNot Nothing Then
            Dim I As Integer
            For I = 0 To 加入的小聊天群.Length - 1
                If String.Compare(加入的小聊天群(I).群主.英语讯宝地址, 讯宝地址) = 0 AndAlso 加入的小聊天群(I).编号 = 群编号 Then
                    Return 加入的小聊天群(I)
                End If
            Next
        End If
        Return Nothing
    End Function

    Friend Function 查找大聊天群(ByVal 子域名 As String, ByVal 群编号 As Long) As 类_聊天群_大
        If 加入的大聊天群 IsNot Nothing Then
            Dim I As Integer
            For I = 0 To 加入的大聊天群.Length - 1
                If String.Compare(加入的大聊天群(I).子域名, 子域名) = 0 AndAlso 加入的大聊天群(I).编号 = 群编号 Then
                    Return 加入的大聊天群(I)
                End If
            Next
        End If
        Return Nothing
    End Function

    Friend Function 已登录() As Boolean
        If 编号 > 0 AndAlso String.IsNullOrEmpty(凭据_中心服务器) = False AndAlso 检查英语域名(域名_英语) = True Then
            Return True
        Else
            Return False
        End If
    End Function

    Friend Sub 获取小宇宙凭据(ByVal 聊天控件 As 控件_聊天, ByVal 英语子域名 As String, Optional ByVal 是写入凭据 As Boolean = False)
        If 是写入凭据 = True Then
            If String.IsNullOrEmpty(小宇宙写入凭据) = False AndAlso String.Compare(英语子域名, 子域名_小宇宙写入) = 0 Then
                聊天控件.收到小宇宙的连接凭据(英语子域名, 小宇宙写入凭据, False, 是写入凭据)
                Return
            End If
        End If
        If 小宇宙读取凭据 IsNot Nothing Then
            Dim I As Integer
            For I = 0 To 小宇宙读取凭据.Length - 1
                If String.Compare(小宇宙读取凭据(I).英语子域名, 英语子域名) = 0 Then Exit For
            Next
            If I < 小宇宙读取凭据.Length Then
                Dim 小宇宙凭据2 As 小宇宙凭据_复合数据 = 小宇宙读取凭据(I)
                If I > 0 Then
                    For I = I To 1 Step -1
                        小宇宙读取凭据(I) = 小宇宙读取凭据(I - 1)
                    Next
                    小宇宙读取凭据(0) = 小宇宙凭据2
                End If
                If 是写入凭据 = True Then
                    子域名_小宇宙写入 = 英语子域名
                    小宇宙写入凭据 = 小宇宙凭据2.连接凭据
                End If
                聊天控件.收到小宇宙的连接凭据(英语子域名, 小宇宙凭据2.连接凭据, 小宇宙凭据2.是商品编辑, 是写入凭据)
                Return
            End If
        End If
        If 小宇宙凭据获取器 IsNot Nothing Then
            Dim I As Integer
            For I = 0 To 小宇宙凭据获取器.Length - 1
                If String.Compare(小宇宙凭据获取器(I).英语子域名, 英语子域名) = 0 Then Exit For
            Next
            If I < 小宇宙凭据获取器.Length Then
                小宇宙凭据获取器(I).添加聊天控件(聊天控件)
            Else
                Dim 获取器 As New 类_小宇宙凭据获取器(聊天控件, 英语子域名, 是写入凭据)
                ReDim Preserve 小宇宙凭据获取器(小宇宙凭据获取器.Length)
                小宇宙凭据获取器(小宇宙凭据获取器.Length - 1) = 获取器
                获取器.获取()
            End If
        Else
            Dim 获取器 As New 类_小宇宙凭据获取器(聊天控件, 英语子域名, 是写入凭据)
            ReDim 小宇宙凭据获取器(0)
            小宇宙凭据获取器(0) = 获取器
            获取器.获取()
        End If
    End Sub

    'Friend Function 是否正在获取小宇宙凭据(ByVal 聊天控件 As 控件_聊天) As Boolean
    '    If 小宇宙凭据获取器 Is Nothing Then Return False
    '    Dim I As Integer
    '    For I = 0 To 小宇宙凭据获取器.Length - 1
    '        If 小宇宙凭据获取器(I).查找聊天控件(聊天控件) = True Then Return True
    '    Next
    '    Return False
    'End Function

    Friend Sub 获取小宇宙凭据结束(ByVal 获取器 As 类_小宇宙凭据获取器, ByVal 字节数组() As Byte, ByVal 聊天控件() As 控件_聊天, ByVal 是写入凭据 As Boolean)
        If 小宇宙凭据获取器 IsNot Nothing Then
            Dim I As Integer
            For I = 0 To 小宇宙凭据获取器.Length - 1
                If 小宇宙凭据获取器(I).Equals(获取器) Then Exit For
            Next
            If I < 小宇宙凭据获取器.Length Then
                If 小宇宙凭据获取器.Length > 1 Then
                    Dim 小宇宙凭据获取器2(小宇宙凭据获取器.Length - 2) As 类_小宇宙凭据获取器
                    Dim J, K As Integer
                    For J = 0 To 小宇宙凭据获取器.Length - 1
                        If J <> I Then
                            小宇宙凭据获取器2(K) = 小宇宙凭据获取器(J)
                            K += 1
                        End If
                    Next
                    小宇宙凭据获取器 = 小宇宙凭据获取器2
                Else
                    小宇宙凭据获取器 = Nothing
                End If
            End If
        End If
        If 字节数组 IsNot Nothing Then
            Try
                Dim SS包解读器 As New 类_SS包解读器(字节数组)
                If SS包解读器.查询结果 = 查询结果_常量集合.成功 Then
                    Dim 子域名 As String = Nothing
                    Dim 连接凭据 As String = Nothing
                    Dim 是商品编辑 As Boolean
                    SS包解读器.读取_有标签("子域名", 子域名)
                    SS包解读器.读取_有标签("连接凭据", 连接凭据)
                    SS包解读器.读取_有标签("是商品编辑", 是商品编辑)
                    If 是写入凭据 = False Then
                        If 小宇宙读取凭据 IsNot Nothing Then
                            Dim I As Integer
                            For I = 0 To 小宇宙读取凭据.Length - 1
                                If String.Compare(小宇宙读取凭据(I).英语子域名, 子域名) = 0 Then
                                    小宇宙读取凭据(I).连接凭据 = 连接凭据
                                    Exit For
                                End If
                            Next
                            If I = 小宇宙读取凭据.Length Then
                                ReDim Preserve 小宇宙读取凭据(小宇宙读取凭据.Length)
                                With 小宇宙读取凭据(小宇宙读取凭据.Length - 1)
                                    .英语子域名 = 子域名
                                    .连接凭据 = 连接凭据
                                    .是商品编辑 = 是商品编辑
                                End With
                            End If
                        Else
                            ReDim 小宇宙读取凭据(0)
                            With 小宇宙读取凭据(0)
                                .英语子域名 = 子域名
                                .连接凭据 = 连接凭据
                                .是商品编辑 = 是商品编辑
                            End With
                        End If
                    Else
                        子域名_小宇宙写入 = 子域名
                        小宇宙写入凭据 = 连接凭据
                    End If
                    For I = 0 To 聊天控件.Length - 1
                        If 聊天控件(I).IsDisposed = False Then
                            聊天控件(I).收到小宇宙的连接凭据(子域名, 连接凭据, 是商品编辑, 是写入凭据)
                        End If
                    Next
                ElseIf SS包解读器.查询结果 = 查询结果_常量集合.发送序号不一致 Then
                    If 主控机器人 IsNot Nothing Then 主控机器人.启动访问线程_传送服务器()
                ElseIf String.IsNullOrEmpty(SS包解读器.出错提示文本) = False Then
                    Dim I As Integer
                    For I = 0 To 聊天控件.Length - 1
                        If 聊天控件(I).IsDisposed = False Then
                            聊天控件(I).机器人.说(SS包解读器.出错提示文本)
                        End If
                    Next
                End If
            Catch ex As Exception
            End Try
        End If
    End Sub

End Class
