Imports System.Threading
Imports MF = SharpDX.MediaFoundation

Friend Class ��_��Ƶ_����
    Implements MF.IAsyncCallback

#Region "���������"

    Friend Enum ��������_״̬ As Byte
        �ر� = 0
        ���� = 1
        ��ͣ = 2
        ֹͣ = 3
    End Enum

    Friend Enum ��������_�¼� As Byte
        ���ڴ�ý�� = 0
        ý������ɹ� = 1
        ���ſ�ʼ = 2
        ������ͣ = 3
        ����ֹͣ_��Ϊ = 4
        ����ֹͣ_��� = 5
        ý��ر� = 6
    End Enum

    Dim ״̬1 As ��������_״̬

    Dim ��Ƶ���Ŵ��� As Control
    Dim ������ As MF.MediaSession
    Dim ý������Դ As MF.MediaSource
    Dim ������������ As MF.AudioStreamVolume
    Dim ��Ƶ��ʾ������ As MF.VideoDisplayControl
    Dim ý������ As MF.SourceResolver
    Dim CO As SharpDX.ComObject
    Dim ʱ��1 As Long
    Dim ����1 As Single = 1.0
    Dim ����Ƶ, �������ļ� As Boolean
    Dim �߳� As Thread
    Dim �ļ���ַ As String
    Friend ��ǰ����ʱ��� As Long
    WithEvents ��ʱ�� As System.Windows.Forms.Timer

    Friend Event ���ڴ�ý��(ByVal sender As Object)
    Friend Event ý������ɹ�(ByVal sender As Object)
    Friend Event ���ſ�ʼ(ByVal sender As Object)
    Friend Event ������ͣ(ByVal sender As Object)
    Friend Event ����ֹͣ_��Ϊ(ByVal sender As Object)
    Friend Event ����ֹͣ_���(ByVal sender As Object)
    Friend Event ý��ر�(ByVal sender As Object)

    Private Delegate Sub �����¼�_���߳�(ByVal �¼� As ��������_�¼�)
    Private Delegate Sub ���������ļ�2_���߳�()

