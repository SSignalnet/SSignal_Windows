
Friend Class ��_��������

#Region "���������"

    Private Structure ����_��������
        Dim ���� As String
        Dim �ı� As String
    End Structure

    Private Structure ��������_��������
        Dim ���� As String
        Dim ����() As ����_��������
        Dim �������� As Integer
    End Structure

    Const �滻��ʶ As String = "#%"
    Const ��������ʶ_ǰ�� As String = "$%"
    Const ��������ʶ_���� As String = "/%"
    '������There is$% are$% #% apple/% apples/% on the table.

    ReadOnly �ֶη�_�滻��ʶ() As String = New String() {�滻��ʶ}
    ReadOnly �ֶη�_��������ʶ_ǰ��() As String = New String() {��������ʶ_ǰ��}
    ReadOnly �ֶη�_��������ʶ_����() As String = New String() {��������ʶ_����}

    Dim ����() As ��������_��������
    Dim ���� As Boolean

#End Region

    Friend Sub New(Optional ByVal �ı� As String = Nothing,
                   Optional ByVal ����1 As Boolean = False)
        ���� = Nothing
        ���� = ����1
        If String.IsNullOrEmpty(�ı�) = True Then Return
        Dim �ַ�() As Char = �ı�.ToCharArray
        Dim I, J, ������������ As Integer
        Dim D As Char
        Dim A As String = ""
        Dim ���� As String
        Dim �����ֱ�� As Boolean
        ReDim ����(9)
        For I = 0 To �ַ�.Length - 1
            D = �ַ�(I)
            If D = ChrW(13) Then
                A = A.TrimStart
                If A.StartsWith("[") = True Then
                    A = A.TrimEnd
                    If A.EndsWith("]") = True Then
                        If A.Length > 2 Then
                            ���� = A.Substring(1, A.Length - 2)
Line1:
                            If ������������ = ����.Length Then ReDim Preserve ����(������������ + 9)
                            ����(������������).���� = ����
                            �����ֱ�� = False
                            Select Case ����
                                Case ����_һ��
                                    J = 499
                                    �����ֱ�� = True
                                Case ����_����
                                    J = 49
                                    �����ֱ�� = True
                                Case Else : J = 99
                            End Select
                            ReDim ����(������������).����(J)
                            ������������ += 1
                        End If
                    Else
                        If A.Length > 1 Then
                            ���� = A.Substring(1, A.Length - 1)
                            GoTo Line1
                        End If
                    End If
                ElseIf A.StartsWith("<") = True Then
                    J = A.IndexOf(">")
                    If J > 1 Then
                        J += 1
                        With ����(������������ - 1)
                            If .�������� = .����.Length Then ReDim Preserve .����(.�������� + 99)
                            With .����(.��������)
                                .���� = A.Substring(1, J - 2)
                                If A.Length > J Then
                                    .�ı� = A.Substring(J, A.Length - J)
                                End If
                                If �����ֱ�� Then J = Val(.����)
                            End With
                            If �����ֱ�� Then
                                If J <> .�������� Then
                                    ���� = Nothing
                                    Return
                                End If
                            End If
                            .�������� += 1
                        End With
                    End If
                End If
                A = ""
            Else
                If D <> ChrW(10) Then A &= D
            End If
        Next
        If ������������ > 0 Then
            For I = 0 To ������������ - 1
                With ����(I)
                    If .�������� < .����.Length Then ReDim Preserve .����(.�������� - 1)
                End With
            Next
            If ������������ < ����.Length Then ReDim Preserve ����(������������ - 1)
        Else
            ���� = Nothing
        End If
    End Sub

    Friend ReadOnly Property ������() As Boolean
        Get
            If ���� Is Nothing Then
                Return False
            Else
                Return True
            End If
        End Get
    End Property

    Friend ReadOnly Property �Ƿ���() As Boolean
        Get
            Return ����
        End Get
    End Property

    Friend Function ��ȡ(ByVal ���� As Object, ByVal �������� As String,
                         Optional ByVal Ҫ������ı�() As Object = Nothing,
                         Optional ByVal ����2 As Boolean = False) As String
        Return ��ȡ(����_һ��, ����, ��������, Ҫ������ı�, ����2)
    End Function

    Friend Function ��ȡ(ByVal ���� As String, ByVal ���� As Object, ByVal �������� As String,
                         Optional ByVal Ҫ������ı�() As Object = Nothing,
                         Optional ByVal ����2 As Boolean = False) As String
        If ���� IsNot Nothing Then
            If String.IsNullOrEmpty(����) = False Then
��ת��1:
                Dim I As Integer
                For I = 0 To ����.Length - 1
                    If String.Compare(����(I).����, ����) = 0 Then Exit For
                Next
                If I < ����.Length Then
                    Dim J As Integer
                    If TypeOf ���� Is String Then
Line1:
                        With ����(I)
                            For J = 0 To .�������� - 1
                                If String.Compare(.����(J).����, ����) = 0 Then Exit For
                            Next
                            If J < .�������� Then
                                If ���� = False AndAlso ����2 = False Then
                                    If Ҫ������ı� Is Nothing Then
                                        Return .����(J).�ı�
                                    Else
                                        Return �����ı�(.����(J).�ı�, Ҫ������ı�)
                                    End If
                                Else
                                    If Ҫ������ı� Is Nothing Then
                                        Return Strings.StrConv(.����(J).�ı�, VbStrConv.TraditionalChinese)
                                    Else
                                        Return Strings.StrConv(�����ı�(.����(J).�ı�, Ҫ������ı�), VbStrConv.TraditionalChinese)
                                    End If
                                End If
                            Else
                                GoTo Line2
                            End If
                        End With
                    ElseIf TypeOf ���� Is Integer Then
