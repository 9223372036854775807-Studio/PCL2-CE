'���ڰ����ӽ��ܵȰ�ȫ��Ϣ�����ļ��еĲ��ִ����ѱ�ɾ��

Imports System.ComponentModel
Imports System.Net
Imports System.Reflection
Imports System.Text
Imports System.Security.Cryptography
Imports NAudio.Midi
Imports System.Management
Imports System
Imports System.IO.Compression

Friend Module ModSecret

#Region "����"

#If RELEASE Or BETA Then
    Public Const RegFolder As String = "PCLCE" 'PCL �������ע����� PCL ��ע�����룬�Է����ݳ�ͻ
#Else
    Public Const RegFolder As String = "PCLCEDebug" '�����������ע���������������ע�����룬�Է����ݳ�ͻ
#End If

    '����΢���¼�� ClientId
    Public Const OAuthClientId As String = ""
    'CurseForge API Key
    Public Const CurseForgeAPIKey As String = ""
    ' LittleSkin OAuth ClientId
    Public Const LittleSkinClientId As String = ""

    Friend Sub SecretOnApplicationStart()
        '���� UI �߳����ȼ�
        Thread.CurrentThread.Priority = ThreadPriority.Highest
        'ȷ�� .NET Framework �汾
        Try
            Dim VersionTest As New FormattedText("", Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Fonts.SystemTypefaces.First, 96, New MyColor, DPI)
        Catch ex As UriFormatException '�޸� #3555
            Environment.SetEnvironmentVariable("windir", Environment.GetEnvironmentVariable("SystemRoot"), EnvironmentVariableTarget.User)
            Dim VersionTest As New FormattedText("", Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Fonts.SystemTypefaces.First, 96, New MyColor, DPI)
        End Try
        '��⵱ǰ�ļ���Ȩ��
        Try
            Directory.CreateDirectory(Path & "PCL")
        Catch ex As Exception
            MsgBox($"PCL �޷����� PCL �ļ��У�{Path & "PCL"}�����볢�ԣ�" & vbCrLf &
                  "1. �� PCL �ƶ��������ļ���" & If(Path.StartsWithF("C:", True), "������ C �̺��������������λ�á�", "��") & vbCrLf &
                  "2. ɾ����ǰĿ¼�е� PCL �ļ��У�Ȼ�����ԡ�" & vbCrLf &
                  "3. �Ҽ� PCL ѡ�����ԣ��� ������ �е� �Թ���Ա������д˳���",
                MsgBoxStyle.Critical, "���л�������")
            Environment.[Exit](ProcessReturnValues.Cancel)
        End Try
        If Not CheckPermission(Path & "PCL") Then
            MsgBox("PCL û�жԵ�ǰ�ļ��е�д��Ȩ�ޣ��볢�ԣ�" & vbCrLf &
                  "1. �� PCL �ƶ��������ļ���" & If(Path.StartsWithF("C:", True), "������ C �̺��������������λ�á�", "��") & vbCrLf &
                  "2. ɾ����ǰĿ¼�е� PCL �ļ��У�Ȼ�����ԡ�" & vbCrLf &
                  "3. �Ҽ� PCL ѡ�����ԣ��� ������ �е� �Թ���Ա������д˳���",
                MsgBoxStyle.Critical, "���л�������")
            Environment.[Exit](ProcessReturnValues.Cancel)
        End If
        '��������ʾ
        If Setup.Get("UiLauncherCEHint") Then ShowCEAnnounce()
    End Sub
    ''' <summary>
    ''' չʾ��������ʾ
    ''' </summary>
    ''' <param name="IsUpdate">�Ƿ�Ϊ����ʱ����</param>
    Public Sub ShowCEAnnounce(Optional IsUpdate As Boolean = False)
        MyMsgBox($"������ʹ������ PCL-Community �� PCL �����汾�����������벻Ҫ��ٷ��ֿⷴ����
PCL-Community �����Ա������èԾ�޴�����ϵ���Ҿ�����Ϊ����ʹ����������

��������������ص������棬�������عٷ��� PCL ʹ�á�

�ð汾��ٷ��汾����������
- ����֪ͨ����ʱû�У�������������.jpg
- �����л�������������������Ҫ���������ļ������ʵĹ���
- �ٱ��䣺�������ݸ��ĺ�ȱʧ�����߷�֧û���ṩ�������{If(IsUpdate, $"{vbCrLf}{vbCrLf}����ʾ�ܻ��ڸ���������ʱչʾһ�Ρ�", "")}", "�����汾˵��", "��֪����")
    End Sub

    Private _RawCodeCache As String = Nothing
    Private ReadOnly _cacheLock As New Object()
    ''' <summary>
    ''' ��ȡԭʼ���豸��ʶ��
    ''' </summary>
    ''' <returns></returns>
    Friend Function SecretGetRawCode() As String
        SyncLock _cacheLock
            Try
                If _RawCodeCache IsNot Nothing Then Return _RawCodeCache
                Dim rawCode As String = Nothing
                Dim searcher As New ManagementObjectSearcher("select ProcessorId from Win32_Processor") ' ��ȡ CPU ���к�
                For Each obj As ManagementObject In searcher.Get()
                    rawCode = obj("ProcessorId")?.ToString()
                    Exit For
                Next
                If String.IsNullOrWhiteSpace(rawCode) Then Throw New Exception("��ȡ CPU ���к�ʧ��")
                Using sha256 As SHA256 = SHA256.Create() ' SHA256 ����
                    Dim hash As Byte() = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawCode))
                    rawCode = BitConverter.ToString(hash).Replace("-", "")
                End Using
                _RawCodeCache = rawCode
                Return rawCode
            Catch ex As Exception
                Log(ex, "[System] ��ȡ�豸ԭʼ��ʶ��ʧ�ܣ�ʹ��Ĭ�ϱ�ʶ��")
                Return "b09675a9351cbd1fd568056781fe3966dd936cc9b94e51ab5cf67eeb7e74c075".ToUpper()
            End Try
        End SyncLock
    End Function

    ''' <summary>
    ''' ��ȡ�豸�Ķ̱�ʶ��
    ''' </summary>
    Friend Function SecretGetUniqueAddress() As String
        Dim code As String
        Dim rawCode As String = SecretGetRawCode()
        Try
            Using MD5 As MD5 = MD5.Create()
                Dim buffer = MD5.ComputeHash(Encoding.UTF8.GetBytes(rawCode))
                code = BitConverter.ToString(buffer).Replace("-", "")
            End Using
            code = code.Substring(6, 16)
            code = code.Insert(4, "-").Insert(9, "-").Insert(14, "-")
            Return code
        Catch ex As Exception
            Return "PCL2-CECE-GOOD-2025"
        End Try
    End Function

    Private _EncryptKeyCache As String = Nothing
    Private ReadOnly _cacheEncryptKeyLock As New Object()
    ''' <summary>
    ''' ��ȡ AES ������Կ
    ''' </summary>
    ''' <returns></returns>
    Friend Function SecretGetEncryptKey() As String
        SyncLock _cacheEncryptKeyLock
            If _EncryptKeyCache IsNot Nothing Then Return _EncryptKeyCache
            Dim rawCode = SecretGetRawCode()
            Using SHA512 As SHA512 = SHA512.Create()
                Dim hash As Byte() = SHA512.ComputeHash(Encoding.UTF8.GetBytes(rawCode))
                Dim key As String = BitConverter.ToString(hash).Replace("-", "")
                key = key.Substring(4, 32)
                _EncryptKeyCache = key
                Return key
            End Using
        End SyncLock
    End Function

    Friend Sub SecretLaunchJvmArgs(ByRef DataList As List(Of String))
        Dim DataJvmCustom As String = Setup.Get("VersionAdvanceJvm", Version:=McVersionCurrent)
        DataList.Insert(0, If(DataJvmCustom = "", Setup.Get("LaunchAdvanceJvm"), DataJvmCustom)) '�ɱ� JVM ����
        McLaunchLog("��ǰʣ���ڴ棺" & Math.Round(My.Computer.Info.AvailablePhysicalMemory / 1024 / 1024 / 1024 * 10) / 10 & "G")
        DataList.Add("-Xmn" & Math.Floor(PageVersionSetup.GetRam(McVersionCurrent) * 1024 * 0.15) & "m")
        DataList.Add("-Xmx" & Math.Floor(PageVersionSetup.GetRam(McVersionCurrent) * 1024) & "m")
        If Not DataList.Any(Function(d) d.Contains("-Dlog4j2.formatMsgNoLookups=true")) Then DataList.Add("-Dlog4j2.formatMsgNoLookups=true")
    End Sub

    ''' <summary>
    ''' �����ַ����е� AccessToken��
    ''' </summary>
    Friend Function SecretFilter(Raw As String, FilterChar As Char) As String
        '���� "accessToken " �������
        If Raw.Contains("accessToken ") Then
            For Each Token In RegexSearch(Raw, "(?<=accessToken ([^ ]{5}))[^ ]+(?=[^ ]{5})")
                Raw = Raw.Replace(Token, New String(FilterChar, Token.Count))
            Next
        End If
        '���뵱ǰ��¼�Ľ��
        Dim AccessToken As String = McLoginLoader.Output.AccessToken
        If AccessToken Is Nothing OrElse AccessToken.Length < 10 OrElse Not Raw.ContainsF(AccessToken, True) OrElse
            McLoginLoader.Output.Uuid = McLoginLoader.Output.AccessToken Then 'UUID �� AccessToken һ���򲻴���
            Return Raw
        Else
            Return Raw.Replace(AccessToken, Strings.Left(AccessToken, 5) & New String(FilterChar, AccessToken.Length - 10) & Strings.Right(AccessToken, 5))
        End If
    End Function