#End Region

    Friend Property Shadow As IDisposable Implements SharpDX.ICallbackable.Shadow

    Friend ReadOnly Property Flags As SharpDX.MediaFoundation.AsyncCallbackFlags Implements SharpDX.MediaFoundation.IAsyncCallback.Flags
        Get

        End Get
    End Property

    Friend Sub Invoke(asyncResultRef As SharpDX.MediaFoundation.AsyncResult) Implements SharpDX.MediaFoundation.IAsyncCallback.Invoke
        Dim �¼� As MF.MediaEvent
        Try
            �¼� = ������.EndGetEvent(asyncResultRef)
        Catch ex As Exception
            Return
        End Try
        If �¼�.TypeInfo = SharpDX.MediaFoundation.MediaEventTypes.SessionClosed Then

        Else
            Try
                ������.BeginGetEvent(Me, Nothing)
            Catch ex As Exception
                Return
            End Try
        End If
        Select Case �¼�.TypeInfo
            Case SharpDX.MediaFoundation.MediaEventTypes.SessionStarted
                If ������������ Is Nothing Then
                    Dim CO As New SharpDX.ComObject(������.NativePointer)
                    Dim GUID As New Guid("76B1BBDB-4EC8-4f36-B106-70A9316DF593")
                    Dim Ptr As IntPtr
                    Try
                        MF.MediaFactory.GetService(CO, MF.MediaServiceKeys.StreamVolume, GUID, Ptr)
                        ������������ = New MF.AudioStreamVolume(Ptr)
                        If ������������.ChannelCount > 0 Then
                            Dim ��������(������������.ChannelCount - 1) As Single
                            Dim I As Integer
                            For I = 0 To ��������.Length - 1
                                ��������(I) = ����1
                            Next
                            ������������.SetAllVolumes(������������.ChannelCount, ��������)
                        Else
                            ������������.Dispose()
                            ������������ = Nothing
                        End If
                    Catch ex As Exception
                    End Try
                End If
                If ����Ƶ = True AndAlso ��Ƶ��ʾ������ Is Nothing Then
                    Dim CO As New SharpDX.ComObject(������.NativePointer)
                    Dim GUID1 As New Guid("{0x1092a86c, 0xab1a, 0x459a, {0xa3, 0x36, 0x83, 0x1f, 0xbc, 0x4d, 0x11, 0xff}}")
                    Dim GUID2 As New Guid("a490b1e4-ab84-4d31-a1b2-181e03b1077a")
                    Dim Ptr As New IntPtr
                    Try
                        MF.MediaFactory.GetService(CO, GUID1, GUID2, Ptr)
                        ��Ƶ��ʾ������ = New MF.VideoDisplayControl(Ptr)
                    Catch ex As Exception
                    End Try
                End If
                If ״̬1 <> ��������_״̬.���� Then
                    ״̬1 = ��������_״̬.����
                    Call �����¼�(��������_�¼�.���ſ�ʼ)
                End If
            Case SharpDX.MediaFoundation.MediaEventTypes.SessionStopped
                If ״̬1 <> ��������_״̬.ֹͣ Then
                    ״̬1 = ��������_״̬.ֹͣ
                    Call �����¼�(��������_�¼�.����ֹͣ_��Ϊ)
                End If
            Case SharpDX.MediaFoundation.MediaEventTypes.SessionPaused
                If ״̬1 <> ��������_״̬.��ͣ Then
                    ״̬1 = ��������_״̬.��ͣ
                    Call �����¼�(��������_�¼�.������ͣ)
                End If
            Case SharpDX.MediaFoundation.MediaEventTypes.SessionEnded
                If ״̬1 <> ��������_״̬.ֹͣ Then
                    ״̬1 = ��������_״̬.ֹͣ
                    Call �����¼�(��������_�¼�.����ֹͣ_���)
                End If
            Case SharpDX.MediaFoundation.MediaEventTypes.SessionClosed
                If ״̬1 <> ��������_״̬.ֹͣ Then
                    ״̬1 = ��������_״̬.ֹͣ
                    Call �����¼�(��������_�¼�.ý��ر�)
                End If
        End Select
        �¼�.Dispose()
    End Sub

    Friend ReadOnly Property WorkQueueId As SharpDX.MediaFoundation.WorkQueueId Implements SharpDX.MediaFoundation.IAsyncCallback.WorkQueueId
        Get

        End Get
    End Property

    Friend Sub New(ByVal ��Ƶ���Ŵ���1 As Control)
        ��Ƶ���Ŵ��� = ��Ƶ���Ŵ���1
    End Sub

    Friend ReadOnly Property ʱ�� As Double
        Get
            Return ʱ��1 / 1000 / 10000
        End Get
    End Property

    Friend ReadOnly Property ���Ž��� As Single
        Get
            If ʱ��1 > 0 Then
                Try
                    ������.Clock.GetCorrelatedTime(0, ��ǰ����ʱ���, 0)
                Catch ex As Exception
                    Return 0
                End Try
                Return ��ǰ����ʱ��� / ʱ��1
            Else
                Return 0
            End If
        End Get
    End Property

    Friend ReadOnly Property ״̬ As ��������_״̬
        Get
            Return ״̬1
        End Get
    End Property

    Friend Property ���� As Single
        Get
            Return ����1
        End Get
        Set(value As Single)
            If value < 0 Then
                ����1 = 0
            ElseIf value > 1.0 Then
                ����1 = 1.0
            Else
                ����1 = value
            End If
            If ������������ IsNot Nothing Then
                If ������������.ChannelCount > 0 Then
                    Dim ��������(������������.ChannelCount - 1) As Single
                    Dim I As Integer
                    For I = 0 To ��������.Length - 1
                        ��������(I) = ����1
                    Next
                    ������������.SetAllVolumes(������������.ChannelCount, ��������)
                End If
            End If
        End Set
    End Property

    'Friend Property ȫ�� As Boolean
    '    Get
    '        If ��Ƶ��ʾ������ IsNot Nothing Then
    '            Return ��Ƶ��ʾ������.Fullscreen
    '        Else
    '            Return False
    '        End If
    '    End Get
    '    Set(value As Boolean)
    '        If ��Ƶ��ʾ������ IsNot Nothing Then ��Ƶ��ʾ������.Fullscreen = value
    '    End Set
    'End Property

    Friend Sub ���ű����ļ�(Optional ByVal �����ļ�·�� As String = Nothing)
        If String.IsNullOrEmpty(�����ļ�·��) = True Then
            If String.IsNullOrEmpty(�ļ���ַ) = True Then Return
        Else
            �ļ���ַ = �����ļ�·��
        End If
        �������ļ� = False
        If ������ IsNot Nothing Then
            ������.Stop()
            ������.Close()
            ������.Dispose()
            ������ = Nothing
        End If
        If ������������ IsNot Nothing Then
            ������������.Dispose()
            ������������ = Nothing
        End If
        If ��Ƶ��ʾ������ IsNot Nothing Then
            ��Ƶ��ʾ������.Dispose()
            ��Ƶ��ʾ������ = Nothing
        End If
        If ý������Դ IsNot Nothing Then
            ý������Դ.Shutdown()
            ý������Դ.Dispose()
            ý������Դ = Nothing
        End If
        ״̬1 = ��������_״̬.�ر�
        ����Ƶ = False
        Dim ý������ As MF.SourceResolver = Nothing
        Dim �ڵ������ As MF.Topology = Nothing
        Dim �ܲ������� As MF.PresentationDescriptor = Nothing
        Dim ���������� As MF.StreamDescriptor = Nothing
        Dim ���ݽڵ�_���� As MF.TopologyNode = Nothing
        Dim ���ݽڵ�_��� As MF.TopologyNode = Nothing
        Call �����¼�(��������_�¼�.���ڴ�ý��)
        Try
            If ������ý������� = False Then
                MF.MediaManager.Startup()
                ������ý������� = True
            End If
            MF.MediaFactory.CreateMediaSession(Nothing, ������)
            ������.BeginGetEvent(Me, Nothing)

            ý������ = New MF.SourceResolver
            Dim �������� As MF.ObjectType = SharpDX.MediaFoundation.ObjectType.Invalid
            Dim CO As SharpDX.ComObject = ý������.CreateObjectFromURL(�ļ���ַ, MF.SourceResolverFlags.MediaSource, ��������)
            If �������� <> SharpDX.MediaFoundation.ObjectType.MediaSource Then Return
            ý������Դ = New MF.MediaSource(CO)

            MF.MediaFactory.CreateTopology(�ڵ������)
            ý������Դ.CreatePresentationDescriptor(�ܲ�������)
            ʱ��1 = �ܲ�������.Get(MF.PresentationDescriptionAttributeKeys.Duration)
            Dim ѡ�� As Boolean
            Dim I As Integer
            For I = 0 To �ܲ�������.StreamDescriptorCount - 1
                ѡ�� = False
                �ܲ�������.GetStreamDescriptorByIndex(I, ѡ��, ����������)
                If ѡ�� = True Then
                    MF.MediaFactory.CreateTopologyNode(SharpDX.MediaFoundation.TopologyType.SourceStreamNode, ���ݽڵ�_����)
                    ���ݽڵ�_����.Set(MF.TopologyNodeAttributeKeys.Source, ý������Դ)
                    ���ݽڵ�_����.Set(MF.TopologyNodeAttributeKeys.PresentationDescriptor, �ܲ�������)
                    ���ݽڵ�_����.Set(MF.TopologyNodeAttributeKeys.StreamDescriptor, ����������)

                    Dim ý������������ As MF.MediaTypeHandler = ����������.MediaTypeHandler
                    Dim ������ As Guid = ý������������.MajorType
                    Dim ��������� As MF.Activate = Nothing
                    If ������ = MF.MediaTypeGuids.Audio Then
                        MF.MediaFactory.CreateAudioRendererActivate(���������)
                    ElseIf ������ = MF.MediaTypeGuids.Video Then
                        MF.MediaFactory.CreateVideoRendererActivate(��Ƶ���Ŵ���.Handle, ���������)
                        ����Ƶ = True
                    Else
                        Return
                    End If

                    MF.MediaFactory.CreateTopologyNode(SharpDX.MediaFoundation.TopologyType.OutputNode, ���ݽڵ�_���)
                    ���ݽڵ�_���.Object = ���������

                    �ڵ������.AddNode(���ݽڵ�_����)
                    �ڵ������.AddNode(���ݽڵ�_���)
                    ���ݽڵ�_����.ConnectOutput(0, ���ݽڵ�_���, 0)

                    ���������.Dispose()
                    ý������������.Dispose()
                End If
                ����������.Dispose()
                ���������� = Nothing
                ���ݽڵ�_����.Dispose()
                ���ݽڵ�_���� = Nothing
                ���ݽڵ�_���.Dispose()
                ���ݽڵ�_��� = Nothing
            Next
            ������.SetTopology(SharpDX.MediaFoundation.SessionSetTopologyFlags.None, �ڵ������)
        Catch ex As Exception
            If ������ IsNot Nothing Then
                ������.Dispose()
                ������ = Nothing
            End If
            If ������������ IsNot Nothing Then
                ������������.Dispose()
                ������������ = Nothing
            End If
            If ��Ƶ��ʾ������ IsNot Nothing Then
                ��Ƶ��ʾ������.Dispose()
                ��Ƶ��ʾ������ = Nothing
            End If
            If ý������Դ IsNot Nothing Then
                ý������Դ.Shutdown()
                ý������Դ.Dispose()
                ý������Դ = Nothing
            End If
            Call �����¼�(��������_�¼�.ý��ر�)
            Return
        Finally
            If ���������� IsNot Nothing Then ����������.Dispose()
            If ���ݽڵ�_���� IsNot Nothing Then ���ݽڵ�_����.Dispose()
            If ���ݽڵ�_��� IsNot Nothing Then ���ݽڵ�_���.Dispose()
            If �ܲ������� IsNot Nothing Then �ܲ�������.Dispose()
            If �ڵ������ IsNot Nothing Then �ڵ������.Dispose()
            If ý������ IsNot Nothing Then ý������.Dispose()
        End Try
        Call �����¼�(��������_�¼�.ý������ɹ�)
        Call ����()
    End Sub

    Friend Sub ���������ļ�(Optional ByVal �����ļ���ַ As String = Nothing)
        If String.IsNullOrEmpty(�����ļ���ַ) = True Then
            If String.IsNullOrEmpty(�ļ���ַ) = True Then Return
        Else
            �ļ���ַ = �����ļ���ַ
        End If
        �������ļ� = True
        If ������ IsNot Nothing Then
            If ״̬1 = ��������_״̬.���� Then ������.Stop()
            ������.Close()
            ������.Dispose()
            ������ = Nothing
        End If
        If ������������ IsNot Nothing Then
            ������������.Dispose()
            ������������ = Nothing
        End If
        If ��Ƶ��ʾ������ IsNot Nothing Then
            ��Ƶ��ʾ������.Dispose()
            ��Ƶ��ʾ������ = Nothing
        End If
        If ý������Դ IsNot Nothing Then
            ý������Դ.Shutdown()
            ý������Դ.Dispose()
            ý������Դ = Nothing
        End If
        ״̬1 = ��������_״̬.�ر�
        ����Ƶ = False
        Call �����¼�(��������_�¼�.���ڴ�ý��)
        Try
            If ������ý������� = False Then
                MF.MediaManager.Startup()
                ������ý������� = True
            End If
            MF.MediaFactory.CreateMediaSession(Nothing, ������)
            ������.BeginGetEvent(Me, Nothing)

            �߳� = New Thread(New ThreadStart(AddressOf ��������))
            �߳�.Start()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub ��������()
        Dim ���Դ��� As Byte
