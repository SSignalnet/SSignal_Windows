Imports System.Threading
Imports MF = SharpDX.MediaFoundation

Friend Class 类_视频_播放
    Implements MF.IAsyncCallback

#Region "定义和声明"

    Friend Enum 常量集合_状态 As Byte
        关闭 = 0
        播放 = 1
        暂停 = 2
        停止 = 3
    End Enum

    Friend Enum 常量集合_事件 As Byte
        正在打开媒体 = 0
        媒体载入成功 = 1
        播放开始 = 2
        播放暂停 = 3
        播放停止_人为 = 4
        播放停止_完毕 = 5
        媒体关闭 = 6
    End Enum

    Dim 状态1 As 常量集合_状态

    Dim 视频播放窗体 As Control
    Dim 播放器 As MF.MediaSession
    Dim 媒体数据源 As MF.MediaSource
    Dim 流音量控制器 As MF.AudioStreamVolume
    Dim 视频显示控制器 As MF.VideoDisplayControl
    Dim 媒体解读器 As MF.SourceResolver
    Dim CO As SharpDX.ComObject
    Dim 时长1 As Long
    Dim 音量1 As Single = 1.0
    Dim 是视频, 是网络文件 As Boolean
    Dim 线程 As Thread
    Dim 文件地址 As String
    Friend 当前播放时间点 As Long
    WithEvents 定时器 As System.Windows.Forms.Timer

    Friend Event 正在打开媒体(ByVal sender As Object)
    Friend Event 媒体载入成功(ByVal sender As Object)
    Friend Event 播放开始(ByVal sender As Object)
    Friend Event 播放暂停(ByVal sender As Object)
    Friend Event 播放停止_人为(ByVal sender As Object)
    Friend Event 播放停止_完毕(ByVal sender As Object)
    Friend Event 媒体关闭(ByVal sender As Object)

    Private Delegate Sub 触发事件_跨线程(ByVal 事件 As 常量集合_事件)
    Private Delegate Sub 播放网络文件2_跨线程()

