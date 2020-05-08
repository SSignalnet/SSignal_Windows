
Public Class 控件_进度条

#Region "变量声明"

    Dim 当前值1, 最大值1, 上次, 最小变量 As Integer
    Dim 提示文字1 As String
    Dim rect As Rectangle
    Dim 格式 As StringFormat
    Dim 画笔 As SolidBrush

    Public Sub New()
        InitializeComponent()
        格式 = New StringFormat(StringFormat.GenericTypographic)
        格式.Alignment = StringAlignment.Center
        格式.LineAlignment = StringAlignment.Center
        画笔 = New SolidBrush(Color.Gray)
    End Sub

#End Region

#Region "属性"

    Public Property 当前值() As Integer
        Get
            Return 当前值1
        End Get
        Set(ByVal Value As Integer)
            If Value >= 0 And Value <= 最大值1 + 1 Then
                当前值1 = Value
                If 最大值1 = 0 Then Exit Property
                If (System.Math.Abs(当前值1 - 上次) * Me.Width) / 最大值1 >= 最小变量 Then
                    Call 调整矩形()
                    上次 = 当前值1
                End If
            End If
        End Set
    End Property

    Public Property 最大值() As Integer
        Get
            Return 最大值1
        End Get
        Set(ByVal Value As Integer)
            If Value <> 最大值1 Then
                If Value > 当前值1 Then
                    最大值1 = Value
                Else
                    最大值1 = 当前值1
                End If
                If 最大值1 = 0 Then Exit Property
                最小变量 = Me.Width / 最大值1
                If 最小变量 < 1 Then 最小变量 = 1
                Call 调整矩形()
            End If
        End Set
    End Property

    Public Property 无边框() As Boolean
        Get
            If Me.BorderStyle = BorderStyle.None Then
                Return True
            Else
                Return False
            End If
        End Get
        Set(ByVal value As Boolean)
            If value = True Then
                Me.BorderStyle = BorderStyle.None
            Else
                Me.BorderStyle = BorderStyle.FixedSingle
            End If
        End Set
    End Property

    Public Property 提示文字() As String
        Get
            Return 提示文字1
        End Get
        Set(ByVal Value As String)
            提示文字1 = Value
            Me.Invalidate()
        End Set
    End Property

#End Region

#Region "绘图"

    Private Sub 调整矩形()
        If 最大值1 > 0 Then
            rect = New Rectangle(0, 0, (当前值1 / 最大值1) * Me.Width, Me.Height)
            Me.Invalidate()
        End If
    End Sub

    Private Sub 进度条控件_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        If 最大值1 = 0 Then Exit Sub
        e.Graphics.FillRectangle(New SolidBrush(Me.ForeColor), rect)
        If String.IsNullOrEmpty(提示文字1) = False Then
            e.Graphics.DrawString(提示文字1, Me.Font, 画笔, New Rectangle(0, 0, Width, Height), 格式)
        End If
    End Sub

    Private Sub 控件_进度条_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Call 调整矩形()
    End Sub

#End Region

End Class
