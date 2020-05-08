
Friend Class 类_界面文字

#Region "定义和声明"

    Private Structure 文字_复合数据
        Dim 代码 As String
        Dim 文本 As String
    End Structure

    Private Structure 界面文字_复合数据
        Dim 组名 As String
        Dim 文字() As 文字_复合数据
        Dim 文字数量 As Integer
    End Structure

    Const 替换标识 As String = "#%"
    Const 单复数标识_前面 As String = "$%"
    Const 单复数标识_后面 As String = "/%"
    '举例：There is$% are$% #% apple/% apples/% on the table.

    ReadOnly 分段符_替换标识() As String = New String() {替换标识}
    ReadOnly 分段符_单复数标识_前面() As String = New String() {单复数标识_前面}
    ReadOnly 分段符_单复数标识_后面() As String = New String() {单复数标识_后面}

    Dim 数据() As 界面文字_复合数据
    Dim 繁体 As Boolean

#End Region

    Friend Sub New(Optional ByVal 文本 As String = Nothing,
                   Optional ByVal 繁体1 As Boolean = False)
        数据 = Nothing
        繁体 = 繁体1
        If String.IsNullOrEmpty(文本) = True Then Return
        Dim 字符() As Char = 文本.ToCharArray
        Dim I, J, 界面文字组数 As Integer
        Dim D As Char
        Dim A As String = ""
        Dim 组名 As String
        Dim 是数字编号 As Boolean
        ReDim 数据(9)
        For I = 0 To 字符.Length - 1
            D = 字符(I)
            If D = ChrW(13) Then
                A = A.TrimStart
                If A.StartsWith("[") = True Then
                    A = A.TrimEnd
                    If A.EndsWith("]") = True Then
                        If A.Length > 2 Then
                            组名 = A.Substring(1, A.Length - 2)
Line1:
                            If 界面文字组数 = 数据.Length Then ReDim Preserve 数据(界面文字组数 + 9)
                            数据(界面文字组数).组名 = 组名
                            是数字编号 = False
                            Select Case 组名
                                Case 组名_一般
                                    J = 499
                                    是数字编号 = True
                                Case 组名_任务
                                    J = 49
                                    是数字编号 = True
                                Case Else : J = 99
                            End Select
                            ReDim 数据(界面文字组数).文字(J)
                            界面文字组数 += 1
                        End If
                    Else
                        If A.Length > 1 Then
                            组名 = A.Substring(1, A.Length - 1)
                            GoTo Line1
                        End If
                    End If
                ElseIf A.StartsWith("<") = True Then
                    J = A.IndexOf(">")
                    If J > 1 Then
                        J += 1
                        With 数据(界面文字组数 - 1)
                            If .文字数量 = .文字.Length Then ReDim Preserve .文字(.文字数量 + 99)
                            With .文字(.文字数量)
                                .代码 = A.Substring(1, J - 2)
                                If A.Length > J Then
                                    .文本 = A.Substring(J, A.Length - J)
                                End If
                                If 是数字编号 Then J = Val(.代码)
                            End With
                            If 是数字编号 Then
                                If J <> .文字数量 Then
                                    数据 = Nothing
                                    Return
                                End If
                            End If
                            .文字数量 += 1
                        End With
                    End If
                End If
                A = ""
            Else
                If D <> ChrW(10) Then A &= D
            End If
        Next
        If 界面文字组数 > 0 Then
            For I = 0 To 界面文字组数 - 1
                With 数据(I)
                    If .文字数量 < .文字.Length Then ReDim Preserve .文字(.文字数量 - 1)
                End With
            Next
            If 界面文字组数 < 数据.Length Then ReDim Preserve 数据(界面文字组数 - 1)
        Else
            数据 = Nothing
        End If
    End Sub

    Friend ReadOnly Property 有数据() As Boolean
        Get
            If 数据 Is Nothing Then
                Return False
            Else
                Return True
            End If
        End Get
    End Property

    Friend ReadOnly Property 是繁体() As Boolean
        Get
            Return 繁体
        End Get
    End Property

    Friend Function 获取(ByVal 代码 As Object, ByVal 现有文字 As String,
                         Optional ByVal 要插入的文本() As Object = Nothing,
                         Optional ByVal 繁体2 As Boolean = False) As String
        Return 获取(组名_一般, 代码, 现有文字, 要插入的文本, 繁体2)
    End Function

    Friend Function 获取(ByVal 组名 As String, ByVal 代码 As Object, ByVal 现有文字 As String,
                         Optional ByVal 要插入的文本() As Object = Nothing,
                         Optional ByVal 繁体2 As Boolean = False) As String
        If 数据 IsNot Nothing Then
            If String.IsNullOrEmpty(代码) = False Then
跳转点1:
                Dim I As Integer
                For I = 0 To 数据.Length - 1
                    If String.Compare(数据(I).组名, 组名) = 0 Then Exit For
                Next
                If I < 数据.Length Then
                    Dim J As Integer
                    If TypeOf 代码 Is String Then
