Imports System.Net
Imports System.Threading
Imports System.IO
Imports SSignalDB

Friend Class ��_����ý���ļ�
    Implements IDisposable

#Region "���������"

    Friend Enum ��������_��� As Byte
        �� = 0
        ʧ��_δ�ҵ��ļ� = 1
        ʧ��_δ�ɹ�������վ = 2
        ʧ��_�����ж� = 3
        ʧ��_�ֽ�����һ�� = 4
        �ɹ� = 5
    End Enum

    Dim �������ص���Ŀ As ��_���������Ʒ
    Friend ���ص�ַ As String
    Dim ����·�� As String
    Dim ���ֽ���, �������ֽ��� As Integer
    Friend �������� As Boolean
    Shared �߳� As Thread
    Dim HTTP�������� As HttpWebRequest
    Dim �ڴ��� As MemoryStream
    Dim �ļ��޸�ʱ�� As Date

    Friend Event ���ؽ���(ByVal sender As Object, ByVal ��� As ��������_���, ByVal �������ص���Ŀ As ��_���������Ʒ)

#End Region

    Friend Sub ����(ByVal �������ص���Ŀ1 As ��_���������Ʒ, ByVal ���ص�ַ1 As String, ByVal ����·��1 As String)
        If �߳� Is Nothing Then
            �������ص���Ŀ = �������ص���Ŀ1
            If String.Compare(���ص�ַ, ���ص�ַ1) <> 0 Then
                ���ص�ַ = ���ص�ַ1
                Call ����()
            End If
            ����·�� = ����·��1
            �������� = True
            If �߳� Is Nothing Then
                �߳� = New Thread(New ThreadStart(AddressOf ��ʼ����))
                �߳�.Start()
            End If
        End If
    End Sub

    Private Sub ��ʼ����()
        Try
            Dim �ļ�δ�ҵ�, �ļ�δ�ı� As Boolean
            Dim HTTP�����Ӧ As HttpWebResponse = Nothing
            Dim ���ؿ�ʼʱ�� As Date = Date.Now
            Try
                If ���ֽ��� > 0 AndAlso �������ֽ��� > 0 Then
                    If �ڴ��� IsNot Nothing Then
                        If �ڴ���.Length >= ���ֽ��� OrElse �������ֽ��� <> �ڴ���.Length Then
                            Call ����()
                        End If
                    Else
                        Call ����()
                    End If
                Else
                    Call ����()
                End If
                Dim �ļ��޸�ʱ��_�� As Date
                Dim �� As Stream
HTTP��ͷ��ʼ:
                HTTP�������� = WebRequest.Create(���ص�ַ)
                HTTP��������.Timeout = 10000
                If ���ֽ��� > 0 AndAlso �������ֽ��� > 0 Then
                    HTTP��������.AddRange(�������ֽ���, ���ֽ��� - 1)
                    HTTP�����Ӧ = HTTP��������.GetResponse
                    Select Case HTTP�����Ӧ.StatusCode
                        Case HttpStatusCode.NotFound
                            �ļ�δ�ҵ� = True
                            Exit Try
                        Case HttpStatusCode.OK, HttpStatusCode.RequestedRangeNotSatisfiable  '��������֧�ֶϵ�����
                            HTTP�����Ӧ.Close()
                            Call ����()
                            GoTo HTTP��ͷ��ʼ
                        Case HttpStatusCode.PartialContent
                            �ļ��޸�ʱ��_�� = HTTP�����Ӧ.LastModified
                            If �������ֽ��� + HTTP�����Ӧ.ContentLength = ���ֽ��� Then
                                If �ڴ��� IsNot Nothing Then
                                    If �ļ��޸�ʱ��.Ticks <> �ļ��޸�ʱ��_��.Ticks Then
                                        GoTo ��ת��1
                                    End If
                                Else
                                    GoTo ��ת��1
                                End If
                            Else