#End Region

    Friend Property Shadow As IDisposable Implements SharpDX.ICallbackable.Shadow

    Friend ReadOnly Property Flags As SharpDX.MediaFoundation.AsyncCallbackFlags Implements SharpDX.MediaFoundation.IAsyncCallback.Flags
        Get

        End Get
    End Property

    Friend Sub Invoke(asyncResultRef As SharpDX.MediaFoundation.AsyncResult) Implements SharpDX.MediaFoundation.IAsyncCallback.Invoke
        Dim 事件 As MF.MediaEvent
        Try
            事件 = 播放器.EndGetEvent(asyncResultRef)
        Catch ex As Exception
            Return
        End Try
        If 事件.TypeInfo = SharpDX.MediaFoundation.MediaEventTypes.SessionClosed Then

        Else
            Try
                播放器.BeginGetEvent(Me, Nothing)
            Catch ex As Exception
                Return
            End Try
        End If
        Select Case 事件.TypeInfo
            Case SharpDX.MediaFoundation.MediaEventTypes.SessionStarted
                If 流音量控制器 Is Nothing Then
                    Dim CO As New SharpDX.ComObject(播放器.NativePointer)
                    Dim GUID As New Guid("76B1BBDB-4EC8-4f36-B106-70A9316DF593")
                    Dim Ptr As IntPtr
                    Try
                        MF.MediaFactory.GetService(CO, MF.MediaServiceKeys.StreamVolume, GUID, Ptr)
                        流音量控制器 = New MF.AudioStreamVolume(Ptr)
                        If 流音量控制器.ChannelCount > 0 Then
                            Dim 声道音量(流音量控制器.ChannelCount - 1) As Single
                            Dim I As Integer
                            For I = 0 To 声道音量.Length - 1
                                声道音量(I) = 音量1
                            Next
                            流音量控制器.SetAllVolumes(流音量控制器.ChannelCount, 声道音量)
                        Else
                            流音量控制器.Dispose()
                            流音量控制器 = Nothing
                        End If
                    Catch ex As Exception
                    End Try
                End If
                If 是视频 = True AndAlso 视频显示控制器 Is Nothing Then
                    Dim CO As New SharpDX.ComObject(播放器.NativePointer)
                    Dim GUID1 As New Guid("{0x1092a86c, 0xab1a, 0x459a, {0xa3, 0x36, 0x83, 0x1f, 0xbc, 0x4d, 0x11, 0xff}}")
                    Dim GUID2 As New Guid("a490b1e4-ab84-4d31-a1b2-181e03b1077a")
                    Dim Ptr As New IntPtr
                    Try
                        MF.MediaFactory.GetService(CO, GUID1, GUID2, Ptr)
                        视频显示控制器 = New MF.VideoDisplayControl(Ptr)
                    Catch ex As Exception
                    End Try
                End If
                If 状态1 <> 常量集合_状态.播放 Then
                    状态1 = 常量集合_状态.播放
                    Call 触发事件(常量集合_事件.播放开始)
                End If
            Case SharpDX.MediaFoundation.MediaEventTypes.SessionStopped
                If 状态1 <> 常量集合_状态.停止 Then
                    状态1 = 常量集合_状态.停止
                    Call 触发事件(常量集合_事件.播放停止_人为)
                End If
            Case SharpDX.MediaFoundation.MediaEventTypes.SessionPaused
                If 状态1 <> 常量集合_状态.暂停 Then
                    状态1 = 常量集合_状态.暂停
                    Call 触发事件(常量集合_事件.播放暂停)
                End If
            Case SharpDX.MediaFoundation.MediaEventTypes.SessionEnded
                If 状态1 <> 常量集合_状态.停止 Then
                    状态1 = 常量集合_状态.停止
                    Call 触发事件(常量集合_事件.播放停止_完毕)
                End If
            Case SharpDX.MediaFoundation.MediaEventTypes.SessionClosed
                If 状态1 <> 常量集合_状态.停止 Then
                    状态1 = 常量集合_状态.停止
                    Call 触发事件(常量集合_事件.媒体关闭)
                End If
        End Select
        事件.Dispose()
    End Sub

    Friend ReadOnly Property WorkQueueId As SharpDX.MediaFoundation.WorkQueueId Implements SharpDX.MediaFoundation.IAsyncCallback.WorkQueueId
        Get

        End Get
    End Property

    Friend Sub New(ByVal 视频播放窗体1 As Control)
        视频播放窗体 = 视频播放窗体1
    End Sub

    Friend ReadOnly Property 时长 As Double
        Get
            Return 时长1 / 1000 / 10000
        End Get
    End Property

    Friend ReadOnly Property 播放进度 As Single
        Get
            If 时长1 > 0 Then
                Try
                    播放器.Clock.GetCorrelatedTime(0, 当前播放时间点, 0)
                Catch ex As Exception
                    Return 0
                End Try
                Return 当前播放时间点 / 时长1
            Else
                Return 0
            End If
        End Get
    End Property

    Friend ReadOnly Property 状态 As 常量集合_状态
        Get
            Return 状态1
        End Get
    End Property

    Friend Property 音量 As Single
        Get
            Return 音量1
        End Get
        Set(value As Single)
            If value < 0 Then
                音量1 = 0
            ElseIf value > 1.0 Then
                音量1 = 1.0
            Else
                音量1 = value
            End If
            If 流音量控制器 IsNot Nothing Then
                If 流音量控制器.ChannelCount > 0 Then
                    Dim 声道音量(流音量控制器.ChannelCount - 1) As Single
                    Dim I As Integer
                    For I = 0 To 声道音量.Length - 1
                        声道音量(I) = 音量1
                    Next
                    流音量控制器.SetAllVolumes(流音量控制器.ChannelCount, 声道音量)
                End If
            End If
        End Set
    End Property

    'Friend Property 全屏 As Boolean
    '    Get
    '        If 视频显示控制器 IsNot Nothing Then
    '            Return 视频显示控制器.Fullscreen
    '        Else
    '            Return False
    '        End If
    '    End Get
    '    Set(value As Boolean)
    '        If 视频显示控制器 IsNot Nothing Then 视频显示控制器.Fullscreen = value
    '    End Set
    'End Property

    Friend Sub 播放本地文件(Optional ByVal 本地文件路径 As String = Nothing)
        If String.IsNullOrEmpty(本地文件路径) = True Then
            If String.IsNullOrEmpty(文件地址) = True Then Return
        Else
            文件地址 = 本地文件路径
        End If
        是网络文件 = False
        If 播放器 IsNot Nothing Then
            播放器.Stop()
            播放器.Close()
            播放器.Dispose()
            播放器 = Nothing
        End If
        If 流音量控制器 IsNot Nothing Then
            流音量控制器.Dispose()
            流音量控制器 = Nothing
        End If
        If 视频显示控制器 IsNot Nothing Then
            视频显示控制器.Dispose()
            视频显示控制器 = Nothing
        End If
        If 媒体数据源 IsNot Nothing Then
            媒体数据源.Shutdown()
            媒体数据源.Dispose()
            媒体数据源 = Nothing
        End If
        状态1 = 常量集合_状态.关闭
        是视频 = False
        Dim 媒体解读器 As MF.SourceResolver = Nothing
        Dim 节点关联器 As MF.Topology = Nothing
        Dim 总播放设置 As MF.PresentationDescriptor = Nothing
        Dim 流播放设置 As MF.StreamDescriptor = Nothing
        Dim 数据节点_输入 As MF.TopologyNode = Nothing
        Dim 数据节点_输出 As MF.TopologyNode = Nothing
        Call 触发事件(常量集合_事件.正在打开媒体)
        Try
            If 开启了媒体管理器 = False Then
                MF.MediaManager.Startup()
                开启了媒体管理器 = True
            End If
            MF.MediaFactory.CreateMediaSession(Nothing, 播放器)
            播放器.BeginGetEvent(Me, Nothing)

            媒体解读器 = New MF.SourceResolver
            Dim 对象类型 As MF.ObjectType = SharpDX.MediaFoundation.ObjectType.Invalid
            Dim CO As SharpDX.ComObject = 媒体解读器.CreateObjectFromURL(文件地址, MF.SourceResolverFlags.MediaSource, 对象类型)
            If 对象类型 <> SharpDX.MediaFoundation.ObjectType.MediaSource Then Return
            媒体数据源 = New MF.MediaSource(CO)

            MF.MediaFactory.CreateTopology(节点关联器)
            媒体数据源.CreatePresentationDescriptor(总播放设置)
            时长1 = 总播放设置.Get(MF.PresentationDescriptionAttributeKeys.Duration)
            Dim 选中 As Boolean
            Dim I As Integer
            For I = 0 To 总播放设置.StreamDescriptorCount - 1
                选中 = False
                总播放设置.GetStreamDescriptorByIndex(I, 选中, 流播放设置)
                If 选中 = True Then
                    MF.MediaFactory.CreateTopologyNode(SharpDX.MediaFoundation.TopologyType.SourceStreamNode, 数据节点_输入)
                    数据节点_输入.Set(MF.TopologyNodeAttributeKeys.Source, 媒体数据源)
                    数据节点_输入.Set(MF.TopologyNodeAttributeKeys.PresentationDescriptor, 总播放设置)
                    数据节点_输入.Set(MF.TopologyNodeAttributeKeys.StreamDescriptor, 流播放设置)

                    Dim 媒体类型引用器 As MF.MediaTypeHandler = 流播放设置.MediaTypeHandler
                    Dim 总类型 As Guid = 媒体类型引用器.MajorType
                    Dim 输出关联器 As MF.Activate = Nothing
                    If 总类型 = MF.MediaTypeGuids.Audio Then
                        MF.MediaFactory.CreateAudioRendererActivate(输出关联器)
                    ElseIf 总类型 = MF.MediaTypeGuids.Video Then
                        MF.MediaFactory.CreateVideoRendererActivate(视频播放窗体.Handle, 输出关联器)
                        是视频 = True
                    Else
                        Return
                    End If

                    MF.MediaFactory.CreateTopologyNode(SharpDX.MediaFoundation.TopologyType.OutputNode, 数据节点_输出)
                    数据节点_输出.Object = 输出关联器

                    节点关联器.AddNode(数据节点_输入)
                    节点关联器.AddNode(数据节点_输出)
                    数据节点_输入.ConnectOutput(0, 数据节点_输出, 0)

                    输出关联器.Dispose()
                    媒体类型引用器.Dispose()
                End If
                流播放设置.Dispose()
                流播放设置 = Nothing
                数据节点_输入.Dispose()
                数据节点_输入 = Nothing
                数据节点_输出.Dispose()
                数据节点_输出 = Nothing
            Next
            播放器.SetTopology(SharpDX.MediaFoundation.SessionSetTopologyFlags.None, 节点关联器)
        Catch ex As Exception
            If 播放器 IsNot Nothing Then
                播放器.Dispose()
                播放器 = Nothing
            End If
            If 流音量控制器 IsNot Nothing Then
                流音量控制器.Dispose()
                流音量控制器 = Nothing
            End If
            If 视频显示控制器 IsNot Nothing Then
                视频显示控制器.Dispose()
                视频显示控制器 = Nothing
            End If
            If 媒体数据源 IsNot Nothing Then
                媒体数据源.Shutdown()
                媒体数据源.Dispose()
                媒体数据源 = Nothing
            End If
            Call 触发事件(常量集合_事件.媒体关闭)
            Return
        Finally
            If 流播放设置 IsNot Nothing Then 流播放设置.Dispose()
            If 数据节点_输入 IsNot Nothing Then 数据节点_输入.Dispose()
            If 数据节点_输出 IsNot Nothing Then 数据节点_输出.Dispose()
            If 总播放设置 IsNot Nothing Then 总播放设置.Dispose()
            If 节点关联器 IsNot Nothing Then 节点关联器.Dispose()
            If 媒体解读器 IsNot Nothing Then 媒体解读器.Dispose()
        End Try
        Call 触发事件(常量集合_事件.媒体载入成功)
        Call 播放()
    End Sub

    Friend Sub 播放网络文件(Optional ByVal 网络文件地址 As String = Nothing)
        If String.IsNullOrEmpty(网络文件地址) = True Then
            If String.IsNullOrEmpty(文件地址) = True Then Return
        Else
            文件地址 = 网络文件地址
        End If
        是网络文件 = True
        If 播放器 IsNot Nothing Then
            If 状态1 = 常量集合_状态.播放 Then 播放器.Stop()
            播放器.Close()
            播放器.Dispose()
            播放器 = Nothing
        End If
        If 流音量控制器 IsNot Nothing Then
            流音量控制器.Dispose()
            流音量控制器 = Nothing
        End If
        If 视频显示控制器 IsNot Nothing Then
            视频显示控制器.Dispose()
            视频显示控制器 = Nothing
        End If
        If 媒体数据源 IsNot Nothing Then
            媒体数据源.Shutdown()
            媒体数据源.Dispose()
            媒体数据源 = Nothing
        End If
        状态1 = 常量集合_状态.关闭
        是视频 = False
        Call 触发事件(常量集合_事件.正在打开媒体)
        Try
            If 开启了媒体管理器 = False Then
                MF.MediaManager.Startup()
                开启了媒体管理器 = True
            End If
            MF.MediaFactory.CreateMediaSession(Nothing, 播放器)
            播放器.BeginGetEvent(Me, Nothing)

            线程 = New Thread(New ThreadStart(AddressOf 载入数据))
            线程.Start()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub 载入数据()
        Dim 重试次数 As Byte