Line1:
                        With 数据(I)
                            For J = 0 To .文字数量 - 1
                                If String.Compare(.文字(J).代码, 代码) = 0 Then Exit For
                            Next
                            If J < .文字数量 Then
                                If 繁体 = False AndAlso 繁体2 = False Then
                                    If 要插入的文本 Is Nothing Then
                                        Return .文字(J).文本
                                    Else
                                        Return 插入文本(.文字(J).文本, 要插入的文本)
                                    End If
                                Else
                                    If 要插入的文本 Is Nothing Then
                                        Return Strings.StrConv(.文字(J).文本, VbStrConv.TraditionalChinese)
                                    Else
                                        Return Strings.StrConv(插入文本(.文字(J).文本, 要插入的文本), VbStrConv.TraditionalChinese)
                                    End If
                                End If
                            Else
                                GoTo Line2
                            End If
                        End With
                    ElseIf TypeOf 代码 Is Integer Then
Line3:
                        J = 代码
                        If J > 0 Then
                            With 数据(I)
                                If J < .文字数量 Then
                                    If 繁体 = False AndAlso 繁体2 = False Then
                                        If 要插入的文本 Is Nothing Then
                                            Return .文字(J).文本
                                        Else
                                            Return 插入文本(.文字(J).文本, 要插入的文本)
                                        End If
                                    Else
                                        If 要插入的文本 Is Nothing Then
                                            Return Strings.StrConv(.文字(J).文本, VbStrConv.TraditionalChinese)
                                        Else
                                            Return Strings.StrConv(插入文本(.文字(J).文本, 要插入的文本), VbStrConv.TraditionalChinese)
                                        End If
                                    End If
                                Else
                                    GoTo Line1
                                End If
                            End With
                        Else
                            GoTo Line1
                        End If
                    ElseIf TypeOf 代码 Is Short OrElse TypeOf 代码 Is Byte OrElse TypeOf 代码 Is Long Then
                        GoTo Line3
                    Else
                        GoTo Line1
                    End If
                End If
            End If
        End If
Line2:
        If 繁体 = False AndAlso 繁体2 = False Then
            If 要插入的文本 Is Nothing Then
                Return 现有文字
            Else
                Return 插入文本(现有文字, 要插入的文本)
            End If
        Else
            If 要插入的文本 Is Nothing Then
                Return Strings.StrConv(现有文字, VbStrConv.TraditionalChinese)
            Else
                Return Strings.StrConv(插入文本(现有文字, 要插入的文本), VbStrConv.TraditionalChinese)
            End If
        End If
    End Function

    Private Function 插入文本(ByRef 原文本 As String, ByVal 要插入的文本() As Object) As String
        Dim 段() As String = 原文本.Split(分段符_替换标识, StringSplitOptions.None)
        If 段.Length > 1 Then
            Dim I, K As Integer
            Dim 替换后的文本 As String = ""
            For I = 0 To 段.Length - 1
                If I = 0 Then
                    If I < 要插入的文本.Length Then
                        If 段(I).IndexOf(单复数标识_前面) < 0 Then
                            替换后的文本 = 段(I)
                        Else
                            Dim 段1() As String = 段(I).Split(分段符_单复数标识_前面, StringSplitOptions.RemoveEmptyEntries)
                            If 段1.Length > 1 Then
                                K = 0
                                If 段1.Length > 2 Then
                                    For K = 0 To 段1.Length - 3
                                        替换后的文本 &= 段1(K)
                                    Next
                                End If
                                Select Case Val(要插入的文本(I))
                                    Case 1, 0 : 替换后的文本 &= 段1(K)
                                    Case Else : 替换后的文本 &= 段1(K + 1)
                                End Select
                            Else
                                替换后的文本 = 段(I)
                            End If
                        End If
                    Else
                        替换后的文本 = 段(I)
                    End If
                Else
                    K = I - 1
                    If K < 要插入的文本.Length Then
                        If 段(I).IndexOf(单复数标识_后面) > 0 Then
                            Dim 段1() As String = 段(I).Split(分段符_单复数标识_后面, StringSplitOptions.RemoveEmptyEntries)
                            If 段1.Length > 1 Then
                                Select Case Val(要插入的文本(K))
                                    Case 1, 0 : 替换后的文本 &= 要插入的文本(K) & 段1(0)
                                    Case Else : 替换后的文本 &= 要插入的文本(K) & 段1(1)
                                End Select
                                If 段1.Length > 2 Then
                                    For K = 2 To 段1.Length - 1
                                        替换后的文本 &= 段1(K)
                                    Next
                                End If
                            Else
                                GoTo Line1
                            End If
                        Else
                            If I < 要插入的文本.Length Then
                                If 段(I).IndexOf(单复数标识_前面) >= 0 Then
                                    Dim 段1() As String = 段(I).Split(分段符_单复数标识_前面, StringSplitOptions.RemoveEmptyEntries)
                                    If 段1.Length > 1 Then
                                        If 段1.Length > 2 Then
                                            For K = 0 To 段1.Length - 3
                                                替换后的文本 &= 段1(K)
                                            Next
                                        End If
                                        Select Case Val(要插入的文本(I))
                                            Case 1, 0 : 替换后的文本 &= 段1(0)
                                            Case Else : 替换后的文本 &= 段1(1)
                                        End Select
                                    Else
                                        GoTo Line1
                                    End If
                                Else
                                    GoTo Line1
                                End If
                            Else
Line1:
                                替换后的文本 &= 要插入的文本(K) & 段(I)
                            End If
                        End If
                    Else
                        替换后的文本 &= 替换标识 & 段(I)
                    End If
                End If
            Next
            Return 替换后的文本
        Else
            Return 原文本
        End If
    End Function

End Class
