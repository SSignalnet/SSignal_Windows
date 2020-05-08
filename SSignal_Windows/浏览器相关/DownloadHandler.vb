Imports CefSharp

Public Class DownloadHandler
    Implements IDownloadHandler

    Dim 主窗体1 As 主窗体

    Private Delegate Sub 跨线程1(ByVal 下载路径 As String)

    Public Sub New(ByVal 主窗体2 As 主窗体)
        主窗体1 = 主窗体2
    End Sub

    Public Sub OnBeforeDownload(chromiumWebBrowser As IWebBrowser, browser As IBrowser, downloadItem As DownloadItem, callback As IBeforeDownloadCallback) Implements IDownloadHandler.OnBeforeDownload
        下载文件(downloadItem.Url)
    End Sub

    Private Sub 下载文件(ByVal 下载路径 As String)
        If 主窗体1.InvokeRequired Then
            Dim d As New 跨线程1(AddressOf 下载文件)
            主窗体1.Invoke(d, New Object() {下载路径})
        Else
            主窗体1.下载文件(下载路径)
        End If
    End Sub

    Public Sub OnDownloadUpdated(chromiumWebBrowser As IWebBrowser, browser As IBrowser, downloadItem As DownloadItem, callback As IDownloadItemCallback) Implements IDownloadHandler.OnDownloadUpdated

    End Sub

End Class