行1:
        Try
            媒体解读器 = New MF.SourceResolver
            Dim 对象类型 As MF.ObjectType = SharpDX.MediaFoundation.ObjectType.Invalid
            CO = 媒体解读器.CreateObjectFromURL(文件地址, MF.SourceResolverFlags.MediaSource, 对象类型)
            If 对象类型 <> SharpDX.MediaFoundation.ObjectType.MediaSource Then Return
            媒体解读器 = Nothing
            Call 播放网络文件2()
        Catch ex As Exception
            If 媒体解读器 IsNot Nothing Then 媒体解读器.Dispose()
            If 重试次数 < 2 Then
                重试次数 += 1
                GoTo 行1
            End If
            If 播放器 IsNot Nothing Then
                播放器.Dispose()
                播放器 = Nothing
            End If
            Call 触发事件(常量集合_事件.媒体关闭)
        End Try
    End Sub

    Private Sub 播放网络文件2()
        If 线程 IsNot Nothing Then 线程 = Nothing
        If 视频播放窗体.InvokeRequired Then
            Dim d As New 播放网络文件2_跨线程(AddressOf 播放网络文件2)
            视频播放窗体.Invoke(d, New Object() {})
        Else
            Dim 节点关联器 As MF.Topology = Nothing
            Dim 总播放设置 As MF.PresentationDescriptor = Nothing
            Dim 流播放设置 As MF.StreamDescriptor = Nothing
            Dim 数据节点_输入 As MF.TopologyNode = Nothing
            Dim 数据节点_输出 As MF.TopologyNode = Nothing
            Try
                媒体数据源 = New MF.MediaSource(CO)
                MF.MediaFactory.CreateTopology(节点关联器)
                媒体数据源.CreatePresentationDescriptor(总播放设置)
                时长1 = 总播放设置.Get(MF.PresentationDescriptionAttributeKeys.Duration)
                Dim 选中 As Boolean
                Dim I As Integer
                For I = 0 To 总播放设置.StreamDescriptorCount - 1
                    选中 = False
                    总播放设置.GetStreamDescriptorByIndex(I, 选中, 流播放设置)
                    If 选中 = True Then
                        MF.MediaFactory.CreateTopologyNode(SharpDX.MediaFoundation.TopologyType.SourceStreamNode, 数据节点_输入)
                        数据节点_输入.Set(MF.TopologyNodeAttributeKeys.Source, 媒体数据源)
                        数据节点_输入.Set(MF.TopologyNodeAttributeKeys.PresentationDescriptor, 总播放设置)
                        数据节点_输入.Set(MF.TopologyNodeAttributeKeys.StreamDescriptor, 流播放设置)

                        Dim 媒体类型引用器 As MF.MediaTypeHandler = 流播放设置.MediaTypeHandler
                        Dim 总类型 As Guid = 媒体类型引用器.MajorType
                        Dim 输出关联器 As MF.Activate = Nothing
                        If 总类型 = MF.MediaTypeGuids.Audio Then
                            MF.MediaFactory.CreateAudioRendererActivate(输出关联器)
                        ElseIf 总类型 = MF.MediaTypeGuids.Video Then
                            MF.MediaFactory.CreateVideoRendererActivate(视频播放窗体.Handle, 输出关联器)
                            是视频 = True
                        Else
                            Return
                        End If

                        MF.MediaFactory.CreateTopologyNode(SharpDX.MediaFoundation.TopologyType.OutputNode, 数据节点_输出)
                        数据节点_输出.Object = 输出关联器

                        节点关联器.AddNode(数据节点_输入)
                        节点关联器.AddNode(数据节点_输出)
                        数据节点_输入.ConnectOutput(0, 数据节点_输出, 0)

                        输出关联器.Dispose()
                        媒体类型引用器.Dispose()
                    End If
                    流播放设置.Dispose()
                    流播放设置 = Nothing
                    数据节点_输入.Dispose()
                    数据节点_输入 = Nothing
                    数据节点_输出.Dispose()
                    数据节点_输出 = Nothing
                Next
                播放器.SetTopology(SharpDX.MediaFoundation.SessionSetTopologyFlags.None, 节点关联器)
            Catch ex As Exception
                If 播放器 IsNot Nothing Then
                    播放器.Dispose()
                    播放器 = Nothing
                End If
                If 流音量控制器 IsNot Nothing Then
                    流音量控制器.Dispose()
                    流音量控制器 = Nothing
                End If
                If 视频显示控制器 IsNot Nothing Then
                    视频显示控制器.Dispose()
                    视频显示控制器 = Nothing
                End If
                If 媒体数据源 IsNot Nothing Then
                    媒体数据源.Shutdown()
                    媒体数据源.Dispose()
                    媒体数据源 = Nothing
                End If
                Call 触发事件(常量集合_事件.媒体关闭)
                Return
            Finally
                If 流播放设置 IsNot Nothing Then 流播放设置.Dispose()
                If 数据节点_输入 IsNot Nothing Then 数据节点_输入.Dispose()
                If 数据节点_输出 IsNot Nothing Then 数据节点_输出.Dispose()
                If 总播放设置 IsNot Nothing Then 总播放设置.Dispose()
                If 节点关联器 IsNot Nothing Then 节点关联器.Dispose()
            End Try
            Call 触发事件(常量集合_事件.媒体载入成功)
            Call 播放()
        End If
    End Sub

    Friend Sub 播放()
        If 播放器 IsNot Nothing Then
            Dim 开始位置 As SharpDX.Win32.Variant
            播放器.Start(Nothing, 开始位置)
        End If
    End Sub

    Friend Sub 暂停()
        If 播放器 IsNot Nothing Then
            If 是否包含值(播放器.SessionCapabilities, 4, 1) = True Then 播放器.Pause()
        End If
    End Sub

    Private Function 是否包含值(ByVal 总值 As Integer, ByRef 查找的值 As Integer, ByRef 取模值 As Integer) As Boolean
        If 总值 = 查找的值 Then
            Return True
        ElseIf 总值 > 查找的值 Then
            取模值 *= 2
            Dim 模 As Integer = 总值 Mod 取模值
            If 模 = 查找的值 Then
                Return True
            Else
                If 模 <> 0 Then 总值 -= 模
                If 总值 > 查找的值 Then
                    Return 是否包含值(总值, 查找的值, 取模值)
                ElseIf 总值 = 查找的值 Then
                    Return True
                End If
            End If
        End If
        Return False
    End Function

    Friend Function 停止() As Boolean
        If 线程 IsNot Nothing Then Return False
        If 播放器 IsNot Nothing Then
            播放器.Stop()
            Return True
        Else
            Return False
        End If
    End Function

    Friend Sub 调整视频显示区(ByVal 宽高 As Size)
        If 视频显示控制器 IsNot Nothing Then 视频显示控制器.SetVideoPosition(Nothing, New SharpDX.Rectangle(0, 0, 宽高.Width, 宽高.Height))
    End Sub

    Friend Sub 重绘()
        If 视频显示控制器 IsNot Nothing Then 视频显示控制器.RepaintVideo()
    End Sub

    Private Sub 触发事件(ByVal 事件 As 常量集合_事件)
        If 视频播放窗体.IsDisposed = True Then Return
        If 视频播放窗体.InvokeRequired Then
            Dim d As New 触发事件_跨线程(AddressOf 触发事件)
            视频播放窗体.Invoke(d, New Object() {事件})
        Else
            Select Case 事件
                Case 常量集合_事件.正在打开媒体 : RaiseEvent 正在打开媒体(Me)
                Case 常量集合_事件.媒体载入成功 : RaiseEvent 媒体载入成功(Me)
                Case 常量集合_事件.播放开始 : RaiseEvent 播放开始(Me)
                Case 常量集合_事件.播放暂停 : RaiseEvent 播放暂停(Me)
                Case 常量集合_事件.播放停止_人为 : RaiseEvent 播放停止_人为(Me)
                Case 常量集合_事件.播放停止_完毕 : RaiseEvent 播放停止_完毕(Me)
                Case 常量集合_事件.媒体关闭 : RaiseEvent 媒体关闭(Me)
            End Select
        End If
    End Sub

    Friend Sub 再次播放()
        If 定时器 Is Nothing Then
            定时器 = New System.Windows.Forms.Timer
            定时器.Interval = 1000
        Else
            If 定时器.Enabled = True Then 定时器.Stop()
        End If
        定时器.Start()
    End Sub

    Private Sub 定时器_Tick(sender As Object, e As EventArgs) Handles 定时器.Tick
        定时器.Stop()
        If 是网络文件 = False Then
            Call 播放本地文件()
        Else
            Call 播放网络文件()
        End If
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 检测冗余的调用

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                If 定时器 IsNot Nothing Then
                    定时器.Dispose()
                    定时器 = Nothing
                End If
            End If
            Try
                If 线程 IsNot Nothing Then
                    线程.Abort()
                    线程 = Nothing
                End If
                If 媒体解读器 IsNot Nothing Then
                    媒体解读器.Dispose()
                    媒体解读器 = Nothing
                End If
            Catch ex As Exception
            End Try
            If 播放器 IsNot Nothing Then
                播放器.Stop()
                播放器.Close()
                播放器.Dispose()
                播放器 = Nothing
            End If
            If 流音量控制器 IsNot Nothing Then
                流音量控制器.Dispose()
                流音量控制器 = Nothing
            End If
            If 视频显示控制器 IsNot Nothing Then
                视频显示控制器.Dispose()
                视频显示控制器 = Nothing
            End If
            If 媒体数据源 IsNot Nothing Then
                媒体数据源.Shutdown()
                媒体数据源.Dispose()
                媒体数据源 = Nothing
            End If
            ' TODO:  释放非托管资源(非托管对象)并重写下面的 Finalize()。
            ' TODO:  将大型字段设置为 null。
        End If
        Me.disposedValue = True
    End Sub

    ' TODO:  仅当上面的 Dispose(ByVal disposing As Boolean)具有释放非托管资源的代码时重写 Finalize()。
    Protected Overrides Sub Finalize()
        ' 不要更改此代码。    请将清理代码放入上面的 Dispose(ByVal disposing As Boolean)中。
        Dispose(False)
        MyBase.Finalize()
    End Sub

    ' Visual Basic 添加此代码是为了正确实现可处置模式。
    Friend Sub Dispose() Implements IDisposable.Dispose
        ' 不要更改此代码。    请将清理代码放入上面的 Dispose (disposing As Boolean)中。
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

End Class