#End Region

#Region "�����Ȩ"

    Friend Function SecretCdnSign(UrlWithMark As String)
        If Not UrlWithMark.EndsWithF("{CDN}") Then Return UrlWithMark
        Return UrlWithMark.Replace("{CDN}", "").Replace(" ", "%20")
    End Function
    ''' <summary>
    ''' ���� Headers �� UA��Referer��
    ''' </summary>
    Friend Sub SecretHeadersSign(Url As String, ByRef Client As WebClient, Optional UseBrowserUserAgent As Boolean = False)
        If Url.Contains("baidupcs.com") OrElse Url.Contains("baidu.com") Then
            Client.Headers("User-Agent") = "LogStatistic" '#4951
        ElseIf UseBrowserUserAgent Then
            Client.Headers("User-Agent") = "PCL2/" & UpstreamVersion & "." & VersionBranchCode & " PCLCE/" & VersionStandardCode & " Mozilla/5.0 AppleWebKit/537.36 Chrome/63.0.3239.132 Safari/537.36"
        Else
            Client.Headers("User-Agent") = "PCL2/" & UpstreamVersion & "." & VersionBranchCode & " PCLCE/" & VersionStandardCode
        End If
        Client.Headers("Referer") = "http://" & VersionCode & ".ce.open.pcl2.server/"
        If Url.Contains("api.curseforge.com") Then Client.Headers("x-api-key") = CurseForgeAPIKey
    End Sub
    ''' <summary>
    ''' ���� Headers �� UA��Referer��
    ''' </summary>
    Friend Sub SecretHeadersSign(Url As String, ByRef Request As HttpWebRequest, Optional UseBrowserUserAgent As Boolean = False)
        If Url.Contains("baidupcs.com") OrElse Url.Contains("baidu.com") Then
            Request.UserAgent = "LogStatistic" '#4951
        ElseIf UseBrowserUserAgent Then
            Request.UserAgent = "PCL2/" & UpstreamVersion & "." & VersionBranchCode & " PCLCE/" & VersionStandardCode & " Mozilla/5.0 AppleWebKit/537.36 Chrome/63.0.3239.132 Safari/537.36"
        Else
            Request.UserAgent = "PCL2/" & UpstreamVersion & "." & VersionBranchCode & " PCLCE/" & VersionStandardCode
        End If
        Request.Referer = "http://" & VersionCode & ".ce.open.pcl2.server/"
        If Url.Contains("api.curseforge.com") Then Request.Headers("x-api-key") = CurseForgeAPIKey
    End Sub

#End Region

