Imports CefSharp

Public Class LifeSpanHandler
    Implements ILifeSpanHandler

    Dim 主窗体1 As 主窗体

    Public Sub New(ByVal 主窗体2 As 主窗体)
        主窗体1 = 主窗体2
    End Sub

    Public Sub OnAfterCreated(chromiumWebBrowser As IWebBrowser, browser As IBrowser) Implements ILifeSpanHandler.OnAfterCreated
    End Sub

    Public Sub OnBeforeClose(chromiumWebBrowser As IWebBrowser, browser As IBrowser) Implements ILifeSpanHandler.OnBeforeClose
    End Sub

    Public Function OnBeforePopup(chromiumWebBrowser As IWebBrowser, browser As IBrowser, frame As IFrame, targetUrl As String, targetFrameName As String, targetDisposition As WindowOpenDisposition, userGesture As Boolean, popupFeatures As IPopupFeatures, windowInfo As IWindowInfo, browserSettings As IBrowserSettings, ByRef noJavascriptAccess As Boolean, ByRef newBrowser As IWebBrowser) As Boolean Implements ILifeSpanHandler.OnBeforePopup
        newBrowser = Nothing
        If 主窗体1 IsNot Nothing Then 主窗体1.访问网页(targetUrl)
        Return True
    End Function

    Public Function DoClose(chromiumWebBrowser As IWebBrowser, browser As IBrowser) As Boolean Implements ILifeSpanHandler.DoClose
        Return True
    End Function

End Class
