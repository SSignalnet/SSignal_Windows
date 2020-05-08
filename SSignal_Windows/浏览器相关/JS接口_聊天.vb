Public Class JS接口_聊天

    Dim 聊天控件 As 控件_聊天

    Private Delegate Sub 跨线程0()
    Private Delegate Sub 跨线程1(ByVal Text As String)
    Private Delegate Sub 跨线程3(ByVal Command As String, ByVal Parameter1 As String, ByVal Parameter2 As String)

    Public Sub New(ByVal 聊天控件2 As 控件_聊天)
        聊天控件 = 聊天控件2
    End Sub

    Public Sub PlayVoice(ByVal VoiceSrc As String, ByVal VoiceID As String, ByVal IsNew As String)
        If 聊天控件.InvokeRequired Then
            Dim d As New 跨线程3(AddressOf PlayVoice)
            聊天控件.Invoke(d, New Object() {VoiceSrc, VoiceID, IsNew})
        Else
            聊天控件.主窗体1.播放语音(聊天控件, VoiceSrc, VoiceID, IsNew)
        End If
    End Sub

    Public Sub ToRobot(ByVal Text As String)
        If 聊天控件.InvokeRequired Then
            Dim d As New 跨线程1(AddressOf ToRobot)
            聊天控件.Invoke(d, New Object() {Text})
        Else
            聊天控件.对机器人说(Text)
        End If
    End Sub

    Public Sub ToRobot2(ByVal Command As String, ByVal Parameter1 As String, ByVal Parameter2 As String)
        If 聊天控件.InvokeRequired Then
            Dim d As New 跨线程3(AddressOf ToRobot2)
            聊天控件.Invoke(d, New Object() {Command, Parameter1, Parameter2})
        Else
            聊天控件.对机器人说2(Command, Parameter1, Parameter2)
        End If
    End Sub

    Public Sub ReachTop()
        If 聊天控件.InvokeRequired Then
            Dim d As New 跨线程0(AddressOf ReachTop)
            聊天控件.Invoke(d, New Object() {})
        Else
            聊天控件.滚动至顶部()
        End If
    End Sub

End Class
