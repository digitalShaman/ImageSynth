'ImageSynth by D.Aue. This is all provided as is and in no aspect optimized for performance.
' V 1.1.0.0  04.2019 OpenGl 
' V 1.0.0.2  07.2018 Performancetune Voronoi, performance tune synth: berechnungspfad vorermittelt 
' V 1.0.0.1  07.2018 Location/Scale bei allen Generatoren angepasst, Histogramm ergänzt.
'

Imports System.Runtime.InteropServices
Imports OpenTK

Public Class frmMain
    Private WithEvents oSynth As New ImageSynth

    Private ptMouse As Point
    Private ptMouseDown As Point

    Private sLoadedFile As String
    Private sLastSaved As String    'zuletzt gespeicherter zustand

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ConfigManager.Client = pnlConfig
        SetupTest()

        UpdateWindowText()
        UpdateImage()
        ConfigManager.DrawConfig()
    End Sub


    Private Sub UpdateWindowText()
        Dim sFile As String = IO.Path.GetFileName(sLoadedFile)
        Me.Text = "Image Synth Version " & Application.ProductVersion & If(sFile <> "", " - " & sFile & If(sLastSaved <> ltUndoLevels(iCurrentUndolevel).Data, "*", ""), "")
    End Sub

    Private swTiming As New Stopwatch
    Private Sub UpdateImage()
        lblProgress.Text = "Berechne..."
        swTiming.Reset()
        swTiming.Start()
        oSynth.GenerateImageAsync(picOutput.Width, picOutput.Height)
    End Sub

    Private Sub tmr_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles tmr.Tick
        Static dtCalcfinished As DateTime
        If oSynth.IsRunning Then
            lblProgress.Visible = True
            picProgress.Visible = True
            dtCalcfinished = DateTime.MinValue
            picProgress.Invalidate()
            picOutput.Invalidate()
        ElseIf dtCalcfinished = DateTime.MinValue Then
            picProgress.Invalidate()
            dtCalcfinished = DateTime.Now
        ElseIf (DateTime.Now - dtCalcfinished).TotalSeconds > 1 Then
            lblProgress.Visible = False
            picProgress.Visible = False
        End If
    End Sub

    Private Sub oSynth_ImageReady(ByVal IsError As Boolean) Handles oSynth.ImageReady
        If picOutput.InvokeRequired Then
            Me.Invoke(Sub() oSynth_ImageReady(IsError))
        Else
            If IsError Then
                MessageBox.Show("Fehler bei der Berechnung!")
            Else
                If swTiming.IsRunning Then
                    swTiming.Stop()
                    lblProgress.Text = $"{swTiming.ElapsedMilliseconds} ms"
                End If
                If Me.IsHandleCreated Then
                    picOutput.Image = oSynth.OutputImage
                End If
            End If
        End If
    End Sub

    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        oSynth.CleanUp()
        View?.Close()
    End Sub

    Private Sub btnHelp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnHelp.Click
        frmHelp.Show()
    End Sub

    Private Sub AddFunctionButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddFunction.Click
        Dim frm As New frmChooseFunction
        frm.ShowDialog()
        If frm.SelectedFunction IsNot Nothing Then
            Dim oNewItem As New ConfigInstance With {.Function = frm.SelectedFunction}
            AddModule(oNewItem)
            ConfigManager.DrawConfig()
            UpdateWindowText()
        End If
    End Sub


    Private Sub btnSize_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSize.Click
        'bild aus volle fenster vergrößern
        If btnSize.Text = ">" Then
            btnSize.Text = "<"
            pnlOutput.Width = Me.ClientSize.Width - picOutput.Left - 20
            pnlOutput.Height = Me.ClientSize.Height - picOutput.Top - btnSize.Height
        Else
            btnSize.Text = ">"
            pnlOutput.Location = New Point(12, 12)
            pnlOutput.Size = New Size(279, 306)
        End If
        picOutput.Image = Nothing
        For Each ctl As Control In {CType(pnlConfig, Control), btnAddFunction, btnLoad, btnSave, btnHelp, picProgress, lblProgress, btnCompile, btnClearAll}
            ctl.Visible = Not ctl.Visible
        Next
        UpdateImage()
        picOutput.Image = oSynth.OutputImage
    End Sub

    Private Sub btnClearAll_Click(sender As Object, e As EventArgs) Handles btnClearAll.Click
        If MessageBox.Show("Wirklich alle aktuellen Module löschen und neu beginnen?", "Sicher?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            ConfigInstances.Where(Function(o) o.Function.Type <> FunctionType.ImageOutput).ToList.ForEach(Sub(o) RemoveModule(o))
            ConfigInstances(0).Rect.Location = New Point(500, 50)

            UpdateImage()
            ConfigManager.DrawConfig()
            UpdateWindowText()
        End If
    End Sub

    Private Sub btnDebug_Click(sender As Object, e As EventArgs) Handles btnDebug.Click
        Dim xml As New Xml.XmlDocument
        xml.Load("C:\Users\shaman\Desktop\Generierte Texturen mit OpenTk Shader\Image SynthOGL\Examples\test.xml")
        LoadXml(xml)
        UpdateImage()
        UpdateWindowText()
    End Sub




    Private Sub picConfig_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles picConfig.Paint
        ConfigManager.PaintConfig(e.Graphics, ptMouse)
    End Sub


    Private Sub picOutput_Paint(ByVal sender As Object, ByVal e As PaintEventArgs) Handles picOutput.Paint
        Dim oInstance As ConfigInstance = ConfigInstances.FirstOrDefault(Function(o) o.[Function].IsSolo)
        If oInstance IsNot Nothing Then
            'ein Solo ist aktiv -> min und max werte einblenden
            Dim sText As String = String.Format("Min: {0:N3}, Max: {1:N3}", oSynth.LastMinValue, oSynth.LastMaxValue)
            Dim oFont As Font = New Font("Microsoft Sans Serif", 15, GraphicsUnit.Pixel)
            Dim fSize As SizeF = e.Graphics.MeasureString(sText, oFont)
            Dim rect As Rectangle = New Rectangle(10, 10, CInt(fSize.Width), CInt(fSize.Height))
            e.Graphics.FillRectangle(New SolidBrush(Color.FromArgb(150, 0, 0, 0)), rect)
            e.Graphics.DrawString(sText, oFont, Brushes.HotPink, New Point(10, 10))
            If oSynth.IsNormalized Then
                sText = "Normalized"
                fSize = e.Graphics.MeasureString(sText, oFont)
                rect = New Rectangle(10, rect.Bottom + 2, CInt(fSize.Width), CInt(fSize.Height))
                e.Graphics.FillRectangle(New SolidBrush(Color.FromArgb(150, 0, 0, 0)), rect)
                e.Graphics.DrawString(sText, oFont, Brushes.LightGreen, New Point(10, rect.Top))
            End If

            'Histogramm
            Dim xPos As Int32 = 0
            e.Graphics.DrawLines(Pens.Red, oSynth.Histogram.Select(Function(f)
                                                                       Dim pt As New Point(xPos, CInt(picOutput.Height - (f * 10 * picOutput.Height)))
                                                                       xPos += 1
                                                                       Return pt
                                                                   End Function).ToArray)
        End If
    End Sub



    Private Sub picProgress_Paint(ByVal sender As Object, ByVal e As PaintEventArgs) Handles picProgress.Paint
        e.Graphics.Clear(Color.Empty)
        e.Graphics.FillRectangle(Brushes.LightGreen, New Rectangle(0, 0, CInt(picProgress.Width * oSynth.Progress), picProgress.Height))
    End Sub


#Region "Mousehandling über Konfiguration"
    Private Sub picConfig_MouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles picConfig.MouseDoubleClick
        'bei doppelklick auf parameter->input fenster, bei Repeater->Table
        If ConfigInstances Is Nothing Then Exit Sub

        Dim oInstance As ConfigInstance = ConfigInstances.LastOrDefault(Function(o) o.IsHit(e.Location))
        If oInstance IsNot Nothing Then
            Dim iHittestResult As ConfigInstance.HittestResult = oInstance.Hittest(e.Location)
            Select Case iHittestResult And &HFF00
                Case ConfigInstance.HittestResult.ParameterValue, ConfigInstance.HittestResult.ParameterName
                    'doppelklick auf parametername/value
                    If oInstance.Function.Type = FunctionType.Repeater Then
                        Dim oRep As Repeater = CType(oInstance.[Function], Repeater)
                        Dim frm As New frmRepeaterValues
                        frm.Display(oRep, AddressOf UpdateImage)
                    Else
                        Dim iParameter As Int32 = iHittestResult And &HFF
                        Dim frm As New frmEnterValue
                        frm.Display(oInstance, iParameter)

                        frm.ShowDialog()
                        If frm.IsValid Then
                            SetParameterValue(oInstance, iParameter, frm.Value)
                        End If
                    End If

                    UpdateImage()
                    ConfigManager.DrawConfig()
                    UpdateWindowText()
            End Select
        End If
    End Sub


    Private Sub picConfig_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles picConfig.MouseDown
        If ConfigInstances Is Nothing Then Exit Sub

        ptMouseDown = e.Location

        Dim oInstance As ConfigInstance = ConfigInstances.LastOrDefault(Function(o) o.IsHit(ptMouseDown))
        If oInstance IsNot Nothing Then
            Dim iHittestResult As ConfigInstance.HittestResult = oInstance.Hittest(ptMouseDown)
            Dim isHandled As Boolean = False
            Dim isRedraw As Boolean = False
            Select Case iHittestResult
                Case ConfigInstance.HittestResult.Delete
                    'Löschen
                    RemoveModule(oInstance)
                    isRedraw = True

                Case ConfigInstance.HittestResult.Info
                    Dim frm As New frmModuleInfo
                    frm.lblText.Text = oInstance.[Function].Help
                    frm.Show()
                    isHandled = True

                Case ConfigInstance.HittestResult.Solo
                    ToggleSolo(oInstance)
                    isRedraw = True

                Case ConfigInstance.HittestResult.Mute
                    ToggleMute(oInstance)
                    isRedraw = True

                Case ConfigInstance.HittestResult.OutputConnector
                    'neue verbindung am output
                    oInstance.IsNewOutputAdd = True
                    oInstance.NewOutputAddPin = -1
                    isHandled = True

                Case Is > ConfigInstance.HittestResult.OutputConnector
                    Dim iParameter As Int32 = iHittestResult And &HFF
                    Select Case iHittestResult And &HFF00
                        'Parameter zeugs
                        Case ConfigInstance.HittestResult.ParameterInc, ConfigInstance.HittestResult.ParameterDec
                            '+/-
                            HandleParameterMouseDown(oInstance, iParameter, CType((iHittestResult And &HFF00), ConfigInstance.HittestResult))
                            isRedraw = True

                        Case ConfigInstance.HittestResult.InputConnector
                            If oInstance.Function.Type = FunctionType.Repeater Then
                                'neue verbindung am Ausgang eines Repeaters
                                oInstance.IsNewOutputAdd = True
                                oInstance.NewOutputAddPin = iParameter
                                isHandled = True
                            Else
                                If oInstance.Function.Name = "Add" Then
                                    iParameter = 1  'beim add immer den Signal Parameter nehmen
                                End If

                                'klick an parameter connector -> ist da was verbunden?
                                Dim oInstanceConnectedFrom As ConfigInstance
                                oInstanceConnectedFrom = ConfigInstances.Where(
                                        Function(o) o.[Function].Outputs.Any(
                                            Function(o2) o2.ToFunction.Equals(oInstance.[Function]) _
                                                         AndAlso o2.ToPin = iParameter
                                        )).OrderBy(Function(o) o.ConnectorHitscore(ptMouseDown, oInstance)).FirstOrDefault



                                If oInstanceConnectedFrom IsNot Nothing Then
                                    'mit diesem input connector ist etwas verbunden -> Verbindung lösen und Drag Start
                                    Dim oOutput As OutputTo = oInstanceConnectedFrom.[Function].Outputs.FirstOrDefault(
                                            Function(o) o.ToFunction.Equals(oInstance.[Function]) AndAlso
                                                        o.ToPin = iParameter)
                                    HandleDisconnect(oInstanceConnectedFrom, oOutput)
                                    isRedraw = True
                                Else
                                    'mit diesem input connector ist nichts verbunden -> ignorieren
                                End If
                            End If
                    End Select
                Case Else
            End Select

            isHandled = isHandled Or isRedraw
            If Not isHandled Then
                If e.Button = MouseButtons.Left Then
                    'Drag start
                    oInstance.IsDrag = True
                    oInstance.MouseDown = New Point(oInstance.Rect.X - e.Location.X, oInstance.Rect.Y - e.Location.Y)
                    ConfigManager.DrawConfig()
                ElseIf e.Button = MouseButtons.Right Then
                    'rechtsklick -> Modul duplizieren und drag start
                    If {FunctionType.Function, FunctionType.Generator, FunctionType.Player}.ToList.Contains(oInstance.Function.Type) Then
                        Dim oNew As ConfigInstance = oInstance.Clone
                        oNew.Function.IsSolo = False
                        oNew.IsDrag = True
                        oNew.MouseDown = New Point(oInstance.Rect.X - e.Location.X, oInstance.Rect.Y - e.Location.Y)
                        AddModule(oNew, True)
                        UpdateWindowText()
                    End If
                End If
            ElseIf isRedraw Then
                UpdateImage()
                ConfigManager.DrawConfig()
                UpdateWindowText()
            End If
        End If
    End Sub



    Private Sub picConfig_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles picConfig.MouseMove
        If ConfigInstances Is Nothing Then Exit Sub

        ptMouse = e.Location

        Dim oInstance As ConfigInstance

        oInstance = ConfigInstances.FirstOrDefault(Function(o) o.IsDrag)
        If oInstance IsNot Nothing Then
            'drag eines moduls
            Dim ptNew As Point = ClampLocation(ptMouse)
            oInstance.Rect.Location = New Point(ptNew.X + oInstance.MouseDown.X, ptNew.Y + oInstance.MouseDown.Y)
            ConfigManager.DrawConfig()
        End If

        oInstance = ConfigInstances.FirstOrDefault(Function(o) o.IsNewOutputAdd)
        If oInstance IsNot Nothing Then
            'drag eines neuen output connectors
            picConfig.Invalidate()
        End If

        oInstance = ConfigInstances.FirstOrDefault(Function(o) o.IsParameterDrag)
        If oInstance IsNot Nothing AndAlso e.Button = MouseButtons.Left Then
            'drag einer Parameter value
            Dim yDiff As Int32 = oInstance.MouseDown.Y - ptMouse.Y
            Dim iSteps As Int32 = CInt(yDiff / 10)

            If iSteps <> oInstance.PrevParameterStep Then
                ScrollParameter(oInstance, oInstance.ParameterIndexAtMousedown, -(oInstance.PrevParameterStep - iSteps))
                oInstance.PrevParameterStep = iSteps
                UpdateImage()
                ConfigManager.DrawConfig()
                UpdateWindowText()
            End If
        End If

        oInstance = ConfigInstances.LastOrDefault(Function(o) o.IsHit(ptMouse))
        Static oLastHit As ConfigInstance
        If oInstance IsNot Nothing Then
            picConfig.Invalidate()  'wenn über einem modul, dann neu painten (hoover)
            oLastHit = oInstance    'und merken um hoover später wieder aufzuheben
        ElseIf oLastHit IsNot Nothing Then
            picConfig.Invalidate()  'hoover aufheben
            oLastHit = Nothing
        End If

    End Sub

    Private Sub picConfig_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles picConfig.MouseUp
        If ConfigInstances Is Nothing Then Exit Sub

        Dim isRedrawImage As Boolean

        Dim oInstance As ConfigInstance

        oInstance = ConfigInstances.FirstOrDefault(Function(o) o.IsDrag)
        If oInstance IsNot Nothing Then
            If ptMouseDown <> e.Location Then
                'ein modul wurde verschoben
                PushUndo()
                Dim ptNew As Point = ClampLocation(ptMouse)
                oInstance.Rect.Location = New Point(ptNew.X + oInstance.MouseDown.X, ptNew.Y + oInstance.MouseDown.Y)
                Do While ConfigInstances.Any(Function(o) o IsNot oInstance AndAlso
                                              Math.Abs(o.Rect.Left - oInstance.Rect.Left) < 5 AndAlso
                                              Math.Abs(o.Rect.Right - oInstance.Rect.Right) < 5 AndAlso
                                              Math.Abs(o.Rect.Top - oInstance.Rect.Top) < 5 AndAlso
                                              Math.Abs(o.Rect.Bottom - oInstance.Rect.Bottom) < 5)
                    oInstance.Rect.Offset(10, 10)
                Loop
            End If
            oInstance.IsDrag = False
            ConfigManager.DrawConfig()
            UpdateWindowText()
        End If

        oInstance = ConfigInstances.FirstOrDefault(Function(o) o.IsNewOutputAdd)
        If oInstance IsNot Nothing Then
            'drop eines neuen output connectors
            Dim oInstanceConnectedTo As ConfigInstance = ConfigInstances.LastOrDefault(Function(o) o.IsHit(e.Location))
            If oInstanceConnectedTo IsNot Nothing AndAlso Not oInstance.Equals(oInstanceConnectedTo) Then
                Dim iHittestResult As ConfigInstance.HittestResult = oInstanceConnectedTo.Hittest(e.Location)
                If (iHittestResult And &HFF00) = ConfigInstance.HittestResult.InputConnector Then
                    'Mouse up auf einem Input Connector
                    Dim iParameter As Int32 = iHittestResult And &HFF
                    'connection hinzufügen

                    HandleConnect(oInstance, oInstance.NewOutputAddPin, oInstanceConnectedTo, iParameter)
                    isRedrawImage = True
                End If
            End If

            oInstance.IsNewOutputAdd = False
            If isRedrawImage Then
                UpdateImage()
                UpdateWindowText()
            End If
            ConfigManager.DrawConfig()
        End If


        oInstance = ConfigInstances.FirstOrDefault(Function(o) o.IsParameterDrag)
        If oInstance IsNot Nothing Then
            oInstance.IsParameterDrag = False
        End If

    End Sub


    Private Sub PicMouseWheel(ByVal sender As Object, ByVal e As MouseEventArgs) Handles picConfig.MouseWheel
        Dim oInstance As ConfigInstance = ConfigInstances.LastOrDefault(Function(o) o.IsHit(e.Location))
        If oInstance IsNot Nothing Then
            Dim iHittestResult As ConfigInstance.HittestResult = oInstance.Hittest(e.Location)
            Select Case iHittestResult And &HFF00
                Case ConfigInstance.HittestResult.ParameterInc, ConfigInstance.HittestResult.ParameterDec

                    Dim iParameter As Int32 = iHittestResult And &HFF
                    '+/-
                    ScrollParameter(oInstance, iParameter, CInt(e.Delta / 120))

                    UpdateImage()
                    ConfigManager.DrawConfig()
                    UpdateWindowText()
            End Select
        End If
    End Sub

    Private Function ClampLocation(ByVal MouseLocation As Point) As Point
        Dim ptNew As Point
        ptNew.X = If(MouseLocation.X < 0, 0, MouseLocation.X)
        ptNew.Y = If(MouseLocation.Y < 0, 0, MouseLocation.Y)
        Return ptNew
    End Function

    Private Sub pnlConfig_SizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles pnlConfig.SizeChanged
        If Me.IsHandleCreated And Not Me.WindowState = FormWindowState.Minimized Then
            ConfigManager.DrawConfig()
        End If
    End Sub
#End Region




#Region "Open GL"
    'OpenGl View
    Private View As Scene

    Private Sub btnCompile_Click(sender As Object, e As EventArgs) Handles btnCompile.Click
        View?.Close()
        View?.Dispose()

        If Not oSynth.IsRunning AndAlso oSynth.Functions.Any(Function(o) o.IsSolo) Then
            If oSynth.LastMinValue >= 1 Then
                MessageBox.Show("Alle Ausgabewerte sind größer oder gleich 1.0. Das Bild wird nur weiß sein.", "Warnung:")
            ElseIf oSynth.LastMaxValue <= 0 Then
                MessageBox.Show("Alle Ausgabewerte sind kleiner oder gleich 0.0. Das Bild wird nur schwarz sein.", "Warnung:")
            End If
        ElseIf oSynth.LastMinValue = oSynth.LastMaxValue Then
            MessageBox.Show("Alle Ausgabewerte haben die selbe Farbe.", "Warnung:")
        End If

        Dim isImageUpdateRequired As Boolean = False
        If oSynth.Functions.Any(Function(o) Not o.HasOpenGlSupport AndAlso Not o.IsMute) OrElse oSynth.Repeaters.Count > 0 Then
            Dim ltNotSupported As List(Of IFunction) = oSynth.Functions.Where(Function(o) Not o.HasOpenGlSupport AndAlso Not o.IsMute).ToList
            MessageBox.Show("Die folgenden Module haben keine Open GL Implementierung und werden gemutet/ignoriert:" & Environment.NewLine &
                            String.Join(","c, ltNotSupported.Union(oSynth.Repeaters).Select(Function(o) o.Name).Distinct), "Warnung:", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            ltNotSupported.ForEach(Sub(o) o.IsMute = True)
            isImageUpdateRequired = True
            ConfigManager.DrawConfig()
        End If

        View = New Scene
        Dim isGood As Boolean = View.UploadScene(oSynth)
        If isImageUpdateRequired Then
            PushUndo()
            UpdateImage()
            UpdateWindowText()
        End If
        If isGood Then
            ResetGameRotation()
            View.Run()
        End If
    End Sub

    Private Sub frmMain_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        RegisterInput(Me.Handle)
    End Sub





#Region "Mouse Event"

    Private Sub MouseEvent(MouseData As RAWMOUSE)
        If View Is Nothing Then Exit Sub

        Dim xDelta As Double = MouseData.lLastX / 1600.0F * Math.PI
        Dim yDelta As Double = MouseData.lLastY / 1600.0F * Math.PI

        If xDelta <> 0 OrElse yDelta <> 0 Then
            Gamerotation.X += CSng(yDelta)

            If Gamerotation.X < -Math.PI / 2 Then
                Gamerotation.X = CSng(-Math.PI) / 2
            ElseIf Gamerotation.X > Math.PI / 2 Then
                Gamerotation.X = CSng(Math.PI) / 2
            End If

            Gamerotation.Z += CSng(xDelta)
        End If

        If (MouseData.Wheel <> 0) Then
            Dim fDisplace As Single = CSng(0.01 * MouseData.Wheel)
            View.CameraPos = -GetLookDirectionPoint(fDisplace)
        End If
    End Sub

    Private Function GetLookDirectionPoint(Distance As Single) As Vector3
        Dim rot As Matrix3 = Matrix3.Mult(
                          Matrix3.CreateRotationX(-Gamerotation.X + CSng(Math.PI / 2)),
                          Matrix3.CreateRotationZ(-Gamerotation.Z)
                          )
        Dim vecCurrentLookAt As Vector3 = New Vector3(0, 0, -Distance) * rot + -View.CameraPos
        Return vecCurrentLookAt
    End Function

    Private Sub KeyboardEvent(KeyboardData As RAWKEYBOARD)

    End Sub



#Region "Register Raw Input"
    'für mouse bewegung und WD Steuerung

    Private Declare Function RegisterRawInputDevices Lib "User32.dll" (ByVal pRawInputDevice As RAWINPUTDEVICE(),
                                                                       ByVal uiNumDevices As UInteger, ByVal cbSize As Integer) As Boolean

    Private Declare Auto Function GetRawInputData Lib "User32.dll" _
        (ByVal hRawInput As IntPtr, ByVal uiCommand As UInteger, <Out> ByRef pData As RAWINPUT, ByRef pcbSize As Integer, ByVal cbSizeHeader As Integer) As Integer

    Private Structure RAWINPUTDEVICE
        Public usUsagePage As UShort
        Public usUsage As UShort
        Public dwFlags As Int32
        Public hwndTarget As IntPtr
    End Structure


    'achtung: offset 16 klappt nur als 32 bit applikation! als 64bit sinds 20, da hDevice dann 8 bytes sind
    <StructLayout(LayoutKind.Explicit)>
    Private Structure RAWINPUT
        Public header As RAWINPUTHEADER
        <FieldOffset(16)>
        Public mouse As RAWMOUSE
        <FieldOffset(16)>
        Public keyboard As RAWKEYBOARD
    End Structure

    Private Structure RAWINPUTHEADER
        Public dwType As Int32
        Public dwSize As Int32
        Public hDevice As IntPtr
        Public wParam As Int32
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure RAWMOUSE
        Public usFlags As UShort
        Public usButtonFlags As UInt16
        Public usButtonData As UInt16
        Public Wheel As Int16
        Public ulRawButtons As UInt32
        Public lLastX As Int32
        Public lLastY As Int32
        Public ulExtraInformation As UInt32
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure RAWKEYBOARD
        Public MakeCode As UShort
        Public Flags As UShort
        Public Reserved As UShort
        Public VKey As UShort
        Public Message As UInt16
        Public ExtraInformation As ULong
    End Structure



    Private Const RIDEV_EXINPUTSINK As Int32 = &H1000
    Private Const RID_INPUT As Int32 = &H10000003
    Private Const RIM_TYPEMOUSE As Int32 = 0
    Private Const RIM_TYPEKEYBOARD As Int32 = 1
    Private Const WM_INPUT As Int32 = &HFF
    Private Const WM_KEYDOWN As Int32 = &H100
    Private Const WM_KEYUP As Int32 = &H101


    Private Sub RegisterInput(ByVal hwnd As IntPtr)
        Dim devs As RAWINPUTDEVICE() = New RAWINPUTDEVICE(1) {}
        devs(0).usUsagePage = &H1
        devs(0).usUsage = &H2
        devs(0).dwFlags = RIDEV_EXINPUTSINK
        devs(0).hwndTarget = hwnd

        devs(1).usUsagePage = &H1
        devs(1).usUsage = &H6
        devs(1).dwFlags = RIDEV_EXINPUTSINK
        devs(1).hwndTarget = hwnd

        If Not RegisterRawInputDevices(devs, 2, Marshal.SizeOf(devs(0))) Then
            Throw New Exception("Fehler beim Registrieren von RawInput!")
        End If
    End Sub


    Protected Overrides Sub WndProc(ByRef m As Message)
        MyBase.WndProc(m)

        If m.Msg = WM_INPUT Then
            Dim data As RAWINPUT = New RAWINPUT()
            Dim size As Int32 = Marshal.SizeOf(data)
            Dim result As Integer = GetRawInputData(m.LParam, RID_INPUT, data, size, Marshal.SizeOf(GetType(RAWINPUTHEADER)))

            If result > 0 Then

                If data.header.dwType = RIM_TYPEMOUSE Then
                    MouseEvent(data.mouse)

                ElseIf data.header.dwType = RIM_TYPEKEYBOARD Then
                    KeyboardEvent(data.keyboard)

                End If
            End If
        End If
    End Sub







#End Region

#End Region

#End Region



    Private iCurrentUndolevel As Int32 = -1
    Private ltUndoLevels As New List(Of UndoLevel)

    Private Class UndoLevel
        Public dt As DateTime
        Public Data As String
    End Class

    Private Sub PushUndo()
        Dim xml As Xml.XmlDocument = SaveXml()

        If iCurrentUndolevel + 1 < ltUndoLevels.Count Then
            'aktuell nicht am ende der kette->rest der kette verwerfen
            ltUndoLevels = ltUndoLevels.Take(iCurrentUndolevel + 1).ToList
        End If

        ltUndoLevels.Add(New UndoLevel With {.Data = xml.InnerXml, .dt = DateTime.Now})

        'wenn innerhalb der letzten sekunde mehr als x events waren, dann alle bis auf das erste und das letzte rausschmeissen
        Dim ltToCrunch As IEnumerable(Of UndoLevel) = ltUndoLevels.Where(Function(o) (DateTime.Now - o.dt).TotalSeconds < 1).ToList
        If ltToCrunch.Count > 3 Then
            Dim ltKeep As IEnumerable(Of UndoLevel) = {ltToCrunch.First, ltToCrunch.Last}
            ltToCrunch.Except(ltKeep).ToList.ForEach(Sub(o) ltUndoLevels.Remove(o))
        End If

        'max 100 levels
        If ltUndoLevels.Count > 100 Then
            ltUndoLevels = ltUndoLevels.Skip(ltUndoLevels.Count - 100).ToList
        End If
        iCurrentUndolevel = ltUndoLevels.Count - 1  'am ende zeigen wir immer auf den aktuellsten eintrag
    End Sub

    Private Sub PopUndo()
        If iCurrentUndolevel > 0 Then
            Dim xml As New Xml.XmlDocument
            iCurrentUndolevel -= 1
            xml.LoadXml(ltUndoLevels(iCurrentUndolevel).Data)
            LoadXml(xml)
            UpdateImage()
            UpdateWindowText()
        End If
    End Sub

    Private Sub Redo()
        If iCurrentUndolevel + 1 < ltUndoLevels.Count Then
            Dim xml As New Xml.XmlDocument
            iCurrentUndolevel += 1
            xml.LoadXml(ltUndoLevels(iCurrentUndolevel).Data)
            LoadXml(xml)
            UpdateImage()
            UpdateWindowText()
        End If
    End Sub


    Private Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click

        Dim dlg As New SaveFileDialog
        dlg.Filter = ".xml|*.xml|Bitmap|*.bmp"
        If sLoadedFile <> "" Then
            dlg.FileName = IO.Path.GetFileName(sLoadedFile)
        End If
        dlg.ShowDialog()

        Dim sPath As String = dlg.FileName

        If dlg.FilterIndex = 1 Then
            If Not sPath.ToLower.EndsWith(".xml") Then
                sPath &= ".xml"
            End If

            Dim xml As Xml.XmlDocument = SaveXml()
            xml.Save(sPath)
            sLoadedFile = sPath
            sLastSaved = xml.InnerXml
            UpdateWindowText()
        Else
            If Not sPath.ToLower.EndsWith(".bmp") Then
                sPath &= ".bmp"
            End If
            picOutput.Image.Save(sPath)
        End If

    End Sub

    Private Sub btnLoad_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnLoad.Click
        oSynth.AbortCalculation()

        Dim dlg As New OpenFileDialog
        dlg.Filter = ".xml|*.xml"
        dlg.ShowDialog()


        If dlg.FileName.ToLower.EndsWith(".xml") Then
            sLoadedFile = dlg.FileName
            Dim xml As New Xml.XmlDocument
            xml.Load(sLoadedFile)
            LoadXml(xml)
            sLastSaved = xml.InnerXml
            ltUndoLevels.Clear()
            iCurrentUndolevel = 0
            PushUndo()

            UpdateImage()
            UpdateWindowText()
        End If

    End Sub






    Private Sub SetupTest()
        Dim oNoise As New SimplexNoise
        oNoise.SetParameterValue("Intensity", 1)
        Dim oSinus As New Sinus
        Dim oVoronoi As New Voronoi
        Dim oImage As New ImageOutput

        oVoronoi.AddOutput(New OutputTo With {.ToFunction = oImage, .ToPin = 0})
        oVoronoi.AddOutput(New OutputTo With {.ToFunction = oImage, .ToPin = 2})

        ' Noise schickt output an sinus
        'oNoise.AddOutput(New OutputTo With {.ToFunction = oSinus, .ToPin = 0})

        ' sinus schickt an image GB
        oSinus.AddOutput(New OutputTo With {.ToFunction = oImage, .ToPin = 1})
        oSinus.AddOutput(New OutputTo With {.ToFunction = oImage, .ToPin = 2})

        oSynth.Functions.AddRange({oSinus, oVoronoi, oImage})

        'oSynth.Players.Add(New FunctionInstance With {.Function = New LinearPlayer})

        AddInstance(New ConfigInstance With {.Function = oSinus})
        AddInstance(New ConfigInstance With {.Function = oVoronoi})
        AddInstance(New ConfigInstance With {.Function = oImage})

        Dim pt As New Point(10, 10)
        ConfigInstances.ForEach(Sub(o)
                                    o.Rect.Location = pt
                                    pt.Offset(250, 50)
                                End Sub)
        PushUndo()
    End Sub

    Private Sub LoadXml(Data As Xml.XmlDocument)
        ConfigInstances.Clear()


        oSynth.AbortCalculation()
        SyncLock oSynth.Functions
            oSynth.Functions.Clear()
        End SyncLock

        For Each nd As Xml.XmlElement In Data.SelectNodes("Main/ConfigInstance")
            Dim oInstance As New ConfigInstance(nd)
            AddModule(oInstance, True)
        Next


        'jetzt sind alle instanzen verfügbar->die connections aufbauen
        ConfigInstances.ForEach(Sub(o)
                                    o.Function.Outputs.ForEach(Sub(o2)
                                                                   If Not o2.InitAfterLoad(ConfigInstances, o.Function) Then
                                                                       o.[Function].Outputs.Remove(o2)
                                                                   End If
                                                               End Sub)
                                End Sub)

        ConfigManager.DrawConfig()
    End Sub


    Private Function SaveXml() As Xml.XmlDocument
        oSynth.AbortCalculation()

        Dim xml As New Xml.XmlDocument
        Dim ndMain As Xml.XmlNode = xml.CreateElement("Main")
        xml.AppendChild(ndMain)

        ConfigInstances.SelectMany(Function(o) o.[Function].Outputs).ToList.
                ForEach(Sub(o)
                            o.PrepareSave()
                        End Sub)

        For Each oInstance As ConfigInstance In ConfigInstances
            oInstance.Serialize(ndMain)
        Next

        Return xml
    End Function

    Private Sub AddModule(Instance As ConfigInstance, Optional NoUndo As Boolean = False)
        oSynth.AbortCalculation()

        SyncLock oSynth.Functions
            If {FunctionType.Generator, FunctionType.Function, FunctionType.ImageOutput, FunctionType.Player}.Contains(Instance.Function.Type) Then
                oSynth.Functions.Add(Instance.Function)
            ElseIf Instance.Function.Type = FunctionType.Repeater Then
                oSynth.Repeaters.Add(CType(Instance.Function, Repeater))
            End If
        End SyncLock

        If Instance.Rect.IsEmpty Then
            Dim pt As New Point(pnlConfig.HorizontalScroll.Value + 10, pnlConfig.VerticalScroll.Value + 10)
            Instance.Rect.Location = pt
            Instance.UpdateParametersAndRectSize()
            Do While ConfigInstances.Any(Function(o) o.Rect.Location = pt)
                pt.Offset(10, 10)
            Loop
            Instance.Rect.Location = pt
        End If

        ConfigManager.AddInstance(Instance)

        If Not NoUndo Then PushUndo()
    End Sub


    Private Sub RemoveModule(ByVal Instance As ConfigInstance)
        oSynth.AbortCalculation()

        SyncLock oSynth.Functions
            ConfigInstances.Remove(Instance)
            oSynth.Functions.Remove(Instance.[Function])

            'alles was am output hängt lösen
            Instance.[Function].Outputs.ToList.ForEach(Sub(o) Instance.[Function].RemoveOutput(o))

            'alles was am input hängt lösen
            Dim ltReceivingFrom As List(Of IFunction) = oSynth.Functions.Where(Function(o) o.Outputs.Any(Function(o2)
                                                                                                             Return o2.ToFunction.Equals(Instance.[Function])
                                                                                                         End Function)).ToList
            ltReceivingFrom.ForEach(Sub(o)
                                        Dim ltOutputs As List(Of OutputTo) = o.Outputs.Where(Function(o2) o2.ToFunction.Equals(Instance.[Function])).ToList
                                        ltOutputs.ForEach(Sub(o2) o.RemoveOutput(o2))
                                    End Sub)
        End SyncLock
        PushUndo()
    End Sub


    Private Sub HandleParameterMouseDown(ByVal Instance As ConfigInstance, ByVal ParameterIndex As Int32, ByVal Direction As ConfigInstance.HittestResult)
        ScrollParameter(Instance, ParameterIndex, If(Direction = ConfigInstance.HittestResult.ParameterInc, 1, -1))
        UpdateWindowText()

        Instance.IsParameterDrag = True
        Instance.MouseDown = ptMouseDown
        Instance.PrevParameterStep = 0
        Instance.ParameterIndexAtMousedown = ParameterIndex
    End Sub

    Private Sub SetParameterValue(Instance As ConfigInstance, Parameter As Int32, Value As Double)
        Instance.[Function].SetParameterValue(Parameter, Value)
        PushUndo()
    End Sub


    Private Sub HandleDisconnect(ByVal FromInstance As ConfigInstance, ByVal Output As OutputTo)

        oSynth.AbortCalculation()

        SyncLock oSynth.Functions
            FromInstance.[Function].RemoveOutput(Output)
        End SyncLock

        FromInstance.IsNewOutputAdd = True
        FromInstance.NewOutputAddPin = Output.FromPin
        PushUndo()
    End Sub

    Private Sub HandleConnect(ByVal FromInstance As ConfigInstance, FromPin As Int32, ToInstance As ConfigInstance, ToPin As Int32)

        If ToInstance.Function.Name = "Add" Then
            ToPin = 1   'add immer am Value2 verbinden
        End If

        Dim oNewOutput As New OutputTo With {.ToFunction = ToInstance.Function,
                                                                .ToPin = ToPin,
                                                                .FromPin = FromPin}
        oSynth.AbortCalculation()

        SyncLock oSynth.Functions
            FromInstance.[Function].AddOutput(oNewOutput)
        End SyncLock

        PushUndo()
    End Sub


    Private Sub ToggleSolo(ByVal Instance As ConfigInstance)

        Dim oPrevSolo As ConfigInstance = ConfigInstances.FirstOrDefault(Function(o) o.[Function].IsSolo = True AndAlso Not o.Equals(Instance))
        If oPrevSolo IsNot Nothing Then
            oPrevSolo.[Function].IsSolo = False
        End If
        Instance.[Function].IsSolo = Not Instance.[Function].IsSolo
        If Instance.[Function].IsSolo AndAlso Instance.[Function].IsMute Then
            Instance.[Function].IsMute = False
        End If
        PushUndo()
    End Sub

    Private Sub ToggleMute(ByVal Instance As ConfigInstance)
        Instance.[Function].IsMute = Not Instance.[Function].IsMute
        If Instance.[Function].IsSolo AndAlso Instance.[Function].IsMute Then
            Instance.[Function].IsSolo = False
        End If
        PushUndo()
    End Sub

    Private Sub ScrollParameter(ByVal Instance As ConfigInstance, ByVal ParameterIndex As Int32, ByVal Steps As Int32)

        Dim tbl As DataTable = Instance.Function.ParameterInfos
        Dim fValue As Double = CDbl(tbl.Rows(ParameterIndex)("Current"))
        Dim fStep As Double = CDbl(tbl.Rows(ParameterIndex)("Step"))

        If fStep = 1 Then
            SetParameterValue(Instance, ParameterIndex, fValue + fStep * Steps)
        Else
            fStep = Math.Abs(fValue * fStep) * Steps
            If Math.Abs(fStep) < 0.001 Then
                fStep = 0.001 * Math.Sign(Steps)
            End If
            SetParameterValue(Instance, ParameterIndex, fValue + fStep)
        End If
    End Sub

    Private Sub frmMain_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If e.Control AndAlso e.KeyCode = Keys.Z Then
            'undo
            PopUndo()
        ElseIf e.Control AndAlso e.KeyCode = Keys.Y Then
            Redo()
        End If
    End Sub
End Class