Line3:
                        J = ����
                        If J > 0 Then
                            With ����(I)
                                If J < .�������� Then
                                    If ���� = False AndAlso ����2 = False Then
                                        If Ҫ������ı� Is Nothing Then
                                            Return .����(J).�ı�
                                        Else
                                            Return �����ı�(.����(J).�ı�, Ҫ������ı�)
                                        End If
                                    Else
                                        If Ҫ������ı� Is Nothing Then
                                            Return Strings.StrConv(.����(J).�ı�, VbStrConv.TraditionalChinese)
                                        Else
                                            Return Strings.StrConv(�����ı�(.����(J).�ı�, Ҫ������ı�), VbStrConv.TraditionalChinese)
                                        End If
                                    End If
                                Else
                                    GoTo Line1
                                End If
                            End With
                        Else
                            GoTo Line1
                        End If
                    ElseIf TypeOf ���� Is Short OrElse TypeOf ���� Is Byte OrElse TypeOf ���� Is Long Then
                        GoTo Line3
                    Else
                        GoTo Line1
                    End If
                End If
            End If
        End If
Line2:
        If ���� = False AndAlso ����2 = False Then
            If Ҫ������ı� Is Nothing Then
                Return ��������
            Else
                Return �����ı�(��������, Ҫ������ı�)
            End If
        Else
            If Ҫ������ı� Is Nothing Then
                Return Strings.StrConv(��������, VbStrConv.TraditionalChinese)
            Else
                Return Strings.StrConv(�����ı�(��������, Ҫ������ı�), VbStrConv.TraditionalChinese)
            End If
        End If
    End Function

    Private Function �����ı�(ByRef ԭ�ı� As String, ByVal Ҫ������ı�() As Object) As String
        Dim ��() As String = ԭ�ı�.Split(�ֶη�_�滻��ʶ, StringSplitOptions.None)
        If ��.Length > 1 Then
            Dim I, K As Integer
            Dim �滻����ı� As String = ""
            For I = 0 To ��.Length - 1
                If I = 0 Then
                    If I < Ҫ������ı�.Length Then
                        If ��(I).IndexOf(��������ʶ_ǰ��) < 0 Then
                            �滻����ı� = ��(I)
                        Else
                            Dim ��1() As String = ��(I).Split(�ֶη�_��������ʶ_ǰ��, StringSplitOptions.RemoveEmptyEntries)
                            If ��1.Length > 1 Then
                                K = 0
                                If ��1.Length > 2 Then
                                    For K = 0 To ��1.Length - 3
                                        �滻����ı� &= ��1(K)
                                    Next
                                End If
                                Select Case Val(Ҫ������ı�(I))
                                    Case 1, 0 : �滻����ı� &= ��1(K)
                                    Case Else : �滻����ı� &= ��1(K + 1)
                                End Select
                            Else
                                �滻����ı� = ��(I)
                            End If
                        End If
                    Else
                        �滻����ı� = ��(I)
                    End If
                Else
                    K = I - 1
                    If K < Ҫ������ı�.Length Then
                        If ��(I).IndexOf(��������ʶ_����) > 0 Then
                            Dim ��1() As String = ��(I).Split(�ֶη�_��������ʶ_����, StringSplitOptions.RemoveEmptyEntries)
                            If ��1.Length > 1 Then
                                Select Case Val(Ҫ������ı�(K))
                                    Case 1, 0 : �滻����ı� &= Ҫ������ı�(K) & ��1(0)
                                    Case Else : �滻����ı� &= Ҫ������ı�(K) & ��1(1)
                                End Select
                                If ��1.Length > 2 Then
                                    For K = 2 To ��1.Length - 1
                                        �滻����ı� &= ��1(K)
                                    Next
                                End If
                            Else
                                GoTo Line1
                            End If
                        Else
                            If I < Ҫ������ı�.Length Then
                                If ��(I).IndexOf(��������ʶ_ǰ��) >= 0 Then
                                    Dim ��1() As String = ��(I).Split(�ֶη�_��������ʶ_ǰ��, StringSplitOptions.RemoveEmptyEntries)
                                    If ��1.Length > 1 Then
                                        If ��1.Length > 2 Then
                                            For K = 0 To ��1.Length - 3
                                                �滻����ı� &= ��1(K)
                                            Next
                                        End If
                                        Select Case Val(Ҫ������ı�(I))
                                            Case 1, 0 : �滻����ı� &= ��1(0)
                                            Case Else : �滻����ı� &= ��1(1)
                                        End Select
                                    Else
                                        GoTo Line1
                                    End If
                                Else
                                    GoTo Line1
                                End If
                            Else
Line1:
                                �滻����ı� &= Ҫ������ı�(K) & ��(I)
                            End If
                        End If
                    Else
                        �滻����ı� &= �滻��ʶ & ��(I)
                    End If
                End If
            Next
            Return �滻����ı�
        Else
            Return ԭ�ı�
        End If
    End Function

End Class