#Region "�ַ����ӽ���"

    Friend Function SecretDecrptyOld(SourceString As String) As String
        Dim Key = "00000000"
        Dim btKey As Byte() = Encoding.UTF8.GetBytes(Key)
        Dim btIV As Byte() = Encoding.UTF8.GetBytes("87160295")
        Dim des As New DESCryptoServiceProvider
        Using MS As New MemoryStream
            Dim inData As Byte() = Convert.FromBase64String(SourceString)
            Using cs As New CryptoStream(MS, des.CreateDecryptor(btKey, btIV), CryptoStreamMode.Write)
                cs.Write(inData, 0, inData.Length)
                cs.FlushFinalBlock()
                Return Encoding.UTF8.GetString(MS.ToArray())
            End Using
        End Using
    End Function

    ''' <summary>
    ''' �����ַ������Ż��棩��
    ''' </summary>
    Friend Function SecretEncrypt(SourceString As String) As String
        If SourceString = "" Then Return ""
        If String.IsNullOrWhiteSpace(SourceString) Then Return Nothing
        Dim Key = SecretGetEncryptKey()

        Using aes = AesCng.Create()
            aes.KeySize = 256
            aes.BlockSize = 128
            aes.Mode = CipherMode.CBC
            aes.Padding = PaddingMode.PKCS7

            Dim salt As Byte() = New Byte(31) {}
            Using rng = New RNGCryptoServiceProvider()
                rng.GetBytes(salt)
            End Using

            Using deriveBytes = New Rfc2898DeriveBytes(Key, salt, 1000)
                aes.Key = deriveBytes.GetBytes(aes.KeySize \ 8)
                aes.GenerateIV()
            End Using

            Using ms = New MemoryStream()
                ms.Write(salt, 0, salt.Length)
                ms.Write(aes.IV, 0, aes.IV.Length)

                Using cs = New CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write)
                    Dim data = Encoding.UTF8.GetBytes(SourceString)
                    cs.Write(data, 0, data.Length)
                End Using

                Return Convert.ToBase64String(ms.ToArray())
            End Using
        End Using
    End Function

    ''' <summary>
    ''' �����ַ�����
    ''' </summary>
    Friend Function SecretDecrypt(SourceString As String) As String
        If SourceString = "" Then Return ""
        If String.IsNullOrWhiteSpace(SourceString) Then Return Nothing
        Dim Key = SecretGetEncryptKey()
        Dim encryptedData = Convert.FromBase64String(SourceString)

        Using aes = AesCng.Create()
            aes.KeySize = 256
            aes.BlockSize = 128
            aes.Mode = CipherMode.CBC
            aes.Padding = PaddingMode.PKCS7

            Dim salt = New Byte(31) {}
            Array.Copy(encryptedData, 0, salt, 0, salt.Length)

            Dim iv = New Byte(aes.BlockSize \ 8 - 1) {}
            Array.Copy(encryptedData, salt.Length, iv, 0, iv.Length)
            aes.IV = iv

            If encryptedData.Length < salt.Length + iv.Length Then
                Throw New ArgumentException("�������ݸ�ʽ��Ч������")
            End If

            Using deriveBytes = New Rfc2898DeriveBytes(Key, salt, 1000)
                aes.Key = deriveBytes.GetBytes(aes.KeySize \ 8)
            End Using

            Dim cipherTextLength = encryptedData.Length - salt.Length - iv.Length
            Using ms = New MemoryStream(encryptedData, salt.Length + iv.Length, cipherTextLength)
                Using cs = New CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read)
                    Using sr = New StreamReader(cs, Encoding.UTF8)
                        Return sr.ReadToEnd()
                    End Using
                End Using
            End Using
        End Using
    End Function

#End Region

