Imports System.IO
Imports System.Text.Encoding
Imports NAudio.Wave
Imports NAudio.MediaFoundation
Imports AMRConvertor

Public Class 类_音频_录制

    Public Enum 音频格式_常量集合 As Byte
        wave = 0
        mp3 = 1
        aac = 2
        amr = 3
    End Enum

    Private WithEvents Wave输入 As WaveIn
    Dim Wave格式 As WaveFormat
    Dim Wave文件写入器 As WaveFileWriter
    Dim 内存流 As MemoryStream
    Dim 文件路径1 As String
    Dim 音频格式1 As 音频格式_常量集合
    Dim MediaFoundationApi已启动 As Boolean
    Dim AMR转换器 As AMRConverting

    Public Event 录音完毕(ByVal sender As Object, ByVal 文件路径 As String)

    Public ReadOnly Property 正在录音 As Boolean
        Get
            If Wave输入 IsNot Nothing Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property

    Public Function 开始录音(ByVal 目录 As String, ByVal 文件名 As String, Optional ByVal 音频格式 As 音频格式_常量集合 = 音频格式_常量集合.wave) As Boolean
        If Wave输入 IsNot Nothing Then Return False
        If Directory.Exists(目录) = False Then Directory.CreateDirectory(目录)
        If 目录.EndsWith("\") = False Then 目录 &= "\"
        音频格式1 = 音频格式
        Dim 扩展名 As String
        Select Case 音频格式1
            Case 音频格式_常量集合.amr
                扩展名 = "amr"
                Wave格式 = New WaveFormat(8000, 1)
            Case 音频格式_常量集合.mp3
                扩展名 = "mp3"
                Wave格式 = New WaveFormat(44100, 1)
            Case 音频格式_常量集合.aac
                扩展名 = "aac"
                Wave格式 = New WaveFormat(44100, 1)
            Case Else
                扩展名 = "wav"
                Wave格式 = New WaveFormat(22050, 1)
        End Select
        文件路径1 = 目录 & 文件名 & "." & 扩展名
        Wave输入 = New WaveIn With {
            .WaveFormat = Wave格式
        }
        If 音频格式1 = 音频格式_常量集合.amr Then
            内存流 = New MemoryStream
        ElseIf 音频格式1 <> 音频格式_常量集合.wave Then
            内存流 = New MemoryStream
            Wave文件写入器 = New WaveFileWriter(内存流, Wave输入.WaveFormat)
        Else
            Wave文件写入器 = New WaveFileWriter(文件路径1, Wave输入.WaveFormat)
        End If
        Wave输入.StartRecording()
        Return True
    End Function

    Private Sub Wave源_DataAvailable(sender As Object, e As WaveInEventArgs) Handles Wave输入.DataAvailable
        If 音频格式1 = 音频格式_常量集合.amr Then
            If 内存流 IsNot Nothing Then
                Try
                    内存流.Write(e.Buffer, 0, e.BytesRecorded)
                    内存流.Flush()
                Catch ex As Exception
                    内存流.Close()
                    内存流 = Nothing
                End Try
            End If
        Else
            If Wave文件写入器 IsNot Nothing Then
                Try
                    Wave文件写入器.Write(e.Buffer, 0, e.BytesRecorded)
                    Wave文件写入器.Flush()
                Catch ex As Exception
                    Wave文件写入器.Close()
                    Wave文件写入器 = Nothing
                End Try
            End If
        End If
    End Sub

    Public Sub 停止录音()
        If Wave输入 IsNot Nothing Then Wave输入.StopRecording()
    End Sub

    Private Sub Wave源_RecordingStopped(sender As Object, e As StoppedEventArgs) Handles Wave输入.RecordingStopped
        If Wave输入 IsNot Nothing Then
            Wave输入.Dispose()
            Wave输入 = Nothing
        End If
        If 音频格式1 <> 音频格式_常量集合.wave Then
            If 内存流 IsNot Nothing Then
                Try
                    内存流.Seek(0, SeekOrigin.Begin)
                    If 音频格式1 = 音频格式_常量集合.amr Then
                        If AMR转换器 Is Nothing Then AMR转换器 = New AMRConverting
                        Dim AMR数据体() As Byte = AMR转换器.EncodeWAVEToAMR(内存流.ToArray)
                        Dim AMR数据头() As Byte = ASCII.GetBytes("#!AMR" & ChrW(10))
                        Dim AMR数据(AMR数据头.Length + AMR数据体.Length - 1) As Byte
                        Array.Copy(AMR数据头, 0, AMR数据, 0, AMR数据头.Length)
                        Array.Copy(AMR数据体, 0, AMR数据, AMR数据头.Length, AMR数据体.Length)
                        File.WriteAllBytes(文件路径1, AMR数据)
                    Else
                        If MediaFoundationApi已启动 = False Then
                            MediaFoundationApi.Startup()
                            MediaFoundationApi已启动 = True
                        End If
                        Using Wave文件读取器 As New WaveFileReader(内存流)
                            Using Wave格式转换流 As New WaveFormatConversionStream(Wave格式, Wave文件读取器)
                                Select Case 音频格式1
                                    Case 音频格式_常量集合.mp3 : MediaFoundationEncoder.EncodeToMp3(Wave格式转换流, 文件路径1)
                                    Case 音频格式_常量集合.aac : MediaFoundationEncoder.EncodeToAac(Wave格式转换流, 文件路径1)
                                End Select
                            End Using
                        End Using
                    End If
                Catch ex As Exception
                    Throw ex
                Finally
                    If 内存流 IsNot Nothing Then
                        内存流.Close()
                        内存流 = Nothing
                    End If
                End Try
                RaiseEvent 录音完毕(Me, 文件路径1)
            End If
        Else
            If Wave文件写入器 IsNot Nothing Then
                Wave文件写入器.Close()
                Wave文件写入器 = Nothing
                RaiseEvent 录音完毕(Me, 文件路径1)
            End If
        End If
    End Sub

    Public Sub 关闭()
        停止录音()
        If MediaFoundationApi已启动 = True Then
            MediaFoundationApi.Shutdown()
            MediaFoundationApi已启动 = False
        End If
    End Sub

End Class
