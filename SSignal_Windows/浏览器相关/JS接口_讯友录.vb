Public Class JS接口_讯友录

    Dim 主窗体1 As 主窗体

    Private Delegate Sub 跨线程0()
    Private Delegate Sub 跨线程1(ByVal id As String)

    Public Sub New(ByVal 主窗体2 As 主窗体)
        主窗体1 = 主窗体2
    End Sub

    Public Sub ClickAContact(ByVal id As String)
        If 主窗体1.InvokeRequired Then
            Dim d As New 跨线程1(AddressOf ClickAContact)
            主窗体1.Invoke(d, New Object() {id})
        Else
            主窗体1.点击某一讯友(id)
        End If
    End Sub

    Public Sub SelectRange()
        If 主窗体1.InvokeRequired Then
            Dim d As New 跨线程0(AddressOf SelectRange)
            主窗体1.Invoke(d, New Object() {})
        Else
            主窗体1.显示可选范围()
        End If
    End Sub

    Public Sub ClickARange(ByVal id As String)
        If 主窗体1.InvokeRequired Then
            Dim d As New 跨线程1(AddressOf ClickARange)
            主窗体1.Invoke(d, New Object() {id})
        Else
            主窗体1.点击某一范围(id)
        End If
    End Sub

End Class
