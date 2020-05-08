
Imports AxWMPLib

Public Class 窗体_播放视频

    Dim 视频路径 As String

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(ByVal 视频路径1 As String)
        InitializeComponent()
        视频路径 = 视频路径1
    End Sub

    Private Sub 窗体_播放视频_Load(sender As Object, e As EventArgs) Handles Me.Load
        文字_关闭.Text = 界面文字.获取(22, 文字_关闭.Text)
        媒体播放器.uiMode = "none"
        媒体播放器.enableContextMenu = False
        媒体播放器.stretchToFit = True
        媒体播放器.URL = 视频路径
    End Sub

    Private Sub 媒体播放器_PlayStateChange(sender As Object, e As _WMPOCXEvents_PlayStateChangeEvent) Handles 媒体播放器.PlayStateChange
        If e.newState = 1 Then Me.Close()
    End Sub

    Private Sub 文字_关闭_Click(sender As Object, e As EventArgs) Handles 文字_关闭.Click
        Me.Close()
    End Sub

    Private Sub 文字_关闭_MouseEnter(sender As Object, e As EventArgs) Handles 文字_关闭.MouseEnter
        文字_关闭.ForeColor = Color.White
    End Sub

    Private Sub 文字_关闭_MouseLeave(sender As Object, e As EventArgs) Handles 文字_关闭.MouseLeave
        文字_关闭.ForeColor = Color.Gray
    End Sub

End Class
