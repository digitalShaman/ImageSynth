'ImageSynth by D.Aue. This is all provided as is and in no aspect optimized for performance.
' V 1.1.0.0  04.2019 OpenGl 
' V 1.0.0.2  07.2018 Performancetune Voronoi, performance tune synth: berechnungspfad vorermittelt 
' V 1.0.0.1  07.2018 Location/Scale bei allen Generatoren angepasst, Histogramm ergänzt.
'
Imports System.Runtime.InteropServices

Public Class ImageSynth
    Public Functions As New List(Of IFunction)
    Public Repeaters As New List(Of Repeater)

    Event ImageReady(ByVal IsError As Boolean)

    Public Progress As Double
    Public LastMinValue As Double = Double.MaxValue 'nur sinnvolle werte wenn solo!
    Public LastMaxValue As Double = Double.MinValue 'nur sinnvolle werte wenn solo!
    Public IsNormalized As Boolean = False
    Public Histogram() As Double 'nur sinnvolle werte wenn solo!

    Private dwData() As Int32
    Public OutputImage As Bitmap
    Private cpWidth As Int32
    Private cpHeight As Int32
    Private hData As GCHandle

    Private oCalcThread As Threading.Thread
    Private _isRunning As Boolean
    Private isExitFlag As Boolean   'zeigt dem thread an, dass er sich beeinden soll

    Public Sub GenerateImageAsync(ByVal Width As Int32, ByVal Height As Int32)
        AbortCalculation()
        Prepare(Width, Height)
        isExitFlag = False
        _isRunning = True
        oCalcThread = New Threading.Thread(AddressOf CalcImage)
        oCalcThread.Start()
    End Sub

    Public Sub AbortCalculation()
        If _isRunning Then
            isExitFlag = True
            Do While _isRunning
                Threading.Thread.Sleep(10)
            Loop
        End If
    End Sub

    ReadOnly Property IsRunning As Boolean
        Get
            Return _isRunning
        End Get
    End Property

    Private Sub CalcImage()
        Dim isError As Boolean = False
        Dim oSoloInstance As IFunction = Functions.FirstOrDefault(Function(o) o.IsSolo)

        'alle muted funktionen, wo deren output hingeht, den parameter dieser funktion auf den default wert setzen
        Dim ltMuted As List(Of IFunction) = Functions.Where(Function(o) o.IsMute).ToList
        ltMuted.ForEach(Sub(o)
                            o.Outputs.ForEach(Sub(o2)
                                                  Dim tbl As DataTable = o2.ToFunction.ParameterInfos
                                                  o2.ToFunction.SetParameterValue(o2.ToPin, CDbl(tbl.Rows(o2.ToPin)("Default")))
                                              End Sub)
                        End Sub)


        Dim fResults(cpWidth * cpHeight - 1) As Double  'array für die bilddaten und zwischenwerte
        SyncLock Functions
            Dim ltFunctionPath As List(Of IFunction) = GetFunctionPath(isError)
            If isError Then
                isExitFlag = True
            Else

                LastMinValue = Double.MaxValue
                LastMaxValue = Double.MinValue

                Dim ltOutputs As List(Of OutputTo) = ltFunctionPath.SelectMany(Function(o) o.Outputs).ToList
                Dim fX As Double
                Dim fY As Double
                Dim fThisFuncResult As Double
                For y As Int32 = 0 To cpHeight - 1
                    Progress = y / cpHeight
                    fY = y / cpHeight
                    For x As Int32 = 0 To cpWidth - 1

                        ltOutputs.ForEach(Sub(o) o.ZeroValue()) 'Alle eingäbge wo ein input signal dranhängt auf 0 setzen

                        fX = x / cpWidth

                        For Each oFunc As IFunction In ltFunctionPath
                            Dim ltConnectedRepeaters As List(Of Repeater) = Repeaters.Where(Function(o) o.Outputs.Any(Function(o2)
                                                                                                                          Return o2.ToFunction.Equals(oFunc)
                                                                                                                      End Function)).ToList
                            'Ergebniswert berechnen
                            If ltConnectedRepeaters.Count = 0 Then
                                fThisFuncResult = oFunc.Evaluate(x / cpWidth, y / cpHeight)
                            Else
                                'bei repeater berechnung x-fach wiederholen
                                ltConnectedRepeaters.SelectMany(Function(o) o.Outputs.Where(Function(o2) o2.ToFunction.Equals(oFunc))).ToList.
                                    ForEach(Sub(o2)
                                                o2.ZeroValue()
                                            End Sub)
                                Dim iIteration As Int32 = 1
                                Dim isStillIterating As Boolean = True
                                fThisFuncResult = 0
                                Do While isStillIterating
                                    isStillIterating = False
                                    For Each oRep As Repeater In ltConnectedRepeaters
                                        Dim tbl As DataTable = oRep.DataSequence
                                        If tbl.Rows.Count >= iIteration Then
                                            Dim row As DataRow = tbl.Rows(iIteration - 1)
                                            For Each oOutput As OutputTo In oRep.Outputs.Where(Function(o) o.ToFunction.Equals(oFunc)).ToList
                                                Dim oValue As Object = row(oOutput.FromPin)
                                                If Not oValue.Equals(DBNull.Value) Then
                                                    oFunc.SetParameterValue(oOutput.ToPin, CDbl(oValue))
                                                End If
                                            Next
                                            isStillIterating = True 'weitermachen solange es repeater mit genügend rows gibt
                                        End If
                                    Next
                                    If iIteration = 1 OrElse isStillIterating Then
                                        fThisFuncResult += oFunc.Evaluate(x / cpWidth, y / cpHeight)
                                        iIteration += 1
                                    End If
                                Loop
                            End If

                            'Ergebnis an folge funktionen weitergeben
                            For Each oChild As OutputTo In oFunc.Outputs
                                oChild.ToFunction.AddParameterValue(oChild.ToPin, fThisFuncResult)
                            Next
                        Next

                        fResults(y * cpWidth + x) = fThisFuncResult
                        If LastMaxValue < fThisFuncResult Then LastMaxValue = fThisFuncResult
                        If LastMinValue > fThisFuncResult Then LastMinValue = fThisFuncResult

                        If isExitFlag Then Exit For
                    Next 'nächstes pixel in x

                    'diese Zeile ins bmp übertragen
                    Dim bt As Int32
                    For xPixel As Int32 = 0 To cpWidth - 1
                        Dim fValue As Double = fResults(y * cpWidth + xPixel)
                        If oSoloInstance Is Nothing Then
                            dwData(y * cpWidth + xPixel) = CInt(fValue)
                        Else
                            If fValue > 1 Then
                                fValue = 1
                            ElseIf fValue < 0 Then
                                fValue = 0
                            End If
                            bt = CInt(fValue * 255) And &HFF
                            dwData(y * cpWidth + xPixel) = bt << 16 Or bt << 8 Or bt
                        End If
                    Next

                    If isExitFlag Then Exit For
                Next 'nächstes pixel in y
            End If
        End SyncLock

        If Not isExitFlag Then
            Array.Clear(Histogram, 0, Histogram.Length)
            Dim fStep As Double = (LastMaxValue - LastMinValue) / (Histogram.Count - 1)
            Dim fPercent As Double = 1 / (cpWidth * cpHeight)
            If fStep > 0 Then
                Array.ForEach(fResults, Sub(f) Histogram(CInt((f - LastMinValue) / fStep)) += fPercent)
            End If
        End If

        IsNormalized = False
        If Not isExitFlag AndAlso oSoloInstance IsNot Nothing Then
            'solo -> normalisieren?
            If LastMaxValue = Double.MinValue Then LastMaxValue = 0
            If LastMinValue = Double.MaxValue Then LastMinValue = 0

            IsNormalized = ((LastMinValue > 0.7) OrElse (LastMaxValue < 0.3)) AndAlso Not (LastMaxValue = LastMinValue)

            Dim fNormalize As Double = 1 / (LastMaxValue - LastMinValue)

            If IsNormalized Then
                Dim bt As Int32
                Dim fResult As Double
                For yPixel As Int32 = 0 To cpHeight - 1
                    For xPixel As Int32 = 0 To cpWidth - 1
                        fResult = fResults(yPixel * cpWidth + xPixel)
                        fResult = (fResult - LastMinValue) * fNormalize
                        If fResult > 1 Then
                            fResult = 1
                        ElseIf fResult < 0 Then
                            fResult = 0
                        End If
                        bt = CInt(fResult * 255) And &HFF
                        dwData(yPixel * cpWidth + xPixel) = bt << 16 Or bt << 8 Or bt
                    Next
                Next
            End If
        End If

        _isRunning = False
        Progress = 1
        RaiseEvent ImageReady(isError)
    End Sub


    Public Function GetFunctionPath(ByRef IsError As Boolean) As List(Of IFunction)
        'ordnet die funktionen in einer reihenfolge, sodass sie berechnet werden können

        Dim ltResult As New List(Of IFunction)

        Dim oSoloInstance As IFunction = Me.Functions.FirstOrDefault(Function(o) o.IsSolo)
        Functions.ForEach(Sub(o) o.ProvidedInputCount = 0)
        IsError = False
        Dim isFinished As Boolean = False
        Do While Functions.Any(Function(o) o.ProvidedInputCount <= o.RequiredInputCount)
            Dim ltProcessable As List(Of IFunction) = Functions.Where(
                Function(o)
                    Return (o.ProvidedInputCount = o.RequiredInputCount AndAlso
                            (
                                (o.Outputs.Count > 0 OrElse (o.Type = FunctionType.ImageOutput AndAlso oSoloInstance Is Nothing)) OrElse
                                (o.Equals(oSoloInstance))
                            )
                           )
                End Function).ToList
            If ltProcessable.Count = 0 Then
                IsError = True
                Exit Do
            End If
            For Each oFunc As IFunction In ltProcessable
                If oFunc.IsMute Then
                    For Each oChild As OutputTo In oFunc.Outputs
                        oChild.ToFunction.ProvidedInputCount += 1
                    Next
                Else
                    ltResult.Add(oFunc)
                    'Ergebnis an folge funktionen weitergeben
                    For Each oChild As OutputTo In oFunc.Outputs
                        oChild.ToFunction.ProvidedInputCount += 1
                    Next
                    isFinished = (oFunc.Type = FunctionType.ImageOutput AndAlso oSoloInstance Is Nothing) OrElse oFunc.Equals(oSoloInstance)

                End If
                oFunc.ProvidedInputCount = Int32.MaxValue
                If isFinished Then Exit For
            Next
            If isFinished Then Exit Do 'Solo oder Image erreicht->aus
        Loop

        If ltResult.Count > 0 Then
            RemoveDeadBranches(ltResult)
        End If

        Return ltResult
    End Function

    Private Sub RemoveDeadBranches(FunctionTree As List(Of IFunction))
        'tote äste die nicht in img/Solo münden entfernen, mutes aus der berechnung nehmen
        Dim oSolo As IFunction = FunctionTree.FirstOrDefault(Function(o) o.IsSolo)
        Dim ltToCheck As New List(Of IFunction)
        ltToCheck.Add(If(oSolo IsNot Nothing, oSolo, FunctionTree.Last))

        FunctionTree.ForEach(Sub(o) o.Tag = False)

        Do While ltToCheck.Count > 0
            Dim ltNextCheck As New List(Of IFunction)
            For Each oInstance As IFunction In ltToCheck
                If Not oInstance.IsMute AndAlso CBool(oInstance.Tag) = False Then
                    oInstance.Tag = True    'als benötigt markieren
                    ltNextCheck.AddRange(FunctionTree.Where(Function(o) o.Outputs.Any(Function(oOut) oOut.ToFunction Is oInstance)))
                End If
            Next
            ltToCheck = ltNextCheck
        Loop
        FunctionTree.RemoveAll(Function(o) CBool(o.Tag) = False)    'alle nicht benötigten rauswerfen
    End Sub



    Private Sub Prepare(ByVal Width As Int32, ByVal Height As Int32)
        If OutputImage IsNot Nothing AndAlso (Width <> cpWidth OrElse Height <> cpHeight) Then
            CleanUp()
        End If

        If OutputImage Is Nothing Then
            cpWidth = Width
            cpHeight = Height
            Dim cbStride As Int32 = (cpWidth * 4 + 3) And &HFFFFFFFC

            ReDim dwData(cpWidth * cpHeight - 1)
            hData = GCHandle.Alloc(dwData, GCHandleType.Pinned)
            OutputImage = New Bitmap(cpWidth, cpHeight, cbStride, Imaging.PixelFormat.Format32bppRgb, hData.AddrOfPinnedObject)

            ReDim Histogram(cpWidth - 1)
        End If
    End Sub

    Public Sub CleanUp()
        If OutputImage IsNot Nothing Then
            OutputImage.Dispose()
            OutputImage = Nothing
            hData.Free()
        End If
    End Sub
End Class
