
Public Class �ؼ�_������

#Region "��������"

    Dim ��ǰֵ1, ���ֵ1, �ϴ�, ��С���� As Integer
    Dim ��ʾ����1 As String
    Dim rect As Rectangle
    Dim ��ʽ As StringFormat
    Dim ���� As SolidBrush

    Public Sub New()
        InitializeComponent()
        ��ʽ = New StringFormat(StringFormat.GenericTypographic)
        ��ʽ.Alignment = StringAlignment.Center
        ��ʽ.LineAlignment = StringAlignment.Center
        ���� = New SolidBrush(Color.Gray)
    End Sub

#End Region

#Region "����"

    Public Property ��ǰֵ() As Integer
        Get
            Return ��ǰֵ1
        End Get
        Set(ByVal Value As Integer)
            If Value >= 0 And Value <= ���ֵ1 + 1 Then
                ��ǰֵ1 = Value
                If ���ֵ1 = 0 Then Exit Property
                If (System.Math.Abs(��ǰֵ1 - �ϴ�) * Me.Width) / ���ֵ1 >= ��С���� Then
                    Call ��������()
                    �ϴ� = ��ǰֵ1
                End If
            End If
        End Set
    End Property

    Public Property ���ֵ() As Integer
        Get
            Return ���ֵ1
        End Get
        Set(ByVal Value As Integer)
            If Value <> ���ֵ1 Then
                If Value > ��ǰֵ1 Then
                    ���ֵ1 = Value
                Else
                    ���ֵ1 = ��ǰֵ1
                End If
                If ���ֵ1 = 0 Then Exit Property
                ��С���� = Me.Width / ���ֵ1
                If ��С���� < 1 Then ��С���� = 1
                Call ��������()
            End If
        End Set
    End Property

    Public Property �ޱ߿�() As Boolean
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

    Public Property ��ʾ����() As String
        Get
            Return ��ʾ����1
        End Get
        Set(ByVal Value As String)
            ��ʾ����1 = Value
            Me.Invalidate()
        End Set
    End Property

#End Region

#Region "��ͼ"

    Private Sub ��������()
        If ���ֵ1 > 0 Then
            rect = New Rectangle(0, 0, (��ǰֵ1 / ���ֵ1) * Me.Width, Me.Height)
            Me.Invalidate()
        End If
    End Sub

    Private Sub �������ؼ�_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        If ���ֵ1 = 0 Then Exit Sub
        e.Graphics.FillRectangle(New SolidBrush(Me.ForeColor), rect)
        If String.IsNullOrEmpty(��ʾ����1) = False Then
            e.Graphics.DrawString(��ʾ����1, Me.Font, ����, New Rectangle(0, 0, Width, Height), ��ʽ)
        End If
    End Sub

    Private Sub �ؼ�_������_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Call ��������()
    End Sub

#End Region

End Class