#Region "����"

    Public IsDarkMode As Boolean = False

    Public ColorDark1 As New MyColor(235, 235, 235)
    Public ColorDark2 As New MyColor(102, 204, 255)
    Public ColorDark3 As New MyColor(51, 187, 255)
    Public ColorDark6 As New MyColor(93, 101, 103)
    Public ColorDark7 As New MyColor(69, 75, 79)
    Public ColorDark8 As New MyColor(59, 64, 65)
    Public ColorLight1 As New MyColor(52, 61, 74)
    Public ColorLight2 As New MyColor(11, 91, 203)
    Public ColorLight3 As New MyColor(19, 112, 243)
    Public ColorLight6 As New MyColor(213, 230, 253)
    Public ColorLight7 As New MyColor(222, 236, 253)
    Public ColorLight8 As New MyColor(234, 242, 254)
    Public Color1 As MyColor = If(IsDarkMode, ColorDark1, ColorLight1)
    Public Color2 As MyColor = If(IsDarkMode, ColorDark2, ColorLight2)
    Public Color3 As MyColor = If(IsDarkMode, ColorDark3, ColorLight3)
    'Public Color2 As New MyColor(11, 91, 203)
    'Public Color3 As New MyColor(19, 112, 243)
    Public Color4 As New MyColor(72, 144, 245)
    Public Color5 As New MyColor(150, 192, 249)
    Public Color6 As MyColor = If(IsDarkMode, ColorDark6, ColorLight6)
    Public Color7 As MyColor = If(IsDarkMode, ColorDark7, ColorLight7)
    Public Color8 As MyColor = If(IsDarkMode, ColorDark8, ColorLight8)
    Public ColorBg0 As New MyColor(150, 192, 249)
    Public ColorBg1 As New MyColor(190, Color7)
    Public ColorGrayDark1 As New MyColor(245, 245, 245)
    Public ColorGrayDark2 As New MyColor(240, 240, 240)
    Public ColorGrayDark3 As New MyColor(235, 235, 235)
    Public ColorGrayDark4 As New MyColor(204, 204, 204)
    Public ColorGrayDark5 As New MyColor(166, 166, 166)
    Public ColorGrayDark6 As New MyColor(140, 140, 140)
    Public ColorGrayDark7 As New MyColor(115, 115, 115)
    Public ColorGrayDark8 As New MyColor(64, 64, 64)
    Public ColorGrayLight1 As New MyColor(64, 64, 64)
    Public ColorGrayLight2 As New MyColor(115, 115, 115)
    Public ColorGrayLight3 As New MyColor(140, 140, 140)
    Public ColorGrayLight4 As New MyColor(166, 166, 166)
    Public ColorGrayLight5 As New MyColor(204, 204, 204)
    Public ColorGrayLight6 As New MyColor(235, 235, 235)
    Public ColorGrayLight7 As New MyColor(240, 240, 240)
    Public ColorGrayLight8 As New MyColor(245, 245, 245)
    Public ColorGray1 As MyColor = If(IsDarkMode, ColorGrayDark1, ColorGrayLight1)
    Public ColorGray2 As MyColor = If(IsDarkMode, ColorGrayDark2, ColorGrayLight2)
    Public ColorGray3 As MyColor = If(IsDarkMode, ColorGrayDark3, ColorGrayLight3)
    Public ColorGray4 As MyColor = If(IsDarkMode, ColorGrayDark4, ColorGrayLight4)
    Public ColorGray5 As MyColor = If(IsDarkMode, ColorGrayDark5, ColorGrayLight5)
    Public ColorGray6 As MyColor = If(IsDarkMode, ColorGrayDark6, ColorGrayLight6)
    Public ColorGray7 As MyColor = If(IsDarkMode, ColorGrayDark7, ColorGrayLight7)
    Public ColorGray8 As MyColor = If(IsDarkMode, ColorGrayDark8, ColorGrayLight8)
    Public ColorSemiTransparent As New MyColor(1, Color8)

    Public ThemeNow As Integer = -1
    'Public ColorHue As Integer = If(IsDarkMode, 200, 210), ColorSat As Integer = If(IsDarkMode, 100, 85), ColorLightAdjust As Integer = If(IsDarkMode, 15, 0), ColorHueTopbarDelta As Object = 0
    Public ColorHue As Integer = 210, ColorSat As Integer = 85, ColorLightAdjust As Integer = 0, ColorHueTopbarDelta As Object = 0
    Public ThemeDontClick As Integer = 0

    '��ɫģʽ�¼�

    ' �����Զ����¼�
    Public Event ThemeChanged As EventHandler(Of Boolean)

    ' �����¼��ĺ���
    Public Sub RaiseThemeChanged(isDarkMode As Boolean)
        RaiseEvent ThemeChanged("", isDarkMode)
    End Sub

    Public Sub ThemeRefresh(Optional NewTheme As Integer = -1)
        RaiseThemeChanged(IsDarkMode)
        ThemeRefreshColor()
        ThemeRefreshMain()
    End Sub
    Public Function GetDarkThemeLight(OriginalLight As Double) As Double
        If IsDarkMode Then
            Return OriginalLight * 0.1
        Else
            Return OriginalLight
        End If
    End Function
    Public Sub ThemeRefreshColor()
        ColorGray1 = If(IsDarkMode, ColorGrayDark1, ColorGrayLight1)
        ColorGray2 = If(IsDarkMode, ColorGrayDark2, ColorGrayLight2)
        ColorGray3 = If(IsDarkMode, ColorGrayDark3, ColorGrayLight3)
        ColorGray4 = If(IsDarkMode, ColorGrayDark4, ColorGrayLight4)
        ColorGray5 = If(IsDarkMode, ColorGrayDark5, ColorGrayLight5)
        ColorGray6 = If(IsDarkMode, ColorGrayDark6, ColorGrayLight6)
        ColorGray7 = If(IsDarkMode, ColorGrayDark7, ColorGrayLight7)
        ColorGray8 = If(IsDarkMode, ColorGrayDark8, ColorGrayLight8)

        If IsDarkMode Then
            Application.Current.Resources("ColorBrush1") = New SolidColorBrush(ColorDark1)
            Application.Current.Resources("ColorBrush2") = New SolidColorBrush(ColorDark2)
            Application.Current.Resources("ColorBrush3") = New SolidColorBrush(ColorDark3)
            Application.Current.Resources("ColorBrush6") = New SolidColorBrush(ColorDark6)
            Application.Current.Resources("ColorBrush7") = New SolidColorBrush(ColorDark7)
            Application.Current.Resources("ColorBrush8") = New SolidColorBrush(ColorDark8)
            Application.Current.Resources("ColorBrushGray1") = New SolidColorBrush(ColorGrayDark1)
            Application.Current.Resources("ColorBrushGray2") = New SolidColorBrush(ColorGrayDark2)
            Application.Current.Resources("ColorBrushGray3") = New SolidColorBrush(ColorGrayDark3)
            Application.Current.Resources("ColorBrushGray4") = New SolidColorBrush(ColorGrayDark4)
            Application.Current.Resources("ColorBrushGray5") = New SolidColorBrush(ColorGrayDark5)
            Application.Current.Resources("ColorBrushGray6") = New SolidColorBrush(ColorGrayDark6)
            Application.Current.Resources("ColorBrushGray7") = New SolidColorBrush(ColorGrayDark7)
            Application.Current.Resources("ColorBrushGray8") = New SolidColorBrush(ColorGrayDark8)
            Application.Current.Resources("ColorBrushHalfWhite") = New SolidColorBrush(Color.FromArgb(85, 90, 90, 90))
            Application.Current.Resources("ColorBrushBg0") = New SolidColorBrush(ColorDark2)
            Application.Current.Resources("ColorBrushBg1") = New SolidColorBrush(Color.FromArgb(190, 90, 90, 90))
            Application.Current.Resources("ColorBrushBackgroundTransparentSidebar") = New SolidColorBrush(Color.FromArgb(235, 43, 43, 43))
            Application.Current.Resources("ColorBrushTransparent") = New SolidColorBrush(Color.FromArgb(0, 43, 43, 43))
            Application.Current.Resources("ColorBrushToolTip") = New SolidColorBrush(Color.FromArgb(229, 90, 90, 90))
            Application.Current.Resources("ColorBrushWhite") = New SolidColorBrush(Color.FromRgb(43, 43, 43))
            Application.Current.Resources("ColorBrushMsgBox") = New SolidColorBrush(Color.FromRgb(43, 43, 43))
            Application.Current.Resources("ColorBrushMsgBoxText") = New SolidColorBrush(ColorDark1)
            Application.Current.Resources("ColorBrushMemory") = New SolidColorBrush(Color.FromRgb(255, 255, 255))
        Else
            Application.Current.Resources("ColorBrush1") = New SolidColorBrush(ColorLight1)
            Application.Current.Resources("ColorBrush2") = New SolidColorBrush(ColorLight2)
            Application.Current.Resources("ColorBrush3") = New SolidColorBrush(ColorLight3)
            Application.Current.Resources("ColorBrush6") = New SolidColorBrush(ColorLight6)
            Application.Current.Resources("ColorBrush7") = New SolidColorBrush(ColorLight7)
            Application.Current.Resources("ColorBrush8") = New SolidColorBrush(ColorLight8)
            Application.Current.Resources("ColorBrushGray1") = New SolidColorBrush(ColorGrayLight1)
            Application.Current.Resources("ColorBrushGray2") = New SolidColorBrush(ColorGrayLight2)
            Application.Current.Resources("ColorBrushGray3") = New SolidColorBrush(ColorGrayLight3)
            Application.Current.Resources("ColorBrushGray4") = New SolidColorBrush(ColorGrayLight4)
            Application.Current.Resources("ColorBrushGray5") = New SolidColorBrush(ColorGrayLight5)
            Application.Current.Resources("ColorBrushGray6") = New SolidColorBrush(ColorGrayLight6)
            Application.Current.Resources("ColorBrushGray7") = New SolidColorBrush(ColorGrayLight7)
            Application.Current.Resources("ColorBrushGray8") = New SolidColorBrush(ColorGrayLight8)
            Application.Current.Resources("ColorBrushHalfWhite") = New SolidColorBrush(Color.FromArgb(85, 255, 255, 255))
            Application.Current.Resources("ColorBrushBg0") = New SolidColorBrush(ColorBg0)
            Application.Current.Resources("ColorBrushBg1") = New SolidColorBrush(ColorBg1)
            Application.Current.Resources("ColorBrushBackgroundTransparentSidebar") = New SolidColorBrush(Color.FromArgb(210, 255, 255, 255))
            Application.Current.Resources("ColorBrushTransparent") = New SolidColorBrush(Color.FromArgb(0, 255, 255, 255))
            Application.Current.Resources("ColorBrushToolTip") = New SolidColorBrush(Color.FromArgb(229, 255, 255, 255))
            Application.Current.Resources("ColorBrushWhite") = New SolidColorBrush(Color.FromRgb(255, 255, 255))
            Application.Current.Resources("ColorBrushMsgBox") = New SolidColorBrush(Color.FromRgb(251, 251, 251))
            Application.Current.Resources("ColorBrushMsgBoxText") = New SolidColorBrush(ColorLight1)
            Application.Current.Resources("ColorBrushMemory") = New SolidColorBrush(Color.FromRgb(0, 0, 0))
        End If
    End Sub
    Public Sub ThemeRefreshMain()
        RunInUi(
        Sub()
            If Not FrmMain.IsLoaded Then Exit Sub
            '����������
            Dim Brush = New LinearGradientBrush With {.EndPoint = New Point(1, 0), .StartPoint = New Point(0, 0)}
            If ThemeNow = 5 Then
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0, .Color = New MyColor().FromHSL2(ColorHue, ColorSat, 25)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0.5, .Color = New MyColor().FromHSL2(ColorHue, ColorSat, 15)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 1, .Color = New MyColor().FromHSL2(ColorHue, ColorSat, 25)})
                FrmMain.PanTitle.Background = Brush
                FrmMain.PanTitle.Background.Freeze()
            ElseIf Not (ThemeNow = 12 OrElse ThemeDontClick = 2) Then
                If TypeOf ColorHueTopbarDelta Is Integer Then
                    Brush.GradientStops.Add(New GradientStop With {.Offset = 0, .Color = New MyColor().FromHSL2(ColorHue - ColorHueTopbarDelta, ColorSat, 48 + ColorLightAdjust)})
                    Brush.GradientStops.Add(New GradientStop With {.Offset = 0.5, .Color = New MyColor().FromHSL2(ColorHue, ColorSat, 54 + ColorLightAdjust)})
                    Brush.GradientStops.Add(New GradientStop With {.Offset = 1, .Color = New MyColor().FromHSL2(ColorHue + ColorHueTopbarDelta, ColorSat, 48 + ColorLightAdjust)})
                Else
                    Brush.GradientStops.Add(New GradientStop With {.Offset = 0, .Color = New MyColor().FromHSL2(ColorHue + ColorHueTopbarDelta(0), ColorSat, 48 + ColorLightAdjust)})
                    Brush.GradientStops.Add(New GradientStop With {.Offset = 0.5, .Color = New MyColor().FromHSL2(ColorHue + ColorHueTopbarDelta(1), ColorSat, 54 + ColorLightAdjust)})
                    Brush.GradientStops.Add(New GradientStop With {.Offset = 1, .Color = New MyColor().FromHSL2(ColorHue + ColorHueTopbarDelta(2), ColorSat, 48 + ColorLightAdjust)})
                End If
                FrmMain.PanTitle.Background = Brush
                FrmMain.PanTitle.Background.Freeze()
            Else
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0, .Color = New MyColor().FromHSL2(ColorHue - 21, ColorSat, 53 + ColorLightAdjust)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0.33, .Color = New MyColor().FromHSL2(ColorHue - 7, ColorSat, 47 + ColorLightAdjust)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0.67, .Color = New MyColor().FromHSL2(ColorHue + 7, ColorSat, 47 + ColorLightAdjust)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 1, .Color = New MyColor().FromHSL2(ColorHue + 21, ColorSat, 53 + ColorLightAdjust)})
                FrmMain.PanTitle.Background = Brush
            End If
            '��ҳ�汳��
            If Setup.Get("UiBackgroundColorful") Then
                Brush = New LinearGradientBrush With {.EndPoint = New Point(0.1, 1), .StartPoint = New Point(0.9, 0)}
                Brush.GradientStops.Add(New GradientStop With {.Offset = -0.1, .Color = New MyColor().FromHSL2(ColorHue - 20, Math.Min(60, ColorSat) * 0.5, GetDarkThemeLight(80))})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0.4, .Color = New MyColor().FromHSL2(ColorHue, ColorSat * 0.9, GetDarkThemeLight(90))})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 1.1, .Color = New MyColor().FromHSL2(ColorHue + 20, Math.Min(60, ColorSat) * 0.5, GetDarkThemeLight(80))})
                FrmMain.PanForm.Background = Brush
            Else
                FrmMain.PanForm.Background = New MyColor(If(IsDarkMode, 20, 245), If(IsDarkMode, 20, 245), If(IsDarkMode, 20, 245))
            End If
            FrmMain.PanForm.Background.Freeze()
        End Sub)
    End Sub
    Friend Sub ThemeCheckAll(EffectSetup As Boolean)
    End Sub
    Friend Function ThemeCheckOne(Id As Integer) As Boolean
        Return True
    End Function
    Friend Function ThemeUnlock(Id As Integer, Optional ShowDoubleHint As Boolean = True, Optional UnlockHint As String = Nothing) As Boolean
        Return False
    End Function
    Friend Function ThemeCheckGold(Optional Code As String = Nothing) As Boolean
        Return False
    End Function
    Friend Function DonateCodeInput() As Boolean?
        Return Nothing
    End Function

