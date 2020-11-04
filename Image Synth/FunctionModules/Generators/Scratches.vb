Public Class Scratches
    Inherits FunctionBase

    Private fIntensity As Double = 1
    Private fAdd As Double = 0
    Private iCount As Int32 = 4
    Private fWidth As Double = 0.003
    Private iSeed As Int32 = 1
    Private fMinLength As Double = 0.1
    Private fMaxLength As Double = 1
    Private fDepthMin As Double = 0.5
    Private fDepthMax As Double = 1
    Private fChaos As Double = 1
    Private xScale As Double = 1
    Private yScale As Double = 1
    Private xLocation As Double = 0
    Private yLocation As Double = 0
    Private fRotation As Double = 0


    Public Overrides ReadOnly Property Name As String
        Get
            Return "Scratches"
        End Get
    End Property

    Public Overrides ReadOnly Property Type As FunctionType
        Get
            Return FunctionType.Generator
        End Get
    End Property


    Public Overrides ReadOnly Property ParameterInfos As DataTable
        Get
            Dim tbl As New DataTable
            tbl.Columns.Add("Index", GetType(Int32))
            tbl.Columns.Add("Name", GetType(String))
            tbl.Columns.Add("Step", GetType(Double))
            tbl.Columns.Add("Default", GetType(Double))
            tbl.Columns.Add("Current", GetType(Double))

            tbl.Rows.Add(0, "Add", 0.1, 0, fAdd)
            tbl.Rows.Add(1, "Intensity", 0.1, 1, fIntensity)
            tbl.Rows.Add(2, "Count", 1, 4, iCount)
            tbl.Rows.Add(3, "Width", 0.1, 0.003, fWidth)
            tbl.Rows.Add(4, "Seed", 1, 1, iSeed)
            tbl.Rows.Add(5, "Min.Length", 0.1, 0.1, fMinLength)
            tbl.Rows.Add(6, "Max.Length", 0.1, 1, fMaxLength)
            tbl.Rows.Add(7, "Min.Depth", 0.1, 0.5, fDepthMin)
            tbl.Rows.Add(8, "Max.Depth", 0.1, 1, fDepthMax)
            tbl.Rows.Add(9, "Chaos", 0.1, 1, fChaos)
            tbl.Rows.Add(10, "Scale X", 0.1, 1, xScale)
            tbl.Rows.Add(11, "Scale Y", 0.1, 1, yScale)
            tbl.Rows.Add(12, "Location X", 0.1, 0, xLocation)
            tbl.Rows.Add(13, "Location Y", 0.1, 0, yLocation)
            tbl.Rows.Add(14, "Rotation", 0.1, 0, fRotation)
            Return tbl
        End Get
    End Property

    Public Overrides ReadOnly Property Help As String
        Get
            Return String.Format("Erstellt zufällige Kratzer verschieder Tiefe.{0}" &
                "{0}" &
                "f1 = Distanz zu einer Linie{0}" &
                "{0}" &
                "Result = ({0}" &
                "   für alle Linien, Summe von: {0}" &
                "       wenn f1 > fWidth dann {0}" &
                "           fResult = 0 {0}" &
                "       else {0}" &
                "           fResult = 1 - f1 {0}" &
                ") * Intensity{0}" &
                "(so in etwa...){0}" &
                "", Environment.NewLine)
        End Get
    End Property

    Public Overrides Sub SetParameterValue(Index As Integer, Value As Double)
        Select Case Index
            Case 0
                fAdd = Value
            Case 1
                fIntensity = Value
            Case 2
                iCount = CInt(Value)
            Case 3
                fWidth = Value
            Case 4
                iSeed = CInt(Value)
            Case 5
                fMinLength = Value
            Case 6
                fMaxLength = Value
            Case 7
                fDepthMin = Value
            Case 8
                fDepthMax = Value
            Case 9
                fChaos = Value
            Case 10
                xScale = Value
            Case 11
                yScale = Value
            Case 12
                xLocation = Value
            Case 13
                yLocation = Value
            Case 14
                fRotation = Value
        End Select
    End Sub

    Public Overrides Sub AddParameterValue(Index As Integer, Value As Double)
        Select Case Index
            Case 0
                fAdd += Value
            Case 1
                fIntensity += Value
            Case 2
                iCount += CInt(Value)
            Case 3
                fWidth += Value
            Case 4
                iSeed += CInt(Value)
            Case 5
                fMinLength += Value
            Case 6
                fMaxLength += Value
            Case 7
                fDepthMin += Value
            Case 8
                fDepthMax += Value
            Case 9
                fChaos += Value
            Case 10
                xScale += Value
            Case 11
                yScale += Value
            Case 12
                xLocation += Value
            Case 13
                yLocation += Value
            Case 14
                fRotation += Value
        End Select
    End Sub

    Private arrScratches(,) As List(Of Scratch)
    Private ptCenter As New PointF(0.5, 0.5)
    Public Overrides Function Evaluate(ByVal x As Double, ByVal y As Double) As Double
        If fWidth = 0 Then Return 0    'linien mit breite 0 sieht man nicht
        If fDepthMax < fDepthMin Then fDepthMax = fDepthMin
        If fMaxLength > 1 Then fMaxLength = 1 'linien dürfen nicht länger sein, sonst funktioniert indizierung nicht!
        If fMinLength > fMaxLength Then fMinLength = fMaxLength
        If fChaos > 1 Then fChaos = 1
        If fChaos < 0 Then fChaos = 0


        If x = 0 AndAlso y = 0 Then
            'initialisieren
            GenerateScratches()
        End If

        x = (x - 0.5) * xScale + xLocation
        y = (y - 0.5) * yScale + yLocation

        Dim f As Double = Math.Sqrt((x) * (x) + (y) * (y))
        Dim fAng As Double = Math.Atan2(y, x)
        Dim xRot As Double = Math.Sin(fAng - fRotation) * f
        Dim yRot As Double = Math.Cos(fAng - fRotation) * f
        x = ptCenter.X + xRot
        y = ptCenter.Y + yRot

        'repeat
        x = x - Math.Floor(x)
        y = y - Math.Floor(y)

        Dim yDeltaCurrent, xDeltaCurrent As Double
        Dim fAlpha As Double
        Dim fDistAdjacent As Double
        Dim fAbscissa, fOrdinate, fAbsAbscissa As Double
        Dim fWidthScaled As Double = fWidth * (xScale + yScale) / 2
        Dim ltCandidates As List(Of Scratch) = arrScratches(GetKey(x), GetKey(y))
        Dim fResult As Double = 0
        For Each oScratch As Scratch In ltCandidates

            xDeltaCurrent = x - oScratch.From.X
            yDeltaCurrent = y - oScratch.From.Y

            fAlpha = oScratch.Alpha - Math.Atan2(yDeltaCurrent, xDeltaCurrent)
            fDistAdjacent = Math.Sqrt(xDeltaCurrent * xDeltaCurrent + yDeltaCurrent * yDeltaCurrent)
            fAbscissa = Math.Sin(fAlpha) * fDistAdjacent
            fAbsAbscissa = Math.Abs(fAbscissa)
            fOrdinate = Math.Cos(fAlpha) * fDistAdjacent

            Dim isOnLine As Boolean = Math.Abs(fAlpha) < Math.PI / 2 AndAlso fOrdinate < oScratch.Distance

            Dim fThisScratch As Double = 0
            If isOnLine Then
                If fAbsAbscissa < fWidthScaled AndAlso fAbsAbscissa > 0 Then
                    fThisScratch = 1 - fAbsAbscissa / fWidthScaled
                ElseIf fAbsAbscissa = 0 Then
                    fThisScratch = 1
                End If
            ElseIf fDistAdjacent < fWidthScaled Then
                fThisScratch = 1 - fDistAdjacent / fWidthScaled
            Else
                xDeltaCurrent = x - oScratch.To.X
                yDeltaCurrent = y - oScratch.To.Y
                fDistAdjacent = Math.Sqrt(xDeltaCurrent * xDeltaCurrent + yDeltaCurrent * yDeltaCurrent)
                If fDistAdjacent < fWidthScaled Then
                    fThisScratch = 1 - fDistAdjacent / fWidthScaled
                End If
            End If
            If fThisScratch * oScratch.Depth > fResult Then
                fResult = fThisScratch * oScratch.Depth
            End If

        Next
        Return (fResult + fAdd) * fIntensity
    End Function


    Private Sub ClearArray()
        ReDim arrScratches(60, 60)
        For x As Int32 = 0 To arrScratches.GetLength(0) - 1
            For y As Int32 = 0 To arrScratches.GetLength(1) - 1
                arrScratches(x, y) = New List(Of Scratch)
            Next
        Next
    End Sub

    Private Function HaveParamsChanged() As Boolean
        Static iPrevCount As Double = -1
        Static iPrevSeed As Double = iSeed
        Static fPrevWidth As Double = fWidth
        Static fPrevMinLength As Double = fMinLength
        Static fPrevMaxLength As Double = fPrevMaxLength
        Static fPrevDepthMin As Double = fPrevDepthMin
        Static fPrevDepthMax As Double = fPrevDepthMax
        Static fPrevChaos As Double

        Dim isChanged As Boolean
        If iPrevCount <> iCount Then
            isChanged = True
        ElseIf iPrevSeed <> iSeed Then
            isChanged = True
        ElseIf fPrevWidth <> fWidth Then
            isChanged = True
        ElseIf fPrevMinLength <> fMinLength Then
            isChanged = True
        ElseIf fPrevMaxLength <> fMaxLength Then
            isChanged = True
        ElseIf fPrevDepthMin <> fDepthMin Then
            isChanged = True
        ElseIf fPrevDepthMax <> fDepthMax Then
            isChanged = True
        ElseIf fPrevChaos <> fChaos Then
            isChanged = True
        End If

        If isChanged Then
            iPrevCount = iCount
            iPrevSeed = iSeed
            fPrevWidth = fWidth
            fPrevMinLength = fMinLength
            fPrevMaxLength = fMaxLength
            fPrevDepthMin = fDepthMin
            fPrevDepthMax = fDepthMax
            fPrevChaos = fChaos
        End If
        Return isChanged
    End Function

    Private Sub GenerateScratches()

        If Not HaveParamsChanged() Then
            Exit Sub   'neuberechnung nicht nötig
        End If

        Dim rnd As New Random(iSeed)

        ClearArray()

        Dim ptFrom, ptTo As PointF
        Dim fLength As Double
        Dim xTo, yTo As Double
        Dim fAlpha As Double
        For i As Int32 = 0 To iCount - 1
            ptFrom.X = CSng(rnd.NextDouble)
            ptFrom.Y = CSng(rnd.NextDouble)
            fLength = fMinLength + rnd.NextDouble * (fMaxLength - fMinLength)
            fAlpha = (rnd.NextDouble - 0.5) * fChaos * Math.PI / 2
            xTo = Math.Sin(fAlpha) * fLength
            yTo = Math.Cos(fAlpha) * fLength
            'xTo = (rnd.NextDouble * 2 - 1) * fLength
            'yTo = Math.Sqrt(fLength * fLength - xTo * xTo) * Math.Sign(rnd.NextDouble - 0.5)
            ptTo.X = CSng(ptFrom.X + xTo)
            ptTo.Y = CSng(ptFrom.Y + yTo)

            If ptFrom.X > ptTo.X Then
                Dim ptTemp As PointF = ptFrom
                ptFrom = ptTo
                ptTo = ptTemp
            End If
            Dim fDepth As Double = fDepthMin + rnd.NextDouble * (fDepthMax - fDepthMin)
            For xOffset As Int32 = -1 To 1
                For yOffset As Int32 = -1 To 1
                    Dim oNewScratch As New Scratch With {.From = New PointF(ptFrom.X + xOffset, ptFrom.Y + yOffset),
                                                         .To = New PointF(ptTo.X + xOffset, ptTo.Y + yOffset),
                                                         .Depth = fDepth}
                    oNewScratch.Prep()

                    AddScratchIndexed(oNewScratch)
                Next
            Next
        Next
    End Sub

    Private Sub AddScratchIndexed(NewScratch As Scratch)
        'anfangs und endpunkt hinzufügen
        Add4Times(NewScratch.From, NewScratch)
        Add4Times(NewScratch.To, NewScratch)

        'punklte dazwischen hinzufügen
        Dim ptCurrent As PointF = NewScratch.From

        Dim xNextXCrossing As Double
        Dim xNextYCrossing As Double
        Dim isValid As Boolean
        Do
            Dim iKey As Int32 = GetKey(ptCurrent.X)
            Dim fNextHigher As Double = KeyToValue(iKey) + 0.1
            If Math.Abs(ptCurrent.X - fNextHigher) < 0.0000001 Then
                fNextHigher += 0.1   'FPU rundungsfehler ausgleichen :P
            End If
            xNextXCrossing = fNextHigher - ptCurrent.X

            If NewScratch.yDirection = 0 Then
                xNextYCrossing = 10
            ElseIf NewScratch.yDirection = 1 Then
                iKey = GetKey(ptCurrent.Y)
                fNextHigher = KeyToValue(iKey) + 0.1
                If Math.Abs(ptCurrent.Y - fNextHigher) < 0.0000001 Then
                    fNextHigher += 0.1   'FPU rundungsfehler ausgleichen :P
                End If
                xNextYCrossing = Math.Abs((fNextHigher - ptCurrent.Y) / Math.Tan(NewScratch.Alpha))
            Else
                iKey = GetKey(ptCurrent.Y)
                Dim fNextLower As Double = KeyToValue(iKey)
                If Math.Abs(ptCurrent.Y - fNextLower) < 0.0000001 Then
                    fNextLower -= 0.1   'FPU rundungsfehler ausgleichen :P
                End If
                xNextYCrossing = Math.Abs((ptCurrent.Y - fNextLower) / Math.Tan(NewScratch.Alpha))
            End If


            If xNextXCrossing < xNextYCrossing Then
                ptCurrent.X += CSng(xNextXCrossing)
                ptCurrent.Y += CSng(Math.Tan(NewScratch.Alpha) * xNextXCrossing)
            Else
                ptCurrent.X += CSng(xNextYCrossing)
                ptCurrent.Y += CSng(Math.Tan(NewScratch.Alpha) * xNextYCrossing)
            End If
            isValid = ptCurrent.X <= NewScratch.To.X AndAlso ((ptCurrent.Y < NewScratch.To.Y AndAlso NewScratch.yDirection = 1) OrElse
                                                              (ptCurrent.Y > NewScratch.To.Y AndAlso NewScratch.yDirection = -1))
            If isValid Then
                Add4Times(ptCurrent, NewScratch)
            End If
        Loop While isValid
    End Sub

    Private Sub Add4Times(pt As PointF, Scratch As Scratch)
        Dim fWidthScaled As Double = fWidth * (xScale + yScale) / 2
        Dim x As Int32 = GetKey(pt.X - fWidthScaled)
        Dim y As Int32 = GetKey(pt.Y - fWidthScaled)
        AddOnce(x, y, Scratch)

        x = GetKey(pt.X + fWidthScaled)
        y = GetKey(pt.Y - fWidthScaled)
        AddOnce(x, y, Scratch)

        x = GetKey(pt.X - fWidthScaled)
        y = GetKey(pt.Y + fWidthScaled)
        AddOnce(x, y, Scratch)

        x = GetKey(pt.X + fWidthScaled)
        y = GetKey(pt.Y + fWidthScaled)
        AddOnce(x, y, Scratch)
    End Sub

    Private Sub AddOnce(xKey As Int32, yKey As Int32, Scratch As Scratch)
        If xKey >= 0 AndAlso xKey <= arrScratches.GetLength(0) - 1 AndAlso
           yKey >= 0 AndAlso yKey <= arrScratches.GetLength(1) - 1 Then
            If Not arrScratches(xKey, yKey).Contains(Scratch) Then
                arrScratches(xKey, yKey).Add(Scratch)
            End If
        Else
            Stop 'das sollte nicht passieren
        End If
    End Sub

    Private Function GetKey(ByVal f As Double) As Int32
        Return CInt(Math.Truncate(21 + f * 10))
    End Function

    Private Function KeyToValue(Key As Int32) As Double
        Return (Key - 21) / 10
    End Function



    Private Class Scratch
        Public From As PointF
        Public [To] As PointF
        Public Depth As Double

        Private yDeltaTo As Double
        Private xDeltaTo As Double
        Private fAlpha As Double
        Private fDistance As Double

        Public yDirection As Int32

        Public Sub Prep()
            yDeltaTo = [To].Y - From.Y
            xDeltaTo = [To].X - From.X
            fAlpha = Math.Atan2(yDeltaTo, xDeltaTo)
            fDistance = Math.Sqrt(xDeltaTo * xDeltaTo + (yDeltaTo * yDeltaTo))


            If fAlpha = 0 Then
                yDirection = 0
            ElseIf fAlpha > 0 Then
                yDirection = 1
            Else
                yDirection = -1
            End If

        End Sub

        ReadOnly Property Alpha() As Double
            Get
                Return fAlpha
            End Get
        End Property

        ReadOnly Property Distance() As Double
            Get
                Return fDistance
            End Get
        End Property
    End Class
End Class
