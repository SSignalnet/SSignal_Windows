Public Class 窗体_播放视频1

    Dim 视频文件链接 As String
    WithEvents 播放器 As 类_视频_播放

    Public Sub New(ByVal 视频文件链接1 As String)
        InitializeComponent()
        Me.Icon = My.Resources.icon
        播放器 = New 类_视频_播放(视频播放区)
        视频文件链接 = 视频文件链接1
    End Sub

    Private Sub 窗体_播放视频_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        播放器.播放网络文件(视频文件链接)
    End Sub

    Private Sub 播放器_播放停止_人为(ByVal sender As Object) Handles 播放器.播放停止_人为
        Me.Close()
    End Sub

    Private Sub 播放器_播放停止_完毕(ByVal sender As Object) Handles 播放器.播放停止_完毕
        Me.Close()
    End Sub

    Private Sub 播放器_媒体关闭(ByVal sender As Object) Handles 播放器.媒体关闭
        Me.Close()
    End Sub

    Private Sub 视频播放区_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If 播放器 IsNot Nothing Then 播放器.调整视频显示区(视频播放区.Size)
    End Sub

    Private Sub 窗体_播放视频_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        Select Case e.KeyCode
            Case Keys.Escape : Me.Close()
        End Select
    End Sub

    Private Sub 窗体_播放视频_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        If 播放器 IsNot Nothing Then 播放器.Dispose()
    End Sub

End Class