#End Region

#Region "����"

    Public Class UpdateInfo
        Public Property assets As List(Of UpdateAssetInfo)
    End Class

    Public Class UpdateAssetInfo
        Public Property file_name As String
        Public Property version As UpdateAssetVersionInfo
        Public Property upd_time As String
        Public Property downloads As List(Of String)
        Public Property sha256 As String
    End Class

    Public Class UpdateAssetVersionInfo
        Public Property channel As String
        Public Property name As String
        Public Property code As Integer
    End Class

    Public Class AnnouncementDetialInfo
        Public Property title As String
        Public Property detail As String
        Public Property id As String
        Public Property [date] As String
        Public Property btn1 As AnnouncementBtnInfo
        Public Property btn2 As AnnouncementBtnInfo
    End Class

    Public Class AnnouncementBtnInfo
        Public Property text As String
        Public Property command As String
        Public Property command_paramter As String
    End Class

    Public Class AnnouncementInfo
        Public Property content As List(Of AnnouncementDetialInfo)
    End Class

    Public RemoteVersionData As UpdateInfo = Nothing
    Public RemoteAnnounceData As AnnouncementInfo = Nothing
    Public IsUpdateStarted As Boolean = False
    Public IsUpdateWaitingRestart As Boolean = False
    Public RemoteServerBaseurl As New Dictionary(Of Integer, String) From {}

    Public Sub UpdateCheckByButton()
        If IsUpdateStarted Then
            Hint("���ڼ������У����Ժ����ԡ���")
            Exit Sub
        End If
        Hint("���ڻ�ȡ������Ϣ...")
        RunInNewThread(Sub()
                           Try
                               RefreshUpdatesCache()
                               NoticeUserUpdate()
                           Catch ex As Exception
                               Log(ex, "[Update] ��ȡ������������Ϣʧ��", LogLevel.Hint)
                               Hint("��ȡ������������Ϣʧ�ܣ�������������", HintType.Critical)
                           End Try
                       End Sub)
    End Sub
    Private Sub RefreshUpdatesCache()
        Try
            Dim UpdCaches As JObject = Nothing
            If RemoteServerBaseurl.Count <> 0 Then UpdCaches = NetGetCodeByRequestRetry(GetRemotePath("api/cache.json"), IsJson:=True)
            Dim UpdatesCacheFile = PathTemp & "Cache/updates.json"
            Dim AnnouncementCacheFile = PathTemp & "Cache/announcement.json"
            If RemoteServerBaseurl.Count <> 0 AndAlso GetFileMD5(UpdatesCacheFile) <> UpdCaches("updates") Then
                WriteFile(UpdatesCacheFile, NetGetCodeByRequestRetry(GetRemotePath("api/updates.json")))
            End If
            If RemoteServerBaseurl.Count <> 0 AndAlso GetFileMD5(AnnouncementCacheFile) <> UpdCaches("announcement") Then
                WriteFile(AnnouncementCacheFile, NetGetCodeByRequestRetry(GetRemotePath("api/announcement.json")))
            End If
            RemoteVersionData = CType(GetJson(ReadFile(UpdatesCacheFile)), JObject).ToObject(Of UpdateInfo)()
            RemoteAnnounceData = CType(GetJson(ReadFile(AnnouncementCacheFile)), JObject).ToObject(Of AnnouncementInfo)()
        Catch ex As Exception
            Log(ex, "[System] ˢ�¸�����Ϣʧ�ܡ���")
        End Try
    End Sub
    Private Function GetRemotePath(path As String) As String
        Return RemoteServerBaseurl(Setup.Get("SystemSystemServer")) & path
    End Function
    Public Function GetChannelInfo(Optional TargetMainChannel As String = Nothing) As UpdateAssetInfo
        If RemoteVersionData Is Nothing Then
            Log("[Update] δ��ȡ��Զ�̰汾��Ϣ���������»�ȡ")
            RefreshUpdatesCache()
        End If
        Dim IsBeta As Boolean = Setup.Get("SystemSystemUpdateBranch") = 1
        Dim targetChannel As UpdateAssetInfo = Nothing
        Dim targetMainChannelName = If(TargetMainChannel, If(IsBeta, "fr", "sr"))
        Log($"[System] ���� {targetMainChannelName} ͨ���ĸ�����Ϣ")
        targetChannel = RemoteVersionData.assets.Where(Function(x) x.version.channel = targetMainChannelName & If(IsArm64System, "arm64", "x64")).First()
        Return targetChannel
    End Function

    Public Sub NoticeUserUpdate(Optional Silent As Boolean = False)
        Dim LatestVersion = GetChannelInfo()
        Log($"[System] ��ȡ�����°汾�� {LatestVersion.version.code.ToString()}")
        If LatestVersion.version.code > VersionCode Then
            If Not Val(Environment.OSVersion.Version.ToString().Split(".")(2)) >= 19042 AndAlso Not LatestVersion.version.name.StartsWithF("2.9.") Then
                If MyMsgBox($"���������������£��汾 {LatestVersion.version.name}��������������� Windows �汾���ͣ��������°汾Ҫ��{vbCrLf}����Ҫ���µ� Windows 10 20H2 ����߰汾�ſ��Լ������¡�", "���������� - ϵͳ�汾����", "���� Windows 10", "ȡ��", IsWarn:=True, ForceWait:=True) = 1 Then OpenWebsite("https://www.microsoft.com/zh-cn/software-download/windows10")
                Exit Sub
            End If
            If MyMsgBox($"���������°汾���ã���VersionBaseName�� -> {LatestVersion.version.name}, ������ {DateTime.Parse(LatestVersion.upd_time).ToLocalTime()}){vbCrLf}�Ƿ��������£�", "����������", "����", "ȡ��") = 1 Then
                UpdateStart(LatestVersion, False)
            End If
        Else
            If Not Silent Then Hint("�������������°� " + VersionBaseName + "�������������", HintType.Finish)
        End If
    End Sub

    Public Sub UpdateStart(Version As UpdateAssetInfo, Slient As Boolean, Optional ReceivedKey As String = Nothing, Optional ForceValidated As Boolean = False)
        Dim DlTargetPath As String = Path + "PCL\Plain Craft Launcher Community Edition.exe"
        Dim DlTempPath As String = PathTemp & "Cache\CEUpdates.zip"
        RunInNewThread(Sub()
                           Try
                               '���첽�������
                               Dim Loaders As New List(Of LoaderBase)
                               '����
                               Loaders.Add(New LoaderDownload("���ظ����ļ�", New List(Of NetFile) From {New NetFile(Version.downloads, DlTempPath, New FileChecker(MinSize:=1024 * 64))}) With {.ProgressWeight = 15})
                               Loaders.Add(New LoaderTask(Of Integer, Integer)("�������ļ�", Sub()
                                                                                             Dim NewFileSha256 = GetFileSHA256(DlTempPath)
                                                                                             If String.IsNullOrWhiteSpace(NewFileSha256) Then
                                                                                                 Throw New Exception("�����������ļ� SHA256 ʧ��")
                                                                                             End If
                                                                                             If NewFileSha256 <> Version.sha256 Then
                                                                                                 Throw New Exception($"�ļ����鲻ͨ���������ļ� SHA256 Ϊ {NewFileSha256}��ʵ����Ҫ {Version.sha256}")
                                                                                             End If
                                                                                         End Sub))
                               Loaders.Add(New LoaderTask(Of Integer, Integer)("��ѹ�����ļ�", Sub()
                                                                                             Using archive = New ZipArchive(New FileStream(DlTempPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), ZipArchiveMode.Read)
                                                                                                 Dim entry As ZipArchiveEntry = archive.Entries.FirstOrDefault(Function(x) x.FullName.EndsWithF("Plain Craft Launcher Community Edition.exe"))
                                                                                                 entry.ExtractToFile(DlTargetPath, True)
                                                                                             End Using
                                                                                             File.Delete(DlTempPath)
                                                                                         End Sub))
                               If Not Slient Then
                                   Loaders.Add(New LoaderTask(Of Integer, Integer)("��װ����", Sub() UpdateRestart(True)))
                               End If
                               '����
                               Dim Loader As New LoaderCombo(Of JObject)("����������", Loaders)
                               Loader.Start()
                               If Slient Then
                                   IsUpdateWaitingRestart = True
                               Else
                                   LoaderTaskbarAdd(Loader)
                                   FrmMain.BtnExtraDownload.ShowRefresh()
                                   FrmMain.BtnExtraDownload.Ribble()
                               End If
                           Catch ex As Exception
                               Log(ex, "[Update] ���������������ļ�ʧ��", LogLevel.Hint)
                               Hint("���������������ļ�ʧ�ܣ�������������", HintType.Critical)
                           End Try
                       End Sub)
    End Sub
    Public Sub UpdateRestart(TriggerRestartAndByEnd As Boolean)
        Try
            Dim fileName As String = Path + "PCL\Plain Craft Launcher Community Edition.exe"
            If Not File.Exists(fileName) Then
                Log("[System] ����ʧ�ܣ�δ�ҵ������ļ�")
                Exit Sub
            End If
            ' id old new restart
            Dim text As String = String.Concat(New String() {"--update ", Process.GetCurrentProcess().Id, " """, PathWithName, """ """, fileName, """ ", TriggerRestartAndByEnd})
            Log("[System] ���³���������������" + text, LogLevel.Normal, "���ִ���")
            Process.Start(New ProcessStartInfo(fileName) With {.WindowStyle = ProcessWindowStyle.Hidden, .CreateNoWindow = True, .Arguments = text})
            If TriggerRestartAndByEnd Then
                FrmMain.EndProgram(False)
                Log("[System] �����ڸ���ǿ�ƽ�������", LogLevel.Normal, "���ִ���")
            End If
        Catch ex As Win32Exception
            Log(ex, "�Զ�����ʱ���� Win32 �������Ʊ�����", LogLevel.Debug, "���ִ���")
            If MyMsgBox(String.Format("���ڱ� Windows ��ȫ�������أ����ߴ���Ȩ�����⣬���� PCL �޷����¡�{0}�뽫 PCL �����ļ��м���������������ֶ��� {1}PCL\Plain Craft Launcher Community Edition.exe �滻��ǰ�ļ���", vbCrLf, ModBase.Path), "����ʧ��", "�鿴����", "ȷ��", "", True, True, False, Nothing, Nothing, Nothing) = 1 Then
                TryStartEvent("�򿪰���", "������/Microsoft Defender ����ų���.json")
            End If
        End Try
    End Sub
    Public Sub UpdateReplace(ProcessId As Integer, OldFileName As String, NewFileName As String, TriggerRestart As Boolean)
        Try
            Dim ps = Process.GetProcessById(ProcessId)
            If Not ps.HasExited Then
                ps.Kill()
            End If
        Catch ex As Exception
        End Try
        Dim ex2 As Exception = Nothing
        Dim num As Integer = 0
        Do
            Try
                If File.Exists(OldFileName) Then
                    File.Delete(OldFileName)
                End If
                If Not File.Exists(OldFileName) Then
                    Exit Try
                End If
            Catch ex3 As Exception
                ex2 = ex3
            Finally
                Thread.Sleep(500)
            End Try
            num += 1
        Loop While num <= 4
        If (Not File.Exists(OldFileName)) AndAlso File.Exists(NewFileName) Then
            Try
                CopyFile(NewFileName, OldFileName)
            Catch ex4 As UnauthorizedAccessException
                MsgBox("PCL ����ʧ�ܣ�Ȩ�޲��㡣���ֶ����� PCL �ļ����µ��°汾����" & vbCrLf & "�� PCL λ������� C �̣�����Գ��Խ���Ų�������ļ��У�����ܿ��Խ��Ȩ�����⡣" & vbCrLf + GetExceptionSummary(ex4), MsgBoxStyle.Critical, "����ʧ��")
            Catch ex5 As Exception
                MsgBox("PCL ����ʧ�ܣ��޷��������ļ������ֶ����� PCL �ļ����µ��°汾����" & vbCrLf + GetExceptionSummary(ex5), MsgBoxStyle.Critical, "����ʧ��")
                Return
            End Try
            If TriggerRestart Then
                Try
                    Process.Start(OldFileName)
                Catch ex6 As Exception
                    MsgBox("PCL ����ʧ�ܣ��޷�����������" & vbCrLf + GetExceptionSummary(ex6), MsgBoxStyle.Critical, "����ʧ��")
                End Try
            End If
            Return
        End If
        If TypeOf ex2 Is UnauthorizedAccessException Then
            MsgBox(String.Concat(New String() {"����Ȩ�޲��㣬PCL �޷���ɸ��¡��볢�ԣ�" & vbCrLf,
                                 If((Path.StartsWithF(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), False) OrElse Path.StartsWithF(Environment.GetFolderPath(Environment.SpecialFolder.Personal), False)),
                                 " - �� PCL �ļ��ƶ������桢�ĵ�������ļ��У���������һ�����ݵؽ��Ȩ�����⣩" & vbCrLf, ""),
                                 If(Path.StartsWithF("C", True),
                                 " - �� PCL �ļ��ƶ��� C ��������ļ��У���������һ�����ݵؽ��Ȩ�����⣩" & vbCrLf, ""),
                                 " - �Ҽ��Թ���Ա������� PCL" & vbCrLf & " - �ֶ����������ص� PCL �ļ����µ��°汾���򣬸���ԭ����" & vbCrLf & vbCrLf,
                                 GetExceptionSummary(ex2)}), MsgBoxStyle.Critical, "����ʧ��")
            Return
        End If
        MsgBox("PCL ����ʧ�ܣ��޷�ɾ��ԭ�ļ������ֶ����������ص� PCL �ļ����µ��°汾���򸲸�ԭ����" & vbCrLf + GetExceptionSummary(ex2), MsgBoxStyle.Critical, "����ʧ��")
    End Sub
    ''' <summary>
    ''' ȷ�� PathTemp �µ� Latest.exe ��������ʽ��� PCL�����ᱻ�������ϰ������
    ''' ������ǣ�������һ����
    ''' </summary>
    Friend Sub DownloadLatestPCL(Optional LoaderToSyncProgress As LoaderBase = Nothing)
        'ע�⣺���Ҫ����ʵ��������ܣ��뻻����һ���ļ�·����������ٷ��汾��ͻ
        Dim LatestPCLPath As String = PathTemp & "CE-Latest.exe"
        Dim LatestPCLTempPath As String = PathTemp & "CE-Latest.zip"
        Dim LatestInfo As UpdateAssetInfo = GetChannelInfo("sr")
        If File.Exists(LatestPCLPath) AndAlso GetFileSHA256(LatestPCLPath) = LatestInfo.sha256 Then
            Log("[System] ���°� PCL �Ѵ��ڣ���������")
            Exit Sub
        End If
        If GetFileSHA256(PathWithName) = LatestInfo.sha256 Then
            CopyFile(PathWithName, LatestPCLPath)
            Exit Sub
        End If
        NetDownloadByLoader(LatestInfo.downloads, LatestPCLTempPath, LoaderToSyncProgress)
        Using archive = New ZipArchive(New FileStream(LatestPCLTempPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), ZipArchiveMode.Read)
            Dim entry As ZipArchiveEntry = archive.Entries.FirstOrDefault(Function(x) x.FullName.EndsWithF("Plain Craft Launcher Community Edition.exe"))
            If entry IsNot Nothing Then
                entry.ExtractToFile(LatestPCLPath, True)
            End If
        End Using
        File.Delete(LatestPCLTempPath)
    End Sub

#End Region

#Region "����֪ͨ"

    Public ServerLoader As New LoaderTask(Of Integer, Integer)("PCL ����", AddressOf LoadOnlineInfo, Priority:=ThreadPriority.BelowNormal)

    Private Sub LoadOnlineInfo()
        Dim UpdateDesire = Setup.Get("SystemSystemUpdate")
        Dim AnnouncementDesire = Setup.Get("SystemSystemActivity")
        If UpdateDesire <= 1 OrElse AnnouncementDesire <= 1 Then
            RefreshUpdatesCache()
        End If
        Select Case UpdateDesire
            Case 0
                Dim LatestVersion = GetChannelInfo()
                If LatestVersion.version.code > VersionCode Then
                    UpdateStart(LatestVersion, True) '��Ĭ����
                End If
            Case 1
                NoticeUserUpdate(True)
            Case 2, 3
                Exit Sub
        End Select
        If AnnouncementDesire <= 1 Then
            Dim ShowedAnnounced = Setup.Get("SystemSystemAnnouncement").ToString().Split("|").ToList()
            Dim ShowAnnounce = RemoteAnnounceData.content.Where(Function(x) Not ShowedAnnounced.Contains(x.id)).ToList()
            Log("[System] ��Ҫչʾ�Ĺ���������" + ShowAnnounce.Count.ToString())
            RunInNewThread(Sub()
                               For Each item In ShowAnnounce
                                   Dim SelectedBtn = MyMsgBox(
                                   item.detail,
                                   item.title,
                                   If(item.btn1 Is Nothing, "", item.btn1.text),
                                   If(item.btn2 Is Nothing, "", item.btn2.text),
                                   "�ر�",
                                   Button1Action:=Sub()
                                                      TryStartEvent(item.btn1.command, item.btn1.command_paramter)
                                                  End Sub,
                                   Button2Action:=Sub()
                                                      TryStartEvent(item.btn2.command, item.btn2.command_paramter)
                                                  End Sub
)
                               Next
                           End Sub)
            ShowedAnnounced.AddRange(ShowAnnounce.Select(Function(x) x.id).ToList())
            ShowedAnnounced = ShowedAnnounced.Distinct().ToList()
            Setup.Set("SystemSystemAnnouncement", ShowedAnnounced.Join("|"))
        End If
    End Sub

#End Region

End Module