��ת��1:
                                HTTP�����Ӧ.Close()
                                Call ����()
                                GoTo HTTP��ͷ��ʼ
                            End If
                            �ļ��޸�ʱ�� = �ļ��޸�ʱ��_��
                            If �������� = False Then Exit Try
                            �� = HTTP�����Ӧ.GetResponseStream
                        Case Else
                            Exit Try
                    End Select
                Else
                    HTTP�����Ӧ = HTTP��������.GetResponse
                    Select Case HTTP�����Ӧ.StatusCode
                        Case HttpStatusCode.NotFound
                            �ļ�δ�ҵ� = True
                            Exit Try
                        Case HttpStatusCode.OK
                            ���ֽ��� = HTTP�����Ӧ.ContentLength
                            �ļ��޸�ʱ��_�� = HTTP�����Ӧ.LastModified
                            �ļ��޸�ʱ�� = �ļ��޸�ʱ��_��
                            If �������� = False Then Exit Try
                            �� = HTTP�����Ӧ.GetResponseStream
                        Case Else
                            Exit Try
                    End Select
                End If
                If �ڴ��� Is Nothing Then �ڴ��� = New MemoryStream
                Dim �ֽ�����(���ݿ�ǧ�ֽ� * 4 - 1) As Byte
                Dim ��ȡ���ֽ��� As Integer
                Do
                    ��ȡ���ֽ��� = ��.Read(�ֽ�����, 0, �ֽ�����.Length)
                    If ��ȡ���ֽ��� > 0 AndAlso �������� = True Then
                        �ڴ���.Write(�ֽ�����, 0, ��ȡ���ֽ���)
                        �������ֽ��� += ��ȡ���ֽ���
                    End If
                Loop Until ��ȡ���ֽ��� = 0 OrElse �������ֽ��� >= ���ֽ��� OrElse �������� = False
            Catch ex As WebException
                If ex.Message.IndexOf("(404)") > 0 Then �ļ�δ�ҵ� = True
            Catch ex As Exception
            End Try
            �������� = False
            Try
                If HTTP�����Ӧ IsNot Nothing Then HTTP�����Ӧ.Close()
            Catch ex As Exception
            End Try
            �߳� = Nothing
            If �ļ�δ�ҵ� = False Then
                If �ļ�δ�ı� = False Then
                    If �������ֽ��� < ���ֽ��� Then
                        Call ����ֹͣ(��������_���.ʧ��_�����ж�)
                    ElseIf ���ֽ��� > 0 Then
                        If �������ֽ��� = ���ֽ��� Then
                            Try
                                Dim �ļ���Ϣ As New FileInfo(����·��)
                                If �ļ���Ϣ.Exists = True Then
                                    �ļ���Ϣ.Delete()
                                Else
                                    Dim Ŀ¼ As String = Path.GetDirectoryName(����·��)
                                    If Directory.Exists(Ŀ¼) = False Then Directory.CreateDirectory(Ŀ¼)
                                End If
                                File.WriteAllBytes(����·��, �ڴ���.ToArray)
                                �ļ���Ϣ = New FileInfo(����·��)
                                If �ļ���Ϣ.Exists Then �ļ���Ϣ.LastWriteTime = �ļ��޸�ʱ��
                            Catch ex As Exception
                                Call ����ֹͣ(��������_���.ʧ��_�����ж�)
                                Return
                            End Try
                            Dim �� As Long = DateDiff(DateInterval.Second, ���ؿ�ʼʱ��, Date.Now)
                            If �� < 60 Then
                                Call ����ֹͣ(��������_���.�ɹ�)
                            Else
                                Dim ���� As Integer = Int(�� / 60)
                                Call ����ֹͣ(��������_���.�ɹ�)
                            End If
                        Else
                            Call ����ֹͣ(��������_���.ʧ��_�ֽ�����һ��)
                        End If
                    Else
                        Call ����ֹͣ(��������_���.ʧ��_δ�ɹ�������վ)
                    End If
                Else
                    Call ����ֹͣ(��������_���.�ɹ�)
                End If
            Else
                Call ����ֹͣ(��������_���.ʧ��_δ�ҵ��ļ�)
            End If
        Catch ex As Exception
            �������� = False
            �߳� = Nothing
        End Try
    End Sub

    Private Sub ����()
        ���ֽ��� = 0
        �������ֽ��� = 0
        Call �ر��ڴ���()
    End Sub

    Private Sub �ر��ڴ���()
        If �ڴ��� IsNot Nothing Then
            �ڴ���.Close()
            �ڴ���.Dispose()
            �ڴ��� = Nothing
        End If
    End Sub

    Friend Sub ��ͣ(Optional ByVal �ر��ڴ���1 As Boolean = False)
        �������� = False
        Try
            If HTTP�������� IsNot Nothing Then
                HTTP��������.Abort()
                HTTP�������� = Nothing
            End If
        Catch ex As Exception
        End Try
        If �ر��ڴ���1 = True Then Call �ر��ڴ���()
        If �߳� IsNot Nothing Then
            �߳�.Abort()
            �߳� = Nothing
        End If
    End Sub

    Private Sub ����ֹͣ(ByVal ��� As ��������_���)
        RaiseEvent ���ؽ���(Me, ���, �������ص���Ŀ)
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' �������ĵ���

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                Call ��ͣ(True)
            End If

            ' TODO:  �ͷŷ��й���Դ(���йܶ���)����д����� Finalize()��
            ' TODO:  �������ֶ�����Ϊ null��
        End If
        Me.disposedValue = True
    End Sub

    ' TODO:  ��������� Dispose(ByVal disposing As Boolean)�����ͷŷ��й���Դ�Ĵ���ʱ��д Finalize()��
    'Protected Overrides Sub Finalize()
    '    ' ��Ҫ���Ĵ˴��롣    �뽫��������������� Dispose(ByVal disposing As Boolean)�С�
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' Visual Basic ��Ӵ˴�����Ϊ����ȷʵ�ֿɴ���ģʽ��
    Public Sub Dispose() Implements IDisposable.Dispose
        ' ��Ҫ���Ĵ˴��롣    �뽫��������������� Dispose (disposing As Boolean)�С�
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
