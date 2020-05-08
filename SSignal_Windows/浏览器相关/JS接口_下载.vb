Imports System.IO

Public Class JS接口_下载

    Dim 下载文件的窗体 As 窗体_下载文件

    Private Delegate Sub 跨线程1(ByVal Text As String)

    Public Sub New(ByVal 下载文件的窗体1 As 窗体_下载文件)
        下载文件的窗体 = 下载文件的窗体1
    End Sub

    Public Sub Cancel(ByVal id As String)
        If 下载文件的窗体.InvokeRequired Then
            Dim d As New 跨线程1(AddressOf Cancel)
            下载文件的窗体.Invoke(d, New Object() {id})
        Else
            下载文件的窗体.取消下载(Integer.Parse(id))
        End If
    End Sub

    Public Sub LocateFile(ByVal FileName As String)
        If 下载文件的窗体.InvokeRequired Then
            Dim d As New 跨线程1(AddressOf LocateFile)
            下载文件的窗体.Invoke(d, New Object() {FileName})
        Else
            If FileName.Contains(":\") = False Then
                Dim 保存路径 As String = Path.GetDirectoryName(My.Computer.FileSystem.SpecialDirectories.MyDocuments) & "\Downloads"
                If Directory.Exists(保存路径) = False Then
                    保存路径 = My.Computer.FileSystem.SpecialDirectories.MyDocuments
                End If
                保存路径 &= "\" & FileName
                打开资源管理器并选中文件(保存路径)
            Else
                打开资源管理器并选中文件(FileName)
            End If
        End If
    End Sub

End Class