��1:
        Try
            ý������ = New MF.SourceResolver
            Dim �������� As MF.ObjectType = SharpDX.MediaFoundation.ObjectType.Invalid
            CO = ý������.CreateObjectFromURL(�ļ���ַ, MF.SourceResolverFlags.MediaSource, ��������)
            If �������� <> SharpDX.MediaFoundation.ObjectType.MediaSource Then Return
            ý������ = Nothing
            Call ���������ļ�2()
        Catch ex As Exception
            If ý������ IsNot Nothing Then ý������.Dispose()
            If ���Դ��� < 2 Then
                ���Դ��� += 1
                GoTo ��1
            End If
            If ������ IsNot Nothing Then
                ������.Dispose()
                ������ = Nothing
            End If
            Call �����¼�(��������_�¼�.ý��ر�)
        End Try
    End Sub

    Private Sub ���������ļ�2()
        If �߳� IsNot Nothing Then �߳� = Nothing
        If ��Ƶ���Ŵ���.InvokeRequired Then
            Dim d As New ���������ļ�2_���߳�(AddressOf ���������ļ�2)
            ��Ƶ���Ŵ���.Invoke(d, New Object() {})
        Else
            Dim �ڵ������ As MF.Topology = Nothing
            Dim �ܲ������� As MF.PresentationDescriptor = Nothing
            Dim ���������� As MF.StreamDescriptor = Nothing
            Dim ���ݽڵ�_���� As MF.TopologyNode = Nothing
            Dim ���ݽڵ�_��� As MF.TopologyNode = Nothing
            Try
                ý������Դ = New MF.MediaSource(CO)
                MF.MediaFactory.CreateTopology(�ڵ������)
                ý������Դ.CreatePresentationDescriptor(�ܲ�������)
                ʱ��1 = �ܲ�������.Get(MF.PresentationDescriptionAttributeKeys.Duration)
                Dim ѡ�� As Boolean
                Dim I As Integer
                For I = 0 To �ܲ�������.StreamDescriptorCount - 1
                    ѡ�� = False
                    �ܲ�������.GetStreamDescriptorByIndex(I, ѡ��, ����������)
                    If ѡ�� = True Then
                        MF.MediaFactory.CreateTopologyNode(SharpDX.MediaFoundation.TopologyType.SourceStreamNode, ���ݽڵ�_����)
                        ���ݽڵ�_����.Set(MF.TopologyNodeAttributeKeys.Source, ý������Դ)
                        ���ݽڵ�_����.Set(MF.TopologyNodeAttributeKeys.PresentationDescriptor, �ܲ�������)
                        ���ݽڵ�_����.Set(MF.TopologyNodeAttributeKeys.StreamDescriptor, ����������)

                        Dim ý������������ As MF.MediaTypeHandler = ����������.MediaTypeHandler
                        Dim ������ As Guid = ý������������.MajorType
                        Dim ��������� As MF.Activate = Nothing
                        If ������ = MF.MediaTypeGuids.Audio Then
                            MF.MediaFactory.CreateAudioRendererActivate(���������)
                        ElseIf ������ = MF.MediaTypeGuids.Video Then
                            MF.MediaFactory.CreateVideoRendererActivate(��Ƶ���Ŵ���.Handle, ���������)
                            ����Ƶ = True
                        Else
                            Return
                        End If

                        MF.MediaFactory.CreateTopologyNode(SharpDX.MediaFoundation.TopologyType.OutputNode, ���ݽڵ�_���)
                        ���ݽڵ�_���.Object = ���������

                        �ڵ������.AddNode(���ݽڵ�_����)
                        �ڵ������.AddNode(���ݽڵ�_���)
                        ���ݽڵ�_����.ConnectOutput(0, ���ݽڵ�_���, 0)

                        ���������.Dispose()
                        ý������������.Dispose()
                    End If
                    ����������.Dispose()
                    ���������� = Nothing
                    ���ݽڵ�_����.Dispose()
                    ���ݽڵ�_���� = Nothing
                    ���ݽڵ�_���.Dispose()
                    ���ݽڵ�_��� = Nothing
                Next
                ������.SetTopology(SharpDX.MediaFoundation.SessionSetTopologyFlags.None, �ڵ������)
            Catch ex As Exception
                If ������ IsNot Nothing Then
                    ������.Dispose()
                    ������ = Nothing
                End If
                If ������������ IsNot Nothing Then
                    ������������.Dispose()
                    ������������ = Nothing
                End If
                If ��Ƶ��ʾ������ IsNot Nothing Then
                    ��Ƶ��ʾ������.Dispose()
                    ��Ƶ��ʾ������ = Nothing
                End If
                If ý������Դ IsNot Nothing Then
                    ý������Դ.Shutdown()
                    ý������Դ.Dispose()
                    ý������Դ = Nothing
                End If
                Call �����¼�(��������_�¼�.ý��ر�)
                Return
            Finally
                If ���������� IsNot Nothing Then ����������.Dispose()
                If ���ݽڵ�_���� IsNot Nothing Then ���ݽڵ�_����.Dispose()
                If ���ݽڵ�_��� IsNot Nothing Then ���ݽڵ�_���.Dispose()
                If �ܲ������� IsNot Nothing Then �ܲ�������.Dispose()
                If �ڵ������ IsNot Nothing Then �ڵ������.Dispose()
            End Try
            Call �����¼�(��������_�¼�.ý������ɹ�)
            Call ����()
        End If
    End Sub

    Friend Sub ����()
        If ������ IsNot Nothing Then
            Dim ��ʼλ�� As SharpDX.Win32.Variant
            ������.Start(Nothing, ��ʼλ��)
        End If
    End Sub

    Friend Sub ��ͣ()
        If ������ IsNot Nothing Then
            If �Ƿ����ֵ(������.SessionCapabilities, 4, 1) = True Then ������.Pause()
        End If
    End Sub

    Private Function �Ƿ����ֵ(ByVal ��ֵ As Integer, ByRef ���ҵ�ֵ As Integer, ByRef ȡģֵ As Integer) As Boolean
        If ��ֵ = ���ҵ�ֵ Then
            Return True
        ElseIf ��ֵ > ���ҵ�ֵ Then
            ȡģֵ *= 2
            Dim ģ As Integer = ��ֵ Mod ȡģֵ
            If ģ = ���ҵ�ֵ Then
                Return True
            Else
                If ģ <> 0 Then ��ֵ -= ģ
                If ��ֵ > ���ҵ�ֵ Then
                    Return �Ƿ����ֵ(��ֵ, ���ҵ�ֵ, ȡģֵ)
                ElseIf ��ֵ = ���ҵ�ֵ Then
                    Return True
                End If
            End If
        End If
        Return False
    End Function

    Friend Function ֹͣ() As Boolean
        If �߳� IsNot Nothing Then Return False
        If ������ IsNot Nothing Then
            ������.Stop()
            Return True
        Else
            Return False
        End If
    End Function

    Friend Sub ������Ƶ��ʾ��(ByVal ��� As Size)
        If ��Ƶ��ʾ������ IsNot Nothing Then ��Ƶ��ʾ������.SetVideoPosition(Nothing, New SharpDX.Rectangle(0, 0, ���.Width, ���.Height))
    End Sub

    Friend Sub �ػ�()
        If ��Ƶ��ʾ������ IsNot Nothing Then ��Ƶ��ʾ������.RepaintVideo()
    End Sub

    Private Sub �����¼�(ByVal �¼� As ��������_�¼�)
        If ��Ƶ���Ŵ���.IsDisposed = True Then Return
        If ��Ƶ���Ŵ���.InvokeRequired Then
            Dim d As New �����¼�_���߳�(AddressOf �����¼�)
            ��Ƶ���Ŵ���.Invoke(d, New Object() {�¼�})
        Else
            Select Case �¼�
                Case ��������_�¼�.���ڴ�ý�� : RaiseEvent ���ڴ�ý��(Me)
                Case ��������_�¼�.ý������ɹ� : RaiseEvent ý������ɹ�(Me)
                Case ��������_�¼�.���ſ�ʼ : RaiseEvent ���ſ�ʼ(Me)
                Case ��������_�¼�.������ͣ : RaiseEvent ������ͣ(Me)
                Case ��������_�¼�.����ֹͣ_��Ϊ : RaiseEvent ����ֹͣ_��Ϊ(Me)
                Case ��������_�¼�.����ֹͣ_��� : RaiseEvent ����ֹͣ_���(Me)
                Case ��������_�¼�.ý��ر� : RaiseEvent ý��ر�(Me)
            End Select
        End If
    End Sub

    Friend Sub �ٴβ���()
        If ��ʱ�� Is Nothing Then
            ��ʱ�� = New System.Windows.Forms.Timer
            ��ʱ��.Interval = 1000
        Else
            If ��ʱ��.Enabled = True Then ��ʱ��.Stop()
        End If
        ��ʱ��.Start()
    End Sub

    Private Sub ��ʱ��_Tick(sender As Object, e As EventArgs) Handles ��ʱ��.Tick
        ��ʱ��.Stop()
        If �������ļ� = False Then
            Call ���ű����ļ�()
        Else
            Call ���������ļ�()
        End If
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' �������ĵ���

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                If ��ʱ�� IsNot Nothing Then
                    ��ʱ��.Dispose()
                    ��ʱ�� = Nothing
                End If
            End If
            Try
                If �߳� IsNot Nothing Then
                    �߳�.Abort()
                    �߳� = Nothing
                End If
                If ý������ IsNot Nothing Then
                    ý������.Dispose()
                    ý������ = Nothing
                End If
            Catch ex As Exception
            End Try
            If ������ IsNot Nothing Then
                ������.Stop()
                ������.Close()
                ������.Dispose()
                ������ = Nothing
            End If
            If ������������ IsNot Nothing Then
                ������������.Dispose()
                ������������ = Nothing
            End If
            If ��Ƶ��ʾ������ IsNot Nothing Then
                ��Ƶ��ʾ������.Dispose()
                ��Ƶ��ʾ������ = Nothing
            End If
            If ý������Դ IsNot Nothing Then
                ý������Դ.Shutdown()
                ý������Դ.Dispose()
                ý������Դ = Nothing
            End If
            ' TODO:  �ͷŷ��й���Դ(���йܶ���)����д����� Finalize()��
            ' TODO:  �������ֶ�����Ϊ null��
        End If
        Me.disposedValue = True
    End Sub

    ' TODO:  ��������� Dispose(ByVal disposing As Boolean)�����ͷŷ��й���Դ�Ĵ���ʱ��д Finalize()��
    Protected Overrides Sub Finalize()
        ' ��Ҫ���Ĵ˴��롣    �뽫��������������� Dispose(ByVal disposing As Boolean)�С�
        Dispose(False)
        MyBase.Finalize()
    End Sub

    ' Visual Basic ��Ӵ˴�����Ϊ����ȷʵ�ֿɴ���ģʽ��
    Friend Sub Dispose() Implements IDisposable.Dispose
        ' ��Ҫ���Ĵ˴��롣    �뽫��������������� Dispose (disposing As Boolean)�С�
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

End Class
